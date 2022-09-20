using Pipe_Stress_App_V2._5.Extensions;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.ViewModels
{
    public class SettingsViewModel:NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        public SettingsViewModel(IRegionManager regionManager, IContainerProvider provider):base(provider)
        {
            this.regionManager = regionManager;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            regionManager.Regions[PrismManager.SettingsViewRegionName].RequestNavigate("SkinView");
        }






    }
}
