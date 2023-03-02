using System;
using System.Collections.Generic;

namespace JYUSB101
{
    /// <summary>
    /// 定义JYUSB101Device专用的函数和与器件相关的常量
    /// </summary>
    internal class JYUSB101Device
    {

        #region --------------只读字段---------------

        /// <summary>
        /// 卡的类型，在实例化类对象时根据Device model初始化为对应的类型
        /// </summary>
        public ushort CardType { get; } = USB6101Import.USB_101;

        /// <summary>
        /// 计数器时基
        /// </summary>
        public double BoardClkRate { get; } = 1000000;

        /// <summary>
        /// 差分或伪差分通道数
        /// </summary>
        public uint DiffChannelCount { get; } = 2;

        /// <summary>
        /// 单端或伪单端通道数
        /// </summary>
        public uint DiffChannelCountSEChannelCount { get; } = 2;

        /// <summary>
        /// 单通道最大采样率
        /// </summary>
        public double MaxSampleRateSingleChannel { get; } = 100000;

        /// <summary>
        /// 是否是同步采集卡
        /// </summary>
        public bool IsAISync { get; } = false;


        /// <summary>
        /// AO通道数
        /// </summary>   
        public uint AOChannelCount { get; } = 2;

        /// <summary>
        /// 单通道最大更新率
        /// </summary>
        public double MaxUpdateRateSingleChannel { get; } = 100000;

        /// <summary>
        /// AO输出是否是同步
        /// </summary>
        public bool IsAOSync { get; } = false;

        #endregion

        #region ------保存实例化后的一些参数定义-------
        /// <summary>
        /// 板卡编号，构造此类对象时的入参
        /// </summary>
        private ushort _cardnumber;

        /// <summary>
        /// 用于保存每个cardnumber构造出的实例
        /// </summary>
        private static List<JYUSB101Device> _listThisInst = null;

        /// <summary>
        /// 调用Register后得到的cardID
        /// </summary>
        private short _cardID;
        public ushort CardID => (ushort)_cardID;

        #endregion

        #region --------------AI相关字段-----------------

        /// <summary>
        /// AI double buffer缓冲区的blocksize
        /// </summary>
        public uint AIDBFBlockSize { get; } = 512;

        /// <summary>
        /// AI non double buffer缓冲区的blocksize
        /// </summary>
        public uint AINDBFBlockSize { get; } = 256;


        /// <summary>
        /// AI是否已经占用的标志
        /// </summary>
        public bool AIReserved { get; set; }

        #endregion

        #region ---------------AO相关字段---------------

        /// <summary>
        /// AO double buffer 缓冲区的块大小
        /// </summary>
        public uint AODBFBlockSize { get; } = 512;

        /// <summary>
        /// AO none double buffer 缓冲区的块大小
        /// </summary>
        public uint AONDBFBlockSize { get; } = 256;

        /// <summary>
        /// AO是否占用的标志
        /// </summary>
        public bool AOReserved { get; set; }

        /// <summary>
        /// AOTimeBase
        /// </summary>
        public uint AOTimeBase { get; } = 72000000;

        #endregion

        #region  ----------------DIO和CIO字段---------------------

        /// <summary>
        /// DIO通道数
        /// </summary>
        public int DIO_LineCount = 4;
        /// <summary>
        /// DI端口及其线是否被占用
        /// </summary>
        public bool[] DIOLineReserved = new bool[] { false, false, false, false };


        /// <summary>
        /// 定时器占用情况，
        /// </summary>
        public bool[] TimerReserved = new bool[] { false, false, false, false };

        /// <summary>
        /// GPIO资源配置1
        /// </summary>
        public bool wPart1Cfg = false;

        /// <summary>
        /// GPIO资源配置2
        /// </summary>
        public bool wPart2Cfg = false;


        #endregion

        #region -------------构造实例,初始化及释放----------------
        /// <summary>
        /// 根据board number获取操作实例,保证每张板卡只有一个注册实例
        /// </summary>
        /// <param name="cardNum"></param>
        /// <returns></returns>
        internal static JYUSB101Device GetInstance(ushort cardNum)
        {
            if (_listThisInst == null || !_listThisInst.Exists(t => t._cardnumber == cardNum))
            {
                JYUSB101Device inst = null;
                try
                {
                    inst = new JYUSB101Device(cardNum);
                }
                catch (Exception ex)
                {
                    JYLog.Print("硬件初始化失败，cardNum = {0}, Msg={1}", cardNum, ex.Message);
                    return null;
                }
                if (_listThisInst == null)
                {
                    _listThisInst = new List<JYUSB101Device>();
                }
                _listThisInst.Add(inst);
                return inst;
            }
            else
            {
                JYLog.Print("硬件已经初始化，直接返回实例!");
                return _listThisInst.Find(t => t._cardnumber == cardNum);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        private JYUSB101Device(ushort cardNum)
        {
            _cardID = -1;
            //以下添加参数及硬件初始化代码
            var cid = USB6101Import.UD_Register_Card(CardType, cardNum);
            if (cid < 0)
            {
                _cardID = -1;
                throw new Exception("初始化失败，请检查板卡编号或线缆连接！");
            }
            else
            {
                _cardnumber = cardNum;
                _cardID = cid;
            }
        }

        /// <summary>
        /// 关闭AD设备,禁止传输,并释放资源，该函数自动在类的析构函数中执行
        /// </summary>
        private int Release()
        {
            //以下添加释放硬件资源的代码\
            if (_cardID < 0) return 0;
            var err = USB6101Import.UD_Release_Card((ushort)_cardID);
            JYLog.Print("Release Card, ret = {0}", err);
            _cardID = -1;
            return 0;
        }

        /// <summary>
        /// 析构函数，释放硬件资源
        /// </summary>
        ~JYUSB101Device()
        {
            Release();
        }
        #endregion
    }
}
