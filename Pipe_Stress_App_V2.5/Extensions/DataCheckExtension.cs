using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Extensions
{
    public static class DataCheckExtension
    {
        static UInt16[] wCRCTalbeAbs =
        {
            0x0000,
            0xCC01,
            0xD801,
            0x1400,
            0xF001,
            0x3C00,
            0x2800,
            0xE401,
            0xA001,
            0x6C00,
            0x7800,
            0xB401,
            0x5000,
            0x9C01,
            0x8801,
            0x4400,
        };



        public static Byte Check_Sum(this List<byte> data, int index = 0, int? count = null)
        {
            Byte crcbyte = 0x00;
            if (count == null)
            {
                count = data.Count;
            }
            for (int i = index; i < count + index; i++)
            {
                crcbyte += data[i];
            }

            return crcbyte;
        }

        public static UInt16 Check_CRC16(this List<byte> data, int index = 0, int? count = null)
        {
            UInt16 wCRC = 0xFFFF;
            byte chChar;
            if (count == null)
            {
                count = data.Count;
            }
            for (int i = index; i < count + index; i++)
            {
                chChar = data[i];
                wCRC = (UInt16)(wCRCTalbeAbs[(chChar ^ wCRC) & 15] ^ (wCRC >> 4));
                wCRC = (UInt16)(wCRCTalbeAbs[((chChar >> 4) ^ wCRC) & 15] ^ (wCRC >> 4));
            }

            return wCRC;
        }

    }
}
