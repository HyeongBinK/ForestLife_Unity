using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoPing : MonoBehaviour
{
    [SerializeField] private float PingDisappearDelayTime = 1.0f; //핑이 자연스레 사라지는 시간
    private float PingDisappearDelayClock = 0; //핑이 자연스레 사라지는 타이머

    public void DrawPing(Vector3 Position)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        else
            PingDisappearDelayClock = 0;
        
        Vector2 ScreenLocation = Camera.main.WorldToScreenPoint(Position);

        gameObject.transform.position = ScreenLocation;
        StartCoroutine(PingDisappearEvent());
        
    }

    IEnumerator PingDisappearEvent()
    {
        while(PingDisappearDelayClock <= PingDisappearDelayTime)
        {
            PingDisappearDelayClock += Time.deltaTime;
            yield return null;
        }

        PingDisappearDelayClock = 0;
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
   
}
