using Microsoft.Maui.Controls;
using pengolahancitra.Models;
using pengolahancitra.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pengolahancitra
{
    public partial class DashboardPengawasPage : ContentPage
    {
        private readonly DatabaseService _databaseService = new DatabaseService();

        public DashboardPengawasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await MuatDataPenggunaAsync();
        }

        private async Task MuatDataPenggunaAsync()
        {
            var daftarPengguna = new List<PenggunaItem>();
            var penggunaList = await _databaseService.GetUsersByRoleAsync("Pengguna");

            foreach (var user in penggunaList)
            {
                var riwayat = await _databaseService.GetHistoryByUserAsync(user.Id);
                var terakhir = riwayat.FirstOrDefault(); // sudah diurutkan terbaru duluan

                string statusMerokok = user.StatusMerokok ?? "Tidak diketahui";
                Color merokokColor;
                if (statusMerokok.Contains("Perokok Aktif"))
                    merokokColor = Color.FromArgb("#DC2626");
                else if (statusMerokok.Contains("Mantan Perokok"))
                    merokokColor = Color.FromArgb("#D35400");
                else
                    merokokColor = Color.FromArgb("#1E8449");

                string badge;
                Color statusColor;
                string dataTerakhir;
                string tanggalText;

                if (terakhir != null)
                {
                    switch (terakhir.HasilRisiko)
                    {
                        case "Tinggi":
                            badge = "🚨 Berbahaya";
                            statusColor = Color.FromArgb("#DC2626");
                            break;
                        case "Sedang":
                            badge = "⚠️ Waspada";
                            statusColor = Color.FromArgb("#D35400");
                            break;
                        default: // "Rendah" atau nilai lain
                            badge = "✅ Normal";
                            statusColor = Color.FromArgb("#1E8449");
                            break;
                    }

                    dataTerakhir = $"SpO2: {terakhir.SpO2:F0}% | HR: {terakhir.HeartRate:F0} | RR: {terakhir.RespiratoryRate:F0}";
                    tanggalText = $"Terakhir: {terakhir.Waktu:dd/MM/yyyy HH:mm}";
                }
                else
                {
                    badge = "✅ Normal";
                    statusColor = Color.FromArgb("#1E8449");
                    dataTerakhir = "SpO2: - | HR: - | RR: -";
                    tanggalText = "Belum ada data";
                }

                daftarPengguna.Add(new PenggunaItem
                {
                    Nama = user.Username,
                    StatusMerokok = statusMerokok,
                    MerokokColor = merokokColor,
                    DataTerakhir = dataTerakhir,
                    StatusTerakhir = tanggalText,
                    StatusBadge = badge,
                    StatusColor = statusColor
                });
            }

            ListPengguna.ItemsSource = daftarPengguna;
            LabelNormal.Text = daftarPengguna.FindAll(p => p.StatusBadge.Contains("Normal")).Count.ToString();
            LabelWaspada.Text = daftarPengguna.FindAll(p => p.StatusBadge.Contains("Waspada")).Count.ToString();
            LabelBerbahaya.Text = daftarPengguna.FindAll(p => p.StatusBadge.Contains("Berbahaya")).Count.ToString();
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("logged_in_user");
            Application.Current.MainPage = new MainPage();
        }
    }

    public class PenggunaItem
    {
        public string Nama { get; set; }
        public string StatusMerokok { get; set; }
        public Color MerokokColor { get; set; }
        public string DataTerakhir { get; set; }
        public string StatusTerakhir { get; set; }
        public string StatusBadge { get; set; }
        public Color StatusColor { get; set; }
    }
}