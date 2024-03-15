using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTS_building : MonoBehaviour
{
    public Sprite canBuild;
    public Sprite cannotBuild;
    public float maxHealth;
    public float health;
    public bool finished;
    public healthbar_manager healthBar;
    public List<GameObject> spellButtons = new List<GameObject>();

    private void Awake()
    {
        healthBar = GetComponentInChildren<healthbar_manager>();
    }
}
