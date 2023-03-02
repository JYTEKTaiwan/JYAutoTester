using System;

namespace JYUSB101
{
    /// <summary>
    /// CO操作任务类
    /// </summary>
    public sealed class JYUSB101COTask
    {
        /// <summary>
        /// 创建CO任务
        /// </summary>
        /// <param name="boardNum">设备编号</param>
        /// <param name="chnId">需要用于脉冲生成的通道编号</param>
        public JYUSB101COTask(int boardNum, int chnId)
        {
            _devHandle = JYUSB101Device.GetInstance((ushort)boardNum);
            if (_devHandle == null)
            {
                throw new JYDriverException(JYDriverExceptionPublic.InitializeFailed, "初始化失败，请检查board number或硬件连接！");
            }
            _channelID = chnId;
            Pulse = new COPulse();
            Clock = new COClock();
            Gate = new COGate();
            //To Add: 其他初始化代码
            _applicationType = COMode.ContGatedPulseGenPWM;
        }

        #region -----------------公共属性定义------------------------   
        private COMode _applicationType;
        /// <summary>
        /// 应用类型
        /// </summary>
        public COMode Mode
        {
            get { return _applicationType; }
            set
            {
                _applicationType = value;
                switch (value)
                {
                    //单脉冲生成时，时钟默认内部时钟，Gate只能是外部Gate，默认Pulse参数为Tick
                    case COMode.SingleGatedPulseGen:
                    //case COMode.SingleTrigPulseGen:
                    case COMode.RetrigSinglePulseGen:
                    case COMode.SingleTrigContPulseGen:
                    case COMode.ContGatedPulseGen:
                    //case COMode.MultipleGatedPulseGen:
                        Clock.Edge = COSignalEdge.Rising;
                        Gate.Source = COGateSource.External;
                        Gate.Polarity = COPolarity.HighActive;
                        Pulse.Type = COPulseType.HighLowTick;
                        break;
                    case COMode.SingleTrigContPulseGenPWM:
                    case COMode.ContGatedPulseGenPWM:
                        Clock.Edge = COSignalEdge.Rising;
                        Gate.Source = COGateSource.External;
                        Gate.Polarity = COPolarity.HighActive;
                        Pulse.Type = COPulseType.DutyCycleFrequency;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 计数的时钟边沿
        /// </summary>
        public COClock Clock { get; set; }

        /// <summary>
        /// Gate设置参数
        /// </summary>
        public COGate Gate { get; set; }

        /// <summary>
        /// 脉冲参数设置
        /// </summary>
        public COPulse Pulse { get; set; }

        /// <summary>
        /// 闲置状态(输出开始前和输出完成后输出端的电平状态)
        /// </summary>
        public COSignalLevel IdleState { get; set; }

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
        /// CO是否已经启动
        /// </summary>
        private bool _coStarted;
        #endregion

        #region ----------------公共方法定义-------------------
        /// <summary>
        /// 应用参数配置（PWM生成时可以同时修改频率和占空比）
        /// </summary>
        public void ApplyParam()
        {
            //To Add: 应用参数配置
            if (_coStarted == false)
            {
                JYLog.Print("Task has not been Started...");
                throw new JYDriverException(JYDriverExceptionPublic.IncorrectCallOrder);
            }
            Stop();
            Start();
        }

        /// <summary>
        /// 启动CO任务
        /// </summary>
        public void Start()
        {
            //To Add: 配置CO，占用CO资源，并启动
            int ret;
            if (_coStarted)
            {
                JYLog.Print(JYLogLevel.ERROR, "CO被占用");
                throw new JYDriverException(JYDriverExceptionPublic.HardwareResourceReserved);
            }
            if ((ret = ConfigCO(false)) < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "CO配置失败，error code={0}", ret);
                Stop();
                throw new JYDriverException(ret);
            }

            ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntENABLE, 0);
            if (ret < 0)
            {
                JYLog.Print("UD_GPTC_Control IntENABLE 0 Error,Code={0}", ret);
                Stop();
                throw new JYDriverException(ret);
            }
            ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntENABLE, 1);
            if (ret < 0)
            {
                JYLog.Print("UD_GPTC_Control IntENABLE 1 Error,Code={0}", ret);
                Stop();
                throw new JYDriverException(ret);
            }
            //if (Gate.Source == COGateSource.Internal)
            //{
            //    //ret = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntGate, 1);
            //    //if (ret < 0)
            //    //{
            //    //    JYLog.Print("UD_GPTC_Control IntGate 1 Error,Code={0}", ret);
            //    //    Stop();
            //    //    throw new JYDriverException(ret);
            //    //}
            //}
            _coStarted = true;
        }

        /// <summary>
        /// 停止Count Out任务
        /// </summary>
        public void Stop()
        {
            //To Add: 停止CO操作，释放资源占用
            if (_coStarted == false)
            {
                JYLog.Print(JYLogLevel.DEBUG, "CO并未启动，Stop被忽略");
                return;
            }
            _coStarted = false;
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
        }
        #endregion

        #region -------------私有方法定义-------------
        //此段定义私有方法
        /// <summary>
        /// 配置Counter Out
        /// </summary>
        /// <returns></returns>
        private int ConfigCO(bool changeParam)
        {
            //外部时钟时，只允许按照Tick配置脉冲参数
            if (Clock.Source == COClockSource.External)
            {
                if (Pulse.Type != COPulseType.HighLowTick)
                {
                    return JYErrorCode.ErrorParam;
                }
            }

            //判断通道要使用的引脚是否已经被DIO所占用
            if (changeParam == false)
            {
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
            }

            ushort srcCtrl = 0;
            ushort polCtrl = 0;
            short err = 0;

            //clock source
            if (Clock.Source == COClockSource.Internal)
            {
                srcCtrl |= USB6101Import.GPTC_CLK_SRC_Int;
            }
            else
            {
                srcCtrl |= USB6101Import.GPTC_CLK_SRC_Ext;
            }

            //gate source
            //if (Gate.Source == COGateSource.Internal)
            //{
            //    srcCtrl |= USB6101Import.GPTC_GATE_SRC_Int;
            //}
            //else
            //{
            //    srcCtrl |= USB6101Import.GPTC_GATE_SRC_Ext;
            //}

            srcCtrl |= USB6101Import.GPTC_GATE_SRC_Ext;

            //gate polarity
            if (Gate.Polarity == COPolarity.HighActive)
            {
                polCtrl |= USB6101Import.GPTC_GATE_HACTIVE;
            }
            else
            {
                polCtrl |= USB6101Import.GPTC_GATE_LACTIVE;
            }

            //clock polarity
            if (Clock.Edge == COSignalEdge.Rising)
            {
                polCtrl |= USB6101Import.GPTC_CLKSRC_HACTIVE;
            }
            else
            {
                polCtrl |= USB6101Import.GPTC_CLKSRC_LACTIVE;
            }

            //配置到硬件

            uint pulseLowTick = 0;
            uint pulseHighTick = 0;
            uint reg1 = 0, reg2 = 0;
            var initialDelay = (uint)(Pulse.InitialDelay * _devHandle.BoardClkRate);
            #region -----------Pulse Param Reg-------------
            switch (Pulse.Type)
            {
                case COPulseType.HighLowTime:
                    if (Pulse.Time.Low <= 0 || Pulse.Time.High <= 0)
                    {
                        JYLog.Print("Param Error,PulseLowTime={0}, PulseHighTime={0}.", Pulse.Time.Low, Pulse.Time.High);
                        return JYErrorCode.ErrorParam;
                    }
                    pulseLowTick = (uint)(Pulse.Time.Low * _devHandle.BoardClkRate);
                    pulseHighTick = (uint)(Pulse.Time.High * _devHandle.BoardClkRate);
                    break;
                case COPulseType.HighLowTick:
                    if (Pulse.Tick.Low <= 0 || Pulse.Tick.High <= 0)
                    {
                        JYLog.Print("Param Error,Low={0}, High={0}.", Pulse.Time.Low, Pulse.Time.High);
                        return JYErrorCode.ErrorParam;
                    }
                    pulseLowTick = (uint)Pulse.Tick.Low;
                    pulseHighTick = (uint)Pulse.Tick.High;
                    initialDelay = (uint)Pulse.InitialDelay;
                    break;
                case COPulseType.DutyCycleFrequency:
                    if (Pulse.DutyCycleFrequency.DutyCycle <= 0 || Pulse.DutyCycleFrequency.DutyCycle >= 1 || Pulse.DutyCycleFrequency.Frequency <= 0)
                    {
                        JYLog.Print("Param Error,DutyCycle={0}, Frequency={0}.", Pulse.DutyCycleFrequency.DutyCycle, Pulse.DutyCycleFrequency.Frequency);
                        return JYErrorCode.ErrorParam;
                    }
                    pulseLowTick = (uint)(_devHandle.BoardClkRate * (1 - Pulse.DutyCycleFrequency.DutyCycle) / Pulse.DutyCycleFrequency.Frequency);
                    pulseHighTick = (uint)(_devHandle.BoardClkRate * Pulse.DutyCycleFrequency.DutyCycle / Pulse.DutyCycleFrequency.Frequency);
                    break;
                default:
                    break;
            }

            switch (_applicationType)
            {
                case COMode.SingleTrigContPulseGenPWM:
                case COMode.ContGatedPulseGenPWM:
                case COMode.SingleTrigContPulseGen:
                case COMode.ContGatedPulseGen:
                    {
                        reg1 = pulseLowTick;
                        reg2 = pulseHighTick;
                        break;
                    }
                case COMode.SingleGatedPulseGen:
                //case COMode.SingleTrigPulseGen:
                case COMode.RetrigSinglePulseGen:
                //case COMode.MultipleGatedPulseGen:
                    reg1 = initialDelay;
                    reg2 = pulseHighTick;
                    break;
                default:
                    break;
            }
            #endregion

            uint pulseCount = 0;
            //if (_applicationType == COMode.MultipleGatedPulseGen)
            //{
            //    pulseCount = Pulse.Count;
            //}

            err = USB6101Import.UD_GPTC_Setup_N(_devHandle.CardID, (ushort)_channelID, (ushort)_applicationType,
                                             srcCtrl, polCtrl, reg1, reg2, pulseCount);
            if (err < 0)
            {
                return err;
            }
            //脉冲生成都采用Down计数
            err = USB6101Import.UD_GPTC_Control(_devHandle.CardID, (ushort)_channelID, USB6101Import.IntUpDnCTR, 0);
            return err < 0 ? err : 0;
        }
        #endregion
    }

    #region  --------------COTask需要用到的结构和枚举的定义--------------
    /// <summary>
    /// 信号电平枚举
    /// </summary>
    public enum COSignalLevel
    {
        /// <summary>
        /// 低电平
        /// </summary>
        Low,

        /// <summary>
        /// 高电平
        /// </summary>
        High
    };

    /// <summary>
    /// 极性设置枚举
    /// </summary>
    public enum COPolarity
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
    /// 应用类型枚举
    /// </summary>
    public enum COMode
    {
        /// <summary>
        /// Gate使能单脉冲生成
        /// </summary>
        SingleGatedPulseGen = USB6101Import.SingleGatedPulseGen,

        ///// <summary>
        ///// 触发单脉冲生成
        ///// </summary>
        //SingleTrigPulseGen = USB6101Import.SingleTrigPulseGen,

        /// <summary>
        /// 可重触发单脉冲生成
        /// </summary>
        RetrigSinglePulseGen = USB6101Import.RetrigSinglePulseGen,

        /// <summary>
        /// 触发连续脉冲生成
        /// </summary>
        SingleTrigContPulseGen = USB6101Import.SingleTrigContPulseGen,

        /// <summary>
        /// Gate使能连续冲生成
        /// </summary>
        ContGatedPulseGen = USB6101Import.ContGatedPulseGen,

        /// <summary>
        /// 触发连续脉冲PWM生成
        /// </summary>
        SingleTrigContPulseGenPWM = USB6101Import.SingleTrigContPulseGenPWM,

        /// <summary>
        /// 门使能连续脉冲PWM生成
        /// </summary>
        ContGatedPulseGenPWM = USB6101Import.ContGatedPulseGenPWM,

        ///// <summary>
        ///// 多门使能脉冲生成
        ///// </summary>
        //MultipleGatedPulseGen = USB6101Import.MultipleGatedPulseGen
    };

    /// <summary>
    /// CI的时钟源枚举
    /// </summary>
    public enum COClockSource
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
    public enum COGateSource
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
    /// 信号沿类型
    /// </summary>
    public enum COSignalEdge
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
    /// CO时钟设置
    /// </summary>
    public sealed class COClock
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        internal COClock()
        {
            Edge = COSignalEdge.Rising;
            Source = COClockSource.Internal;
        }

        /// <summary>
        /// 计数器的时钟边沿选择
        /// </summary>
        public COSignalEdge Edge { get; set; }

        /// <summary>
        /// 计数或测量的时钟源选择
        /// </summary>
        public COClockSource Source { get; set; }
    }

    /// <summary>
    /// CO时钟设置
    /// </summary>
    public sealed class COGate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        internal COGate()
        {
            Polarity = COPolarity.HighActive;
            //Source = COGateSource.Internal;
        }

        /// <summary>
        /// 计数器Gate有效电平
        /// </summary>
        public COPolarity Polarity { get; set; }

        /// <summary>
        /// 计数器Gate源选择
        /// </summary>
        public COGateSource Source { get; set; }
    }

    /// <summary>
    /// 按占空比和频率设置
    /// </summary>
    public sealed class DutyCycleFrequency
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        internal DutyCycleFrequency()
        {
            DutyCycle = 0.5;
            Frequency = 1000;
        }

        /// <summary>
        /// 输出脉冲的占空比
        /// </summary>
        public double DutyCycle { get; set; }

        /// <summary>
        /// 输出脉冲的频率
        /// </summary>
        public double Frequency { get; set; }
    }

    /// <summary>
    /// 按高低电平时间设置
    /// </summary>
    public sealed class Time
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        internal Time()
        {
            High = 0.0005;
            Low = 0.0005;
        }

        /// <summary>
        /// 脉冲高电平时间
        /// </summary>
        public double High { get; set; }

        /// <summary>
        /// 脉冲低电平时间
        /// </summary>
        public double Low { get; set; }
    }

    /// <summary>
    /// 按高低电平Tick设置
    /// </summary>
    public sealed class Tick
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        internal Tick()
        {
            High = 40000;
            Low = 40000;
        }

        /// <summary>
        /// 脉冲高电平Tick
        /// </summary>
        public double High { get; set; }

        /// <summary>
        /// 脉冲低电平Tick
        /// </summary>
        public double Low { get; set; }
    }

    /// <summary>
    /// CO脉冲的参数设置
    /// </summary>
    public sealed class COPulse
    {
        /// <summary>
        /// 默认构造函数，做一些初始化操作
        /// </summary>
        internal COPulse(COPulseType type = COPulseType.HighLowTick, double initialDelay = 0, uint count = 0)
        {
            InitialDelay = initialDelay;
            Type = type;
            DutyCycleFrequency = new DutyCycleFrequency();
            Time = new Time();
            Tick = new Tick();
            Count = count;
        }

        /// <summary>
        /// 输出脉冲的初始延迟
        /// </summary>
        public double InitialDelay { get; set; }

        /// <summary>
        /// 是否使用频率和占空比指定脉冲参数
        /// </summary>
        public COPulseType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DutyCycleFrequency DutyCycleFrequency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Time Time { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Tick Tick { get; set; }

        /// <summary>
        /// 输出脉冲的个数（仅针对MultipleGatedPulseGen模式）
        /// </summary>
        public uint Count { get; set; }
    }

    /// <summary>
    /// CO输出脉冲参数的设置类型
    /// </summary>
    public enum COPulseType
    {
        /// <summary>
        /// 用占空比和频率设置
        /// </summary>
        DutyCycleFrequency,

        /// <summary>
        /// 用高电平时间和低电平时间设置
        /// </summary>
        HighLowTime,

        /// <summary>
        /// 用高电平和低电平的Tick数设置
        /// </summary>
        HighLowTick
    }
    #endregion

}
