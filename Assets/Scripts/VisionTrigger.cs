using UnityEngine;

public class VisionTrigger : MonoBehaviour
{
    public UnitRTS ownerUnit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        UnitRTS otherUnit = other.GetComponentInParent<UnitRTS>();
        if (IsEnemy(otherUnit))
        {
            otherUnit.SetVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UnitRTS otherUnit = other.GetComponentInParent<UnitRTS>();
        if (otherUnit != null && otherUnit.owner != null && ownerUnit.owner != null)
        {
            if (otherUnit.owner.team != ownerUnit.owner.team)
            {
                otherUnit.SetVisible(false);
            }
        }
    }

    private bool IsEnemy(UnitRTS otherUnit)
    {
        return otherUnit != null &&
               otherUnit.owner != null &&
               ownerUnit != null &&
               ownerUnit.owner != null &&
               otherUnit.owner.team != ownerUnit.owner.team;
    }
}
