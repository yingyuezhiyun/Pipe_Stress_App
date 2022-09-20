using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models
{
    public class InputInfo : BindableBase, IInputInfo
    {
        private bool isRealTime;
        private bool isScale;
        private double scaleZoom;
        private bool isShowFFT;
        private double fFTLength;
        private bool chartAutoRefresh;
        private double autoRefreshInterval;
        private int frequency;
        private bool isMultiplex;
        private string filePath;
        private string folderPath;
        private string rootDirectory;
        private bool isDetectCycled;
        private int num;
        private int currentIndex;
        private string progress;
        private int selectType;


        public bool IsRealTime { get => isRealTime; set { isRealTime = value; RaisePropertyChanged(); } }
        public bool IsScale { get => isScale; set { isScale = value; RaisePropertyChanged(); } }
        public double ScaleZoom { get => scaleZoom; set { scaleZoom = value; RaisePropertyChanged(); } }
        public bool IsShowFFT { get => isShowFFT; set { isShowFFT = value; RaisePropertyChanged(); } }
        public double FFTLength { get => fFTLength; set { fFTLength = value; RaisePropertyChanged(); } }
        public bool ChartAutoRefresh { get => chartAutoRefresh; set { chartAutoRefresh = value; RaisePropertyChanged(); } }
        public double AutoRefreshInterval { get => autoRefreshInterval; set { autoRefreshInterval = value; RaisePropertyChanged(); } }
        public int Frequency { get => frequency; set { frequency = value; RaisePropertyChanged(); } }
        public bool IsMultiplex { get => isMultiplex; set { isMultiplex = value; RaisePropertyChanged(); } }
        public string FilePath { get => filePath; set { filePath = value; RaisePropertyChanged(); } }
        public string FolderPath { get => folderPath; set { folderPath = value; RaisePropertyChanged(); } }
        public string RootDirectory { get => rootDirectory; set { rootDirectory = value; RaisePropertyChanged(); } }

        public bool IsDetectCycled { get => isDetectCycled; set { isDetectCycled = value; RaisePropertyChanged(); } }

        public int Num { get => num; set { num = value; RaisePropertyChanged(); } }
        public int CurrentIndex { get => currentIndex; set { currentIndex = value; RaisePropertyChanged(); } }
        public string Progress { get => progress; set => progress = value; }
        public int SelectType { get => selectType; set => selectType = value; }
        public DateTime PickerStartTime { get; set; }
        public DateTime PickerEndTime { get; set; }
    }
}
