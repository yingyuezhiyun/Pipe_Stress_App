using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models
{

    /// <summary>
    /// 某个时刻，所有应变片的数据
    /// 即 采集仪在某个时刻发送的所有通道应变数据
    /// </summary>
    public class RevDataArray
    {
        public List<byte> Data { get; set; } = new List<byte>();
        public DateTime Time { get; set; }
    }
}
