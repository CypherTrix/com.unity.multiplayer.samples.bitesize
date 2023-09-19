using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance { get { if (_instance == null) { Debug.LogError("GameManager is NULL"); } return _instance; } }

    public GameState State = GameState.None;
    public event Action<GameState> OnGameStateChanged;

    [SerializeField] public ClientPlayerData PlayerData;


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
        if (!LoadPlayerData()) SavePlayerData();
    }

    private void Update() {
        PlayerData.PlayTime += Time.deltaTime;
    }

    private void OnApplicationQuit() {
        //PlayerData.PlayTime += Time.realtimeSinceStartup;
        SaveManager.SaveData(SaveManager.PLAYER_SAVE_FILE_NAME, PlayerData);
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
    #region PlayerData
    [ContextMenu("SavePlayerData")]
    public void SavePlayerData() {
        if (PlayerData.PlayerName != "") {
            SaveManager.SaveData(SaveManager.PLAYER_SAVE_FILE_NAME, PlayerData);
            Debug.Log("GameManager : PlayerData saved to File");
        } else {
            Debug.Log("GameManager : Player Struct is not initialized, PlayerData could not save to File");
        }
    }
    [ContextMenu("LoadPlayerData")]
    public bool LoadPlayerData() {
        if (SaveManager.SaveFileExsis(SaveManager.PLAYER_SAVE_FILE_NAME) && SaveManager.LoadData(SaveManager.PLAYER_SAVE_FILE_NAME, out ClientPlayerData loadedPlayerData)) {
            PlayerData = loadedPlayerData;
            Debug.Log("GameManager : PlayerData loaded from File");
            return true;
        } else {
            Debug.LogWarning("GameManager : PlayerData could not load from File");
            return false;
        }
    }
    [ContextMenu("ResetPlayerData")]
    public void ResetPlayerData() {
        PlayerData = new(RandomPlayerGenerator.GetRandomName(), RandomPlayerGenerator.GetRandomColor());
    }
    [ContextMenu("DeletePlayerData")]
    public void DeletePlayerData() {
        SaveManager.DeleteData(SaveManager.PLAYER_SAVE_FILE_NAME);
    }
    [ContextMenu("getkd")]
    public void getkd() {
        Debug.Log(PlayerData.GetPlayerKD());
    }


    #endregion

}

public enum GameState : int {
    None,
    Spawn,
    Lose,
    Win
}
