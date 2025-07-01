using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

[Serializable]
public class GameSaveData
{
    public int Wood = 0;
    public int Mineral = 0;
    public int Meat = 0;
    public int Gold = 0;

    public int ItemDbIdGenerator = 1; // 1씩 증가해서 사용
    public List<ItemSaveData> Items = new List<ItemSaveData>();
    public List<QuestSaveData> AllQuests = new List<QuestSaveData>(); 
    public List<HeroSaveData> Heroes = new List<HeroSaveData>();
}

[Serializable]
public class HeroSaveData
{
    public int DataId = 0;
    public int Level = 1;
    public int Exp = 0;
    public EHeroOwningState OwningState = EHeroOwningState.Unowned;
}

[Serializable]
public class ItemSaveData
{
    public int InstanceId;  // 인겜에서 사용하는 고유 ID
    public int DbId;        // MSSQL MYSQL ...
    public int TemplateId;
    public int Count;
    public int EquipSlot; // 장착 아이템 + 인벤 아이템 + 창고 아이템(index 느낌)
    // public int OwnerId;
    public int EnchantCount;
}
[Serializable]
public class QuestSaveData
{
    public int TemplateId;
    public EQuestState State = EQuestState.None;
    public List<int> ProgressCount = new List<int>();
    public DateTime NextResetTime; // 클라면 조작 가능, 서버는 절대 못함
}
public class GameManager
{
    #region GameData
    GameSaveData _saveData = new GameSaveData();
    public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }

    public int Wood
    {
        get { return _saveData.Wood; }
        private set
        {
            int diff = _saveData.Wood - value;
            _saveData.Wood = value;
            BroadcastEvent(EBroadcastEventType.ChangeWood, diff);
        }
    }

    public int Mineral
    {
        get { return _saveData.Mineral; }
        private set
        {
            int diff = _saveData.Mineral - value;
            _saveData.Mineral = value;
            BroadcastEvent(EBroadcastEventType.ChangeMineral, diff);
        }
    }

    public int Meat
    {
        get { return _saveData.Meat; }
        private set
        {
            int diff = _saveData.Meat - value;
            _saveData.Meat = value;
            BroadcastEvent(EBroadcastEventType.ChangeMeat, diff);
        }
    }

    public int Gold
    {
        get { return _saveData.Gold; }
        private set
        {
            int diff = _saveData.Gold - value;
            _saveData.Gold = value;
            BroadcastEvent(EBroadcastEventType.ChangeGold, diff);
        }
    }


    public bool CheckResource(EResourceType eResourceType, int amount)
    {
        switch (eResourceType)
        {
            case EResourceType.Wood:
                return Wood >= amount;
            case EResourceType.Mineral:
                return Mineral >= amount;
            case EResourceType.Meat:
                return Meat >= amount;
            case EResourceType.Gold:
                return Gold >= amount;
            case EResourceType.Dia:
                return true;
            case EResourceType.Materials:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 소모
    /// </summary>
    public bool SpendResource(EResourceType eResourceType, int amount)
    {
        if (CheckResource(eResourceType, amount) == false)
            return false;

        switch (eResourceType)
        {
            case EResourceType.Wood:
                Wood -= amount;
                break;
            case EResourceType.Mineral:
                Mineral -= amount;
                break;
            case EResourceType.Meat:
                Meat -= amount;
                break;
            case EResourceType.Gold:
                Gold -= amount;
                break;
            case EResourceType.Dia:
                break;
            case EResourceType.Materials:
                break;
        }

        return true;
    }

    /// <summary>
    /// 득
    /// </summary>
    public void EarnResource(EResourceType eResourceType, int amount)
    {
        switch (eResourceType)
        {
            case EResourceType.Wood:
                Wood += amount;
                break;
            case EResourceType.Mineral:
                Mineral += amount;
                break;
            case EResourceType.Meat:
                Meat += amount;
                break;
            case EResourceType.Gold:
                Gold += amount;
                break;
            case EResourceType.Dia:
                break;
            case EResourceType.Materials:
                break;
        }
    }


    /// <summary>
    /// 재화 등이 변경될 때 변경될 함수를 호출하게되면 복잡해지고 코드가 늘어난다.
    /// 필요한 함수를 구독해둬서 broadcast로 전달한다.
    /// </summary>
    public void BroadcastEvent(EBroadcastEventType eventType, int value)
    {
        OnBroadcastEvent?.Invoke(eventType, value);
    }


    public List<HeroSaveData> AllHeroes { get { return _saveData.Heroes; } }
    public int TotalHeroCount { get { return _saveData.Heroes.Count; } }
    public int UnownedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == EHeroOwningState.Unowned).Count(); } }
    public int OwnedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == EHeroOwningState.Owned).Count(); } }
    public int PickedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == EHeroOwningState.Picked).Count(); } }
    
    public int GenerateItemDbId() 
    {
        int itemDbId = _saveData.ItemDbIdGenerator;
        _saveData.ItemDbIdGenerator++;
        return itemDbId;
    }

    #endregion

    #region Hero
    private Vector2 _moveDir;
	public Vector2 MoveDir
	{
		get { return _moveDir; }
		set
		{
			_moveDir = value;
			OnMoveDirChanged?.Invoke(value);
		}
	}

	private Define.EJoystickState _joystickState;
	public Define.EJoystickState JoystickState
	{
		get { return _joystickState; }
		set
		{
			_joystickState = value;
			OnJoystickStateChanged?.Invoke(_joystickState); 
		}
	}
    #endregion

    #region Teleport
    public void TeleportHeroes(Vector3 position)
    {
        TeleportHeroes(Managers.Map.World2Cell(position));
    }

    public void TeleportHeroes(Vector3Int cellPos)
    {
        foreach (var hero in Managers.Object.Heroes)
        {
            Vector3Int randCellPos = Managers.Game.GetNearbyPosition(hero, cellPos);
            Managers.Map.MoveTo(hero, randCellPos, forceMove: true);
        }

        Vector3 worldPos = Managers.Map.Cell2World(cellPos);
        Managers.Object.Camp.ForceMove(worldPos);
        Camera.main.transform.position = worldPos;
    }
    #endregion

    #region Helper
    public Vector3Int GetNearbyPosition(BaseObject hero, Vector3Int pivot, int range = 5)
    {
        int x = Random.Range(-range, range);
        int y = Random.Range(-range, range);

        for (int i = 0; i < 100; i++)
        {
            Vector3Int randCellPos = pivot + new Vector3Int(x, y, 0);
            if (Managers.Map.CanGo(hero, randCellPos))
                return randCellPos;
        }

        Debug.LogError($"GetNearbyPosition Failed");

        return Vector3Int.zero;
    }
    #endregion

    #region Save & Load	
    public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }

    /// <summary>
    /// 완전 처음 만들 때
    /// </summary>
    public void InitGame()
    {
        if (File.Exists(Path)) // 원래 파일 있으면 return
            return;

        // Hero
        var heroes = Managers.Data.HeroDic.Values.ToList();
        foreach (HeroData hero in heroes)
        {
            HeroSaveData saveData = new HeroSaveData()
            {
                DataId = hero.DataId,
            };

            SaveData.Heroes.Add(saveData);
        }

        // Item
        {

        }

        // Quest
        {
            var quests = Managers.Data.QuestDic.Values.ToList();

            foreach (QuestData questData in quests)
            {
                QuestSaveData saveData = new QuestSaveData()
                {
                    TemplateId = questData.DataId,
                    State = EQuestState.None,
                    ProgressCount = new List<int>(),
                    NextResetTime = DateTime.Now,
                };

                for (int i = 0; i < questData.QuestTasks.Count; i++)
                {
                    saveData.ProgressCount.Add(0);
                }

                Debug.Log("SaveDataQuest");
                Managers.Quest.AddQuest(saveData);
            }
        }
        // TEMP
        SaveData.Heroes[0].OwningState = EHeroOwningState.Picked;
        SaveData.Heroes[1].OwningState = EHeroOwningState.Owned;

        Wood = 100;
        Gold = 100;
        Mineral = 100;
        Meat = 100; 
        Managers.Hero.AddUnknownHeroes();
    }

    public void SaveGame()
    {
        // Hero
        {
            SaveData.Heroes.Clear();
            foreach (var heroInfo in Managers.Hero.AllHeroInfos.Values)
            {
                SaveData.Heroes.Add(heroInfo.SaveData);
            }
        }

        // Item
        {

            SaveData.Items.Clear();
            foreach (var item in Managers.Inventory.AllItems)
            {
                SaveData.Items.Add(item.SaveData);
            }
        }

        // Quest
        {
            SaveData.AllQuests.Clear();
            foreach (Quest quest in Managers.Quest.AllQuests.Values)
            {
                SaveData.AllQuests.Add(quest.SaveData);
            }
        }


        string jsonStr = JsonUtility.ToJson(Managers.Game.SaveData);
        File.WriteAllText(Path, jsonStr);
        Debug.Log($"Save Game Completed : {Path}");
    }

    public bool LoadGame()
    {
        if (File.Exists(Path) == false)
            return false;

        string fileStr = File.ReadAllText(Path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(fileStr); // 역직렬화로 가져옴

        if (data != null)
            Managers.Game.SaveData = data;

        // Hero
        {
            Managers.Hero.AllHeroInfos.Clear();
            foreach (var saveData in data.Heroes)
            {
                Managers.Hero.AddHeroInfo(saveData);
            }
            Managers.Hero.AddUnknownHeroes();
        }

        // Item
        // 처음에만 전체 다 넣어줌. 나머지는 변경사항만 서버에서 받음
        {
            Managers.Inventory.Clear();
            foreach (ItemSaveData itemSaveData in data.Items)
            {
                Managers.Inventory.AddItem(itemSaveData);
            }

        }
        // Quest
        {
            Managers.Quest.Clear();

            foreach (QuestSaveData questSaveData in data.AllQuests)
            {
                Managers.Quest.AddQuest(questSaveData);
            }

            Managers.Quest.AddUnknownQuests();
        }


        Debug.Log($"Save Game Loaded : {Path}");
        return true;
    }
    #endregion


    #region Action
    public event Action<Vector2> OnMoveDirChanged;						// 리스너 형태. 전체 브로드케스트를함
    public event Action<Define.EJoystickState> OnJoystickStateChanged;	// 리스너 형태. 전체 브로드케스트를함

    public event Action<EBroadcastEventType, int> OnBroadcastEvent;	// 변경될 사항으로 전달
    #endregion
}
