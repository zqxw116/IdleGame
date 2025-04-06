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
	[MenuItem("Tools/ParseExcel %#K")] // ctrl + shift + k
	public static void ParseExcelDataToJson()
	{
		ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster");
		ParseExcelDataToJson<HeroDataLoader, HeroData>("Hero");
		ParseExcelDataToJson<SkillDataLoader, SkillData>("Skill");
		ParseExcelDataToJson<EnvDataLoader, EnvData>("Env");
		//LEGACY_ParseTestData("Test"); // 과거 노가다형식으로 데이터 파싱할 때

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

			System.Reflection.FieldInfo[] fields = typeof(LoaderData).GetFields();  // LoaderData 클래스의 모든 필드 목록을 가져옴

            for (int f = 0; f < fields.Length; f++)
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
	#endregion

#endif
}