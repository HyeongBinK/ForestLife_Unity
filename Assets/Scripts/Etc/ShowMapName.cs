using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShowMapName : MonoBehaviour
{
    [SerializeField] private Image MapNameImage;

    public void SetMapNameAndShow() //씬(맵)이름에 따라 이미지 변경후 활성화
    {
        Scene CurScene = SceneManager.GetActiveScene();
        MapNameImage.sprite = Resources.Load<Sprite>("UI/MapName/" + CurScene.name);
        gameObject.SetActive(true);
        StartCoroutine(FadeAway());
    }


    IEnumerator FadeAway() //1.5초후사라짐
    {
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
    }


}
