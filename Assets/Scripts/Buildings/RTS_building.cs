using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Resource;

public class RTS_building : MonoBehaviour, IAttackable
{
    public Sprite canBuild;
    public Sprite cannotBuild;
    public float maxHealth;
    public virtual float health { get; set; } = 30f;
    public bool finished;
    public string team;
    public healthbar_manager healthBar;
    public List<GameObject> spellButtons = new List<GameObject>();
    public List<UnitRTS> unitsQueue;
    public event Action<RTS_building> OnDeath;
    public float remainingSpawnTime = 0f;
    public Player owner;

    protected Dictionary<ResourceType, int> RequiredResources { get; set; }
    protected RTS_controller rtsController;

    private bool makingUnit;

    public RTS_building()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 0 },
            { ResourceType.Wood, 0 }
        };
    }

    public Dictionary<ResourceType, int> GetRequiredResources()
    {
        return RequiredResources;
    }

    private void Awake()
    {
        healthBar = GetComponentInChildren<healthbar_manager>();
        makingUnit = false;
        InvokeRepeating("CheckAndSpawnUnits", 0f, 1f);
    }

    private void Start()
    {
        rtsController = owner.rtsController;
    }

    public bool CanBuild(Player owner)
    {
        foreach (var kvp in RequiredResources)
        {
            if (!owner.HasEnoughResources(kvp.Key, kvp.Value))
            {
                return false;
            }
        }
        return true;
    }

    public void TakeDamage(float damage, GameObject attacker)
    {
        // attacker argument need for future if I'll make buildings that would attack other units

        health -= damage;
        this.healthBar.updateHealthBar(this.health, this.maxHealth);
        Debug.Log($"{this.name} was hit by {attacker.name}");

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
            this.owner.ChangePlayerResources(unit.GetRequiredResources(), "-");
            unitsQueue.Add(unit);
            UI_controller.handleMiddleSection(unitsQueue, rtsController.progressButtonPrefab);
        }
    }

    public void restartSpawnCoroutine()
    {
        if (makingUnit)
        {
            StopAllCoroutines();

            if(unitsQueue.Count > 0)
            {
                UnitRTS unit = unitsQueue[0];
                StartCoroutine(SpawnUnitAfterDelay(unit));
            } else
            {
                makingUnit = false;
            }
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
        remainingSpawnTime = unit.spawnTime;

        while (remainingSpawnTime > 0)
        {
            remainingSpawnTime -= Time.deltaTime;
            yield return null;
        }

        Transform spawnMarker = this.transform.Find("SpawnMarker");
        if(spawnMarker != null)
        {
            Vector2 spawnPosition = spawnMarker.transform.position;
            UnitRTS newUnit = Instantiate(unit, spawnPosition, Quaternion.identity);
            unitsQueue.RemoveAt(0);
            newUnit.team = this.team;
            newUnit.owner = this.owner;
            makingUnit = false;

            if (rtsController.selectedBuilding == this)
            {
                if(this.GetComponent<GoldenMine>() != null)
                {
                    UI_controller.handleMiddleSection(unitsQueue, rtsController.workerButtonPrefab);
                }
                else {
                    UI_controller.handleMiddleSection(unitsQueue, rtsController.progressButtonPrefab);
                }
            }
        } else
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
