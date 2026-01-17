using Microsoft.Playwright;

namespace PlaywrightTestsXunit3;

/// <summary>
/// Testes demonstrando interações com formulários usando Playwright e xUnit v3
/// </summary>
public class FormInteractionTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
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
    public async Task Should_Fill_Text_Input()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='text' id='name' placeholder='Enter name' />
                </body>
            </html>
        ");

        // Act
        await _page.FillAsync("#name", "João Silva");
        var value = await _page.InputValueAsync("#name");

        // Assert
        Assert.Equal("João Silva", value);
    }

    [Fact]
    public async Task Should_Select_Dropdown_Option()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <select id='country'>
                        <option value=''>Selecione</option>
                        <option value='br'>Brasil</option>
                        <option value='us'>Estados Unidos</option>
                        <option value='uk'>Reino Unido</option>
                    </select>
                </body>
            </html>
        ");

        // Act
        await _page.SelectOptionAsync("#country", "br");
        var selectedValue = await _page.EvaluateAsync<string>("document.getElementById('country').value");

        // Assert
        Assert.Equal("br", selectedValue);
    }

    [Fact]
    public async Task Should_Check_And_Uncheck_Checkbox()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='checkbox' id='terms' />
                    <label for='terms'>Aceito os termos</label>
                </body>
            </html>
        ");

        // Act - Check
        await _page.CheckAsync("#terms");
        var isChecked = await _page.IsCheckedAsync("#terms");
        Assert.True(isChecked, "Checkbox should be checked");

        // Act - Uncheck
        await _page.UncheckAsync("#terms");
        isChecked = await _page.IsCheckedAsync("#terms");
        Assert.False(isChecked, "Checkbox should be unchecked");
    }

    [Fact]
    public async Task Should_Select_Radio_Button()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='radio' name='gender' value='male' id='male' />
                    <label for='male'>Masculino</label>
                    <input type='radio' name='gender' value='female' id='female' />
                    <label for='female'>Feminino</label>
                </body>
            </html>
        ");

        // Act
        await _page.CheckAsync("#female");
        var isFemaleChecked = await _page.IsCheckedAsync("#female");
        var isMaleChecked = await _page.IsCheckedAsync("#male");

        // Assert
        Assert.True(isFemaleChecked, "Female radio should be checked");
        Assert.False(isMaleChecked, "Male radio should not be checked");
    }

    [Fact]
    public async Task Should_Upload_File()
    {
        // Arrange
        var testFilePath = Path.Combine(Path.GetTempPath(), "test-upload.txt");
        await File.WriteAllTextAsync(testFilePath, "Conteúdo de teste");

        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='file' id='upload' />
                </body>
            </html>
        ");

        // Act
        await _page.SetInputFilesAsync("#upload", testFilePath);
        var fileName = await _page.EvaluateAsync<string>(
            "document.getElementById('upload').files[0].name"
        );

        // Assert
        Assert.Equal("test-upload.txt", fileName);

        // Cleanup
        File.Delete(testFilePath);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("another@test.co.uk", true)]
    public async Task Should_Validate_Email_Input(string email, bool shouldBeValid)
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <form>
                        <input type='email' id='email' required />
                        <button type='submit'>Enviar</button>
                    </form>
                </body>
            </html>
        ");

        // Act
        await _page.FillAsync("#email", email);
        var isValid = await _page.EvaluateAsync<bool>(
            "document.getElementById('email').checkValidity()"
        );

        // Assert
        Assert.Equal(shouldBeValid, isValid);
    }

    [Fact]
    public async Task Should_Handle_Form_Submission()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <form id='testForm'>
                        <input type='text' id='username' name='username' />
                        <input type='password' id='password' name='password' />
                        <button type='submit'>Login</button>
                    </form>
                    <div id='result'></div>
                    <script>
                        document.getElementById('testForm').addEventListener('submit', (e) => {
                            e.preventDefault();
                            document.getElementById('result').textContent = 'Form submitted!';
                        });
                    </script>
                </body>
            </html>
        ");

        // Act
        await _page.FillAsync("#username", "testuser");
        await _page.FillAsync("#password", "password123");
        await _page.ClickAsync("button[type='submit']");

        // Wait for result
        await _page.WaitForSelectorAsync("#result");
        var resultText = await _page.TextContentAsync("#result");

        // Assert
        Assert.Equal("Form submitted!", resultText);
    }

    [Fact]
    public async Task Should_Clear_Input_Field()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='text' id='search' value='texto inicial' />
                </body>
            </html>
        ");

        // Act
        await _page.FillAsync("#search", ""); // Clear usando Fill
        var value = await _page.InputValueAsync("#search");

        // Assert
        Assert.Empty(value);
    }

    [Fact]
    public async Task Should_Type_With_Delay()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <input type='text' id='typewriter' />
                </body>
            </html>
        ");

        // Act - Simula digitação humana
        await _page.TypeAsync("#typewriter", "Playwright", new PageTypeOptions
        {
            Delay = 100 // 100ms entre cada tecla
        });

        var value = await _page.InputValueAsync("#typewriter");

        // Assert
        Assert.Equal("Playwright", value);
    }

    [Fact]
    public async Task Should_Handle_Multiple_Select()
    {
        // Arrange
        await _page.SetContentAsync(@"
            <html>
                <body>
                    <select id='skills' multiple>
                        <option value='csharp'>C#</option>
                        <option value='javascript'>JavaScript</option>
                        <option value='python'>Python</option>
                        <option value='java'>Java</option>
                    </select>
                </body>
            </html>
        ");

        // Act
        await _page.SelectOptionAsync("#skills", new[] { "csharp", "python" });
        var selectedValues = await _page.EvaluateAsync<string[]>(@"
            Array.from(document.getElementById('skills').selectedOptions)
                .map(option => option.value)
        ");

        // Assert
        Assert.Equal(2, selectedValues.Length);
        Assert.Contains("csharp", selectedValues);
        Assert.Contains("python", selectedValues);
    }
}
