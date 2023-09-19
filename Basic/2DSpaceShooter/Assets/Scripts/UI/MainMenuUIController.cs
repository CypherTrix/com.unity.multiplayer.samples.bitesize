using System;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIController : MonoBehaviour {

    [Header("UI Elements")]
    private UIDocument mainDoc;

    //Main Menu
    private Button playBtn;
    private Button profilBtn;
    private Button settingsBtn;
    private Button exitBtn;
    //Settings
    [Header("Settings")]
    [SerializeField] AudioMixer audioMixer;
    private VisualElement settingsMenu;
    private Slider masterVolume;
    private Label masterVolumeLabel;
    private const string MASTER_VOLUME = "Master_Volume";
    private Slider masterMusic;
    private Label masterMusicLabel;
    private const string MASTER_MUSIC = "Master_Music";
    private Slider masterSound;
    private Label masterSoundLabel;
    private const string MASTER_SOUND = "Master_Sound";
    private RadioButtonGroup graphicsSelection;
    private const string GRAPHIC_QUALITY = "Graphic_Quality";
    //Profil
    private VisualElement profilMenu;
    private Label playerStats;
    private Label playerStatsValue;
    private Button resetPlayer;
    private VisualElement playerShipImage;
    private VisualElement playerShipColor;
    private EnumField enumColors;
    private const string PLAYER_COLOR = "Player_Color";
    private TextField playerName;
    private const string PLAYER_NAME = "Player_Name";



    private void Awake() {
        mainDoc = GetComponent<UIDocument>();
        //MainMenu
        playBtn = mainDoc.rootVisualElement.Q<Button>("BtnPlay");
        playBtn.clicked += PlayBtn_clicked;
        profilBtn = mainDoc.rootVisualElement.Q<Button>("BtnProfil");
        profilBtn.clicked += ProfilBtn_clicked;
        settingsBtn = mainDoc.rootVisualElement.Q<Button>("BtnSettings");
        settingsBtn.clicked += SettingsBtn_clicked;
        exitBtn = mainDoc.rootVisualElement.Q<Button>("BtnExit");
        exitBtn.clicked += ExitBtn_clicked;
        //Settings Menu
        settingsMenu = mainDoc.rootVisualElement.Q<VisualElement>("SettingsMenu");
        masterVolume = mainDoc.rootVisualElement.Q<Slider>("SliderMasterVolume");
        masterVolumeLabel = mainDoc.rootVisualElement.Q<Label>("LblMasterVolume");
        masterMusic = mainDoc.rootVisualElement.Q<Slider>("SliderMusicVolume");
        masterMusicLabel = mainDoc.rootVisualElement.Q<Label>("LblMusicVolume");
        masterSound = mainDoc.rootVisualElement.Q<Slider>("SliderSoundVolume");
        masterSoundLabel = mainDoc.rootVisualElement.Q<Label>("LblSoundVolume");
        resetPlayer = mainDoc.rootVisualElement.Q<Button>("ResetPlayer");
        resetPlayer.clicked += ResetPlayer_onClick;
        graphicsSelection = mainDoc.rootVisualElement.Q<RadioButtonGroup>("GraphicsSelection");

        //Profil Menu
        profilMenu = mainDoc.rootVisualElement.Q<VisualElement>("ProfilMenu");
        playerStats = mainDoc.rootVisualElement.Q<Label>("PlayerStats");
        playerStatsValue = mainDoc.rootVisualElement.Q<Label>("PlayerStatsValue");

        playerShipColor = mainDoc.rootVisualElement.Q<VisualElement>("PlayerShipColor");
        playerShipImage = mainDoc.rootVisualElement.Q<VisualElement>("PlayerShipImage");
        enumColors = mainDoc.rootVisualElement.Q<EnumField>("ColorsEnum");
        playerName = mainDoc.rootVisualElement.Q<TextField>("PlayerName");

        enumColors.Init(PlayerColors.PlayerColorNames.White);
    }

    private void ResetPlayer_onClick() {
        PlayerPrefs.DeleteAll();
    }

    private void Start() {
        //Apply Settings to UI ELements
        masterVolume.RegisterValueChangedCallback(evt => masterVolumeLabel.text = $"{Mathf.FloorToInt(evt.newValue * 100)} %");
        masterVolume.RegisterValueChangedCallback(evt => audioMixer.SetFloat(MASTER_VOLUME, Mathf.Log(evt.newValue) * 20));
        masterVolume.RegisterValueChangedCallback(evt => PlayerPrefs.SetFloat(MASTER_VOLUME, evt.newValue));
        masterVolume.value = PlayerPrefs.HasKey(MASTER_VOLUME) ? PlayerPrefs.GetFloat(MASTER_VOLUME) : 1;

        masterMusic.RegisterValueChangedCallback(evt => masterMusicLabel.text = $"{Mathf.FloorToInt(evt.newValue * 100)} %");
        masterMusic.RegisterValueChangedCallback(evt => audioMixer.SetFloat(MASTER_MUSIC, Mathf.Log(evt.newValue) * 20));
        masterMusic.RegisterValueChangedCallback(evt => PlayerPrefs.SetFloat(MASTER_MUSIC, evt.newValue));
        masterMusic.value = PlayerPrefs.HasKey(MASTER_MUSIC) ? PlayerPrefs.GetFloat(MASTER_MUSIC) : 1;

        masterSound.RegisterValueChangedCallback(evt => masterSoundLabel.text = $"{Mathf.FloorToInt(evt.newValue * 100)} %");
        masterSound.RegisterValueChangedCallback(evt => audioMixer.SetFloat(MASTER_SOUND, Mathf.Log(evt.newValue) * 20));
        masterSound.RegisterValueChangedCallback(evt => PlayerPrefs.SetFloat(MASTER_SOUND, evt.newValue));
        masterSound.value = PlayerPrefs.HasKey(MASTER_SOUND) ? PlayerPrefs.GetFloat(MASTER_SOUND) : 1;

        graphicsSelection.SetValueWithoutNotify(PlayerPrefs.HasKey(GRAPHIC_QUALITY) ? PlayerPrefs.GetInt(GRAPHIC_QUALITY) : 1); //Default Level 'Normal'
        graphicsSelection.RegisterValueChangedCallback(evt => QualitySettings.SetQualityLevel(evt.newValue));
        graphicsSelection.RegisterValueChangedCallback(evt => PlayerPrefs.SetInt(GRAPHIC_QUALITY, evt.newValue));

        //Profil
        StringBuilder statsBuilder = new();
        StringBuilder statsValueBuilder = new();
        statsBuilder.AppendLine($"Shoots Fired");
        statsValueBuilder.AppendLine($"{GameManager.Instance.PlayerData.ShootsFired}x");
        statsBuilder.AppendLine("Kills");
        statsValueBuilder.AppendLine($"{GameManager.Instance.PlayerData.Kills}x");
        statsBuilder.AppendLine($"Deaths");
        statsValueBuilder.AppendLine($"{GameManager.Instance.PlayerData.Deaths}x");
        statsBuilder.AppendLine($"K/D Ratio");
        statsValueBuilder.AppendLine($"{GameManager.Instance.PlayerData.GetPlayerKD()}");
        statsBuilder.AppendLine($"Used PowerUps");
        statsValueBuilder.AppendLine($"{GameManager.Instance.PlayerData.PowerUpsCollected}x");
        statsBuilder.AppendLine($"Play Time");
        statsValueBuilder.AppendLine($"{GameManager.Instance.PlayerData.GetTimePlayed()}");
        playerStats.text = statsBuilder.ToString();
        playerStatsValue.text = statsValueBuilder.ToString();

        // enumColors.RegisterValueChangedCallback(evt => playerShipColor.style.unityBackgroundImageTintColor = PlayerColors.ToColor(evt.newValue.ToString()));
        enumColors.RegisterValueChangedCallback(evt => {
            Enum.TryParse(evt.newValue.ToString(), true, out PlayerColors.PlayerColorNames playerColorName);
            PlayerPrefs.SetString(PLAYER_COLOR, "#" + ColorUtility.ToHtmlStringRGB(PlayerColors.GetPlayerColor(playerColorName)));
            playerShipColor.style.unityBackgroundImageTintColor = PlayerColors.GetPlayerColor(playerColorName);
        });
        //enumColors.RegisterValueChangedCallback(evt => PlayerPrefs.SetString(PLAYER_COLOR,ColorUtility.ToHtmlStringRGBA(PlayerColors.ToColor(evt.newValue.ToString()))));
        if (PlayerPrefs.HasKey(PLAYER_COLOR)) {
            ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(PLAYER_COLOR), out Color parseColor);
            PlayerColors.PlayerColorNames playerColorName = PlayerColors.GetPlayerColorNameByColor(parseColor);
            enumColors.value = playerColorName;
        } else {
            //On First Startup set Color
            PlayerPrefs.SetString(PLAYER_COLOR, "#" + ColorUtility.ToHtmlStringRGB(Color.white));
        }

        playerName.RegisterValueChangedCallback(evt => PlayerPrefs.SetString(PLAYER_NAME, evt.newValue));
        playerName.value = PlayerPrefs.HasKey(PLAYER_NAME) ? PlayerPrefs.GetString(PLAYER_NAME) : Environment.UserName;
    }

    #region MainMenu
    private void PlayBtn_clicked() {
        SceneManager.LoadScene(1);
    }

    private void ProfilBtn_clicked() {
        settingsMenu.style.display = DisplayStyle.None;
        profilMenu.style.display = profilMenu.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;

    }
    private void SettingsBtn_clicked() {
        profilMenu.style.display = DisplayStyle.None;
        settingsMenu.style.display = settingsMenu.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }
    private void ExitBtn_clicked() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    #endregion




}
