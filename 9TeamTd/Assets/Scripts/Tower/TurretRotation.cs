using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

// 타워 스크립트를 상속받아 수정
// 회전이 필요한 타워만 사용
public class TurretRotation : MonoBehaviour
{
    [SerializeField] private Transform _Turret;
    [SerializeField] private Vector3 moveDirection;

    public void SetTurretRotation(Transform _target)
    {
        if (_target != null)
        {
            moveDirection = (_target.position - transform.position).normalized; // 방향벡터 노멀라이즈 해서 받아오기
            _Turret.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    
}
