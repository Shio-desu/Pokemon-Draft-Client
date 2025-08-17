using System.Collections.Concurrent;
using System.Security.Claims;
using DataAccessLibrary.Models;
namespace Pokemon_Draft_Client.Models;

public class User
{
    public required string Username { get; set; }
    public bool IsConnected { get; set; }
    public bool IsReady { get; set; }
    public bool IsOwner { get; set; }
}

public class LobbyUsers
{
    
    public required SessionModel Session { get; set; }
    public ConcurrentDictionary<string, User> UsersDict { get; set; } = [];
    public List<string> PickOrder = new();
    public string CurrentPick = "";
    
    private int PickSequenceDirection = Forwards;

    private const int Forwards = 1; 
    private const int Backwards = -1; 
    
    public bool AddUser(string username, bool isOwner)
    {
        var user = new User
        {
            Username = username,
            IsConnected = true,
            IsOwner = isOwner,
            IsReady = false
        };
        
        PickOrder.Add(username);
        
       return UsersDict.TryAdd(username, user);
    }

    public bool RemoveUser(string username)
    {
        var foundUser = UsersDict.TryRemove(username, out _);
        PickOrder.Remove(username);
        return foundUser;
    }

    public void ProgressCurrentPick()
    {
        int currentIndex = PickOrder.IndexOf(CurrentPick);
        
        if (currentIndex == PickOrder.Count - 1 && PickSequenceDirection == Forwards)
        {
            PickSequenceDirection = Backwards;
        }
        else if (currentIndex == 0 && PickSequenceDirection == Backwards)
        {
            PickSequenceDirection = Forwards;
        }
        else
        {   // only progressing the index if the edge cases above don't apply, so the person in the front and back get a double pick
            currentIndex += PickSequenceDirection;
        }
        
        CurrentPick = PickOrder[currentIndex];
    }
    
    public void RandomizePickOrder()
    {
        Random random = new Random();
        List<string> randomizedPickOrder = [];
        
        for (int i = 0; i < UsersDict.Count; i++)
        {
            int next = random.Next(0, PickOrder.Count);
            randomizedPickOrder.Add(PickOrder[next]);
            PickOrder.RemoveAt(next);
        }
        PickOrder = randomizedPickOrder;
    }
}

