using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour, IDamagable
{
    [Header("컴포넌트")] 
    [SerializeField] private MonsterAgent agent;
    
    // 데이터 보관
    private MonsterDatas stat;
    private MonsterResourcesDatas resource;

    private float currentHp;
    private bool isDead = false;
    
    // 매니저에게 반납하기 위한 이벤트
    public event Action<Monster> OnDeath;

    private void Awake()
    {
        agent = GetComponent<MonsterAgent>();
    }
    
    // MonsterManager가 소환 직후 호출
    public void Initialize(MonsterDatas statData, MonsterResourcesDatas ressData, Transform baseTransform)
    {
        this.stat = statData;
        this.resource = ressData;
        
        // 스탯 적용
        this.currentHp = statData.maxHP;
        this.isDead = false;
        
        // 디버깅용 이름 변경
        gameObject.name = $"{statData.name} _{statData.id}";
        gameObject.SetActive(true);
        
        // Agent 이동 시작
        if (agent != null)
        {
            agent.Initialize(statData.moveSpeed, baseTransform);
        }
    }

    public void TakeDamage(float attackValue, float ratio)
    {
        if (isDead) return;

        float myDef = stat.defenceValue;

        int finalDamage = DamageCalculator.CalculatingDamage((int)attackValue, ratio, (int)myDef);
        
        currentHp -= finalDamage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die(isKilledByPlayer: true);
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
            Debug.Log("기지 타격!");
        }

        OnDeath?.Invoke(this);
        
        MonsterManager.Instance.ReturnMonster(this);
    }

    private void ReachBase()
    {
        // 나중에 게임 매니저랑 연결 (플레이어에게 데미지 주기)
        // 
        Debug.Log("기지 도착");
        
        MonsterManager.Instance.ReturnMonster(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Base"))
        {
            Die(isKilledByPlayer: false);
        }
    }
}
