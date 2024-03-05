using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRTS : MonoBehaviour
{
    private Vector2 destination;
    protected virtual float moveSpeed => 5f; // default move speed for units
    internal virtual int selectionPriority => 0; // default selection priority for units
    public List<GameObject> spellButtons = new List<GameObject>();

    protected virtual void Start()
    {
        destination = transform.position;
    }

    protected virtual void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
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
}
