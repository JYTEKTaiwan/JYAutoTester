using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Runtime.InteropServices;

namespace JYUSB101
{
    /// <summary>
    /// AO输出任务类
    /// </summary>
    public sealed class JYUSB101AOTask
    {
        /// <summary>
        /// JYUSB101AOTask构造函数
        /// </summary>
        /// <param name="boardNum">板卡编号</param>
        public JYUSB101AOTask(int boardNum)
        {
            //获取板卡操作类的实例
            _devHandle = JYUSB101Device.GetInstance((ushort) boardNum);
            if (_devHandle == null)
            {
                throw new JYDriverException(JYDriverExceptionPublic.InitializeFailed,
                    "Initialization failed, please check board number or connection.");
            }
            _adjustedUpdateRate = 1000; //根据需要的采样率获取真实的采样率
            Mode = AOMode.Finite; //更新模式
            SamplesToUpdate = 1024; //有限点更新时，每通道采样点数
            Channels = new List<AOChannel>();
            Trigger.Type = AOTriggerType.Immediate;
            _bufLenInSamples = 0; //默认为0，任务启动的时候如果用户没有配置过就根据采样率设置为缓冲10s
            _localBuffer = new CircularBufferEx<short>(); //本地软件缓冲区设置
            _waitUntilDoneEvent = new WaitEvent(() => _taskDone); //等待任务结束时间初始化
            EventQueue = new Queue<WaitEvent>(8); //事件队列初始化
            _bufferLenSetByUser = false;
            _threadException = new JYDriverThreadExceptionManager();
        }

        #region -----------------私有字段------------------

        //添加需要使用的私有属性字段

        /// <summary>
        /// 操作硬件的对象
        /// </summary>
        private JYUSB101Device _devHandle;

        /// <summary>
        /// AO是否已启动标志
        /// </summary>
        private bool _aoStarted;

        /// <summary>
        /// 本地缓冲队列
        /// </summary>
        private CircularBufferEx<short> _localBuffer;

        /// <summary>
        /// 后台写硬件缓冲区线程
        /// </summary>
        private Thread _thdWriteData;

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
                else if (Mode != AOMode.Single)
                {
                    _aoStarted = true;
                }

            }

        }

        /// <summary>
        /// WaitUntilDone等待事件
        /// </summary>
        private WaitEvent _waitUntilDoneEvent;

        /// <summary>
        /// AI是否使能了Double Buffer模式
        /// </summary>
        private bool _enableAODbfMode;


        /// <summary>
        /// 硬件是否使能Wrapping
        /// </summary>
        private bool _enableHardwareWrapping;

        /// <summary>
        /// AI硬件双缓冲区的大小
        /// </summary>
        private uint _AODoubleBuffSize;

        /// <summary>
        /// AO写缓冲区的索引号
        /// </summary>
        private byte _aoWritebufIdx;

        /// <summary>
        /// AO缓冲区
        /// </summary>
        private short[] _AOWriteBuffer;

        /// <summary>
        /// 每通道写入到硬件缓冲区的点数
        /// </summary>
        private int _samplesWrittenPerChannel;

        /// <summary>
        /// 等待锁, 用于限制多线程并行写操作. 需要等一个线程读取完成后, 另一个线程才能读(排队).
        /// </summary>
        private StatusLock _startedLock = new StatusLock(); // 控制对_buffer的修改

        /// <summary>
        /// 等待锁, 用于限制多线程并行写操作. 需要等一个线程写入完成后, 另一个线程才能写(排队).
        /// </summary>
        private object _waitLock = new object();

        /// <summary>
        /// 事件队列。调用WaitUntilDone()或者ReadBuffer()时，使用事件通知方式，提高效率。
        /// </summary>
        private Queue<WaitEvent> EventQueue { get; }

        /// <summary>
        /// AO采样间隔
        /// </summary>
        private uint _AOScanInterval;

        /// <summary>
        /// 缓冲区下溢出标志
        /// </summary>
        private bool _isUnderflow;

        /// <summary>
        /// NoWrapping模式下第一次写入的数据点数至少是10K
        /// </summary>
        private readonly uint _firstWriteSamples = 1000;

        /// <summary>
        /// NoWrapping模式时，最小的DoubleBuffer
        /// </summary>
        private readonly uint _doublebufferSizeMin = 2560;

        /// <summary>
        /// 用于标志用户是否设置过BufLenInSamples变量
        /// </summary>
        private bool _bufferLenSetByUser;

        /// <summary>
        /// 线程抛出的异常管理
        /// </summary>
        private JYDriverThreadExceptionManager _threadException;

        #endregion

        #region -------------------公共属性---------------------

        /// <summary>
        /// 通道列表
        /// </summary>
        public List<AOChannel> Channels { get; }

        /// <summary>
        /// 输出模式，支持Single/Finite/Continuous三种类型
        /// </summary>
        public AOMode Mode { get; set; }

        private double _adjustedUpdateRate;

        /// <summary>
        /// 每通道更新速率
        /// </summary>
        /// <remarks>若硬件不支持硬件定时，则忽略设置的速率值</remarks>
        public double UpdateRate
        {
            get { return _adjustedUpdateRate; }
            set
            {
                //需要根据分频系数反算真实采样率
                _adjustedUpdateRate = value;
            }
        }

        /// <summary>
        /// 每通道更新点数，在有限点模式下有效
        /// </summary>
        public int SamplesToUpdate { get; set; }

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
                _bufferLenSetByUser = true;
                Interlocked.Exchange(ref _bufLenInSamples, value);
            }
        }

        /// <summary>
        /// 本地缓冲区当前每通道可容纳的样点数（当前每通道可写入缓冲区的样点数）
        /// </summary>
        public int AvaliableLenInSamples => _bufLenInSamples - _localBuffer.NumOfElement/Channels.Count;


        /// <summary>
        /// AO触发参数配置
        /// </summary>
        public AOTrigger Trigger { get; set; } = new AOTrigger();

        #endregion

        #region --------------公共方法定义-----------------

        /// <summary>
        /// 添加通道
        /// </summary>
        /// <param name="chnId">通道物理序号</param>
        public void AddChannel(int chnId)
        {
            if (_aoStarted == true)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            //To Add 添加通道的代码  
            if (-1 == chnId)
            {
                Channels.Clear();
                var chnCount = (int) _devHandle.AOChannelCount;
                for (var i = 0; i < chnCount; i++)
                {
                    var chn = new AOChannel(i, -5, 5);
                    Channels.Add(chn);
                }
            }
            else
            {
                if (chnId < 0 || chnId >= (int) _devHandle.AOChannelCount)
                {
                    throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
                }
                var chn = new AOChannel(chnId, -5, 5);
                Channels.Add(chn);
            }
        }

        /// <summary>
        /// 添加多通道
        /// </summary>
        /// <param name="chnsId">要添加通道的所有物理序号</param>
        public void AddChannel(int[] chnsId)
        {
            if (_aoStarted == true)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            //To Add 添加通道的代码  

            if (chnsId.Max() >= _devHandle.AOChannelCount || chnsId.Min() < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "Add Channels Error.");
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            foreach (var item in chnsId)
            {
                var chn = new AOChannel(item, -5, 5);
                Channels.Add(chn);
            }
        }

        /// <summary>
        /// 删除指定通道号的通道
        /// </summary>
        /// <param name="chnId">要删除的通道的通道号，-1删除所有通道</param>
        public void RemoveChannel(int chnId)
        {
            if (_aoStarted == true)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            //To Add 删除通道的代码  
            if (-1 == chnId)
            {
                JYLog.Print(JYLogLevel.DEBUG, "Clear All AO Channels!");
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
        /// 将数据写入到缓冲区，多通道数据写入
        /// </summary>
        /// <param name="buf">要写入的数据，每通道按列存放</param>
        /// <param name="timeout">操作时间</param> 
        public void WriteData(double[,] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            var tmpBuf = new short[buf.GetLength(0), buf.GetLength(1)];
            //To Add: Scale To Raw Data
            for (var i = 0; i < buf.GetLength(0); i++)
            {
                for (var j = 0; j < buf.GetLength(1); j++)
                {
                    tmpBuf[i, j] = (short)((uint)((buf[i ,j] + 5)/5 * 0x7FF));
                }
            }
            WriteRawData(tmpBuf, timeout);
            tmpBuf = null;
            GC.Collect();
        }

        /// <summary>
        /// 将数据写入到缓冲区，单通道输入写入
        /// </summary>
        /// <param name="buf">要写入的数据，多通道时数据是按行交错存放</param>
        /// <param name="timeout">操作时间</param> 
        public void WriteData(double[] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            var tmpBuf = new short[buf.Length/Channels.Count, Channels.Count];
            //To Add: Scale To Raw Data
            for (var i = 0; i < buf.Length/Channels.Count; i++)
            {
                for (var j = 0; j < Channels.Count; j++)
                {
                    tmpBuf[i, j] = (short) ((uint) ((buf[i*Channels.Count + j] + 5) /5*0x7FF));
                }
            }
            WriteRawData(tmpBuf, timeout);
            tmpBuf = null;
            GC.Collect();
        }

        /// <summary>
        /// 将数据写入到缓冲区，多通道数据写入
        /// </summary>
        /// <param name="buf">要写入的数据，每通道按列存放</param>
        /// <param name="timeout">操作时间</param> 
        public unsafe void WriteData(IntPtr buf, int samplesToWrite, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            var tmpBuf = Marshal.AllocHGlobal(samplesToWrite * Channels.Count * sizeof(ushort));

            try
            {
                //To Add: Scale To Raw Data
                for (var i = 0; i < Channels.Count; i++)
                {
                    for (var j = 0; j < samplesToWrite; j++)
                    {
                        *((short*)tmpBuf + i + j * Channels.Count) = (short)((*((double*)buf + i + j * Channels.Count) + 5) / 5 * 0x7FF);
                        //tmpBuf[i, j] = (short)((uint)((buf[i, j] + 5) / 5 * 0x7FF));
                    }
                }
                WriteRawData(tmpBuf, timeout);
            }
            finally
            {
                Marshal.FreeHGlobal(tmpBuf);
            }
            GC.Collect();
        }

        /// <summary>
        /// 将原始数据写入到缓冲区，多通道写入
        /// </summary>
        /// <param name="buf">要写入的数据，每通道按列存放</param>
        /// <param name="samplesToWrite"></param>
        /// <param name="timeout">操作时间</param>
        public unsafe void WriteRawData(IntPtr buf, int samplesToWrite, int timeout = -1)
        {
            if (Mode == AOMode.Single)
            {
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            if (_taskDone && AvaliableLenInSamples < SamplesToUpdate)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            if (Mode == AOMode.Finite && SamplesToUpdate <= 0)
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            if (samplesToWrite > BufLenInSamples)
            {
                _bufLenInSamples = samplesToWrite; //预分配缓冲区
            }
            //if ((buf.GetLength(1) < Channels.Count || Mode == AOMode.Finite && buf.GetLength(0) < SamplesToUpdate))
            //{
            //    throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            //}
            short[,] adjustBuf = null;
            int length;

            _threadException.ReportThreadExceptions();
            switch (Mode)
            {
                case AOMode.Single:
                    break;
                case AOMode.Finite:
                    length = Channels.Count;
                    var adjustLength = GetNearestOfMBlocksize((uint)length, _devHandle.AODBFBlockSize);
                    adjustBuf = new short[adjustLength, samplesToWrite];
                    for (var i = 0; i < length; i++)
                    {
                        for (var j = 0; j < samplesToWrite; j++)
                        {
                            //adjustBuf[i, j] = buf[i, j];
                            adjustBuf[i, j] = (short)((double*)buf + i + j * Channels.Count);
                        }
                    }
                    for (var i = length; i < adjustLength; i++) //补最后个点
                    {
                        for (var j = 0; j < samplesToWrite ; j++)
                        {
                            //adjustBuf[i, j] = buf[length - 1, j];
                            adjustBuf[i, j] = (short)((double*)buf + length - 1 + j * Channels.Count);
                        }
                    }
                    break;
                case AOMode.ContinuousNoWrapping:
                    adjustBuf = (short[,])(object)buf;
                    break;
                case AOMode.ContinuousWrapping:
                    var temp = 1;
                    length = Channels.Count;
                    while (length % 512 != 0 || length < 20480)
                    {
                        temp++;
                        length += Channels.Count;
                        if (length >= 20480)
                        {
                            break;
                        }
                    }
                    if (temp > 1)
                    {
                        adjustBuf = new short[Channels.Count * temp, samplesToWrite];
                        //将buf的数据复制temp份到adjustBuf中,使得adjustBuf数据量是512的倍数，并且大于10k
                        for (var i = 0; i < temp; i++)
                        {
                            for (var j = 0; j < Channels.Count; j++)
                            {
                                for (var k = 0; k < samplesToWrite; k++)
                                {
                                    //adjustBuf[i * Channels.Count + j, k] = buf[j, k];
                                    adjustBuf[i * Channels.Count + j, k] = (short)((double*)buf + j + k * Channels.Count);
                                }
                            }
                        }
                    }
                    else
                    {
                        adjustBuf = (short[,])(object)buf;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (adjustBuf != null && adjustBuf.GetLength(0) > _bufLenInSamples)
            {
                _bufLenInSamples = adjustBuf.GetLength(0); //预分配缓冲区
            }
            var retWroteSamples = 0;
            lock (_startedLock)
            {
                //To Add: 如果是第一次写入，则写入本地缓冲区，在Start时刻使用
                if (Mode == AOMode.ContinuousWrapping && _startedLock.Marked)
                {
                    //EnableWrapping，且任务已启动，此时不允许写入
                    throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
                }
                if (!_startedLock.Marked && !_aoStarted) //任务未启动时
                {
                    if (_localBuffer.BufferSize < _bufLenInSamples * Channels.Count)
                    {
                        //_localBuffer.AdjustSize(_bufLenInSamples * Channels.Count);
                        _localBuffer = new CircularBufferEx<short>(_bufLenInSamples * Channels.Count);
                    }
                    _localBuffer.Clear();
                    _localBuffer.Enqueue(adjustBuf); //数据放入本地缓冲区队列
                    if (adjustBuf != null) retWroteSamples = adjustBuf.Length / Channels.Count;
                    JYLog.Print(JYLogLevel.DEBUG, "First Write {0} Samples...", retWroteSamples);
                    return;
                }
            }

            _threadException.ReportThreadExceptions();
            var isTimeout = false;
            lock (_waitLock) //防止多个线程同时写入；要求“排队”写入。
            {
                //To Add: 等待缓冲区内的数据足够之后进行读取
                var waitEvent = new WaitEvent(() => adjustBuf != null && (TaskDone
                                                                          ||
                                                                          (AvaliableLenInSamples >=
                                                                           adjustBuf.Length / Channels.Count)));
                if (!waitEvent.EnqueueWait(EventQueue, timeout))
                {
                    isTimeout = true;
                }
                var avaliableSamples = AvaliableLenInSamples;
                //缓冲区在超时时间内空间不够（即超时了），或者任务已经结束，则写入0点到本地缓冲区，并返回错误
                if (adjustBuf != null && ((avaliableSamples < adjustBuf.Length / Channels.Count) || TaskDone))
                {
                    JYLog.Print(JYLogLevel.ERROR, "写入失败，TaskDone={0}, isTimeout={1}", TaskDone, isTimeout);
                    if (TaskDone)
                    {
                        throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
                    }
                    throw new JYDriverException(JYDriverExceptionPublic.TimeOut);
                }
                if (adjustBuf != null)
                {
                    _localBuffer.Enqueue(adjustBuf);
                    JYLog.Print(JYLogLevel.DEBUG, "{0}个通道，写入{1}个点到本地缓冲！",
                        Channels.Count, adjustBuf.Length / Channels.Count);
                }
            }
            if (_isUnderflow)
            {
                _isUnderflow = false;
                JYLog.Print(JYLogLevel.ERROR, "Buffer UnderFlow! ");
                throw new JYDriverException(JYDriverExceptionPublic.BufferDownflow);
            }
            GC.Collect();
        }
        
        /// <summary>
        /// 将原始数据写入到缓冲区，多通道写入
        /// </summary>
        /// <param name="buf">要写入的数据，每通道按列存放</param>
        /// <param name="timeout">操作时间</param>
        public void WriteRawData(short[,] buf, int timeout = -1)
        {
            if (Mode == AOMode.Single)
            {
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            if (_taskDone && AvaliableLenInSamples < SamplesToUpdate)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            if (Mode == AOMode.Finite && SamplesToUpdate <= 0)
            {
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            if ((buf.GetLength(1) < Channels.Count || Mode == AOMode.Finite && buf.GetLength(0) < SamplesToUpdate))
            {
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }
            short[,] adjustBuf = null;
            int length;

            _threadException.ReportThreadExceptions();
            switch (Mode)
            {
                case AOMode.Single:
                    break;
                case AOMode.Finite:
                    length = buf.GetLength(0);
                    var adjustLength = GetNearestOfMBlocksize((uint) length, _devHandle.AODBFBlockSize);
                    adjustBuf = new short[adjustLength, buf.GetLength(1)];
                    for (var i = 0; i < length; i++)
                    {
                        for (var j = 0; j < buf.GetLength(1); j++)
                        {
                            adjustBuf[i, j] = buf[i, j];
                        }
                    }
                    for (var i = length; i < adjustLength; i++) //补最后个点
                    {
                        for (var j = 0; j < buf.GetLength(1); j++)
                        {
                            adjustBuf[i, j] = buf[length - 1, j];
                        }
                    }
                    break;
                case AOMode.ContinuousNoWrapping:
                    adjustBuf = buf;
                    break;
                case AOMode.ContinuousWrapping:
                    var temp = 1;
                    length = buf.GetLength(0);
                    while (length%512 != 0 || length < 20480)
                    {
                        temp++;
                        length += buf.GetLength(0);
                        if (length >= 20480)
                        {
                            break;
                        }
                    }
                    if (temp > 1)
                    {
                        adjustBuf = new short[buf.GetLength(0)*temp, buf.GetLength(1)];
                        //将buf的数据复制temp份到adjustBuf中,使得adjustBuf数据量是512的倍数，并且大于10k
                        for (var i = 0; i < temp; i++)
                        {
                            for (var j = 0; j < buf.GetLength(0); j++)
                            {
                                for (var k = 0; k < buf.GetLength(1); k++)
                                {
                                    adjustBuf[i*buf.GetLength(0) + j, k] = buf[j, k];
                                }
                            }
                        }
                    }
                    else
                    {
                        adjustBuf = buf;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (adjustBuf != null && adjustBuf.GetLength(0) > _bufLenInSamples)
            {
                _bufLenInSamples = adjustBuf.GetLength(0); //预分配缓冲区
            }
            var retWroteSamples = 0;
            lock (_startedLock)
            {
                //To Add: 如果是第一次写入，则写入本地缓冲区，在Start时刻使用
                if (Mode == AOMode.ContinuousWrapping && _startedLock.Marked)
                {
                    //EnableWrapping，且任务已启动，此时不允许写入
                    throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
                }
                if (!_startedLock.Marked && !_aoStarted) //任务未启动时
                {
                    if (_localBuffer.BufferSize < _bufLenInSamples*Channels.Count)
                    {
                        //_localBuffer.AdjustSize(_bufLenInSamples*Channels.Count);
                        _localBuffer = new CircularBufferEx<short>(_bufLenInSamples * Channels.Count);
                    }
                    _localBuffer.Clear();
                    _localBuffer.Enqueue(adjustBuf); //数据放入本地缓冲区队列
                    if (adjustBuf != null) retWroteSamples = adjustBuf.Length/Channels.Count;
                    JYLog.Print(JYLogLevel.DEBUG, "First Write {0} Samples...", retWroteSamples);
                    return;
                }
            }

            _threadException.ReportThreadExceptions();
            var isTimeout = false;
            lock (_waitLock) //防止多个线程同时写入；要求“排队”写入。
            {
                //To Add: 等待缓冲区内的数据足够之后进行读取
                var waitEvent = new WaitEvent(() => adjustBuf != null && (TaskDone
                                                                          ||
                                                                          (AvaliableLenInSamples >=
                                                                           adjustBuf.Length/Channels.Count)));
                if (!waitEvent.EnqueueWait(EventQueue, timeout))
                {
                    isTimeout = true;
                }
                var avaliableSamples = AvaliableLenInSamples;
                //缓冲区在超时时间内空间不够（即超时了），或者任务已经结束，则写入0点到本地缓冲区，并返回错误
                if (adjustBuf != null && ((avaliableSamples < adjustBuf.Length/Channels.Count) || TaskDone))
                {
                    JYLog.Print(JYLogLevel.ERROR, "写入失败，TaskDone={0}, isTimeout={1}", TaskDone, isTimeout);
                    if (TaskDone)
                    {
                        throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
                    }
                    throw new JYDriverException(JYDriverExceptionPublic.TimeOut);
                }
                if (adjustBuf != null)
                {
                    _localBuffer.Enqueue(adjustBuf);
                    JYLog.Print(JYLogLevel.DEBUG, "{0}个通道，写入{1}个点到本地缓冲！",
                        Channels.Count, adjustBuf.Length/Channels.Count);
                }
            }
            if (_isUnderflow)
            {
                _isUnderflow = false;
                JYLog.Print(JYLogLevel.ERROR, "Buffer UnderFlow! ");
                throw new JYDriverException(JYDriverExceptionPublic.BufferDownflow);
            }
        }

        /// <summary>
        /// 将原始数据写入到缓冲区，单通道写入
        /// </summary>
        /// <param name="buf">要写入的数据，多通道时，数据是按行交错存放</param>
        /// <param name="timeout">操作时间</param>
        public void WriteRawData(short[] buf, int timeout = -1)
        {
            _threadException.ReportThreadExceptions();
            var tmpbuf = new short[buf.Length/Channels.Count, Channels.Count];
            Buffer.BlockCopy(buf, 0, tmpbuf, 0, buf.Length*sizeof(short));
            WriteRawData(tmpbuf, timeout);
        }

        /// <summary>
        /// 每通道更新一个点。直接更新，不经过缓冲区。
        /// </summary>
        public void WriteSinglePoint(double[] buf)
        {
            lock (this)
            {
                if (Mode == AOMode.Single)
                {
                    this.Start();
                    for (var i = 0; i < Channels.Count; i++)
                    {
                        int err = USB6101Import.UD_AO_VWriteChannel(_devHandle.CardID, (ushort) Channels[i].ChannelID,
                            buf[i]);
                        if (err == USB6101Import.NoError) continue;
                        Stop();
                        JYLog.Print(JYLogLevel.ERROR, "Write Single AO Failed! Code={0}", err);
                        throw new JYDriverException(err);
                    }
                }
                else
                {
                    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
                }
            }
        }

        /// <summary>
        /// 单通道时，更新一个点。直接更新，不经过缓冲区。
        /// </summary>
        public void WriteSinglePoint(double buf)
        {
            lock (this)
            {
                if (Mode == AOMode.Single && Channels.Count == 1)
                {
                    var b = new double[1] {buf};
                    WriteSinglePoint(b);
                }
                else
                {
                    JYLog.Print("Write single AO failed for channel count > 1");
                    throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
                }
            }
        }

        /// <summary>
        /// 等待当前任务完成
        /// </summary>
        /// <param name="timeout">等待的时间(单位:ms)</param>
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
        /// 启动AO任务
        /// </summary>
        public void Start()
        {
            if (_aoStarted)
            {
                JYLog.Print(JYLogLevel.DEBUG, "AO已经启动，忽略了Start的调用");
                return;
            }
            _threadException.ClearThreadExceptions();
            if (_devHandle.AOReserved)
            {
                throw new JYDriverException(JYDriverExceptionPublic.HardwareResourceReserved);
            }
            int ret;
            _isUnderflow = false;
            if (Mode == AOMode.ContinuousWrapping &&
                ((_localBuffer.NumOfElement/Channels.Count)%_devHandle.AODBFBlockSize) == 0)
            {
                _bufLenInSamples = (_localBuffer.NumOfElement/Channels.Count);
            }
            else if (!_bufferLenSetByUser)
            {
                _bufLenInSamples = (int)Math.Max((UpdateRate*10),1024*100);
            }
            else if (_bufferLenSetByUser && _bufLenInSamples < 30720) //缓冲区不能设置小于30K
            {
                JYLog.Print(JYLogLevel.ERROR, "采样点缓冲区设置不能低于30720");
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }

            //To Add: 配置硬件的AO参数，将本地缓冲区中的数据写入硬件缓冲区，启动AO输出
            //如果是连续NoWrapping模式则还需要启动线程不断从本地缓冲区取出数据写入到硬件缓冲区
            if ((ret = AOConfig()) < 0) //配置AI的触发，通道等
            {
                JYLog.Print(JYLogLevel.ERROR, $"AOConfig fail,error code={ret}");
                throw new JYDriverException(ret);
            }
            if (Mode == AOMode.Single)
            {
                _aoStarted = true;
                _devHandle.AOReserved = true;
                return;
            }
            _samplesWrittenPerChannel = 0;
            if ((ret = ConfigContAO()) != 0)
            {
                JYLog.Print(JYLogLevel.ERROR, $"ConfigContAO fail,error code={ret}");
                throw new JYDriverException(ret);
            }

            if (_localBuffer.NumOfElement < _firstWriteSamples && Mode == AOMode.ContinuousNoWrapping)
            {
                JYLog.Print("NoWrapping 模式下第一次写入的点数小于30K");
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError, "ContinuousNoWrapping模式，首次写入的点数应大于30*1024点！");
            }

            if (_localBuffer.NumOfElement < _AODoubleBuffSize*Channels.Count)
            {
                //如果不是例外情况：有限点，不使能double buffer模式可能写入的点数小于buffersize，则返回错误
                if (!(Mode == AOMode.Finite && _enableAODbfMode == false &&
                      GetNearestOfMBlocksize((uint) (_localBuffer.NumOfElement/Channels.Count),
                          _devHandle.AONDBFBlockSize) >= _AODoubleBuffSize))
                {
                    JYLog.Print(JYLogLevel.ERROR, "本地缓冲区中的数据不足！");
                    throw new JYDriverException(JYDriverExceptionPublic.BufferDownflow);
                }
            }

            if (_localBuffer.BufferSize < _bufLenInSamples*Channels.Count)
            {
                var tmpArray = new short[_localBuffer.NumOfElement];
                _localBuffer.Dequeue(ref tmpArray, tmpArray.Length);
                _localBuffer = new CircularBufferEx<short>(_bufLenInSamples*Channels.Count);
                _localBuffer.Enqueue(tmpArray);
                tmpArray = null;
            }
            _thdWriteData = new Thread(ThdWriteData);
            _aoWritebufIdx = 0;

            var firstWriteCount = (int) _AODoubleBuffSize *Channels.Count;
            //对于有限点输出，可能由于调整到Block的整数倍，而实际写入点没有缓冲区大小那么多，则忽略
            if (Mode == AOMode.Finite && _enableAODbfMode == false)
            {
                if (_localBuffer.NumOfElement < firstWriteCount)
                {
                    firstWriteCount = _localBuffer.NumOfElement;
                }
            }
            _localBuffer.Dequeue(ref _AOWriteBuffer, firstWriteCount);
            if (Mode == AOMode.ContinuousWrapping && _enableHardwareWrapping == false)
            {
                //对于环绕模式，但硬件并未环绕的情况，需要将Dequeue的数据再次Enqueue到本地缓冲区
                _localBuffer.Enqueue(_AOWriteBuffer.Take(firstWriteCount).ToArray());
            }
            _devHandle.AOReserved = true;
            //StopContAO
            byte bStopped;
            uint dwAccessCnt;
            ret = USB6101Import.UD_AO_AsyncCheck(_devHandle.CardID, out bStopped, out dwAccessCnt);
            if (ret != USB6101Import.NoError)
            {
                JYLog.Print(JYLogLevel.WARN, "UD_AO_AsyncCheck Failed! Error Code:{0}", ret);
            }
            if (bStopped != 1)
            {
                uint accessCnt;
                ret = USB6101Import.UD_AO_AsyncClear(_devHandle.CardID, out accessCnt, 0); //启动前先停掉之前没有停掉的AO操作
                if (ret < 0)
                {
                    JYLog.Print(JYLogLevel.WARN, "UD_AO_AsyncClear Failed! Error Code:{0}", ret);
                }
            }
            var adlinkErr = USB6101Import.UD_AO_ContWriteMultiChannels(_devHandle.CardID, (ushort) Channels.Count,
                GetChannelArray(),
                _AOWriteBuffer, (uint) (_AODoubleBuffSize),
                (uint) (_enableAODbfMode ? 0 : 1), _AOScanInterval,
                (ushort) (_enableAODbfMode ? 0 : 1), USB6101Import.ASYNCH_OP);
            if (adlinkErr >= 0)
            {
                _aoStarted = true;
                TaskDone = false;
                _samplesWrittenPerChannel += (int) _AODoubleBuffSize;
                JYLog.Print(JYLogLevel.DEBUG, "AO Started OK!");
                _thdWriteData.Start();
                _startedLock.Mark();
            }
            else
            {
                _devHandle.AOReserved = false;
                throw new JYDriverException(adlinkErr);
            }
        }

        /// <summary>
        /// 停止AO任务
        /// </summary>
        public void Stop()
        {
            if (!_aoStarted) //已经调用过Stop了，直接返回
            {
                JYLog.Print(JYLogLevel.DEBUG, "已经调用过Stop了");
                return;
            }
            _taskDone = true;
            _aoStarted = false;
            _startedLock.UnMark();
            _devHandle.AOReserved = false;

            //本地缓冲区清理
            if (_localBuffer != null)
            {
                _localBuffer.Clear();
            }

            //单点模式直接退出，不用等线程结束
            if (Mode == AOMode.Single)
            {
                return;
            }
            //连续或有限点采集时，需要停止从硬件取数据的线程
            if (_thdWriteData.IsAlive)
            {
                if (false == _thdWriteData.Join(200))
                {
                    _thdWriteData.Abort();
                    JYLog.Print(JYLogLevel.DEBUG, "Update Thread Exit Abnormally...");
                }
            }
            JYLog.Print(JYLogLevel.DEBUG, "Update Thread Exit OK...");
            if (_devHandle == null) return;
            //StopContAI
            uint accessCnt;
            int ret = USB6101Import.UD_AO_AsyncClear(_devHandle.CardID, out accessCnt, 0);
            if (ret < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "UD_AO_AsyncClear Failed! Error Code:{0}", ret);
                throw new JYDriverException(ret);
            }
            JYLog.Print(JYLogLevel.DEBUG, "UD_AO_AsyncClear OK...accessCnt = {0}", accessCnt);
            _threadException.ReportThreadExceptions();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~JYUSB101AOTask()
        {
            JYLog.Print("deconstructror calling...");
            Stop();
            _devHandle = null;
        }

        #endregion

        #region -------------私有方法定义-------------

        //此段定义私有方法
        /// <summary>
        /// 将本地缓冲区的数据写入到硬件
        /// </summary>
        private void ThdWriteData()
        {
            short[] buffer = null;
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

            if (Mode == AOMode.ContinuousNoWrapping ||
                Mode == AOMode.ContinuousWrapping && _enableHardwareWrapping == false)
            {
                try
                {
                    buffer = new short[_AODoubleBuffSize*Channels.Count/2];
                }
                catch (Exception ex)
                {
                    _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
                }
            }
            while (TaskDone == false)
            {
                //To Add: 以下添加从本地缓冲区取数据写入硬件缓冲区的代码，同时需要修改_sampleWritten的值        
                var stoped = false;
                if (Mode == AOMode.Finite)
                {
                    if (true == GetAOState(out stoped))
                    {
                        _samplesWrittenPerChannel += SamplesToUpdate;
                        TaskDone = true;
                        ActivateWaitEvents();
                        break;
                    }
                }
                else
                {
                    if (true == GetAOState(out stoped))
                    {
                        //返回true则表示half ready == 1    
                        if (_enableHardwareWrapping == false)
                        {
                            if (buffer != null)
                            {
                                try
                                {
                                    if (_localBuffer.Dequeue(ref buffer, buffer.Length) == -1)
                                    {
                                        _threadException.AppendThreadException(new JYDriverException(JYErrorCode.UserBufferError, "用户写入的数据不够"));
                                    }
                                    var ret = WriteBuffer((ushort[])((object)buffer));
                                    if (Mode == AOMode.ContinuousWrapping && _enableHardwareWrapping == false)
                                    {
                                        //对于环绕模式，但硬件并未环绕的情况，需要将Dequeue的数据再次Enqueue到本地缓冲区
                                        _localBuffer.Enqueue(buffer);
                                    }
                                    _samplesWrittenPerChannel += buffer.Length / Channels.Count;
                                    //JYLog.Print("写入{0}个点到硬件缓冲区！_samplesWrittenPerChannel={1}, ret={2},buffer max = {3}, local buffer count = {4}",
                                    //    buffer.Length / Channels.Count, _samplesWrittenPerChannel, ret, buffer.Max(), _localBuffer.NumOfElement);
                                }
                                catch (Exception ex)
                                {
                                    _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
                                    stopAction.BeginInvoke(null, null);
                                    TaskDone = true;
                                }        
                            }
                        }
                    }
                    if (stoped == true)
                    {
                        TaskDone = true;
                        ActivateWaitEvents();
                    }
                    ActivateWaitEvents();
                }
                Thread.Sleep(1);
            }
            JYLog.Print("Task Done!");
        }

        /// <summary>
        /// 写缓冲区
        /// </summary>
        /// <param name="aobuffer"></param>
        /// <returns></returns>
        private int WriteBuffer(ushort[] aobuffer)
        {
            try
            {
                short err = 0;
                if (_aoWritebufIdx == 0) //根据_writebufIdx决定写哪一个缓冲区,如何EnableWrapping则两个缓冲区都写一样的数据
                {
                    if ((err = USB6101Import.UD_AO_AsyncDblBufferTransfer(_devHandle.CardID, 0, aobuffer)) < 0)
                    {
                        if ((err = USB6101Import.UD_AO_AsyncDblBufferTransfer(_devHandle.CardID, 1, aobuffer)) < 0)
                        {
                            _threadException.AppendThreadException(new JYDriverException("Inner Exception",new JYDriverException(err)));
                            return err;
                        }
                    }
                }
                if (_aoWritebufIdx == 1)
                {
                    if ((err = USB6101Import.UD_AO_AsyncDblBufferTransfer(_devHandle.CardID, 1, aobuffer)) < 0)
                    {
                        if ((err = USB6101Import.UD_AO_AsyncDblBufferTransfer(_devHandle.CardID, 0, aobuffer)) < 0)
                        {
                            _threadException.AppendThreadException(new JYDriverException("Inner Exception", new JYDriverException(err)));
                            return err;
                        }
                    }
                }
                _aoWritebufIdx = (byte)(_aoWritebufIdx == 0 ? 1 : 0);
            }
            catch(Exception ex)
            {
                _threadException.AppendThreadException(new JYDriverException("Inner Exception", ex));
                return -2;
            }
            return 0;
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
                if (!waitEvent.IsEnabled) continue; //Just Dequeue when no one is waiting

                if (TaskDone || waitEvent.ConditionHandler())
                    waitEvent.Set();
                else
                    EventQueue.Enqueue(waitEvent);
            }
        }

        /// <summary>
        /// 根据输入范围匹配一个原厂驱动的Range值
        /// </summary>
        /// <param name="rangeLow">输入下限</param>
        /// <param name="rangeHigh">输入上限</param>
        /// <returns>
        /// 小于0：错误
        /// 大于0：实际的Range
        /// </returns>
        private int GetVendorRange(double rangeLow, double rangeHigh)
        {
            if (rangeLow >= rangeHigh || rangeLow < -5|| rangeHigh > 5)
            {
                return JYErrorCode.ErrorParam;
            }
            return USB6101Import.AD_B_5_V;
        }

        /// <summary>
        /// 配置AO的触发及时钟属性
        /// </summary>
        /// <returns></returns>
        private int AOConfig()
        {
            ushort wConvSrc = 0;
            ushort wTriggerMode = 0;
            ushort wTrigCtrl = 0;
            if (Mode != AOMode.Single)
            {
                var maxRate = _devHandle.IsAOSync
                    ? _devHandle.MaxUpdateRateSingleChannel
                    : _devHandle.MaxUpdateRateSingleChannel/Channels.Count;
                if (UpdateRate > maxRate)
                {
                    JYLog.Print(JYLogLevel.ERROR,"设置的更新率超过了最大更新率");
                    return JYErrorCode.ErrorParam;
                }
                _AOScanInterval = (uint) (_devHandle.AOTimeBase/UpdateRate);
                wConvSrc = USB6101Import.UD_AO_CONVSRC_INT;
                
            }


            #region Trigger Type and Edge Setting

            if (Trigger.Type != AOTriggerType.Immediate)
            {
                wTriggerMode = USB6101Import.UD_AO_TRGMOD_POST;
                wTrigCtrl |= USB6101Import.UD_AO_TRGSRC_DTRIG;

                if (Trigger.Digital.Edge == AODigitalTriggerEdge.Rising)
                {
                    wTrigCtrl |= USB6101Import.UD_AO_TrigPositive;
                }
                else
                {
                    wTrigCtrl |= USB6101Import.UD_AO_TrigNegative;
                }
            }
            else
            {
                wTrigCtrl |= USB6101Import.UD_AO_TRGSRC_SOFT;
            }

            #endregion
            return USB6101Import.UD_AO_Trigger_Config(_devHandle.CardID, wConvSrc, wTriggerMode, wTrigCtrl);
        }

        /// <summary>
        ///为连续模拟输出设置缓冲区
        /// </summary>
        /// <returns></returns>
        private int ConfigContAO()
        {
            _enableAODbfMode = true;
            //缓冲区的大小,双缓冲时，为每一个缓冲区的大小（大小都是所有通道大小的总和）
            var samplesInBuffer = _localBuffer.NumOfElement / Channels.Count;
            switch (Mode)
            {
                case AOMode.Single:
                    _enableAODbfMode = false;
                    return JYErrorCode.CannotCall;
                case AOMode.ContinuousWrapping:
                    if (samplesInBuffer % _devHandle.AODBFBlockSize == 0)
                    {
                        _AODoubleBuffSize = (uint)samplesInBuffer;
                        _enableHardwareWrapping = true;
                    }
                    else  //用软件做Wrapping输出
                    {
                        _AODoubleBuffSize = GetNearestOfMBlocksize((uint)samplesInBuffer, _devHandle.AODBFBlockSize);
                        _AODoubleBuffSize -= _devHandle.AODBFBlockSize;
                        _enableHardwareWrapping = false;
                    }
                    break;
                case AOMode.ContinuousNoWrapping:
                    //写入的数据刚好是AO块的整数倍，则直接使用硬件的Wrapping模式
                    var initialSize = (uint)(_adjustedUpdateRate / 5);
                    _AODoubleBuffSize = Math.Max(GetNearestOfMBlocksize(initialSize, _devHandle.AODBFBlockSize), _doublebufferSizeMin); 
                    break;
                case AOMode.Finite:
                    _AODoubleBuffSize = GetNearestOfMBlocksize((uint)SamplesToUpdate, _devHandle.AODBFBlockSize);
                    _enableAODbfMode = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var adlinkErr = USB6101Import.UD_AO_AsyncDblBufferMode(_devHandle.CardID, _enableAODbfMode, false);
            if (adlinkErr < 0)
            {
                return adlinkErr;
            }
            _AOWriteBuffer = new short[((int)_AODoubleBuffSize * Channels.Count)];
            return JYErrorCode.NoError;
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
            var tmp = requestSize;
            while (true)
            {
                if (tmp % blockSize == 0)
                {
                    break;
                }
                else
                {
                    tmp++;
                }
            }
            return tmp;
        }

        /// <summary>
        /// 获取当前添加通道的数组
        /// </summary>
        /// <returns></returns>
        private ushort[] GetChannelArray()
        {
            var channelsArray = new ushort[Channels.Count];
            for (var i = 0; i < Channels.Count; i++)
            {
                channelsArray[i] = (ushort)(Channels[i].ChannelID);
            }
            return channelsArray;
        }

        /// <summary>
        /// 取得当前AO更新的状态,half ready 则返回true，有限点完成输出了也返回true
        /// </summary>
        /// <returns></returns>
        private bool GetAOState(out bool isStoped)
        {
            if (Mode == AOMode.Finite)
            {
                return isStoped = GetFiniteAOState(); //获取有限点输出的状态
            }
            byte halfRdy = 0;
            isStoped = false;
            if (_aoStarted == false)
            {
                isStoped = true;
                return false;
            }
            var err = USB6101Import.UD_AO_AsyncDblBufferHalfReady(_devHandle.CardID, out halfRdy);
            if (err != USB6101Import.NoError)
            {
                _threadException.AppendThreadException(new JYDriverException(err, "Inner Exception"));
                return false;
            }
            return (halfRdy != 0);
        }

        /// <summary>
        /// 取得当前有限点AO更新的状态,half ready != 0 返回true
        /// </summary>
        /// <returns>stoped ?</returns>
        private bool GetFiniteAOState()
        {
            short err = 0;
            byte bStopped;
            uint writeCnt;
            err = USB6101Import.UD_AO_AsyncCheck(_devHandle.CardID, out bStopped, out writeCnt);
            if (err != USB6101Import.NoError)
            {
                _threadException.AppendThreadException(new JYDriverException(err, "Inner Exception"));
            }
            if (_aoStarted == false)
            {
                return false;
            }
            return (bStopped != 0);
        }
        #endregion
    }

    #region ----------------aotask需要用到的结构和枚举的定义---------------
    /// <summary>
    /// AO通道参数类
    /// </summary>
    public sealed class AOChannel
    {
        /// <summary>
        /// 通道号。与AO通道的物理序号相对应。
        /// </summary>
        public int ChannelID { get; private set; }


        /// <summary>
        /// 通道量程下限
        /// </summary>
        public double RangeLow{ get; set; }

        /// <summary>
        /// 通道量程上限
        /// </summary>
        public double RangeHigh{ get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channelId">通道物理序号</param>
        /// <param name="rangeLow">通道量程下限</param>
        /// <param name="rangeHigh">通道量程上限</param>
        internal AOChannel(int channelId, double rangeLow, double rangeHigh)
        {
            ChannelID = channelId;
            RangeLow = rangeLow;
            RangeHigh = rangeHigh;
        }
    }


    /// <summary>
    /// 触发沿类型
    /// </summary>
    public enum AODigitalTriggerEdge
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
    /// 触发信号源
    /// </summary>
    public enum AODigitalTriggerSource
    {
        /// <summary>
        /// 外部数字触发
        /// </summary>
        IO_0
    };

    /// <summary>
    /// 状态锁
    /// </summary>
    internal class StatusLock
    {
        /// <summary>
        /// 状态标记, 默认为未使用
        /// </summary>
        public bool Marked { get; private set; } = false;

        /// <summary>
        /// 标记为使用中
        /// </summary>
        public void Mark()
        {
            lock (this)
            {
                Marked = true;
            }
        }

        /// <summary>
        /// 标记为未使用
        /// </summary>
        public void UnMark()
        {
            lock (this)
            {
                Marked = false;
            }
        }
    }

    /// <summary>
    /// AO工作模式枚举类型
    /// </summary>
    public enum AOMode
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
        /// 连续非环绕输出方式
        /// </summary>
        ContinuousNoWrapping,

        /// <summary>
        /// 连续环绕输出方式
        /// </summary>
        ContinuousWrapping
    };


    /// <summary>
    /// AI触发类型，需要根据板卡的实际支持情况修改
    /// </summary>
    public enum AOTriggerType
    {
        /// <summary>
        /// 无触发
        /// </summary>
        Immediate,

        /// <summary>
        /// 数字触发
        /// </summary>
        Digital
    };

    

    /// <summary>
    /// AO 触发参数设置类
    /// </summary>
    public sealed class AOTrigger
    {
        /// <summary>
        /// 触发类型，包括：Immediate/Digital
        /// </summary>
        public AOTriggerType Type { get; set; }

        /// <summary>
        /// 数字触发设置
        /// </summary>
        public AODigitalTrigger Digital { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        internal AOTrigger(AOTriggerType type= AOTriggerType.Immediate)
        {
            Digital = new AODigitalTrigger();
            Type = type;
        }  
    }

    /// <summary>
    /// AO数字触发设置
    /// </summary>
    public sealed class AODigitalTrigger
    {
        /// <summary>
        /// 触发源
        /// </summary>
        public AODigitalTriggerSource Source { get; }

        /// <summary>
        /// 数字触发边沿类型，Rising/Falling
        /// </summary>
        public AODigitalTriggerEdge Edge { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="edge">触发沿</param>
        internal AODigitalTrigger(AODigitalTriggerEdge edge = AODigitalTriggerEdge.Rising)
        {
            Source = AODigitalTriggerSource.IO_0;
            Edge = edge;
        }
    }
    #endregion
}
