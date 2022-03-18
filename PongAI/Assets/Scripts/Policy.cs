using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HashedState = System.Tuple<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2>;
[System.Serializable]
public class Policy
{
    List<PlayerState> policy = new List<PlayerState>();

    public bool ContainsState(HashedState hashedState)
    {
        return StateDictionary.ContainsState(hashedState);
    }

    public void AddPlayerState(PlayerState playerState)
    {
        StateDictionary.AddPlayerState(playerState);
    }

    public void InitializeStateDictionary()
    {
        foreach(PlayerState playerState in policy)
        {
            playerState.GenerateHashCode();
            StateDictionary.AddPlayerState(playerState);
        }
    }

    public void EpsilonGreedyUpdate(HashedState hashedState, Action bestAction)
    {
        PlayerState playerState = StateDictionary.GetPlayerState(hashedState);
        playerState.UpdateProbabilities(bestAction);
    }

    public Action ChooseAction(HashedState hashedState)
    {
        PlayerState playerState = StateDictionary.GetPlayerState(hashedState);
        return playerState.ChooseAction();
    }
}
