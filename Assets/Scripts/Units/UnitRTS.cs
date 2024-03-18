using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRTS : MonoBehaviour
{
    private Vector2 destination;
    private bool isAttacking;
    private UnitRTS followTarget;

    protected virtual float moveSpeed { get; set; } = 5f; 
    protected virtual float maxHp => 10f;
    protected virtual float health { get; set; } = 30f;
    protected virtual float attackSpeed { get; set; } = 1f;
    protected virtual float attackDamage { get; set; } = 1f;
    protected RTS_controller rtsController;
    [SerializeField] public string team;

    internal virtual int selectionPriority => 0; // default selection priority for units

    public virtual float spawnTime => 10f; // default time in seconds to create a new unit via "SpawnUnit"
    public healthbar_manager healthBar;
    public List<GameObject> spellButtons = new List<GameObject>();
    public event Action<UnitRTS> OnDeath;

    protected virtual void Awake()
    {
        rtsController = FindObjectOfType<RTS_controller>();
    }

    protected virtual void Start()
    {
        destination = transform.position;
    }

    protected virtual void Update()
    {
        if(followTarget != null)
        {
            Vector2 direction = (followTarget.transform.position - transform.position).normalized;

            float distanceToTarget = Vector2.Distance(transform.position, followTarget.transform.position);

            if(distanceToTarget <= 0.5f)
            {
                destination = transform.position;
            }
            else
            {
                Collider2D followTargetCollider = followTarget.GetComponent<Collider2D>();

                float targetColliderSize = Mathf.Max(followTargetCollider.bounds.size.x, followTargetCollider.bounds.size.y);

                // Calculate dynamic offset distance based on the collider size
                float dynamicOffsetDistance = targetColliderSize * 0.5f;

                destination = (Vector2)followTarget.transform.position - (direction * dynamicOffsetDistance);
            }
        }

        transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {
            List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;
            if (selectedUnits.Contains(this))
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                HandleRightClick(clickPosition);
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
        // Check if the distance between the current position and the destination is very small
        return Vector2.Distance(transform.position, destination) < 0.1f;
    }

    public void takeDamage(float damage, UnitRTS attacker)
    {
        // TODO: Need to think about making a following system when counter attack
        this.health -= damage;
        this.healthBar.updateHealthBar(this.health, this.maxHp);
        Debug.Log($"{this.name} was hit by {attacker.name} on {damage} dmg and has {this.health} hp");

        // if not moving - counter attack
        if (HasReachedDestination())
        {
            GameObject counterAttackTarget = attacker.gameObject;
            StartCoroutine(attackPath(counterAttackTarget));
        }

        if(health <= 0)
        {
            Die();
        }
    }

    private void HandleRightClick(Vector3 clickPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Unit"))
            {
                UnitRTS clickedUnit = clickedObject.GetComponent<UnitRTS>();
                if (clickedUnit != null && clickedUnit.team != this.team)
                {
                    followUnit(clickedUnit);
                    clickedUnit = this.followTarget;
                    StartCoroutine(attackPath(clickedObject));
                }
            } else if (clickedObject.CompareTag("Building"))
            {
                RTS_building clickedBuilding = clickedObject.GetComponent<RTS_building>();
                if(clickedBuilding != null && clickedBuilding.team != this.team)
                {
                    StartCoroutine(attackPath(clickedObject));
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

    public void followUnit(UnitRTS leader)
    {
        Vector3 direction = (leader.transform.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpeed;
        setFollowTarget(leader);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.gameObject.GetComponent<UnitRTS>() != null)
            {
                Vector3 avoidDirection = (transform.position - collider.transform.position).normalized;
                transform.position += avoidDirection * Time.deltaTime * moveSpeed;
            }
        }
    }

    public void setFollowTarget(UnitRTS newTarget)
    {
        followTarget = newTarget;
    }

    private IEnumerator attackPath(GameObject target)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        if(target.CompareTag("Unit"))
        {
            UnitRTS attackedUnit = target.GetComponent<UnitRTS>();
            while (attackedUnit.health > 0)
            {

                while (!HasReachedDestination())
                {
                    yield return null;
                }

                float attackDelay = 1 / attackSpeed;
                yield return new WaitForSeconds(attackDelay);

                if (attackedUnit.health > 0)
                {
                    attackedUnit.takeDamage(attackDamage, this);
                }
                else
                {
                    break;
                }
            }
        } else if (target.CompareTag("Building"))
        {
            RTS_building attackedBuilding = target.GetComponent<RTS_building>();

            while(attackedBuilding.health > 0)
            {
                while (!HasReachedDestination())
                {
                    yield return null;
                }

                float attackDelay = 1 / attackSpeed;
                yield return new WaitForSeconds(attackDelay);

                if (attackedBuilding.health > 0)
                {
                    attackedBuilding.TakeDamage(attackDamage);
                }
                else
                {
                    break;
                }
            }
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

}
