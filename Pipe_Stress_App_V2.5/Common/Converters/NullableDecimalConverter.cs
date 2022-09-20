using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Pipe_Stress_App_V2._5.Common.Converters
{
    public class NullableDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (string.IsNullOrEmpty(strValue))
            {
                return null;
            }
            decimal result;
            if (strValue.IndexOf('.') == strValue.Length - 1 || !decimal.TryParse(strValue, out result))
            {
                return DependencyProperty.UnsetValue;
            }
            return result;

            //return value.ToString().EndsWith(".") ? "." : value;
        }
    }
}
