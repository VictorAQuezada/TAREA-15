using System;
using Xamarin.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Xamarin.Essentials;
using System.Collections.Generic;
using System.Linq;

namespace App45
{
    public partial class MainPage : ContentPage
    {
        private const string ApiKey = "89e9c37a78ebf64c23cfec623a7382d1"; // ¡Reemplaza con tu API Key!
        private const string ApiUrl = "https://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}&units=metric";
        private List<string> favoriteCitiesList;

        public MainPage()
        {
            InitializeComponent();
            cityEntry.Text = "Santo Domingo";
            GetWeather("Santo Domingo");

            // Suscribirse al mensaje para buscar una ciudad desde la página de favoritos
            MessagingCenter.Subscribe<FavoritesPage, string>(this, "SearchCity", (sender, city) =>
            {
                cityEntry.Text = city;
                GetWeather(city);
            });
        }

        private async void GetWeatherButtonClicked(object sender, EventArgs e)
        {
            string city = cityEntry.Text;

            if (string.IsNullOrWhiteSpace(city))
            {
                await DisplayAlert("❌ Error", "Por favor, ingresa una ciudad 🗺.", "OK");
                return;
            }

            GetWeather(city);
        }

        private async void GetWeather(string city)
        {
            string apiUrl = string.Format(ApiUrl, city, ApiKey);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(apiUrl);
                    JObject json = JObject.Parse(response);

                    string cityName = json["name"]?.ToString();
                    double temperature = json["main"]["temp"].ToObject<double>();
                    double feelsLike = json["main"]["feels_like"].ToObject<double>();
                    int humidity = json["main"]["humidity"].ToObject<int>();
                    int pressure = json["main"]["pressure"].ToObject<int>();
                    double windSpeed = json["wind"]["speed"].ToObject<double>();
                    string description = json["weather"][0]["description"]?.ToString();
                    double tempMin = json["main"]["temp_min"].ToObject<double>();
                    double tempMax = json["main"]["temp_max"].ToObject<double>();

                    long sunriseUnix = json["sys"]["sunrise"].ToObject<long>();
                    long sunsetUnix = json["sys"]["sunset"].ToObject<long>();
                    DateTime sunrise = DateTimeOffset.FromUnixTimeSeconds(sunriseUnix).ToLocalTime().DateTime;
                    DateTime sunset = DateTimeOffset.FromUnixTimeSeconds(sunsetUnix).ToLocalTime().DateTime;

                    string emoji = GetWeatherEmoji(description);

                    cityLabel.Text = $"{cityName} ☀️";
                    temperatureLabel.Text = $"{temperature:F1} °C 🌡️";
                    feelsLikeLabel.Text = $"Sensación térmica: {feelsLike:F1} °C 🔥";
                    descriptionLabel.Text = $"{description} {emoji}";
                    humidityLabel.Text = $"Humedad: {humidity}% 💧";
                    pressureLabel.Text = $"Presión: {pressure} hPa 🌪️";
                    windLabel.Text = $"Viento: {windSpeed} m/s 🍃";
                    minMaxLabel.Text = $"Mín: {tempMin:F1} °C | Máx: {tempMax:F1} °C 📈";
                    sunriseLabel.Text = $"🌅 Amanecer: {sunrise:HH:mm}";
                    sunsetLabel.Text = $"🌇 Atardecer: {sunset:HH:mm}";

                    weatherCard.IsVisible = true;
                    addToFavoritesButton.IsVisible = true; // Mostrar el botón de agregar a favoritos al obtener el clima

                    // Actualizar la lista local de favoritos para verificar si la ciudad actual ya está guardada
                    LoadFavoriteCitiesList();
                    addToFavoritesButton.Text = favoriteCitiesList.Contains(cityName) ? "⭐ Favorito" : "⭐ Agregar a Favoritos";
                }
                catch (Exception ex)
                {
                    await DisplayAlert("❌Error", $"Error al obtener el clima 🥵: {ex.Message}", "OK");
                    weatherCard.IsVisible = false;
                    addToFavoritesButton.IsVisible = false;
                }
            }
        }

        private string GetWeatherEmoji(string description)
        {
            description = description.ToLower();
            if (description.Contains("clear")) return "☀️";
            if (description.Contains("clouds")) return "☁️";
            if (description.Contains("rain")) return "🌧️";
            if (description.Contains("snow")) return "❄️";
            if (description.Contains("storm")) return "⛈️";
            if (description.Contains("fog") || description.Contains("mist")) return "🌫️";
            return "❓";
        }

        private async void AddToFavoritesButtonClicked(object sender, EventArgs e)
        {
            if (weatherCard.IsVisible && !string.IsNullOrEmpty(cityLabel.Text))
            {
                var cityName = cityLabel.Text.Replace(" ☀️", ""); // Remover el emoji del nombre para guardar
                LoadFavoriteCitiesList();

                if (!favoriteCitiesList.Contains(cityName))
                {
                    favoriteCitiesList.Add(cityName);
                    SaveFavoriteCitiesList();
                    addToFavoritesButton.Text = "⭐ Favorito";
                    await DisplayAlert("¡Éxito!", $"{cityName} ha sido añadido a tus favoritos.", "OK");
                }
                else
                {
                    await DisplayAlert("¡Ya está!", $"{cityName} ya está en tus favoritos.", "OK");
                }
            }
            else
            {
                await DisplayAlert("¡Ups!", "Primero busca el clima de una ciudad.", "OK");
            }
        }

        private async void ViewFavoritesButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FavoritesPage());
        }

        private void LoadFavoriteCitiesList()
        {
            var savedFavorites = Preferences.Get("FavoriteCities", "");
            favoriteCitiesList = savedFavorites.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        }

        private void SaveFavoriteCitiesList()
        {
            Preferences.Set("FavoriteCities", string.Join(",", favoriteCitiesList));
        }
    }
}