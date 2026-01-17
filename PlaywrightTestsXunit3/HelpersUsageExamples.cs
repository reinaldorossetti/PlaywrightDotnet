using Microsoft.Playwright;
using PlaywrightTestsXunit3.Helpers;

namespace PlaywrightTestsXunit3;

/// <summary>
/// Exemplos de uso dos helpers e configurações customizadas
/// </summary>
public class HelpersUsageExamples : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        
        // Usar configuração de CI/CD
        _browser = await _playwright.Chromium.LaunchAsync(
            PlaywrightConfig.GetCILaunchOptions()
        );
        
        _page = await _browser.NewPageAsync();
        
        // Configurar timeouts padrão
        _page.SetDefaultTimeout(PlaywrightConfig.Timeouts.DefaultTimeout);
        _page.SetDefaultNavigationTimeout(PlaywrightConfig.Timeouts.NavigationTimeout);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [SmokeTest]
    public async Task Should_Execute_Smoke_Test()
    {
        // Este teste é marcado como Smoke Test
        await _page.GotoAsync("https://playwright.dev");
        await Expect(_page).ToHaveTitleAsync(new Regex("Playwright"));
    }

    [Fact]
    public async Task Should_Use_Screenshot_Helper()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var screenshotPath = await TestHelpers.CaptureScreenshotAsync(_page, "HomePage");

        // Assert
        Assert.True(File.Exists(screenshotPath), "Screenshot should be saved");
    }

    [Fact]
    public async Task Should_Use_WaitForElement_Helper()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var isVisible = await TestHelpers.WaitForElementVisibleAsync(_page, "nav");

        // Assert
        Assert.True(isVisible, "Navigation should be visible");
    }

    [Fact]
    public async Task Should_Use_ClearAndFill_Helper()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='text' id='name' value='Initial Value' />
                </body>
            </html>
        ");

        // Act
        await TestHelpers.ClearAndFillAsync(_page, "#name", "New Value");
        var value = await _page.InputValueAsync("#name");

        // Assert
        Assert.Equal("New Value", value);
    }

    [Fact]
    public async Task Should_Generate_Test_Data()
    {
        // Arrange & Act
        var email = TestHelpers.TestDataGenerator.GenerateEmail();
        var name = TestHelpers.TestDataGenerator.GenerateName();
        var phone = TestHelpers.TestDataGenerator.GeneratePhoneNumber();
        var randomString = TestHelpers.TestDataGenerator.GenerateRandomString(10);

        // Assert
        Assert.Contains("@example.com", email);
        Assert.Contains(" ", name); // Nome e sobrenome
        Assert.Contains("(11)", phone);
        Assert.Equal(10, randomString.Length);
    }

    [Fact]
    public async Task Should_Wait_For_API_Response()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var response = await TestHelpers.WaitForApiResponseAsync(
            _page,
            "**/api/**",
            async () => await _page.ReloadAsync()
        );

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Should_Check_Element_Contains_Text()
    {
        // Arrange
        await _page.GotoAsync("https://playwright.dev");

        // Act
        var containsText = await TestHelpers.ElementContainsTextAsync(
            _page, 
            "h1", 
            "Playwright"
        );

        // Assert
        Assert.True(containsText);
    }

    [Fact]
    public async Task Should_Wait_For_Full_Page_Load()
    {
        // Arrange & Act
        await _page.GotoAsync("https://playwright.dev");
        await TestHelpers.WaitForFullPageLoadAsync(_page);

        // Assert
        var title = await _page.TitleAsync();
        Assert.NotEmpty(title);
    }

    [Fact]
    public async Task Should_Scroll_To_Element()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body style='height: 3000px;'>
                    <div id='top'>Top</div>
                    <div id='bottom' style='margin-top: 2500px;'>Bottom</div>
                </body>
            </html>
        ");

        // Act
        await TestHelpers.ScrollToElementAsync(_page, "#bottom");
        
        // Verificar se o elemento está no viewport
        var isInViewport = await _page.EvaluateAsync<bool>(@"
            const element = document.getElementById('bottom');
            const rect = element.getBoundingClientRect();
            return rect.top >= 0 && rect.bottom <= window.innerHeight;
        ");

        // Assert
        Assert.True(isInViewport, "Element should be scrolled into viewport");
    }
}

/// <summary>
/// Exemplo de uso do BrowserFixture para compartilhar browser entre testes
/// </summary>
public class SharedBrowserTests : IClassFixture<BrowserFixture>
{
    private readonly BrowserFixture _fixture;

    public SharedBrowserTests(BrowserFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test1_Should_Use_Shared_Browser()
    {
        // Arrange
        var page = await _fixture.CreateNewPageAsync();

        // Act
        await page.GotoAsync("https://playwright.dev");
        var title = await page.TitleAsync();

        // Assert
        Assert.Contains("Playwright", title);

        // Cleanup
        await page.CloseAsync();
    }

    [Fact]
    public async Task Test2_Should_Use_Shared_Browser()
    {
        // Arrange
        var page = await _fixture.CreateNewPageAsync();

        // Act
        await page.GotoAsync("https://github.com");
        var title = await page.TitleAsync();

        // Assert
        Assert.Contains("GitHub", title);

        // Cleanup
        await page.CloseAsync();
    }

    [Fact]
    public async Task Test3_Should_Use_Custom_Context()
    {
        // Arrange
        var context = await _fixture.CreateNewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            UserAgent = "Custom User Agent"
        });
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync("https://playwright.dev");
        var viewport = page.ViewportSize;

        // Assert
        Assert.Equal(1920, viewport?.Width);
        Assert.Equal(1080, viewport?.Height);

        // Cleanup
        await context.CloseAsync();
    }
}
