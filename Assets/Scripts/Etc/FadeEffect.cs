using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    [SerializeField] private Image m_Image;
    [SerializeField] [Range(0.1f, 10f)] private float m_fFadeTime; //페이트되는 타임
    [SerializeField] AnimationCurve m_FadeCurve;
   

    public void Fadein()
    {
        StartCoroutine(Fade(1,0));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(0,1));
    }

    IEnumerator Fade(int Start, int Result) //알파값조절
    {
        float CurTime = 0.0f;
        float Percent = 0.0f;
        while (Percent < 1.0)
        {
            CurTime += Time.deltaTime;
            Percent = CurTime / m_fFadeTime;
            Color color = m_Image.color; 
            color.a = Mathf.Lerp(Start, Result, m_FadeCurve.Evaluate(Percent)); 
            m_Image.color = color;
            yield return null;
        }
    }

}
