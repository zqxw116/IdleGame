using Data;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    // 역직렬화를 복사한 데이터
    public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; } = new Dictionary<int, Data.MonsterData>();
    public Dictionary<int, Data.HeroData> HeroDic { get; private set; } = new Dictionary<int, Data.HeroData>();
    public Dictionary<int, Data.HeroInfoData> HeroInfoDic { get; private set; } = new Dictionary<int, Data.HeroInfoData>();
    public Dictionary<int, Data.SkillData> SkillDic { get; private set; } = new Dictionary<int, Data.SkillData>();
    public Dictionary<int, Data.ProjectileData> ProjectileDic { get; private set; } = new Dictionary<int, Data.ProjectileData>();
    public Dictionary<int, Data.EnvData> EnvDic { get; private set; } = new Dictionary<int, Data.EnvData>();
    public Dictionary<int, Data.EffectData> EffectDic { get; private set; } = new Dictionary<int, Data.EffectData>();
    public Dictionary<int, Data.AoEData> AoEDic { get; private set; } = new Dictionary<int, Data.AoEData>();
    public Dictionary<int, Data.NpcData> NpcDic { get; private set; } = new Dictionary<int, Data.NpcData>();
    public Dictionary<string, Data.TextData> TextDic { get; private set; } = new Dictionary<string, Data.TextData>();


    public Dictionary<int, Data.EquipmentData> EquipmentDic { get; private set; } = new Dictionary<int, Data.EquipmentData>();
    public Dictionary<int, Data.ConsumableData> ConsumableDic { get; private set; } = new Dictionary<int, Data.ConsumableData>();
    public Dictionary<int, Data.ItemData> ItemDic { get; private set; } = new Dictionary<int, Data.ItemData>(); // item data를 모아서 한번에 관리한다.
    public Dictionary<int, Data.DropTableData> DropTableDic { get; private set; } = new Dictionary<int, Data.DropTableData>();
    public Dictionary<int, Data.QuestData> QuestDic { get; private set; } = new Dictionary<int, Data.QuestData>();


    public void Init()
    {
        // ItemDataLoader처럼 제네릭으로 나머지 데이터들을 설정하려 했지만
        // 추후 후반부에 수백개의 데이터들이 추가되는데, 이때 valied로 유효한지 체크할 예정이기 때문이다.
        MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        HeroDic = LoadJson<Data.HeroDataLoader, int, Data.HeroData>("HeroData").MakeDict();
        HeroInfoDic = LoadJson<Data.HeroInfoDataLoader, int, Data.HeroInfoData> ("HeroInfoData").MakeDict();
        SkillDic = LoadJson<Data.SkillDataLoader, int, Data.SkillData>("SkillData").MakeDict();
        ProjectileDic = LoadJson<Data.ProjectileDataLoader, int, Data.ProjectileData>("ProjectileData").MakeDict();
        EnvDic = LoadJson<Data.EnvDataLoader, int, Data.EnvData>("EnvData").MakeDict();
        EffectDic = LoadJson<Data.EffectDataLoader, int, Data.EffectData>("EffectData").MakeDict();
        AoEDic = LoadJson<Data.AoEDataLoader, int, Data.AoEData>("AoEData").MakeDict();
        var npcLoader = LoadJson<Data.NpcDataLoader, int, Data.NpcData>("NpcData");
        NpcDic = npcLoader.MakeDict();
        TextDic = LoadJson<Data.TextDataLoader, string, Data.TextData>("TextData").MakeDict();

        EquipmentDic = LoadJson<Data.ItemDataLoader<Data.EquipmentData>, int, Data.EquipmentData>("Item_EquipmentData").MakeDict();
        ConsumableDic = LoadJson<Data.ItemDataLoader<Data.ConsumableData>, int, Data.ConsumableData>("Item_ConsumableData").MakeDict();
        DropTableDic = LoadJson<Data.DropTableDataLoader, int, Data.DropTableData>("DropTableData").MakeDict();
        QuestDic = LoadJson<Data.QuestDataLoader, int, Data.QuestData>("QuestData").MakeDict();


        ItemDic.Clear();

        foreach (var item in EquipmentDic)
            ItemDic.Add(item.Key, item.Value);
        foreach (var item in ConsumableDic)
            ItemDic.Add(item.Key, item.Value);

        // Validation 나중에 유효한지 확인 체크하면 좋다.
        // npcLoader.
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        // JsonConvert.DeserializeObject<T>()는 JSON의 키와 클래스의 필드/프로퍼티 이름을 기반으로 자동 매핑합니다.
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);   // 역직렬화
    }
}
