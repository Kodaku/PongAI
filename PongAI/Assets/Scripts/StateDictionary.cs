using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HashedState = System.Tuple<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2>;

public class StateDictionary
{
    private static Dictionary<HashedState, PlayerState> stateDictionary = new Dictionary<HashedState, PlayerState>();

    public static void AddPlayerState(PlayerState playerState)
    {
        playerState.GenerateHashCode();
        HashedState hashedState = playerState.GetHashedState();
        if(!stateDictionary.ContainsKey(hashedState))
        {
            stateDictionary.Add(hashedState, playerState);
        }
    }

    public static PlayerState GetPlayerState(HashedState hashedState)
    {
        if(stateDictionary.ContainsKey(hashedState))
        {
            return stateDictionary[hashedState];
        }
        else
        {
            return null;
        }
    }
    public static PlayerState GetRandomPlayerState()
    {
        int totalStates = stateDictionary.Keys.Count;
        int index = UnityEngine.Random.Range(0, totalStates);
        return stateDictionary[stateDictionary.Keys.ToArray<HashedState>()[index]];
    }

    public static bool ContainsState(HashedState hashedState)
    {
        return stateDictionary.ContainsKey(hashedState);
    }
}
