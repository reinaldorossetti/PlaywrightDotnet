using Microsoft.Playwright;
using PlaywrightTestsXunit3.PageObjects;

namespace PlaywrightTestsXunit3;

/// <summary>
/// Testes utilizando o padrão Page Object Model
/// Demonstra como organizar testes de forma escalável e manutenível
/// </summary>
public class PageObjectTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private PlaywrightHomePage _homePage = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
        _homePage = new PlaywrightHomePage(_page);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Should_Load_HomePage_Successfully()
    {
        // Arrange & Act
        await _homePage.NavigateAsync();

        // Assert
        var hasLoaded = await _homePage.HasLoadedCorrectlyAsync();
        Assert.True(hasLoaded, "Home page should load correctly");
    }

    [Fact]
    public async Task Should_Display_Get_Started_Button()
    {
        // Arrange
        await _homePage.NavigateAsync();

        // Act
        var isVisible = await _homePage.IsGetStartedVisibleAsync();

        // Assert
        Assert.True(isVisible, "Get Started button should be visible");
    }

    [Fact]
    public async Task Should_Navigate_To_Docs_Page()
    {
        // Arrange
        await _homePage.NavigateAsync();

        // Act
        await _homePage.ClickDocsAsync();

        // Assert
        var currentUrl = _homePage.GetCurrentUrl();
        Assert.Contains("/docs", currentUrl);
    }

    [Fact]
    public async Task Should_Have_Correct_Page_Title()
    {
        // Arrange
        await _homePage.NavigateAsync();

        // Act
        var title = await _homePage.GetPageTitleAsync();

        // Assert
        Assert.Contains("Playwright", title);
    }

    [Fact]
    public async Task Should_Display_Navigation_Bar()
    {
        // Arrange
        await _homePage.NavigateAsync();

        // Act
        var isNavVisible = await _homePage.IsNavigationBarVisibleAsync();

        // Assert
        Assert.True(isNavVisible, "Navigation bar should be visible");
    }

    [Fact]
    public async Task Should_Navigate_Using_Get_Started()
    {
        // Arrange
        await _homePage.NavigateAsync();
        var originalUrl = _homePage.GetCurrentUrl();

        // Act
        await _homePage.ClickGetStartedAsync();
        await _homePage.WaitForPageLoadAsync();

        // Assert
        var newUrl = _homePage.GetCurrentUrl();
        Assert.NotEqual(originalUrl, newUrl);
    }
}
