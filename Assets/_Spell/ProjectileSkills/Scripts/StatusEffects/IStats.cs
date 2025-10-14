// 능력치를 가지고 있는 모든 캐릭터(플레이어, 몬스터)가 구현할 인터페이스
public interface IStats
{
    // 외부에서 현재 이동 속도를 읽고 쓸 수 있도록 허용
    float MoveSpeed { get; set; }

    // 나중에 여기에 다른 공통 능력치를 추가할 수 있음 (ex 방어력 감소, 공격력 증가, 각종 버프 디버프 요소들)
    // float AttackSpeed { get; set; }
    // float Defense { get; set; }
}