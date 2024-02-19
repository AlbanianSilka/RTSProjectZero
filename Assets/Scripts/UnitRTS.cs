using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRTS : MonoBehaviour
{
    private Vector2 destination;
    public float moveSpeed = 5f;

    void Start()
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
