using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace pengolahancitra
{
    public partial class MainDashboardPage : ContentPage
    {
        public MainDashboardPage()
        {
            InitializeComponent();
            CekRoleDanArahkan();
        }

        private async void CekRoleDanArahkan()
        {
            await Task.Delay(1500); // biar splash screen keliatan

            string username = Preferences.Get("logged_in_user", "");
            string role = Preferences.Get($"user_role_{username}", "");

            if (role == "Pengawas")
                Application.Current.MainPage = new DashboardPengawasPage();
            else if (role == "Pengguna")
                Application.Current.MainPage = new DashboardPenggunaPage();
            else
                Application.Current.MainPage = new MainPage(); // balik login kalau role kosong
        }
    }
}