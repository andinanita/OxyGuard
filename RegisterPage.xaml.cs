using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System.IO;
using pengolahancitra.Models;
using pengolahancitra.Services;

namespace pengolahancitra
{
    public partial class RegisterPage : ContentPage
    {
        private byte[] _faceData;
        private readonly DatabaseService _db = new DatabaseService();

        public RegisterPage()
        {
            InitializeComponent();
            SetupPickers();
        }

        private void SetupPickers()
        {
            PickerRole.Items.Clear();
            PickerRole.Items.Add("Pengawas");
            PickerRole.Items.Add("Pengguna");

            PickerMerokok.Items.Clear();
            PickerMerokok.Items.Add("🚬 Perokok Aktif");
            PickerMerokok.Items.Add("⏳ Mantan Perokok");
            PickerMerokok.Items.Add("✅ Bukan Perokok");
        }

        private async void OnCaptureFaceClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null) return;

                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                _faceData = memoryStream.ToArray();
                CameraPreview.Source = ImageSource.FromStream(() => new MemoryStream(_faceData));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error Kamera", ex.Message, "OK");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string user = EntryUsername.Text?.Trim();
            string pass = EntryPassword.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                await DisplayAlert("Gagal", "Username dan password wajib diisi!", "OK");
                return;
            }

            if (PickerRole.SelectedIndex == -1)
            {
                await DisplayAlert("Gagal", "Silakan pilih hak akses!", "OK");
                return;
            }

            if (PickerMerokok.SelectedIndex == -1)
            {
                await DisplayAlert("Gagal", "Silakan pilih status merokok!", "OK");
                return;
            }

            if (_faceData == null)
            {
                await DisplayAlert("Gagal", "Foto wajah wajib diambil!", "OK");
                return;
            }

            var existingUser = await _db.GetUserByUsernameAsync(user);
            if (existingUser != null)
            {
                await DisplayAlert("Gagal", $"Username '{user}' sudah digunakan!", "OK");
                return;
            }

            string selectedRole = PickerRole.SelectedItem.ToString();
            string statusMerokok = PickerMerokok.SelectedItem.ToString();

            var newUser = new User
            {
                Username = user,
                Password = pass,
                Role = selectedRole,
                StatusMerokok = statusMerokok,
                FotoWajah = _faceData
            };

            await _db.SaveUserAsync(newUser);

            await DisplayAlert("Sukses", $"Akun '{user}' berhasil didaftarkan sebagai {selectedRole}!", "OK");
            Application.Current.MainPage = new MainPage();
        }

        private void OnBackToLoginClicked(object sender, EventArgs e) =>
            Application.Current.MainPage = new MainPage();
    }
}