using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.Generic;

namespace pengolahancitra
{
    public partial class DashboardPenggunaPage : ContentPage
    {
        public DashboardPenggunaPage()
        {
            InitializeComponent();
            MuatDataPengguna();
        }

        
        private void MuatDataPengguna()
        {
            string nama = Preferences.Get("logged_in_user", "Pengguna");
            LabelWelcome.Text = $"Halo, {nama} 👋";
            LabelRole.Text = "Pengguna Wearable OxyGuard";

            string statusMerokok = Preferences.Get($"user_merokok_{nama}", "Tidak diketahui");
            LabelMerokok.Text = statusMerokok;

            if (statusMerokok.Contains("Perokok Aktif"))
            {
                CardMerokok.BackgroundColor = Color.FromArgb("#FEE2E2");
                CardMerokok.BorderColor = Color.FromArgb("#FECACA");
                LabelMerokok.TextColor = Color.FromArgb("#DC2626");
            }
            else if (statusMerokok.Contains("Mantan Perokok"))
            {
                CardMerokok.BackgroundColor = Color.FromArgb("#FEF9C3");
                CardMerokok.BorderColor = Color.FromArgb("#FDE68A");
                LabelMerokok.TextColor = Color.FromArgb("#D35400");
            }
            else
            {
                CardMerokok.BackgroundColor = Color.FromArgb("#DCFCE7");
                CardMerokok.BorderColor = Color.FromArgb("#BBF7D0");
                LabelMerokok.TextColor = Color.FromArgb("#1E8449");
            }

            string hasilTerakhir = Preferences.Get($"hasil_terakhir_{nama}", "");
            if (hasilTerakhir.Contains("Berbahaya"))
            {
                LabelStatus.Text = "🚨 Berbahaya";
                LabelStatus.TextColor = Color.FromArgb("#DC2626");
                LabelStatusDesc.Text = "Segera hubungi tenaga medis!";
            }
            else if (hasilTerakhir.Contains("Waspada"))
            {
                LabelStatus.Text = "⚠️ Waspada";
                LabelStatus.TextColor = Color.FromArgb("#D35400");
                LabelStatusDesc.Text = "Kondisi perlu diperhatikan.";
            }
            else
            {
                LabelStatus.Text = "✅ Normal";
                LabelStatus.TextColor = Color.FromArgb("#1E8449");
                LabelStatusDesc.Text = "Kondisi paru-paru kamu baik!";
            }

            LabelSpO2.Text = Preferences.Get($"spo2_terakhir_{nama}", "-");
            LabelHR.Text = Preferences.Get($"hr_terakhir_{nama}", "-");
            LabelNapas.Text = Preferences.Get($"napas_terakhir_{nama}", "-");

            ListRiwayat.ItemsSource = new List<RiwayatItem>
            {
                new RiwayatItem { Tanggal = "01 Jun 2026 - 08:00", Status = "✅ Normal", StatusColor = Color.FromArgb("#1E8449") },
                new RiwayatItem { Tanggal = "31 Mei 2026 - 20:00", Status = "⚠️ Waspada", StatusColor = Color.FromArgb("#D35400") },
                new RiwayatItem { Tanggal = "30 Mei 2026 - 09:00", Status = "✅ Normal", StatusColor = Color.FromArgb("#1E8449") },
            };
        }

        private void OnMulaiDeteksiClicked(object sender, EventArgs e) =>
            Application.Current.MainPage = new DeteksiPage();

        private void OnChatClicked(object sender, EventArgs e) =>
            Application.Current.MainPage = new ChatPage();

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("logged_in_user");
            Application.Current.MainPage = new MainPage();
        }
    }

    public class RiwayatItem
    {
        public string Tanggal { get; set; }
        public string Status { get; set; }
        public Color StatusColor { get; set; }
    }
}