using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Data;
using ISelectable = Interfaces.ISelectable;
using static Resource;

public class RTS_building : CombatEntity, IAttackable, ISelectable
{
    public Sprite canBuild;
    public Sprite cannotBuild;
    public float maxHealth;
    public virtual float health { get; set; } = 30f;
    public bool finished;
    public healthbar_manager healthBar;
    public List<SpellSO> assignedSpells = new();
    public SpellSO cancelSpell;
    public List<UnitRTS> unitsQueue;
    public float remainingSpawnTime = 0f;

    protected virtual float attackSpeed { get; set; } = 0f;
    protected override float attackDamage => 0f;
    protected override float arrowSpeed => 0f;
    protected virtual float attackRange { get; set; } = 0f;
    protected virtual bool isAttacking { get; set; }
    protected Dictionary<ResourceType, int> RequiredResources { get; set; }
    protected RTS_controller rtsController;

    private bool makingUnit;

    public SelectableData OnSelect()
    {
        SelectableData data = new SelectableData()
        {
            Spells = assignedSpells,
            ShowBuildingUI = true
        };
        return data;
    }


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
        if (this == null || health <= 0)
            return;

        health -= damage;
        this.healthBar.updateHealthBar(this.health, this.maxHealth);
        Debug.Log($"{this.name} was hit by {attacker.name}");

        if(attackDamage > 0)
        {
            GameObject counterAttackTarget = attacker;
            StartCoroutine(buildingFight(counterAttackTarget.GetComponent<IAttackable>(), counterAttackTarget));
        }

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
            if (!(owner is BotPlayer))
            {
                UI_controller.handleMiddle(unitsQueue);
            }
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
                UI_controller.handleMiddle(unitsQueue);
            }
        } else
        {
            Debug.LogError("SpawnMarker not found!");
        }
    }

    private IEnumerator buildingFight(IAttackable target, GameObject targetObject)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        while (target.health > 0) {
            float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);

            if (distanceToTarget > attackRange)
            {
                isAttacking = false;
                yield break;
            }

            if (distanceToTarget <= attackRange)
            {
                StartCoroutine(ShootArrow(target, targetObject));

                yield return new WaitForSeconds(attackSpeed);
            }
        }
    }
}
