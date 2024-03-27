using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class M_HPBar_FollowUp : MonoBehaviour
{
    private Slider HP_Bar;
    [SerializeField] private float Y_OffSet = 0.3f; //ü�¹ٸ� ���� �Ӹ����� �ø���ġ 
    private void Awake()
    {
        HP_Bar = GetComponent<Slider>();
    }
    private void OnEnable()
    {
        StartCoroutine(FollowUp());
    }

    private IEnumerator FollowUp()
    {
        while(true)
        {
            HP_Bar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, Y_OffSet, 0));
            yield return null;
        }
    }
}
