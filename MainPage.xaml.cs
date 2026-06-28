using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using pengolahancitra.Services;

namespace pengolahancitra
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _db = new DatabaseService();

        public MainPage() { InitializeComponent(); }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string user = EntryUsername.Text?.Trim();
            string pass = EntryPassword.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                await DisplayAlert("Peringatan", "Username dan password tidak boleh kosong!", "OK");
                return;
            }

            var existingUser = await _db.GetUserByUsernameAsync(user);

            if (existingUser == null)
            {
                await DisplayAlert("Ditolak", "Username tidak ditemukan!", "OK");
                return;
            }

            if (pass != existingUser.Password)
            {
                await DisplayAlert("Ditolak", "Password salah!", "OK");
                return;
            }

            // session tetep pakai Preferences, ini cuma buat nyimpen "siapa yg lagi login", bukan data utama
            Preferences.Set("session_username", existingUser.Username);
            Preferences.Set("session_role", existingUser.Role);

            Application.Current.MainPage = new FaceVerificationPage(existingUser.Username, existingUser.Role);
        }

        private void OnGoToRegisterClicked(object sender, EventArgs e)
            => Application.Current.MainPage = new RegisterPage();
    }
}