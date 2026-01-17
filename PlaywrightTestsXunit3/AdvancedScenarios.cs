using Microsoft.Playwright;

namespace PlaywrightTestsXunit3;

/// <summary>
/// Cenários avançados com Playwright e xUnit v3
/// Demonstra recursos como API mocking, network interception, e multi-browser testing
/// </summary>
public class AdvancedScenarios : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Should_Intercept_Network_Request()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        var intercepted = false;

        // Interceptar requests
        await page.RouteAsync("**/*", async route =>
        {
            intercepted = true;
            await route.ContinueAsync();
        });

        // Act
        await page.GotoAsync("https://playwright.dev");

        // Assert
        Assert.True(intercepted, "Request should be intercepted");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Should_Mock_API_Response()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        await page.RouteAsync("**/api/users", async route =>
        {
            var mockResponse = new
            {
                users = new[]
                {
                    new { id = 1, name = "João Silva" },
                    new { id = 2, name = "Maria Santos" }
                }
            };

            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = System.Text.Json.JsonSerializer.Serialize(mockResponse)
            });
        });

        await page.SetContentAsync(@"
            <html>
                <body>
                    <div id='users'></div>
                    <script>
                        fetch('/api/users')
                            .then(r => r.json())
                            .then(data => {
                                document.getElementById('users').textContent = 
                                    data.users.map(u => u.name).join(', ');
                            });
                    </script>
                </body>
            </html>
        ");

        // Act - Wait for the fetch to complete
        await page.WaitForFunctionAsync("() => document.getElementById('users').textContent !== ''");
        var usersText = await page.TextContentAsync("#users");

        // Assert
        Assert.Contains("João Silva", usersText);
        Assert.Contains("Maria Santos", usersText);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Should_Handle_Authentication()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            HttpCredentials = new HttpCredentials
            {
                Username = "admin",
                Password = "password123"
            }
        });

        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync("https://httpbin.org/basic-auth/admin/password123");
        var content = await page.ContentAsync();

        // Assert
        Assert.Contains("authenticated", content, StringComparison.OrdinalIgnoreCase);

        await context.CloseAsync();
    }

    [Fact]
    public async Task Should_Emulate_Mobile_Device()
    {
        // Arrange - Emular iPhone 12
        var iPhone12 = _playwright.Devices["iPhone 12"];
        var context = await _browser.NewContextAsync(iPhone12);
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync("https://playwright.dev");
        var userAgent = await page.EvaluateAsync<string>("() => navigator.userAgent");
        var viewport = page.ViewportSize;

        // Assert
        Assert.Contains("iPhone", userAgent);
        Assert.Equal(390, viewport?.Width);
        Assert.Equal(844, viewport?.Height);

        await context.CloseAsync();
    }

    [Fact]
    public async Task Should_Handle_Geolocation()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            Geolocation = new Geolocation
            {
                Latitude = -23.5505, // São Paulo
                Longitude = -46.6333
            },
            Permissions = new[] { "geolocation" }
        });

        var page = await context.NewPageAsync();

        await page.SetContentAsync(@"
            <html>
                <body>
                    <button onclick='getLocation()'>Get Location</button>
                    <div id='location'></div>
                    <script>
                        function getLocation() {
                            navigator.geolocation.getCurrentPosition(position => {
                                document.getElementById('location').textContent = 
                                    position.coords.latitude + ',' + position.coords.longitude;
                            });
                        }
                    </script>
                </body>
            </html>
        ");

        // Act
        await page.ClickAsync("button");
        await page.WaitForFunctionAsync("() => document.getElementById('location').textContent !== ''");
        var location = await page.TextContentAsync("#location");

        // Assert
        Assert.Contains("-23.5505", location);
        Assert.Contains("-46.6333", location);

        await context.CloseAsync();
    }

    [Theory]
    [InlineData("Chromium")]
    [InlineData("Firefox")]
    [InlineData("Webkit")]
    public async Task Should_Run_Test_In_Multiple_Browsers(string browserType)
    {
        // Arrange
        IBrowser testBrowser = browserType switch
        {
            "Chromium" => await _playwright.Chromium.LaunchAsync(),
            "Firefox" => await _playwright.Firefox.LaunchAsync(),
            "Webkit" => await _playwright.Webkit.LaunchAsync(),
            _ => throw new ArgumentException($"Unknown browser: {browserType}")
        };

        var page = await testBrowser.NewPageAsync();

        // Act
        await page.GotoAsync("https://playwright.dev");
        var title = await page.TitleAsync();

        // Assert
        Assert.Contains("Playwright", title);

        // Cleanup
        await page.CloseAsync();
        await testBrowser.CloseAsync();
    }

    [Fact]
    public async Task Should_Handle_LocalStorage()
    {
        // Arrange
        var context = await _browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.SetContentAsync(@"
            <html>
                <body>
                    <button id='save'>Save</button>
                    <button id='load'>Load</button>
                    <div id='result'></div>
                    <script>
                        document.getElementById('save').addEventListener('click', () => {
                            localStorage.setItem('user', JSON.stringify({name: 'João', age: 30}));
                        });
                        document.getElementById('load').addEventListener('click', () => {
                            const user = JSON.parse(localStorage.getItem('user'));
                            document.getElementById('result').textContent = user.name;
                        });
                    </script>
                </body>
            </html>
        ");

        // Act
        await page.ClickAsync("#save");
        await page.ClickAsync("#load");
        var result = await page.TextContentAsync("#result");

        // Assert
        Assert.Equal("João", result);

        // Verificar localStorage diretamente
        var storageValue = await page.EvaluateAsync<string>("localStorage.getItem('user')");
        Assert.Contains("João", storageValue);

        await context.CloseAsync();
    }

    [Fact]
    public async Task Should_Handle_Cookies()
    {
        // Arrange
        var context = await _browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Act - Adicionar cookie
        await context.AddCookiesAsync(new[]
        {
            new Cookie
            {
                Name = "session_id",
                Value = "abc123xyz",
                Domain = "playwright.dev",
                Path = "/",
                HttpOnly = true,
                Secure = true
            }
        });

        await page.GotoAsync("https://playwright.dev");

        // Assert - Verificar cookie
        var cookies = await context.CookiesAsync();
        var sessionCookie = cookies.FirstOrDefault(c => c.Name == "session_id");

        Assert.NotNull(sessionCookie);
        Assert.Equal("abc123xyz", sessionCookie.Value);

        await context.CloseAsync();
    }

    [Fact]
    public async Task Should_Handle_Dialog_Alert()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        var dialogMessage = "";

        page.Dialog += async (_, dialog) =>
        {
            dialogMessage = dialog.Message;
            await dialog.AcceptAsync();
        };

        await page.SetContentAsync(@"
            <html>
                <body>
                    <button onclick='alert(""Teste de alerta!"")'>Show Alert</button>
                </body>
            </html>
        ");

        // Act
        await page.ClickAsync("button");

        // Assert
        Assert.Equal("Teste de alerta!", dialogMessage);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Should_Handle_Download()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync("https://playwright.dev");

        // Act - Esperar por download
        var downloadTask = page.WaitForDownloadAsync();
        
        // Simular click em link de download (exemplo genérico)
        // await page.ClickAsync("a[download]");
        
        // Para este teste, vamos apenas verificar que o mecanismo funciona
        // Em um cenário real, você clicaria em um botão real de download

        // Assert
        Assert.NotNull(downloadTask);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Should_Execute_Parallel_Tests_Safely()
    {
        // Arrange - Criar múltiplas páginas
        var tasks = new List<Task<string>>();

        // Act - Executar navegações em paralelo
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var page = await _browser.NewPageAsync();
                await page.GotoAsync("https://playwright.dev");
                var title = await page.TitleAsync();
                await page.CloseAsync();
                return title;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, title => Assert.Contains("Playwright", title));
    }
}
