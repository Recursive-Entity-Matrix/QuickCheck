using QuickCheck.Interfaces;

namespace QuickCheck.Models;

public class CheckList
{
    public string Name { get; set; } = "My New List";
    public string Description { get; set; } = string.Empty;
    public string? ProviderUrl { get; set; }
    public ProviderType ProviderType { get; set; } = ProviderType.None;
    public List<VIPPlayer> VipPlayers { get; private set; } = [];

    public async Task UpdateCheckList()
    {
        try
        {
            switch (ProviderType)
            {
                case ProviderType.None:
                    break;
                case ProviderType.GoogleDoc:
                    var googleDocProvider = new GoogleDocProvider();
                    VipPlayers = await googleDocProvider.GetPlayers(ProviderUrl ?? string.Empty);
                    break;
                case ProviderType.GoogleSheet:
                    var googleSheetProvider = new GoogleSheetsProvider();
                    VipPlayers = await googleSheetProvider.GetPlayers(ProviderUrl ?? string.Empty);
                    break;
                case ProviderType.Pastebin:
                    var pastebinProvider = new PastebinProvider();
                    VipPlayers = await pastebinProvider.GetPlayers(ProviderUrl ?? string.Empty);
                    break;
                case ProviderType.GitHubGist:
                    var githubGistProvider = new GitHubGistProvider();
                    VipPlayers = await githubGistProvider.GetPlayers(ProviderUrl ?? string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Services.Log.Error($"Error updating checklist '{Name}': {ex.Message}");
        }
    }
}