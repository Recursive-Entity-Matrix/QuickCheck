using QuickCheck.Models;

namespace QuickCheck.Interfaces;

public interface IListProvider
{
    public List<VIPPlayer> GetPlayers(string providerId);
}