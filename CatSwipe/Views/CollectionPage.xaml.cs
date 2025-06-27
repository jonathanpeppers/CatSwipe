using CatSwipe.Models;
using CatSwipe.Services;

using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace CatSwipe.Views;

public partial class CollectionPage : ContentPage
{
    private readonly ICatService _catService;
    private readonly HttpClient _httpClient;

    private List<Cat> _likedCats = [];
    public List<Cat> LikedCats
    {
        get => _likedCats;
        set
        {
            _likedCats = value;
            OnPropertyChanged();
        }
    }

    private bool _isSharing = false;
    public bool IsSharing
    {
        get => _isSharing;
        set
        {
            _isSharing = value;
            OnPropertyChanged();
        }
    }

    public CollectionPage(ICatService catService, HttpClient httpClient)
    {
        _catService = catService;
        _httpClient = httpClient;
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadLikedCatsAsync();
    }

    private async Task LoadLikedCatsAsync()
    {
        try
        {
            LikedCats = await _catService.GetLikedCatsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load your cat collection: {ex.Message}", "OK");
        }
    }

    private async void OnCatTapped(object? sender, EventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not Cat cat)
            return;

        try
        {
            await ShareCatAsync(cat);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to share cat: {ex.Message}", "OK");
        }
    }

    private async Task ShareCatAsync(Cat cat)
    {
        // Create a temporary file to download the image
        var tempPath = Path.Combine(FileSystem.CacheDirectory, $"cat_{cat.Id}.jpg");

        try
        {
            // Show loading indicator
            IsSharing = true;

            // Download the cat image
            using var response = await _httpClient.GetAsync(cat.ImageUrl);
            response.EnsureSuccessStatusCode();

            // Save image to temporary file
            using var fileStream = File.Create(tempPath);
            await response.Content.CopyToAsync(fileStream);

            // Share the image
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Check out this {cat.Breed ?? "adorable cat"}!",
                File = new ShareFile(tempPath)
            });
        }
        finally
        {
            // Hide loading indicator
            IsSharing = false;

            // Clean up temporary file
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch
            {
                // Silently ignore cleanup errors
            }
        }
    }
}
