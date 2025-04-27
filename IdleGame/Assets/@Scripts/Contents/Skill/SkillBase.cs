using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = Spine.Event;

public abstract class SkillBase : InitBase
{
	public Creature Owner { get; protected set; }
	public float RemainCoolTime { get; protected set; }
    public Data.SkillData SkillData { get; private set; }

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		return true;
	}

	public virtual void SetInfo(Creature owner, int skillTemplateID)
	{
		Owner = owner;
		SkillData = Managers.Data.SkillDic[skillTemplateID];

		// Register AnimEvent
		if (Owner.SkeletonAnim != null && Owner.SkeletonAnim.AnimationState != null)
		{
			Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
			Owner.SkeletonAnim.AnimationState.Event += OnOwnerAnimEventHandler;
		}
	}

	private void OnDisable()
	{
		if (Managers.Game == null)
			return;
		if (Owner.IsValid() == false)
			return;
		if (Owner.SkeletonAnim == null)
			return;
		if (Owner.SkeletonAnim.AnimationState == null)
			return;

		Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
	}

	public virtual void DoSkill()
	{
		RemainCoolTime = SkillData.CoolTime;

		// 준비된 스킬에서 해제
		if (Owner.Skills != null)
			Owner.Skills.SkillList.Remove(this);

		float timeScale = 1;
		if (Owner.Skills.DefaultSkill == this)
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = timeScale;
        else
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = 1;

		StartCoroutine(CoCountDownCoolDown());
    }
	private IEnumerator CoCountDownCoolDown()
	{
		RemainCoolTime = SkillData.CoolTime;
		yield return new WaitForSeconds(RemainCoolTime);
		RemainCoolTime = 0;


        // 준비된 스킬에 추가
        if (Owner.Skills != null)
            Owner.Skills.SkillList.Add(this);

    }
    public virtual void CancelSkill()
    {
    }

    protected virtual void GenerateProjectile(Creature owner, Vector3 spawnPos)
    {
        Projectile projectile = Managers.Object.Spawn<Projectile>(spawnPos, SkillData.ProjectileId);

        LayerMask excludeMask = 0; // 제외할 애들  비트플레그
        excludeMask.AddLayer(Define.ELayer.Default);
        excludeMask.AddLayer(Define.ELayer.Projectile);
        excludeMask.AddLayer(Define.ELayer.Env);
        excludeMask.AddLayer(Define.ELayer.Obstacle);

        switch (owner.CreatureType)
        {
            case Define.ECreatureType.Hero:
                excludeMask.AddLayer(Define.ELayer.Hero);
                break;
            case Define.ECreatureType.Monster:
                excludeMask.AddLayer(Define.ELayer.Monster);
                break;
        }

        projectile.SetSpawnInfo(Owner, this, excludeMask);
    }

	protected void OnOwnerAnimEventHandler(TrackEntry trackEntry, Event e)
	{
		if (trackEntry.Animation.Name == SkillData.AnimName) // 이벤트가 겹칠 수 있기 때문
            OnAttackEvent();
    }

	protected abstract void OnAttackEvent();
}
