using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ProjectileEnumData;

// 작성자 : 한성우

public class Projectile : MonoBehaviour
{
    // ProjectileStats 변수는 여기 넣을 필요 없음
    [Header("테스트를 위해 잠시 보이게 한 변수")]
    [SerializeField] private GameObject attacker; 
    [SerializeField] private GameObject target;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private float moveSpeed;   // ProjectileStats 동기화된 이후 삭제 예정
    [SerializeField] private float lifeTime;    // ProjectileStats 동기화된 이후 삭제 예정
    [SerializeField] private ProjectileSpwanType projectileSpwanType;    // ProjectileStats 동기화된 이후 삭제 예정


    private void Start()
    {
        // 시작시 방향벡터 노멀라이즈 해서 받아오기
        moveDirection = (target.transform.position - transform.position).normalized;
    }


    // 임시용 이동
    private void Update()
    {
        if (projectileSpwanType == ProjectileSpwanType.AttackerToTarget) Move();
        else if (projectileSpwanType == ProjectileSpwanType.AttackerToTargetHoming) MoveHoming();
    }

    private void Move()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void MoveHoming()
    {
        // 타겟이 있다면 타겟의 방향을 실시간으로 받아옴 (타겟이 없다면(비활성화 되었다면) 마지막으로 받은 타겟 방향에서 멈춤)
        if (target != null)
        {
            moveDirection = (target.transform.position - transform.position).normalized;       
        }
        Move(); // 기능 통일을 위해 MoveTowards 사용 안하고 Move() 에서 처리

    }




    // setActive false 상태인 오브젝트하나를 받아옴

    // ProjectileStats 데이터들 + 타워 위치 필드랑, 대상 적 위치 필드 받아 데이터 초기화

    // 데이터 초기화 이후 ProjectileSpwanType 에 맞춰서 스폰(setActive true) 처리

    // 


    // 충돌 되었을 때 데미지 주는 처리


    // 생명력 다 되었을 때, setActive false 및 데이터 초기화(오브젝트 풀링)
}
