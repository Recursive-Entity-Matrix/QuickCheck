using System.Text.RegularExpressions;
using QuickCheck.Models;
using QuickCheck.Util;

namespace QuickCheck.Interfaces;

public class GitHubGistProvider : IListProvider
{
    private readonly HttpClient _httpClient = new();

    // Pattern captures: Group 1 = Username, Group 2 = Gist ID
    // Works for: 
    // - https://gist.github.com/username/8a7b6c5d4e3f2a1b
    // - https://gist.githubusercontent.com/username/8a7b6c5d4e3f2a1b/raw/...
    private readonly Regex _gistRegex = new Regex(
        @"gist\.github(?:usercontent)?\.com\/([a-zA-Z0-9_-]+)\/([a-fA-F0-9]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public async Task<List<VIPPlayer>> GetPlayers(string providerUrl)
    {
        if (_gistRegex.Match(providerUrl) is not { Success: true } match)
        {
            throw new ArgumentException("Invalid GitHub Gist URL.");
        }

        var username = match.Groups[1].Value;
        var gistId = match.Groups[2].Value;

        var rawUrl = $"https://gist.githubusercontent.com/{username}/{gistId}/raw/";

        var players = new List<VIPPlayer>();

        try
        {
            var response = await _httpClient.GetAsync(rawUrl);
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
                        $"Invalid line format in GitHub Gist: {line}. Expected format: 'Player Name@HomeWorld'.");
                }
            }
        }
        catch (HttpRequestException e)
        {
            Services.Log.Error($"HTTP Error from GitHub Gist: {e.Message}");
        }
        catch (Exception e)
        {
            Services.Log.Error($"Unexpected error while fetching players from GitHub Gist: {e.Message}");
        }

        return players;
    }
}