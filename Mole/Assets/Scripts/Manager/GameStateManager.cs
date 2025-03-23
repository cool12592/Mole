using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class GameStateManager : MonoBehaviour
{
    public PhotonView PV;
    private static GameStateManager instance = null;
    
    public enum GameState {None, Lobby, Ready, Fight, Result};
    public GameState NowGameState { get; private set; }

    public event Action LobbyStateAction;
    public event Action ReadyStateAction;
    public event Action FightStateAction;
    public event Action ResultStateAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NowGameState = GameState.None;
    }

    [PunRPC]
    public void ChangeGameStateForAllUser(GameState gameState)
    {
        PV.RPC("ChangeGameStateRPC", RpcTarget.All, (int)gameState);
    }

    [PunRPC]
    public void ChangeGameStateRPC(int gameState)
    {
        ChangeGameState((GameState)gameState);
    }

    public void ChangeGameState(GameState gameState)
    {
        if (NowGameState != gameState)
        {
            switch (gameState)
            {
                case GameState.Lobby:
                    EnterLobbyState();
                    break;
                case GameState.Ready:
                    EnterReadyState();
                    break;
                case GameState.Fight:
                    EnterFightState();
                    break;
                case GameState.Result:
                    EnterResultState();
                    break;
                default:
                    break;
            }
        }
        NowGameState = gameState;
    }

    private void EnterLobbyState()
    {
        LobbyStateAction?.Invoke();
    }

    private void EnterReadyState()
    {
        ReadyStateAction?.Invoke();
    }

    private void EnterFightState()
    {
        FightStateAction?.Invoke();
    }

    private void EnterResultState()
    {
        ResultStateAction?.Invoke();
    }

}