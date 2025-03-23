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
	public Dictionary<int, Data.TestData> TestDic { get; private set; } = new Dictionary<int, Data.TestData>();

	/// <summary>
	/// 어드레서블로 로드 다 끝난 이후에 데이터 초기화를 해줘야함
	/// </summary>
	public void Init()
	{
		TestDic = LoadJson<Data.TestDataLoader, int, Data.TestData>("TestData").MakeDict(); 
	}

	private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
	{
		TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
		return JsonConvert.DeserializeObject<Loader>(textAsset.text); // 역직렬화. 파일에 있는걸 메모리로 변경
	}
}
