using Pipe_Stress_App_V2._5.Common.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers.Interface
{
    public interface ISerialPortHandler
    {
        /// <summary>
        /// 应变仪串口
        /// </summary>
        public SerialPort DeviceSerialPort { get; set; }

        /// <summary>
        /// 继电器串口
        /// </summary>
        public SerialPort RelaySerialPort { get; set; }

        void DevCMDSend(SendCommand command);
        void MultiplexChange(int ch);
        void RelayAllOpenOrClose(bool open);

        /// <summary>
        /// 继电器 串口发送
        /// </summary>
        /// <param name="relayCMD"></param>
        public void RelayCMDSend(List<byte> relayCMD);
        void RelayToggle(int id, int index);

        public System.Timers.Timer DevRevTimer { get; set; }
    }
}
