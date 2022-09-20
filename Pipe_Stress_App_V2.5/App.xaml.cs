using Pipe_Stress_App_V2._5.Common.Confi;
using Pipe_Stress_App_V2._5.Common.DataHandlers;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Dialog;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.ViewModels;
using Pipe_Stress_App_V2._5.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Pipe_Stress_App_V2._5
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.MainView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogHostService, DialogHostService>();


            containerRegistry.RegisterSingleton<IConfiManager, ConfiManager>();
            containerRegistry.RegisterSingleton<ISerialPortHandler, SerialPortHandler>();
            containerRegistry.RegisterSingleton<IDataHandler, DataHandler>();
            containerRegistry.RegisterSingleton<IMultiplexInfo, MultiplexInfo>();
            containerRegistry.RegisterSingleton<IInputInfo, InputInfo>();
            containerRegistry.RegisterSingleton<IFileSaveInfo, FileSaveInfo>();
            containerRegistry.RegisterSingleton<IRelayInfo, RelayInfo>();

            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();

            containerRegistry.RegisterForNavigation<SerialPortView, SerialPortViewModel>();
            containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
            containerRegistry.RegisterForNavigation<ChannelSelectView, ChannelSelectViewModel>("ChannelSelectView1");
            containerRegistry.RegisterForNavigation<ChannelSelectView, ChannelSelectViewModel>("ChannelSelectView2");
            containerRegistry.RegisterForNavigation<ChartView, ChartViewModel>("ChartView1");
            containerRegistry.RegisterForNavigation<ChartView, ChartViewModel>("ChartView2");
            containerRegistry.RegisterForNavigation<FFTChartView, FFTChartViewModel>("FFTChartView1");
            containerRegistry.RegisterForNavigation<FFTChartView, FFTChartViewModel>("FFTChartView2");

            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
            containerRegistry.RegisterForNavigation<MonitorView, MonitorViewModel>();
            containerRegistry.RegisterForNavigation<DataAnalysisView, DataAnalysisViewModel>();



        }
        protected override void OnInitialized()
        {
            var ConfiInit = Container.Resolve<IConfiManager>();
            if (ConfiInit != null)
            {
                ConfiInit.NavigationConfi();
                ConfiInit.SkinInit();
                ConfiInit.InputConfiInit();
                ConfiInit.MultiplexConfiInit();
                ConfiInit.FileSaveConfiInit();
            }
            base.OnInitialized();
        }
    }
}
