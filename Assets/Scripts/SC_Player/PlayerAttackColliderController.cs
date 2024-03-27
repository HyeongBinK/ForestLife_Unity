using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackColliderController : MonoBehaviour
{
    [SerializeField] private Collider NormalAttackRange;
    [SerializeField] private Collider PowerSwingRange;
    [SerializeField] private Collider EarthquakeRange;
    [SerializeField] private Collider ThunderRange;

    public const float NormalAttackDamageTerm = 0.3f;
    public const float PowerSwingDamageTerm = 0.9f;
    public const float EarthquakeDamageTerm = 1.0f;
    public const float ThunderDamageTerm = 1.0f;

    public void NormalAttack()
    {
       StartCoroutine(DelayDisableCollider(NormalAttackDamageTerm, NormalAttackRange));
    }

    public void UseSkill(string SkillName)
    {
        switch (SkillName)
        {
            case "POWERSWING":
                StartCoroutine(DelayDisableCollider(PowerSwingDamageTerm, PowerSwingRange));
                break;
            case "EARTHQUAKE":
                StartCoroutine(DelayDisableCollider(EarthquakeDamageTerm, EarthquakeRange));
                break;
            case "THUNDER":
                StartCoroutine(DelayDisableCollider(ThunderDamageTerm, ThunderRange));
                break;
        }
    }

    IEnumerator DelayDisableCollider(float DelayTime, Collider collider)
    {
        collider.gameObject.SetActive(false);
        yield return new WaitForSeconds(DelayTime);
        collider.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        collider.gameObject.SetActive(false);
    }
}
