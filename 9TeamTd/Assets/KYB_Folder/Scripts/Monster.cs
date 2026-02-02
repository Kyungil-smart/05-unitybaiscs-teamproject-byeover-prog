using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    private bool isDead = false;
    
    // Json 변수들
    [SerializeField]private int id;
    [SerializeField]private string name;
    [SerializeField]private int level;
    [SerializeField]private int maxHP;
    [SerializeField]private float attackValue;
    [SerializeField]private float defenceValue;
    [SerializeField]private int Type;
    [SerializeField]private string enemyRank;
    [SerializeField]private float moveSpeed;

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
    }
    
    // 매니저에게 반납하기 위한 이벤트
    public event Action<Monster> OnDeath;

    private void Awake()
    {
        agent = GetComponent<MonsterAgent>();
    }
    
    // MonsterManager가 소환 직후 호출
    public void Initialize(MonsterDatas statData, MonsterResourcesDatas resData, Transform baseTransform)
    {
        this.stat = statData;
        this.resource = resData;
        
        // 스탯 적용
        this.currentHp.Value = statData.maxHP;
        this.isDead = false;
        
        // 디버깅용 이름 변경
        gameObject.name = $"{statData.name} _{statData.id}";
        gameObject.SetActive(true);
        
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
        
        currentHp.Value -= finalDamage;
        // Debug.Log($"{finalDamage}피해 입음. 남은 체력: {currentHp}"); << 주석 빼도 됩니다
        
        if (currentHp.Value <= 0)
        {
            currentHp.Value = 0;
            Die(isKilledByPlayer: true);
            Destroy(gameObject);
        }
    }

    private void Die(bool isKilledByPlayer)
    {
        isDead = true;

        if (isKilledByPlayer)
        {
            // 골드 보상 (나중에 GameManager 생기면 연결)
            // GameManager.Instance.골드얻기(resource.gold);
            Debug.Log($"골드 획득: {resource.gold}");

            if (UnityEngine.Random.value <= resource.DropProp)
            {
                Debug.Log($"아이템 드랍 성공! ID: {resource.DropItemId}");
                // ItemManager.Instance.DropItem(transform.position, resource.dropItem);
            }
        }
        else
        {
            // 기지에서 죽음 (보상 X)
            Debug.Log("기지 타격! 후 소멸");
        }

        OnDeath?.Invoke(this);
        
        // 풀 반납
        MonsterManager.Instance.ReturnMonster(this);
    }
    
    private void OnTriggerEnter(Collider other)
    {
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
                baseTarget.TakeDamage(stat.attackValue, 1.0f);
            }
            else
            {
                Debug.LogWarning("기지에 IDamageble 스크립트가 없습니다");
            }
            
            Die(isKilledByPlayer: false);
        }
    }
}
