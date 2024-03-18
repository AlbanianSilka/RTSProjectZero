using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RTS_building : MonoBehaviour
{
    public Sprite canBuild;
    public Sprite cannotBuild;
    public float maxHealth;
    public float health;
    public bool finished;
    public string team;
    public healthbar_manager healthBar;
    public List<GameObject> spellButtons = new List<GameObject>();
    public List<UnitRTS> unitsQueue;
    public Canvas buildingCanvas;
    public event Action<RTS_building> OnDeath;

    private bool makingUnit;

    private void Awake()
    {
        healthBar = GetComponentInChildren<healthbar_manager>();
        makingUnit = false;
        InvokeRepeating("CheckAndSpawnUnits", 0f, 1f);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        this.healthBar.updateHealthBar(this.health, this.maxHealth);
        Debug.Log($"{this.name} was hit");

        if (health <= 0)
        {
            Die();
        }
    }

    public void AddUnitToQueue(UnitRTS unit)
    {
        if(unitsQueue.Count < 10)
        {
            unitsQueue.Add(unit);
        }
    }

    private void CheckAndSpawnUnits()
    {
        if (unitsQueue.Count > 0 && !makingUnit)
        {
            UnitRTS unit = unitsQueue[0];

            makingUnit = true;
            StartCoroutine(SpawnUnitAfterDelay(unit));

            unitsQueue.RemoveAt(0);
        }
    }

    private IEnumerator SpawnUnitAfterDelay(UnitRTS unit)
    {
        yield return new WaitForSeconds(unit.spawnTime);

        Transform spawnMarker = this.transform.Find("SpawnMarker");

        if (spawnMarker != null)
        {
            Vector2 spawnPosition = spawnMarker.transform.position;
            UnitRTS newUnit = Instantiate(unit, spawnPosition, Quaternion.identity);
            newUnit.team = this.team;
            makingUnit = false;
        }
        else
        {
            Debug.LogError("SpawnMarker not found!");
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);

        Destroy(gameObject);
    }
}
