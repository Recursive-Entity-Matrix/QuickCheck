using System.Globalization;
using System.Text.RegularExpressions;
using QuickCheck.Models;
using CsvHelper;
using QuickCheck.Util;

namespace QuickCheck.Interfaces;

public class GoogleSheetsProvider : IListProvider
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    // Regex to extract the Document ID. 
    // It looks for the /d/ and captures all valid ID characters (alphanumeric, dashes, underscores) until the next slash or end of string.
    private readonly Regex _docIdRegex = new Regex(@"docs\.google\.com\/spreadsheets\/d\/([a-zA-Z0-9_-]+)", RegexOptions.Compiled);
    
    public async Task<List<VIPPlayer>> GetPlayers(string providerUrl)
    {
        if (_docIdRegex.Match(providerUrl) is not { Success: true } match)
        {
            throw new ArgumentException("Invalid Google Sheets URL.");
        }
        
        var sheetId  = match.Groups[1].Value;
        
        var exportUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv";
        
        var players = new List<VIPPlayer>();

        try
        {
            var response = await _httpClient.GetAsync(exportUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            using var reader = new StringReader(content);
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();
            foreach (var row in records)
            {
                var dict = (IDictionary<string, object>)row;
                if (dict.TryGetValue("Player Name", out var name) && name is string playerName)
                {
                    var split = playerName.Split('@');
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
                            $"Invalid line format in Google Sheets: {playerName}. Expected format: 'Player Name@HomeWorld'.");
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Services.Log.Error($"HTTP Error from Google Sheets: {e.Message}");
        }
        catch (Exception e)
        {
            Services.Log.Error($"Unexpected error while fetching players from Google Sheets: {e.Message}");
        }
        
        return players;
    }
}