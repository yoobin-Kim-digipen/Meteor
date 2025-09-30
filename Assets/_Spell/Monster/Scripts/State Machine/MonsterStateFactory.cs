public class MonsterStateFactory
{
    private MonsterStateMachine _context;

    // 각 상태 객체를 저장할 변수들
    private MonsterChaseState _chaseState;
    private MonsterAttackState _attackState; // 근접 공격 상태
    private MonsterRangedAttackState _rangedAttackState; // 원거리 공격 상태

    public MonsterStateFactory(MonsterStateMachine currentContext)
    {
        _context = currentContext;
        _chaseState = new MonsterChaseState(_context, this);
        _attackState = new MonsterAttackState(_context, this);
        _rangedAttackState = new MonsterRangedAttackState(_context, this);
    }

    // 상태를 요청하는 메서드들
    public MonsterBaseState Chase()
    {
        return _chaseState;
    }

    public MonsterBaseState Attack()
    {
        // 몬스터의 데이터 타입을 확인하여 적절한 공격 상태를 반환
        if (_context.MonsterFSM.monsterData is RangeMonsterData)
        {
            return _rangedAttackState;
        }
        else
        {
            return _attackState;
        }
    }
}