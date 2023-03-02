using System.Collections.Generic;
using System.Linq;

namespace JYUSB101
{
    /// <summary>
    /// DIO操作任务类
    /// </summary>
    public sealed class JYUSB101DOTask
    {
        /// <summary>
        /// 创建DO任务
        /// </summary>
        /// <param name="boardNum">板卡编号</param>
        public JYUSB101DOTask(int boardNum)
        {
            //获取板卡操作类的实例
            _deviceHandle = JYUSB101Device.GetInstance((ushort)boardNum);
            if (_deviceHandle == null)
            {
                throw new JYDriverException(JYDriverExceptionPublic.InitializeFailed, "初始化失败，请检查board number或硬件连接！");
            }
            Channels = new List<DOChannel>();
            //To Add: 其他初始化代码
        }

        #region -------------私有字段定义---------------
        /// <summary>
        /// 操作硬件的对象
        /// </summary>
        private JYUSB101Device _deviceHandle;

        #endregion

        #region -------------公共属性定义----------------

        /// <summary>
        /// 通道列表
        /// </summary>
        public List<DOChannel> Channels { get; }

        #endregion

        #region ---------------公共方法定义-------------------

        /// <summary>
        /// 添加通道
        /// </summary>
        /// <param name="lineNum"></param>
        public void AddChannel(int lineNum)
        {
            if (lineNum >= _deviceHandle.DIO_LineCount || lineNum < 0)
            {
                JYLog.Print(JYLogLevel.ERROR, "Error LineNumber,lineNum={0}", lineNum);
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            var idx = Channels.FindIndex(t => t.LineNum == lineNum);
            if (idx >= 0) //如果已经存在了该端口，则不再添加已经存在的Line
            {
                return;
            }
            else //如果不存在该端口，则直接添加
            {
                Channels.Add(new DOChannel(lineNum));
            }
            return;
        }

        /// <summary>
        /// 添加通道数组
        /// </summary>
        /// <param name="lineNum">端口号，从0开始。小于0则添加整个端口。</param>
        public void AddChannel(int[] lineNum)
        {
            //To Add 添加通道的代码  
            var ret = JYErrorCode.NoError;
            if (lineNum != null && lineNum.Length != 0 && (lineNum.Max() >= _deviceHandle.DIO_LineCount || lineNum.Min() < 0))
            {
                JYLog.Print(JYLogLevel.ERROR, "Error LineNumber, Min lineNum={0}, Max LineNum={1}", lineNum.Min(), lineNum.Max());
                throw new JYDriverException(JYDriverExceptionPublic.ErrorParam);
            }
            if (lineNum == null || lineNum.Length == 0) //数组为空则添加所有的line
            {
                for (var i = 0; i < _deviceHandle.DIO_LineCount; i++)
                {
                    Channels.Add(new DOChannel(i));
                }

                return;
            }
            else
            {
                for (int i = 0; i < lineNum.Length; i++)
                {
                    var idx = Channels.FindIndex(t => t.LineNum == lineNum[i]);
                    if (idx >= 0) //如果已经存在了该端口，则不再添加已经存在的Line
                    {
                        continue;
                    }
                    else //如果不存在该端口，则直接添加
                    {
                        Channels.Add(new DOChannel(lineNum[i]));
                    }
                }

            }
            return;
        }

        /// <summary>
        /// 删除指定端口号和位号的通道。
        /// </summary>
        /// <param name="lineNum">端口中的位号,如果是null或在Length为0，则删除全部bit</param>
        public void RemoveChannel(int lineNum)
        {
            //To Add 删除通道的代码  

            if (lineNum == -1)
            {
                for (int i = 0; i < Channels.Count; i++)
                {
                    Channels.RemoveAt(i);
                }
            }

            else
            {
                var idx = Channels.FindIndex(t => t.LineNum == lineNum);
                if (idx < 0) return;
                else
                {
                    Channels.RemoveAt(idx);
                }
            }

            return;
        }

        /// <summary>
        /// 删除指定位号的通道
        /// </summary>
        /// <param name="lineNum">位号,元素个数为0就删除所有的通道</param>
        public void RemoveChannel(int[] lineNum)
        {
            //To Add 删除通道的代码  
            if (lineNum == null || lineNum.Length == 0)
            {
                for (int i = 0; i < Channels.Count; i++)
                {
                    Channels.RemoveAt(i);
                }
                return;
            }
            else
            {
                int idx = 0;
                foreach (var item in lineNum)
                {
                    idx = Channels.FindIndex(t => t.LineNum == item);   
                    if(idx >=0)
                    {
                        Channels.RemoveAt(idx);
                    }         
                }
            }
        }


        /// <summary>
        /// 每通道更新一个点, 非缓冲式更新
        /// </summary>
        /// <param name="buf">用户缓冲区数组</param>
        public void WriteSinglePoint(bool[] buf)
        {
            int ret;
            var idx = 0;
            if (buf.Length < Channels.Count)
            {
                throw new JYDriverException(JYDriverExceptionPublic.UserBufferError);
            }

            if (ReserveCfgDO() < 0)
            {
                JYLog.Print("硬件已经被占用，DO失败");
                throw new JYDriverException(JYDriverExceptionPublic.HardwareResourceReserved);
            }       
            foreach (var item in Channels)
            {
                var line = (ushort)item.LineNum;
                var state = (ushort)(buf[idx++] ? 1 : 0);
                USB6101Import.UD_DO_WriteLine(_deviceHandle.CardID, 0, line, state);
            }
            if ((ret = UnReserveDO()) < 0)
            {
                throw new JYDriverException(ret);
            }
        }
        #endregion

        #region -------------私有方法定义-------------
        //此段定义私有方法
        /// <summary>
        /// 标志DO占用
        /// </summary>
        /// <returns></returns>
        private int ReserveCfgDO()
        {
            ushort wConfig = 0;
            if (Channels.Count <= 0)
            {
                JYLog.Print("当前没有添加DO通道");
                return JYErrorCode.IncorrectCallOrder;
            }
            var chn = Channels[0];
            foreach (var item in Channels)
            {
                if (_deviceHandle.DIOLineReserved[item.LineNum] == true)
                {
                    JYLog.Print("DIO资源已被占用，DILineNumber={0}", item);
                    return JYErrorCode.HardwareResourceReserved;
                }
            }
            foreach (var item in Channels)
            {
                _deviceHandle.DIOLineReserved[item.LineNum] = true;

                switch (item.LineNum)
                {
                    case 0:
                        wConfig |= USB6101Import.GPO0;
                        break;
                    case 1:
                        wConfig |= USB6101Import.GPO1;
                        break;
                    case 2:
                        wConfig |= USB6101Import.GPO2;
                        break;
                    case 3:
                        wConfig |= USB6101Import.GPO3;
                        break;
                    default:
                        wConfig = 0;
                        break;
                }
            }

            var err = USB6101Import.UD_DIO_Config(_deviceHandle.CardID, wConfig, 0);
            if (err < 0)
            {
                UnReserveDO();
            }
            return JYErrorCode.NoError;
        }

        /// <summary>
        /// 解除DO占用标志
        /// </summary>
        /// <returns></returns>
        private int UnReserveDO()
        {
            if (Channels.Count <= 0)
            {
                JYLog.Print("当前没有添加DO通道");
                return JYErrorCode.IncorrectCallOrder;
            }
            foreach (var item in Channels)
            {
                _deviceHandle.DIOLineReserved[item.LineNum] = false;
            }
            return JYErrorCode.NoError;
        }
        #endregion
    }

    #region ---------------DIOTask需要用到的结构和枚举的定义------------------
    /// <summary>
    /// DIO通道参数类
    /// </summary>
    public sealed class DOChannel
    {
        /// <summary>
        /// 端口号，从0开始。提供小于0的值，将获得值0.
        /// </summary>
        public int LineNum { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lineNum">端口中的位号</param>
        internal DOChannel(int lineNum)
        {
            LineNum = lineNum;
        }
    }

    #endregion
}