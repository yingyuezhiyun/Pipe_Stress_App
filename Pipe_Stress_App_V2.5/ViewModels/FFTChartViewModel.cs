using MathNet.Numerics.IntegralTransforms;
using Pipe_Stress_App_V2._5.Common.Events;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class FFTChartViewModel : NavigationViewModel, ITest
    {

        public string _sourcenamespace { get; set; }


        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public FFTChartViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            scottplot = new WpfPlot();
            ///设置透明背景
            scottplot.Plot.Style(figureBackground: System.Drawing.Color.Transparent);
            scottplot.Plot.Style(dataBackground: System.Drawing.Color.Transparent);
            ///禁止DPI缩放 以获得更高的清晰度
            scottplot.Configuration.DpiStretch = false;
            ///设置边框为折叠
            scottplot.Plot.YAxis2.IsVisible = false;
            scottplot.Plot.XAxis2.IsVisible = false;
            ///Y轴标签旋转90度
            scottplot.Plot.YAxis.TickLabelStyle(rotation: 90);

            scottplot.Refresh();
            FFTSource = new ObservableCollection<string>
            {
                "1#-0°","1#-45°","1#-90°","1#-Mix",
                "2#-0°","2#-45°","2#-90°","2#-Mix",
                "3#-0°","3#-45°","3#-90°","3#-Mix",
                "4#-0°","4#-45°","4#-90°","4#-Mix",
                "5#-0°","5#-45°","5#-90°","5#-Mix",
                "6#-0°","6#-45°","6#-90°","6#-Mix",
                "7#-0°","7#-45°","7#-90°","7#-Mix",
                "8#-0°","8#-45°","8#-90°","8#-Mix"
            };
            FFTLengths = new ObservableCollection<double>
            {
                1,2,3,5,10,20,30,60
            };



            aggregator.GetEvent<FFTUpDataEvent>().Subscribe(a =>
            {
                    if (a.Filter == _sourcenamespace)
                    {
                        FFT(a.Data[FFTSourceSelectedIndex],100);
                    }
            });

            ExecuteCommand = new DelegateCommand<string>(Execute);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if (navigationContext.Parameters.ContainsKey("NameSpace"))
            {
                //取出传过来的值
                _sourcenamespace = navigationContext.Parameters.GetValue<string>("NameSpace");
            }
        }


        #region 参数变量


        private ObservableCollection<string> fFTSource;
        private int fFTSourceSelected;
        private ObservableCollection<double> fFTLengths;
        private double fFTLength = 1;
        private bool tooltipIsVisible = true;

        /// <summary>
        /// 所有傅里叶变换的源
        /// </summary>
        public ObservableCollection<string> FFTSource { get => fFTSource; set { fFTSource = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 当前选中的傅里叶变换的源
        /// </summary>
        public int FFTSourceSelectedIndex { get => fFTSourceSelected; set { fFTSourceSelected = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 傅里叶变换时长 （秒）
        /// </summary>
        public ObservableCollection<double> FFTLengths { get => fFTLengths; set { fFTLengths = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 当前傅里叶变换时长 （秒）
        /// </summary>
        public double FFTLength { get => fFTLength; set { fFTLength = value; RaisePropertyChanged(); } }

        public WpfPlot scottplot { get; set; }

        #endregion



        /// <summary>
        /// 快速傅里叶变换
        /// </summary>
        /// <param name="rawdata"></param>
        /// <param name="validcount">取前validcount个有效的数据</param>
        private async void FFT(List<double> rawdata, int? validcount = null)
        {

            List<double> data = new List<double>();

            if (rawdata.Count > FFTLength * 1000)
            {
                data.AddRange(rawdata.GetRange(rawdata.Count - (int)(FFTLength * 1000), (int)(FFTLength * 1000)));
            }
            else
            {
                data.AddRange(rawdata.GetRange(0, rawdata.Count));
            }

            var count = data.Count;
            Complex[] samples = new Complex[count];
            for (int i = 0; i < count; i++)
            {
                samples[i] = (Complex)(data[i]);
            }
            await Task.Run(() =>
            {
                Fourier.Forward(samples, FourierOptions.NoScaling);
            });

            List<AF> aFs = new List<AF>();

            ///0频幅值，表示0点偏移(直流偏量)
            var azeroAF = new AF
            {
                Magnitude = (1.0 / count) * (Math.Abs(Math.Sqrt(Math.Pow(samples[0].Real, 2) + Math.Pow(samples[0].Imaginary, 2)))),
                Frequency = 0
            };

            ///幅频值 （由于对称性，只取一半）
            ///Deteremine how many HZ represetned by each sample
            ///double hzPerSample = sampleRate / numSamples;
            for (int i = 1; i < count / 2; i++)
            {
                aFs.Add(new AF
                {
                    Magnitude = (2.0 / count) * (Math.Abs(Math.Sqrt(Math.Pow(samples[i].Real, 2) + Math.Pow(samples[i].Imaginary, 2)))),
                    Frequency = (1000.0 * i) / count
                });
            }


            ///排序 大 -> 小
            //aFs.Sort();
            aFs.Sort((p1, p2) =>
            {
                return p2.Magnitude.CompareTo(p1.Magnitude);
            });

            ///选出前validcount个有效数据
            var AFcount = aFs.Count;
            var validData = new List<AF>();
            if (validcount != null)
            {
                validData = aFs.GetRange(0, Math.Min((int)validcount, AFcount));
            }
            ///添加0处直流
            validData.Insert(0, azeroAF);

            
            Plot(validData);
        }

        /// <summary>
        /// 图形绘制
        /// </summary>
        private void Plot(List<AF> validData)
        {

            ///如果在进行鼠标缩放操作 那么跳过此次渲染
            bool skipRender = App.Current.Dispatcher.Invoke(() =>
            {
                if ((Mouse.DirectlyOver is WpfPlot) &&
                (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed || Mouse.MiddleButton == MouseButtonState.Pressed))
                {
                    return true;
                }
                return false;
            });
            if (skipRender)
            {
                return;
            }
            scottplot.Plot.Clear();//TODO: place other 
            ///把幅频数据添加到图表
            scottplot.MouseDraggable(left: false, right: false, middle: false,scrollwheel:false);
            scottplot.Plot.AddLollipop(validData.Select(a => a.Magnitude).ToArray(),
                 validData.Select(a => a.Frequency).ToArray());

            if (tooltipIsVisible)
            {
                ///添加标签
                ///零点
                scottplot.Plot.AddTooltip(validData[0].ToString(), validData[0].Frequency, validData[0].Magnitude);
                ///幅值点
                scottplot.Plot.AddTooltip(validData[1].ToString(), validData[1].Frequency, validData[1].Magnitude);
            }

            scottplot.Plot.Render();
            RenderGraph();
            scottplot.MouseDraggable();


        }

        /// <summary>
        /// 渲染图表
        /// </summary>
        /// <param name="renderType"></param>
        private void RenderGraph(RenderType renderType = RenderType.LowQualityThenHighQualityDelayed)
        {
            Application.Current.Dispatcher.Invoke(() => scottplot.RefreshRequest(renderType));
        }

        /// <summary>
        /// 一些命令
        /// </summary>
        /// <param name="obj"></param>
        private void Execute(string obj)
        {
            switch (obj)
            {

              
                case "FFT区间改变": break;
                case "FFT时长改变": break;
                case "截图": scottplot.ScreenShot(); break;
                case "清空波形": scottplot.Plot.Clear(); RenderGraph(); break;
                case "显示/隐藏标签": tooltipIsVisible = !tooltipIsVisible; break;
                case "添加X轴辅助线": scottplot.AddAxisLine(HorizontalLine: true, VerticalLine: false); RenderGraph(); break;
                case "添加Y轴辅助线": scottplot.AddAxisLine(HorizontalLine: false, VerticalLine: true); RenderGraph(); break;
                case "删除辅助线": scottplot.ReMoveAxisLine(); RenderGraph(); break;


                default: break;
            }
        }

    }


    /// <summary>
    /// 幅频
    /// </summary>
    public class AF : IComparable<AF>
    {
        /// <summary>
        /// 频率
        /// </summary>
        public double Frequency { get; set; }
        /// <summary>
        /// 幅值
        /// </summary>
        public double Magnitude { get; set; }

        /// <summary>
        /// 从大到小
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(AF other)
        {
            return other.Magnitude.CompareTo(this.Magnitude);
        }
        public override string ToString()
        {
            if (Frequency == 0)
            {
                return "AVG:" + Magnitude.ToString("F2");
            }
            else
                return Frequency.ToString("F2") + "Hz," + Magnitude.ToString("F2");
        }

    }
}
