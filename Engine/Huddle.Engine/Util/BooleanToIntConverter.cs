using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Huddle.Engine.Util
{
    [ValueConversion(typeof(int), typeof(Boolean))]
    public class BooleanToIntConverter : ConverterMarkupExtension<BooleanToIntConverter>, IValueConverter
    {
        #region ctor

        public BooleanToIntConverter()
        {
        }

        #endregion

        public object Convert(object v, Type t, object p, System.Globalization.CultureInfo l)
        {
            return v.Equals(int.Parse((string)p));
        }
        public object ConvertBack(object v, Type t, object p, System.Globalization.CultureInfo l)
        {
            if ((bool)v)
                return int.Parse((string)p);
            else
                return Binding.DoNothing;
        }
    }
}
