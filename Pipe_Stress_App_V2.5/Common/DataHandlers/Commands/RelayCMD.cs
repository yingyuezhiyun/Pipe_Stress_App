using Pipe_Stress_App_V2._5.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers.Commands
{
    public class RelayCMD
    {
        public static int ID01 { get; set; } = 1;
        public static int ID02 { get; set; } = 0xff;
        public enum State
        {
            Open = 1,
            Close = 2,
        }

        /// <summary>
        /// Open Or Close
        /// </summary>
        /// <param name="ID">继电器ID</param>
        /// <param name="ch">通道 0-7 或者0-3</param>
        /// <param name="state">开为true，否则为false</param>
        public static List<byte> OC(int ID, int ch, State state)
        {
            List<byte> Send = new List<byte>();
            UInt16 crc;
            switch (ch)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    Send.Add((byte)ID);
                    Send.Add(5);
                    Send.Add(0);
                    Send.Add((byte)ch);
                    if (state == State.Open)
                    {
                        Send.Add(0xff);
                    }
                    else
                    {
                        Send.Add(0);
                    }
                    Send.Add(0);
                    //crc = DataCheck.CRC16_2(Send, Send.Count);
                    crc = Send.Check_CRC16();
                    Send.Add((byte)(crc & 0x00ff));
                    Send.Add((byte)(crc >> 8));
                    break;
                case 0xff:
                    Send.Add((byte)ID);
                    Send.Add(0x0f);
                    Send.Add(0);
                    Send.Add(0);
                    Send.Add(0);
                    Send.Add(8);
                    Send.Add(1);
                    if (state == State.Open)
                    {
                        Send.Add(0xff);
                    }
                    else
                    {
                        Send.Add(0);
                    }
                    //crc = DataCheck.CRC16_2(Send, Send.Count);
                    crc = Send.Check_CRC16();
                    Send.Add((byte)(crc & 0x00ff));
                    Send.Add((byte)(crc >> 8));
                    break;
                default:
                    break;
            }
            return Send;
        }

        /// <summary>
        /// give the device a new ID 
        /// </summary>
        /// <param name="IDnew"></param>
        /// <param name="overall">whether take effect to all</param>
        /// <param name="IDold"></param>
        /// <returns></returns>
        public static List<byte> ChangeID(int IDnew, bool overall = false, int? IDold = null)
        {
            List<byte> Send = new List<byte>();
            if ((overall == false) && (IDold == null))
            {
                return Send;
            }
            if (overall)
            {
                Send.Add(0);
            }
            else
            {
                Send.Add((byte)IDold);
            }
            Send.Add(0x10);
            Send.Add(0);
            Send.Add(0);
            Send.Add(0);
            Send.Add(01);
            Send.Add(02);
            Send.Add(0);
            Send.Add((byte)IDnew);
            UInt16 crc;
            //crc = DataCheck.CRC16_2(Send, Send.Count);
            crc = Send.Check_CRC16();
            Send.Add((byte)(crc & 0x00ff));
            Send.Add((byte)(crc >> 8));
            return Send;
        }


    }
}
