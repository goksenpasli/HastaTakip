using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WebEye.Controls.Wpf;
using Webp;

namespace HastaTakip
{
    /// <summary>
    ///     Interaction logic for Resim.xaml
    /// </summary>
    public partial class Resim
    {
        public Resim()
        {
            InitializeComponent();
            DataContext = this;
            InitializeComboBox();
            comboBox.ItemsSource = Kisiler.Kişiler;
        }

        public static double Kalite { get; set; } = 45;

        private static bool EskiIşletimSistemi() => Environment.OSVersion.Version.Major.ToString() == "5";

        private void BtnResimYükle_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Resim Dosyaları (*.jpg;*.bmp;*.png;*.gif:*.tif)|*.jpg;*.bmp;*.png;*.gif:*.tif"
            };
            if (openFileDialog.ShowDialog() == true)
                TxtResimYolu.Text = openFileDialog.FileName;
        }

        private static byte[] ResimYükle(Bitmap bmp)
        {
            using (var webp = new WebP())
            {
                return webp.EncodeLossy(bmp, (int)Kalite);
            }
        }

        private void BtnResimKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (TxtResimYolu.Text.Length == 0)
            {
                MessageBox.Show("Önce Resim Seç.", "Resim", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                if (!(comboBox.SelectedItem is Kisiler)) return;
                var bmp = new Bitmap(TxtResimYolu.Text);
                if (bmp.Width * bmp.Height > 1280 * 960)
                {
                    MessageBox.Show(
                        "Veritabanının Gereksiz Şişmesini Önlemek ve Menülerin Açılmasını Yavaşlatmamak İçin 1280x960=1228800 Pikselden Büyük Resim Yükleyemezsiniz. Yüklenmek İstenen Resim " +
                        bmp.Width * bmp.Height + " Piksel Resmi Küçültüp Tekrar Deneyin.", "Resim", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                using (var dataContext = new Database().Veriler)
                {
                    var kişiler =
                        dataContext.Kisiler.SingleOrDefault(z => z.KisiID == (comboBox.SelectedItem as Kisiler).KisiID);
                    kişiler.Resim = ResimYükle(bmp);
                    ((Kisiler)comboBox.SelectedItem).Resim = kişiler.Resim;
                    dataContext.SubmitChanges();
                }

                MessageBox.Show("Resim Kaydedildi.", "Resim", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void InitializeComboBox()
        {
            comboBox1.ItemsSource = new WebCameraControl().GetVideoCaptureDevices();

            if (comboBox1.Items.Count > 0) comboBox1.SelectedItem = comboBox1.Items[0];
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) => startButton.IsEnabled = e.AddedItems.Count > 0;

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            var cameraId = (WebCameraId)comboBox1.SelectedItem;
            webCameraControl.StartCapture(cameraId);
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e) => webCameraControl.StopCapture();

        private void OnImageButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Bitmap Resmi|*.bmp" };
            if (dialog.ShowDialog() == true) webCameraControl.GetCurrentImage().Save(dialog.FileName);
        }

        public static void ComboBoxResimKaydet(RoutedEventArgs e)
        {
            if (EskiIşletimSistemi())
            {
                MessageBox.Show("Eski İşletim Sistemlerinde Resim Kaydedemezsin. Sisteminizi Güncelleyin.");
                return;
            }

            try
            {
                var kişi = (e.Source as Button)?.DataContext as Kisiler;
                if (kişi.Resim == null) return;
                var dialog = new SaveFileDialog
                {
                    Filter = "Webp Image|*.webp",
                    FileName = kişi.Adi + " " + kişi.Soyadi
                };
                if (dialog.ShowDialog() == true) File.WriteAllBytes(dialog.FileName, kişi.Resim.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void ResimEkranı(MainWindow mv)
        {
            if (EskiIşletimSistemi())
            {
                MessageBox.Show("Eski İşletim Sistemlerinde Resim Yükleyemezsin. Sisteminizi Güncelleyin.");
                return;
            }

            new Resim
            {
                Owner = mv,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Normal
            }.ShowDialog();
        }

        private void BtnResimSil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(comboBox.SelectedItem is Kisiler kişi)) return;
                using (var dataContext = new Database().Veriler)
                {
                    var kişiler = dataContext.Kisiler.SingleOrDefault(z => z.KisiID == kişi.KisiID);
                    kişiler.Resim = null;
                    kişi.Resim = null;
                    dataContext.SubmitChanges();
                }

                MessageBox.Show("Resim Kaldırıldı.", "Resim", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}