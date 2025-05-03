using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager
{
	public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
	public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
	public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();
	public HashSet<Env> Envs { get; } = new HashSet<Env>();
	public HashSet<EffectBase> Effects { get; } = new HashSet<EffectBase>();
	public HeroCamp Camp{ get; private set; }

	#region Roots
	public Transform GetRootTransform(string name)
	{
		GameObject root = GameObject.Find(name);
		if (root == null)
			root = new GameObject { name = name };

		return root.transform;
	}

	public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
	public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
	public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
	public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
	public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
    #endregion

    public void ShowDamageFont(Vector2 position, float damage, Transform parent, bool isCritical = false)
    {
        GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
        DamageFont damageText = go.GetComponent<DamageFont>();
        damageText.SetInfo(position, damage, parent, isCritical);
    }

	public GameObject SpawnGameObject(Vector3 position, string prefName)
	{
		GameObject go = Managers.Resource.Instantiate(prefName, pooling: true);
        go.transform.position = position;
        return go;
	}

    /// <summary>
    /// Cell 영역 기준으로 생성
    /// </summary>
    public T Spawn<T>(Vector3Int cellPos, int templateID) where T : BaseObject
	{
		Vector3 spawnPos = Managers.Map.Cell2World(cellPos);
		return Spawn<T>(cellPos, templateID);
    }

    public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
	{
		string prefabName = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate(prefabName);
		go.name = prefabName;
		go.transform.position = position;

		BaseObject obj = go.GetComponent<BaseObject>();

		if (obj.ObjectType == EObjectType.Creature)
		{
            Creature creature = go.GetComponent<Creature>();
			switch (creature.CreatureType)
			{
				case ECreatureType.Hero:
					obj.transform.parent = HeroRoot; // 루트 설정
					Hero hero = creature as Hero;
					Heroes.Add(hero);
					break;
				case ECreatureType.Monster:
					obj.transform.parent = MonsterRoot; // 루트 설정
					Monster monster = creature as Monster;
					Monsters.Add(monster);
					break;
			}

			creature.SetInfo(templateID);
		}
		else if (obj.ObjectType == EObjectType.Projectile)
		{
			obj.transform.parent = ProjectileRoot;

			Projectile projectile = go.GetComponent<Projectile>();
			Projectiles.Add(projectile);

			projectile.SetInfo(templateID);
		}
		else if (obj.ObjectType == EObjectType.Env)
        {
            obj.transform.parent = EnvRoot;

            Env env = go.GetComponent<Env>();
            Envs.Add(env);

            env.SetInfo(templateID);
        }
        else if (obj.ObjectType == EObjectType.HeroCamp)
        {
			Camp = go.GetComponent<HeroCamp>();
        }

        return obj as T; // obj만 반환하면 에러가 생김. 제네릭이 아니기 때문
    }

	public void Despawn<T>(T obj) where T : BaseObject
	{
		EObjectType objectType = obj.ObjectType;

		if (obj.ObjectType == EObjectType.Creature)
		{
			Creature creature = obj.GetComponent<Creature>();
			switch (creature.CreatureType)
			{
				case ECreatureType.Hero:
					Hero hero = creature as Hero;
					Heroes.Remove(hero);
					break;
				case ECreatureType.Monster:
					Monster monster = creature as Monster;
					Monsters.Remove(monster);
					break;
			}
		}
		else if (obj.ObjectType == EObjectType.Projectile)
		{
			Projectile projectile = obj.GetComponent<Projectile>();
			Projectiles.Remove(projectile);
		}
		else if (obj.ObjectType == EObjectType.Env)
		{
            Env env = obj.GetComponent<Env>();
			Envs.Remove(env);
        }
        else if (obj.ObjectType == EObjectType.HeroCamp)
        {
            Camp = null;
        }
        Managers.Resource.Destroy(obj.gameObject);
	}

    #region Skill 판정
    public List<Creature> FindConeRangeTargets(Creature owner, Vector3 dir, float range, int angleRange, bool isAllies = false)
    {
        List<Creature> targets = new List<Creature>();
        List<Creature> ret = new List<Creature>();

		// hero, monster 인지 확인
        ECreatureType targetType = Util.DetermineTargetType(owner.CreatureType, isAllies);

        if (targetType == ECreatureType.Monster)
        {
            var objs = Managers.Map.GatherObjects<Monster>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }
        else if (targetType == ECreatureType.Hero)
        {
            var objs = Managers.Map.GatherObjects<Hero>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }

        // 가져온 목록으로 전체 확인.
        foreach (var target in targets) 
        {
            // 1. 거리안에 있는지 확인
            var targetPos = target.transform.position;
            float distance = Vector3.Distance(targetPos, owner.transform.position);

            if (distance > range)
                continue;

            // 2. 각도 확인
            if (angleRange != 360)
            {
                BaseObject ownerTarget = (owner as Creature).Target;

                // 2. 부채꼴 모양 각도 계산
                // owner에서 targetPos로 향하는 벡터와
                // owner가 바라보는 dir(앞방향) 사이의 각도 계산
                float dot = Vector3.Dot((targetPos - owner.transform.position).normalized, dir.normalized);
                float degree = Mathf.Rad2Deg * Mathf.Acos(dot); // 코사인 값 -> 라디안 -> 도(°) 변환

                if (degree > angleRange / 2f)
                    continue;
            }

            ret.Add(target);
        }

        return ret;
    }

    #endregion
}