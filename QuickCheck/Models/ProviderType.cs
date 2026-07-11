namespace QuickCheck.Models;

public enum ProviderType
{
    None = 0,
    GoogleDoc = 1,
    GoogleSheet = 2,
    Pastebin = 3,
    GitHubGist = 4,
}

public class ProviderTypeHelper
{
    public static string GetProviderPretty(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.None => "None",
            ProviderType.GoogleDoc => "Google Doc",
            ProviderType.GoogleSheet => "Google Sheet",
            ProviderType.Pastebin => "Pastebin",
            ProviderType.GitHubGist => "GitHub Gist",
            _ => "Unknown"
        };
    }
}