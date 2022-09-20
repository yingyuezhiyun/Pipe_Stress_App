using Microsoft.Win32;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Dialog;
using Pipe_Stress_App_V2._5.Common.Events;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.Extensions;
using Pipe_Stress_App_V2._5.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class MonitorViewModel : NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IDialogHostService dialog;
        private readonly ISerialPortHandler serialPortHandler;
        private readonly IDataHandler dataHandler;
        private readonly IContainerProvider provider;
        private readonly string _sourcenamespace = "MonitorViewModel";

        #region 委托命令
        /// <summary>
        /// 一些简单的命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        /// <summary>
        /// 复用时 通道的测量时间 修改命令
        /// </summary>
        public DelegateCommand<object> HoldingTimeChangeCommand { get; private set; }
        /// <summary>
        /// 继电器1 单独控制命令
        /// </summary>
        public DelegateCommand<object> Relay1StateChangeCommand { get; private set; }
        /// <summary>
        /// 继电器2 单独控制命令
        /// </summary>
        public DelegateCommand<object> Relay2StateChangeCommand { get; private set; }
        #endregion


        public MonitorViewModel(IDialogHostService dialog, IRegionManager regionManager, IContainerProvider provider) : base(provider)
        {
            this.regionManager = regionManager;
            this.dialog = dialog;
            this.provider = provider;
            MultiplexConfi = provider.Resolve<IMultiplexInfo>();
            RelayStatus = provider.Resolve<IRelayInfo>();
            InputConfi = provider.Resolve<IInputInfo>();
            FileSaveConfi = provider.Resolve<IFileSaveInfo>();
            serialPortHandler = provider.Resolve<ISerialPortHandler>();
            dataHandler = provider.Resolve<IDataHandler>();

            dataHandler._sourcenamespace = "MonitorViewModel";

            ExecuteCommand = new DelegateCommand<string>(Execute);
            HoldingTimeChangeCommand = new DelegateCommand<object>((obj) => { ChangeHoldTime(obj); });
            Relay1StateChangeCommand = new DelegateCommand<object>((obj) => { serialPortHandler?.RelayToggle(0, Convert.ToInt32(obj)); });
            Relay2StateChangeCommand = new DelegateCommand<object>((obj) => { serialPortHandler?.RelayToggle(1, Convert.ToInt32(obj)); });


        }




        public override void OnNavigatedTo(NavigationContext navigationContext)
        {

            base.OnNavigatedTo(navigationContext);
            var para = new NavigationParameters();
            para.Add("NameSpace", _sourcenamespace);//key必须为字符串，value可以传递object类型
            para.Add("Title", "实时震动波形数据");
            regionManager.Regions[PrismManager.MonitorChartViewRegionName].RequestNavigate("ChartView1", para);
            regionManager.Regions[PrismManager.MonitorFFTChartViewRegionName].RequestNavigate("FFTChartView1", para);
            regionManager.Regions[PrismManager.MonitorChannelSelectViewRegionName].RequestNavigate("ChannelSelectView1", para);

          
        }

        #region 私有变量

        private IMultiplexInfo multiplexConfi;
        private IRelayInfo relayStatus;
        private IInputInfo inputConfi;
        private IFileSaveInfo fileSaveConfi;
        private ObservableCollection<int> frequency = new ObservableCollection<int> { 10, 40, 100, 400, 1000 };

        #endregion


        #region 参数变量
        /// <summary>
        /// 复用时的一些参数
        /// </summary>
        public IMultiplexInfo MultiplexConfi
        {
            get { return multiplexConfi; }
            set { multiplexConfi = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 继电器的一些参数
        /// </summary>
        public IRelayInfo RelayStatus
        {
            get { return relayStatus; }
            set { relayStatus = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 输入配置文件的一些参数
        /// </summary>
        public IInputInfo InputConfi { get => inputConfi; set { inputConfi = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 文件保持时的一些参数
        /// </summary>
        public IFileSaveInfo FileSaveConfi { get => fileSaveConfi; set { fileSaveConfi = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 采样频率
        /// </summary>
        public ObservableCollection<int> Frequency { get => frequency; set { frequency = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 采集仪连接状态信息
        /// </summary>
        public MenuBar DeviceConnectedStatus
        {
            get
            {
                return serialPortHandler.DeviceSerialPort.IsOpen ?
                     new MenuBar { Color = "Blue", Icon = "CheckboxMarkedCircleOutline", Title = "Device Online" } :
                     new MenuBar { Color = "Red", Icon = "CloseOctagonOutline", Title = "Device Offline" };
            }

            set { RaisePropertyChanged(); }
        }

        /// <summary>
        /// 继电器连接状态信息
        /// </summary>
        public MenuBar RelayConnectedStatus
        {
            get
            {
                return serialPortHandler.RelaySerialPort.IsOpen ?
                    new MenuBar { Color = "Blue", Icon = "CheckboxMarkedCircleOutline", Title = "Relay Online" } :
                    new MenuBar { Color = "Red", Icon = "CloseOctagonOutline", Title = "Relay Offline" };
            }
            set
            {
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 采集仪和继电器连接状态对应的图片
        /// </summary>
        public string ConnectedStatusImage
        {
            get
            {

                if (serialPortHandler.DeviceSerialPort.IsOpen && serialPortHandler.RelaySerialPort.IsOpen)
                {
                    return "/Assets/Images/AllConnected.png";
                }
                if (serialPortHandler.DeviceSerialPort.IsOpen && !serialPortHandler.RelaySerialPort.IsOpen)
                {
                    return "/Assets/Images/DeviceConnected.png";
                }
                if (!serialPortHandler.DeviceSerialPort.IsOpen && serialPortHandler.RelaySerialPort.IsOpen)
                {
                    return "/Assets/Images/RelayConnected.png";
                }
                return "/Assets/Images/NoneConnected.png";
            }
            set { RaisePropertyChanged(); }
        }

        #endregion


        /// <summary>
        /// 改变复用时 通道的测量时间
        /// </summary>
        /// <param name="obj"></param>
        public void ChangeHoldTime(object obj)
        {
            switch ((string)obj)
            {
                case "5#": OperateIniFile.WriteIniData("MultiplexConfi", "HoldingTime0", MultiplexConfi.Multiplexes[0].HoldingTime.ToString()); break;
                case "6#": OperateIniFile.WriteIniData("MultiplexConfi", "HoldingTime1", MultiplexConfi.Multiplexes[1].HoldingTime.ToString()); break;
                case "7#": OperateIniFile.WriteIniData("MultiplexConfi", "HoldingTime2", MultiplexConfi.Multiplexes[2].HoldingTime.ToString()); break;
                case "8#": OperateIniFile.WriteIniData("MultiplexConfi", "HoldingTime3", MultiplexConfi.Multiplexes[3].HoldingTime.ToString()); break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 一些简单的 命令
        /// </summary>
        /// <param name="obj"></param>
        private void Execute(string obj)
        {
            switch (obj)
            {

                case "串口": SerialPortConnect(); break;

                case "自动平衡":
                case "单次测量":
                case "连续测量": DeviceCommand(obj); break;
                case "采样频率改变": serialPortHandler?.DevCMDSend(new SendCommand { Name = "SetFrequency", Value = InputConfi.Frequency }); break;



                case "继电器全开":
                case "继电器全关":
                case "切换#5":
                case "切换#6":
                case "切换#7":
                case "切换#8": RelayCommand(obj); break;
                case "清空初值": dataHandler?.ClearInitialValues(); break;
                case "取初值": dataHandler?.GetCurrentInitialValue(); break;
                case "设定初值": dataHandler?.SetInitialValues(); break;
                case "自动复用": AutoMultiplex(); break;

                case "保存文件根目录": FolderPiker(obj); break;


                case "打开保存文件根目录": GoToSurce(obj); break;

                default: break;
            }
        }

        /// <summary>
        /// 采集仪和继电器 串口连接弹窗
        /// </summary>
        private async void SerialPortConnect()
        {
            DialogParameters param = new DialogParameters();
            //param.Add("SerialPort",serialPortModel);
            var dialogResult = await dialog.ShowDialog("SerialPortView", param);
            if (dialogResult.Result == ButtonResult.OK)
            {
            }
            DeviceConnectedStatus = new MenuBar();
            RelayConnectedStatus = new MenuBar();
            ConnectedStatusImage = " ";
        }


        /// <summary>
        /// 采集仪控制命令
        /// </summary>
        /// <param name="obj"></param>
        public void DeviceCommand(string obj)
        {

            if (!serialPortHandler.DeviceSerialPort.IsOpen)
            {
                aggregator.SendMessage("设备未连接");
                return;
            }
            switch (obj)
            {
                case "自动平衡": serialPortHandler.DevCMDSend(new SendCommand { Name = "ReturnToZero" }); break;
                case "单次测量": serialPortHandler.DevCMDSend(new SendCommand { Name = "Detect" }); break;
                case "连续测量":
                    if (InputConfi.IsDetectCycled)
                    {
                        serialPortHandler?.DevCMDSend(new SendCommand { Name = "DetectStop" });
                        serialPortHandler.DeviceSerialPort.ReceivedBytesThreshold = 1;
                        serialPortHandler.DevRevTimer.Interval = 200;
                        InputConfi.IsDetectCycled = false;
                    }
                    else
                    {
                        serialPortHandler?.DevCMDSend(new SendCommand { Name = "DetectCycled" });
                        serialPortHandler.DeviceSerialPort.ReceivedBytesThreshold = 53 * 1000;
                        serialPortHandler.DevRevTimer.Interval = 1500;
                        InputConfi.IsDetectCycled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 继电器控制命令
        /// </summary>
        /// <param name="obj"></param>
        public void RelayCommand(string obj)
        {

            if (!serialPortHandler.RelaySerialPort.IsOpen)
            {
                aggregator.SendMessage("继电器未连接");
                return;
            }
            switch (obj)
            {
                case "继电器全开": serialPortHandler.RelayAllOpenOrClose(open: true); break;
                case "继电器全关": serialPortHandler.RelayAllOpenOrClose(open: false); break;
                case "切换#5": serialPortHandler.MultiplexChange(0); break;
                case "切换#6": serialPortHandler.MultiplexChange(1); break;
                case "切换#7": serialPortHandler.MultiplexChange(2); break;
                case "切换#8": serialPortHandler.MultiplexChange(3); break;
                default:
                    break;
            }


        }


        /// <summary>
        /// 设置复用的开启和关闭
        /// </summary>
        public async void AutoMultiplex()
        {
            await Task.Delay(100);
            if (InputConfi.IsMultiplex)
            {
                dataHandler?.AutoMultiplex(run: true);
            }
            else
            {
                dataHandler?.AutoMultiplex(run: false);
            }

        }


        public void FolderPiker(string obj)
        {
            var dlg = new PickupFolderDialog();
            if (dlg.ShowDialog())
            {
                FileSaveConfi.RootDirectory = dlg.SelectedPath;
                FileSaveConfi.FolderDirectory = FileManager.CreateNewFolder(FileSaveConfi.RootDirectory);
                OperateIniFile.WriteIniData("FileSaveConfi", "RootDirectory", FileSaveConfi.RootDirectory);
            }
            
        }

        public void GoToSurce(string obj)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "explorer.exe";
            proc.StartInfo.Arguments = FileSaveConfi.RootDirectory;
            proc.Start();
        }
    }
}
