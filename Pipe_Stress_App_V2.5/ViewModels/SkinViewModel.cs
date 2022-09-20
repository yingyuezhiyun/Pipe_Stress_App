using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using Pipe_Stress_App_V2._5.Common.Dialog;
using Pipe_Stress_App_V2._5.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Pipe_Stress_App_V2._5.ViewModels
{

    public class SkinViewModel : BindableBase, IDialogHostAware, INavigationAware
    {
        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    ModifyTheme(theme => theme.SetBaseTheme(value ? Theme.Dark : Theme.Light));
                    OperateIniFile.WriteIniData("Skin", "Dark", value.ToString());
                }
            }
        }

       
        public IEnumerable<ISwatch> Swatches { get; } = SwatchHelper.Swatches;

        public DelegateCommand<object> ChangeHueCommand { get; private set; }
        public string DialogHostName { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public SkinViewModel()
        {
            ChangeHueCommand = new DelegateCommand<object>(ChangeHue);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private Color? _selectedColor = new Color();
        public Color? SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    RaisePropertyChanged();

                    if (value is Color color)
                    {
                        ChangeClolor(color);
                    }
                }
            }
        }



        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
        }

        private void Save()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters param = new DialogParameters();
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
            }
        }

        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            modificationAction?.Invoke(theme);
            paletteHelper.SetTheme(theme);
        }

        private void ChangeHue(object obj)
        {
            var hue = (Color)obj;
            SelectedColor = hue;
            ChangeClolor(hue);
        }
     

        private void ChangeClolor(Color color)
        {
            ITheme theme = paletteHelper.GetTheme();
            theme.PrimaryLight = new ColorPair(color.Lighten());
            theme.PrimaryMid = new ColorPair(color);
            theme.PrimaryDark = new ColorPair(color.Darken());
            paletteHelper.SetTheme(theme);
            OperateIniFile.WriteIniData("Skin", "PrimaryMid", color.ToString());
        }

        public void OnDialogOpend(IDialogParameters parameters)
        {
            ITheme theme = paletteHelper.GetTheme();
            if (theme.GetBaseTheme() is BaseTheme.Dark)
            {
                _isDarkTheme = true;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ITheme theme = paletteHelper.GetTheme();
            if (theme.GetBaseTheme() is BaseTheme.Dark)
            {
                _isDarkTheme = true;
            }
            SelectedColor= theme.PrimaryMid.Color;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
            

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }
    }

}
