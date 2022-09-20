using Pipe_Stress_App_V2._5.Common.Events;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class ChannelSelectViewModel : NavigationViewModel
    {

        private  string _sourcenamespace;
        public DelegateCommand<object> ChannelChangeCommand { get; private set; }
        public ChannelSelectViewModel(IContainerProvider provider) : base(provider)
        {
            Channels.Add(new Channel { Name = "1#", ID = 0 });
            Channels.Add(new Channel { Name = "2#", ID = 1 });
            Channels.Add(new Channel { Name = "3#", ID = 2 });
            Channels.Add(new Channel { Name = "4#", ID = 3 });
            Channels.Add(new Channel { Name = "5#", ID = 4 });
            Channels.Add(new Channel { Name = "6#", ID = 5 });
            Channels.Add(new Channel { Name = "7#", ID = 6 });
            Channels.Add(new Channel { Name = "8#", ID = 7 });

            ChannelChangeCommand = new DelegateCommand<object>((obj) => { ChannelChange(obj); });
            Channels[0].Contents[0].CheckState=true;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if (navigationContext.Parameters.ContainsKey("NameSpace"))
            {
                //取出传过来的值
                _sourcenamespace = navigationContext.Parameters.GetValue<string>("NameSpace");
            }
        }

     

        private ObservableCollection<Channel> channels = new ObservableCollection<Channel>();
        public ObservableCollection<Channel> Channels
        {
            get { return channels; }
            set { channels = value; RaisePropertyChanged(); }
        }



        public void ChannelChange(object obj)
        {
            List<bool> isVisible = new List<bool>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((bool)Channels[i].Contents[j].CheckState)
                    {
                        isVisible.Add(true);
                    }
                    else
                    {
                        isVisible.Add (false);
                    }
                }
            }

            aggregator.GetEvent<ChannelChangeEvent>().Publish( new ChannelChangeModel { Filter= _sourcenamespace ,IsVisible=isVisible});
            //RenderGraph();
        }



    }

    /// <summary>
    /// 一个应变片对应一个Channel
    /// 一个应变片对应三个角度及一个复合应力，即3+1个Content
    /// </summary>
    public class Channel : BindableBase
    {

        public string Name { get; set; }

        public int ID { get; set; }
        public class Content
        {
            /// <summary>
            /// true为选中
            /// </summary>
            public bool CheckState { get; set; }
            /// <summary>
            /// 角度通道/应力 描述
            /// </summary>
            public string Describe { get; set; }
        }
        public List<Content> Contents { get; set; } = new List<Content>();
        public Channel()
        {

            Contents.Add(new Content { CheckState = false, Describe = "0°" });
            Contents.Add(new Content { CheckState = false, Describe = "45°" });
            Contents.Add(new Content { CheckState = false, Describe = "90°" });
            Contents.Add(new Content { CheckState = false, Describe = "Mix" });
        }
    }
}
