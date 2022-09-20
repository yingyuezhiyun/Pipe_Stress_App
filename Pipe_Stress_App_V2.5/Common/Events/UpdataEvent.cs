using Pipe_Stress_App_V2._5.Common.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Events
{
    public class UpdataEvent : PubSubEvent<UpdataModel>
    {

    }
    public class UpdataModel
    {
        public string Filter { get; set; }
        public List<DetData> Data { get; set; } = new List<DetData>();
    }
}
