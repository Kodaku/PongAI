using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using HashedState = System.Tuple<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2>;

[System.Serializable]
public class Model
{
    private List<PlayerState> playerStates;

    public Model()
    {
        playerStates = new List<PlayerState>();
    }
    
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
        foreach(PlayerState playerState in playerStates)
        {
            playerState.GenerateHashCode();
            StateDictionary.AddPlayerState(playerState);
        }
    }

    public void Update(HashedState currentHashedState, Action currentAction, HashedState nextHashedState, float reward)
    {
        PlayerState currentState = StateDictionary.GetPlayerState(currentHashedState);
        PlayerState nextState = StateDictionary.GetPlayerState(nextHashedState);
        currentState.SetNextStateAndReward(currentAction, nextState, reward);
    }

    public Tuple<PlayerState, PlayerState, float, float> GetRandomPlayerStateAndReward()
    {
        PlayerState simulatedState = StateDictionary.GetRandomPlayerState();
        return simulatedState.GetRandomNextStateAndReward();
    }

    public void IncreaseAllTau()
    {
        foreach(PlayerState playerState in playerStates)
        {
            playerState.IncreaseTau();
        }
    }

    public void ResetTau(HashedState hashedState, Action currentAction)
    {
        PlayerState playerState = StateDictionary.GetPlayerState(hashedState);
        playerState.ResetTau(currentAction);
    }
}
