using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnumData : MonoBehaviour
{
    // 투사체가 어떻게 스폰되는지
    public enum ProjectileSpwanType
    {
        Default,   // 사용 안하는 디폴트 값 (버그 확인용)
        AttackerToTarget,   // 공격자 위치에서 생성되어 피격자 위치로 날아가는 투사체
        AttackerToTargetHoming, // 공격자 위치에서 생성되어 피격자 위치로 유도로 날아가는 투사체 
        AttackerToTargetInstance,   // 공격자 위치에서 생성되어 피격자 에게 즉각 피해를 주는 투사체, ex) 레이저빔 등
        AttackerPosition,   // 공격자의 위치에서 생성되는 투사체, ex) 원형 불 장판 등
        AttackerPositionTargetDirection, // 공격자의 위치에서 타겟의 방향으로 생성되는 투사체, 투사체 이동 X, ex) 파이어벳 화염 방사 등
    }

    // 투사체가 어느 팀에 피해를 주는지 (폭발 등 광역 피해가 있을 수 있어서 필요)
    public enum DamageTargetTeamType
    {
        Default,
        ForAll,    // 팀에 상관 없이 모두 피해를 줌
        Neutral,    // 중립이면 피해를 줌, ex) 생명력이 있는 맵 상 장애물 같은게 있으면 부수기 용
        Ally,   // 아군이면 피해를 줌, ex) 힐, 버프 등
        Enemy,   // 적군이면 피해를 줌, ex) 피해, 디버프 등
    }

    // 투사체의 특별한 능력이 있을 경우
    public enum ProjectileSpacialAbility
    {
        Default,
        None,   // 기믹 없는 일반 투사체
        Piercing,    // 적을 관통하는 투사체
        Explosive,   // 폭발하여 광역 피해를 주는 투사체
        Chain,  // 체인 피해를 주는 투사체 ex) 워크래프트 3 체인 라이트닝 등
        GroundDoT,  // 장판 지속 피해를 주는 투사체    
    }

    // 투사체의 공격 속성
    public enum ProjectileDamageCategory
    {
        Default,
        Physical,   // 무 속성 용 물리 속성
        Fire,   // 불 속성
        Water,  // 물 속성
        Wind,   // 바람 속성
        Earth,  // 땅 속성
        Lightning,  // 번개 속성
        Light,  // 빛 속성
        Darkness,   // 어둠 속성
    }
}
