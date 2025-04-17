using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Resource;

public class UnitRTS : MonoBehaviour, IAttackable
{
    private Vector3 destination;
    private UnitRTS followTarget;
    NavMeshAgent agent;

    protected virtual bool isAttacking { get; set; }
    protected virtual float moveSpeed { get; set; } = 5f; 
    protected virtual float maxHp => 10f;
    protected virtual float attackSpeed { get; set; } = 1f;
    protected virtual float attackDamage { get; set; } = 1f;
    protected virtual float attackRange { get; set; } = 3f;
    protected RTS_controller rtsController;
    protected Dictionary<ResourceType, int> RequiredResources { get; set; }
    protected AttackType attackType;

    internal virtual int selectionPriority => 0; // default selection priority for units

    public virtual float health { get; set; } = 30f;
    [SerializeField] public string team;
    public virtual float spawnTime => 10f; // default time in seconds to create a new unit via "SpawnUnit"
    public healthbar_manager healthBar;
    public List<GameObject> spellButtons = new List<GameObject>();
    public event Action<UnitRTS> OnDeath;
    public Sprite unitIcon;
    public Player owner;
    public enum AttackType
    {
        Melee, Ranged
    };

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
        rtsController = owner.rtsController;
        destination = transform.position;
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
                destination = Camera.main.WorldToScreenPoint(Input.mousePosition);
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = Mathf.Abs(Camera.main.transform.position.z); 

                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
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

        Debug.Log(followTarget.name);
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

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);

        Destroy(gameObject);
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
        // TODO: Need to think about making a following system when counter attack
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

    private IEnumerator attackPath(IAttackable target, GameObject targetObject)
    {
        if (isAttacking)
            yield break;

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
                // TODO: move ranged stats to ranged units
                float arrowSpeed = 20f;
                float shootDelay = 2.0f;

                if (distanceToTarget <= attackRange)
                {
                    agent.SetDestination(transform.position);
                    StartCoroutine(ShootArrow(target, targetObject, arrowSpeed));

                    yield return new WaitForSeconds(shootDelay);
                }
            }

            // wait for the next frame to check the distance again
            yield return null;
        }

        isAttacking = false;
    }

    // TODO: probably move this method to some common class to avoid duplication with building
    private IEnumerator ShootArrow(IAttackable target, GameObject targetObject, float arrowSpeed)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Building"))
        {
            destination = transform.position;
        }
    }

}
