using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

namespace JYUSB101
{
    /// <summary>
    /// AI采集任务类
    /// </summary>
    public sealed class JYUSB101AITask
    {
        /// <summary>
        /// JYUSB101AITask构造函数
        /// </summary>
        /// <param name="boardNum">板卡编号</param>
        public JYUSB101AITask(int boardNum)
        {
            //获取板卡操作类的实例
            _devHandle = JYUSB101Device.GetInstance((ushort) boardNum);
            if (_devHandle == null)
            {
                throw new JYDriverException(JYDriverExceptionPublic.InitializeFailed,
                    "Initialization failed, please check board number or connection.");
            }
            Channels = new List<AIChannel>();
            SamplesToAcquire = 1024; //每通道采样点数
            SampleRate = 1000;
            Mode = AIMode.Finite; //采样模式
            ClockEdge = AIClockEdge.Rising;
            Trigger = new AITrigger();
            _localBuffer = new CircularBufferEx<short>(); //本地软件缓冲区设置            
            //初始化N个固定的用于存放通道的一维数组
            _channelsArray = new ushort[_devHandle.DiffChannelCount][];
            for (var i = 0; i < _devHandle.DiffChannelCount; i++)
            {
                _channelsArray[i] = new ushort[i + 1];
            }
            _rangesArray = new ushort[_devHandle.DiffChannelCount][];
            for (var i = 0; i < _devHandle.DiffChannelCount; i++)
            {
                _rangesArray[i] = new ushort[i + 1];
            }
            _channelValuesArray = new ushort[_devHandle.DiffChannelCount][];
            for (var i = 0; i < _devHandle.DiffChannelCount; i++)
            {
                _channelValuesArray[i] = new ushort[i + 1];
            }
            _waitUntilDoneEvent = new WaitEvent(() => _taskDone);
            EventQueue = new Queue<WaitEvent>(8);
            _bufferLenSetByUser = false;
            _threadException = new JYDriverThreadExceptionManager();
        }

        #region -------------------私有字段-------------------------

        //添加需要使用的私有属性字段
        /// <summary>
        /// 操作硬件的对象
        /// </summary>
        private readonly JYUSB101Device _devHandle;

        /// <summary>
        /// 100ms
        /// </summary>
        private const double _doubleBufferLength = 0.5;

        /// <summary>
        /// AI是否已启动
        /// </summary>
        private bool _aiStarted;

        /// <summary>
        /// 本地缓冲内存
        /// </summary>
        private CircularBufferEx<short> _localBuffer;

        /// <summary>
        /// 流盘预览缓冲内存
        /// </summary>
        private CircularBufferEx<short> _localPreviewBuffer;

        /// <summary>
        /// 流盘预览缓冲区锁
        /// </summary>
        private Mutex _previewBufferLock;

        /// <summary>
        /// 流盘预览缓冲转换前的数组
        /// </summary>
        private short[,] _previewDataConvertBuffer;

        /// <summary>
        /// 流盘块大小
        /// </summary>
        private int _recordBlockSize;

        /// <summary>
        /// 取数据的线程
        /// </summary>
        private Thread _thdAcquireData;

        /// <summary>
        /// 取数据的线程
        /// </summary>
        private Thread _thdWriteDataFile;

        /// <summary>
        /// 线程抛出的异常管理
        /// </summary>
        private JYDriverThreadExceptionManager _threadException;

        /// <summary>
        /// 任务结束标志 
        /// </summary>
        private bool _taskDone;

        private bool TaskDone
        {
            get { return _taskDone; }
            set
            {
                _taskDone = value;
                if (_taskDone)
                {
                    _waitUntilDoneEvent.Set();
                }
                else if (Mode != AIMode.Single)
                {
                    _aiStarted = true;
                }
            }
        }

        /// <summary>
        /// 通道数组，申明了N个固定的数组，有多少个通道对应使用哪一个一维数组
        /// </summary>
        private readonly ushort[][] _channelsArray;

        /// <summary>
        /// 量程数组
        /// </summary>
        private ushort[][] _rangesArray { get; set; }

        /// <summary>
        /// 单通道Value的数组
        /// </summary>
        private ushort[][] _channelValuesArray;

        /// <summary>
        /// WaitUntilDone等待事件
        /// </summary>
        private WaitEvent _waitUntilDoneEvent;

        /// <summary>
        /// AI是否使能了Double Buffer模式
        /// </summary>
        private bool _enableAIDbfMode;

        /// <summary>
        /// AI硬件双缓冲区的大小
        /// </summary>
        private uint _AIDoubleBuffSize;

        /// <summary>
        /// AI读缓冲区（对齐地址）
        /// </summary>
        private IntPtr _AIReadbuffer_alignment;

        /// <summary>
        /// AI读缓冲区（非对齐地址）
        /// </summary>
        private IntPtr _AIReadbuffer;

        private int _samplesFetchedPerChannel;

        /// <summary>
        /// 等待锁, 用于限制多线程并行读操作. 需要等一个线程读取完成后, 另一个线程才能读(排队).
        /// </summary>
        private object _waitLock = new object();

        /// <summary>
        /// 事件队列。调用WaitUntilDone()或者ReadBuffer()时，使用事件通知方式，提高效率。
        /// </summary>
        private Queue<WaitEvent> EventQueue { get; }

        /// <summary>
        /// 缓冲区是否溢出标志
        /// </summary>
        private bool _isOverflow;

        /// <summary>
        /// 用于有限次重触发，读取缓冲区数据的offset标记
        /// </summary>
        private uint _readyCount;

        /// <summary>
        /// 是否使能重触发
        /// </summary>
        private bool _enableRetrigger;

        /// <summary>
        /// 已经流盘的采样点数
        /// </summary>
        private double _recordedLength;

        /// <summary>
        /// 流盘是否完成
        /// </summary>
        private bool _recordDone;

        /// <summary>
        /// 有限次重触发时FetchBuffer共采集的点数
        /// </summary>
        private int _finiteRetriggerFetchSamples;

        /// <summary>
        /// 用于标志用户是否设置过BufLenInSamples变量
        /// </summary>
        private bool _bufferLenSetByUser;

        /// <summary>
        /// FileStream对象
        /// </summary>
        private FileStream _fs;

        /// <summary>
        /// 写二进制对象
        /// </summary>
        private BinaryWriter _wt;
        private int _fetchCnt;
        private int _recordCnt;

        #endregion

        #region --------------------公共属性定义----------------------

        /// <summary>
        /// 通道列表
        /// </summary>
        public List<AIChannel> Channels { get; }

        /// <summary>
        /// 采集模式，支持Single/Finite/Continuous三种类型
        /// </summary>
        public AIMode Mode { get; set; }

        private double _adjustedSampleRate;
        /// <summary>
        /// 每通道采样率
        /// </summary>
        public double SampleRate
        {
            get { return _adjustedSampleRate; }
            set
            {
                //先根据value的值计算出分频倍数，然后在写入到_adjustedSampleRate
                _adjustedSampleRate = value;
            }
        }

        /// <summary>
        /// 有限点采集时, 每通道采集的样点数。若设置为小于0，则采集无穷个点。
        /// <para>默认值为1024</para>
        /// </summary>
        public int SamplesToAcquire { get; set; }

        private int _bufLenInSamples;

        /// <summary>
        /// 缓冲区能容纳的每通道样点数。一次读取的样点数不能超过此容量。        
        /// <remarks>在调用 Start() 方法后分配或者调整缓冲区。</remarks>
        /// </summary>
        public int BufLenInSamples
        {
            get { return _bufLenInSamples; }
            set
            {
                Interlocked.Exchange(ref _bufLenInSamples, value);
                _bufferLenSetByUser = true;
            }
        }

        /// <summary>
        /// 缓冲区内可以读取的点数
        /// </summary>
        public int AvailableSamples => _localBuffer.NumOfElement/Channels.Count;

        /// <summary>
        /// 时钟沿。仅在外部时钟时有效。
        /// </summary>
        public AIClockEdge ClockEdge { get; set; }

        /// <summary>
        /// AI触发参数设置
        /// </summary>
        public AITrigger Trigger { get; set; }

        /// <summary>
        /// 流盘相关参数设置
        /// </summary>
        public AIRecord Record { get; } = new AIRecord();

        #endregion

        #region --------------------公共方法定义--------------------

        /// <summary>
        /// 添加通道
        /// </summary>
        /// <param name="chnId">通道物理序号</param>
        public void AddChannel(int chnId)
        {
            //To Add 添加通道的代码
            if (_aiStarted == true)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            if (chnId == -1)
            {
                Channels.Clear();
                int chnCount;
                chnCount = (int)_devHandle.DiffChannelCount;

                for (var i = 0; i < chnCount; i++)
                {
                    var chn = new AIChannel(i, -5, 5, AITerminal.Differential);
                    Channels.Add(chn);
                }
            }
            else
            {
                if (chnId < 0 || chnId >= _devHandle.DiffChannelCount)
                {
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
                }
                var chn = new AIChannel(chnId, -5, 5, AITerminal.Differential);
                Channels.Add(chn);
            }
        }

        /// <summary>
        /// 添加多通道
        /// </summary>
        /// <param name="chnsId">要添加通道的所有物理序号</param>
        public void AddChannel(int[] chnsId)
        {
            if (_aiStarted == true)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            //To Add 添加通道的代码  
            foreach (var item in chnsId)
            {
                if (item < 0 ||  item >= _devHandle.DiffChannelCount)
                {
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
                }
                var chn = new AIChannel(item, -5, 5, AITerminal.Differential);
                Channels.Add(chn);
            }
        }

        /// <summary>
        /// 删除指定通道号的通道,为-1则删除所有通道
        /// </summary>
        /// <param name="chnId">要删除的通道的通道号</param>
        public void RemoveChannel(int chnId)
        {
            if (_aiStarted == true)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            //To Add 添加通道的代码  
            if (chnId == -1)
            {
                JYLog.Print(JYLogLevel.DEBUG, "Clear All AI Channels!");
                Channels.Clear();
            }
            else
            {
                var idx = Channels.FindIndex(t => t.ChannelID == chnId);
                if (idx >= 0)
                {
                    Channels.RemoveAt(idx);
                }
                else
                {
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
                }
            }
        }

        /// <summary>
        /// 读取数据，按列返回采集到的电压值
        /// </summary>
        ///<param name="buf">数组</param> 
        /// <param name="samplesPerChannel">每通道读取数据长度</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>   
        public void ReadData(ref double[,] buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            var ret = 0;
            //避开用户缓冲区不足的情况
            if (buf.Length < samplesPerChannel*Channels.Count)
            {
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }
            var tmpBuf = new short[samplesPerChannel, Channels.Count];

            ReadRawData(ref tmpBuf, samplesPerChannel, timeout);
            //To Add: Scale Data的代码
            if ((ret = ScaleRawData(tmpBuf, buf, samplesPerChannel, GetRangeArray())) < 0)
            {
                throw new JYDriverException(ret);
            }
            tmpBuf = null;
            GC.Collect();
        }

        /// <summary>
        /// 重载函数，采样点数默认为数组的行数
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>
        public void ReadData(ref double[,] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            if (Mode == AIMode.Continuous || (Mode == AIMode.Finite && buf.GetLength(0) <= SamplesToAcquire))
            {
                ReadData(ref buf, buf.GetLength(0), timeout);
            }
            else
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
        }

        /// <summary>
        /// 读取数据，按列返回采集到的电压值
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="samplesPerChannel">用户缓冲区能容纳的每通道样点数</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>
        public void ReadData(ref double[] buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            var ret = 0;
            //避开用户缓冲区不足的情况
            if (buf.Length < samplesPerChannel*Channels.Count)
            {
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }
            var tmpRawBuf = new short[samplesPerChannel, Channels.Count];
            ReadRawData(ref tmpRawBuf, samplesPerChannel, timeout);
            //To Add: Scale Data的代码
            if ((ret = ScaleRawData(tmpRawBuf, buf, samplesPerChannel, GetRangeArray())) < 0)
            {
                throw new JYDriverException(ret);
            }
            tmpRawBuf = null; //回收
            GC.Collect();
        }

        /// <summary>
        /// 重载函数，按数组长度推算读取数据点数 
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>
        public void ReadData(ref double[] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            if (Mode == AIMode.Continuous || (Mode == AIMode.Finite && buf.Length <= SamplesToAcquire*Channels.Count))
            {
                ReadData(ref buf, buf.Length/Channels.Count, timeout);
            }
            else
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
        }

        /// <summary>
        /// 读取数据，按列返回采集到的电压值
        /// </summary>
        ///<param name="buf">数组</param> 
        /// <param name="samplesPerChannel">每通道读取数据长度</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>   
        public void ReadData(IntPtr buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            var ret = 0;
            if (Channels.Count <= 0)
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam, "通道数要大于0。");
            }
            var tmpBuf = Marshal.AllocHGlobal(samplesPerChannel * Channels.Count * sizeof(short));

            try
            {
                ReadRawData(tmpBuf, samplesPerChannel, timeout);
                //To Add: Scale Data的代码
                if ((ret = ScaleRawData(tmpBuf, buf, samplesPerChannel, GetRangeArray())) < 0)
                {
                    throw new JYDriverException(ret);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tmpBuf);
            }
            GC.Collect();
        }

        /// <summary>
        /// 读取数据，按列返回采集到的电压值,返回单通道的数据
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="samplesPerChannel">用户缓冲区能容纳的每通道样点数</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>  
        public void ReadRawData(IntPtr buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            if (Mode != AIMode.Continuous && Mode != AIMode.Finite)
            {
                JYLog.Print(JYLogLevel.ERROR, "Mode不是Finite或Continuous，不允许调用ReadData或ReadRawData");
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            //如果是有限点采集，并且AI未启动，则启动
            //这样可以支持直接调用ReadData进行读取，而不用显示调用Start
            if (Mode == AIMode.Finite && _aiStarted == false && samplesPerChannel > AvailableSamples)
            {
                Start();
            }
            if (Mode == AIMode.Continuous && _aiStarted == false)
            {
                JYLog.Print(JYLogLevel.ERROR, "连续模式不允许未调用Start而读取数据");
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            if (Mode == AIMode.Single)
            {
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            if (_taskDone && samplesPerChannel > AvailableSamples)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }

            if (samplesPerChannel <= 0)
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }

            var isTimeout = false;
            var retSamples = samplesPerChannel;
            lock (_waitLock) //防止多个线程同时读取；要求“排队”读取。
            {
                //To Add: 等待缓冲区内的数据足够之后进行读取
                // Handle Wait & TaskDone
                var waitEvent = new WaitEvent(() => TaskDone || (AvailableSamples >= samplesPerChannel));
                _threadException.ReportThreadExceptions();
                if (!waitEvent.EnqueueWait(EventQueue, timeout))
                {
                    isTimeout = true;
                }
                var availableSamples = AvailableSamples;
                if (isTimeout == false && availableSamples < samplesPerChannel)
                {
                    retSamples = AvailableSamples;
                }
                else if (isTimeout)
                {
                    JYLog.Print(JYLogLevel.ERROR, "读取超时，需要返回确定性结果！");
                    throw new JYDriverException(JYDriverExceptionPublic.TimeOut);
                }
                _localBuffer.Dequeue(buf, 0, retSamples * Channels.Count);
            }
            //缓冲区队列溢出
            if (!_isOverflow) return;
            JYLog.Print(JYLogLevel.ERROR, "缓冲区溢出，请提高读取速度");
            throw new JYDriverException(JYDriverExceptionPublic.BufferOverflow);
        }

        /// <summary>
        /// 读取数据，按列返回采集到的电压值,返回单通道的数据
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="samplesPerChannel">用户缓冲区能容纳的每通道样点数</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>  
        public void ReadRawData(ref short[,] buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            if (Mode != AIMode.Continuous && Mode != AIMode.Finite)
            {
                JYLog.Print(JYLogLevel.ERROR, "Mode不是Finite或Continuous，不允许调用ReadData或ReadRawData");
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            if (buf.GetLength(0) < samplesPerChannel || buf.GetLength(1) != Channels.Count)
            {
                JYLog.Print("User Buffer Error!Buf.GetLength(0)={0}, Buf.GetLength(1)={1}", buf.GetLength(0),
                    buf.GetLength(1));
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }
            //如果是有限点采集，并且AI未启动，则启动
            //这样可以支持直接调用ReadData进行读取，而不用显示调用Start
            if (Mode == AIMode.Finite && _aiStarted == false && samplesPerChannel > AvailableSamples)
            {
                Start();
            }
            if (Mode == AIMode.Continuous && _aiStarted == false)
            {
                JYLog.Print(JYLogLevel.ERROR, "连续模式不允许未调用Start而读取数据");
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            if (Mode == AIMode.Single)
            {
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            if (_taskDone && samplesPerChannel > AvailableSamples)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }

            if (samplesPerChannel <= 0)
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            if ((buf.GetLength(1) < Channels.Count || buf.GetLength(0) < samplesPerChannel))
            {
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }

            var isTimeout = false;
            var retSamples = samplesPerChannel;
            lock (_waitLock) //防止多个线程同时读取；要求“排队”读取。
            {
                //To Add: 等待缓冲区内的数据足够之后进行读取
                // Handle Wait & TaskDone
                var waitEvent = new WaitEvent(() => TaskDone || (AvailableSamples >= samplesPerChannel));
                _threadException.ReportThreadExceptions();
                if (!waitEvent.EnqueueWait(EventQueue, timeout))
                {
                    isTimeout = true;
                }
                var availableSamples = AvailableSamples;
                if (isTimeout == false && availableSamples < samplesPerChannel)
                {
                    retSamples = AvailableSamples;
                }
                else if (isTimeout)
                {
                    JYLog.Print(JYLogLevel.ERROR, "读取超时，需要返回确定性结果！");
                    throw new JYDriverException(JYDriverExceptionPublic.TimeOut);
                }
                _localBuffer.Dequeue(ref buf, retSamples*Channels.Count);
            }
            //缓冲区队列溢出
            if (!_isOverflow) return;
            JYLog.Print(JYLogLevel.ERROR, "缓冲区溢出，请提高读取速度");
            throw new JYDriverException(JYDriverExceptionPublic.BufferOverflow);
        }

        /// <summary>
        /// 重载函数，读取数组行数个点
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>
        public void ReadRawData(ref short[,] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            if (Mode == AIMode.Continuous || (Mode == AIMode.Finite && buf.GetLength(0) <= SamplesToAcquire))
            {
                ReadRawData(ref buf, buf.GetLength(0), timeout);
            }
            else
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
        }

        /// <summary>
        /// 读取数据，按列返回采集到的电压值
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="samplesPerChannel">用户缓冲区能容纳的每通道样点数</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>  
        public void ReadRawData(ref short[] buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            var tempBuf = new short[samplesPerChannel, Channels.Count];
            ReadRawData(ref tempBuf, samplesPerChannel, timeout);
            Buffer.BlockCopy(tempBuf, 0, buf, 0, samplesPerChannel*Channels.Count*sizeof(short));
            tempBuf = null;
            GC.Collect();
        }

        /// <summary>
        /// 重载函数，按数组长度读取数据点数
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        /// <param name="timeout">当数据不足时，最多等待的时间（单位：ms）</param>
        public void ReadRawData(ref short[] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            if (Mode == AIMode.Continuous ||
                (Mode == AIMode.Finite && buf.Length <= SamplesToAcquire*Channels.Count))
            {
                ReadRawData(ref buf, buf.Length/Channels.Count, timeout);
            }
            else
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
        }

        

        /// <summary>
        /// 获取流盘时预览的数据
        /// </summary>
        /// <param name="buf">用户定义返回数据的缓冲区</param>
        /// <param name="timeout">超时时间，单位ms，-1为无限等待</param>
        /// <returns></returns>
        public void GetRecordPreviewData(ref double[,] buf, int timeout = -1)
        {
            GetRecordPreviewData(ref buf, buf.Length/Channels.Count, timeout);
        }

        /// <summary>
        /// 获取流盘时预览的数据
        /// </summary>
        /// <param name="buf">用户定义返回数据的缓冲区</param>
        /// <param name="timeout">超时时间，单位ms，-1为无限等待</param>
        /// <returns></returns>
        public void GetRecordPreviewData(ref double[] buf, int timeout = -1)
        {
            GetRecordPreviewData(ref buf, buf.Length / Channels.Count, timeout);
        }

        /// <summary>
        /// 获取流盘时预览的数据
        /// </summary>
        /// <param name="buf">用户定义返回数据的缓冲区</param>
        /// <param name="samplesPerChannel">每通道要取的数据</param>
        /// <param name="timeout">超时时间，单位ms，-1为无限等待</param>
        public void GetRecordPreviewData(ref double[] buf, int samplesPerChannel, int timeout)
        {
            if (buf.Length < samplesPerChannel*Channels.Count)
            {
                JYLog.Print("GetRecordPreviewData:参数buf长度不足");
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }
            var tempBuf = new double[samplesPerChannel, Channels.Count];
            Buffer.BlockCopy(buf, 0, tempBuf, 0, buf.Length * sizeof(double));
            GetRecordPreviewData(ref tempBuf, buf.Length / Channels.Count, timeout);
            Buffer.BlockCopy(tempBuf, 0, buf, 0, buf.Length * sizeof(double));
        }

        /// <summary>
        /// 每通道读取一个样点，非缓冲式读取。返回应用变换系数变换后的数据。
        /// </summary>
        /// <param name="buffer">用户缓冲区数组</param>
        public void ReadSinglePoint(ref double[] buffer)
        {
            lock (this)
            {
                if (buffer.Length != Channels.Count)
                {
                    JYLog.Print(JYLogLevel.ERROR, "ReadSinglePoint:读取数据数组元素个数与同道数不匹配");
                    throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
                }
                if (Mode == AIMode.Single)
                {
                    this.Start();
                    var adlinkErr = USB6101Import.NoError;
                    if ((adlinkErr = USB6101Import.UD_AI_ReadMultiChannels(_devHandle.CardID, (ushort) Channels.Count,
                            GetChannelArray(), GetRangeArray(),
                            _channelValuesArray[Channels.Count - 1])) < 0)
                    {
                        JYLog.Print(JYLogLevel.ERROR, "UD_AI_ReadMultiChannels Error:{0}", adlinkErr);
                        Stop();
                        throw new JYDriverException(adlinkErr);
                    }
                    else
                    {
                        for (var i = 0; i < Channels.Count; i++)
                        {
                            if ((adlinkErr = USB6101Import.UD_AI_VoltScale(_devHandle.CardID, GetRangeArray()[i],
                                    _channelValuesArray[Channels.Count - 1][i], out buffer[i])) >= 0)
                            {
                                continue;
                            }
                            JYLog.Print(JYLogLevel.ERROR, "Voltage Scale Error: {0}", adlinkErr);
                            Stop();
                            throw new JYDriverException(adlinkErr);
                        }
                    }
                }
                else
                {
                    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
                }
                Stop();
            }
        }

        /// <summary>
        /// 单通道读取一个样点，非缓冲式读取。返回应用变换系数变换后的数据。
        /// </summary>
        public void ReadSinglePoint(ref double readValue)
        {
            //多线程读取要求排队读取
            lock (this)
            {
                if (Mode == AIMode.Single && Channels.Count == 1)
                {
                    var b = new double[1];
                    ReadSinglePoint(ref b);
                    readValue = b[0];
                }
                else
                {
                    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
                }
            }
        }

        /// <summary>
        /// 获取流盘状态
        /// </summary>
        /// <param name="recordedLength">已流盘的长度</param>
        /// <param name="recordDone">流盘是否结束</param>
        public void GetRecordStatus(out double recordedLength, out bool recordDone)
        {
            if (Mode != AIMode.Record)
            {
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            //获取两个状态
            recordedLength = _recordedLength;
            recordDone = _recordDone;
            //if (_aiStarted == false)
            //{
            //    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            //}
            _threadException.ReportThreadExceptions();
        }

        /// <summary>
        /// 等待当前任务完成
        /// </summary>
        /// <param name="timeout">等待的时间(单位:ms) </param>
        /// <returns>true:任务结束,false:超时</returns>
        public bool WaitUntilDone(int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            if (!_taskDone)
            {
                var waitResult = _waitUntilDoneEvent.Wait(timeout);
                _threadException.ReportThreadExceptions();
                return waitResult;
            }
            return true;
        }

        /// <summary>
        /// 启动AI任务
        /// </summary>
        public void Start()
        {
            int ret;
            if (_aiStarted)
            {
                JYLog.Print(JYLogLevel.DEBUG, "AI已经启动，忽略Start");
                return;
            }
            if (_devHandle.AIReserved)
            {
                JYLog.Print(JYLogLevel.ERROR, "AI已被占用！");
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            _isOverflow = false;
            if (Channels.Count < 1 || Channels.Count > 256)
            {
                JYLog.Print(JYLogLevel.ERROR, "错误:未添加通道或添加的通道数超过了256");
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder, "Please check or add channel firstly！");
            }
            _samplesFetchedPerChannel = 0;
            _readyCount = 0;
            _recordDone = false;

            _recordCnt = 0;
            _fetchCnt = 0;
            _finiteRetriggerFetchSamples = 0;
            _threadException.ClearThreadExceptions();
            //To Add: 添加Start硬件采集的代码，如果是单点则只需要标记AI的启动和占用
            //如果是连续采集则需要启动后台线程，不断从硬件缓冲区中读取数据
            if ((ret = AIConfig()) < 0) //配置AI的触发，通道等
            {
                throw new JYDriverException(ret);
            }
            if (Mode == AIMode.Single)
            {
                _aiStarted = true;
                _devHandle.AIReserved = true;
                return;
            }
            else
            {
                if (_bufLenInSamples < (int) (SampleRate*0.5) && _bufferLenSetByUser)
                {
                    JYLog.Print(JYLogLevel.ERROR, "缓冲区设置太小！");
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
                }
                else
                {
                    _bufLenInSamples = (int) (SampleRate*20);
                }
                //配置连续或有限点采样的缓冲区
                if ((ret = ConfigContAcq()) == JYErrorCode.NoError)
                {
                    _localBuffer = new CircularBufferEx<short>(_bufLenInSamples*Channels.Count);

                    if (Mode == AIMode.Record)
                    {
                        _recordBlockSize = (int)GetNearest2N((uint)(_adjustedSampleRate / 20) * (uint)Channels.Count);
                        //写入文件的块大小，50ms的长度，并调整到2的N次方的数
                        _localPreviewBuffer = new CircularBufferEx<short>(Math.Max(_recordBlockSize, (int)_adjustedSampleRate * Channels.Count)); //流盘预览缓冲区建立,1s每通道
                        CreateRecordFile(); //创建流盘文件
                        _previewBufferLock = new Mutex();
                        _thdWriteDataFile = new Thread(ThdWriteDataFile);
                        _previewDataConvertBuffer = new short[_localPreviewBuffer.BufferSize * 2, Channels.Count];
                    }

                    //StopContAI
                    byte bStopped;
                    ulong dwAccessCnt;
                    ret = USB6101Import.UD_AI_AsyncCheck(_devHandle.CardID, out bStopped, out dwAccessCnt);
                    if (ret != USB6101Import.NoError)
                    {
                        JYLog.Print(JYLogLevel.WARN, "UD_AI_AsyncCheck Failed! Error Code:{0}", ret);
                    }
                    if (bStopped != 1)
                    {
                        uint accessCnt;
                        ret = USB6101Import.UD_AI_AsyncClear(_devHandle.CardID, out accessCnt);
                        if (ret < 0)
                        {
                            JYLog.Print(JYLogLevel.WARN, "UD_AI_AsyncClear Failed! Error Code:{0}", ret);
                        }
                    }

                    _thdAcquireData = new Thread(ThdAcquireData);
                    _devHandle.AIReserved = true;
                    uint readCount = (uint)( _AIDoubleBuffSize * Channels.Count);
                    ret = USB6101Import.UD_AI_ContReadMultiChannels(_devHandle.CardID, (ushort) Channels.Count,
                        GetChannelArray(),
                        GetRangeArray(), _AIReadbuffer_alignment, readCount, _adjustedSampleRate, USB6101Import.ASYNCH_OP);
                    if (ret == 0)
                    {  
                        JYLog.Print(JYLogLevel.DEBUG, "AI Started OK!");
                        TaskDone = false;
                        _aiStarted = true;
                        _thdAcquireData.Priority = ThreadPriority.Highest;//确保Sleep(1)不被强占，延迟时间足够短
                        _thdAcquireData.Start();
                        if (Mode == AIMode.Record)
                        {
                            _thdWriteDataFile.Start();
                        }
                    }
                    else
                    {
                        JYLog.Print(JYLogLevel.ERROR, "WD_AI_ContReadMultiChannels Failed! Error Code={0}", ret);
                        _devHandle.AIReserved = false;
                        throw new JYDriverException(ret);
                    }
                }
                else
                {
                    throw new JYDriverException(ret);
                }
            }
        }

        /// <summary>
        /// 停止AI任务
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (_aiStarted == false || _devHandle.CardID == 65535) //已经调用过Stop了，直接返回
                {
                    JYLog.Print(JYLogLevel.DEBUG, "Stopped! Return...");
                    return;
                }
                _aiStarted = false;
            }
            if (_devHandle != null)
            {
                _devHandle.AIReserved = false;
            }
            else
            {
                return;
            }
            _taskDone = true;
            //单点模式直接退出，不用等线程结束
            if (Mode == AIMode.Single)
            {
                return;
            }
            //连续或有限点采集时，需要停止从硬件取数据的线程
            if (_thdAcquireData.IsAlive)
            {
                if (false == _thdAcquireData.Join(200))
                {
                    _thdAcquireData.Abort();
                    JYLog.Print("Acquire Thread Exit Abnormally...");
                }
            }
            JYLog.Print(JYLogLevel.DEBUG, "Acquire Thread Exit OK...");

            if (Mode == AIMode.Record)
            {
                if (_thdWriteDataFile.IsAlive == true)
                {
                    if (false == _thdWriteDataFile.Join(30000))
                    {
                        _thdWriteDataFile.Abort();
                        JYLog.Print(JYLogLevel.DEBUG, "WriteDataFile Thread Exit Abnormally...");
                    }
                }
                JYLog.Print(JYLogLevel.DEBUG, "WriteDataFile Thread Exit OK...");
            }
            JYLog.Print("Acquire Thread Exit OK...");
            if (_devHandle == null) return;
            //StopContAI
            uint accessCnt;
            int ret = USB6101Import.UD_AI_AsyncClear(_devHandle.CardID, out accessCnt);
            if (ret < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "UD_AI_AsyncClear Failed! Error Code:{0}", ret);
                throw new JYDriverException(ret);
            }
            JYLog.Print(JYLogLevel.DEBUG, "UD_AI_AsyncClear OK...");
            Marshal.FreeHGlobal(_AIReadbuffer);
            _AIReadbuffer = IntPtr.Zero;
            _threadException.ReportThreadExceptions();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~JYUSB101AITask()
        {
            if (_aiStarted == true)
            {
                Stop();
            }
        }

        #endregion

        #region -------------私有方法定义-------------

        /// <summary>
        /// 从缓冲区取数据的线程函数
        /// </summary>
        private void ThdAcquireData()
        {
            JYLog.Print("AI Task Started...");
            var buffer = new short[_enableAIDbfMode ? _AIDoubleBuffSize*Channels.Count/2 : _AIDoubleBuffSize*Channels.Count];
            bool stopped = false;
            var stopAction = new Action(() =>
            {
                try
                {
                    Stop();
                }
                catch (Exception ex)
                {
                    _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
                }
            });

            //DateTime oldTime ;
            //DateTime newTime = DateTime.Now;
            //DateTime Time1 = DateTime.Now;
            while (TaskDone == false)
            {
                //To Add: 以下添加从缓冲区取数据到本地缓冲区的代码，同时需要修改_samplesFetchedPerChannel的值
                var readCnt = 0;
                if ((readCnt = FetchBuffer(ref buffer, ref stopped)) > 0)
                {
                    //oldTime = newTime;
                    //newTime = DateTime.Now;
                    //JYLog.Print(JYLogLevel.DEBUG,"Sleep用时{0}", (newTime - Time1).TotalMilliseconds);
                    //JYLog.Print(JYLogLevel.DEBUG,"FetchBuffer 间隔 {0}ms", (newTime - oldTime).TotalMilliseconds);
                    //DateTime start = DateTime.Now;
                    try
                    {
                        if (_enableRetrigger) //如果使能重触发，则每次入队列的点数为设置的有限点数
                        {
                            int adjustSamplesToAcquire = (int)GetNearestOfMBlocksize((uint)SamplesToAcquire, _devHandle.AIDBFBlockSize);
                            EnQueueElems(buffer.Skip(_finiteRetriggerFetchSamples - adjustSamplesToAcquire * Channels.Count).Take(SamplesToAcquire * Channels.Count).ToArray());
                        }
                        else
                        {
                            _samplesFetchedPerChannel += readCnt / Channels.Count;
                            EnQueueElems(buffer);
                            //JYLog.Print(JYLogLevel.DEBUG,"Enqueue {0}Samples, _samplesFetchedPerChannel={1}", readCnt / Channels.Count,
                            //    _samplesFetchedPerChannel);
                        }
                    }
                    catch (Exception ex)
                    {
                        JYLog.Print(JYLogLevel.ERROR,"ThdAcquireData 发生未知错误");
                        _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
                        stopAction.Invoke();
                        TaskDone = true;
                    }
                    //DateTime end = DateTime.Now;
                    //JYLog.Print("EnQueueElems time:{0}", (end - start).TotalMilliseconds);

                    //有限点模式，并且重触发计数为0，则结束采集
                    if (Mode == AIMode.Finite)
                    {
                        //使能重触发
                        if (_enableRetrigger)
                        {
                            //操作是否结束
                            if (stopped)
                            {
                                JYLog.Print(JYLogLevel.DEBUG,"Finite Task Done!_enableRetrigger = {0},stopped = {1}", _enableRetrigger,stopped);
                                stopAction.Invoke();
                                TaskDone = true;
                            }
                        }
                        else if (_samplesFetchedPerChannel >= SamplesToAcquire)
                        {
                            JYLog.Print(JYLogLevel.DEBUG,"Finite Task Done!");
                            stopAction.Invoke();
                            TaskDone = true;
                        }
                    }
                    ActivateWaitEvents(); //激活等待事件
                }

                //Time1 = DateTime.Now;
                //DateTime starttime = DateTime.Now;
                Thread.Sleep(1);
                //DateTime endtime = DateTime.Now;
                //if ((endtime - starttime).TotalMilliseconds > 20)
                //{
                //    JYLog.Print("Sleep time:{0}", (endtime - starttime).TotalMilliseconds);
                //}
            }
            ActivateWaitEvents(); //激活等待事件
            JYLog.Print(JYLogLevel.DEBUG,"Fetch data Thread Exit!");
        }

        /// <summary>
        /// 从缓冲区取数据的线程函数
        /// </summary>
        private void ThdWriteDataFile()
        {
            byte[] bufferToFile = new byte[_recordBlockSize * sizeof(short)];
            short[] tempBuff = new short[_recordBlockSize];
            short[] tempBuffDeq = new short[_recordBlockSize];
            int currentElementCount = 0;
            long recordedCount = 0; //写入文件的点数（所有通道）
            while (TaskDone == false)
            {
                //To Add: 以下添加从缓冲区取数据并存入文件
                WaitEvent waitEvent =
                    new WaitEvent(() => TaskDone || (_localBuffer.NumOfElement >= _recordBlockSize));
                try
                {
                    if (waitEvent.EnqueueWait(EventQueue, 100))
                    {
                        currentElementCount = _localBuffer.NumOfElement;
                        if (TaskDone == false && _localBuffer.NumOfElement >= _recordBlockSize)
                        {
                            _recordCnt += _recordBlockSize;
                            _localBuffer.Dequeue(ref tempBuff, _recordBlockSize); //从本地缓冲区取出数据
                            Buffer.BlockCopy(tempBuff, 0, bufferToFile, 0, _recordBlockSize * sizeof(short));
                            WriteDataToFile(bufferToFile, 0, _recordBlockSize * sizeof(short)); //写入文件

                            recordedCount += _recordBlockSize; //累加写入的点数
                            _recordedLength = (double)recordedCount / Channels.Count / _adjustedSampleRate;
                            if (_previewBufferLock.WaitOne(1))
                            {
                                //如果放不下则，先清空，保证该缓冲区始终只有BlockSize那么多数据
                                if (_localPreviewBuffer.CurrentCapacity < _recordBlockSize)
                                {
                                    _localPreviewBuffer.Dequeue(ref tempBuffDeq,
                                        ((_recordBlockSize - _localPreviewBuffer.CurrentCapacity)/Channels.Count+1)*Channels.Count);
                                }
                                _localPreviewBuffer.Enqueue(tempBuff, _recordBlockSize);
                                _previewBufferLock.ReleaseMutex();
                                ActivateWaitEvents();
                                //JYLog.Print(JYLogLevel.DEBUG,"Write {0} samples to preview queue.", _recordBlockSize / Channels.Count);
                            }
                            if (Record.Mode == RecordMode.Finite && _recordedLength >= Record.Length)
                            {
                                JYLog.Print("Record 完成数据采集");
                                TaskDone = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
                }
            }

            if (Record.Mode == RecordMode.Finite)
            {
               // JYLog.Print(JYLogLevel.DEBUG,"Record的Finite模式自动调用stop");
                new Action(Stop).Invoke();
            }

            try
            {
                //有限模式写入的点还不够时,再写入当前有的点或者不超过需要流盘的总点数
                if (Record.Mode == RecordMode.Finite && _recordedLength < Record.Length && _localBuffer.NumOfElement > 0)
                {
                    while (_recordedLength < Record.Length && _localBuffer.NumOfElement > 0)
                    {
                        var samplesToWrite =
                            (int)
                            ((_localBuffer.NumOfElement > (int)((Record.Length - _recordedLength) * _adjustedSampleRate * Channels.Count))
                                ? (int)((Record.Length - _recordedLength) * _adjustedSampleRate * Channels.Count)
                                : _localBuffer.NumOfElement);
                        samplesToWrite = Math.Min(samplesToWrite, tempBuff.Length);
                        _localBuffer.Dequeue(ref tempBuff, samplesToWrite); //从本地缓冲区取出数据
                        Buffer.BlockCopy(tempBuff, 0, bufferToFile, 0, samplesToWrite * sizeof(short));
                        WriteDataToFile(bufferToFile, 0, samplesToWrite * sizeof(short)); //写入文件
                        recordedCount += samplesToWrite; //累加写入的点数
                        _recordedLength = (double)recordedCount / Channels.Count / _adjustedSampleRate;
                        //_recordedLength += ((double)samplesToWrite / Channels.Count / _adjustedSampleRate);
                    }
                }
                else if (Record.Mode == RecordMode.Infinite)
                {
                    //任务结束，并且是无限流盘，则将缓冲区的所有点写入文件
                    while (_localBuffer.NumOfElement > 0)
                    {
                        var samplesToWrite = (int)Math.Min(_localBuffer.NumOfElement, _recordBlockSize);
                        _localBuffer.Dequeue(ref tempBuff, samplesToWrite); //从本地缓冲区取出数据
                        Buffer.BlockCopy(tempBuff, 0, bufferToFile, 0, samplesToWrite * sizeof(short));
                        WriteDataToFile(bufferToFile, 0, samplesToWrite * sizeof(short)); //写入文件
                        recordedCount += samplesToWrite; //累加写入的点数
                        _recordedLength = (double)recordedCount / Channels.Count / _adjustedSampleRate;
                    }
                    //JYLog.Print(JYLogLevel.DEBUG,"Infinite Write OK!");
                }

                if (Mode == AIMode.Record)
                {
                    _recordDone = true;
                }
            }
            catch(Exception ex)
            {
                _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
            }

            try
            {
                CloseRecordFile();
            }
            catch (Exception ex)
            {
                JYLog.Print(ex.Message);
            }
           // JYLog.Print(JYLogLevel.DEBUG,"WriteDataFile Thread Exit! Write Length={0} seconds.", _recordedLength);
        }

        /// <summary>
        /// 数据放入队列尾部
        /// </summary>
        /// <param name="buffer">入队数据</param>
        private void EnQueueElems(short[] buffer)
        {
            if (_localBuffer.NumOfElement + buffer.Length > _bufLenInSamples * Channels.Count)
            {
                _isOverflow = true;
                //JYLog.Print(JYLogLevel.DEBUG,"EnQueueElems isOverflow");
                return;
            }
            _localBuffer.Enqueue(buffer);
        }

        /// <summary>
        /// 获取流盘时预览的数据
        /// </summary>
        /// <param name="buf">用户定义返回数据的缓冲区</param>
        /// <param name="samplesPerChannel">每通道要取的数据</param>
        /// <param name="timeout">超时时间，单位ms，-1为无限等待</param>
        /// <returns></returns>
        public void GetRecordPreviewData(ref double[,] buf, int samplesPerChannel, int timeout)
        {
            _threadException.ReportThreadExceptions();
            if (buf.GetLength(0) < samplesPerChannel || buf.GetLength(1) != Channels.Count)
            {
                JYLog.Print("GetRecordPreviewData:参数buf的大小不合理");
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }
            //从_localPreviewBuffer中取数据,无数据则返回0,有数据则返回实际的samples
            lock (_waitLock) //防止多个线程同时读取；要求“排队”读取。
            {
                int ret = JYErrorCode.NoError;
                //如果是不是Record模式，则返回错误
                if (Mode != AIMode.Record)
                {
                    JYLog.Print("Not Record Mode cannot call this Method...");
                    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
                }

                if (_recordDone == true || _taskDone == true)
                {
                    JYLog.Print("Record is Done...");
                    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
                }

                if (samplesPerChannel <= 0)
                {
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam); //数组长度不够
                }
                if (buf.GetLength(0) < samplesPerChannel || buf.GetLength(1) < Channels.Count)
                {
                    JYLog.Print(JYLogLevel.ERROR, "User Buffer Error!Buf.GetLength(0)={0}, Buf.GetLength(1)={1}",
                        buf.GetLength(0), buf.GetLength(1));
                    throw new JYDriverException(JYDriverExceptionPublic.UserBufferError,
                        "User Buffer Error!Buf Row={0}, Buf Col={1}", buf.GetLength(0), buf.GetLength(1));
                }

                WaitEvent waitEvent =
                    new WaitEvent(() => TaskDone || (_localPreviewBuffer.NumOfElement / Channels.Count >= samplesPerChannel));
                if (!waitEvent.EnqueueWait(EventQueue, timeout))
                {
                    JYLog.Print(JYLogLevel.DEBUG, "Timeout...");
                }
                JYLog.Print(JYLogLevel.DEBUG, "_localPreviewBuffer.NumOfElement={0}...",
                    _localPreviewBuffer.NumOfElement);

                if (_localPreviewBuffer.NumOfElement <= 0)
                {
                    JYLog.Print(JYLogLevel.DEBUG, "No samples...");
                    return;
                }

                int retSamples = Math.Min(samplesPerChannel, _localPreviewBuffer.NumOfElement / Channels.Count);

                if (true == _previewBufferLock.WaitOne(-1))
                {
                    _localPreviewBuffer.Dequeue(ref _previewDataConvertBuffer, retSamples * Channels.Count);
                    _previewBufferLock.ReleaseMutex();

                    ushort[] range = GetRangeArray();
                    //根据Range转化成实际物理量
                    if ((ret = ScaleRawData(_previewDataConvertBuffer, buf, retSamples, range)) < 0)
                    {
                        JYLog.Print(JYLogLevel.ERROR, "ScaleRawData Failed,Error Code = {0}", ret);
                        throw new JYDriverException(ret);
                    }
                    JYLog.Print(JYLogLevel.DEBUG, "Get Record {0}", retSamples);
                }
            }
        }

        /// <summary>
        /// 激活等待事件
        /// </summary>
        private void ActivateWaitEvents()
        {
            WaitEvent waitEvent;
            for (var i = 0; i < EventQueue.Count; i++)
            {
                waitEvent = EventQueue.Dequeue();
                if (waitEvent == null || !waitEvent.IsEnabled) continue; //Just Dequeue when no one is waiting
                if (TaskDone || waitEvent.ConditionHandler())
                    waitEvent.Set();
                else
                    EventQueue.Enqueue(waitEvent);
            }
        }

        

        /// <summary>
        /// 创建流盘文件
        /// </summary>
        /// <returns></returns>
        private int CreateRecordFile()
        {
            _fs = new FileStream(Record.FilePath, FileMode.OpenOrCreate);
            _wt = new BinaryWriter(_fs);
            return 0;
        }

        /// <summary>
        /// 数据写入流盘文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int WriteDataToFile(byte[] data, int index, int count)
        {  
            if (data == null) throw new System.ArgumentException("data parameter cannot be null!");
            _wt.Write(data, index, count);
            return 0;
        }

        /// <summary>
        /// 关闭流盘文件
        /// </summary>
        /// <returns></returns>
        private int CloseRecordFile()
        {
            _wt.Close();
            _fs.Close();
            return 0;
        }

        /// <summary>
        /// 根据输入范围匹配一个原厂驱动的Range值
        /// </summary>
        /// <param name="rangeLow">输入下限</param>
        /// <param name="rangeHigh">输入上限</param>
        /// <returns>
        /// 小于0：错误
        /// 大于0：原厂驱动的Range
        /// </returns>
        private int GetVendorRange(double rangeLow, double rangeHigh)
        {
            if (rangeLow >= rangeHigh || rangeLow < -5 || rangeHigh > 5)
            {
                return JYErrorCode.ErrorParam;
            }
            //else if(rangeLow >= -0.2 && rangeHigh <= 0.2 )
            //{
            //    return USB6101Import.AD_B_0_2_V;
            //}
            //else if (rangeLow >= -1 && rangeHigh <= 1)
            //{
            //    return USB6101Import.AD_B_1_V;
            //}
            //else if (rangeLow >= -2 && rangeHigh <= 2)
            //{
            //    return USB6101Import.AD_B_2_V;
            //}
            else
            {
                return USB6101Import.AD_B_5_V;
            }
        }

        /// <summary>
        /// 根据原厂驱动Range查询出真实的电压值
        /// </summary>
        /// <param name="AD_Range">原厂驱动的Range</param>
        /// <returns>真实的电压范围</returns>
        private double FullRange(int AD_Range)
        {
            double fullrange;
            switch (AD_Range)
            {
                case USB6101Import.AD_B_0_2_V :
                    fullrange = 0.4;
                    break;
                case USB6101Import.AD_B_1_V:
                    fullrange = 2;
                    break;
                case USB6101Import.AD_B_2_V:
                    fullrange = 4;
                    break;
                case USB6101Import.AD_B_5_V:
                    fullrange = 10;
                    break;
                default:
                    JYLog.Print(JYLogLevel.ERROR, "FullRange：不是软件原厂驱动中的量程");
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            return (fullrange);
        }


        /// <summary>
        /// AI相关的配置，调用原厂Config函数进行配置
        /// </summary>
        /// <returns>
        /// 0:成功
        /// 非0:失败，具体看错误代码
        /// </returns>
        private int AIConfig()
        {
            short err = 0;
            ushort wTriggerMode = 0;
            ushort wConvSrc = 0;
            ushort wTrigCtrl = 0;

            #region SampleRateConfig
            if ((_adjustedSampleRate < 0) || (_adjustedSampleRate > _devHandle.MaxSampleRateSingleChannel))
            {
                JYLog.Print(JYLogLevel.ERROR, "SampleRateError：采样率参数不正确");
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }

            if((err = USB6101Import.UD_GetActualRate(_devHandle.CardID, _adjustedSampleRate, out _adjustedSampleRate)) < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "SampleRateConfigError：采样率配置出错");
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }

            #endregion

            #region AD Conversion Clock Source Setting

            wConvSrc = USB6101Import.UD_AI_CONVSRC_INT;

            #endregion


            #region Trigger Type and Edge Setting

            if (Trigger.Type != AITriggerType.Immediate)
            {
                wTriggerMode = USB6101Import.UD_AI_TRGMOD_POST;
                wTrigCtrl |= USB6101Import.UD_AI_TRGSRC_DTRIG;

                if (Trigger.Digital.Edge == AIDigitalTriggerEdge.Rising)
                {
                    wTrigCtrl |= USB6101Import.UD_AI_TrigPositive;
                }
                else
                {
                    wTrigCtrl |= USB6101Import.UD_AI_TrigNegative;
                }
            }
            else
            {
                wTrigCtrl |= USB6101Import.UD_AI_TRGSRC_SOFT;
            }
            
            #endregion

            if((err = USB6101Import.UD_AI_Trigger_Config(_devHandle.CardID, wConvSrc,wTriggerMode,wTrigCtrl,0,0,0,0)) < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "TriggerConfigError：触发配置出错");
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            return JYErrorCode.NoError;
 }

        /// <summary>
        /// 对原始数据RawData进行Scale转换
        /// </summary>
        /// <param name="rawData">原始数据</param>
        /// <param name="scaledData">Scale后的数据</param>
        /// <param name="samples">采集到点数</param>
        /// <param name="scaleRange">需要scale的range</param>
        private int ScaleRawData(short[,] rawData, double[] scaledData, int samples, ushort[] scaleRange)
        {
            if (scaleRange.Length != rawData.GetLength(1))
            {
                JYLog.Print("scale range error!");
                return JYErrorCode.ErrorParam;
            }
            int col = Channels.Count;
            int row = Math.Min(rawData.Length / Channels.Count, samples);
            for (int i = 0; i < col; i++)
            {
                switch (scaleRange[i])
                {
                    case USB6101Import.AD_B_5_V:
                        for (int j = 0; j < row; j++)
                        {
                            //scaledData[j * col + i] = (double)(rawData[j, i] / 32768.0 * 5.0);
                            USB6101Import.UD_AI_VoltScale(_devHandle.CardID, USB6101Import.AD_B_5_V, (ushort)rawData[j, i], out scaledData[j * col + i]);
                        }
                        break;
                    //case USB6101Import.AD_B_2_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j * col + i] = (double)rawData[j, i] / 32768.0 * 2.0;
                    //    }
                    //    break;
                    //case USB6101Import.AD_B_1_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j * col + i] = (double)rawData[j, i] / 32768.0 * 1.0;
                    //    }
                    //    break;
                    //case USB6101Import.AD_B_0_2_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j * col + i] = (double)rawData[j, i] / 32768.0 * 0.2;
                    //    }
                    //    break;
                    default:
                        return JYErrorCode.ErrorParam;
                }
            }
            return JYErrorCode.NoError;
        }

        /// <summary>
        /// 对原始数据RawData进行Scale转换
        /// </summary>
        /// <param name="rawData">原始数据</param>
        /// <param name="scaledData">Scale后的数据</param>
        /// <param name="samples">采集到的点数</param>
        /// <param name="scaleRange">需要scale的range</param>
        private unsafe int ScaleRawData(IntPtr rawData, IntPtr scaledData, int samples, ushort[] scaleRange)
        {
            if (scaleRange.Length != Channels.Count)
            {
                JYLog.Print("scale range error!");
                return JYErrorCode.ErrorParam;
            }
            int col = Channels.Count;
            int row = Math.Min(samples / Channels.Count * scaleRange.Length, samples);
            for (int i = 0; i < col; i++)
            {
                switch (scaleRange[i])
                {
                    case USB6101Import.AD_B_5_V:
                        for (int j = 0; j < row; j++)
                        {
                            *((double*)scaledData + i + j * Channels.Count) = (*((short*)rawData + i + j * Channels.Count) / 32768.0 * 5.0);
                            //scaledData[j, i] = (double)(rawData[j, i] / 32768.0 * 5.0);
                            //USB6101Import.UD_AI_VoltScale(_devHandle.CardID, USB6101Import.AD_B_5_V, (ushort)rawData[j, i], out scaledData[j, i]);

                        }
                        break;
                    //case USB6101Import.AD_B_2_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j, i] = (double)rawData[j, i] / 32768.0 * 2.0;
                    //    }
                    //    break;
                    //case USB6101Import.AD_B_1_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j, i] = (double)rawData[j, i] / 32768.0 * 1.0;
                    //    }
                    //    break;
                    //case USB6101Import.AD_B_0_2_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j, i] = (double)rawData[j, i] / 32768.0 * 0.2;
                    //    }
                    //    break;
                    default:
                        return JYErrorCode.ErrorParam;
                }
            }
            return JYErrorCode.NoError;
        }

        /// <summary>
        /// 对原始数据RawData进行Scale转换
        /// </summary>
        /// <param name="rawData">原始数据</param>
        /// <param name="scaledData">Scale后的数据</param>
        /// <param name="samples">采集到的点数</param>
        /// <param name="scaleRange">需要scale的range</param>
        private int ScaleRawData(short[,] rawData, double[,] scaledData, int samples, ushort[] scaleRange)
        {
            if (scaleRange.Length != rawData.GetLength(1))
            {
                JYLog.Print("scale range error!");
                return JYErrorCode.ErrorParam;
            }
            int col = rawData.GetLength(1);
            int row = Math.Min(rawData.GetLength(0), samples);
            for (int i = 0; i < col; i++)
            {
                switch (scaleRange[i])
                {
                    case USB6101Import.AD_B_5_V:
                        for (int j = 0; j < row; j++)
                        {
                            //scaledData[j, i] = (double)(rawData[j, i] / 32768.0 * 5.0);
                            USB6101Import.UD_AI_VoltScale(_devHandle.CardID, USB6101Import.AD_B_5_V, (ushort)rawData[j, i], out scaledData[j ,i]);

                        }
                        break;
                    //case USB6101Import.AD_B_2_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j, i] = (double)rawData[j, i] / 32768.0 * 2.0;
                    //    }
                    //    break;
                    //case USB6101Import.AD_B_1_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j, i] = (double)rawData[j, i] / 32768.0 * 1.0;
                    //    }
                    //    break;
                    //case USB6101Import.AD_B_0_2_V:
                    //    for (int j = 0; j < row; j++)
                    //    {
                    //        scaledData[j, i] = (double)rawData[j, i] / 32768.0 * 0.2;
                    //    }
                    //    break;
                    default:
                        return JYErrorCode.ErrorParam;
                }
            }
            return JYErrorCode.NoError;
        }

        /// <summary>
        /// 获取当前添加通道的数组
        /// </summary>
        /// <returns></returns>
        private ushort[] GetChannelArray()
        {
            for (var i = 0; i < Channels.Count; i++)
            {
                _channelsArray[Channels.Count - 1][i] = (ushort)(Channels[i].ChannelID);
            }
            return _channelsArray[Channels.Count - 1];
        }

        /// <summary>
        /// 获取当前添加通道的数组
        /// </summary>
        /// <returns></returns>
        private ushort[] GetRangeArray()
        {
            for (var i = 0; i < Channels.Count; i++)
            {
                _rangesArray[Channels.Count - 1][i] = (ushort)(GetVendorRange(Channels[i].RangeLow, Channels[i].RangeHigh));
            }
            return _rangesArray[Channels.Count - 1];
        }

        /// <summary>
        /// 获取里requestsize最近的Blocksize的整数倍的数
        /// </summary>
        /// <param name="requestSize"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        private static uint GetNearestOfMBlocksize(uint requestSize, uint blockSize)
        {
            if (blockSize <= 0)
            {
                return requestSize;
            }
            while (true)
            {
                if (requestSize % blockSize == 0)
                {
                    break;
                }
                requestSize++;
            }
            return requestSize;
        }

        /// <summary>
        /// 取大于输入整数a的2的N方的数
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private uint GetNearest2N(uint a)
        {
            uint ret = 1;
            if (a <= 0)
            {
                return ret;
            }
            for (int i = 0; i < 32; i++)
            {
                a <<= 1;
                if ((a & 0x80000000) == 0x80000000)
                {
                    if ((a & 0x7fffffff) > 0)
                    {
                        ret <<= (32 - i - 1);
                    }
                    else
                    {
                        ret <<= (32 - i - 2);
                    }
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 连续采集配置
        /// </summary>
        /// <returns>
        /// 0:成功
        /// 非0:失败，具体看错误代码
        /// </returns>
        private int ConfigContAcq()
        {
            _enableAIDbfMode = false;

            uint initalSize;
            switch (Mode)
            {
                case AIMode.Single:
                    return JYErrorCode.CannotCall;
                case AIMode.Finite:
                    initalSize = (uint)(_adjustedSampleRate * _doubleBufferLength * 2);
                    initalSize = initalSize == 0 ? 1 : initalSize;
                    var initBuffersize = GetNearestOfMBlocksize(initalSize, _devHandle.AIDBFBlockSize);
                    if (SamplesToAcquire < initBuffersize * 4 ) //如果有限点的点数较小，则不使用double buffer模式
                    {
                        //Finite模式下,,每通道取的点数必须是块的整数倍,如果用户配置的不是，则向上调整到最近的
                        _AIDoubleBuffSize = GetNearestOfMBlocksize((uint)SamplesToAcquire, _devHandle.AIDBFBlockSize);

                    }
                    else
                    {
                        _AIDoubleBuffSize = initBuffersize;
                        _enableAIDbfMode = true; //使用双缓冲模式
                    }
                    break;
                case AIMode.Continuous:
                    case AIMode.Record:
                    //纠正双缓冲的大小为通道数的偶数倍
                    initalSize = (uint)(_adjustedSampleRate * 0.1 * 2);
                    initalSize = initalSize == 0 ? 1 : initalSize;
                    _AIDoubleBuffSize = GetNearestOfMBlocksize(initalSize, _devHandle.AIDBFBlockSize);
                    _enableAIDbfMode = true; //使用双缓冲模式
                    break;
            }

            //设置Double Buffer Mode
            var adlinkErr = USB6101Import.UD_AI_AsyncDblBufferMode(_devHandle.CardID, _enableAIDbfMode);
            if (adlinkErr < 0)
            {
                JYLog.Print("配置AI双缓冲失败！errorcode={0}", adlinkErr);
                return adlinkErr;
            }
            const int alignment_byte = 16;
            _AIReadbuffer = Marshal.AllocHGlobal((int)_AIDoubleBuffSize * Channels.Count * sizeof(short) + alignment_byte);
            _AIReadbuffer_alignment = new IntPtr(alignment_byte * (((long)_AIReadbuffer + (alignment_byte - 1)) / alignment_byte));
            return JYErrorCode.NoError;
        }

        /// <summary>
        /// 从本地缓冲区中取采集的数据(取到的数据，多个通道是interleaved的)
        /// </summary>
        /// <param name="stopped">停止线程标志</param>
        /// <param name="retbuffer"></param>
        /// <returns>
        /// 小于0：失败，具体看错误代码
        /// 大于0：成功，值代表每通道返回的样点数
        /// </returns>
        private int FetchBuffer(ref short[] retbuffer, ref bool stopped)
        {
            if (_aiStarted == false)
            {
                JYLog.Print("FetchBuffer Error, AI 未启动！");
                return -1;
            }
            short err = 0;
            byte bStopped;
            byte bReady;
            uint dwRdyBufIdx;
            stopped = false;

            if (_enableAIDbfMode == true)
            {
                byte bHalfReady;
                if ((err = USB6101Import.UD_AI_AsyncDblBufferHalfReady(_devHandle.CardID, out bHalfReady, out bStopped)) < 0)
                {
                    JYLog.Print("D2K_AI_AsyncDblBufferHalfReady failed! Error code={0}", err);
                    _threadException.AppendThreadException(new JYDriverException(err, "Inner Exception"));
                    return 0;
                }
                stopped = (bStopped == 1);
                if (bHalfReady == 1)
                {
                    _fetchCnt++;
                    //JYLog.Print("UD_AI_AsyncDblBufferHalfReady Count:{0}", _fetchCnt);
                    err = USB6101Import.UD_AI_AsyncDblBufferTransfer(_devHandle.CardID, _AIReadbuffer_alignment);
                    if (err != USB6101Import.NoError)
                    {
                        JYLog.Print("D2K_AI_AsyncDblBufferHalfReady failed! Error code={0}", err);
                        _threadException.AppendThreadException(new JYDriverException(err, "Inner Exception"));
                        return err;
                    }
                    else
                    {
                        Marshal.Copy(_AIReadbuffer_alignment, retbuffer, 0, (int)(_AIDoubleBuffSize * Channels.Count / 2));
                        return (int)_AIDoubleBuffSize * Channels.Count / 2;
                    }
                }
            }
            else
            {
                uint dwAccessCnt;
                err = USB6101Import.UD_AI_AsyncCheck(_devHandle.CardID, out bStopped, out dwAccessCnt);
                stopped = (bStopped == 1);
                if (err != USB6101Import.NoError)
                {
                    _threadException.AppendThreadException(new JYDriverException(err, "Inner Exception"));
                    return err;
                }
                if (bStopped == 1)
                {
                    err = USB6101Import.UD_AI_AsyncClear(_devHandle.CardID, out dwAccessCnt);
                    if (err != USB6101Import.NoError)
                    {
                        _threadException.AppendThreadException(new JYDriverException(err, "Inner Exception"));
                        return err;
                    }
                    Marshal.Copy(_AIReadbuffer_alignment, retbuffer, 0, (int)(_AIDoubleBuffSize * Channels.Count));
                    return (int)_AIDoubleBuffSize * Channels.Count;
                }
            }
            return 0;
        }
        #endregion
    }    

    #region ----------------AITask需要用到的结构和枚举的定义---------------
    /// <summary>
    /// AI通道参数类
    /// </summary>
    public sealed class AIChannel
    {
        /// <summary>
        /// 通道号。与AI通道的物理序号相对应。
        /// </summary>
        public int ChannelID { get; private set; }

        /// <summary>
        /// 通道量程下限
        /// </summary>
        public double RangeLow { get; set; }

        /// <summary>
        /// 通道量程上限
        /// </summary>
        public double RangeHigh { get; set; }

        /// <summary>
        /// 端口模式配置，仅支持差分
        /// </summary>
        public AITerminal Terminal { get; internal set; }

        /// <summary>
        /// 构造函数，创建AIChnParam对象
        /// </summary>
        /// <param name="channelId">通道物理序号</param>
        /// <param name="rangeLow">通道量程下限</param>
        /// <param name="rangeHigh">通道量程上限</param>
        /// <param name="terminalCfg">输入配置方式</param>
        public AIChannel(int channelId, double rangeLow, double rangeHigh, 
             AITerminal terminalCfg = AITerminal.Differential)
        {
            ChannelID = channelId;
            RangeLow = rangeLow;
            RangeHigh = rangeHigh;
            Terminal = terminalCfg;
        }

    }

    /// <summary>
    /// 触发类型 Digital/Analog/Internal/Immediate
    /// </summary>
    public enum AITriggerType
    {
        /// <summary>
        /// 数字触发
        /// </summary>
        Digital,
        /// <summary>
        /// 无触发
        /// </summary>
        Immediate
    }

    /// <summary>
    /// AI时钟沿类型
    /// </summary>
    public enum AIClockEdge
    {
        /// <summary>
        /// 上升沿
        /// </summary>
        Rising,

        /// <summary>
        /// 下降沿
        /// </summary>
        Falling,
    };


    /// <summary>
    /// AI触发沿类型
    /// </summary>
    public enum AIDigitalTriggerEdge
    {
        /// <summary>
        /// 上升沿
        /// </summary>
        Rising,

        /// <summary>
        /// 下降沿
        /// </summary>
        Falling,
    };

   
    /// <summary>
    /// AI工作模式枚举类型
    /// </summary>
    public enum AIMode
    {
        /// <summary>
        /// 单点方式
        /// </summary>
        Single,

        /// <summary>
        /// 有限点方式
        /// </summary>
        Finite,

        /// <summary>
        /// 连续方式
        /// </summary>
        Continuous,

        /// <summary>
        /// 连续流盘模式
        /// </summary>
        Record
    };

    /// <summary>
    /// 输入配置枚举类型
    /// </summary>
    public enum AITerminal
    {

        /// <summary>
        /// 差分模式
        /// </summary>
        Differential
    };


    /// <summary>
    /// 设置触发参数
    /// </summary>
    public sealed class AITrigger
    {
        /// <summary>
        /// 触发类型，包括：Immediate/DigitalEdge/AnalogEdge
        /// </summary>
        public AITriggerType Type { get; set; }

        /// <summary>
        /// 数字触发设置
        /// </summary>
        public  AIDigitalTrigger Digital { get;}


        /// <summary>
        /// 构造函数
        /// </summary>
        public AITrigger(AITriggerType type = AITriggerType.Immediate,
                               int retriggerCount = 0,  uint delay = 0)
        {
            Digital = new AIDigitalTrigger();
            Type = type;
        }
    }


    /// <summary>
    /// AI数字触发源定义
    /// </summary>
    public enum AIDigitalTriggerSource
    {
        /// <summary>
        /// 外部触发源
        /// </summary>
        IO_0
    }

    /// <summary>
    /// AI数字触发设置参数定义
    /// </summary>
    public sealed class AIDigitalTrigger
    {
        /// <summary>
        /// 触发源
        /// </summary>
        public AIDigitalTriggerSource Source { get;}

        /// <summary>
        /// 数字触发边沿类型，Rising/Falling
        /// </summary>
        public AIDigitalTriggerEdge Edge { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="gate"></param>
        internal AIDigitalTrigger(AIDigitalTriggerEdge edge = AIDigitalTriggerEdge.Rising)
        {
            Source = AIDigitalTriggerSource.IO_0;
            Edge = edge;
        }
    }


    /// <summary>
    /// 流盘相关的参数定义
    /// </summary>
    public sealed class AIRecord
    {
        /// <summary>
        /// 流盘文件的路径，绝对路径，包含文件名
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 流盘文件格式
        /// </summary>
        public FileFormat FileFormat { get; }

        /// <summary>
        /// 流盘模式
        /// </summary>
        public RecordMode Mode { get; set; }

        /// <summary>
        /// 流盘时间长度，单位为秒，当Mode为Finite时有效
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        internal AIRecord()
        {
            FilePath = Environment.CurrentDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + @".bin";
            FileFormat = FileFormat.Bin;
            Mode = RecordMode.Finite;
            Length = 1;
        }
    }

    /// <summary>
    /// 流盘文件格式枚举定义
    /// </summary>
    public enum FileFormat
    {
        /// <summary>
        /// 二进制文件格式
        /// </summary>
        Bin
    }

    /// <summary>
    /// 流盘模式枚举定义
    /// </summary>
    public enum RecordMode
    {
        /// <summary>
        /// 有限长度流盘
        /// </summary>
        Finite,

        /// <summary>
        /// 无限长度流盘
        /// </summary>
        Infinite
    }
    #endregion
}
