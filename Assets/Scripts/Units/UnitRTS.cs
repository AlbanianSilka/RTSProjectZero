using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRTS : MonoBehaviour
{
    private Vector2 destination;
    private bool isAttacking;
    private UnitRTS followTarget;

    protected virtual float moveSpeed => 5f; 
    protected virtual float maxHp => 10f;
    protected virtual float health { get; set; } = 100f;
    protected virtual float attackSpeed { get; set; } = 1f;
    protected RTS_controller rtsController;
    [SerializeField] string team;

    internal virtual int selectionPriority => 0; // default selection priority for units

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
            destination = followTarget.transform.position;
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

    public void takeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Ouch!");

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

                if(clickedUnit != null && clickedUnit.team != this.team)
                {
                    followUnit(clickedUnit);
                    clickedUnit = this.followTarget;
                    //MoveTo(clickPosition);
                    //StartCoroutine(attackPath(clickedUnit));
                }
            }
        }
        else
        {
            StopAllCoroutines();
            isAttacking = false;
        }
    }

    public void followUnit(UnitRTS leader)
    {
        Vector3 direction = (leader.transform.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpeed;

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

    private IEnumerator attackPath(UnitRTS attackedUnit)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        while (attackedUnit.health > 0)
        {
            // TODO: Need to create working following system
            // TODO #2: attack if have been attacked system need to be imply
            MoveTo(attackedUnit.transform.position);

            while (!HasReachedDestination())
            {
                yield return null;
            }

            float attackDelay = 1 / attackSpeed;
            yield return new WaitForSeconds(attackDelay);

            attackedUnit.takeDamage(1);
        }

        isAttacking = false; 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment"))
        {
            destination = transform.position;
            Debug.Log("You've hit a stone");
        }
    }
}
