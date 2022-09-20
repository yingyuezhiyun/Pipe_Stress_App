using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers.Commands
{
    public class DeviceCMD
    {



        static DeviceInfo deviceInfo = new DeviceInfo();
        /// <summary>
        /// 获取出厂信息
        /// </summary>
        /// <returns></returns>
        public static List<byte> GetFactoryVersion()
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x05);
            Send.Add(0x42);
            Send.Add(Send.Check_Sum());
            return Send;
        }

        /// <summary>
        /// 获取采样频率
        /// </summary>
        /// <returns></returns>
        public static List<byte> GetFrequency()
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x05);
            Send.Add(0x43);
            Send.Add(Send.Check_Sum());
            return Send;
        }

        /// <summary>
        /// 设置采样频率
        /// </summary>
        /// <param name="fre"></param>
        /// <returns></returns>
        public static List<byte> SetFrequency(int fre)
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x06);
            Send.Add(0x45);
            switch (fre)
            {
                case 10:
                    Send.Add(0x20);
                    break;
                case 40:
                    Send.Add(0x30);
                    break;
                case 100:
                    Send.Add(0x40);
                    break;
                case 400:
                    Send.Add(0x50);
                    break;
                case 1000:
                    Send.Add(0x60);
                    break;
                default: break;
            }
            Send.Add(Send.Check_Sum());
            return Send;
        }

        /// <summary>
        /// 单次测量
        /// </summary>
        /// <returns></returns>
        public static List<byte> Detect()
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x05);
            Send.Add(0x44);
            Send.Add(Send.Check_Sum());
            return Send;
        }

        /// <summary>
        /// 连续测量
        /// </summary>
        /// <returns></returns>
        public static List<byte> DetectCycled()
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x05);
            Send.Add(0x48);
            Send.Add(Send.Check_Sum());
            return Send;
        }

        /// <summary>
        /// 停止测量
        /// </summary>
        /// <returns></returns>
        public static List<byte> DetectStop()
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x05);
            Send.Add(0x4e);
            Send.Add(Send.Check_Sum());
            return Send;
        }


        /// <summary>
        /// 自动平衡
        /// </summary>
        /// <returns></returns>
        public static List<byte> ReturnToZero()
        {
            List<byte> Send = new List<byte>();
            Send.Add(0xaa);
            Send.Add(0x55);

            Send.Add(0x05);
            Send.Add(0x4a);
            Send.Add(Send.Check_Sum());
            return Send;
        }

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="revDatas"></param>
        /// <returns></returns>
        public static List<DetData> Unpack(List<RevDataArray> revDatas)
        {
            List<DetData> unpack = new List<DetData>();

            while (revDatas.Count >= 1)
            {
                if (revDatas[0].Data.Last()
                    != revDatas[0].Data.Check_Sum(count: revDatas[0].Data.Count - 1))
                //【和校验】 未通过
                {
                    revDatas.RemoveAt(0);
                    continue;
                }
                List<byte> validData = new List<byte>();

                validData = revDatas[0].Data;//添加至有效数据中

                switch (validData[3])
                {
                    case 0x42://出厂信息
                        deviceInfo.Model = validData[4].ToString("x2") + validData[5].ToString("x2");
                        deviceInfo.Number = validData[6].ToString("x2") + validData[7].ToString("x2") + validData[8].ToString("x2") + validData[9].ToString("x2");
                        deviceInfo.DateOfProduction = 2000 + validData[12] + "年" + validData[11] + "月" + validData[10] + "日";
                        break;
                    case 0x43://采集频率
                        switch (validData[4])
                        {
                            case 0x20:
                                deviceInfo.Frequency = 10;
                                break;
                            case 0x30:
                                deviceInfo.Frequency = 40;
                                break;
                            case 0x40:
                                deviceInfo.Frequency = 100;
                                break;
                            case 0x50:
                                deviceInfo.Frequency = 400;
                                break;
                            case 0x60:
                                deviceInfo.Frequency = 1000;
                                break;
                        }
                        break;
                    case 0x57: //错误信息
                        switch (validData[4])
                        {
                            case 0xd6:
                                deviceInfo.Error = "通讯失败";
                                break;
                            case 0xd7:
                                deviceInfo.Error = "无此功能";
                                break;
                        }
                        break;
                    case 0x44://单次
                    case 0x48://连续采集数据
                        List<int> strainUnconvert = new List<int>();//某时刻未经过转换的应变数据
                        List<double> strainInstant = new List<double>();//某时刻的应变数据
                        for (int i = 0; i < 16; i++)
                        {
                            strainUnconvert.Add(0);
                            strainUnconvert[i] = validData[3 * i + 4] + (validData[3 * i + 5] << 8) + (validData[3 * i + 6] << 16);
                            if (strainUnconvert[i] > 0x800000)
                            {
                                strainUnconvert[i] = -((~(strainUnconvert[i] - 1)) & 0x00ffffff);
                            }
                            if (strainUnconvert[i] == 0x800000)
                            {
                                strainInstant.Add(0);
                            }
                            else
                            {
                                strainInstant.Add((double)strainUnconvert[i] / 10.0);
                            }

                        }
                        unpack.Add(new DetData
                        {
                            Time = revDatas[0].Time,
                            StrainList = strainInstant
                        });
                        break;
                    default:
                        break;
                }
                revDatas.RemoveAt(0);//删除已转移或者校验不过的数据
            }
            return unpack;
        }


    }
}
