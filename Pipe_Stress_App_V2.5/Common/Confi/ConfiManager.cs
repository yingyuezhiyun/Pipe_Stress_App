using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Pipe_Stress_App_V2._5.Common.Confi
{
    public class ConfiManager : IConfiManager
    {
        private readonly IContainerExtension container;
        private readonly IRegionManager regionManager;
        public ConfiManager(IContainerExtension container, IRegionManager regionManager)
        {
            this.container = container;
            this.regionManager = regionManager;
        }
        public void NavigationConfi()
        {
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("MonitorView");
            
        }
        public void SkinInit()
        {
            var dark = false;
            if (bool.TryParse(OperateIniFile.ReadIniData("Skin", "Dark"), out dark))
            {
                ModifyTheme(theme => theme.SetBaseTheme(dark ? Theme.Dark : Theme.Light));
            }
            var temp = OperateIniFile.ReadIniData("Skin", "PrimaryMid");
            if (temp != null && temp != string.Empty)
            {
                //var color = System.Drawing.ColorTranslator.FromHtml(temp);
                var color = ColorConverter.ConvertFromString(temp);
                ChangeHue(color);
            }

        }

        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            modificationAction?.Invoke(theme);
            paletteHelper.SetTheme(theme);
        }
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private void ChangeHue(object obj)
        {
           
            var hue = (Color)obj;
            ITheme theme = paletteHelper.GetTheme();
            theme.PrimaryLight = new ColorPair(hue.Lighten());
            theme.PrimaryMid = new ColorPair(hue);
            theme.PrimaryDark = new ColorPair(hue.Darken());
            paletteHelper.SetTheme(theme);
        }



        public void FileSaveConfiInit()
        {
            var FileSaveConfi = container.Resolve<IFileSaveInfo>();

            FileSaveConfi.Period = 1;
            FileSaveConfi.NewFolderInterval = 1;
            FileSaveConfi.AutoSave = true;


            var temp = "";
            temp = OperateIniFile.ReadIniData("FileSaveConfi", "Period");
            if (temp != null && temp != string.Empty)
            {
                var vaule = 0.0;
                double.TryParse(temp, out vaule);
                FileSaveConfi.Period = vaule;
            }
            temp = OperateIniFile.ReadIniData("FileSaveConfi", "Interval");
            if (temp != null && temp != string.Empty)
            {
                var vaule = 0.0;
                double.TryParse(temp, out vaule);
                FileSaveConfi.Interval = vaule;
            }
            temp = OperateIniFile.ReadIniData("FileSaveConfi", "NewFolderInterval");
            if (temp != null && temp != string.Empty)
            {
                var vaule = 0.0;
                double.TryParse(temp, out vaule);
                FileSaveConfi.NewFolderInterval = vaule;
            }

            temp = OperateIniFile.ReadIniData("FileSaveConfi", "RootDirectory");
            if (temp != null && temp != string.Empty&& (new DirectoryInfo(temp)).Exists)
            {
                FileSaveConfi.RootDirectory = temp;
            }
            else
            {
                FileSaveConfi.RootDirectory = System.Environment.CurrentDirectory;
            }
            FileSaveConfi.FolderDirectory = FileManager.CreateNewFolder(FileSaveConfi.RootDirectory);
        }

        public void InputConfiInit()
        {
            var InputConfi = container.Resolve<IInputInfo>();


            InputConfi.ChartAutoRefresh = true;
            //InputConfi.IsMultiplex = true;

            InputConfi.FFTLength = 5;
            InputConfi.ScaleZoom = 1;
            InputConfi.Frequency = 1000;
            InputConfi.AutoRefreshInterval = 1;
            InputConfi.IsRealTime = true;


            string temp;
            temp = OperateIniFile.ReadIniData("InputConfi", "RootDirectory");
            if (temp != null && temp != string.Empty)
            {
                InputConfi.RootDirectory = temp;
            }
            temp = OperateIniFile.ReadIniData("InputConfi", "FolderPath");
            if (temp != null && temp != string.Empty)
            {
                InputConfi.FolderPath = temp;
            }
            temp = OperateIniFile.ReadIniData("InputConfi", "FilePath");
            if (temp != null && temp != string.Empty)
            {
                InputConfi.FilePath = temp;
            }
            DateTime time;
            if (DateTime.TryParse(OperateIniFile.ReadIniData("InputConfi", "PickerStartTime"), out time))
            {
                InputConfi.PickerStartTime = time;
            }
            if (DateTime.TryParse(OperateIniFile.ReadIniData("InputConfi", "PickerEndTime"), out time))
            {
                InputConfi.PickerEndTime = time;
            }

        }

        public void MultiplexConfiInit()
        {
            var MultiplexConfi = container.Resolve<IMultiplexInfo>();
            for (int i = 0; i < 4; i++)
            {
                var holdtime = 0.0;
                if (double.TryParse(OperateIniFile.ReadIniData("MultiplexConfi", "HoldingTime" + i), out holdtime))
                {
                    MultiplexConfi.Multiplexes[i].HoldingTime = holdtime;
                }
            }

            for (int i = 0; i < 4; i++)
            {

                for (int j = 0; j < 3; j++)
                {
                    var value = 0.0;
                    if (double.TryParse(OperateIniFile.ReadIniData("MultiplexConfi", "InitialValue" + (3 * i + j)), out value))
                        MultiplexConfi.Multiplexes[i].InitialValue[j] = value;
                }
            }
            MultiplexConfi.Multiplexes[0].State = true;
        }
    }
}
