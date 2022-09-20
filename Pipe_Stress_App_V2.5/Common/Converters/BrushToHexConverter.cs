﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Pipe_Stress_App_V2._5.Common.Converters
{
    public class BrushToHexConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return null;
            string lowerHexString(int i) => i.ToString("X2").ToLower();
            var brush = (SolidColorBrush)value;
            var hex = lowerHexString(brush.Color.R) +
                      lowerHexString(brush.Color.G) +
                      lowerHexString(brush.Color.B);
            return "#" + hex;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}