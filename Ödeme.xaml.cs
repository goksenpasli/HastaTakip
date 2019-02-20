using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HastaTakip
{
    /// <summary>
    ///     Interaction logic for Ödeme.xaml
    /// </summary>
    public partial class Ödeme : Window
    {
        public Ödeme()
        {
            InitializeComponent();
            TbToplam.DataContext = new Tahsilat();
            TbÖdenen.DataContext = new Tahsilat();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => lvödeme.ItemsSource = Kisiler.Tahsilat.Where(z => z.KisiID == (window.DataContext as Kisiler)?.KisiID);

        private void Btödemeekle_Click(object sender, RoutedEventArgs e)
        {
            if (!Doğrula.Geçerli(this))
            {
                MessageBox.Show("Tüm Alanlara Doğru Giriş Yaptığınızdan Emin Olun.", "Kişi", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            if (Convert.ToDouble(TbToplam.Text) < Convert.ToDouble(TbÖdenen.Text))
            {
                MessageBox.Show("Toplam Ödenenden Küçük Olamaz.", "Kişi", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                using (var dataContext = new Database().Veriler)
                {
                    var tahsilat = new Tahsilat
                    {
                        KisiID = (window.DataContext as Kisiler)?.KisiID,
                        Toplam = Convert.ToDouble(TbToplam.Text),
                        Odenen = Convert.ToDouble(TbÖdenen.Text),
                        Gun = DateTime.Now
                    };
                    tahsilat.Bitti = tahsilat.Toplam == tahsilat.Odenen;
                    Kisiler.Tahsilat.Add(tahsilat);
                    dataContext.Tahsilat.InsertOnSubmit(tahsilat);
                    dataContext.SubmitChanges();
                    lvödeme.ItemsSource =
                        Kisiler.Tahsilat.Where(z => z.KisiID == (window.DataContext as Kisiler)?.KisiID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtSil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dataContext = new Database().Veriler)
                {
                    var silinecek = dataContext.Tahsilat.SingleOrDefault(x =>
                        x.TahsilatID == (lvödeme.SelectedItem as Tahsilat).TahsilatID);
                    Kisiler.Tahsilat.Remove(Kisiler.Tahsilat.SingleOrDefault(x =>
                        x.TahsilatID == ((Tahsilat)lvödeme.SelectedItem).TahsilatID));
                    dataContext.Tahsilat.DeleteOnSubmit(silinecek);
                    dataContext.SubmitChanges();
                    lvödeme.ItemsSource =
                        Kisiler.Tahsilat.Where(z => z.KisiID == (window.DataContext as Kisiler)?.KisiID);
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        public static void ÖdemeEkranı(object sender, MainWindow mv) => new Ödeme
        {
            Owner = mv,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            WindowState = WindowState.Normal,
            DataContext = (sender as Button)?.DataContext as Kisiler
        }.ShowDialog();

        private void BtKapat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dataContext = new Database().Veriler)
                {
                    var database = dataContext.Tahsilat.SingleOrDefault(x =>
                        x.TahsilatID == (lvödeme.SelectedItem as Tahsilat).TahsilatID);
                    database.Bitti = true;
                    database.Odenen = database.Toplam;
                    var kapanacak = Kisiler.Tahsilat.SingleOrDefault(x =>
                        x.TahsilatID == ((Tahsilat)lvödeme.SelectedItem).TahsilatID);
                    kapanacak.Bitti = true;
                    kapanacak.Odenen = kapanacak.Toplam;
                    dataContext.SubmitChanges();
                    lvödeme.ItemsSource =
                        Kisiler.Tahsilat.Where(z => z.KisiID == (window.DataContext as Kisiler)?.KisiID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}