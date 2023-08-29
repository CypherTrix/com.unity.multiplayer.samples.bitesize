using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenuUIController : MonoBehaviour {
    [Header("Mute Audio Button")]
    [SerializeField] AudioMixer audioMixer;

    [Header("UI Elements")]
    private UIDocument mainDoc;

    //Main Menu
    private Button playBtn;
    private Button profilBtn;
    private Button settingsBtn;
    private Button exitBtn;
    //Settings
    private VisualElement settingsMenu;
    private Button backBtnSettings;


    [Header("Settings")]
    private float masterVolume = 1f;
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
        backBtnSettings = mainDoc.rootVisualElement.Q<Button>("BtnBack");
        backBtnSettings.clicked += BackBtnSettings_clicked;


        profilBtn.SetEnabled(false);
    }

    private void BackBtnSettings_clicked() {
        settingsMenu.style.display = DisplayStyle.None;
        settingsBtn.SetEnabled(true);
    }


    #region MainMenu
    private void PlayBtn_clicked() {
        SceneManager.LoadScene(1);
    }

    private void ProfilBtn_clicked() {

    }
    private void SettingsBtn_clicked() {
        settingsMenu.style.display = DisplayStyle.Flex;
        settingsBtn.SetEnabled(false);
    }
    private void ExitBtn_clicked() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    #endregion

    #region Settings
    private void MuteBtn_onClick() {
       //isMuted = !isMuted;
       // StyleBackground bg = muteBtn.style.backgroundImage;
       // bg.value = Background.FromSprite(isMuted ? muteSprite : unmuteSprite);
       // muteBtn.style.backgroundImage = bg;

       // audioMixer.SetFloat("Master_Volume", isMuted ? -80 : 0);
    }
    #endregion



}
