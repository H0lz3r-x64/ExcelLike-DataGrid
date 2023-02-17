using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ExcelLikeDataGrid.Utilities
{
    public class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToList();
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class FilterModeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;

            if ((value as string).Equals("IsNull", StringComparison.OrdinalIgnoreCase) || (value as string).Equals("Is Null", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else if ((value as string).Equals("NotIsNull", StringComparison.OrdinalIgnoreCase) || (value as string).Equals("Not Is Null", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else { return true; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
