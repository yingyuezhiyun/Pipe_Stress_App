using Pipe_Stress_App_V2._5.Common.DataHandlers.Commands;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers
{
    public class SerialPortHandler : ISerialPortHandler
    {
        private readonly IEventAggregator aggregator;
        private readonly IContainerExtension container;


        public SerialPort DeviceSerialPort { get; set; } = new SerialPort();
        public SerialPort RelaySerialPort { get; set; } = new SerialPort();

        public SerialPortHandler(IEventAggregator aggregator, IContainerExtension container)
        {

            DeviceSerialPort.BaudRate = 921600;
            DeviceSerialPort.DataBits = 8;
            //DeviceSerialPort.ReceivedBytesThreshold = (53 * 1000 );
            DeviceSerialPort.ReadBufferSize = (106 * 1000);
            DeviceSerialPort.DataReceived += DevDataReceived;

            RelaySerialPort.BaudRate = 9600;
            RelaySerialPort.DataBits = 8;
            RelaySerialPort.DataReceived += Relay_DataReceived;

            this.aggregator = aggregator;
            this.container = container;

            DevRevTimerInit();

        }




        public void RelayCMDSend(List<byte> relayCMD)
        {
            List<byte> cmd = new List<byte>();
            cmd.AddRange(relayCMD);
            if (RelaySerialPort.IsOpen)
            {
                RelaySerialPort.Write(cmd.ToArray(), 0, cmd.Count);
            }
        }
        /// <summary>
        /// 0-3  to 5#-8#
        /// </summary>
        /// <param name="ch"></param>
        public async void MultiplexChange(int ch)
        {
            var relayConfi = container.Resolve<IRelayInfo>();
            var multiplexConfi = container.Resolve<IMultiplexInfo>();

            for (int i = 0; i < 4; i++)
            {
                multiplexConfi.Multiplexes[i].State = false;
            }
            switch (ch)//判断当前复用状态 5# 6# 7# 8# 
            {
                case 1://  切换至 6#
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 00, state: RelayCMD.State.Close));
                    relayConfi.Relay1[0].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 01, state: RelayCMD.State.Open));
                    relayConfi.Relay1[1].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 03, state: RelayCMD.State.Close));
                    relayConfi.Relay1[3].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 04, state: RelayCMD.State.Open));
                    relayConfi.Relay1[4].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 06, state: RelayCMD.State.Open));
                    relayConfi.Relay1[6].OpenState = true;
                    await Task.Delay(50);

                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 00, state: RelayCMD.State.Close));
                    relayConfi.Relay2[0].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 01, state: RelayCMD.State.Close));
                    relayConfi.Relay2[1].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 02, state: RelayCMD.State.Open));
                    relayConfi.Relay2[2].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 04, state: RelayCMD.State.Close));
                    relayConfi.Relay2[4].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 05, state: RelayCMD.State.Open));
                    relayConfi.Relay2[5].OpenState = true;
                    await Task.Delay(50);

                    multiplexConfi.Multiplexes[ch].State = true;

                    break;
                case 2:// 切换至 7#
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 00, state: RelayCMD.State.Open));
                    relayConfi.Relay1[0].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 02, state: RelayCMD.State.Close));
                    relayConfi.Relay1[2].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 03, state: RelayCMD.State.Open));
                    relayConfi.Relay1[3].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 05, state: RelayCMD.State.Close));
                    relayConfi.Relay1[5].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 07, state: RelayCMD.State.Close));
                    relayConfi.Relay1[7].OpenState = false;
                    await Task.Delay(50);

                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 00, state: RelayCMD.State.Open));
                    relayConfi.Relay2[0].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 01, state: RelayCMD.State.Open));
                    relayConfi.Relay2[1].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 03, state: RelayCMD.State.Close));
                    relayConfi.Relay2[3].OpenState = false;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 04, state: RelayCMD.State.Open));
                    relayConfi.Relay2[4].OpenState = true;
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 06, state: RelayCMD.State.Close));
                    relayConfi.Relay2[6].OpenState = false;
                    await Task.Delay(50);

                    multiplexConfi.Multiplexes[ch].State = true;


                    break;
                case 3://  切换至 8#


                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 0xff, state: RelayCMD.State.Open));
                    for (int i = 0; i < 8; i++)
                    {
                        relayConfi.Relay1[i].OpenState = true;
                    }
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 0xff, state: RelayCMD.State.Open));
                    for (int i = 0; i < 8; i++)
                    {
                        relayConfi.Relay2[i].OpenState = true;
                    }
                    multiplexConfi.Multiplexes[ch].State = true;


                    break;
                case 0:// 切换至 5#

                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 0xff, state: RelayCMD.State.Close));
                    for (int i = 0; i < 8; i++)
                    {
                        relayConfi.Relay1[i].OpenState = false;
                    }
                    await Task.Delay(50);
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 0xff, state: RelayCMD.State.Close));
                    for (int i = 0; i < 8; i++)
                    {
                        relayConfi.Relay2[i].OpenState = false;
                    }
                    multiplexConfi.Multiplexes[ch].State = true;
                    break;
                default:
                    break;
            }
        }

        public void RelayToggle(int id, int index)
        {

            if (id == 0)
            {
                if (container.Resolve<IRelayInfo>().Relay1[index].OpenState)
                {
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: index, state: RelayCMD.State.Close));
                }
                else
                {
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: index, state: RelayCMD.State.Open));
                }
            }
            else
            {
                if (container.Resolve<IRelayInfo>().Relay1[index].OpenState)
                {
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: index, state: RelayCMD.State.Close));
                }
                else
                {
                    RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: index, state: RelayCMD.State.Open));
                }
            }

        }

        public async void RelayAllOpenOrClose(bool open)
        {
            var relayConfi = container.Resolve<IRelayInfo>();
            if (open)
            {
                RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 0xff, state: RelayCMD.State.Open));
                for (int i = 0; i < 8; i++)
                {
                    relayConfi.Relay1[i].OpenState = true;
                }
                await Task.Delay(50);
                RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 0xff, state: RelayCMD.State.Open));
                for (int i = 0; i < 8; i++)
                {
                    relayConfi.Relay2[i].OpenState = true;
                }
            }
            else
            {
                RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID01, ch: 0xff, state: RelayCMD.State.Close));
                for (int i = 0; i < 8; i++)
                {
                    relayConfi.Relay1[i].OpenState = false;
                }
                await Task.Delay(50);
                RelayCMDSend(RelayCMD.OC(ID: RelayCMD.ID02, ch: 0xff, state: RelayCMD.State.Close));
                for (int i = 0; i < 8; i++)
                {
                    relayConfi.Relay2[i].OpenState = false;
                }
            }




        }

        /// <summary>
        /// 继电器 串口接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Relay_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }





        /// <summary>
        /// 应变仪 控制命令串口发送
        /// </summary>
        /// <param name="command"></param>
        public void DevCMDSend(SendCommand command)
        {
            var Send = new List<byte>();
            switch (command.Name)
            {
                case "Detect": Send.AddRange(DeviceCMD.Detect()); break;
                case "DetectCycled": Send.AddRange(DeviceCMD.DetectCycled()); break;
                case "DetectStop": Send.AddRange(DeviceCMD.DetectStop()); break;
                case "ReturnToZero": Send.AddRange(DeviceCMD.ReturnToZero()); break;
                case "SetFrequency": Send.AddRange(DeviceCMD.SetFrequency(command.Value)); break;
                case "GetFactoryVersion": Send.AddRange(DeviceCMD.GetFactoryVersion()); break;
                default:
                    break;
            }
            if (Send.Count != 0 && DeviceSerialPort.IsOpen)
            {
                DeviceSerialPort.Write(Send.ToArray(), 0, Send.Count);
            }
        }



        /// <summary>
        /// 应变仪 不完整数据缓存  Receive Incomplete Data Buffer
        /// </summary>
        List<byte> DevRIDB = new List<byte>();
        /// <summary>
        /// 应变仪串口数据处理量
        /// </summary>
        int DevRevCapacity = 1000;
        /// <summary>
        /// 应变仪接收的原始数据包
        /// </summary>
        List<RevDataArray> DevRevDataArray = new List<RevDataArray>();
        /// <summary>
        /// 应变仪 串口原始数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DevDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = DeviceSerialPort.BytesToRead;
            byte[] tempBuffer = new byte[bytesToRead];
            DeviceSerialPort.Read(tempBuffer, 0, bytesToRead);
            List<byte> receiveBufferTemp = new List<byte>();

            receiveBufferTemp.AddRange(DevRIDB);
            receiveBufferTemp.AddRange(tempBuffer);
            if (receiveBufferTemp.Count < 3)//接收数据量过少
            {
                DevRIDB = receiveBufferTemp;//放入全局缓存，下次拼接
                return;
            }
            DevRevTimer.Stop();
            DevRevTimer.Start();

            while (receiveBufferTemp.Count > 0)
            {
                int? index = null;
                bool findHeader = false;
                for (int i = 0; i < receiveBufferTemp.Count - 1; i++)
                {
                    if ((receiveBufferTemp[i] == 0xAA) && (receiveBufferTemp[i + 1] == 0x55))//判断是否存在帧头
                    {
                        index = i;//记录当前的位置
                        findHeader = true;
                        receiveBufferTemp.RemoveRange(0, i);
                        break;
                    }
                }

                if ((findHeader == false) ||
                    (receiveBufferTemp.Count < 3) ||
                    (receiveBufferTemp.Count < receiveBufferTemp[2]))
                //【无帧头 或 仅有帧头 或未接收完一个数据包】 则返回
                {
                    DevRIDB = receiveBufferTemp;//放入RIDB，下次拼接
                    return;
                }
                int packetNum = receiveBufferTemp[2];//数据包字节

                if (receiveBufferTemp.Count == packetNum)//【刚好接收完一个数据包】
                {
                    DevRIDB.Clear();
                }

                lock (DevRevDataArray)
                {
                    RevDataArray RevData = new RevDataArray();
                    RevData.Time = DateTime.Now;
                    RevData.Data = receiveBufferTemp.GetRange(0, packetNum);
                    DevRevDataArray.Add(RevData);//将数据转移到DeviceRevDataArray
                    receiveBufferTemp.RemoveRange(0, packetNum);//移除已转移的数据

                    var dataHandler = container.Resolve<IDataHandler>();
                    if ((DevRevDataArray.Count >= DevRevCapacity) && dataHandler.IsComplete && container.Resolve<IInputInfo>().IsDetectCycled)
                    {
                        dataHandler.IsComplete = false;
                        var DeviceRevDataArrayTemp = new List<RevDataArray>();
                        DeviceRevDataArrayTemp.AddRange(DevRevDataArray);
                        DevRevDataArray.Clear();//确保数据转移后，清空

                        Task.Run(() => { dataHandler.DeviceRevDataHandler(DeviceRevDataArrayTemp); });
                    }

                }
            }
        }


        #region 应变仪 监测定时器
        /// <summary>
        /// 单次测量 使用定时器进入线程，进行解码
        /// 连续测量 意外掉线时，进行重连
        /// </summary>       
        public System.Timers.Timer DevRevTimer { get; set; }

        private void DevRevTimerInit()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 200;
            DevRevTimer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            DevRevTimer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            //DevRevTimer.Enabled = true;
            //绑定Elapsed事件
            DevRevTimer.Elapsed += DevRevTimer_Elapsed;
        }
        private void DevRevTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 触发了就把定时器关掉，防止重复触发。
            DevRevTimer.Stop();

            //如果处于连续测量，进入定时器
            //则判断为意外掉线，进行重连
            if (container.Resolve<IInputInfo>().IsDetectCycled)
            {
                DevCMDSend(new SendCommand { Name = "DetectCycled" });
                DevRevTimer.Start();
            }
            else
            {
                var dataHandler = container.Resolve<IDataHandler>();
                if ((DevRevDataArray.Count != 0) && dataHandler.IsComplete)
                {
                    dataHandler.IsComplete = false;
                    var DeviceRevDataArrayTemp = new List<RevDataArray>();
                    DeviceRevDataArrayTemp.AddRange(DevRevDataArray);
                    DevRevDataArray.Clear();//数据转移后，清空

                    Task.Run(() => { dataHandler.DeviceRevDataHandler(DeviceRevDataArrayTemp); });
                }
            }
        }
        #endregion

    }
}
