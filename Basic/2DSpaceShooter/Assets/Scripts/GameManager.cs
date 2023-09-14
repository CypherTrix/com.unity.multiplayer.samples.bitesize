using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance { get { if (_instance == null) { Debug.LogError("GameManager is NULL"); } return _instance; } }

    public GameState State = GameState.None;
    public static event Action<GameState> OnGameStateChanged;
    private void Awake() {
        _instance = this;
    }

    public void UpdateGameState(GameState newState) {
        if (State == newState) return;
        State = newState;
        switch (newState) {
            case GameState.Spawn:
                break;
            case GameState.Lose:
                break;
            case GameState.Win:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);

        }
        OnGameStateChanged?.Invoke(newState);
    }
    
}

public enum GameState : int {
    None,
    Spawn,
    Lose,
    Win
}
