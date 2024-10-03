using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace APPHUELLA
{
    public partial class Huella : ContentPage
    {
        private readonly string username;
        private readonly string password;
        private const string ESP32_IP = "192.168.1.1"; // IP del ESP32

        public Huella(string user, string pass)
        {
            InitializeComponent();
            username = user;
            password = pass;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ConnectToESP32Wifi();
        }

        private async Task ConnectToESP32Wifi()
        {
            try
            {
                await Xamarin.Essentials.Permissions.RequestAsync<Permissions.NetworkState>();
                // Aquí deberías implementar la lógica para conectarte a la red WiFi del ESP32
                // Esto puede requerir un plugin adicional o código específico de la plataforma
                // Por ahora, asumiremos que la conexión es exitosa
                await DisplayAlert("Conexión", "Conectado a la red del ESP32", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo conectar a la red del ESP32: {ex.Message}", "OK");
            }
        }

        private async void OnCaptureHuellaClicked(object sender, EventArgs e)
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Por favor, escanee su huella",
                    "Necesitamos verificar su identidad");

                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);

                if (result.Authenticated)
                {
                    HuellaResult.Text = "Huella capturada exitosamente";
                    SaveHuellaButton.IsEnabled = true;
                }
                else
                {
                    HuellaResult.Text = "Falló la autenticación de la huella";
                }
            }
            catch (Exception ex)
            {
                HuellaResult.Text = $"Error durante la autenticación: {ex.Message}";
            }
        }

        private async void OnSaveHuellaClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "El nombre de usuario y la contraseña no pueden estar vacíos", "OK");
                return;
            }

            await SaveUserData();
            await SaveHuellaData();
        }

        private async Task SaveUserData()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(
                        $"usuario={Uri.EscapeDataString(username)}&contrasena={Uri.EscapeDataString(password)}",
                        Encoding.UTF8,
                        "application/x-www-form-urlencoded");

                    var response = await client.PostAsync($"http://{ESP32_IP}/save", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Datos de usuario guardados en el ESP32", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudieron guardar los datos de usuario en el ESP32", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar datos de usuario: {ex.Message}", "OK");
            }
        }

        private async Task SaveHuellaData()
        {
            try
            {
                // En un escenario real, aquí obtendrías los datos de la huella
                // Por ahora, usaremos un valor de ejemplo
                string huellaData = "ejemplo_huella_123";

                using (var client = new HttpClient())
                {
                    var content = new StringContent(
                        $"huella={Uri.EscapeDataString(huellaData)}",
                        Encoding.UTF8,
                        "application/x-www-form-urlencoded");

                    var response = await client.PostAsync($"http://{ESP32_IP}/saveHuella", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Datos de huella guardados en el ESP32", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudieron guardar los datos de huella en el ESP32", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar datos de huella: {ex.Message}", "OK");
            }
        }
    }
}