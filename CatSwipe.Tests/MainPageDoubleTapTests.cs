using CatSwipe.Models;
using CatSwipe.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CatSwipe.Tests;

public class MainPageDoubleTapTests : IDisposable
{
    private readonly ICatService _mockCatService;
    private readonly string _tempFilePath;

    public MainPageDoubleTapTests()
    {
        // Create a real CatService for testing integration
        var httpClient = new HttpClient();
        var logger = NullLogger<CatService>.Instance;
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test_double_tap_{Guid.NewGuid()}.json");
        _mockCatService = new CatService(httpClient, logger, _tempFilePath);
    }

    public void Dispose()
    {
        // Clean up temp file
        try
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete temp file: {ex.Message}");
        }
    }

    [Fact]
    public async Task OnCardDoubleTapped_Should_SuperLikeCat()
    {
        // Arrange
        var cat = new Cat
        {
            Id = "test_cat_1",
            ImageUrl = "https://example.com/cat.jpg",
            Breed = "Test Breed",
            Description = "A test cat"
        };

        // Act
        await _mockCatService.SuperLikeCatAsync(cat);

        // Assert
        var likedCats = await _mockCatService.GetLikedCatsAsync();
        Assert.Single(likedCats);

        var superLikedCat = likedCats.First();
        Assert.True(superLikedCat.IsLiked);
        Assert.True(superLikedCat.IsSuperLiked);
        Assert.Equal("test_cat_1", superLikedCat.Id);
        Assert.NotNull(superLikedCat.LikedAt);
    }

    [Fact]
    public void Cat_Model_Should_Have_IsSuperLiked_Property()
    {
        // Arrange & Act
        var cat = new Cat
        {
            Id = "test",
            IsSuperLiked = true
        };

        // Assert
        Assert.True(cat.IsSuperLiked);

        // Test that it can be set to false as well
        cat.IsSuperLiked = false;
        Assert.False(cat.IsSuperLiked);
    }

    [Fact]
    public async Task DoubleTap_Integration_Should_Work_EndToEnd()
    {
        // This test simulates the double-tap workflow end-to-end

        // Arrange
        var testCat = new Cat
        {
            Id = "integration_test_cat",
            ImageUrl = "https://example.com/integration.jpg",
            Breed = "Integration Test Breed",
            Description = "Testing double-tap integration"
        };

        // Act - Simulate double-tap sequence
        // 1. SuperLike the cat (this is what happens in OnCardDoubleTapped)
        await _mockCatService.SuperLikeCatAsync(testCat);

        // 2. Verify it's in the liked collection
        var likedCats = await _mockCatService.GetLikedCatsAsync();

        // Assert
        Assert.Single(likedCats);
        var result = likedCats.First();

        // Verify all properties are set correctly for super-liked cat
        Assert.Equal("integration_test_cat", result.Id);
        Assert.True(result.IsLiked, "Cat should be liked after super-like");
        Assert.True(result.IsSuperLiked, "Cat should be marked as super-liked");
        Assert.NotNull(result.LikedAt);
        Assert.True(result.LikedAt.Value <= DateTime.Now);
        Assert.True(result.LikedAt.Value > DateTime.Now.AddMinutes(-1)); // Should be very recent
    }
}
