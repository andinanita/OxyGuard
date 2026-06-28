using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using pengolahancitra.Models;
using pengolahancitra.Services;

namespace pengolahancitra
{
    public partial class DeteksiPage : ContentPage
    {
        private readonly DatabaseService _databaseService = new DatabaseService();

        public DeteksiPage()
        {
            InitializeComponent();
        }

        private async void OnAnalisisClicked(object sender, EventArgs e)
        {
            if (!double.TryParse(EntrySpO2.Text, out double spo2) ||
                !double.TryParse(EntryHR.Text, out double hr) ||
                !double.TryParse(EntryNapas.Text, out double napas))
            {
                await DisplayAlert("Input Salah", "Masukkan angka yang valid!", "OK");
                return;
            }

            if (spo2 < 0 || spo2 > 100 || hr < 0 || hr > 300 || napas < 0 || napas > 60)
            {
                await DisplayAlert("Input Tidak Valid",
                    "SpO2: 0-100%\nHeart Rate: 0-300 bpm\nFrekuensi napas: 0-60/menit", "OK");
                return;
            }

            string username = Preferences.Get("logged_in_user", "");

            var user = await _databaseService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                await DisplayAlert("Error", "Data pengguna tidak ditemukan. Silakan login ulang.", "OK");
                return;
            }

            var hasil = HitungFuzzy(spo2, hr, napas, user.StatusMerokok ?? "");

            CardHasil.IsVisible = true;
            LabelHasil.Text = hasil.Status;
            LabelHasil.TextColor = hasil.Warna;
            LabelSkor.Text = $"Normal: {hasil.Normal:F1}% | Waspada: {hasil.Waspada:F1}% | Berbahaya: {hasil.Berbahaya:F1}%";
            LabelSaran.Text = hasil.Saran;

            string hasilRisiko;
            double skorUtama;
            if (hasil.Status.Contains("Berbahaya"))
            {
                hasilRisiko = "Tinggi";
                skorUtama = hasil.Berbahaya;
            }
            else if (hasil.Status.Contains("Waspada"))
            {
                hasilRisiko = "Sedang";
                skorUtama = hasil.Waspada;
            }
            else
            {
                hasilRisiko = "Rendah";
                skorUtama = hasil.Normal;
            }

            await _databaseService.SaveReadingAsync(new ReadingHistory
            {
                UserId = user.Id,
                SpO2 = spo2,
                HeartRate = hr,
                RespiratoryRate = napas,
                HasilRisiko = hasilRisiko,
                SkorFuzzy = skorUtama,
                Waktu = DateTime.Now
            });
        }

        private HasilFuzzy HitungFuzzy(double spo2, double hr, double napas, string statusMerokok)
        {
            bool perokokAktif = statusMerokok.Contains("Perokok Aktif");
            bool mantanPerokok = statusMerokok.Contains("Mantan Perokok");

            double offsetSpo2 = perokokAktif ? 2.0 : (mantanPerokok ? 1.0 : 0.0);
            double offsetNapas = perokokAktif ? 2.0 : (mantanPerokok ? 1.0 : 0.0);
            double offsetHR = perokokAktif ? 5.0 : (mantanPerokok ? 2.0 : 0.0);

            double spo2Adj = spo2 - offsetSpo2;
            double napasAdj = napas + offsetNapas;
            double hrAdj = hr + offsetHR;

            double spo2Normal = 0, spo2Waspada = 0, spo2Berbahaya = 0;
            if (spo2Adj >= 95) spo2Normal = 1.0;
            else if (spo2Adj >= 90) spo2Normal = (spo2Adj - 90) / 5.0;
            if (spo2Adj >= 90 && spo2Adj <= 95) spo2Waspada = (spo2Adj - 90) / 5.0;
            else if (spo2Adj >= 85 && spo2Adj < 90) spo2Waspada = (spo2Adj - 85) / 5.0;
            if (spo2Adj <= 85) spo2Berbahaya = 1.0;
            else if (spo2Adj <= 90) spo2Berbahaya = (90 - spo2Adj) / 5.0;

            double hrNormal = 0, hrWaspada = 0, hrBerbahaya = 0;
            if (hrAdj >= 60 && hrAdj <= 100) hrNormal = 1.0;
            else if (hrAdj >= 55 && hrAdj < 60) hrNormal = (hrAdj - 55) / 5.0;
            else if (hrAdj > 100 && hrAdj <= 110) hrNormal = (110 - hrAdj) / 10.0;
            if (hrAdj >= 100 && hrAdj <= 120) hrWaspada = (hrAdj - 100) / 20.0;
            else if (hrAdj > 120 && hrAdj <= 140) hrWaspada = (140 - hrAdj) / 20.0;
            else if (hrAdj >= 40 && hrAdj < 60) hrWaspada = (hrAdj - 40) / 20.0;
            if (hrAdj >= 140) hrBerbahaya = 1.0;
            else if (hrAdj >= 120) hrBerbahaya = (hrAdj - 120) / 20.0;
            else if (hrAdj <= 40) hrBerbahaya = 1.0;
            else if (hrAdj <= 55) hrBerbahaya = (55 - hrAdj) / 15.0;

            double napasNormal = 0, napasWaspada = 0, napasBerbahaya = 0;
            if (napasAdj >= 12 && napasAdj <= 20) napasNormal = 1.0;
            else if (napasAdj >= 10 && napasAdj < 12) napasNormal = (napasAdj - 10) / 2.0;
            else if (napasAdj > 20 && napasAdj <= 24) napasNormal = (24 - napasAdj) / 4.0;
            if (napasAdj >= 20 && napasAdj <= 25) napasWaspada = (napasAdj - 20) / 5.0;
            else if (napasAdj > 25 && napasAdj <= 30) napasWaspada = (30 - napasAdj) / 5.0;
            else if (napasAdj > 8 && napasAdj < 12) napasWaspada = (napasAdj - 8) / 4.0;
            if (napasAdj >= 30) napasBerbahaya = 1.0;
            else if (napasAdj >= 25) napasBerbahaya = (napasAdj - 25) / 5.0;
            else if (napasAdj <= 8) napasBerbahaya = 1.0;
            else if (napasAdj <= 10) napasBerbahaya = (10 - napasAdj) / 2.0;

            double scoreNormal = Math.Min(spo2Normal, Math.Min(hrNormal, napasNormal));
            double scoreWaspada = Math.Max(
                Math.Min(spo2Waspada, hrNormal),
                Math.Max(
                    Math.Min(spo2Normal, hrWaspada),
                    Math.Max(
                        Math.Min(spo2Normal, napasWaspada),
                        Math.Min(spo2Waspada, napasNormal)
                    )
                )
            );
            double scoreBerbahaya = Math.Max(
                Math.Min(spo2Berbahaya, hrBerbahaya),
                Math.Max(
                    Math.Min(spo2Berbahaya, napasNormal),
                    Math.Max(
                        Math.Min(spo2Normal, napasBerbahaya),
                        Math.Min(hrBerbahaya, napasNormal)
                    )
                )
            );

            double total = scoreNormal + scoreWaspada + scoreBerbahaya;
            if (total == 0) total = 1;

            double pctNormal = (scoreNormal / total) * 100;
            double pctWaspada = (scoreWaspada / total) * 100;
            double pctBerbahaya = (scoreBerbahaya / total) * 100;

            string status; Color warna; string saran;

            if (scoreBerbahaya >= scoreWaspada && scoreBerbahaya >= scoreNormal)
            {
                status = "🚨 Berbahaya";
                warna = Color.FromArgb("#DC2626");
                saran = perokokAktif
                    ? "Kondisi kritis! Sebagai perokok aktif, risiko PPOK sangat tinggi. Segera hubungi dokter!"
                    : "Segera hubungi tenaga medis! Kondisi paru-paru memerlukan penanganan segera.";
            }
            else if (scoreWaspada >= scoreNormal)
            {
                status = "⚠️ Waspada";
                warna = Color.FromArgb("#D35400");
                saran = perokokAktif
                    ? "Kondisi perlu diperhatikan. Sangat disarankan berhenti merokok dan konsultasi ke dokter."
                    : "Kondisi perlu diperhatikan. Istirahat cukup dan konsultasi ke dokter jika berlanjut.";
            }
            else
            {
                status = "✅ Normal";
                warna = Color.FromArgb("#1E8449");
                saran = perokokAktif
                    ? "Kondisi saat ini normal, namun kebiasaan merokok tetap berisiko. Pertimbangkan untuk berhenti merokok."
                    : "Kondisi paru-paru Anda baik. Pertahankan gaya hidup sehat!";
            }

            return new HasilFuzzy
            {
                Status = status,
                Warna = warna,
                Saran = saran,
                Normal = pctNormal,
                Waspada = pctWaspada,
                Berbahaya = pctBerbahaya
            };
        }

        private void OnKembaliClicked(object sender, EventArgs e) =>
            Application.Current.MainPage = new DashboardPenggunaPage();
    }

    public class HasilFuzzy
    {
        public string Status { get; set; }
        public Color Warna { get; set; }
        public string Saran { get; set; }
        public double Normal { get; set; }
        public double Waspada { get; set; }
        public double Berbahaya { get; set; }
    }
}