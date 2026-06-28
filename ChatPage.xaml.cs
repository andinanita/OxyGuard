using Microsoft.Maui.Controls;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace pengolahancitra
{
    public partial class ChatPage : ContentPage
    {
        private const string ApiKey = Secrets.GeminiApiKey;
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";
        private readonly HttpClient _httpClient = new HttpClient();

        public ChatPage()
        {
            InitializeComponent();
            TampilkanPesan("bot", "Halo! Saya asisten kesehatan OxyGuard. Silakan tanya apapun tentang PPOK, gejala, atau hasil deteksi kamu! 🫁");
        }

        private async void OnKirimClicked(object sender, EventArgs e)
        {
            string pesan = EntryChat.Text?.Trim();
            if (string.IsNullOrEmpty(pesan)) return;

            EntryChat.Text = "";
            TampilkanPesan("user", pesan);
            TampilkanPesan("bot", "⏳ Sedang memproses...");

            string jawaban = await KirimKeGemini(pesan);

            // Hapus pesan loading
            StackChat.Children.RemoveAt(StackChat.Children.Count - 1);
            TampilkanPesan("bot", jawaban);

            await ScrollChat.ScrollToAsync(0, StackChat.Height, true);
        }

        private async Task<string> KirimKeGemini(string pesan)
        {
            try
            {
                string prompt = $@"Kamu adalah asisten kesehatan untuk aplikasi OxyGuard yang membantu pengguna memahami PPOK (Penyakit Paru Obstruktif Kronik). 
Jawab pertanyaan berikut dengan bahasa Indonesia yang mudah dipahami orang awam, singkat dan jelas.
PENTING: Jangan gunakan format Markdown seperti **, *, #, atau simbol formatting lainnya. Tulis dalam teks biasa saja.
Pertanyaan: {pesan}";


                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiUrl}?key={ApiKey}", content);
                string responseJson = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseJson);
                string jawaban = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Maaf, tidak ada jawaban.";

                return jawaban;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private void TampilkanPesan(string pengirim, string pesan)
        {
            bool isUser = pengirim == "user";

            var frame = new Frame
            {
                BackgroundColor = isUser ? Color.FromArgb("#1A5276") : Color.FromArgb("#FFFFFF"),
                CornerRadius = 16,
                Padding = new Thickness(14, 10),
                HasShadow = false,
                HorizontalOptions = isUser ? LayoutOptions.End : LayoutOptions.Start,
                MaximumWidthRequest = 280
            };

            var label = new Label
            {
                Text = pesan,
                TextColor = isUser ? Colors.White : Color.FromArgb("#111827"),
                FontSize = 13,
                LineBreakMode = LineBreakMode.WordWrap
            };

            frame.Content = label;
            StackChat.Children.Add(frame);
        }

        private void OnKembaliClicked(object sender, EventArgs e) =>
            Application.Current.MainPage = new DashboardPenggunaPage();
    }
}