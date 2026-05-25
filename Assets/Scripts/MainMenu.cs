using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private InputAction start;
    private InputAction quit;

    [SerializeField]
    private string gameScene;

    [SerializeField]
    private AudioClip mainMenuMusic;

    public static AudioClip mainMenuClip;

    private void Start() {
        MusicManager.instance.SetMusic(mainMenuMusic);
        MusicManager.instance.FadeInMusic(2f);
        mainMenuClip = mainMenuMusic;
    }

    void Update() {
        start = InputSystem.actions.FindAction("Start");
        quit = InputSystem.actions.FindAction("Quit");

        if (start.WasPressedThisFrame()) {
            MusicManager.instance.FadeOutMusic(2f);
            LoadScene(gameScene);
        }
        else if (quit.WasPressedThisFrame()) {
            Quit();
        }
    }

    public static void LoadScene(string name) {
        SceneManager.LoadScene(name);
    }

    private void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }
}