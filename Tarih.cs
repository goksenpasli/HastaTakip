using System;
using System.Globalization;
using System.Windows.Controls;

namespace HastaTakip
{
    public static class Tarih
    {
        public static void AyDüzGiriş(object sender)
        {
            var dp = sender as DatePicker;

            if (DateTime.TryParseExact(dp?.Text, "d", new DateTimeFormatInfo { ShortDatePattern = "ddMMyyyy" },
                DateTimeStyles.None, out var dt))
                dp.SelectedDate = dt;
        }
    }
}