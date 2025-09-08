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
    public bool finished;
    public SpellSO cancelSpell;
    public List<UnitRTS> unitsQueue;
    public float remainingSpawnTime = 0f;

    protected override float attackSpeed { get; set; } = 0f;
    protected override float attackDamage => 0f;
    protected override float arrowSpeed => 0f;
    protected override float attackRange { get; set; } = 0f;

    private bool makingUnit;

    public override SelectableData OnSelect()
    {
        SelectableData data = base.OnSelect();
        data.ShowBuildingUI = true;
        return data;
    }

    public bool CanBuild(Player owner)
    {
        return CanAffordResources(owner);
    }

    protected override void Awake()
    {
        base.Awake();
        makingUnit = false;
        InvokeRepeating("CheckAndSpawnUnits", 0f, 1f);
    }

    protected override void Start()
    {
        base.Start();
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
}
