using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spell_box : MonoBehaviour
{
    public int boxIndex;

    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;

    private void Awake()
    {
        Clear();
    }

    public void Setup(RTS_controller controller, SpellSO spell)
    {
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
        }
    }

    private void Clear()
    {
        _icon.sprite = null;
        _icon.color = Color.clear;
    }
}
