using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class EffectComponent : MonoBehaviour
{
    public List<EffectBase> ActiveEffects = new List<EffectBase>(); // 가지고 있는 효과 목록
    private Creature _owner;

    public void SetInfo(Creature Owner)
    {
        _owner = Owner;
    }

    /// <summary>
    /// 도트 뎀
    /// 도트 힐
    /// 패시브 영구적 등..
    /// 어떠한 스킬이 묻었을 때 작동되는
    /// </summary>
    public List<EffectBase> GenerateEffects(IEnumerable<int> effectIds, EEffectSpawnType spawnType)
    {
        List<EffectBase> generatedEffects = new List<EffectBase>();

        foreach (int id in effectIds)
        {
            string className = Managers.Data.EffectDic[id].ClassName;
            Type effectType = Type.GetType(className);

            if (effectType == null)
            {
                Debug.LogError($"Effect Type not found: {className}");
                return null;
            }

            GameObject go = Managers.Object.SpawnGameObject(_owner.CenterPosition, "EffectBase");
            go.name = Managers.Data.EffectDic[id].ClassName;

            EffectBase effect = go.AddComponent(effectType) as EffectBase;
            effect.transform.parent = _owner.Effects.transform;
            effect.transform.localPosition = Vector2.zero;
            Managers.Object.Effects.Add(effect);

            ActiveEffects.Add(effect);
            generatedEffects.Add(effect);


            // Effect 목록을 추가
            effect.SetInfo(id, _owner, spawnType);
            effect.ApplyEffect(); // Effect 실행
        }

        return generatedEffects;
    }

    public void RemoveEffects(EffectBase effects)
    {

    }

    public void ClearDebuffsBySkill()
    {
        foreach (var buff in ActiveEffects.ToArray())
        {
            if (buff.EffectType != EEffectType.Buff)
            {
                buff.ClearEffect(EEffectClearType.ClearSkill);
            }
        }
    }
}
