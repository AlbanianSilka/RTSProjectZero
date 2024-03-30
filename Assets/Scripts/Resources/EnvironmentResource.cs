using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentResource : MonoBehaviour
{
    public virtual float maxHp { get; set; } = 10f;
    public virtual float health { get; set; } = 30f;

    public event Action<EnvironmentResource> OnDeath;
    public Resource.ResourceType providedResourceType;

    public Resource.ResourceType GetProvidedResourceType()
    {
        return providedResourceType;
    }

    public void takeDamage(float damage, UnitRTS attacker)
    {
        this.health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);

        Destroy(gameObject);
    }
}
