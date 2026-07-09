using QuickCheck.Models;

namespace QuickCheck.Interfaces;

public interface IListProvider
{
    public Task<List<VIPPlayer>> GetPlayers(string providerUrl);
}