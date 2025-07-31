using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class spell_box : MonoBehaviour
{
    public int boxIndex;

    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _keyLabel;

    private SpellSO _spell;
    private RTS_controller _controller;

    private void Awake()
    {
        Clear();
    }

    private void Update()
    {
        if (_spell == null || _controller == null) return;

        if (boxIndex < KeybindingManager.SpellKeys.Length && Input.GetKeyDown(KeybindingManager.SpellKeys[boxIndex]))
        {
            _spell.Cast(_controller);
        }
    }

    public void Setup(RTS_controller controller, SpellSO spell)
    {
        _spell = spell;
        _controller = controller;

        _button.onClick.RemoveAllListeners();
        if (spell == null)
        {
            Clear();
        }
        else
        {
            _icon.color = Color.white;
            _icon.sprite = spell.icon;
            _button.onClick.AddListener(() => spell.Cast(controller));
            _keyLabel.gameObject.SetActive(true);
        }

        UpdateKeyLabel();
    }

    private void Clear()
    {
        _icon.sprite = null;
        _icon.color = Color.clear;
        _controller = null;
        _spell = null;
        _keyLabel.gameObject.SetActive(false);
    }

    private void UpdateKeyLabel()
    {
        if (_keyLabel != null && boxIndex < KeybindingManager.SpellKeys.Length)
        {
            _keyLabel.text = KeybindingManager.SpellKeys[boxIndex].ToString();
        }
    }
}
