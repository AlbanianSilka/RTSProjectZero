using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class KeybindingsUI : MonoBehaviour
{
    [Header("Keybinding Buttons")]
    [SerializeField] private Button[] spellKeyButtons;
    [SerializeField] private TextMeshProUGUI[] spellKeyTexts;

    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;

    private int waitingForIndex = -1;
    public bool waitingForInput = false;

    private float escapeSuppressTime = 0f;
    public bool IsSuppressingEscape => Time.time < escapeSuppressTime;

    private void Start()
    {
        UpdateKeyLabels();

        for (int i = 0; i < spellKeyButtons.Length; i++)
        {
            int index = i; // local copy for closure
            spellKeyButtons[i].onClick.AddListener(() => StartRebind(index));
        }

        popupPanel.SetActive(false);
    }

    private void Update()
    {
        if (!waitingForInput) return;

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (key == KeyCode.Escape)
                {
                    CancelRebind();
                    return;
                }

                waitingForInput = false;
                popupPanel.SetActive(false);
                KeybindingManager.SpellKeys[waitingForIndex] = key;
                UpdateKeyLabels();
                return;
            }
        }
    }

    private void StartRebind(int index)
    {
        waitingForIndex = index;
        waitingForInput = true;
        popupText.text = $"Press a key for Spell {index + 1} or ESC to cancel.";
        popupPanel.SetActive(true);
    }

    private void CancelRebind()
    {
        waitingForInput = false;
        waitingForIndex = -1;
        popupPanel.SetActive(false);

        // short delay so escape won't close the whole settings menu
        escapeSuppressTime = Time.time + 0.1f;
    }

    private void UpdateKeyLabels()
    {
        for (int i = 0; i < spellKeyTexts.Length; i++)
        {
            spellKeyTexts[i].text = $"Spell {i + 1}: {KeybindingManager.SpellKeys[i]}";
        }
    }
}
