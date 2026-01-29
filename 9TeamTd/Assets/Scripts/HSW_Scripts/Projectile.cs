using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ProjectileEnumData;

// 작성자 : 한성우

public class Projectile : MonoBehaviour
{
    // ProjectileStats 변수는 여기 넣을 필요 없음
    // 테스트 이후     [SerializeField] 제거 필요
    [SerializeField] private GameObject attacker;
    [SerializeField] private GameObject target;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private int attackValue; // 생성한 타워로부터 공격력 받아오기
    [SerializeField] private float damageRatio; // 생성한 타워로부터 공격력 받아오기
    [SerializeField] private float moveSpeed;   // ProjectileStats 에서 받아오기
    [SerializeField] private float lifeTime;    // ProjectileStats 에서 받아오기
    [SerializeField] private ProjectileSpwanType projectileSpwanType;    // ProjectileStats 에서 받아오기

    DamageCalculator damageCalculator = new DamageCalculator();

    // setActive false 상태인 오브젝트하나를 받아오는 기능 필요

    private void OnEnable()
    {
        //damageCalculator = new DamageCalculator();  // 이거 여기 넣어도 오브젝트 풀링에서 객체 여러번 생성되는지 확인 필요

        // 자신에게 붙은 ProjectileStats 에서 필요한 변수 받아오기 (만약에 스크립트가 없다면 예외 처리 어떻게할지 고민 필요)
        moveSpeed = this.GetComponent<ProjectileStats>().moveSpeed;
        lifeTime = this.GetComponent<ProjectileStats>().lifeTime;
        projectileSpwanType = this.GetComponent<ProjectileStats>().projectileSpwanType;

        // 활성화 최초에 방향벡터 노멀라이즈 해서 받아오기
        moveDirection = (target.transform.position - transform.position).normalized;
    }

    private void Start()
    {
        // 객체를 미리 여러개 생성해 놓고 코루틴으로 시간 제어
        StartCoroutine(LifeTimeCoroutine());
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


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            // 데미지 주는 식은 따로 빼서 계산하고 takedamage 처리해야함
            // other.GetComponent<Monster>().TakeDamage(damageCalculator.CalculatingDamage(공격력, 비율, 방어력));
            Debug.Log($"{other.gameObject.layer} 충돌 확인! {damageCalculator.CalculatingDamage(attackValue, damageRatio, 0)} 데미지를 주었습니다.");
        }
        SetEnableObject();
    }



    // 투사체가 지속 시간 이후에도 남아있다면 비활성화 시켜주는 코루틴, 오브젝트 풀링을 위해 사용
    IEnumerator LifeTimeCoroutine()
    {
        if (this.gameObject.activeSelf == true)
            yield return new WaitForSeconds(lifeTime);
        SetEnableObject();        
    }



    // 충돌이나 지속 시간 등으로 비활성화시 리셋해야 하는 요소 모음
    private void SetEnableObject()
    {
        Reset();
        this.gameObject.SetActive(false);
    }


    private void Reset()
    {
        // 오브젝트 풀링용으로 초기화 기능 추가 필요
    }





    // 데이터 초기화 이후 ProjectileSpwanType 에 맞춰서 스폰(setActive true) 처리


    // 충돌 되었을 때 데미지 주는 처리


}
