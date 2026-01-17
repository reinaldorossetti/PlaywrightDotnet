using Microsoft.Playwright;

namespace PlaywrightTestsXunit3.Helpers;

/// <summary>
/// Helper class para configurações comuns do Playwright
/// </summary>
public static class PlaywrightConfig
{
    /// <summary>
    /// Configuração padrão para execução em CI/CD
    /// </summary>
    public static BrowserTypeLaunchOptions GetCILaunchOptions()
    {
        return new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        };
    }

    /// <summary>
    /// Configuração para debugging local
    /// </summary>
    public static BrowserTypeLaunchOptions GetDebugLaunchOptions()
    {
        return new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 500,
            Devtools = true
        };
    }

    /// <summary>
    /// Configuração de contexto com screenshot e trace habilitados
    /// </summary>
    public static BrowserNewContextOptions GetTestContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true,
            RecordVideoDir = "videos/",
            RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 }
        };
    }

    /// <summary>
    /// Configuração para mobile testing (iPhone 12)
    /// </summary>
    public static DeviceDescriptor GetMobileDevice(IPlaywright playwright)
    {
        return playwright.Devices["iPhone 12"];
    }

    /// <summary>
    /// Timeouts padrão recomendados
    /// </summary>
    public static class Timeouts
    {
        public const int DefaultTimeout = 30000; // 30 segundos
        public const int NavigationTimeout = 60000; // 60 segundos
        public const int ShortTimeout = 5000; // 5 segundos
    }
}

/// <summary>
/// Helper class para ações comuns em testes
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Captura screenshot com timestamp
    /// </summary>
    public static async Task<string> CaptureScreenshotAsync(IPage page, string testName)
    {
        var directory = Path.Combine("screenshots", DateTime.Now.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(directory);

        var filename = $"{testName}_{DateTime.Now:HHmmss}.png";
        var fullPath = Path.Combine(directory, filename);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = fullPath,
            FullPage = true
        });

        return fullPath;
    }

    /// <summary>
    /// Espera elemento estar visível com retry
    /// </summary>
    public static async Task<bool> WaitForElementVisibleAsync(
        IPage page, 
        string selector, 
        int timeoutMs = 5000)
    {
        try
        {
            await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Scroll até elemento estar visível
    /// </summary>
    public static async Task ScrollToElementAsync(IPage page, string selector)
    {
        await page.EvaluateAsync($@"
            document.querySelector('{selector}').scrollIntoView({{
                behavior: 'smooth',
                block: 'center'
            }});
        ");
        await page.WaitForTimeoutAsync(500); // Aguardar scroll completar
    }

    /// <summary>
    /// Limpa e preenche input
    /// </summary>
    public static async Task ClearAndFillAsync(IPage page, string selector, string value)
    {
        await page.FillAsync(selector, "");
        await page.FillAsync(selector, value);
    }

    /// <summary>
    /// Aguarda por requisição de rede específica
    /// </summary>
    public static async Task<IResponse> WaitForApiResponseAsync(
        IPage page, 
        string urlPattern, 
        Func<Task> action)
    {
        var responseTask = page.WaitForResponseAsync(urlPattern);
        await action();
        return await responseTask;
    }

    /// <summary>
    /// Verifica se elemento contém texto específico
    /// </summary>
    public static async Task<bool> ElementContainsTextAsync(
        IPage page, 
        string selector, 
        string expectedText)
    {
        var text = await page.TextContentAsync(selector);
        return text?.Contains(expectedText, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Aguarda página estar completamente carregada
    /// </summary>
    public static async Task WaitForFullPageLoadAsync(IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForLoadStateAsync(LoadState.Load);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Gera dados de teste aleatórios
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random Random = new();

        public static string GenerateEmail()
        {
            return $"test{Random.Next(1000, 9999)}@example.com";
        }

        public static string GenerateName()
        {
            var firstNames = new[] { "João", "Maria", "Pedro", "Ana", "Carlos", "Julia" };
            var lastNames = new[] { "Silva", "Santos", "Oliveira", "Souza", "Lima", "Costa" };
            
            return $"{firstNames[Random.Next(firstNames.Length)]} {lastNames[Random.Next(lastNames.Length)]}";
        }

        public static string GeneratePhoneNumber()
        {
            return $"(11) 9{Random.Next(1000, 9999)}-{Random.Next(1000, 9999)}";
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}

/// <summary>
/// Fixture reutilizável para testes que compartilham browser instance
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }

    public async Task<IPage> CreateNewPageAsync()
    {
        return await Browser.NewPageAsync();
    }

    public async Task<IBrowserContext> CreateNewContextAsync(BrowserNewContextOptions? options = null)
    {
        return await Browser.NewContextAsync(options);
    }
}

/// <summary>
/// Attribute customizado para marcar testes como smoke tests
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SmokeTestAttribute : FactAttribute
{
    public SmokeTestAttribute()
    {
        DisplayName = $"[SMOKE] {DisplayName}";
    }
}

/// <summary>
/// Attribute customizado para marcar testes como flaky (instáveis)
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FlakyTestAttribute : FactAttribute
{
    public FlakyTestAttribute(string reason)
    {
        DisplayName = $"[FLAKY - {reason}] {DisplayName}";
    }
}
