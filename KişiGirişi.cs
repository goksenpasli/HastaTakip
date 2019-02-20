using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace HastaTakip
{
    public static class KişiGirişi
    {
        public static void Ekle(MainWindow form)
        {
            if (!Doğrula.Geçerli(form))
            {
                MessageBox.Show("Tüm Alanlara Doğru Giriş Yaptığınızdan Emin Olun.", "Kişi", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                using (var dataContext = new Database().Veriler)
                {
                    var kişi = new Kisiler
                    {
                        Adi = form.txadı.Text,
                        DTarihi = form.dtdoğum.SelectedDate.Value,
                        KTarihi = form.dtkayıt.SelectedDate.Value,
                        Soyadi = form.txsoyadı.Text,
                        Adresi = form.txadresi.Text,
                        Cinsiyeti = form.cbcinsiyet.Text,
                        Ili = form.cbİlAdi.Text,
                        Ilcesi = form.cbilçesi.Text,
                        Meslek = form.txmesleği.Text,
                        Tc = form.txtc.Text,
                        Telefon = Convert.ToString(form.txtelefon.Value),
                        Yas = form.txyaşı.Text
                    };
                    var fizikimuayene = new FizikiMuayene
                    {
                        Ates = form.txateş.Text,
                        Boy = form.txboy.Text,
                        Kilo = form.txkilo.Text,
                        Nabiz = form.txnabız.Text,
                        Solunum = form.txsolunum.Text,
                        Tansiyon = form.txtansiyon.Text
                    };

                    var bulgu = new Bulgular
                    {
                        Sikayetler = form.txşikayet.Text,
                        Bogaz = form.cbboğaz.Text,
                        Sindirim = form.cbsindirim.Text,
                        Burun = form.cbburun.Text,
                        Deri = form.cbderi.Text,
                        Dolasim = form.cbdolaşım.Text,
                        Solunum = form.cbsolunum.Text,
                        Göz = form.cbgöz.Text,
                        Uriner = form.cbüriner.Text,
                        Kulak = form.cbkulak.Text,
                        Yenidogan = form.cbyenidoğan.Text,
                        Ilac = form.cbilaçreaksiyon.Text,
                        Hastalik = form.cbhastalıkarttıranetmen.Text
                    };

                    var laboratuar = new Laboratuar
                    {
                        WBC = form.txwbc.Text,
                        HB = form.txhb.Text,
                        HCT = form.txhct.Text,
                        PLT = form.txplt.Text,
                        MCV = form.txmcv.Text,
                        AKŞ = form.txakş.Text,
                        TKŞ = form.txtkş.Text,
                        ÜRE = form.txüre.Text,
                        KREATİNİN = form.txkreatinin.Text,
                        ALT = form.txalt.Text,
                        AST = form.txast.Text,
                        ALKF = form.txalkf.Text,
                        GGT = form.txggt.Text,
                        TPROTEİN = form.txtrotein.Text,
                        ALBUMİN = form.txalbumin.Text,
                        NA = form.txna.Text,
                        K = form.txk.Text,
                        CA = form.txca.Text,
                        P = form.txp.Text,
                        TKOL = form.txtkol.Text,
                        TG = form.txtg.Text,
                        LDL = form.txldl.Text,
                        HDL = form.txhdl.Text,
                        VLDL = form.txvldl.Text,
                        HBA1C = form.txhba1c.Text,
                        DEMİR = form.txdemir.Text,
                        TDEMİRBK = form.txtdemir.Text,
                        FERRİTTİN = form.txferrittin.Text,
                        B12 = form.txb12.Text,
                        FOLAT = form.txfolat.Text,
                        TBİL = form.txtbil.Text,
                        DBİL = form.txdbil.Text,
                        LDH = form.txldh.Text,
                        SEDİM = form.txsedim.Text,
                        CRP = form.txcrp.Text,
                        CPK = form.txcpk.Text,
                        ÜRİKASİT = form.txürik.Text,
                        GFR = form.txgfr.Text
                    };

                    kişi.Bulgular.Add(bulgu);
                    kişi.FizikiMuayenes.Add(fizikimuayene);
                    kişi.Laboratuars.Add(laboratuar);
                    dataContext.Kisiler.InsertOnSubmit(kişi);
                    Kisiler.Kişiler.Add(kişi);
                    dataContext.SubmitChanges();
                    EkranTemizle(form);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void Güncelle(object sender, MainWindow form)
        {
            if (!Doğrula.Geçerli(form.dataGrid))
            {
                MessageBox.Show("Tüm Alanlara Doğru Giriş Yaptığınızdan Emin Olun.", "Kişi", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                using (var dataContext = new Database().Veriler)
                {
                    var datagrid = (sender as Button)?.DataContext as Kisiler;
                    var kişiler = dataContext.Kisiler.SingleOrDefault(z => z.KisiID == datagrid.KisiID);
                    kişiler.Adi = datagrid.Adi;
                    kişiler.DTarihi = datagrid.DTarihi;
                    kişiler.KTarihi = datagrid.KTarihi;
                    kişiler.Soyadi = datagrid.Soyadi;
                    kişiler.Adresi = datagrid.Adresi;
                    kişiler.Cinsiyeti = datagrid.Cinsiyeti;
                    kişiler.Ili = datagrid.Ili;
                    kişiler.Ilcesi = datagrid.Ilcesi;
                    kişiler.Meslek = datagrid.Meslek;
                    kişiler.Tc = datagrid.Tc;
                    kişiler.Telefon = datagrid.Telefon;
                    kişiler.Yas = datagrid.Yas;
                    kişiler.RandevuTarihi = datagrid.RandevuTarihi;

                    var bulgular = dataContext.Bulgular.SingleOrDefault(z => z.KisiID == datagrid.KisiID);
                    bulgular.Sikayetler = form.txşikayet.Text;
                    bulgular.Bogaz = form.cbboğaz.Text;
                    bulgular.Sindirim = form.cbsindirim.Text;
                    bulgular.Burun = form.cbburun.Text;
                    bulgular.Deri = form.cbderi.Text;
                    bulgular.Dolasim = form.cbdolaşım.Text;
                    bulgular.Solunum = form.cbsolunum.Text;
                    bulgular.Göz = form.cbgöz.Text;
                    bulgular.Uriner = form.cbüriner.Text;
                    bulgular.Kulak = form.cbkulak.Text;
                    bulgular.Yenidogan = form.cbyenidoğan.Text;
                    bulgular.Ilac = form.cbilaçreaksiyon.Text;
                    bulgular.Hastalik = form.cbhastalıkarttıranetmen.Text;

                    var fizikiMuayene = dataContext.FizikiMuayene.SingleOrDefault(z => z.KisiID == datagrid.KisiID);
                    fizikiMuayene.Ates = form.txateş.Text;
                    fizikiMuayene.Boy = form.txboy.Text;
                    fizikiMuayene.Kilo = form.txkilo.Text;
                    fizikiMuayene.Nabiz = form.txnabız.Text;
                    fizikiMuayene.Solunum = form.txsolunum.Text;
                    fizikiMuayene.Tansiyon = form.txsolunum.Text;

                    var laboratuar = dataContext.Laboratuar.SingleOrDefault(z => z.KisiID == datagrid.KisiID);
                    laboratuar.WBC = form.txwbc.Text;
                    laboratuar.HB = form.txhb.Text;
                    laboratuar.HCT = form.txhct.Text;
                    laboratuar.PLT = form.txplt.Text;
                    laboratuar.MCV = form.txmcv.Text;
                    laboratuar.AKŞ = form.txakş.Text;
                    laboratuar.TKŞ = form.txtkş.Text;
                    laboratuar.ÜRE = form.txüre.Text;
                    laboratuar.KREATİNİN = form.txkreatinin.Text;
                    laboratuar.ALT = form.txalt.Text;
                    laboratuar.AST = form.txast.Text;
                    laboratuar.ALKF = form.txalkf.Text;
                    laboratuar.GGT = form.txggt.Text;
                    laboratuar.TPROTEİN = form.txtrotein.Text;
                    laboratuar.ALBUMİN = form.txalbumin.Text;
                    laboratuar.NA = form.txna.Text;
                    laboratuar.K = form.txk.Text;
                    laboratuar.CA = form.txca.Text;
                    laboratuar.P = form.txp.Text;
                    laboratuar.TKOL = form.txtkol.Text;
                    laboratuar.TG = form.txtg.Text;
                    laboratuar.LDL = form.txldl.Text;
                    laboratuar.HDL = form.txhdl.Text;
                    laboratuar.VLDL = form.txvldl.Text;
                    laboratuar.HBA1C = form.txhba1c.Text;
                    laboratuar.DEMİR = form.txdemir.Text;
                    laboratuar.TDEMİRBK = form.txtdemir.Text;
                    laboratuar.FERRİTTİN = form.txferrittin.Text;
                    laboratuar.B12 = form.txb12.Text;
                    laboratuar.FOLAT = form.txfolat.Text;
                    laboratuar.TBİL = form.txtbil.Text;
                    laboratuar.DBİL = form.txdbil.Text;
                    laboratuar.LDH = form.txldh.Text;
                    laboratuar.SEDİM = form.txsedim.Text;
                    laboratuar.CRP = form.txcrp.Text;
                    laboratuar.CPK = form.txcpk.Text;
                    laboratuar.ÜRİKASİT = form.txürik.Text;
                    laboratuar.GFR = form.txgfr.Text;

                    dataContext.SubmitChanges();
                    MessageBox.Show("Kişi Güncellendi.", "Kişi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void Sil(object sender)
        {
            try
            {
                using (var dataContext = new Database().Veriler)
                {
                    var kişi = (sender as Button)?.DataContext as Kisiler;

                    if (Kisiler.Tahsilat.Any(z => z.KisiID == kişi?.KisiID && z.Bitti == false))
                    {
                        MessageBox.Show("Dikkat Bu Kişinin Bitmemiş Tahsilatı Var. Gerekli Düzeltmeyi Yapın.", "Kişi",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    if (MessageBox.Show(kişi?.Adi + " Adlı Kişiyi Silmek İstiyor Musun?", "Kişi", MessageBoxButton.YesNo,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.No) != MessageBoxResult.Yes)
                        return;
                    var database = (from x in dataContext.Kisiler where x.KisiID == kişi.KisiID select x).Single();
                    dataContext.Kisiler.DeleteOnSubmit(database);
                    Kisiler.Kişiler.Remove(kişi);
                    dataContext.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void RandevuKontrol(object sender,MainWindow mv)
        {
            try
            {
                var randevutarihi = (sender as DateTimePicker)?.Value;
                var öncesi = randevutarihi.Value.AddHours(-0.5);
                var sonrası = randevutarihi.Value.AddHours(0.5);
                ((DateTimePicker)sender).Background = Kisiler.Kişiler.Any(z =>
                   z.RandevuTarihi != randevutarihi && z.RandevuTarihi > öncesi && z.RandevuTarihi < sonrası)
                    ? Brushes.Red
                    : null;
                mv.dataGrid.CommitEdit();
            }
            catch (Exception)
            {
            }
        }

        private static void EkranTemizle(MainWindow form)
        {
            form.txşikayet.Text = "";
            form.ughastabilgileri.Children.OfType<MaskedTextBox>().ToList().ForEach(z => z.Text = string.Empty);
            form.ughastabilgileri.Children.OfType<DatePicker>().ToList().ForEach(z => z.Text = null);
            form.ughastabilgileri.Children.OfType<TextBox>().ToList().ForEach(z => z.Clear());
            form.ugfizikimuayene.Children.OfType<TextBox>().ToList().ForEach(z => z.Clear());
            form.uglaboratuar.Children.OfType<TextBox>().ToList().ForEach(z => z.Clear());
            form.ughastabilgileri.Children.OfType<ComboBox>().ToList().ForEach(z => z.SelectedIndex = -1);
            form.ugbulgular.Children.OfType<CheckComboBox>().ToList().ForEach(z => z.SelectedValue = null);
        }
    }
}