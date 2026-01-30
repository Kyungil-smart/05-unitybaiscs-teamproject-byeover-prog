using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 문형근
public class Bullet : MonoBehaviour
{
    // 테스트용 총알 함수 
    // 발사 된 것을 구현하기 위해서 
    private Transform _target; // 타겟
    private float _speed = 20f; // 총알 속도
    private float _damage; // 데미지
    private Vector3 _direction; // 총알 방향 저장

    void Start()
    {
        // 살아 남은 총알은 3초 후 자동 삭제 (이정도면 맵 밖에 나갈 거 같아서)
        Destroy(gameObject,3f);
    }

    
    void Update()
    {
        // 저장된 방향으로 이동
        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //타겟이 몬스터여야만함
        if (other.CompareTag("Monster"))
        {
            //몬스터에게 데미지 주기 (몬스터 스크립트생기면 해주면됨)
            // other.GetComponent<Monster>().TakeDamage(_damage);
            
            //총알 삭제
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform target, float damage)
    {
        _target = target;
        _damage = damage;

        // 발사 순간 방향 저장
        _direction = (target.position - transform.position).normalized;
    }
}
