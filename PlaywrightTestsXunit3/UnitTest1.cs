using Microsoft.Playwright;

namespace PlaywrightTestsXunit3;

/// <summary>
/// Testes básicos demonstrando a integração entre xUnit v3 e Playwright
/// </summary>
public class BasicPlaywrightTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    /// <summary>
    /// Inicializa o Playwright e o navegador antes de cada teste
    /// </summary>
    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // Executar sem interface gráfica para CI/CD
            SlowMo = 50 // Adiciona um pequeno delay entre ações (útil para debugging)
        });
        _page = await _browser.NewPageAsync();
    }

    /// <summary>
    /// Limpa os recursos após cada teste
    /// </summary>
    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Should_Navigate_To_Playwright_Website()
    {
        // Arrange & Act
        await _page.GotoAsync("https://playwright.dev");

        // Assert
        await Expect(_page).ToHaveTitleAsync(new Regex("Playwright"));
        Assert.Contains("playwright.dev", _page.Url);
    }

    [Fact]
    public async Task Should_Find_Get_Started_Link()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var getStartedLink = _page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

        // Assert
        await Expect(getStartedLink).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("https://playwright.dev", "Playwright")]
    [InlineData("https://github.com", "GitHub")]
    [InlineData("https://dotnet.microsoft.com", "NET")]
    public async Task Should_Verify_Different_Page_Titles(string url, string expectedTitle)
    {
        // Arrange & Act
        await _page.GotoAsync(url);
        var actualTitle = await _page.TitleAsync();

        // Assert
        Assert.Contains(expectedTitle, actualTitle, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Handle_Page_With_JavaScript()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var docsLink = _page.GetByRole(AriaRole.Link, new() { Name = "Docs" });
        await docsLink.ClickAsync();

        // Wait for navigation
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        await Expect(_page).ToHaveURLAsync(new Regex("docs"));
    }

    [Fact]
    public async Task Should_Take_Screenshot()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");
        var screenshotPath = Path.Combine("screenshots", $"playwright-home-{DateTime.Now:yyyyMMddHHmmss}.png");

        // Criar diretório se não existir
        Directory.CreateDirectory("screenshots");

        // Act
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = screenshotPath,
            FullPage = true
        });

        // Assert
        Assert.True(File.Exists(screenshotPath), "Screenshot should be saved");
    }

    [Fact]
    public async Task Should_Evaluate_JavaScript()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var pageHeight = await _page.EvaluateAsync<int>("() => document.body.scrollHeight");
        var userAgent = await _page.EvaluateAsync<string>("() => navigator.userAgent");

        // Assert
        Assert.True(pageHeight > 0, "Page height should be greater than 0");
        Assert.NotEmpty(userAgent);
    }

    [Fact]
    public async Task Should_Handle_Multiple_Tabs()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act - Abrir nova aba
        var newPage = await _browser.NewPageAsync();
        await newPage.GotoAsync("https://github.com");

        // Assert
        Assert.NotEqual(_page.Url, newPage.Url);
        await Expect(_page).ToHaveTitleAsync(new Regex("Playwright"));
        await Expect(newPage).ToHaveTitleAsync(new Regex("GitHub"));

        // Cleanup
        await newPage.CloseAsync();
    }

    [Fact]
    public async Task Should_Wait_For_Selector()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act & Assert
        var selector = await _page.WaitForSelectorAsync("nav", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        Assert.NotNull(selector);
        Assert.True(await selector.IsVisibleAsync());
    }

    [Fact]
    public async Task Should_Get_Text_Content()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var heading = _page.Locator("h1").First;
        var textContent = await heading.TextContentAsync();

        // Assert
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent);
    }

    [Fact]
    public async Task Should_Check_Element_Visibility()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var logo = _page.Locator(".navbar__logo");

        // Assert
        await Expect(logo).ToBeVisibleAsync();
        var isVisible = await logo.IsVisibleAsync();
        Assert.True(isVisible, "Logo should be visible");
    }
}
