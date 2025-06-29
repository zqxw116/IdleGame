using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class QuestManager
{
	public Dictionary<int, Quest> AllQuests = new Dictionary<int, Quest>();

	public List<Quest> WaitingQuests { get; } = new List<Quest>();	 // 대기 
	public List<Quest> ProcessingQuests { get; } = new List<Quest>();// 진행
	public List<Quest> CompletedQuests { get; } = new List<Quest>(); // 완료
	public List<Quest> RewardedQuests { get; } = new List<Quest>();	 // 보상

	public QuestManager()
	{
		Managers.Game.OnBroadcastEvent -= OnHandleBroadcastEvent;
		Managers.Game.OnBroadcastEvent += OnHandleBroadcastEvent;
	}

	/// <summary>
	/// 데이터가 변경됐을 수 있으니 Save Data 목록 추가 확인
	/// </summary>
	public void AddUnknownQuests()
	{
		foreach (QuestData questData in Managers.Data.QuestDic.Values.ToList())
		{
			if (AllQuests.ContainsKey(questData.TemplateId))
				continue;

			QuestSaveData questSaveData = new QuestSaveData()
			{
				TemplateId = questData.TemplateId,
				State = Define.EQuestState.None,
				NextResetTime = DateTime.MaxValue,
			};

			for (int i = 0; i < questData.QuestTasks.Count; i++)
				questSaveData.ProgressCount.Add(0);

			AddQuest(questSaveData);
		}
	}

	// 1초마다, 레벨이 올랐거나 등등 주기적으로 퀘스트 확인
	public void CheckWaitingQuests()
	{
		// TODO
	}

	public void CheckProcessingQuests()
	{
		// TODO
	}

	public Quest AddQuest(QuestSaveData questInfo)
	{
		Quest quest = Quest.MakeQuest(questInfo);
		if (quest == null)
			return null;

		switch (quest.State)
		{
			case Define.EQuestState.None:
				WaitingQuests.Add(quest);
				break;
			case Define.EQuestState.Processing:
				ProcessingQuests.Add(quest);
				break;
			case Define.EQuestState.Completed:
				CompletedQuests.Add(quest);
				break;
			case Define.EQuestState.Rewarded:
				RewardedQuests.Add(quest);
				break;
		}

		AllQuests.Add(quest.TemplateId, quest);

		return quest;
	}

	public void Clear()
	{
		AllQuests.Clear();

		WaitingQuests.Clear();
		ProcessingQuests.Clear();
		CompletedQuests.Clear();
		RewardedQuests.Clear();
	}

	void OnHandleBroadcastEvent(EBroadcastEventType eventType, int value)
	{
		foreach (Quest quest in ProcessingQuests)
		{
			quest.OnHandleBroadcastEvent(eventType, value);
		}
	}
}
