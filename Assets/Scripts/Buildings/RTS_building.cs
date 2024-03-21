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
    public event Action<RTS_building> OnDeath;

    protected RTS_controller rtsController;

    private bool makingUnit;

    private void Awake()
    {
        rtsController = FindObjectOfType<RTS_controller>();
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
        // currently max queue is 7 unuts (or 7 upgrades but that's for future)

        if(unitsQueue.Count < 7)
        {
            unitsQueue.Add(unit);
            UI_controller.handleMiddleSection(unitsQueue, rtsController.progressButtonPrefab);
        }
    }

    private void CheckAndSpawnUnits()
    {
        if (unitsQueue.Count > 0 && !makingUnit)
        {
            UnitRTS unit = unitsQueue[0];

            makingUnit = true;
            StartCoroutine(SpawnUnitAfterDelay(unit));
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
            unitsQueue.RemoveAt(0);
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
