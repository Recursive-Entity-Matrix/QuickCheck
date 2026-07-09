using System.Text.RegularExpressions;
using QuickCheck.Models;
using QuickCheck.Util;

namespace QuickCheck.Interfaces;

public class GoogleDocProvider : IListProvider
{
    private static readonly HttpClient HttpClient = new HttpClient();
        
    // Regex to extract the Document ID. 
    // It looks for the /d/ and captures all valid ID characters (alphanumeric, dashes, underscores) until the next slash or end of string.
    private static readonly Regex DocIdRegex = new Regex(@"docs\.google\.com\/document\/d\/([a-zA-Z0-9_-]+)", RegexOptions.Compiled);
    
    public async Task<List<VIPPlayer>> GetPlayers(string providerUrl)
    {
        if (DocIdRegex.Match(providerUrl) is not { Success: true } match)
        {
            throw new ArgumentException("Invalid Google Docs URL.");
        }
        
        var docId = match.Groups[1].Value;
        
        var exportUrl = $"https://docs.google.com/document/d/{docId}/export?format=txt";

        var players = new List<VIPPlayer>();

        try
        {
            var response = await HttpClient.GetAsync(exportUrl);
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
                        $"Invalid line format in Google Doc: {line}. Expected format: 'Player Name@HomeWorld'.");
                }
            }
        }
        catch (HttpRequestException e)
        {
            Services.Log.Error($"HTTP Error from Google Docs: {e.Message}");
        }
        catch (Exception e)
        {
            Services.Log.Error($"Unexpected error while fetching players from Google Docs: {e.Message}");
        }
        
        return players;
    }
}