using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Models.Interfaces
{
    public interface IRelayInfo
    {
        /// <summary>
        /// 继电器1 的打开状态
        /// </summary>
        public List<Relay> Relay1 { get; set; }

        /// <summary>
        /// 继电器2 的打开状态
        /// </summary>
        public List<Relay> Relay2 { get; set; }

    }
}
