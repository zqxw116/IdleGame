using Spine;
using System;
using UnityEngine;
using static Define;

public class Hero : Creature
{
    bool _needArrange = true;
    public bool NeedArrange // 완전히 멈춰있는지 아닌지에 따라서 설정.
    {
        get { return _needArrange; }
        set
        {
            _needArrange = value;

            if (value)// 움직이는 상태면 사이즈 크게 적용
                ChangeColliderSize(EColliderSize.Big);
            else //멈췄으면 콜라이더 사이즈 재설정
                TryResizeCollider();
        }
    }

    public override ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;

                switch (value)
                {
                    case ECreatureState.None:
                        RigidBody.mass = CreatureData.Mass * 5.0f;
                        break;
                    case ECreatureState.Idle:
                        RigidBody.mass = CreatureData.Mass * 500.0f;
                        break;
                    case ECreatureState.Move:
                        RigidBody.mass = CreatureData.Mass;
                        break;
                }
                if (value == ECreatureState.Move) // 뒤에 있는 애들은 밀치면서 안으로 들어오게 하려고 무게 가볍게 설정
                    RigidBody.mass = CreatureData.Mass;
                else
                    RigidBody.mass = CreatureData.Mass * 0.1f;
            }
        }
    }

    EHeroMoveState _heroMoveState = EHeroMoveState.None;
    public EHeroMoveState HeroMoveState
    {
        get { return _heroMoveState; }
        private set
        {
            _heroMoveState = value;
            switch (value)
            {
                case EHeroMoveState.CollectEnv:
                    NeedArrange = true;
                    break;
                case EHeroMoveState.TargetMonster:
                    NeedArrange = true;
                    break;
                case EHeroMoveState.ForceMove:
                    NeedArrange = true;
                    break;
            }
        }
    }

    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = ECreatureType.Hero;

		// +만 해도 된다. 위에서 Init으로 예외처리를 하지만,
		// 실수로 혹여나 같은 개체가 두번 처리될 수 있기 때문에 습관적으로 - + 추가.
		Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
		Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

		StartCoroutine(CoUpdateAI());

		return true;
	}
    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        // State
        CreatureState = ECreatureState.Idle;

        // Skill
        Skills = gameObject.GetOrAddComponent<SkillComponent>();
        Skills.SetInfo(this, CreatureData.SkillIdList);
    }

    public Transform HeroCampDest
    {
        get
        {
            HeroCamp camp = Managers.Object.Camp;
            if (HeroMoveState == EHeroMoveState.ReturnToCamp)
                return camp.Pivot;

            return camp.Destination;
        }
    }
    #region AI
    public float AttackDistance // 값이 너무 작으면 공격범위 안들어가고 충돌만 됨.
    {
        get
        {
            float targetRadius = (Target.IsValid() ? Target.ColliderRadius : 0);
            return ColliderRadius + targetRadius + 2.0f; // 보스몬스터면 다가갈 수 없을 수 있기 때문.
        }
    }

    protected override void UpdateIdle()
    {
        SetRigidBodyVelocity(Vector2.zero);// 임시코드

        // 0. 이동 상태라면 강제 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        // 0. 너무 멀어졌다면 강제로 이동

        // 1. 몬스터
        Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
        if (creature != null)
        {
            Target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.TargetMonster;
            return;
        }

        // 2. 주변 Env 채굴
        Creature env = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Creature;
        if (env != null)
        {
            Target = env;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.CollectEnv;
            return;
        }

        // 3. Camp 주변으로 모이기. 딱 한번만 호출해야한다. NeedArrange로 체크 
        if (NeedArrange)
        {
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.ReturnToCamp;
            return;
        }
    }

    protected override void UpdateMove()
    {
        // 0. 누르고 있다면, 강제 이동
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            SetRigidBodyVelocity(dir.normalized * MoveSpeed); // 방향 x 이동크기(속도)
            return;
        }

        // 1. 주변 몬스터 서치
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 몬스터 죽었으면 포기.
            if (Target.IsValid() == false) // null 체크 안하는 이유는 나중에 풀링하게 되면 활성화 기준으로 봐야하기 때문
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
            return;
        }

        // 2. 주변 Env 채굴
        if (HeroMoveState == EHeroMoveState.CollectEnv)
        {
            // 몬스터가 있으면 포기. 채굴이여도 한번 더 체크
            Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
            if (creature != null)
            {
                Target = creature;
                HeroMoveState = EHeroMoveState.TargetMonster;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Env 이미 채집했으면 포기.
            if (Target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
            return;
        }

        // 3. Camp 주변으로 모이기
        if (HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            float stopDistanceSqr = HERO_DEFAULT_STOP_RANGE * HERO_DEFAULT_STOP_RANGE;
            if (dir.sqrMagnitude <= stopDistanceSqr)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Idle;
                NeedArrange = false;
                return;
            }
            else
            {
                // 멀리 있을 수록 빨라짐
                float ratio = Mathf.Min(1, dir.magnitude); // TEMP
                float moveSpeed = MoveSpeed * (float)Math.Pow(ratio, 3); // 3제곱
                SetRigidBodyVelocity(dir.normalized * moveSpeed);
                return;
            }
        }

        //// 4. 기타 (누르다 뗐을 때)
        CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateSkill()
    {
        if (HeroMoveState == EHeroMoveState.ForceMove) // 당장 돌아오세요 용사여!!
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (Target.IsValid() == false)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateDead()
    {
    }
    
    #endregion


    private void TryResizeCollider()
    {
        // 일단 충돌체 아주 작게.
        ChangeColliderSize(EColliderSize.Small);

        foreach (var hero in Managers.Object.Heroes)
        {
            if (hero.HeroMoveState == EHeroMoveState.ReturnToCamp)
                return;
        }

        // ReturnToCamp가 한 명도 없으면 콜라이더 조정.
        foreach (var hero in Managers.Object.Heroes)
        {
            // 단 채집이나 전투중이면 스킵.
            if (hero.CreatureState == ECreatureState.Idle)
                hero.ChangeColliderSize(EColliderSize.Big);
        }
    }


    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
	{
		switch (joystickState)
		{
			case EJoystickState.PointerDown:
                HeroMoveState = EHeroMoveState.ForceMove;
				break;
			case EJoystickState.Drag:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;
			case EJoystickState.PointerUp:
                HeroMoveState = EHeroMoveState.None;
                break;
			default:
				break;
		}
	}


    /// <summary>
    /// spine과 연관된 함수
    /// </summary>
    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        base.OnAnimEventHandler(trackEntry, e);

        // TODO
        CreatureState = ECreatureState.Move;

        // Skill
        if (Target.IsValid() == false)
            return;

        Target.OnDamaged(this);
    }
}
