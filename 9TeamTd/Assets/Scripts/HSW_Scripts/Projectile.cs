using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using static ProjectileEnumData;
using static TowerEnumData;
using static UnityEngine.GraphicsBuffer;

// 작성자 : 한성우

public class Projectile : MonoBehaviour
{
    // ProjectileStats 변수는 여기 넣을 필요 없음
    // 테스트 이후     [SerializeField] 제거 필요
    [SerializeField] private GameObject attacker;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 moveDirection;


    [SerializeField] private int attackValue; // 생성한 타워로부터 공격력 받아오기
    [SerializeField] private float damageRatio = 1; // 비율은 일단 1로
    [SerializeField] private float moveSpeed;   // ProjectileStats 에서 받아오기
    [SerializeField] private float lifeTime;    // ProjectileStats 에서 받아오기
    [SerializeField] private float damageInterval;  // ProjectileStats 에서 받아오기
    [SerializeField] private ProjectileSpwanType projectileSpwanType;    // ProjectileStats 에서 받아오기
    [SerializeField] private ProjectileSpacialAbility projectileSpacialAbility;    // ProjectileStats 에서 받아오기
    [SerializeField] private DamageTargetTeamType damageTargetTeamType;
    [SerializeField] private ProjectileDamageCategory projectileDamageCategory;

    [SerializeField] private int effectRate;   // 이 아래는 상태 효과를 위해 추가
    [SerializeField] private float effectValue;
    [SerializeField] private float effectInterval;
    [SerializeField] private float duration;
    [SerializeField] private int overlapCount;



    // 데미지 계산 스크립트
    // DamageCalculator damageCalculator = new DamageCalculator();

    // 콜라이더 범위 내 적 담아둘 리스트(주기에 따라 여러 번 데미지를 주는 경우 사용)
    private List<Collider> dmgTrgets = new List<Collider>();

    private Coroutine GroundDoTCoroutine;

    // setActive false 상태인 오브젝트하나를 받아오는 기능 필요

    private void OnEnable()
    {
        //damageCalculator = new DamageCalculator();  // 이거 여기 넣어도 오브젝트 풀링에서 객체 여러번 생성되는지 확인 필요

        // 자신에게 붙은 ProjectileStats 에서 필요한 변수 받아오기 (만약에 스크립트가 없다면 예외 처리 어떻게할지 고민 필요)
        ProjectileStats stats = this.GetComponent<ProjectileStats>();

        if (stats != null)
        {
            // 안전을 위해 다시 projectileStats 을 부르고 다시 셋팅
            stats.Init();
            InitStats(stats);
        }


        // 콜라이더 범위 내 리스트 초기화 (오브젝트 풀링을 할거라, 무조건 초기화 해야함)
        dmgTrgets.Clear();


        // 객체를 미리 여러개 생성해 놓고 코루틴으로 투사체 유지 시간 제어, Start에 넣으면 에러 생김
        StartCoroutine(LifeTimeCoroutine());

        // 있으면 이동 및 방향 재설정(적 위치에 생성되는 방향성 있는 투사체를 위해 필요)
        if (_target != null)
        {
            InitProjectile();
        }



        //  주기에 따라 여러 번 데미지 주는 경우 데미지 코루틴 시작
        if (projectileSpacialAbility == ProjectileSpacialAbility.GroundDoT)
        {
            GroundDoTCoroutine = StartCoroutine(DoTIntervalCoroutine());
        }

    }


    public void InitStats(ProjectileStats stats)
    {
        if (stats == null) return;
        else
        {
            moveSpeed = stats.moveSpeed;
            lifeTime = stats.lifeTime;
            damageInterval = stats.damageInterval;
            projectileSpwanType = stats.projectileSpwanType;
            projectileSpacialAbility = stats.projectileSpacialAbility;
            damageTargetTeamType = stats.damageTargetTeamType;
            projectileDamageCategory = stats.projectileDamageCategory;

            
            effectRate = stats.effectRate;
            effectValue = stats.effectValue;
            effectInterval = stats.effectInterval;
            duration = stats.duration;
            overlapCount = stats.overlapCount;

            Debug.Log($"InitStats {effectRate}");



        }
    }


    // 이동 및 방향 재설정
    public void InitProjectile()
    {



        // 방향 초기화, 이 함수가 아래의 SetFirstPosition 함수보다 먼저 있어야 적 위치에서 생성되는 투사체 방향 제대로 잡힘
        SetRotation();


        // 만약 타겟의 위치에 생성되는 투사체면 타겟의 위치로 이동시키기, OnEnable에서 실행되면 정보를받지 못해 오류남
        if (projectileSpwanType == ProjectileSpwanType.TargetPosition ||
           projectileSpwanType == ProjectileSpwanType.TargetPositionAtkToTrgDirection)
        {
            SetFirstPosition(_target);
        }


    }

    // 투사체 비활성화시 처리
    private void OnDisable()
    {
        // 코루틴과 리스트 초기화
        if (GroundDoTCoroutine != null) StopCoroutine(GroundDoTCoroutine);
        dmgTrgets.Clear();
    }


    private void Update()
    {
        // 이동하기, 이동이 있을 경우에만 사용
        if (projectileSpwanType == ProjectileSpwanType.AttackerToTarget) Move();
        else if (projectileSpwanType == ProjectileSpwanType.AttackerToTargetHoming) MoveHoming();
    }

    // 방향 받기 함수들

    private void SetRotation()
    {
        // 방향이 필요없는 공격 방식은 벡터 앞 방향으로
        if (projectileSpwanType == ProjectileSpwanType.AttackerPosition ||
            projectileSpwanType == ProjectileSpwanType.TargetPosition)
        {
            moveDirection = Vector3.forward;
        }

        // 방향 필요없는 공격 방식을 제외하면 오브젝트의 방향을 적 방향으로 초기화
        else
        {
            if (_target != null)
            {
                moveDirection = (_target.position - transform.position).normalized; // 방향벡터 노멀라이즈 해서 받아오기
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }

    }


    // 이동 관련 함수들

    private void Move()
    {
        // 직진 이동, 이동 전에 타겟 방향으로 회전을 해줘야 함
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void MoveHoming()
    {
        // 타겟이 있다면 타겟의 방향을 실시간으로 받아옴 (타겟이 없다면(비활성화 되었다면) 마지막으로 받은 타겟 방향에서 멈춤)
        if (_target != null) SetRotation();  // 이동식 계속 회전

        Move(); // 기능 통일을 위해 MoveTowards 사용 안하고 Move() 에서 처리
    }


    // 충돌 및 피해 관련 함수들

    private void OnTriggerEnter(Collider other)
    {
        // 데미지 주는 대상인지 판단
        if (CheckHostileTarget(other) == true)
        {
            // 주기에 따라 여러번 피해를 줄 때
            if (projectileSpacialAbility == ProjectileSpacialAbility.GroundDoT)
            {
                // 리스트에 없을때만 대상 추가하기
                if (!dmgTrgets.Contains(other)) dmgTrgets.Add(other);
            }
            // 한 번 데미지를 줄 때
            else GiveDamageChance(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 범위를 벗어나면 데미지 대상에서 제외
        if (projectileSpacialAbility == ProjectileSpacialAbility.GroundDoT)
        {
            if (dmgTrgets.Contains(other)) dmgTrgets.Remove(other);
        }
    }


    private bool CheckHostileTarget(Collider other)
    {
        // 대상은 몬스터이고, 투사체는 적이나 전체에 피해를 주는 것이 맞는지 함께 확인
        bool isMonster = other.gameObject.layer == LayerMask.NameToLayer("Monster");
        bool isTargetTeam = (damageTargetTeamType == DamageTargetTeamType.Enemy ||
                             damageTargetTeamType == DamageTargetTeamType.ForAll);

        return isMonster && isTargetTeam;
    }


    // 데미지 및 후처리 함수
    private void GiveDamageChance(Collider other)
    {
        // 데미지 계산
        int finalDMG = DamageCalculator.CalculatingDamage(attackValue, damageRatio, 0); // 현재는 방어력 0으로 놓지만, 추후 수정 필요

        // 실제로 데미지 주는 처리 (방어력 들어가도록 수정 필요)
        other.GetComponent<Monster>().TakeDamage(attackValue, damageRatio);
        Debug.Log($"{gameObject.name} -> {other.gameObject.name}, {finalDMG} 데미지를 주었습니다.");



        // 랜덤 확률로 속성 별 상태 효과 주는 처리
        float temp = Time.time * 100f;
        Random.InitState((int)temp);

        int randomInt = UnityEngine.Random.Range(0, 10000);
        Debug.Log(randomInt);
        Debug.Log(effectRate);

        // 발동 확률이 현재 선택된 랜덤 값 보다 클 경우 발동
        if (effectRate >= randomInt)
        {
            GiveStatusEffectChance(other);
            Debug.Log("상태 효과 발동");
        }
        

        // 단일 피해면 오브젝트 비활성화
        if (projectileSpacialAbility == ProjectileSpacialAbility.Single) SetEnableObject();
    }

    private void GiveStatusEffectChance(Collider other)
    {
        switch(projectileDamageCategory)
        {
            case ProjectileDamageCategory.Default:
                Debug.LogWarning("투사체 공격 속성이 디폴트 값입니다");
                break;
            case ProjectileDamageCategory.Physical:
                Debug.Log("물리 확인");
                KnockBack(other);
                break;

            case ProjectileDamageCategory.Fire:
                Burn( other);
                break;

            case ProjectileDamageCategory.Water:
                Freeze( other);
                break;

            case ProjectileDamageCategory.Wind:
                Debug.LogWarning("기능 추가 필요");
                break;

            case ProjectileDamageCategory.Earth:
                Debug.LogWarning("기능 추가 필요");
                break;

            case ProjectileDamageCategory.Lightning:
                Stun( other);
                break;

            case ProjectileDamageCategory.Light:
                Debug.LogWarning("기능 추가 필요");
                break;

            case ProjectileDamageCategory.Darkness:
                Debug.LogWarning("기능 추가 필요");
                break;
        }
            
    }





    // 주기에 따라 여러 번 데미지 주는 경우 데미지 처리 함수
    private IEnumerator DoTIntervalCoroutine()
    {
        // 세팅이 끝날 때까지 한 프레임 기다리기 (생성 후 처음에 데미지 안들어감)
        yield return null;

        // 생성 후 첫 데미지를 주고, 그 후 damageInterval 초 만큼 기다렸다가 다시 데미지를 줌
        while (true)
        {
            ApplyAreaDamage();

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void ApplyAreaDamage()
    {
        // 리스트 내에서 순차적으로 데미지 처리
        for (int i = dmgTrgets.Count - 1; i >= 0; i--)
        {
            Collider other = dmgTrgets[i];

            if (other != null && other.gameObject.activeInHierarchy == true)    // activeSelf 는 부모가 꺼져도 자식 오브젝트가 켜지면 true가 될 수 있어서 activeInHierarchy 로 처리
            {
                GiveDamageChance(other);
            }
            else
            {
                // 죽은 대상은 제거
                dmgTrgets.RemoveAt(i);
            }
        }
    }


    // 생명 주기 관련 함수들


    // 투사체가 지속 시간 이후에도 남아있다면 비활성화 시켜주는 코루틴, 오브젝트 풀링을 위해 사용
    IEnumerator LifeTimeCoroutine()
    {
        if (this.gameObject.activeInHierarchy == true) yield return new WaitForSeconds(lifeTime);
        SetEnableObject();
    }


    // 비활성화 관련 함수들

    // 충돌이나 지속 시간 등으로 비활성화시 리셋해야 하는 요소 모음
    private void SetEnableObject()
    {
        // Reset();
        // this.gameObject.SetActive(false);

        Destroy(this.gameObject);  // !!!!! 추후 오브젝트풀링 하면 삭제 예정
    }


    private void Reset()
    {
        // 오브젝트 풀링용으로 초기화 기능 추가 필요
    }


    // 타워랑 연결하는 코드
    public void SetTarget(Transform target, float attackPower)
    {
        _target = target;
        attackValue = (int)attackPower;


        // 발사 순간 방향 저장
        InitProjectile();


    }

    // 적의 위치에 생성되는 경우 소환 즉시 적의 위치로 이동시키기
    private void SetFirstPosition(Transform target)
    {
        this.transform.position = target.position;
    }


    // 속성 별 상태 효과 구현 함수들

    // 물리 -> 넉백
    public void KnockBack(Collider other)
    {
        other.GetComponent<Rigidbody>().isKinematic = false;
        other.GetComponent<Rigidbody>().AddForce(moveDirection * effectValue, ForceMode.Impulse);
        Debug.Log("넉백 실행");
    }

    // 불 -> 화상
    public void Burn(Collider other)
    {

    }

    // 물 -> 빙결
    public void Freeze(Collider other)
    {

    }

    // 번개 -> 스턴
    public void Stun(Collider other)
    {

    }








    // 데이터 초기화 이후 ProjectileSpwanType 에 맞춰서 스폰(setActive true) 처리


}
