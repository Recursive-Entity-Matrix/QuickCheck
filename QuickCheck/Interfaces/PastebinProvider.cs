using System.Text.RegularExpressions;
using QuickCheck.Models;
using QuickCheck.Util;

namespace QuickCheck.Interfaces;

public class PastebinProvider : IListProvider
{
    public PastebinProvider()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "QuickCheck-Plugin/1.0");
    }
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly Regex _pastebinRegex = new Regex(@"pastebin\.com\/(?:raw\/)?([a-zA-Z0-9]+)", RegexOptions.Compiled);
    public async Task<List<VIPPlayer>> GetPlayers(string providerUrl)
    {
        if (_pastebinRegex.Match(providerUrl) is not { Success: true } match)
        {
            throw new ArgumentException("Invalid Pastebin URL.");
        }
        
        var pasteId = match.Groups[1].Value;
        
        string exportUrl = $"https://pastebin.com/raw/{pasteId}";

        var players = new List<VIPPlayer>();

        try
        {
            var response = await _httpClient.GetAsync(exportUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var split = line.Split('@');
                if (split.Length == 2)
                {
                    var player = new VIPPlayer()
                    {
                        Name = split[0].Trim(),
                        HomeWorld = ServerHelper.GetIdFromName(split[1].Trim()),
                    };
                    players.Add(player);
                }
                else
                {
                    Services.Log.Warning(
                        $"Invalid line format in Pastebin: {line}. Expected format: 'Player Name@HomeWorld'.");
                }
            }
        }
        catch (HttpRequestException e)
        {
            Services.Log.Error($"HTTP Error from Pastebin: {e.Message}");
        }
        catch (Exception e)
        {
            Services.Log.Error($"Unexpected error while fetching players from Pastebin: {e.Message}");
        }
        
        return players;
    }
}