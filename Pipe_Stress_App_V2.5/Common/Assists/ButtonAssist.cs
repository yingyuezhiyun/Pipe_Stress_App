using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pipe_Stress_App_V2._5.Common.Assists
{
    public static class ButtonAssist
    {
        public static readonly DependencyProperty UniformCornerRadiusProperty = DependencyProperty.RegisterAttached(
            "UniformCornerRadius", typeof(double), typeof(ButtonAssist), new PropertyMetadata(2.0, OnUniformCornerRadius));

        private static void OnUniformCornerRadius(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MaterialDesignThemes.Wpf.ButtonAssist.SetCornerRadius(d, new CornerRadius((double)e.NewValue));
        }

        public static void SetUniformCornerRadius(DependencyObject element, double value)
        {
            element.SetValue(UniformCornerRadiusProperty, value);
        }

        public static double GetUniformCornerRadius(DependencyObject element)
        {
            return (double)element.GetValue(UniformCornerRadiusProperty);
        }
    }
}
