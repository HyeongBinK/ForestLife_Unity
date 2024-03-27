using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private string m_TargetSceneName;
    [SerializeField] private Vector3 m_TargetPosition; 

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            GameManager.instance.SetPlayerNewPosition(m_TargetPosition);
            Player.instance.OnPortal(m_TargetSceneName);
        }
    }

}
