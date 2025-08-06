using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject keybindingsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Settings Menu Buttons")]
    [SerializeField] private KeybindingsUI keybindingsManager;
    [SerializeField] private Button keybindingsButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Main menu
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Settings
        keybindingsButton.onClick.AddListener(OnKeybindingsClicked);
        backButton.onClick.AddListener(OnBackToMainMenu);
    }

    public void ToggleMenu()
    {
        if (keybindingsManager.waitingForInput)
        {
            keybindingsManager.CancelRebind();
            return;
        }

        bool anyOpen = menuPanel.activeSelf || settingsPanel.activeSelf || keybindingsPanel.activeSelf;

        if (anyOpen)
        {
            HideAllMenus();
        }
        else
        {
            menuPanel.SetActive(true);
        }

        // Pause or resume the game
        Time.timeScale = anyOpen ? 1f : 0f;
    }

    private void HideAllMenus()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        keybindingsPanel.SetActive(false);
    }

    private void OnSettingsClicked()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }

    private void OnBackToMainMenu()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OnKeybindingsClicked()
    {
        settingsPanel.SetActive(false);
        keybindingsPanel.SetActive(true);
    }

    public void OnBackFromKeybindings()
    {
        keybindingsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
}
