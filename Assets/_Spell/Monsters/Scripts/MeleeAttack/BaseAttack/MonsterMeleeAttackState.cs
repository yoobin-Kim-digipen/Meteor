using System.Collections.Generic;
using UnityEngine;

public class MonsterMeleeAttackState : MonsterBaseState
{
    public MeleeMonsterData MeleeData { get; private set; }
    public MonsterFSM Monster { get { return _monster; } }
    private IAttackPhase _currentPhase; // 현재 진행 중인 공격 단계
    private Dictionary<System.Type, IAttackPhase> _phases; // 모든 공격 단계를 저장하는 사전

    public MonsterMeleeAttackState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory)
    {
        // 모든 하위 상태(Phase) 객체들을 미리 생성해서 사전에 등록
        _phases = new Dictionary<System.Type, IAttackPhase>
        {
            { typeof(WindupPhase), new WindupPhase(this) },
            { typeof(ActionPhase), new ActionPhase(this) },
            { typeof(RecoveryPhase), new RecoveryPhase(this) }
        };
    }

    public override void EnterState()
    {
        MeleeData = _monster.monsterData as MeleeMonsterData;

        if (MeleeData == null)
        {
            Debug.LogError("MeleeMonsterData가 할당되지 않았습니다! Chase 상태로 돌아갑니다.");
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        _monster.agent.isStopped = true;
        SwitchPhase(typeof(WindupPhase));
    }

    public override void UpdateState()
    {
        if (!_monster.IsPlayerInAttackRange() && _currentPhase is RecoveryPhase)
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        if (_currentPhase != null)
        {   
            _currentPhase.UpdatePhase();
        }
    }

    public override void ExitState()
    {
        _monster.agent.isStopped = false;

        if (_currentPhase != null)
        {
            _currentPhase.ExitPhase();
        }
    }

    public void SwitchPhase(System.Type nextPhaseType)
    {
        if (_currentPhase != null)
        {
            _currentPhase.ExitPhase();
        }
        _currentPhase = _phases[nextPhaseType];
        _currentPhase.EnterPhase();
    }

    public void BackToChase()
    {
        _ctx.SwitchState(_factory.Chase());
    }
}