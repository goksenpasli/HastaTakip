using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static HastaTakip.FindControl;

namespace HastaTakip
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ughastabilgileri.DataContext = new Kisiler();
            dataGrid.DataContext = new Kisiler();
            dataGrid.ItemsSource = Kisiler.Kişiler;
            cbİlAdi.ItemsSource = Kisiler.Iller;
            Cv = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
        }

        private static ICollectionView Cv { get; set; }

        private void Btkişikaydet_Click(object sender, RoutedEventArgs e) => KişiGirişi.Ekle(this);

        private void DtLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => Tarih.AyDüzGiriş(sender);

        private void Btkişigüncelle_Click(object sender, RoutedEventArgs e) => KişiGirişi.Güncelle(sender, this);

        private void BtKişiSil_Click(object sender, RoutedEventArgs e) => KişiGirişi.Sil(sender);

        private void Btrapor_Click(object sender, RoutedEventArgs e) => new Raporla("HastaTakip.Rapor.rapor.frx");

        private void DateTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) => KişiGirişi.RandevuKontrol(sender,this);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dtdoğum.Text = null;
            dtkayıt.SelectedDate = DateTime.Now;

        }

        private void Btresimekle_Click(object sender, RoutedEventArgs e) => Resim.ResimEkranı(this);

        private void BtKişiTahsilat_Click(object sender, RoutedEventArgs e) => Ödeme.ÖdemeEkranı(sender, this);

        private void HypAra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cv.Filter = cv => ((Kisiler)cv).Adi.StartsWith(FindVisualChildByName<TextBox>(dataGrid, "fltAd").Text);

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + Environment.NewLine + "Meali: Seçimi Enter Tuşu İle Onaylayın.");
            }
        }

        private void Hypİptal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cv.Filter = null;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message+Environment.NewLine+"Meali: Seçimi Enter Tuşu İle Onaylayın.");
            }
        }
    }
}