using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models
{
    public class MultiplexInfo : IMultiplexInfo
    {


        private List<Multiplex> multiplexes = new List<Multiplex>();

        public List<Multiplex> Multiplexes
        {
            get { return multiplexes; }
            set { multiplexes = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public MultiplexInfo()
        {
            Multiplexes.Add(new Multiplex { Name = "5#" });
            Multiplexes.Add(new Multiplex { Name = "6#" });
            Multiplexes.Add(new Multiplex { Name = "7#" });
            Multiplexes.Add(new Multiplex { Name = "8#" });

        }


        /// <summary>
        /// 实现通知更新
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    /// <summary>
    /// #5,#6,#7,#8初始值
    /// </summary>
    public class Multiplex : BindableBase
    {
        private string name;
        private double holdingTime;
        private bool state;
        private ObservableCollection<double> initialValue;



        /// <summary>
        /// 通道名
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// 复用的时长
        /// </summary>
        public double HoldingTime { get => holdingTime; set { holdingTime = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 是否处于复用中
        /// </summary>
        public bool State { get => state; set => state = value; }

        /// <summary>
        /// 通道3个角度的初始值
        /// </summary>
        public ObservableCollection<double> InitialValue { get => initialValue; set { initialValue = value; RaisePropertyChanged(); } }

        public Multiplex()
        {
            InitialValue = new ObservableCollection<double> { 0, 0, 0 };
        }
    }
}
