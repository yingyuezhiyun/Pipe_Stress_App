using Pipe_Stress_App_V2._5.Common.DataHandlers.Commands;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Common.Events;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.Extensions;

using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers
{
    public class DataHandler : IDataHandler
    {
        private readonly IContainerExtension container;
        private readonly IEventAggregator aggregator;
        public string _sourcenamespace { get; set; }

        public DataHandler(IContainerExtension container, IEventAggregator aggregator)
        {
            this.container = container;
            this.aggregator = aggregator;
            GetInitialValues();
            GetMultiplexHoldingTime();

            ExportTimerInit();
            MultiplexTimerInit();

        }



        #region 接收数据处理


        /// <summary>
        /// 数据处理是否完成
        /// </summary>
        public bool IsComplete { get; set; } = true;
        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="revDatas"></param>
        public void DeviceRevDataHandler(List<RevDataArray> revDatas)
        {

            var Multiplexes = container.Resolve<IMultiplexInfo>().Multiplexes;
            // 数据解包
            List<DetData> strainsOrigin = DeviceCMD.Unpack(revDatas);

            if (strainsOrigin.Count == 0)//如果没有数据
            {
                IsComplete = true;
                return;
            }

            CurrentInitialValue.Clear();
            CurrentInitialValue.Add(strainsOrigin.Select(a => a.StrainList[12]).ToList().Average());
            CurrentInitialValue.Add(strainsOrigin.Select(a => a.StrainList[13]).ToList().Average());
            CurrentInitialValue.Add(strainsOrigin.Select(a => a.StrainList[14]).ToList().Average());

            ///数据解包
            List<DetData> strainsCompensated = new List<DetData>();
            ///补偿处理
            for (int i = 0; i < strainsOrigin.Count; i++)
            {
                List<double> strainInstant = new List<double>();

                strainInstant.AddRange(strainsOrigin[i].StrainList.GetRange(0, 12));///获取前4个通道的值 前4个通道无复用
                for (int j = 0; j < 4; j++)
                {

                    if (Multiplexes[j].State)///是否处于复用中
                    {
                        strainInstant.Add(Math.Round(strainsOrigin[i].StrainList[12] - InitialValues[3 * j + 0], 1));
                        strainInstant.Add(Math.Round(strainsOrigin[i].StrainList[13] - InitialValues[3 * j + 1], 1));
                        strainInstant.Add(Math.Round(strainsOrigin[i].StrainList[14] - InitialValues[3 * j + 2], 1));
                    }
                    else
                    {
                        strainInstant.Add(0);
                        strainInstant.Add(0);
                        strainInstant.Add(0);
                    }
                }
                strainsCompensated.Add(new DetData
                {
                    Time = strainsOrigin[i].Time,
                    StrainList = strainInstant
                });
            }

            var fileSaveConfi = container.Resolve<IFileSaveInfo>();
            lock (exportData)//保存的数据
            {
                if (fileSaveConfi.AutoSave)
                {
                    exportData.AddRange(strainsCompensated);
                }
            }
            if (container.Resolve<IInputInfo>().IsRealTime)
            {
                aggregator.GetEvent<UpdataEvent>().Publish(new UpdataModel { Filter = _sourcenamespace, Data = strainsCompensated });
            }
            IsComplete = true;
        }

        #endregion


        #region 文件定时保存

        public List<DetData> exportData { get; set; } = new List<DetData>();
        System.Timers.Timer exportTimer;
        private void ExportTimerInit()
        {
            int exportTimerInterval = 1000;
            exportTimer = new System.Timers.Timer(exportTimerInterval);
            exportTimer.AutoReset = true;
            exportTimer.Elapsed += ExportTimer_Elapsed;
            exportTimer.Start();
        }
        int CreateFileTimeCount = 0;
        int TimeCount = 0;
        private void ExportTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            var fileSaveConfi = container.Resolve<IFileSaveInfo>();
            var inputConfi = container.Resolve<IInputInfo>();
            if (fileSaveConfi.AutoSave & inputConfi.IsDetectCycled)
            {
                if (CreateFileTimeCount >= 1000 / exportTimer.Interval * 60 * fileSaveConfi.Period)
                {
                    CreateFileTimeCount = -1;
                    lock (exportData)
                    {
                        var exportDataTemp = new List<DetData>();
                        exportDataTemp.AddRange(exportData);
                        if (exportData.Count == 0)
                        {
                            return;
                        }
                        exportData.Clear();
                        var now = DateTime.Now;
                        var lastFolder = DateTime.ParseExact(fileSaveConfi.FolderDirectory.Name, "yyyy年MM月dd日HH时", System.Globalization.CultureInfo.CurrentCulture);

                        if ((now - lastFolder).TotalSeconds >= 60.0 * 60.0 * fileSaveConfi.NewFolderInterval)///创建文件夹
                        {
                            fileSaveConfi.FolderDirectory = FileManager.CreateNewFolder(fileSaveConfi.RootDirectory);
                        }
                        //Thread thread = new Thread(() => MyEPPlus.CreateAndSave(fileSaveConfi.FolderDirectory, exportDataTemp));
                        //thread.Start();
                        Task.Run(() => { DataSaveHandler.CreateAndSave(fileSaveConfi.FolderDirectory, exportDataTemp); });
                    }
                }

                if (TimeCount >= 1000 / exportTimer.Interval * 60 * (fileSaveConfi.Interval + fileSaveConfi.Period))
                {
                    TimeCount = 0;
                    CreateFileTimeCount = 0;
                    if (fileSaveConfi.Interval != 0)
                    {
                        lock (exportData)
                        {
                            exportData.Clear();
                        }
                    }
                }
                if (CreateFileTimeCount >= 0)
                {
                    CreateFileTimeCount++;
                }
                TimeCount++;
            }

        }

        #endregion



        #region 初始复用补偿值 设定

        /// <summary>
        /// 全部初始值
        /// </summary>
        public List<double> InitialValues { get; set; } = new List<double>();
        /// <summary>
        /// 当前复用通道的初始值
        /// </summary>
        public List<double> CurrentInitialValue { get; set; } = new List<double>();
        public List<double> GetCurrentInitialValue()
        {
            if (CurrentInitialValue.Count < 3)
            {
                return new List<double>();
            }
            var Multiplexes = container.Resolve<IMultiplexInfo>().Multiplexes;
            for (int i = 0; i < 4; i++)
            {
                if (Multiplexes[i].State)
                {
                    container.Resolve<IMultiplexInfo>().Multiplexes[i].InitialValue[0] = CurrentInitialValue[0];
                    container.Resolve<IMultiplexInfo>().Multiplexes[i].InitialValue[1] = CurrentInitialValue[1];
                    container.Resolve<IMultiplexInfo>().Multiplexes[i].InitialValue[2] = CurrentInitialValue[2];
                    break;
                }
            }
            return CurrentInitialValue;
        }
        public void SetInitialValues()
        {
            var Multiplexes = container.Resolve<IMultiplexInfo>().Multiplexes;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    InitialValues[3 * i + j] = Multiplexes[i].InitialValue[j];
                    OperateIniFile.WriteIniData("MultiplexConfi", "InitialValue" + (3 * i + j), InitialValues[3 * i + j].ToString());
                }
            }
        }
        public void GetInitialValues()
        {
            for (int i = 0; i < 12; i++)
            {
                var value = 0.0;
                double.TryParse(OperateIniFile.ReadIniData("MultiplexConfi", "InitialValue" + i), out value);
                InitialValues.Add(value);
            }
        }
        public void ClearInitialValues()
        {
            var Multiplexes = container.Resolve<IMultiplexInfo>().Multiplexes;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    InitialValues[3 * i + j] = 0;
                    Multiplexes[i].InitialValue[j] = 0;
                    OperateIniFile.WriteIniData("MultiplexConfi", "InitialValue" + 3 * i + j, InitialValues[3 * i + j].ToString());
                }
            }
        }

        #endregion


        #region 定时复用
        public List<double> MultiplexHoldingTime { get; set; } = new List<double>();

        public void GetMultiplexHoldingTime()
        {
            MultiplexHoldingTime.Clear();
            for (int i = 0; i < 4; i++)
            {
                MultiplexHoldingTime.Add(container.Resolve<IMultiplexInfo>().Multiplexes[i].HoldingTime);
            }
        }

        public void AutoMultiplex(bool run)
        {
            if (run)
            {
                multiplexTimer.Start();
            }
            else
                multiplexTimer.Stop();
        }


        System.Timers.Timer multiplexTimer;
        public int MultiplexTimeCount = 0;
        public int MultiplexState = 0;


        private void MultiplexTimerInit()
        {
            int emultiplexTimerInterval = 1000;
            multiplexTimer = new System.Timers.Timer(emultiplexTimerInterval);
            multiplexTimer.AutoReset = true;
            multiplexTimer.Elapsed += MultiplexTimer_Elapsed;
            multiplexTimer.Stop();
        }
        private void MultiplexTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MultiplexTimeCount++;

            if (MultiplexTimeCount >= 1000 / multiplexTimer.Interval * 60 * MultiplexHoldingTime[MultiplexState])//到达设定时间
            {
                MultiplexTimeCount = 0;
                MultiplexState++;
                if (MultiplexState > 3)
                {
                    MultiplexState = 0;
                }
                container.Resolve<ISerialPortHandler>()?.MultiplexChange(MultiplexState);
            }
        }

        #endregion


        /// <summary>
        /// 应变转应力
        ///  TO: 有待优化
        /// </summary>
        /// <param name="OriginData"></param>
        /// <returns></returns>
        public List<DetData> StainToValidStress(List<DetData> OriginData)
        {
            var ConvertedData = new List<DetData>();
            var count = OriginData.Count;
            var avg = new List<double>();
            for (int i = 0; i < 24; i++)
            {
                avg.Add(OriginData.Select(a => a.StrainList[i]).ToList().Average());

                //avg.Add(0);
            }
            for (int i = 0; i < count; i++)
            {
                var data = new DetData();
                //data.StressList = new List<double> { 0, 0, 0, 0, 0, 0, 0,0 };

                for (int j = 0; j < 8; j++)
                {


                    var dx = 0.206 / (1.0 - Math.Pow(0.3, 2.0)) * ((OriginData[i].StrainList[3 * j + 0] - avg[3 * j + 0]) + 0.3 * (OriginData[i].StrainList[3 * j + 2]) - avg[3 * j + 2]);
                    var dy = 0.206 / (1.0 - Math.Pow(0.3, 2.0)) * (0.3 * (OriginData[i].StrainList[3 * j + 0] - avg[3 * j + 0]) + (OriginData[i].StrainList[3 * j + 2] - avg[3 * j + 2]));
                    var tx = 0.206 / (1.0 + 0.3) * (0.5 * (OriginData[i].StrainList[3 * j + 0] - avg[3 * j + 0]) - (OriginData[i].StrainList[3 * j + 1] - avg[3 * j + 1]) + 0.5 * (OriginData[i].StrainList[3 * j + 2]) - avg[3 * j + 2]);

                    var stress = 0.0;
                    if (dx + dy < 0)
                    {
                        stress = (dx + dy) / 2.0 - Math.Sqrt(Math.Pow((dx - dy) / 2.0, 2.0) + Math.Pow(tx, 2.0));
                    }
                    else
                    {
                        stress = (dx + dy) / 2.0 + Math.Sqrt(Math.Pow((dx - dy) / 2.0, 2.0) + Math.Pow(tx, 2.0));
                    }
                    //stress = (dx + dy) / 2.0 + Math.Sqrt(Math.Pow((dx - dy)/2.0, 2.0) + Math.Pow(tx, 2.0));
                    data.StressList.Add(stress);
                }

                data.Time = OriginData[i].Time;
                data.StrainList = OriginData[i].StrainList;

                ConvertedData.Add(data);
            }

            return ConvertedData;
        }


    }
}
