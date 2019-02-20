using FastReport;
using FastReport.Utils;

namespace HastaTakip
{
    public class Raporla
    {
        public Raporla(string streamname)
        {
            using (var bordrorapor = GetType().Assembly.GetManifestResourceStream(streamname))
            {
                using (var language = GetType().Assembly.GetManifestResourceStream(
                    "HastaTakip.Rapor.Turkish.frl"))
                {
                    using (var report = new Report())
                    {
                        Res.LoadLocale(language);
                        report.Load(bordrorapor);
                        report.SetParameterValue("Parameter", new Database().Veriler.Connection.ConnectionString);
                        Config.ReportSettings.ShowPerformance = false;
                        Config.PreviewSettings.Buttons = PreviewButtons.All & ~PreviewButtons.Edit;
                        report.Show(true);
                    }
                }
            }
        }
    }
}