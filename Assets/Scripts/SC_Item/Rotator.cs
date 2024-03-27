using UnityEngine;

// 필드에 드랍된 아이템 오브젝트를 지속적으로 회전하는 스크립트
public class Rotator : MonoBehaviour {
    public float rotationSpeed = 80f;

    private void Update() 
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}