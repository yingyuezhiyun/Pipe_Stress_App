using Microsoft.Win32;
using Pipe_Stress_App_V2._5.Common.DataHandlers.Interface;
using Pipe_Stress_App_V2._5.Common.Events;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class ChartViewModel : NavigationViewModel, ITest
    {
        public string _sourcenamespace { get; set; }
        private readonly IDataHandler dataHandler;
        public DelegateCommand<string> ExecuteCommand { get; private set; }

       
        public ChartViewModel(IContainerProvider provider) : base(provider)
        {

          
            InputConfi = provider.Resolve<IInputInfo>();
            dataHandler = provider.Resolve<IDataHandler>();
            scottplot = new WpfPlot();
            scottplot.Plot.Style(figureBackground: System.Drawing.Color.Transparent);
            scottplot.Plot.Style(dataBackground: System.Drawing.Color.Transparent);
            ///禁止DPI缩放 以获得更高的清晰度
            scottplot.Configuration.DpiStretch = false;
            ///设置边框为隐藏
            scottplot.Plot.XAxis2.Line(false);
            scottplot.Plot.YAxis2.Line(false);
            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("hu"); // Hungarian
            scottplot.Plot.SetCulture(culture);
            xAxis3 = scottplot.Plot.AddAxis(ScottPlot.Renderable.Edge.Bottom, axisIndex: 2);
            //xAxis3.Ticks(true);
            xAxis3.DateTimeFormat(true);
            xAxis3.LockLimits(false);
            xAxis3.Hide();

            timeAxis = scottplot.Plot.AddScatterList(lineWidth: 0, markerSize: 0);
            timeAxis.XAxisIndex = 2;

            for (int i = 0; i < 32; i++)
            {
                //signalPlotLists.Add(AddSignalList(sampleRate: 1000, color: CurveColors[i]));
                signalPlotLists.Add(AddSignalList(sampleRate: 1000));
                signalPlotLists[i].IsVisible = false;
            }
            signalPlotLists[0].IsVisible = true;
            scottplot.Refresh();
            MaxSignalCount = 5_400_000; //5400秒
            ExecuteCommand = new DelegateCommand<string>(Execute);

            aggregator.GetEvent<ChannelChangeEvent>().Subscribe(a =>
            {
                ChannelChange(a);
            });

            aggregator.GetEvent<UpdataEvent>().Subscribe(a =>
            {
                if (a.Filter == _sourcenamespace)
                {
                    AddData(a.Data);
                }
            });

            RefreshTimerInit();

            scottplot.AxesChanged += ScaleChanged;
            SendToFFTDelayTimerInit();
        }




        /// <summary>
        /// 当导航到该页面时
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if (navigationContext.Parameters.ContainsKey("NameSpace"))
            {
                //取出传过来的值
                _sourcenamespace = navigationContext.Parameters.GetValue<string>("NameSpace");
            }

            if (navigationContext.Parameters.ContainsKey("Title"))
            {

                Title = navigationContext.Parameters.GetValue<string>("Title");
            }

            if (navigationContext.Parameters.ContainsKey("StopAutoRefresh"))
            {
                if (navigationContext.Parameters.GetValue<bool>("StopAutoRefresh"))
                {
                    stopAutoRefresh = true;
                    refreshTimer.Stop();
                }
            }
            if (navigationContext.Parameters.ContainsKey("HideAutoRefresh"))
            {
                if (navigationContext.Parameters.GetValue<bool>("HideAutoRefresh"))
                {
                    AutoRefreshVisibility = Visibility.Collapsed;
                }
            }

        }




        private IInputInfo inputConfi;
        private ObservableCollection<double> autoRefreshIntervals = new ObservableCollection<double> { 1, 2, 5, 10, 15, 20, 30 };
        private ObservableCollection<double> scaleZooms = new ObservableCollection<double> { 0.1, 0.2, 0.5, 1, 2, 5, 10 };
        private bool stopAutoRefresh = false;
        private Visibility autoRefreshVisibility = Visibility.Visible;
        private string title;



        /// <summary>
        /// 输入配置文件的一些参数
        /// </summary>
        public IInputInfo InputConfi { get => inputConfi; set { inputConfi = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 波形刷新周期
        /// </summary>
        public ObservableCollection<double> AutoRefreshIntervals { get => autoRefreshIntervals; set { autoRefreshIntervals = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 显示 区间时长
        /// </summary>
        public ObservableCollection<double> ScaleZooms
        {
            get { return scaleZooms; }
            set { scaleZooms = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 自动刷新可见属性
        /// </summary>
        public Visibility AutoRefreshVisibility { get => autoRefreshVisibility; set { autoRefreshVisibility = value; RaisePropertyChanged(); } }

        public string Title { get => title; set { title = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 时间轴
        /// </summary>
        public ScatterPlotList<double> timeAxis { get; set; }
        /// <summary>
        /// 时间轴缓存
        /// </summary>
        public List<double> timeAxisTempBuffer { get; set; } = new List<double> { };
        /// <summary>
        /// 波形数据
        /// </summary>
        public List<SignalPlotList> signalPlotLists { get; set; } = new List<SignalPlotList>();
        /// <summary>
        /// 最大的数据点，超出部分从index 向左溢出
        /// </summary>
        public int? MaxSignalCount { get; set; } = null;
        public WpfPlot scottplot { get; set; }
        /// <summary>
        /// 经过转换后的串口实时数据（或读取文件时得到的数据），还未添加到波形显示里
        /// </summary>
        public List<DetData> DataBuffer = new List<DetData>();

        ScottPlot.Renderable.Axis xAxis3;


        /// <summary>
        /// Create a SignalList, add it to the plot, and return it.
        /// A SignalList is a ScatterPlot that is designed to grow using Add() and AddRange() methods.
        /// </summary>
        public SignalPlotList AddSignalList(double sampleRate = 1, Color? color = null, string label = null, int capacity = 100_000)//100_000
        {
            SignalPlotList plottable = new SignalPlotList(capacity)
            {
                SampleRate = sampleRate,
                Color = color ?? scottplot.Plot.GetNextColor(),
                Label = label,

            };
            scottplot.Plot.Add(plottable);
            return plottable;
        }


        /// <summary>
        /// 添加数据 等待转换成波形图
        /// </summary>
        /// <param name="data"></param>
        public async void AddData(List<DetData> data)
        {
            await Task.Run(() =>
            {
                var convertedData = dataHandler.StainToValidStress(data);
                lock (DataBuffer)
                {
                    DataBuffer.AddRange(convertedData);
                    var AddCount = DataBuffer.Count;
                    if (AddCount > (MaxSignalCount ?? AddCount))
                    {
                        DataBuffer.RemoveRange(0, (int)(AddCount - MaxSignalCount));
                    }
                }
                if (stopAutoRefresh)
                {
                    DataToPlot();
                }
            });
        }


        /// <summary>
        /// 一些命令
        /// </summary>
        /// <param name="obj"></param>
        private void Execute(string obj)
        {
            switch (obj)
            {

                case "定时刷新": AutoRefresh(); break;
                case "刷新区间改变": refreshTimer.Interval = InputConfi.AutoRefreshInterval * 1000;   /*checkTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)InputConfi.AutoRefreshInterval * 1000);*/ break;


                case "开启区间显示":
                case "区间显示改变": ZoomAndVisibility(); RenderGraph(); break;
              

                case "截图": scottplot.ScreenShot(); break;
                case "清空波形": ClearPlot(); break;
                case "添加X轴辅助线": scottplot.AddAxisLine(HorizontalLine: true, VerticalLine: false); RenderGraph(); break;
                case "添加Y轴辅助线": scottplot.AddAxisLine(HorizontalLine: false, VerticalLine: true); RenderGraph(); break;
                case "删除辅助线": scottplot.ReMoveAxisLine(); RenderGraph(); break;


                default: break;
            }
        }

        /// <summary>
        /// 清除波形
        /// </summary>
        public void ClearPlot()
        {
            for (int i = 0; i < signalPlotLists.Count; i++)
            {
                signalPlotLists[i].Clear(resetCapacity: true);
            }
            timeAxis.Clear();
            timeAxisTempBuffer.Clear();
            xAxis3.Hide();
            RenderGraph();

        }


        /// <summary>
        /// 渲染图表
        /// </summary>
        /// <param name="renderType"></param>
        public void RenderGraph(RenderType renderType = RenderType.LowQualityThenHighQualityDelayed)
        {
            Application.Current.Dispatcher.Invoke(() => scottplot.RefreshRequest(renderType));
        }


        /// <summary>
        /// 缩放与显示
        /// </summary>
        public void ZoomAndVisibility()
        {

            var MaxX = signalPlotLists.Where(a => a.IsVisible).Select(a => a.LastX).Max();
            if (InputConfi.IsScale && (InputConfi.ScaleZoom < MaxX))
            {

                var MinY = signalPlotLists
                    .Where(a => a.IsVisible)
                    .Select(a => a.GetRange((double)(MaxX - InputConfi.ScaleZoom), (double)MaxX))
                    .Select(a => a.Min()).Min();
                var MaxY = signalPlotLists
                    .Where(a => a.IsVisible)
                    .Select(a => a.GetRange((double)(MaxX - InputConfi.ScaleZoom), (double)MaxX)).Select(a => a.Max()).Max();
                scottplot.Plot.SetAxisLimits(xMin: MaxX - InputConfi.ScaleZoom, xMax: MaxX, yMin: MinY, yMax: MaxY);
                scottplot.Plot.AxisZoom(yFrac: 0.9);
            }
            else
            {
                scottplot.Plot.AxisAuto();
            }

        }


        public void DataToPlot()
        {
            lock (DataBuffer)
            {
                if (DataBuffer.Count == 0)
                {
                    return;
                }

                var dataBufferCount = DataBuffer.Count;


                lock (signalPlotLists)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        var signalCount = signalPlotLists[i].Count;

                        if (signalCount + dataBufferCount > (MaxSignalCount ?? signalCount))
                        {
                            if (signalCount > signalCount + dataBufferCount - MaxSignalCount + 600_000)
                            {
                                signalPlotLists[i].RemoveRange(0, (int)(signalCount + dataBufferCount - MaxSignalCount + 600_000));//再删除10分钟数据，避免频繁超限。
                            }
                            else
                            {
                                signalPlotLists[i].Clear();
                            }

                        }
                    }

                    var timeAxisCount = timeAxisTempBuffer.Count;
                    if (timeAxisCount * 1000 + dataBufferCount > (MaxSignalCount ?? timeAxisCount * 1000))
                    {
                        if (timeAxisCount > timeAxisCount + dataBufferCount - MaxSignalCount + 600_000)
                        {
                            //再删除10分钟数据，避免频繁超限。
                            timeAxisTempBuffer.RemoveRange(0, (int)(timeAxisCount + (dataBufferCount - MaxSignalCount + 600_000) / 1000));
                            timeAxis.Clear();
                            foreach (var item in timeAxisTempBuffer)
                            {
                                timeAxis.Add(item, 0);
                            }
                        }
                        else
                        {
                            timeAxis.Clear();
                            timeAxisTempBuffer.Clear();
                        }
                    }

                    var timexs = DataBuffer.Select(a => a.Time.ToOADate()).ToArray();
                    for (int i = 0; i < timexs.Length; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            timeAxis.Add(timexs[i], 0);
                            timeAxisTempBuffer.Add(timexs[i]);
                        }
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        signalPlotLists[4 * i + 0].AddRange(DataBuffer.Select(a => a.StrainList[3 * i + 0]));
                        signalPlotLists[4 * i + 1].AddRange(DataBuffer.Select(a => a.StrainList[3 * i + 1]));
                        signalPlotLists[4 * i + 2].AddRange(DataBuffer.Select(a => a.StrainList[3 * i + 2]));
                        signalPlotLists[4 * i + 3].AddRange(DataBuffer.Select(a => a.StressList[i]));
                    }
                }


                DataBuffer.Clear();
            }


            xAxis3.Hide(false);
            ZoomAndVisibility();
            scottplot.Plot.Render();
            RenderGraph();
        }

        /// <summary>
        /// 显示通道改变
        /// </summary>
        /// <param name="channelChange"></param>
        public void ChannelChange(ChannelChangeModel channelChange)
        {
            if (channelChange.Filter == _sourcenamespace)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (channelChange.IsVisible[i * 4 + j])
                        {
                            signalPlotLists[4 * i + j].IsVisible = true;
                        }
                        else
                        {
                            signalPlotLists[4 * i + j].IsVisible = false;
                        }
                    }
                }
            }
            RenderGraph();
        }

        #region 定时刷新

        public void AutoRefresh()
        {
            if (InputConfi.ChartAutoRefresh)
            {
                //checkTimer.Start();
                refreshTimer.Start();
            }
            else
            {
                //checkTimer.Stop();
                refreshTimer.Stop();
            }

        }


        System.Timers.Timer refreshTimer;


        private void RefreshTimerInit()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 1000;
            refreshTimer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            refreshTimer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            //refreshTimer.Enabled = true;
            //绑定Elapsed事件
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            //if (InputConfi.ChartAutoRefresh)
            //{
            refreshTimer.Start();
            //}
        }

        private async void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            refreshTimer.Stop();
            await Task.Run(new Action(() => DataToPlot()));
            refreshTimer.Start();

        }


        #endregion

        #region 发送给FFT数据

        System.Timers.Timer sendToFFTDelayTimer;

        private void SendToFFTDelayTimerInit()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 500;
            sendToFFTDelayTimer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            sendToFFTDelayTimer.AutoReset = false;
            //绑定Elapsed事件
            sendToFFTDelayTimer.Elapsed += SendToFFTDelayTimer_Elapsed;
        }

        private void SendToFFTDelayTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (signalPlotLists[0].Count == 0)
            {
                return;
            }
            List<List<double>> FFTdata = new List<List<double>>();
            lock (signalPlotLists)
            {

                var currentAixsLimit = scottplot.Plot.GetAxisLimits();
                if (currentAixsLimit.XMax <= 0)
                {
                    return;
                }
                
                for (int i = 0; i < signalPlotLists.Count; i++)
                {
                    if (currentAixsLimit.XMin >= signalPlotLists[i].Count / 1000.0)
                    {
                        return;
                    }
                    if (currentAixsLimit.XSpan * 1000 > 60_000)
                    {
                        FFTdata.Add(signalPlotLists[i].GetRange(xMin: Math.Max(currentAixsLimit.XMax - 60_000 / 1000,0), xMax: Math.Min(currentAixsLimit.XMax, signalPlotLists[i].Count / 1000.0)).ToList());
                    }
                    else
                    {
                        FFTdata.Add(signalPlotLists[i].GetRange(xMin: Math.Max(currentAixsLimit.XMin, 0), xMax: Math.Min(currentAixsLimit.XMax, signalPlotLists[i].Count / 1000.0)).ToList());
                    }
                }
            }

            aggregator.GetEvent<FFTUpDataEvent>().Publish(new FFTRawData { Filter = _sourcenamespace, Data = FFTdata });
        }

        private void ScaleChanged(object? sender, EventArgs e)
        {
            sendToFFTDelayTimer.Stop();

            sendToFFTDelayTimer.Start();
        }

        #endregion


    }

    public interface ITest
    {
        public string _sourcenamespace { get; set; }
    }




}
