# Playwright + xUnit v3 - Guia Completo de Testes em .NET

## 📋 Índice
- [Sobre o Projeto](#sobre-o-projeto)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Exemplos de Uso](#exemplos-de-uso)
- [Executando os Testes](#executando-os-testes)
- [Boas Práticas](#boas-práticas)
- [Troubleshooting](#troubleshooting)

## 🎯 Sobre o Projeto

Este projeto demonstra a integração entre **Playwright** e **xUnit v3** para criar testes end-to-end (E2E) robustos e confiáveis em aplicações .NET. O Playwright é uma ferramenta moderna de automação de testes que suporta múltiplos navegadores, enquanto o xUnit v3 é a mais recente versão do popular framework de testes para .NET.

## 🚀 Tecnologias Utilizadas

- **.NET 8.0** - Framework de desenvolvimento
- **xUnit v3.2.2** - Framework de testes unitários e integração
- **Playwright 1.57.0** - Ferramenta de automação de navegadores
- **Microsoft.Playwright.Xunit.v3** - Integração entre Playwright e xUnit v3

### Por que xUnit v3?

- **Melhor Performance**: Execução de testes mais rápida e eficiente
- **Suporte Nativo**: Integração com o Microsoft Testing Platform
- **Modernização**: Uso de recursos modernos do C# e .NET
- **Async/Await**: Suporte aprimorado para operações assíncronas

### Por que Playwright?

- **Multi-Browser**: Suporta Chromium, Firefox e WebKit
- **Auto-Wait**: Espera automática por elementos estarem prontos
- **Network Interception**: Controle total sobre requisições de rede
- **Screenshots e Vídeos**: Captura automática de evidências de teste
- **Trace Viewer**: Ferramenta poderosa para debug de testes

## 📦 Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior
- Visual Studio 2022 / VS Code / Rider
- Node.js (para instalação dos navegadores do Playwright)

## 🔧 Instalação

### 1. Criar um novo projeto de testes

```bash
# Criar solução
dotnet new sln -n PlaywrightDotnet

# Criar projeto de testes xUnit
dotnet new xunit -n PlaywrightTestsXunit3 -f net8.0

# Adicionar projeto à solução
dotnet sln add PlaywrightTestsXunit3/PlaywrightTestsXunit3.csproj
```

### 2. Instalar pacotes NuGet

```bash
cd PlaywrightTestsXunit3

# Remover pacotes xUnit v2 padrão
dotnet remove package xunit
dotnet remove package xunit.runner.visualstudio
dotnet remove package coverlet.collector

# Adicionar xUnit v3
dotnet add package xunit.v3.mtp-v2 --version 3.2.2

# Adicionar Playwright
dotnet add package Microsoft.Playwright --version 1.57.0
dotnet add package Microsoft.Playwright.Xunit.v3 --version 1.57.0
```

# resolvendo problemas de bibliotecas, em caso de erro, instale o template do xUnit v3.
```bash
dotnet new search xunit3
dotnet new install xunit.v3.templates
dotnet new xunit3 -n PlaywrightTestsXunit3
dotnet tool install --global PowerShell
```

### 3. Instalar navegadores do Playwright

```bash
# Instalar todos os navegadores
pwsh bin/Debug/net8.0/playwright.ps1 install

# Ou específico (Chromium, Firefox, WebKit)
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

## ⚙️ Configuração

### Estrutura do Projeto

```
PlaywrightTestsXunit3/
├── PlaywrightTestsXunit3.csproj
├── xunit.runner.json
├── Tests/
│   ├── BasicTests.cs
│   ├── FormTests.cs
│   └── NavigationTests.cs
└── PageObjects/
    └── HomePage.cs
```

### Configurar xunit.runner.json

Crie o arquivo `xunit.runner.json` na raiz do projeto:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": true,
  "parallelizeTestCollections": false,
  "maxParallelThreads": 4,
  "methodDisplay": "method",
  "methodDisplayOptions": "all"
}
```

### Configurar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <TestingPlatformDotnetTestSupport>true</TestingPlatformTestSupport>
  </PropertyGroup>

  <ItemGroup>
    <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Playwright" Version="1.57.0" />
    <PackageReference Include="Microsoft.Playwright.Xunit.v3" Version="1.57.0" />
    <PackageReference Include="xunit.v3.mtp-v2" Version="3.2.2" />
  </ItemGroup>

</Project>
```

## 💡 Exemplos de Uso

### Exemplo 1: Teste Básico com Playwright

```csharp
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace PlaywrightTestsXunit3.Tests;

public class BasicTests : PlaywrightTest
{
    [Fact]
    public async Task Should_Navigate_To_Google()
    {
        // Arrange
        await using var browser = await Playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        // Act
        await page.GotoAsync("https://www.google.com");
        
        // Assert
        await Expect(page).ToHaveTitleAsync(new Regex("Google"));
    }

    [Fact]
    public async Task Should_Search_On_Google()
    {
        // Arrange
        await using var browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        var page = await browser.NewPageAsync();

        // Act
        await page.GotoAsync("https://www.google.com");
        await page.FillAsync("textarea[name='q']", "Playwright .NET");
        await page.PressAsync("textarea[name='q']", "Enter");
        
        // Wait for results
        await page.WaitForSelectorAsync("#search");

        // Assert
        var title = await page.TitleAsync();
        Assert.Contains("Playwright .NET", title);
    }
}
```

### Exemplo 2: Testes com Fixture e IAsyncLifetime

```csharp
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace PlaywrightTestsXunit3.Tests;

public class FormTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            SlowMo = 100 // Adiciona delay para debugging
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Should_Fill_Form_Successfully()
    {
        // Arrange
        await _page.GotoAsync("https://www.example.com/form");

        // Act
        await _page.FillAsync("#name", "João Silva");
        await _page.FillAsync("#email", "joao@example.com");
        await _page.SelectOptionAsync("#country", "Brazil");
        await _page.CheckAsync("#terms");
        await _page.ClickAsync("button[type='submit']");

        // Assert
        await Expect(_page.Locator(".success-message"))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Should_Validate_Required_Fields()
    {
        // Arrange
        await _page.GotoAsync("https://www.example.com/form");

        // Act
        await _page.ClickAsync("button[type='submit']");

        // Assert
        var errorMessage = await _page.Locator(".error-message").TextContentAsync();
        Assert.Contains("obrigatório", errorMessage?.ToLower() ?? "");
    }
}
```

### Exemplo 3: Testes Parametrizados com Theory

```csharp
using Microsoft.Playwright;

namespace PlaywrightTestsXunit3.Tests;

public class SearchTests : IAsyncLifetime
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

    [Theory]
    [InlineData("Playwright", "Microsoft Playwright")]
    [InlineData("xUnit", "xUnit.net")]
    [InlineData(".NET 8", "dotnet")]
    public async Task Should_Search_Different_Terms(string searchTerm, string expectedResult)
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync("https://www.google.com");
        await page.FillAsync("textarea[name='q']", searchTerm);
        await page.PressAsync("textarea[name='q']", "Enter");
        await page.WaitForSelectorAsync("#search");

        // Assert
        var content = await page.ContentAsync();
        Assert.Contains(expectedResult, content, StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }
}
```

### Exemplo 4: Page Object Pattern

```csharp
// PageObjects/LoginPage.cs
using Microsoft.Playwright;

namespace PlaywrightTestsXunit3.PageObjects;

public class LoginPage
{
    private readonly IPage _page;
    private const string Url = "https://example.com/login";

    public LoginPage(IPage page)
    {
        _page = page;
    }

    // Locators
    private ILocator UsernameInput => _page.Locator("#username");
    private ILocator PasswordInput => _page.Locator("#password");
    private ILocator LoginButton => _page.Locator("button[type='submit']");
    private ILocator ErrorMessage => _page.Locator(".error-message");

    // Actions
    public async Task NavigateAsync()
    {
        await _page.GotoAsync(Url);
    }

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    // Assertions
    public async Task<string?> GetErrorMessageAsync()
    {
        return await ErrorMessage.TextContentAsync();
    }

    public async Task<bool> IsErrorVisibleAsync()
    {
        return await ErrorMessage.IsVisibleAsync();
    }
}

// Tests/LoginTests.cs
using PlaywrightTestsXunit3.PageObjects;
using Microsoft.Playwright;

namespace PlaywrightTestsXunit3.Tests;

public class LoginTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private LoginPage _loginPage = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _page = await _browser.NewPageAsync();
        _loginPage = new LoginPage(_page);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Should_Login_With_Valid_Credentials()
    {
        // Arrange
        await _loginPage.NavigateAsync();

        // Act
        await _loginPage.LoginAsync("user@example.com", "password123");

        // Assert
        await Expect(_page).ToHaveURLAsync(new Regex("dashboard"));
    }

    [Fact]
    public async Task Should_Show_Error_With_Invalid_Credentials()
    {
        // Arrange
        await _loginPage.NavigateAsync();

        // Act
        await _loginPage.LoginAsync("invalid@example.com", "wrongpass");

        // Assert
        var isErrorVisible = await _loginPage.IsErrorVisibleAsync();
        Assert.True(isErrorVisible);
    }
}
```

### Exemplo 5: Screenshots e Traces

```csharp
using Microsoft.Playwright;

namespace PlaywrightTestsXunit3.Tests;

public class VisualTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        
        // Iniciar tracing
        _context = await _browser.NewContextAsync();
        await _context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        // Salvar trace
        await _context.Tracing.StopAsync(new()
        {
            Path = $"traces/trace-{DateTime.Now:yyyyMMddHHmmss}.zip"
        });

        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Should_Capture_Screenshot_On_Failure()
    {
        try
        {
            // Arrange
            await _page.GotoAsync("https://example.com");

            // Act
            await _page.ClickAsync("#non-existent-element", new() { Timeout = 5000 });
        }
        catch
        {
            // Capturar screenshot em caso de falha
            await _page.ScreenshotAsync(new()
            {
                Path = $"screenshots/failure-{DateTime.Now:yyyyMMddHHmmss}.png",
                FullPage = true
            });
            throw;
        }
    }

    [Fact]
    public async Task Should_Compare_Screenshots()
    {
        // Arrange
        await _page.GotoAsync("https://example.com");

        // Act - Capturar screenshot
        var screenshot = await _page.ScreenshotAsync();

        // Assert - Comparar com baseline
        await Expect(_page).ToHaveScreenshotAsync("homepage.png", new()
        {
            MaxDiffPixels = 100
        });
    }
}
```

## 🏃 Executando os Testes

### Via Command Line

```bash
# Executar todos os testes
dotnet test

# Executar com verbose
dotnet test --logger "console;verbosity=detailed"

# Executar testes específicos
dotnet test --filter "FullyQualifiedName~BasicTests"

# Executar com paralelização
dotnet test -- xunit.parallelizeAssembly=true

# Executar e gerar relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Via Visual Studio

1. Abra o **Test Explorer** (Test > Test Explorer)
2. Clique em "Run All" para executar todos os testes
3. Use filtros para executar testes específicos

### Via VS Code

1. Instale a extensão ".NET Test Explorer"
2. Use o painel de testes para executar/debugar
3. Configure launch.json para debugging

### Visualizar Traces

```bash
# Abrir trace viewer
pwsh bin/Debug/net8.0/playwright.ps1 show-trace traces/trace-20260117.zip
```

## 📚 Boas Práticas

### 1. Organize seus Testes

```
Tests/
├── Unit/              # Testes unitários rápidos
├── Integration/       # Testes de integração
└── E2E/              # Testes end-to-end com Playwright
```

### 2. Use Nomes Descritivos

```csharp
// ❌ Ruim
[Fact]
public async Task Test1() { }

// ✅ Bom
[Fact]
public async Task Should_Display_Error_When_Login_Fails() { }
```

### 3. Implemente o Padrão AAA

```csharp
[Fact]
public async Task Example_Test()
{
    // Arrange - Preparar o ambiente
    await _page.GotoAsync("https://example.com");

    // Act - Executar a ação
    await _page.ClickAsync("#button");

    // Assert - Verificar o resultado
    await Expect(_page.Locator(".result")).ToBeVisibleAsync();
}
```

### 4. Use Locators Estáveis

```csharp
// ❌ Evite seletores frágeis
await _page.ClickAsync("div > div > button:nth-child(3)");

// ✅ Prefira seletores semânticos
await _page.ClickAsync("button[data-testid='submit-button']");
await _page.ClickAsync("text=Enviar");
```

### 5. Implemente Waits Inteligentes

```csharp
// Playwright já faz auto-wait, mas você pode customizar
await _page.ClickAsync("#button");
await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
await _page.WaitForSelectorAsync(".result", new() { State = WaitForSelectorState.Visible });
```

### 6. Gerencie Contextos de Navegador

```csharp
// Isolar testes com contextos separados
var context1 = await _browser.NewContextAsync();
var context2 = await _browser.NewContextAsync();
// Cada contexto tem seu próprio estado (cookies, localStorage, etc.)
```

### 7. Use Fixtures para Compartilhar Setup

```csharp
public class BrowserFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}

public class MyTests : IClassFixture<BrowserFixture>
{
    private readonly BrowserFixture _fixture;

    public MyTests(BrowserFixture fixture)
    {
        _fixture = fixture;
    }
}
```

## 🔍 Troubleshooting

### Problema: "Playwright not installed"

```bash
# Solução: Instalar os navegadores
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### Problema: Testes lentos

```csharp
// Solução: Use headless mode
await Playwright.Chromium.LaunchAsync(new() { Headless = true });

// Ou desabilite recursos desnecessários
await _browser.NewContextAsync(new()
{
    IgnoreHTTPSErrors = true,
    JavaScriptEnabled = true
});
```

### Problema: Timeouts frequentes

```csharp
// Aumentar timeout global
await _page.GotoAsync("https://example.com", new() { Timeout = 60000 });

// Ou configurar timeout padrão
_page.SetDefaultTimeout(30000);
_page.SetDefaultNavigationTimeout(60000);
```

### Problema: Testes instáveis (flaky)

```csharp
// Use assertions do Playwright com retry automático
await Expect(_page.Locator("#element")).ToBeVisibleAsync();

// Em vez de assertions diretas
var isVisible = await _page.Locator("#element").IsVisibleAsync();
Assert.True(isVisible); // ❌ Não tem retry
```

### Problema: Depuração difícil

```csharp
// Use slowMo para ver o que está acontecendo
await Playwright.Chromium.LaunchAsync(new()
{
    Headless = false,
    SlowMo = 1000
});

// Habilite logging
await _page.GotoAsync("https://example.com");
Console.WriteLine($"Current URL: {_page.Url}");
Console.WriteLine($"Title: {await _page.TitleAsync()}");
```

## 📖 Recursos Adicionais

- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [xUnit v3 Documentation](https://xunit.net/docs/getting-started/v3/cmdline)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Microsoft Testing Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/)

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fazer um Fork do projeto
2. Criar uma branch para sua feature (`git checkout -b feature/NovaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova feature'`)
4. Push para a branch (`git push origin feature/NovaFeature`)
5. Abrir um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.

---

**Desenvolvido com ❤️ usando Playwright e xUnit v3**