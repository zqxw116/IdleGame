using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Data;
using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR
    [MenuItem("Tools/RemoveSaveData")]
    public static void RemoveSaveData()
    {
        string path = Application.persistentDataPath + "/SaveData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("SaveFile Deleted");
        }
        else
        {
            Debug.Log("No SaveFile Detected");
        }
    }


    [MenuItem("Tools/ParseExcel %#K")] // ctrl + shift + k
	public static void ParseExcelDataToJson()
	{
		ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster");
		ParseExcelDataToJson<HeroDataLoader, HeroData>("Hero");
		ParseExcelDataToJson<HeroInfoDataLoader, HeroInfoData>("HeroInfo");
		ParseExcelDataToJson<SkillDataLoader, SkillData>("Skill");
		ParseExcelDataToJson<EnvDataLoader, EnvData>("Env");
		ParseExcelDataToJson<ProjectileDataLoader, ProjectileData>("Projectile");
		ParseExcelDataToJson<EffectDataLoader, EffectData>("Effect");
		ParseExcelDataToJson<AoEDataLoader, AoEData>("AoE");
		ParseExcelDataToJson<NpcDataLoader, NpcData>("Npc");
        ParseExcelDataToJson<TextDataLoader, TextData>("Text");

        Debug.Log("DataTransformer Completed");
	}

	#region LEGACY
	//// LEGACY !
	//public static T ConvertValue<T>(string value)
	//{
	//	if (string.IsNullOrEmpty(value))
	//		return default(T);

	//	TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
	//	return (T)converter.ConvertFromString(value);
	//}

	//public static List<T> ConvertList<T>(string value)
	//{
	//	if (string.IsNullOrEmpty(value))
	//		return new List<T>();

	//	return value.Split('&').Select(x => ConvertValue<T>(x)).ToList();
	//}

	//static void LEGACY_ParseTestData(string filename)
	//{
	//	TestDataLoader loader = new TestDataLoader();

	//	string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/ExcelData/{filename}Data.csv").Split("\n");

	//	for (int y = 1; y < lines.Length; y++)
	//	{
	//		string[] row = lines[y].Replace("\r", "").Split(',');
	//		if (row.Length == 0)
	//			continue;
	//		if (string.IsNullOrEmpty(row[0]))
	//			continue;

	//		// **노가다로 데이터 convert
	//		int i = 0;
	//		TestData testData = new TestData();
	//		testData.Level = ConvertValue<int>(row[i++]);
	//		testData.Exp = ConvertValue<int>(row[i++]);
	//		testData.Skills = ConvertList<int>(row[i++]);
	//		testData.Speed = ConvertValue<float>(row[i++]);
	//		testData.Name = ConvertValue<string>(row[i++]);
	//		//**

	//		loader.tests.Add(testData);
	//	}

	//	string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented); // 직렬화. 메모리상에 있는 것을 string으로 저장
	//	File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json", jsonStr);
	//	AssetDatabase.Refresh();
	//}
	#endregion

	#region Helpers
	private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new() where LoaderData : new()
	{
		Loader loader = new Loader();
		FieldInfo field = loader.GetType().GetFields()[0]; //Loader 클래스의 첫 번째 필드(FieldInfo)를 가져옴
        field.SetValue(loader, ParseExcelDataToList<LoaderData>(filename)); // Loader의 첫번째 필드에 리스트를 저장

        string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented); // Indented=들여쓰기
        File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json", jsonStr);
		AssetDatabase.Refresh();
	}

	private static List<LoaderData> ParseExcelDataToList<LoaderData>(string filename) where LoaderData : new()
	{
		List<LoaderData> loaderDatas = new List<LoaderData>();

		string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/ExcelData/{filename}Data.csv").Split("\n");

		for (int l = 1; l < lines.Length; l++)   // 첫 번째 줄은 헤더이므로 1부터 시작
        {
			string[] row = lines[l].Replace("\r", "").Split(',');
			if (row.Length == 0)
				continue;
			if (string.IsNullOrEmpty(row[0]))
				continue;

			LoaderData loaderData = new LoaderData();
			var fields = GetFieldsInBase(typeof(LoaderData));  // LoaderData 클래스의 모든 필드 목록을 가져옴

            for (int f = 0; f < fields.Count; f++)
			{
				FieldInfo field = loaderData.GetType().GetField(fields[f].Name); // LoaderData의 필드를 하나씩 가져와서 처리
                Type type = field.FieldType;

				if (type.IsGenericType) // 리스트면
				{
					object value = ConvertList(row[f], type);
					field.SetValue(loaderData, value);
				}
				else                    // 값이면
				{
					object value = ConvertValue(row[f], field.FieldType);
					field.SetValue(loaderData, value);
				}
			}

			loaderDatas.Add(loaderData);
		}

		return loaderDatas;
	}

	private static object ConvertValue(string value, Type type)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		TypeConverter converter = TypeDescriptor.GetConverter(type);
		return converter.ConvertFromString(value);
	}

	private static object ConvertList(string value, Type type)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		// Reflection
		Type valueType = type.GetGenericArguments()[0];
		Type genericListType = typeof(List<>).MakeGenericType(valueType);
		var genericList = Activator.CreateInstance(genericListType) as IList;

		// Parse Excel
		var list = value.Split('&').Select(x => ConvertValue(x, valueType)).ToList();

		foreach (var item in list)
			genericList.Add(item);

		return genericList;
	}


	/// <summary>
	/// 추출을 해서 자
	/// </summary>
	public static List<FieldInfo> GetFieldsInBase(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
	{
		List<FieldInfo> fields = new List<FieldInfo>();
		HashSet<string> fieldNames = new HashSet<string>(); // 중복방지
		Stack<Type> stack = new Stack<Type>(); //상속 계층을 역순으로 접근하기 위한 스택


		//현재 클래스부터 System.Object까지 부모 클래스들을 stack에 
		//이유: 나중에 가장 위에 있는 부모부터 차례대로 역순으로 처리하려고
		while (type != typeof(object))
		{
			stack.Push(type);
			type = type.BaseType;
		}

		while (stack.Count > 0)
		{
			Type currentType = stack.Pop();

			foreach (var field in currentType.GetFields(bindingFlags))
			{
				if (fieldNames.Add(field.Name))
				{
					fields.Add(field);
				}
			}
		}

		return fields;
	}
	#endregion

#endif
}