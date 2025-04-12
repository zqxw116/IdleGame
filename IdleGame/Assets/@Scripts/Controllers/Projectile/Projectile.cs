using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : BaseObject
{
    public Creature Owner { get; private set; } // 나를 쏜 주인
    public SkillBase Skill { get; private set; } // 스킬
    public Data.ProjectileData ProjectileData { get; private set; }
    public ProjectileMotionBase ProjectileMotion { get; private set; }

    private SpriteRenderer _spriteRenderer;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Projectile;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = SortingLayers.PROJECTILE;
        return true;
    }

    public void SetInfo(int dataTemplateID)
    {
        ProjectileData = Managers.Data.ProjectileDic[dataTemplateID];
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>(ProjectileData.ProjectileSpriteName);
        if (_spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"Projectile Sprite Missing {ProjectileData.ProjectileSpriteName}");
            return;
        }
    }
    public void SetSpawnInfo(Creature onwer, SkillBase skill, LayerMask layer)
    {
        Owner = onwer;
        Skill = skill;

        // Rule
        Collider.excludeLayers = layer; // 제외할 레이어들
        if (ProjectileMotion != null)
            Destroy(ProjectileMotion);

        string componentName = skill.SkillData.ComponentName;
        ProjectileMotion = gameObject.AddComponent(Type.GetType(componentName)) as ProjectileMotionBase;


        // 임시 코드
        StraightMotion straightMotion = ProjectileMotion as StraightMotion;
        if (straightMotion != null)
            straightMotion.SetInfo(ProjectileData.DataId, onwer.CenterPosition, onwer.Target.CenterPosition, () => { Managers.Object.Despawn(this); });
        
        ParabolaMotion ParabolaMotion = ProjectileMotion as ParabolaMotion;
        if (ParabolaMotion != null)
            ParabolaMotion.SetInfo(ProjectileData.DataId, onwer.CenterPosition, onwer.Target.CenterPosition, () => { Managers.Object.Despawn(this); });
        
        StartCoroutine(CoReserveDestroy(5.0f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BaseObject target = collision.GetComponent<BaseObject>();
        if (target.IsValid() == false)
            return;

        // TODO
        target.OnDamaged(Owner, Skill); // 이것 또한 하드코딩이다. 횟수 중첩 될 수 있고 다양한 방법들이 있기 때문
        Managers.Object.Despawn(this);
    }

    private IEnumerator CoReserveDestroy(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Object.Despawn(this);
    }
}
