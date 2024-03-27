using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShowMapName : MonoBehaviour
{
    [SerializeField] private Image MapNameImage;

    public void SetMapNameAndShow() //��(��)�̸��� ���� �̹��� ������ Ȱ��ȭ
    {
        Scene CurScene = SceneManager.GetActiveScene();
        MapNameImage.sprite = Resources.Load<Sprite>("UI/MapName/" + CurScene.name);
        gameObject.SetActive(true);
        StartCoroutine(FadeAway());
    }


    IEnumerator FadeAway() //1.5���Ļ����
    {
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
    }


}
