using System;
using System.Threading;

namespace JYUSB101
{
    /// <summary>
    /// CNT操作任务类
    /// </summary>
    public sealed class JYUSB101CITask
    {
        /// <summary>
        /// 创建CI任务
        /// </summary>
        /// <param name="boardNum">板卡编号</param>
        /// <param name="chnId">通道编号,0或1有效</param>
        public JYUSB101CITask(int boardNum, int chnId)
        {
            _devHandle = JYUSB101Device.GetInstance((ushort)boardNum);
            if (_devHandle == null)
            {
                throw new JYDriverException(JYDriverExceptionPublic.InitializeFailed,
                    "初始化失败，请检查board number或硬件连接！");
            }
            if (_channelID != 0 && _channelID != 1)  //计数器通道不正确
            {
                throw new JYDriverException(JYDriverExceptionPublic.InitializeFailed, "计数器：" + _channelID.ToString() + ",通道不正确！");
            }
            _channelID = chnId;
            Counter = new Counter();
            Measure = new Measure();

            //To Add: 其他初始化代码
            Mode = CIMode.Counter;
        }

        #region ------------------公共属性定义-----------------------        
        /// <summary>
        /// 应用类型
        /// </summary>
        public CIMode Mode { get; set; }

        /// <summary>
        /// 计数器相关参数设置
        /// </summary>
        public Counter Counter { get; set; }

        /// <summary>
        /// 测量相关参数设置
        /// </summary>
        public Measure Measure { get; set; }

        #endregion

        #region ---------------私有属性定义----------------
        //此段定义需要使用的私有属性
        /// <summary>
        /// 操作硬件的对象
        /// </summary>
        private JYUSB101Device _devHandle;

        /// <summary>
        /// 通道ID
        /// </summary>
        private int _channelID;

        /// <summary>
        /// CI是否已经启动
        /// </summary>
        private bool _ciStarted;

        #endregion

        #region --------------公共方法定义--------------------
        /// <summary>
        /// 读取测量值(计数值、频率值、周期值等)
        /// </summary>
        /// <param name="buf">读取到的测量值</param>
        public void ReadCounter(ref uint buf)
        {
            //To Add: 读取CI值的代码
            if (_ciStarted == false)
            {
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            var adlinkErr = USB6101Import.UD_GPTC_Read(_devHandle.CardID, (ushort)_channelID, out buf);
            if (adlinkErr < 0)
            {
                JYLog.Print("GCTR Read Failed");
                throw new JYDriverException(adlinkErr, "GCTR Read Failed");
            }
        }

        /// <summary>
        /// 读取测量结果
        /// </summary>
        /// <param name="buf">读取到的测量值</param>
        public void ReadMeasure(ref double buf)
        {
            //To Add: 读取测量结果的代码
            if (Mode == CIMode.Counter)
            {
                JYLog.Print(JYLogLevel.ERROR, "当前应用类型不允许调用该方法！");
                throw new JYDriverException(JYDriverExceptionPublic.CannotCall);
            }
            if (Measure.ClockSource == CIClockSource.External && Measure.ExternalClockRate <= 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "CI使用了外部时钟源，但是设置的外部时钟频率不合理");
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            if (_ciStarted == false)
            {
                Start();
            }
            uint timeout = 5;
            uint value = 0;
            var startTime = DateTime.Now;
            ushort status;
            Boolean IstimeOut = false;
            while (true)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds > (timeout * 1000))
                {
                    IstimeOut = true;
                    break;
                }
                var adlinkErr = USB6101Import.UD_GPTC_Status(_devHandle.CardID, (ushort)_channelID, out status);

                if (((status >> 1) & 0x1) == 1)
                {
                    ReadCounter(ref value);
                    if (Measure.ClockSource == CIClockSource.Internal)
                    {
                        buf = value / _devHandle.BoardClkRate;
                    }
                    else
                    {
                        buf = value / Measure.ExternalClockRate;
                    }
                    break;
                }

                Thread.Sleep(1);
            }

            Stop();

            if (IstimeOut)
            {
                JYLog.Print(JYLogLevel.ERROR, "CI Measure TimeOut");
                throw new JYDriverException(JYDriverExceptionPublic.TimeOut);
            }

            
        }

        /// <summary>
        /// 启动CI任务
        /// </summary>
        public void Start()
        {
            //To Add: 配置CI，占用CI资源，并启动
            int ret;
            if (_ciStarted == true)
            {
                return;
            }
            if ((ret = ConfigCI()) < 0)
            {
                goto ErrorRET;
            }
            ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntENABLE, 0);
            if (ret < 0)
            {
                JYLog.Print("UD_GPTC_Control IntENABLE 0 Error,Code={0}", ret);
                goto ErrorRET;
            }
            ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntENABLE, 1);
            if (ret < 0)
            {
                JYLog.Print("UD_GPTC_Control IntENABLE 1 Error,Code={0}", ret);
                goto ErrorRET;
            }
            //if (Counter.GateSource == CIGateSource.Internal && Mode == CIMode.Counter)
            //{
            //    //ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntGate, 1);
            //    //if (ret < 0)
            //    //{
            //    //    JYLog.Print("UD_GPTC_Control IntGate 1 Error,Code={0}", ret);
            //    //    goto ErrorRET;
            //    //}
            //}
            _ciStarted = true;
            return;
            ErrorRET: //如果出错则启动失败，需要UnReserve硬件资源的占用，所以需要调用Stop
            {
                Stop();
                JYLog.Print(JYLogLevel.ERROR, "CI Start Failed");
                throw new JYDriverException(ret);
            }
        }

        /// <summary>
        /// 停止CI任务
        /// </summary>
        public void Stop()
        {
            //To Add: 停止CI操作，释放资源占用
            if (_ciStarted == false) //已经调用过Stop了，直接返回
            {
                return;
            }
            _ciStarted = false;
            //释放GPIO通道
            switch (_channelID)
            {
                case 0:
                    _devHandle.wPart1Cfg = false;
                    break;
                case 1:
                    _devHandle.wPart2Cfg = false;
                    break;
                default:
                    break;
            }
            int ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntENABLE, 0);
            if (ret < 0)
            {
                throw new JYDriverException(ret);
            }
            //if (Counter.GateSource == CIGateSource.Internal && Mode == CIMode.Counter)
            //{
            //    ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntGate, 1);
            //    if (ret < 0)
            //    {
            //        JYLog.Print(JYLogLevel.ERROR, "UD_GPTC_Control IntGate 1 Error,Code={0}", ret);
            //        throw new JYDriverException(ret);
            //    }
            //}
        }

        #endregion

        #region -------------私有方法定义-------------
        //此段定义私有方法
        /// <summary>
        /// 配置Counter In
        /// </summary>
        /// <returns></returns>
        private int ConfigCI()
        {
            //判断通道要使用的引脚是否已经被DIO所占用
            #region -----引脚资源占用------
            switch (_channelID)
            {
                case 0:
                    if (_devHandle.wPart1Cfg)
                    {
                        return JYErrorCode.HardwareResourceReserved; //GPIO的引脚已经被占用
                    }
                    break;
                case 1:
                    if (_devHandle.wPart2Cfg)
                    {
                        return JYErrorCode.HardwareResourceReserved; //GPIO的引脚已经被占用
                    }
                    break;
                default:
                    break;
            }
            #endregion

            ushort srcCtrl = 0;
            ushort polCtrl = 0;
            short adlinkErr = 0;

            //clock source
            if (Mode == CIMode.Measure)
            {
                srcCtrl |= USB6101Import.GPTC_CLK_SRC_Int;
                srcCtrl |= USB6101Import.GPTC_GATE_SRC_Ext;
                srcCtrl |= USB6101Import.GPTC_UPDOWN_Int;
                //clock polarity
                if (Measure.ClockEdge == CIClockEdge.Rising)
                {
                    polCtrl |= USB6101Import.GPTC_CLKSRC_HACTIVE;
                }
                else
                {
                    polCtrl |= USB6101Import.GPTC_CLKSRC_LACTIVE;
                }
            }
            else
            {
                if (Counter.ClockSource == CIClockSource.Internal)
                {
                    srcCtrl |= USB6101Import.GPTC_CLK_SRC_Int;
                }
                else
                {
                    srcCtrl |= USB6101Import.GPTC_CLK_SRC_Ext;
                }
                //gate source
                //if (Counter.GateSource == CIGateSource.Internal)
                //{
                //    srcCtrl |= USB6101Import.GPTC_GATE_SRC_Int;
                //}
                //else
                //{
                //    srcCtrl |= USB6101Import.GPTC_GATE_SRC_Ext;
                //}
                // gate source only suppurt Ext;
                srcCtrl |= USB6101Import.GPTC_GATE_SRC_Ext;
                //direction source
                //if (Counter.CountDirection == CountDirection.External)
                //{
                //    srcCtrl |= USB6101Import.GPTC_UPDOWN_Ext;
                //}
                //else
                //{
                //    srcCtrl |= USB6101Import.GPTC_UPDOWN_Int;
                //}
                srcCtrl |= USB6101Import.GPTC_UPDOWN_Int;
                //gate polarity
                //if (Counter.GatePolarity == CIPolarity.HighActive)
                //{
                //    polCtrl |= USB6101Import.GPTC_GATE_HACTIVE;
                //}
                //else
                //{
                //    polCtrl |= USB6101Import.GPTC_GATE_LACTIVE;
                //}

                polCtrl |= USB6101Import.GPTC_GATE_HACTIVE;
                //up down polarity
                //if (Counter.DirectionPolarity == CIPolarity.HighActive)
                //{
                //    polCtrl |= USB6101Import.GPTC_UPDOWN_HACTIVE;
                //}
                //else
                //{
                //    polCtrl |= USB6101Import.GPTC_UPDOWN_LACTIVE;
                //}
                //clock polarity
                if (Counter.ClockEdge == CIClockEdge.Rising)
                {
                    polCtrl |= USB6101Import.GPTC_CLKSRC_HACTIVE;
                }
                else
                {
                    polCtrl |= USB6101Import.GPTC_CLKSRC_LACTIVE;
                }
            }
            //配置到硬件
            //UD_GPTC_Setup_N和UD_GPTC_Setup,_N是增强型，CI和CO都需要修改，2016-07-21
            if (Mode == CIMode.Counter)
            {
                adlinkErr = USB6101Import.UD_GPTC_Setup_N(_devHandle.CardID, (ushort)_channelID, USB6101Import.SimpleGatedEventCNT,
                    srcCtrl, polCtrl, Counter.InitialCount, 0, 0);
            }
            else
            {
                adlinkErr = USB6101Import.UD_GPTC_Setup_N(_devHandle.CardID, (ushort)_channelID, (ushort)Measure.Type,
                    srcCtrl, polCtrl, Counter.InitialCount, 0, 0);
            }
            if (adlinkErr < 0)
            {
                return adlinkErr;
            }
            if (Counter.Direction == CountDirection.Down || Counter.Direction == CountDirection.Up || Mode == CIMode.Measure)
            {
                ushort upDown = 0;
                if (Counter.Direction == CountDirection.Up)
                {
                    upDown = 1;
                }
                adlinkErr = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntUpDnCTR, upDown);
                if (adlinkErr < 0)
                {
                    return adlinkErr;
                }
            }
            return JYErrorCode.NoError;
        }
        #endregion
    }

    #region ---------------CITask需要用到的结构和枚举的定义-----------------  
    /// <summary>
    /// 计数方向枚举
    /// </summary>
    public enum CountDirection
    {
        /// <summary>
        /// 增计数
        /// </summary>
        Up,

        /// <summary>
        /// 减计数
        /// </summary>
        Down,

        ///// <summary>
        ///// 外部增减计数控制引脚决定计数方向
        ///// </summary>
        //External
    };

    /// <summary>
    /// CI的时钟源枚举
    /// </summary>
    public enum CIClockSource
    {
        /// <summary>
        /// 内部时钟
        /// </summary>
        Internal,

        /// <summary>
        /// 外部时钟
        /// </summary>
        External
    }

    /// <summary>
    /// CI的Gate源枚举
    /// </summary>
    public enum CIGateSource
    {
        ///// <summary>
        ///// 软件指定
        ///// </summary>
        //Internal,

        /// <summary>
        /// 外部Gate引脚
        /// </summary>
        External
    }

    /// <summary>
    /// 极性设置枚举
    /// </summary>
    public enum CIPolarity
    {
        ///// <summary>
        ///// 低有效
        ///// </summary>
        //LowActive,

        /// <summary>
        /// 高有效
        /// </summary>
        HighActive
    }

    /// <summary>
    /// 信号沿类型
    /// </summary>
    public enum CIClockEdge
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
    /// 应用类型枚举
    /// </summary>
    public enum CIMode
    {
        /// <summary>
        /// 简单计数
        /// </summary>
        Counter,

        /// <summary>
        /// 测量SinglePeriodMSR/SinglePulseWidthMSR/EdgeSeparationMSR
        /// </summary>
        Measure
    };

    /// <summary>
    /// 测量类型
    /// </summary>
    public enum MeasureType
    {
        /// <summary>
        /// Gate上信号的单周期测量
        /// </summary>
        SinglePeriodMSR = USB6101Import.SinglePeriodMSR,

        /// <summary>
        /// Gate上信号的单脉冲宽度测量
        /// </summary>
        SinglePulseWidthMSR = USB6101Import.SinglePulseWidthMSR,

        /// <summary>
        /// gate 和 aux 信号的差分测量
        /// </summary>
        EdgeSeparationMSR = USB6101Import.EdgeSeparationMSR
    }

    /// <summary>
    /// 计数器参数设置类
    /// </summary>
    public sealed class Counter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        internal Counter()
        {
            InitialCount = 0;
            ClockSource = CIClockSource.Internal;
            ClockEdge = CIClockEdge.Rising;
            //GateSource = CIGateSource.External;
            //GatePolarity = CIPolarity.HighActive;
            Direction = CountDirection.Up;
            //DirectionPolarity = CIPolarity.HighActive;
        }

        /// <summary>
        /// /计数的时钟边沿
        /// </summary>
        public CIClockEdge ClockEdge { get; set; }

        /// <summary>
        /// 计数或测量的时钟源选择
        /// </summary>
        public CIClockSource ClockSource { get; set; }

        /// <summary>
        /// 计数器计数的方向增加外部引脚决定情况 
        /// </summary>
        public CountDirection Direction { get; set; }

        ///// <summary>
        ///// 计数器方向有效电平
        ///// </summary>
        //public CIPolarity DirectionPolarity { get; set; }

        ///// <summary>
        ///// 计数器Gate有效电平
        ///// </summary>
        //public CIPolarity GatePolarity { get; set; }

        ///// <summary>
        ///// 计数器Gate源选择
        ///// </summary>
        //public CIGateSource GateSource { get; set; }

        /// <summary>
        /// 计数器初始计数值
        /// </summary>
        public uint InitialCount { get; set; }
    }

    /// <summary>
    /// 测量参数配置类
    /// </summary>
    public sealed class Measure
    {
        /// <summary>
        /// 计数的时钟边沿
        /// </summary>
        public CIClockEdge ClockEdge { get; set; }

        /// <summary>
        /// 计数或测量的时钟源选择
        /// </summary>
        public CIClockSource ClockSource { get; set; }

        /// <summary>
        /// 外部时钟的频率,只针对外部时钟有效
        /// </summary>
        public double ExternalClockRate { get; set; }

        /// <summary>
        /// 测量类型 
        /// </summary>
        public MeasureType Type { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        internal Measure(MeasureType type = MeasureType.SinglePeriodMSR, CIClockEdge edge = CIClockEdge.Rising,
            CIClockSource source = CIClockSource.Internal)
        {
            Type = type;
            ClockEdge = edge;
            ClockSource = source;
            ExternalClockRate = 0;
        }
    }

    #endregion
}
