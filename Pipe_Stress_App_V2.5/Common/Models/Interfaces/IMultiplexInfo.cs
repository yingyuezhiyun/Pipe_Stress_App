using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models.Interfaces
{
    public interface IMultiplexInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// 复用通道参数设置
        /// </summary>
        List<Multiplex> Multiplexes { get; set; }

    }
}
