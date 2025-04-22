using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App45
{
    public partial class FavoritesPage : ContentPage
    {
        private ObservableCollection<string> favoriteCities;

        public FavoritesPage()
        {
            InitializeComponent();
            favoriteCities = new ObservableCollection<string>();
            favoritesListView.ItemsSource = favoriteCities;
            LoadFavoriteCities();
        }

        private void LoadFavoriteCities()
        {
            var savedFavorites = Preferences.Get("FavoriteCities", "");
            if (!string.IsNullOrEmpty(savedFavorites))
            {
                var cities = savedFavorites.Split(',');
                foreach (var city in cities)
                {
                    if (!string.IsNullOrWhiteSpace(city) && !favoriteCities.Contains(city))
                    {
                        favoriteCities.Add(city.Trim());
                    }
                }
            }
            emptyFavoritesLabel.IsVisible = favoriteCities.Count == 0;
        }

        private async void FavoritesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is string selectedCity)
            {
                // Aquí puedes navegar de vuelta a la MainPage y buscar el clima de la ciudad seleccionada
                await Navigation.PopAsync(); // Volver a la MainPage
                MessagingCenter.Send<FavoritesPage, string>(this, "SearchCity", selectedCity);
                favoritesListView.SelectedItem = null; // Deseleccionar el elemento
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadFavoriteCities(); // Recargar la lista cuando la página aparece
        }
    }
}