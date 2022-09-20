using Microsoft.Win32;
using Pipe_Stress_App_V2._5.Common.DataHandlers;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Dialog;
using Pipe_Stress_App_V2._5.Common.Events;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class DataAnalysisViewModel : NavigationViewModel
    {
        private readonly IRegionManager regionManager;

        private readonly string _sourcenamespace = "DataAnalysisViewModel";

        private readonly IDialogHostService dialogHostService;
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand<object> PauseLoadInputFileCommand { get; private set; }


        CancellationTokenSource tokenSource = new CancellationTokenSource();
        ManualResetEvent resetEvent = new ManualResetEvent(true);



        public DataAnalysisViewModel(IRegionManager regionManager, IContainerProvider provider, IDialogHostService dialogHostService) : base(provider)
        {
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;
            ExecuteCommand = new DelegateCommand<string>(Execute);
            PauseLoadInputFileCommand = new DelegateCommand<object>((obj) => { PauseLoadInputFile(!(bool)obj); });
            InputConfi = provider.Resolve<IInputInfo>();
        }
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);


            var para = new NavigationParameters();
            para.Add("NameSpace", _sourcenamespace);//key必须为字符串，value可以传递object类型
            para.Add("Title", "离线震动波形数据");
            para.Add("StopAutoRefresh", true);
            para.Add("HideAutoRefresh", true);
            regionManager.Regions[PrismManager.DataAnalysisChartViewRegionName].RequestNavigate("ChartView2", para);
            regionManager.Regions[PrismManager.DataAnalysisFFTChartViewRegionName].RequestNavigate("FFTChartView2", para);
            regionManager.Regions[PrismManager.DataAnalysisChannelSelectViewRegionName].RequestNavigate("ChannelSelectView2", para);


        }

        #region 私有变量
        private int loadFileSelectIndex;
        private IInputInfo inputConfi;
        #endregion


        #region 变量
        /// <summary>
        /// 加载输入文件时 滑块index
        /// </summary>
        public int LoadFileSelectIndex { get => loadFileSelectIndex; set { loadFileSelectIndex = value; RaisePropertyChanged(); } }

        public IInputInfo InputConfi { get => inputConfi; set { inputConfi = value; RaisePropertyChanged(); } }


        public string PickerStartDate
        {
            get => InputConfi.PickerStartTime.Date.ToString();
            set
            {
                DateTime time;
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out time))
                {
                    var pickerstarttime = time.Date.Add(InputConfi.PickerStartTime.TimeOfDay);
                    InputConfi.PickerStartTime = pickerstarttime;

                    OperateIniFile.WriteIniData("InputConfi", "PickerStartTime", InputConfi.PickerStartTime.ToString());
                }
                RaisePropertyChanged();
            }
        }

        public string PickerStartTime
        {
            get => InputConfi.PickerStartTime.TimeOfDay.ToString();
            set
            {
                DateTime time;
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out time))
                {
                    var pickerstarttime = InputConfi.PickerStartTime.Date.Add(time.TimeOfDay);
                    InputConfi.PickerStartTime = pickerstarttime;
                    OperateIniFile.WriteIniData("InputConfi", "PickerStartTime", InputConfi.PickerStartTime.ToString());
                }
                RaisePropertyChanged();
            }
        }

        public string PickerEndDate
        {
            get => InputConfi.PickerEndTime.Date.ToString();
            set
            {
                DateTime time;
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out time))
                {
                    var pickerendtime = time.Date.Add(InputConfi.PickerEndTime.TimeOfDay);
                    InputConfi.PickerEndTime = pickerendtime;
                    OperateIniFile.WriteIniData("InputConfi", "PickerEndTime", InputConfi.PickerEndTime.ToString());
                }
                RaisePropertyChanged();
            }
        }

        public string PickerEndTime
        {
            get => InputConfi.PickerEndTime.TimeOfDay.ToString();
            set
            {
                DateTime time;
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out time))
                {
                    var pickerendtime = InputConfi.PickerEndTime.Date.Add(time.TimeOfDay);
                    InputConfi.PickerEndTime = pickerendtime;
                    OperateIniFile.WriteIniData("InputConfi", "PickerEndTime", InputConfi.PickerEndTime.ToString());
                }
                RaisePropertyChanged();
            }
        }

        #endregion



        /// <summary>
        /// 一些委托命令
        /// </summary>
        /// <param name="obj"></param>
        private void Execute(string obj)
        {
            switch (obj)
            {
                case "输入文件根目录":
                case "输入文件夹目录": FolderPiker(obj); break;
                case "输入文件目录": FilePicker(); break;
                case "打开输入文件目录":
                case "打开输入文件夹目录":
                case "打开输入文件根目录": GoToSurce(obj); break;

                case "返回文件加载页": LoadFileSelectIndex = 0; break;
                case "开始加载": LoadFileSelectIndex = 1; StartLoadInputFile(); break;
                case "中止文件加载": AbortLoadInputFile(); break;
                default: break;
            }
        }




        #region LoadInputFile

        public async void StartLoadInputFile()
        {
            switch (InputConfi.SelectType)
            {
                case 0:
                    if (InputConfi.FilePath == null || !(new System.IO.FileInfo(InputConfi.FilePath)).Exists)
                    {
                        var dialogResult = await dialogHostService.Question("温馨提示", "所选文件不存在！\r\n\r\n请检查文件目录，稍后再试！");
                        LoadFileSelectIndex = 0;
                        return;
                    }

                    InputConfi.CurrentIndex = 0;
                    InputConfi.Num = 1;
                    var formsinglefile = new List<DetData>();
                    await Task.Run(() => formsinglefile = DataSaveHandler.Read(new System.IO.FileInfo(InputConfi.FilePath)));
                    InputConfi.CurrentIndex = 1;
                    InputConfi.Num = 1;
                    aggregator.GetEvent<UpdataEvent>().Publish(new UpdataModel { Filter = _sourcenamespace, Data = formsinglefile });

                    break;
                case 1:
                    {
                        if (InputConfi.FolderPath == null || !(new System.IO.DirectoryInfo(InputConfi.FolderPath)).Exists)
                        {
                            var dialogResult = await dialogHostService.Question("温馨提示", "所选文件夹不存在！\r\n\r\n请检查文件夹目录，稍后再试！");
                            LoadFileSelectIndex = 0;
                            return;
                        }

                        DirectoryInfo selectfolder = new DirectoryInfo(InputConfi.FolderPath);
                        //theFolder 包含文件路径
                        FileInfo[] selectfiles = selectfolder.GetFiles("*.xlsx");

                        InputConfi.Num = selectfiles.Length;

                        if (InputConfi.Num == 0)
                        {
                            await dialogHostService.Question("温馨提示", "无查找结果，请重新选择");
                            LoadFileSelectIndex = 0;
                            return;
                        }
                        InputConfi.CurrentIndex = 0;
                        int index = 1;
                        CancellationToken token = tokenSource.Token;

                        await Task.Run(() =>
                        {
                            //遍历文件夹                
                            foreach (var file in selectfiles)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }
                                resetEvent.WaitOne();
                                //MessageBox.Show(file.ToString());
                                var fromfile = new List<DetData>();
                                fromfile = DataSaveHandler.Read(file);
                                aggregator.GetEvent<UpdataEvent>().Publish(new UpdataModel { Filter = _sourcenamespace, Data = fromfile });
                                InputConfi.CurrentIndex = index;
                                index++;
                            }

                        }, token);

                        break;
                    }
                case 2:
                    {
                        if (InputConfi.RootDirectory == null || !(new System.IO.DirectoryInfo(InputConfi.RootDirectory)).Exists)
                        {
                            var dialogResult = await dialogHostService.Question("温馨提示", "所选文件根目录不存在！\r\n\r\n请检查文件根目录，稍后再试！");
                            LoadFileSelectIndex = 0;
                            return;
                        }
                        if (InputConfi.PickerStartTime >= InputConfi.PickerEndTime)
                        {
                            await dialogHostService.Question("温馨提示", "开始时间大于结束时间");
                            LoadFileSelectIndex = 0;
                            return;
                        }
                        DirectoryInfo rootDirectory = new DirectoryInfo(InputConfi.RootDirectory);
                        DirectoryInfo[] allfolders = rootDirectory.GetDirectories();
                        var selectfolders = new List<DirectoryInfo>();
                        var pickerStartTime = InputConfi.PickerStartTime.Date.AddHours(InputConfi.PickerStartTime.Hour);
                        var pickerEndTime = InputConfi.PickerEndTime.Date.AddHours(InputConfi.PickerEndTime.Hour);
                        foreach (var item in allfolders)
                        {
                            DateTime foldertime;
                            if (DateTime.TryParseExact(item.Name, "yyyy年MM月dd日HH时", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out foldertime))
                            {
                                if ((pickerStartTime <= foldertime) && (foldertime <= pickerEndTime))
                                {
                                    //InputConfi.PickerStartTime.Subtract(InputConfi.PickerStartTime.)
                                    selectfolders.Add(item);
                                }
                            }
                        }
                        var selectfiles = new List<FileInfo>();
                        foreach (var folder in selectfolders)
                        {
                            foreach (var item in folder.GetFiles("*.xlsx"))
                            {
                                DateTime filetime;

                                if (DateTime.TryParseExact(item.Name.Split('.').First(), "yyyy年MM月dd日HH时mm分ss秒", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out filetime))
                                {
                                    if ((InputConfi.PickerStartTime <= filetime) && (filetime <= InputConfi.PickerEndTime))
                                    {
                                        selectfiles.Add(item);
                                    }
                                }
                            }

                        }


                        InputConfi.Num = selectfiles.Count;
                        if (InputConfi.Num == 0)
                        {
                            await dialogHostService.Question("温馨提示", "无查找结果，请重新选择");
                            LoadFileSelectIndex = 0;
                            return;
                        }

                        InputConfi.CurrentIndex = 0;
                        int index = 1;
                        CancellationToken token = tokenSource.Token;

                        await Task.Run(() =>
                        {
                            //遍历文件夹                
                            foreach (var file in selectfiles)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }
                                resetEvent.WaitOne();
                                var fromfile = new List<DetData>();
                                fromfile = DataSaveHandler.Read(file);
                                aggregator.GetEvent<UpdataEvent>().Publish(new UpdataModel { Filter = _sourcenamespace, Data = fromfile });
                                InputConfi.CurrentIndex = index;
                                index++;
                            }

                        }, token);

                        break;
                    }
                default:
                    break;
            }
        }


        public void PauseLoadInputFile(bool pause)
        {
            if (pause)
            {
                resetEvent.Reset();
            }
            else
            {
                resetEvent.Set();
            }

        }

        public void AbortLoadInputFile()
        {
            tokenSource.Cancel();
            tokenSource = new CancellationTokenSource();
        }

        #endregion

        /// <summary>
        /// 文件夹选取
        /// </summary>
        /// <param name="obj"></param>
        public void FolderPiker(string obj)
        {
            var dlg = new PickupFolderDialog();

            if (dlg.ShowDialog())
            {
                switch (obj)
                {


                    case "输入文件根目录":
                        InputConfi.RootDirectory = dlg.SelectedPath;
                        OperateIniFile.WriteIniData("InputConfi", "RootDirectory", InputConfi.RootDirectory);
                        break;
                    case "输入文件夹目录":
                        InputConfi.FolderPath = dlg.SelectedPath;
                        OperateIniFile.WriteIniData("InputConfi", "FolderPath", InputConfi.FolderPath);
                        break;
                    default:
                        break;
                }

            }
        }
        /// <summary>
        /// 文件选取
        /// </summary>
        public void FilePicker()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (InputConfi.FilePath!=null&& ((new FileInfo(InputConfi.FilePath)).Exists))
            {
                ofd.InitialDirectory = new FileInfo(InputConfi.FilePath).DirectoryName;
                //ofd.FileName = InputConfi.FilePath;

            }
            ofd.Filter = "XLSX(*.xlsx;)|*.xlsx";

            if (ofd.ShowDialog() == true)
            {
                InputConfi.FilePath = ofd.FileName;
                OperateIniFile.WriteIniData("InputConfi", "FilePath", InputConfi.FilePath);
            }
        }
        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="obj"></param>
        public void GoToSurce(string obj)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "explorer.exe";
            switch (obj)
            {
                case "打开输入文件目录": proc.StartInfo.Arguments = InputConfi.FilePath; break;
                case "打开输入文件夹目录": proc.StartInfo.Arguments = InputConfi.FolderPath; break;
                case "打开输入文件根目录": proc.StartInfo.Arguments = InputConfi.RootDirectory; break;
                default:
                    break;
            }
            proc.Start();
        }
    }
}
