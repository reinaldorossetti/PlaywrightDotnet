using Microsoft.Playwright;

namespace PlaywrightTestsXunit3.PageObjects;

/// <summary>
/// Page Object representando a página inicial do Playwright
/// Demonstra o padrão Page Object Model (POM) para organizar testes
/// </summary>
public class PlaywrightHomePage
{
    private readonly IPage _page;
    private const string Url = "https://playwright.dev";

    public PlaywrightHomePage(IPage page)
    {
        _page = page;
    }

    #region Locators

    private ILocator GetStartedButton => _page.GetByRole(AriaRole.Link, new() { Name = "Get started" });
    private ILocator DocsLink => _page.GetByRole(AriaRole.Link, new() { Name = "Docs" });
    private ILocator ApiLink => _page.GetByRole(AriaRole.Link, new() { Name = "API" });
    private ILocator SearchButton => _page.Locator("button[aria-label='Search']");
    private ILocator MainHeading => _page.Locator("h1").First;
    private ILocator NavigationBar => _page.Locator("nav");

    #endregion

    #region Navigation Actions

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(Url);
    }

    public async Task ClickGetStartedAsync()
    {
        await GetStartedButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickDocsAsync()
    {
        await DocsLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickApiAsync()
    {
        await ApiLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion

    #region Getters

    public async Task<string?> GetMainHeadingTextAsync()
    {
        return await MainHeading.TextContentAsync();
    }

    public async Task<string> GetPageTitleAsync()
    {
        return await _page.TitleAsync();
    }

    public string GetCurrentUrl()
    {
        return _page.Url;
    }

    #endregion

    #region Assertions Helpers

    public async Task<bool> IsGetStartedVisibleAsync()
    {
        return await GetStartedButton.IsVisibleAsync();
    }

    public async Task<bool> IsNavigationBarVisibleAsync()
    {
        return await NavigationBar.IsVisibleAsync();
    }

    public async Task<bool> HasLoadedCorrectlyAsync()
    {
        var title = await GetPageTitleAsync();
        var isGetStartedVisible = await IsGetStartedVisibleAsync();
        var isNavVisible = await IsNavigationBarVisibleAsync();

        return title.Contains("Playwright") && isGetStartedVisible && isNavVisible;
    }

    #endregion

    #region Wait Methods

    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task WaitForNavigationBarAsync()
    {
        await NavigationBar.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });
    }

    #endregion
}
