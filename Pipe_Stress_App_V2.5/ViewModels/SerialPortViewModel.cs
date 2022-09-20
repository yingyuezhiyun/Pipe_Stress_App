using MaterialDesignThemes.Wpf;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Dialog;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class SerialPortViewModel : BindableBase, IDialogHostAware
    {


        public DelegateCommand<string> ExecuteCommand { get; private set; }

        private readonly IEventAggregator aggregator;

        private readonly IContainerExtension container;

        public SerialPortViewModel(IEventAggregator aggregator, IContainerExtension container)
        {
            ExecuteCommand = new DelegateCommand<string>(Execute);
            this.aggregator = aggregator;
            this.container = container;
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);

        }


        private string deviceConnectState;
        private string relayConnectState;
        private List<string> portName;
        private SerialPort deviceSerialPort;
        private SerialPort relaySerialPort;
        private List<int> baudRate = new List<int> { 4800, 9600, 19200, 115200, 128000, 230400, 256000, 460800, 500000, 512000, 921600 };
        private List<int> dataBits = new List<int> { 6, 7, 8 };
        private int devPortSelectIndex;
        private int relayPortSelectIndex;



        /// <summary>
        /// 应变仪连接状态
        /// </summary>
        public string DeviceConnectState
        {
            get { return deviceConnectState; }
            set { deviceConnectState = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 继电器连接状态
        /// </summary>
        public string RelayConnectState
        {
            get { return relayConnectState; }
            set { relayConnectState = value; RaisePropertyChanged(); }
        }
        public List<string> PortName
        {
            get => SerialPort.GetPortNames().ToList();
            set { portName = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 应变仪串口
        /// </summary>
        public SerialPort DeviceSerialPort
        {
            get { return deviceSerialPort; }
            set { deviceSerialPort = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 继电器串口
        /// </summary>
        public SerialPort RelaySerialPort
        {
            get { return relaySerialPort; }
            set { relaySerialPort = value; RaisePropertyChanged(); }
        }
        public List<int> BaudRate
        {
            get { return baudRate; }
            set { baudRate = value; }
        }
        public List<int> DataBits
        {
            get { return dataBits; }
            set { dataBits = value; }
        }
        public int DevPortSelectIndex { get => devPortSelectIndex; set { devPortSelectIndex = value; RaisePropertyChanged(); } }
        public int RelayPortSelectIndex { get => relayPortSelectIndex; set { relayPortSelectIndex = value; RaisePropertyChanged(); } }

        public string DialogHostName { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "应变仪连接": DeviceConnect(); break;
                case "继电器连接": RelayConnect(); break;
                default: break;
            }
        }

        private void RelayConnect()
        {
            if (RelaySerialPort.IsOpen)
            {
                try
                {
                    RelaySerialPort.Close();
                    RelayPortSelectIndex = 0;
                    if (container.Resolve<IInputInfo>().IsMultiplex)
                    {
                        container.Resolve<IDataHandler>()?.AutoMultiplex(run: false);
                    }
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "SerialPort");
                };
            }
            else
            {
                try
                {
                    RelaySerialPort.Open();
                    RelayPortSelectIndex = 1;
                    if (container.Resolve<IInputInfo>().IsMultiplex)
                    {
                        container.Resolve<IDataHandler>()?.AutoMultiplex(run: true);
                    }
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "SerialPort");
                };
            }
            RelayConnectState = (RelaySerialPort.IsOpen ? "断开" : "连接");


        }

        private void DeviceConnect()
        {
            if (DeviceSerialPort.IsOpen)
            {
                try
                {
                    DeviceSerialPort.Close();
                    DevPortSelectIndex = 0;
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "SerialPort");
                };
            }
            else
            {
                try
                {
                    DeviceSerialPort.Open();
                    DevPortSelectIndex = 1;
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "SerialPort");
                };
            }


            DeviceConnectState = (DeviceSerialPort.IsOpen ? "断开" : "连接");



        }

        public void OnDialogOpend(IDialogParameters parameters)
        {
            var iSerialPort = container.Resolve<ISerialPortHandler>();
            DeviceSerialPort = iSerialPort.DeviceSerialPort;
            DeviceConnectState = (DeviceSerialPort.IsOpen ? "断开" : "连接");
            if (DeviceSerialPort.IsOpen)
            {
                DevPortSelectIndex = 1;
            }

            RelaySerialPort = iSerialPort.RelaySerialPort;
            RelayConnectState = (RelaySerialPort.IsOpen ? "断开" : "连接");
            if (RelaySerialPort.IsOpen)
            {
                RelayPortSelectIndex = 1;
            }
        }



        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
        }

        private void Save()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters param = new DialogParameters();
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
            }
        }
    }
}
