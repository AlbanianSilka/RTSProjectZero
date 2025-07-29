using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Interfaces;
using Data;
using static Resource;

public class UnitRTS : CombatEntity, IAttackable, ISelectable
{
    private Vector3 destination;
    private UnitRTS followTarget;
    [SerializeField] private GameObject minimapGFX;
    [SerializeField] private GameObject visionCircleMain;
    [SerializeField] private GameObject visionCircleSecondary;
    private int visionCount = 0;
    NavMeshAgent agent;

    public virtual bool isAttacking { get; set; }
    protected virtual float moveSpeed { get; set; } = 5f; 
    protected virtual float maxHp => 10f;
    protected virtual float attackSpeed { get; set; } = 1f;
    protected override float arrowSpeed => 0f;
    protected override float attackDamage => 1f;
    protected virtual float attackRange { get; set; } = 3f;
    protected virtual float vision { get; set; } = 2f;
    protected RTS_controller rtsController;
    protected Dictionary<ResourceType, int> RequiredResources { get; set; }
    protected AttackType attackType;

    internal virtual int selectionPriority => 0; // default selection priority for units

    public List<SpellSO> assignedSpells = new();
    public virtual float health { get; set; } = 30f;
    public virtual float spawnTime => 10f; // default time in seconds to create a new unit via "SpawnUnit"
    public healthbar_manager healthBar;
    public Sprite unitIcon;
    public enum AttackType
    {
        Melee, Ranged
    };

    public SelectableData OnSelect()
    {
        SelectableData data = new SelectableData()
        {
            Spells = assignedSpells
        };
        return data;
    }

    public UnitRTS()
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

    public bool CanBeTrained(Player owner)
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

    protected virtual void Start()
    {
        if (owner == null)
        {
            Debug.LogWarning($"{name} has no owner assigned at Start(), deferring init...");
            StartCoroutine(WaitForOwner());
            return;
        }

        if (minimapGFX != null)
        {
            SpriteRenderer renderer = minimapGFX.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = owner.teamColor;
            }
        }

        if (owner is BotPlayer)
        {
            if (visionCircleMain != null) visionCircleMain.SetActive(false);
            if (visionCircleSecondary != null) visionCircleSecondary.SetActive(false);
            if (minimapGFX != null) minimapGFX.SetActive(false);
        }

        rtsController = owner.rtsController;
        destination = transform.position;
        SetVisionScale();
    }

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected virtual void Update()
    {
        SetTargetPosition();
        SetAgentPosition();

        if(followTarget != null)
        {
            handleFollowTarget();
        }
    }

    private void SetTargetPosition()
    {
        if (Input.GetMouseButtonDown(1))
        {
            List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;
            if (selectedUnits.Contains(this))
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = Mathf.Abs(Camera.main.transform.position.z);

                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                destination = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
                HandleRightClick(worldPosition);
            }
        }
    }

    private void SetAgentPosition()
    {
        agent.SetDestination(new Vector3(destination.x, destination.y, transform.position.z));
    }

    private void handleFollowTarget(float stopDistance = 3f)
    {
        Vector3 directoon = (followTarget.transform.position - transform.position).normalized;

        float distanceToTarget = Vector3.Distance(transform.position, followTarget.transform.position);

        if(followTarget.team != team)
        {
            stopDistance = attackRange;
        }

        if (distanceToTarget <= stopDistance)
        {
            destination = transform.position;
        }
        else
        {
            Collider followTargetCollider = followTarget.GetComponent<Collider>();

            if (followTargetCollider != null)
            {
                float targetColliderSize = Mathf.Max(followTargetCollider.bounds.size.x, followTargetCollider.bounds.size.y);
                float dynamicOffsetDistance = targetColliderSize * 0.5f;

                destination = followTarget.transform.position - (directoon * dynamicOffsetDistance);
            }
            else
            {
                destination = followTarget.transform.position;
            }
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        destination = targetPosition;
    }

    public bool HasReachedDestination()
    {
        // Calculate the distance between the unit and the target position
        float distance = Vector3.Distance(transform.position, destination);

        return distance <= 3f;
    }

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (this == null || health <= 0)
            return;

        // TODO: Would probably need an update, because counter-attacked follows a bit strange
        this.health -= damage;
        this.healthBar.updateHealthBar(this.health, this.maxHp);
        Debug.Log($"{this.name} was hit by {attacker.name} on {damage} dmg and has {this.health} hp");

        // if not moving - counter attack
        if (HasReachedDestination())
        {
            GameObject counterAttackTarget = attacker;
            StartCoroutine(attackPath(counterAttackTarget.GetComponent<IAttackable>(), counterAttackTarget));
        }

        if(health <= 0)
        {
            Die();
        }
    }

    private void HandleRightClick(Vector3 clickPosition)
    {
        Vector2 raycastOrigin = new Vector2(clickPosition.x, clickPosition.y);

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Unit"))
            {
                UnitRTS clickedUnit = clickedObject.GetComponent<UnitRTS>();
                followTarget = clickedUnit;

                if (clickedUnit.team != team)
                {
                    StartCoroutine(attackPath(clickedObject.GetComponent<IAttackable>(), clickedObject));
                }
            } else if (clickedObject.CompareTag("Building"))
            {
                RTS_building clickedBuilding = clickedObject.GetComponent<RTS_building>();
                if(clickedBuilding != null && clickedBuilding.team != this.team)
                {
                    StartCoroutine(attackPath(clickedObject.GetComponent<IAttackable>(), clickedObject));
                }
            } else
            {
                followTarget = null;
            }
        }
        else
        {
            followTarget = null;
            StopAllCoroutines();
            isAttacking = false;
        }
    }

    public void setFollowTarget(UnitRTS newTarget)
    {
        followTarget = newTarget;
    }

    public IEnumerator attackPath(IAttackable target, GameObject targetObject)
    {
        if (isAttacking)
            yield break;

        while (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            yield return null;
        }

        isAttacking = true;


        while (target.health > 0)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetObject.transform.position);

            if(this.attackType == AttackType.Melee)
            {
                if (distanceToTarget <= attackRange)
                {
                    float attackDelay = 1 / attackSpeed;
                    yield return new WaitForSeconds(attackDelay);

                    target.TakeDamage(attackDamage, this.gameObject);
                }
                else
                {
                    agent.SetDestination(targetObject.transform.position);
                }
            } else if (this.attackType == AttackType.Ranged)
            {
                if (distanceToTarget <= attackRange)
                {
                    agent.SetDestination(transform.position);
                    StartCoroutine(ShootArrow(target, targetObject));

                    yield return new WaitForSeconds(attackSpeed);
                }
            }

            // wait for the next frame to check the distance again
            yield return null;
        }

        isAttacking = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Building"))
        {
            destination = transform.position;
        }
    }

    private IEnumerator WaitForOwner()
    {
        yield return new WaitUntil(() => owner != null);

        rtsController = owner.rtsController;
        destination = transform.position;
    }

    public void SetVisible(bool visible)
    {
        if (minimapGFX == null) return;

        if (visible)
        {
            if (visionCount == 0)
                minimapGFX.SetActive(true);

            visionCount++;
        }
        else
        {
            visionCount = Mathf.Max(visionCount - 1, 0);
            if (visionCount == 0)
                minimapGFX.SetActive(false);
        }
    }

    private void SetVisionScale()
    {
        Vector3 scale = new Vector3(vision, vision, 1f);
        if (visionCircleMain != null) visionCircleMain.transform.localScale = scale;
        if (visionCircleSecondary != null) visionCircleSecondary.transform.localScale = scale;
    }
}
