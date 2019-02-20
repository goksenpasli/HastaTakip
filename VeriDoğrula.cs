using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HastaTakip
{
    public static class Doğrula
    {
        public static bool Geçerli(DependencyObject parent)
        {
            if (Validation.GetHasError(parent)) return false;
            for (var i = 0; i != VisualTreeHelper.GetChildrenCount(parent); ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (!Geçerli(child)) return false;
            }

            return true;
        }
    }

    public class SıfırdanBüyük : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || value.ToString().Trim().Length == 0)
                return new ValidationResult(false, "Değer Boş Olamaz.");
            try
            {
                if (double.Parse(value.ToString()) <= 0)
                    return new ValidationResult(false, "0 dan büyük bir değer girin");
            }
            catch
            {
                return new ValidationResult(false, "Hatalı Giriş Var Düzeltin.");
            }

            return ValidationResult.ValidResult;
        }
    }

    public class SıfırDahilSıfırdandanBüyük : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || value.ToString().Trim().Length == 0)
                return new ValidationResult(false, "Değer Boş Olamaz.");
            try
            {
                if (double.Parse(value.ToString()) < 0)
                    return new ValidationResult(false, "0 dan büyük bir değer girin");
            }
            catch
            {
                return new ValidationResult(false, "Hatalı Giriş Var Düzeltin.");
            }

            return ValidationResult.ValidResult;
        }
    }

    //public class SıfırBirArası : ValidationRule
    //{
    //    public double Enaz { get; set; }

    //    public double Ençok { get; set; }

    //    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    //    {
    //        if (value == null || value.ToString().Trim().Length == 0)
    //            return new ValidationResult(false, "Değer Boş Olamaz.");
    //        try
    //        {
    //            if (double.Parse(value.ToString()) > Ençok || double.Parse(value.ToString()) <= Enaz)
    //                return new ValidationResult(false,
    //                    $"Girilen Değer {Enaz} ile {Ençok} Arasında Olmalıdır.");
    //        }
    //        catch
    //        {
    //            return new ValidationResult(false, "Hatalı Giriş Var Düzeltin.");
    //        }

    //        return ValidationResult.ValidResult;
    //    }
    //}

    public class TcKimlikDoğrula : ValidationRule
    {
        private static bool Rakakmmı(string tcKimlikNo) => tcKimlikNo.Aggregate(true, (current, t) => current & char.IsNumber(t));

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var tekler = 0;
            var ciftler = 0;
            if (value == null) return new ValidationResult(false, "Burayı Boş Geçmeyin.");
            var tcKimlikNo = value as string;
            if (Kisiler.Kişiler.Any(z => z.Tc == tcKimlikNo))
                return new ValidationResult(false,
                    "BU TC KİMLİK NUMARASI İLE KAYIT YAPILMIŞTIR. BAŞKA BİR TC KİMLİK NUMARASI YAZIN.");

            if (tcKimlikNo == null) return new ValidationResult(false, "Burayı Boş Geçmeyin.");
            if (tcKimlikNo.Trim()?.Length == 0) return new ValidationResult(false, "Burayı Boş Geçmeyin.");
            try
            {
                if (tcKimlikNo.Length != 11) return new ValidationResult(false, "TC Kimlik No 11 Hanelidir");
                if (!Rakakmmı(tcKimlikNo)) return new ValidationResult(false, "TC Kimlik No Sadece Rakamlardan Oluşur");
                if (tcKimlikNo.Substring(0, 1) == "0")
                    return new ValidationResult(false, "TC Kimlik İlk Hanesi 0 Olamaz");
                int i;
                for (i = 0; i < 9; i += 2) tekler += int.Parse(tcKimlikNo[i].ToString());
                for (i = 1; i < 8; i += 2) ciftler += int.Parse(tcKimlikNo[i].ToString());
                var k10 = (((tekler * 7) - ciftler) % 10).ToString();
                var k11 = ((tekler + ciftler + int.Parse(tcKimlikNo[9].ToString())) % 10).ToString();
                if (k10 == tcKimlikNo[9].ToString() && k11 == tcKimlikNo[10].ToString())
                    return ValidationResult.ValidResult;
                return new ValidationResult(false, "Girilen TC Kimlik Yanlış");
            }
            catch (Exception hata)
            {
                return new ValidationResult(false, hata.Message);
            }
        }
    }

    public class TarihDoğrula : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value == null) return ValidationResult.ValidResult;
                var date = DateTime.Parse(value.ToString());
                if (DateTime.Now < date || date < new DateTime(1753, 1, 1))
                    return new ValidationResult(false, "Bugünden Büyük veya 01.01.1753'den Küçük Tarih Girmeyin.");

                return ValidationResult.ValidResult;
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Geçerli Tarih Değil");
            }
        }
    }

    public class TxtDoğrula : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || value.ToString().Trim()?.Length == 0)
                return new ValidationResult(false, "Burayı Boş Geçmeyin.");
            return ValidationResult.ValidResult;
        }
    }
}