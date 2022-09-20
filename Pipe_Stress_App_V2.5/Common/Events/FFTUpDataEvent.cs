using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Events
{
    public class FFTUpDataEvent: PubSubEvent<FFTRawData>
    {

    }

    /// <summary>
    /// FFT原始数据
    /// </summary>
    public class FFTRawData
    {
        public string Filter { get; set; }

        public List<List<double>> Data { get; set; }= new List<List<double>>();

       
    }
}
