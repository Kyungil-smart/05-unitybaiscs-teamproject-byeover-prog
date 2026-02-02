using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 테스트를 위한 임시 이동 코드입니다 - 플레이어 담당은 마음대로 수정하세요
public class PlayerMovement : MonoBehaviour
{
    // wasd 로 플레이어 이동
    public float speed = 6f;
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.Translate(new Vector3(moveX, 0, moveZ));
    }
}
