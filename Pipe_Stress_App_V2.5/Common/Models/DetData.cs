using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models
{
    public class DetData
    {
        /// <summary>
        /// 应变值 (某个时刻，所有应变片的应变值)
        /// </summary>
        public List<double> StrainList { get; set; } = new List<double>();

        /// <summary>
        /// 应力值 (某个时刻，所有应变片的应力值)
        /// </summary>
        public List<double> StressList { get; set; } = new List<double>();

        public DateTime Time { get; set; }
    }
}
