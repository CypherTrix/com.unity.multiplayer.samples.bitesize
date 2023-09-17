using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance { get { if (_instance == null) { Debug.LogError("GameManager is NULL"); } return _instance; } }

    public GameState State = GameState.None;
    public static event Action<GameState> OnGameStateChanged;

    [SerializeReference]
    public static ClientPlayerData PlayerData;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        PlayerData = new(RandomPlayerGenerator.GetRandomName(), RandomPlayerGenerator.GetRandomColor());
    }
    private void Start() {

        if (SaveManager.SaveFileExsis(SaveManager.PLAYER_SAVE_FILE_NAME) && SaveManager.LoadData(SaveManager.PLAYER_SAVE_FILE_NAME, out ClientPlayerData loadedPlayerData)) {
            Debug.Log("Save File Loaded");
        } else {
            SaveManager.SaveData(SaveManager.PLAYER_SAVE_FILE_NAME, PlayerData);
            Debug.Log("Save File Saved");
        }
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
