using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models
{
    public class FileSaveInfo : BindableBase, IFileSaveInfo
    {
        private double period;
        private double interval;
        private double newFolderInterval;
        private string rootDirectory;
        private bool autoSave;

        public double Period { get => period; set { period = value; RaisePropertyChanged(); } }
        public double Interval { get => interval; set { interval = value; RaisePropertyChanged(); } }
        public double NewFolderInterval { get => newFolderInterval; set { newFolderInterval = value; RaisePropertyChanged(); } }
        public string RootDirectory { get => rootDirectory; set { rootDirectory = value; RaisePropertyChanged(); } }

        public bool AutoSave { get => autoSave; set { autoSave = value; RaisePropertyChanged(); } }

        public DirectoryInfo FolderDirectory { get; set; }
    }
}
