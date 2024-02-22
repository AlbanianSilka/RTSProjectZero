using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRTS : MonoBehaviour
{
    private Vector2 destination;
    protected virtual float moveSpeed => 5f; // default move speed for units
    internal virtual int selectionPriority => 0; // default selection priority for units
    public List<GameObject> spellButtons = new List<GameObject>();

    public void Start()
    {
        destination = transform.position;
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    public void MoveTo(Vector2 targetPosition)
    {
        destination = targetPosition;
    }
}
