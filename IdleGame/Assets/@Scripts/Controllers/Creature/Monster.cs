using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
	public override ECreatureState CreatureState 
	{
		get { return base.CreatureState; }
		set
		{
			if (_creatureState != value)
			{
				base.CreatureState = value;
				switch (value)
				{
					case ECreatureState.Idle:
						UpdateAITick = 0.5f;
						break;
					case ECreatureState.Move:
						UpdateAITick = 0.0f;
						break;
					case ECreatureState.Skill:
						UpdateAITick = 0.0f;
						break;
					case ECreatureState.Dead:
						UpdateAITick = 1.0f;
						break;
				}
			}
		}
	}

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = ECreatureType.Monster;

		StartCoroutine(CoUpdateAI());

		return true;
	}
    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        CreatureState = ECreatureState.Idle;
    }

    void Start()
	{
		_initPos = transform.position;
	}

	#region AI
	Vector3 _destPos; // 도착할 위치
	Vector3 _initPos; // Init에 넣으면 awake에 호출돼서 위치값이 안맞을 수 있다. start에 적용

	protected override void UpdateIdle()
	{
		// Patrol(순찰)
		{
			int patrolPercent = 10;
			int rand = Random.Range(0, 100);
			if (rand <= patrolPercent)
			{
				_destPos = _initPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2));
				CreatureState = ECreatureState.Move;
				return;
			}
		}

		// Search Player(플레이어 찾기)
		{
			Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
			if (creature != null)
			{
				Target = creature;
				CreatureState = ECreatureState.Move;
				return;
			}
		}
	}

	// 이동, 순찰 다양한 이동 함축적
	protected override void UpdateMove()
	{
		if (Target == null)
		{
			// Patrol or Return
			Vector3 dir = (_destPos - transform.position);

			if (dir.sqrMagnitude <= 0.01f) // float 특성상 0이 아니다
			{
				CreatureState = ECreatureState.Idle;
				return;
			}
			SetRigidBodyVelocity(dir.normalized * MoveSpeed) ;
		}
		else
		{
			// Chase
			ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, 5.0f);

			// 너무 멀어지면 포기
			if (Target.IsValid() == false)
			{
				Target = null;
				_destPos = _initPos;
				return;
			}
		}
	}

	protected override void UpdateSkill()
	{
		Debug.Log("<Color=red>Skill</color>");

		if (_coWait != null) // 일정시간을 기다렸다가 Move로 돌아간다.
			return;

		CreatureState = ECreatureState.Move;
	}

	protected override void UpdateDead()
	{
		Debug.Log("<Color=red>Dead</color>");

	}
	#endregion
}
