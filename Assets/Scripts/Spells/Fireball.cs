using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Spells/Fireball")]
public class Fireball : SpellSO
{
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private GameObject targetCirclePrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDuration = 0.5f;
    [SerializeField] private float damage = 30f;

    private void Awake()
    {
        manaCost = 30f;
    }

    public override void Cast(RTS_controller controller)
    {
        foreach (UnitRTS unit in controller.selectedUnitRTSList)
        {
            if (unit is Archmage)
            {
                if (HasEnoughMana(unit))
                {
                    controller.StartCoroutine(TargetingRoutine(controller, unit));
                }
            }
        }
    }

    private IEnumerator TargetingRoutine(RTS_controller controller, UnitRTS caster)
    {
        GameObject circle = Instantiate(targetCirclePrefab);
        circle.transform.localScale = new Vector3(radius, radius, 1);

        bool targeting = true;

        while (targeting)
        {
            // Follow mouse position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            circle.transform.position = mouseWorld;

            if (Input.GetMouseButtonDown(0))
            {
                targeting = false;
                DeductMana(caster);

                // Start sequence: play sound & then explosion
                controller.StartCoroutine(CastSequence(mouseWorld, caster));
            }

            if (Input.GetMouseButtonDown(1))
            {
                targeting = false;
            }

            yield return null;
        }

        Destroy(circle);
    }

    private IEnumerator CastSequence(Vector3 position, UnitRTS caster)
    {
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, position);
            yield return new WaitForSeconds(explosionSound.length);
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            Destroy(explosion, explosionDuration);

            yield return new WaitForSeconds(explosionDuration * 0.5f);
            ApplyEffect(position, caster);
        }
        else
        {
            ApplyEffect(position, caster);
        }
    }

    private void ApplyEffect(Vector3 position, UnitRTS caster)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);

        foreach (var hit in hits)
        {
            CombatEntity entity = hit.GetComponent<CombatEntity>();
            if (entity != null && entity.team != caster.team)
            {
                entity.TakeDamage(damage, caster.gameObject);
            }
        }
    }
}
