using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Commands;
using Prism.Events;
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
    public class MainViewModel : BindableBase
    {
        private readonly IEventAggregator aggregator;
        private readonly IRegionManager regionManager;
        public MainViewModel(IEventAggregator aggregator,IRegionManager regionManager)
        {
            this.aggregator = aggregator;
            this.regionManager = regionManager;
            //aggregator.SendMessage("1111");
            CreateMenuBar();
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            GoBackCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoBack)
                    journal.GoBack();
            });
            GoForwardCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoForward)
                    journal.GoForward();
            });

        }
        private IRegionNavigationJournal journal;

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        public ObservableCollection<MenuBar> MenuBars
        {
            get => menuBars; set { menuBars = value; RaisePropertyChanged(); }
        }
        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }
        public DelegateCommand GoBackCommand { get; private set; }
        public DelegateCommand GoForwardCommand { get; private set; }

        public string Edition { get; set; } = "V2.5.22.09.19";

        void CreateMenuBar()
        {
            MenuBars.Add(new MenuBar() { Icon = "MonitorMultiple", Title = "实时监测", NameSpace = "MonitorView" });
            MenuBars.Add(new MenuBar() { Icon = "DatabaseClockOutline", Title = "离线数据", NameSpace = "DataAnalysisView" });
            MenuBars.Add(new MenuBar() { Icon = "CogOutline", Title = "更多设定", NameSpace = "SettingsView" });
        }

        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;

            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
            {
                journal = back.Context.NavigationService.Journal;
            });
        }
    }




}
