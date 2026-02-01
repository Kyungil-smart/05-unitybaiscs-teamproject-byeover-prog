using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectileEnumData;
using static TowerEnumData;
using static UnityEngine.GraphicsBuffer;

// 작성자 : 문형근
// 수정 : 한성우

// 자 이제 무엇을 할까요 

// 타워의 기본 속성 및 기능 관리
// 타워 사거리, 공격력, 공격속도, 타겟팅 방식 등 관리
// 적 감지, 타겟정하기, 공격 실행 등 기능 구현 예정  
public class Tower_Re : MonoBehaviour, IDamagable
{
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int level;
    [SerializeField] private TowerType towerType;
    public float _towerheals; // 타워 최대 체력
    public int _currentheals; // 타워 현재 체력
    [SerializeField] private attackType attackType;
    public float _damage;         // 타워 공격력
    public float _range;          // 타워 사거리
    [SerializeField] private int attackProjectileIDs;
    public float _attackSpeed;    // 타워 공격속도
    [SerializeField] private int defenceValue;
    [SerializeField] private int towerCost;




    public enum TargetingMode { Nearest } // 가까운 몬스터 찾기 
    [SerializeField] private Transform _currentTarget;  // 현재 타겟

    //☆★☆★ 테스트 프리팹이라 나중에 이거 다 바꿔야함 ☆★☆★
    public GameObject _bulletPrefab; // 총알 프리팹 (test용) 
    public Transform _firePoint; // 총알 발 사 위치
    private float _attackTimer = 0f; // 공격 타이머 (에임 타이머 같이 활용 가능)


    // 자신에게 붙은 TowerStats 에서 필요한 변수 받아오기 (만약에 스크립트가 없다면 예외 처리 어떻게할지 고민 필요)
    public void GetStats(TowerStats stats)
    {
        
        if (stats == null) return;
        else
        {
            id = stats.id;
            name = stats.name;
            level = stats.level;
            towerType = stats.towerType;
            _towerheals = stats.maxHP;
            attackType = stats.attackType;
            _damage = stats.attackValue;
            _range = stats.attackRange;
            attackProjectileIDs = stats.attackProjectileIDs;
            _attackSpeed = stats.attackSpeed;
            defenceValue = stats.defenceValue;
            towerCost = stats.towerCost;
        }

        // 0 이하라면 ~
        if (_range <= 0) _range = 0;
        if (_damage <= 0) _damage = 0;
        if (_attackSpeed <= 0) _attackSpeed = 0;

        _currentheals = (int)_towerheals;
    }





        void Start()
    {
        _currentTarget = null;  // 현재 타겟은 기본적으로 null 값임
        

    }

    void Update()
    {
        // 공격하지 않는 타워면 리턴시켜 에러 안나도록
        if (attackType == attackType.None || _attackSpeed <= 0) return;

        CheckTarget(); // 타겟이 범위 안에 있는지
        FindTarget(); // 타겟 찾는 함수 진행
        Attack(); // 공격 추가

    }


    // 만약에 타워가 공격하지 않는다면 타겟을 찾는다
    // 공격을 안하면 타겟을 찾는다
    // 공격 중엔 타겟을 차지 않는다 

    public void FindTarget() //타겟 찾는 경로
    {
        // 타겟을 찾자        
        //Debug.Log("타겟 찾는 로직 시작");

        // 타겟이 있으면 찾지 않아
        if (_currentTarget != null)
        {
            return;
        }

        // 범위 안에 있는 Collider들 다 찾아 
        Collider[] colliders = Physics.OverlapSphere(transform.position, _range);

        foreach (Collider col in colliders)
        {
            //타겟이 Monster로 되어 있는 오브젝트만 찾는다
            if (col.CompareTag("Monster"))
            {
                _currentTarget = col.transform;

                // ## 타겟 찾으면 포탐 회전 동기화, 안전을 위해 if 문 안에 넣기
                if (gameObject.GetComponent<TurretRotation>() != null)
                {
                    gameObject.GetComponent<TurretRotation>().SetTurretRotationTarget(_currentTarget);
                }
                break;
            }
        }
        // 이렇게 Debug.Log 쓰면 의미가 있는지 모르겠네요 문제 있으면 여기라고 말하고 싶은데
        //Debug.Log("타겟 찾는 로직 끝");
    }

    //타겟이 죽거나 범위 밖으로 나갈 경우를 타겟을 해제한다 

    private void CheckTarget()
    {
        // 타겟이 없으면 이 코드 진행할 필요 없어 (타겟이 죽거나 잡힌적 없거나)
        // 이렇게 하면 타워랑 몬스터랑 같이 인식 될 거 같은데 ...음...
        if (_currentTarget == null || _currentTarget.gameObject == null)
        {
            return;
        }

        //타겟이 범위에서 나갔을때
        float distrance = Vector3.Distance(transform.position, _currentTarget.position);
        if (distrance > _range)
        {
            _currentTarget = null;

            // ## 포탑 회전 스크립트 있으면 범위 나갔을때 null 값 주기
            if (gameObject.GetComponent<TurretRotation>() != null)
            {
                gameObject.GetComponent<TurretRotation>().SetTurretRotationTarget(_currentTarget);
            }
        }

        //Debug.Log("타겟 범위 찾기 완료");
    }

    //타겟 공격
    private void Attack()
    {
        // 타겟이 없으면 공격 안함
        if (_currentTarget == null)
        {
            return;
        }

        // 공격 쿨타임 설정
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= 1f / _attackSpeed)  // ## 유니티 여러 기능 기준이 초라서 1f로 수정함
        {


            // 발사 순간을 bullet에게 타겟을 넘겨줘야하는데
            GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);

            // 총알을 타겟에게 주고 bullet 스크립트 완성 되어야함 완성되면 아래 내용 추가
            // ## 안전을 위해 if 문 안으로 넣기
            //bullet.GetComponent<Bullet>().SetTarget(_currentTarget, _damage);
            if (bullet != null)
            {
                bullet.GetComponent<Projectile>().SetTarget(_currentTarget, _damage);
            }
            else Debug.LogError("투사체가 없습니다.");

            _attackTimer = 0f; // 끝나면 타이머 초기화 -> 초기화 해야 나중에 0부터 계산함
        }
    }



    // 타워가 데미지를 받는 함수
    public void TakeDamage(float damage, float ratio)
    {
        // if (isDead) return; 오브젝트 풀링으로 바꾸면 안정성을 위해 추가 필요

        _currentheals -= (int)damage;

        if (_currentheals <= 0)
        {
            _currentheals = 0;

            /*파괴 이펙트 추가 필요*/


            Destroy(gameObject, 0.5f);  // 나중에 오브젝트 풀링으로 수정할 수 있으면 바꿔야 함
        }
    }
}
