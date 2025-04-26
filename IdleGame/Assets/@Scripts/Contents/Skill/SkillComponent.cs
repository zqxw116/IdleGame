using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillComponent : InitBase
{
	public List<SkillBase> SkillList { get; } = new List<SkillBase>();
    public List<SkillBase> ActiveSkills { get; set; } = new List<SkillBase>();  // 준비된 스킬 목록

    // list를 index로 관리하던, 이렇게 개별로 관리하던 스타일 차이다.
    public SkillBase DefaultSkill { get; private set; }	// 평타
    public SkillBase EnvSkill { get; private set; }		// 채집
    public SkillBase ASkill { get; private set; }		// 스킬 A
    public SkillBase BSkill { get; private set; }       // 스킬 B


    public SkillBase CurrentSkill
    {
        get
        {
            if (ActiveSkills.Count == 0)
                return DefaultSkill;

            int randomIndex = UnityEngine.Random.Range(0, ActiveSkills.Count);
            return ActiveSkills[randomIndex];
        }
    }


    Creature _owner;

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		return true;
	}

	public void SetInfo(Creature owner, CreatureData creatureData)
	{
		_owner = owner;

		AddSkill(creatureData.DefaultSkillId, ESkillSlot.Default);
		AddSkill(creatureData.EnvSkillId, ESkillSlot.Env);
		AddSkill(creatureData.SkillAId, ESkillSlot.A);
		AddSkill(creatureData.SkillBId, ESkillSlot.B);
	}

	public void AddSkill(int skillTemplateID, ESkillSlot skillSlot)
	{
		if (skillTemplateID == 0) return;

        if (Managers.Data.SkillDic.TryGetValue(skillTemplateID, out var data) == false)
        {
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

		SkillBase skill = gameObject.AddComponent(Type.GetType(data.ClassName)) as SkillBase;
		if (skill == null)
			return;


        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);

        switch (skillSlot)
        {
            case Define.ESkillSlot.Default:
                DefaultSkill = skill;
                break;
            case Define.ESkillSlot.Env:
                EnvSkill = skill;
                break;
            case Define.ESkillSlot.A:
                ASkill = skill;
                ActiveSkills.Add(skill);
                break;
            case Define.ESkillSlot.B:
                BSkill = skill;
                ActiveSkills.Add(skill);
                break;
        }
    }
}
