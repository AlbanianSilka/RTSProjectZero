using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Interfaces;

public abstract class CombatEntity : MonoBehaviour, IBlocksBuildingPlacement
{
    protected abstract float attackDamage { get; }
    protected abstract float arrowSpeed { get; }

    public Player owner;
    [SerializeField] public string team;
    public event Action<CombatEntity> OnDeath;
    public bool BlocksPlacement() => true;

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
