namespace QuickCheck.Models;

public class CheckList
{
    public string Name { get; set; } = "My New List";
    public string Description { get; set; } = string.Empty;
    public string? ProviderUrl { get; set; }
    public ProviderType ProviderType { get; set; } = ProviderType.None;
    public List<VIPPlayer> VipPlayers { get; set; } = [];
}