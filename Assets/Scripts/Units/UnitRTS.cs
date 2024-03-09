using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRTS : MonoBehaviour
{
    private Vector2 destination;
    private bool isAttacking;

    protected virtual float moveSpeed => 5f; 
    protected virtual float maxHp => 10f;
    protected virtual float health { get; set; } = 10f;
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
                    MoveTo(clickPosition);
                    StartCoroutine(attackPath(clickedUnit));
                }
            }
        }
        else
        {
            StopAllCoroutines();
            isAttacking = false;
        }
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

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);

        Destroy(gameObject);
    }

}
