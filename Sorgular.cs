using System.Collections.ObjectModel;
using System.Linq;

namespace HastaTakip
{
    public partial class Kisiler
    {
        public static ObservableCollection<Iller> Iller { get; set; } =
            new ObservableCollection<Iller>(from il in new Database().Veriler.Iller select il);

        public static ObservableCollection<Kisiler> Kişiler { get; set; } =
            new ObservableCollection<Kisiler>(from kişi in new Database().Veriler.Kisiler select kişi);

        public static ObservableCollection<Tahsilat> Tahsilat { get; set; } =
            new ObservableCollection<Tahsilat>(from tahsilat in new Database().Veriler.Tahsilat select tahsilat);

      
    }
}