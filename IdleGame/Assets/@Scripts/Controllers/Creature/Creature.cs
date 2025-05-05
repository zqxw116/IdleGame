using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
    public BaseObject Target { get; protected set; }
    public SkillComponent Skills { get; protected set; }
    public Data.CreatureData CreatureData { get; protected set; }
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;
    public EffectComponent Effects { get; protected set; }

    float DistToTargetSqr   // ExtraCells 추가 적용
    {
        get
        {
            Vector3 dir = (Target.transform.position - transform.position);
            float distToTarget = Math.Max(0, dir.magnitude - Target.ExtraCells * 1f - ExtraCells * 1f); // TEMP
            return distToTarget * distToTarget;
        }
    }
    #region Stats
    public float Hp { get; set; }
    public CreatureStat MaxHp;
    public CreatureStat Atk;
    public CreatureStat CriRate;
    public CreatureStat CriDamage;
    public CreatureStat ReduceDamageRate;
    public CreatureStat LifeStealRate;
    public CreatureStat ThornsDamageRate; // 쏜즈
    public CreatureStat MoveSpeed;
    public CreatureStat AttackSpeedRate;
    #endregion


    protected float AttackDistance
    {
        get
        {
            float env = 2.2f;
            if (Target != null && Target.ObjectType == EObjectType.Env)
                return Mathf.Max(env, Collider.radius + Target.Collider.radius + 0.1f);

            float baseValue = CreatureData.AtkRange;
            return baseValue;
        }
    }


    protected ECreatureState _creatureState = ECreatureState.None;
	public virtual ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value) // 다른 값이 되면 애니메이션 실행
			{
				_creatureState = value;
				UpdateAnimation();
			}
		}
	}

	public override bool Init() // 생성자로 넣어도 된다 
	{
		if (base.Init() == false)
			return false;

		ObjectType = EObjectType.Creature;
		return true;
	}

	public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;
		if (CreatureType == ECreatureType.Hero)
			CreatureData = Managers.Data.HeroDic[templateID];
		else
			CreatureData = Managers.Data.MonsterDic[templateID];
		

        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        // Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.radius = CreatureData.ColliderRadius;

        // RigidBody
        RigidBody.mass = 0;

        // Spine
        SetSpineAnimation(CreatureData.SkeletonDataID, SortingLayers.CREATURE);


		// Skill
		Skills = gameObject.GetOrAddComponent<SkillComponent>();
		Skills.SetInfo(this, CreatureData);

        // Stat
        Hp = CreatureData.MaxHp;
        MaxHp = new CreatureStat(CreatureData.MaxHp);
        Atk = new CreatureStat(CreatureData.Atk);
        CriRate = new CreatureStat(CreatureData.CriRate);
        CriDamage = new CreatureStat(CreatureData.CriDamage);
        ReduceDamageRate = new CreatureStat(0);
        LifeStealRate = new CreatureStat(0);
        ThornsDamageRate = new CreatureStat(0);
        MoveSpeed = new CreatureStat(CreatureData.MoveSpeed);
        AttackSpeedRate = new CreatureStat(1);

        // State
        CreatureState = ECreatureState.Idle;
        
        // Effect
        Effects = gameObject.AddComponent<EffectComponent>();
        Effects.SetInfo(this);

        // Map
        // 매 프레임마다 cell 위치로 스르륵 이동
        StartCoroutine(CoLerpToCellPos());
    }

	protected override void UpdateAnimation()
	{
		switch (CreatureState)
		{
			case ECreatureState.Idle:
				PlayAnimation(0, AnimName.IDLE, true);
				break;
			case ECreatureState.Skill:
				//PlayAnimation(0, AnimName.ATTACK_A, true);
				break;
			case ECreatureState.Move:
				PlayAnimation(0, AnimName.MOVE, true);
                break;
			case ECreatureState.OnDamaged:
				PlayAnimation(0, AnimName.IDLE, true);
                Skills.CurrentSkill.CancelSkill();
				break;
			case ECreatureState.Dead:
				PlayAnimation(0, AnimName.DEAD, true);
				RigidBody.simulated = false;
				break;
			default:
				break;
		}
	}


    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;

	protected IEnumerator CoUpdateAI()
	{
		while (true)
		{
			switch (CreatureState)
			{
				case ECreatureState.Idle:
					UpdateIdle();
					break;
				case ECreatureState.Move:
					UpdateMove();
					break;
				case ECreatureState.Skill:
					UpdateSkill();
					break;
				case ECreatureState.OnDamaged:
                    UpdateOnDamaged();
					break;
				case ECreatureState.Dead:
					UpdateDead();
					break;
			}

			if (UpdateAITick > 0)
				yield return new WaitForSeconds(UpdateAITick);
			else
				yield return null;
		}
	}

	protected virtual void UpdateIdle() { }
	protected virtual void UpdateMove() { }
	protected virtual void UpdateSkill() 
	{

        if (_coWait != null)
            return;

        if (Target.IsValid() == false || Target.ObjectType == EObjectType.HeroCamp)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = AttackDistance * AttackDistance;
        if (distToTargetSqr > attackDistanceSqr)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        // DoSkill
        Skills.CurrentSkill.DoSkill();

        LookAtTarget(Target);

        var trackEntry = SkeletonAnim.state.GetCurrent(0);
        float delay = trackEntry.Animation.Duration;

        StartWait(delay);
    }
	protected virtual void UpdateOnDamaged() { }
	protected virtual void UpdateDead() { }
    #endregion

    #region Wait
    protected Coroutine _coWait;

    protected void StartWait(float seconds)
    {
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancelWait()
    {
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
		base.OnDamaged(attacker, skill);
		if (attacker.IsValid() == false)
			return;

		Creature creature = attacker as Creature;
		if (creature == null)
			return;

		float finalDamage = creature.Atk.Value; // TODO
		Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp.Value);

        Managers.Object.ShowDamageFont(CenterPosition, finalDamage, transform, false);

		if (Hp <= 0)
		{
			OnDead(attacker, skill);
			CreatureState = ECreatureState.Dead;
		}

        // 스킬에 다른 이펙트 적용. 스킬 데이터 시트에 있는 이펙트를 붙여주겠다. 큰 프로젝트일수록 데이터 시트가 복잡해진다.
        if (skill.SkillData.EffectIds != null)
            Effects.GenerateEffects(skill.SkillData.EffectIds.ToArray(), EEffectSpawnType.Skill);
    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
		base.OnDead(attacker, skill);

		// TODO : Drop Item

		//Managers.Object.Despawn(this);
    }

    /// <summary>
    /// 주변 오브젝트 찾기
    /// </summary>
    protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
    {
        BaseObject target = null;
        float bestDistanceSqr = float.MaxValue;
        float searchDistanceSqr = range * range;

        foreach (BaseObject obj in objs)
        {
            Vector3 dir = obj.transform.position - transform.position;
            float distToTargetSqr = dir.sqrMagnitude;

            // 서치 범위보다 멀리 있으면 스킵.
            if (distToTargetSqr > searchDistanceSqr)
                continue;

            // 이미 더 좋은 후보를 찾았으면 스킵.
            if (distToTargetSqr > bestDistanceSqr)
                continue;

            // 추가조건
            if (func?.Invoke(obj) == false)
                continue;

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    /// <summary>
    /// 쫒아가거나 공격을 하겠다
    /// </summary>
    protected void ChaseOrAttackTarget(float chaseRange, float attackRange)
    {
        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = attackRange * attackRange;

        if (distToTargetSqr <= attackDistanceSqr)
        {
            // 공격 범위 이내로 들어왔다면 공격.
            CreatureState = ECreatureState.Skill;
            //skill.DoSkill();
            return;
        }
        else
        {
			// 공격 범위 밖이라면 추적.
			FindPathAndMoveToCellPos(Target.transform.position, HERO_DEFAULT_MOVE_DEPTH);

			// 너무 멀어지면 포기.
			float searchDistanceSqr = chaseRange * chaseRange;
            if (distToTargetSqr > searchDistanceSqr)
            {
                Target = null;
                CreatureState = ECreatureState.Move;
            }
            return;
        }
    }
    #endregion

    #region Mise
    protected bool IsValid(BaseObject bo)
    {
        return bo.IsValid();
    }
	#endregion


	#region Map, A*
	/// <summary>
	/// 이전에 velocity로 이동하는 부분을 대체하는 함수
	/// </summary>
	public EFindPathResult FindPathAndMoveToCellPos(Vector3 destWorldPos, int maxDepth, bool forceMoveCloser = false)
	{
		Vector3Int destCellPos = Managers.Map.World2Cell(destWorldPos);
		return FindPathAndMoveToCellPos(destCellPos, maxDepth, forceMoveCloser);
	}

	public EFindPathResult FindPathAndMoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
	{
		if (LerpCellPosCompleted == false)
			return EFindPathResult.Fail_LerpCell;

        // A* 길찾기
        List<Vector3Int> path = Managers.Map.FindPath(this, CellPos, destCellPos, maxDepth);
        if (path.Count < 2)
            return EFindPathResult.Fail_NoPath;

        if (forceMoveCloser) // 주변에 막고 있는게 많아서 특정 부분에서 와리가리 하는 상황이 생길 때가 있다.
        {
            Vector3Int diff1 = CellPos - destCellPos;
            Vector3Int diff2 = path[1] - destCellPos;
            if (diff1.sqrMagnitude <= diff2.sqrMagnitude)
                return EFindPathResult.Fail_NoPath;
        }

        Vector3Int dirCellPos = path[1] - CellPos;	// 추후 한칸씩 이동하게?
		Vector3Int nextPos = CellPos + dirCellPos;

		if (Managers.Map.MoveTo(this, nextPos) == false)
			return EFindPathResult.Fail_MoveTo;

		return EFindPathResult.Success;
	}

	public bool MoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
	{
		if (LerpCellPosCompleted == false)
			return false;

		return Managers.Map.MoveTo(this, destCellPos);
	}

	/// <summary>
	/// Lerp 해서 Cell 위치로 스르륵 이동
	/// </summary>
	protected IEnumerator CoLerpToCellPos()
	{
		while (true)
		{
			Hero hero = this as Hero;
			if (hero != null)
			{
				float div = 5;
				Vector3 campPos = Managers.Object.Camp.Destination.transform.position;
				Vector3Int campCellPos = Managers.Map.World2Cell(campPos);
				float ratio = Math.Max(1, (CellPos - campCellPos).magnitude / div);

				LerpToCellPos(CreatureData.MoveSpeed * ratio);
			}
			else
				LerpToCellPos(CreatureData.MoveSpeed);

			yield return null;
		}
	}
    #endregion




#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 점 색상 설정
        Gizmos.color = Color.red;

        // 현재 transform 위치에 작은 구(점)를 그림
        Gizmos.DrawSphere(Managers.Map.Cell2World(CellPos), 0.5f);


        // 점 색상 설정
        Gizmos.color = Color.yellow;

        int extraCells = this.ExtraCells;

        for (int dx = -extraCells; dx <= extraCells; dx++)
        {
            for (int dy = -extraCells; dy <= extraCells; dy++)
            {
                Vector3Int checkPos = new Vector3Int(CellPos.x + dx, CellPos.y + dy);

                Gizmos.DrawSphere(Managers.Map.Cell2World(checkPos), 0.3f);
            }
        }
    }

#endif
}
