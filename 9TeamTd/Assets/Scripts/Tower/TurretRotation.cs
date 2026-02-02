using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

// 작성자 : 한성우

// 타워 스크립트를 상속받아 수정
// 회전이 필요한 타워만 사용

public enum TurretRotationType
{
    Default = 0,
    None = 1,
    TurnAndHeading = 2,
    TurnOnly = 3,
    HeadingOnly = 4,
}

public class TurretRotation : MonoBehaviour
{
    [SerializeField] private GameObject _turret;
    [SerializeField] private Transform _target = null;
    [SerializeField] private TurretRotationType turretRotationType = TurretRotationType.TurnAndHeading;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private float rotationSpeed = 30f;


    public void SetTurretRotationTarget(Transform target)
    {
        _target = target;
    }


    private void Update()
    {
        // 타겟을 따라 회전
        if (_target != null &&
        (turretRotationType == TurretRotationType.TurnAndHeading || turretRotationType == TurretRotationType.HeadingOnly))
        {
            moveDirection = (_target.position - transform.position).normalized; // 방향벡터 노멀라이즈 해서 받아오기
            _turret.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        // 일반 회전
        else if (turretRotationType == TurretRotationType.TurnAndHeading || turretRotationType == TurretRotationType.TurnOnly)
        {
            _turret.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }
}
