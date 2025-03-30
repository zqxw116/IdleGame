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
		Speed = 3.0f;

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
	public float SearchDistance { get; private set; } = 8.0f;
	public float AttackDistance { get; private set; } = 4.0f;
	Creature _target;
	Vector3 _destPos; // 도착할 위치
	Vector3 _initPos; // Init에 넣으면 awake에 호출돼서 위치값이 안맞을 수 있다. start에 적용

	protected override void UpdateIdle()
	{
		Debug.Log("<Color=red>Idle</color>");

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
			Creature target = null;
			float bestDistanceSqr = float.MaxValue;// 처음에는 큰값
			float searchDistanceSqr = SearchDistance * SearchDistance; // 거리의 제곱을 비교. 루트를 사용하는 것 보다 효율성이 더 좋기 때문. 

			// 너무 최적화를 신경쓰기에는 클라에서 수백개의 오브젝트는 괜찮다.   코드 진행하는 것이 중요.
			foreach (Hero hero in Managers.Object.Heroes) // 유닛을 다 돌아본다.
			{
				Vector3 dir = hero.transform.position - transform.position;
				float distToTargetSqr = dir.sqrMagnitude; // 루트를 사용하지 않는다.

				Debug.Log(distToTargetSqr);

				if (distToTargetSqr > searchDistanceSqr) // 검색범위를 넘었으면 패스
					continue;

				if (distToTargetSqr > bestDistanceSqr)   // 이미 나보다 가까우면 패스
					continue;

				target = hero;
				bestDistanceSqr = distToTargetSqr;
			}

			_target = target; // 가장 가까운 것 찾음!

			if (_target != null)
			{
				CreatureState = ECreatureState.Move;				
				// ?~~
			}
		}
	}

	// 이동, 순찰 다양한 이동 함축적
	protected override void UpdateMove()
	{
		Debug.Log("<Color=red>Move</color>");

		if (_target == null)
		{
			// Patrol or Return
			Vector3 dir = (_destPos - transform.position);
			float moveDist = Mathf.Min(dir.magnitude, Time.deltaTime * Speed);
			transform.TranslateEx(dir.normalized * moveDist);

			if (dir.sqrMagnitude <= 0.01f) // float 특성상 0이 아니다
			{
				CreatureState = ECreatureState.Idle;
			}
		}
		else
		{
			// Chase
			Vector3 dir = (_target.transform.position - transform.position);
			float distToTargetSqr = dir.sqrMagnitude;
			float attackDistanceSqr = AttackDistance * AttackDistance;

			if (distToTargetSqr < attackDistanceSqr)
			{
				// 공격 범위 이내로 들어왔으면 공격.
				CreatureState = ECreatureState.Skill;
				StartWait(2.0f);
			}
			else
			{
				// 공격 범위 밖이라면 추적.
				float moveDist = Mathf.Min(dir.magnitude, Time.deltaTime * Speed); // 거리는 10인데 speed한 값이 20이면 초과해서 이동할테니 넘어가지 않게 최소값
				transform.TranslateEx(dir.normalized * moveDist);

				// 너무 멀어지면 포기.
				float searchDistanceSqr = SearchDistance * SearchDistance;
				if (distToTargetSqr > searchDistanceSqr)
				{
					_destPos = _initPos;
					_target = null; // 다음 프레임에서 update 들어오면 Idle로 변경되게 됨
					CreatureState = ECreatureState.Move;
				}
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
