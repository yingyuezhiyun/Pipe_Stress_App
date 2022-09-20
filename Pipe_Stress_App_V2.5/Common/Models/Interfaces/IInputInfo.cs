using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models.Interfaces
{
    public interface IInputInfo
    {
        /// <summary>
        /// 是否为实时数据
        /// </summary>
        public bool IsRealTime { get; set; }

        /// <summary>
        /// 是否为区间显示
        /// </summary>
        public bool IsScale { get; set; }

        /// <summary>
        /// 区间显示长度 （秒）
        /// </summary>
        public double ScaleZoom { get; set; }

        /// <summary>
        /// 是否显示FFT
        /// </summary>
        public bool IsShowFFT { get; set; }

        /// <summary>
        /// FFT的时长 （秒）
        /// </summary>
        public double FFTLength { get; set; }

        /// <summary>
        /// 是否自动刷新数据图表
        /// </summary>
        public bool ChartAutoRefresh { get; set; }

        /// <summary>
        /// 自动刷新的时间间隔 (秒)
        /// </summary>
        public double AutoRefreshInterval { get; set; }

        /// <summary>
        /// 数据采集频率
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 是否为 复用
        /// </summary>
        public bool IsMultiplex { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件夹路径
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// 输入根目录
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// 是否处于连续测试中
        /// </summary>
        public bool IsDetectCycled { get; set; }

        /// <summary>
        /// 输入数量
        /// </summary>
        public int Num { get; set; }


        /// <summary>
        /// 当前的读取的序号
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// 读取的进度
        /// </summary>
        public string Progress { get; set; }

        /// <summary>
        /// 输入文件筛选方式
        /// </summary>
        public int SelectType { get; set; }


        public DateTime PickerStartTime { get; set; }

        public DateTime PickerEndTime { get; set; }
    }
}
