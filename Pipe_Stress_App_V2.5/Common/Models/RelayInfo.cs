using Pipe_Stress_App_V2._5.Common.Models.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models
{
    public class RelayInfo : BindableBase, IRelayInfo
    {
        private List<Relay> relay1;
        private List<Relay> relay2;


        //public event PropertyChangedEventHandler PropertyChanged;

        public RelayInfo()
        {
            Relay1 = new List<Relay>();
            Relay2 = new List<Relay>();

            for (int i = 0; i < 8; i++)
            {
                Relay1.Add(new Relay { OpenState = true, ID = i });
                Relay2.Add(new Relay { OpenState = true, ID = i });

            }

        }


        public List<Relay> Relay1
        {
            get => relay1; set
            {
                relay1 = value;
                RaisePropertyChanged();
            }
        }
        public List<Relay> Relay2
        {
            get => relay2; set
            {
                relay2 = value;
                RaisePropertyChanged();
            }
        }

    }

    public class Relay : BindableBase
    {
        private bool openState;
        private string name;
        public string Name { get => name; set => name = value; }
        /// <summary>
        /// 状态 
        /// </summary>
        public bool OpenState { get => openState; set { openState = value; RaisePropertyChanged(); } }


        /// <summary>
        /// 
        /// </summary>
        public int ID { get; set; }
    }
}
