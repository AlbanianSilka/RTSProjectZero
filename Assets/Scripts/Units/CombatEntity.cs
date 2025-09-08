using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Interfaces;
using static Resource;

public abstract class CombatEntity : MonoBehaviour, IBlocksBuildingPlacement
{
    protected abstract float attackDamage { get; }
    protected abstract float arrowSpeed { get; }

    public Player owner;
    [SerializeField] public string team;
    public event Action<CombatEntity> OnDeath;
    public bool BlocksPlacement() => true;

    // Common health properties
    public virtual float health { get; set; } = 30f;
    public virtual float maxHealth { get; set; } = 30f;
    public ValueBarManager healthBar;

    // Common attack properties
    protected virtual float attackSpeed { get; set; } = 1f;
    protected virtual float attackRange { get; set; } = 3f;
    public virtual bool isAttacking { get; set; }

    // Common resource properties
    protected Dictionary<ResourceType, int> RequiredResources { get; set; }
    protected RTS_controller rtsController;

    // Common spell properties
    public List<SpellSO> assignedSpells = new();

    protected virtual void Awake()
    {
        healthBar = GetComponentInChildren<ValueBarManager>();
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 0 },
            { ResourceType.Wood, 0 }
        };
    }

    protected virtual void Start()
    {
        if (owner != null)
        {
            rtsController = owner.rtsController;
        }
    }

    // Common resource methods
    public Dictionary<ResourceType, int> GetRequiredResources()
    {
        return RequiredResources;
    }

    public bool CanAffordResources(Player player)
    {
        foreach (var kvp in RequiredResources)
        {
            if (!player.HasEnoughResources(kvp.Key, kvp.Value))
            {
                return false;
            }
        }
        return true;
    }

    // Common damage handling
    public virtual void TakeDamage(float damage, GameObject attacker)
    {
        if (this == null || health <= 0)
            return;

        health -= damage;
        
        if (healthBar != null)
        {
            healthBar.updateHealthBar(health, maxHealth);
        }
        
        Debug.Log($"{this.name} was hit by {attacker.name} for {damage} damage and has {health} hp");

        // Counter-attack if this entity can attack
        if (attackDamage > 0)
        {
            GameObject counterAttackTarget = attacker;
            StartCoroutine(CounterAttack(counterAttackTarget.GetComponent<IAttackable>(), counterAttackTarget));
        }

        if (health <= 0)
        {
            Die();
        }
    }

    // Common counter-attack logic
    protected virtual IEnumerator CounterAttack(IAttackable target, GameObject targetObject)
    {
        if (isAttacking || target == null)
            yield break;

        isAttacking = true;

        while (target.health > 0)
        {
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

            yield return null;
        }

        isAttacking = false;
    }

    // Common selectable data method
    public virtual SelectableData OnSelect()
    {
        return new SelectableData()
        {
            Spells = assignedSpells
        };
    }

    protected IEnumerator ShootArrow(IAttackable target, GameObject targetObject)
    {
        GameObject arrow = new GameObject("Arrow");
        LineRenderer lineRenderer = arrow.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        Vector3 arrowStartPos = transform.position;
        Vector3 arrowEndPos = targetObject.transform.position;

        float distanceToTarget = Vector3.Distance(arrowStartPos, arrowEndPos);
        float travelTime = distanceToTarget / arrowSpeed;
        float elapsedTime = 0;
        float arrowLength = 0.5f;

        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;

            Vector3 currentPos = Vector3.Lerp(arrowStartPos, arrowEndPos, t);
            Vector3 arrowDirection = (arrowEndPos - arrowStartPos).normalized;
            Vector3 arrowTailPos = currentPos - (arrowDirection * arrowLength);

            lineRenderer.SetPosition(0, arrowTailPos);
            lineRenderer.SetPosition(1, currentPos);

            yield return null;
        }

        if (target != null && target.health > 0)
        {
            target.TakeDamage(attackDamage, this.gameObject);
        }

        Destroy(arrow);
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
