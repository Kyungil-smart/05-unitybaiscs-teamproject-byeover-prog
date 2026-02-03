using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MonsterType
{
    Normal = 0,
    Flying = 1,
    Boss = 2
}

public class Monster : MonoBehaviour, IDamagable
{
    [Header("컴포넌트")] 
    [SerializeField] private MonsterAgent agent;

    // 데이터 보관
    private MonsterDatas stat;
    private MonsterResourcesDatas resource;

    public OP<int> currentHp = new();

    [SerializeField] private bool isDead = false;

    // Json 변수들
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int level;
    [SerializeField] private int maxHP;
    [SerializeField] private float attackValue;
    [SerializeField] private float defenceValue;
    [SerializeField] private int Type;
    [SerializeField] private string enemyRank;
    [SerializeField] private float moveSpeed;

    // 상태이상 변수들
    [SerializeField] private bool isFreeze = false;
    [SerializeField] private bool isStun = false;
    [SerializeField] private float originalSpeed;

    public void GetMonsterStats(MonsterStats stats)
    {
        if (stats == null) return;

        id = stats.id;
        name = stats.name;
        level = stats.level;
        maxHP = stats.maxHP;
        attackValue = stats.attackValue;
        defenceValue = stats.defenceValue;
        Type = stats.Type;
        moveSpeed = stats.moveSpeed;
        enemyRank = stats.enemyRank;

        originalSpeed = stats.moveSpeed;
    }

    // 매니저에게 반납하기 위한 이벤트
    public event Action<Monster> OnDeath;

    // 디버그 확인용
    [Header("Hp 확인용 디버그 전용")] [SerializeField]
    private int debugCurrentHpView;

    private void Awake()
    {
        agent = GetComponent<MonsterAgent>();
    }

    // MonsterManager가 소환 직후 호출
    public void Initialize(MonsterDatas statData, MonsterResourcesDatas resData, Transform baseTransform)
    {
        if (resData == null)
        {
            Debug.LogError($"{statData.id}번 몬스터의 Resource 데이터가 Null입니다.");
        }
        else
        {
            Debug.Log($"[Monster] {statData.id}번의 리소스 로드 성공, 골드: {resData.gold}, 아이템ID: {resData.DropItemId}");
        }
        
        
        this.stat = statData;
        this.resource = resData;

        // 스탯 적용
        this.currentHp.Value = statData.maxHP;
        this.debugCurrentHpView = statData.maxHP; // 인스펙터 확인용
        this.isDead = false;

        // 디버깅용 이름 변경
        gameObject.name = $"{statData.name} _{statData.id}";

        // Agent 이동 시작
        if (agent != null)
        {
            bool isFlying = stat.Type == 1;
            agent.Initialize(statData.moveSpeed, baseTransform, isFlying);
        }
    }

    public void TakeDamage(float attackValue, float ratio)
    {
        if (isDead) return;

        float myDef = stat.defenceValue;

        // 데미지 계산
        int finalDamage = DamageCalculator.CalculatingDamage((int)attackValue, ratio, (int)myDef);

        if (finalDamage < 1) finalDamage = 1;

        currentHp.Value -= finalDamage;
        debugCurrentHpView = currentHp.Value; // 인스펙터 갱신
        Debug.Log($"{finalDamage}피해 입음. 남은 생명력 : {currentHp.Value}"); //<< 주석 빼도 됩니다

        if (currentHp.Value <= 0)
        {
            currentHp.Value = 0;
            Debug.Log(11);
            DestroyMonster();
            Debug.Log(22);
        }
        // Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Base"))
        {
            IDamagable baseTarget = other.gameObject.GetComponent<IDamagable>();

            if (baseTarget != null)
            {
                float finalDamage = stat.attackValue;

                if (stat.Type == 2)
                {
                    Debug.Log("보스 기지 충돌! 게임 오버");
                    finalDamage = 99999999999f;
                }

                // 몬스터 공격력만큼 기지에 데미지 주기 (비율은 1.0)
                baseTarget.TakeDamage(finalDamage, 1.0f);
            }

            //  Debug.Log(1);
            DestroyMonster();
            // Debug.Log(8);
        }
    }


    public void DestroyMonster()
    {
        if (isDead) return;
        isDead = true;
        
        StageManager.Instance.GetGold(resource.gold * 10);

        if (resource != null && !string.IsNullOrEmpty(resource.DropItemId))
        {
            StageManager.Instance.TryDropItem(resource.DropItemId, resource.DropProp, transform.position);
        }

        // Debug.Log(5);
        OnDeath?.Invoke(this);
        // Debug.Log(6);
        GameObject deathEffectPrefab = Resources.Load<GameObject>("VisualEffectPrafabs/VE_DestroyExplosion_03");
        Destroy(Instantiate(deathEffectPrefab, transform.position, Quaternion.identity), 0.3f);
        Destroy(gameObject);
    }


    // 피격 시 속성 별 상태 효과 구현 함수들

    // 물리 -> 넉백
    public void KnockBack(Vector3 moveDirection, float effectValue, float duration)
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().AddForce(moveDirection * effectValue, ForceMode.Impulse);

        Debug.Log("넉백 실행");
        StartCoroutine(EffectTimeKinematic(duration));
    }

    // 불 -> 화상
    public void Burn()
    {
    }

    // 물 -> 빙결
    public void Freeze(float effectValue, float duration, int overlapCount)
    {
        if (isFreeze) return;

        isFreeze = true;
        moveSpeed = Math.Clamp(originalSpeed * (1 + effectValue), 0.05f, moveSpeed);
        Debug.Log(moveSpeed);
        StartCoroutine(EffectTimeFreeze(duration));
        // 추후 오버렙 카운트 사용되도록 수정 필요
    }

    // 번개 -> 스턴
    public void Stun(float duration)
    {
        if (isStun) return;

        isStun = true;
        moveSpeed = originalSpeed * 0;
        Debug.Log(moveSpeed);
        StartCoroutine(EffectTimeStun(duration));
    }


    IEnumerator EffectTimeFreeze(float time)
    {
        Debug.Log("빙결");
        yield return new WaitForSeconds(time);
        moveSpeed = originalSpeed;
        Debug.Log(moveSpeed);
        isFreeze = false;
        Debug.LogWarning("빙결 종료");
    }


    IEnumerator EffectTimeStun(float time)
    {
        Debug.Log("스턴");
        yield return new WaitForSeconds(time);
        moveSpeed = originalSpeed;
        Debug.Log(moveSpeed);
        isStun = false;
        Debug.LogWarning("스턴 종료");
    }


    IEnumerator EffectTimeKinematic(float time)
    {
        yield return new WaitForSeconds(time);
        GetComponent<Rigidbody>().isKinematic = true;
    }
}