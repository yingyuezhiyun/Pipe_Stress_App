using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.Events
{
    public class ChannelChangeEvent : PubSubEvent<ChannelChangeModel>
    {


    }

    public class ChannelChangeModel
    {
        public string Filter { get; set; }
        public List<bool> IsVisible { get; set; } = new List<bool>();
    }
}
