using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
	public Data.MonsterData MonsterData { get { return (Data.MonsterData)CreatureData; } }
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

		ObjectType = EObjectType.Monster;

		StartCoroutine(CoUpdateAI());

		return true;
	}
	public override void SetInfo(int templateID)
	{
		base.SetInfo(templateID);

		// State
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
		if (Target.IsValid() == false)
		{
			// Patrol or Return
			Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
			if (creature != null)
			{
				Target = creature;
				CreatureState = ECreatureState.Move;
				return;
			}

			// Move
			FindPathAndMoveToCellPos(_destPos, MONSTER_DEFAULT_MOVE_DEPTH);

			if (LerpCellPosCompleted)
			{
				CreatureState = ECreatureState.Idle;
				return;
			}
		}
		else
		{
			ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, AttackDistance);

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
		base.UpdateSkill();

		if (Target.IsValid() == false)
		{
			Target = null;
			_destPos = _initPos;
			CreatureState = ECreatureState.Move;
			return;
		}


	}

	protected override void UpdateDead()
	{
		Debug.Log($"<Color=red>[Dead]</color> {this.name}");

		//SetRigidBodyVelocity(Vector2.zero);
	}
	#endregion


	#region Battle
	public override void OnDamaged(BaseObject attacker, SkillBase skill)
	{
		base.OnDamaged(attacker, skill);
	}

	public override void OnDead(BaseObject attacker, SkillBase skill)
	{
		base.OnDead(attacker, skill);
		int dropItemId = MonsterData.DropItemId;

        RewardData rewardData = GetRandomReward();
        if (rewardData != null)
        {
            var itemHolder = Managers.Object.Spawn<ItemHolder>(transform.position, dropItemId);
            Vector2 ran = new Vector2(transform.position.x + Random.Range(-10, -15) * 0.1f, transform.position.y);
            Vector2 ran2 = new Vector2(transform.position.x + Random.Range(10, 15) * 0.1f, transform.position.y);
            Vector2 dropPos = Random.value < 0.5 ? ran : ran2;
            itemHolder.SetInfo(0, rewardData.ItemTemplateId, dropPos);
        }

		// Broadcast
		Managers.Game.BroadcastEvent(EBroadcastEventType.KillMonster, MonsterData.DataId);


        Managers.Object.Despawn(this);
	}
	#endregion

	RewardData GetRandomReward()
	{
		if (MonsterData == null)
			return null;

		if (Managers.Data.DropTableDic.TryGetValue(MonsterData.DropItemId, out var dropTableData) == false)
			return null;

		if (dropTableData.Rewards.Count <= 0)
			return null;

		int sum = 0;
		int randValue = UnityEngine.Random.Range(0, 100);
		foreach (var item in dropTableData.Rewards)
		{
			sum += item.Probability;
			if (randValue <= sum)
				return item;
		}
		//return dropTableData.Rewards.RandomElementByWeight(e => e.Probability);
		return null;
	}


	int GetRewardExp()
	{
		if (MonsterData == null)
			return 0;

		if (Managers.Data.DropTableDic.TryGetValue(MonsterData.DropItemId, out DropTableData dropTableData) == false)
			return 0;

        return dropTableData.RewardExp;
    }
}