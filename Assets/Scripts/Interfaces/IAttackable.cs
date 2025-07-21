using System;
using UnityEngine;

public interface IAttackable
{
    float health { get; }
    void TakeDamage(float amount, GameObject attacker);
}
