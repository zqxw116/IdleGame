using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

/// <summary>
/// 타일정보 데이터로 가져오는 에디터 툴 
///  ctrl + shift + m
/// </summary>
public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR
	// % (Ctrl), # (Shift), & (Alt) 
	[MenuItem("Tools/GenerateMap %#m")]
	private static void GenerateMap()
	{
		GameObject[] gameObjects = Selection.gameObjects;

		foreach (GameObject go in gameObjects)
		{
			Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

			using (var writer = File.CreateText($"Assets/@Resources/Data/MapData/{go.name}Collision.txt"))
			{
				writer.WriteLine(tm.cellBounds.xMin);
				writer.WriteLine(tm.cellBounds.xMax);
				writer.WriteLine(tm.cellBounds.yMin);
				writer.WriteLine(tm.cellBounds.yMax);

				for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
				{
					for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
					{
						TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
						if (tile != null)
						{
							if (tile.name.Contains("O"))
								writer.Write(Define.MAP_TOOL_NONE);
							else
								writer.Write(Define.MAP_TOOL_SEMI_WALL);
						}
						else
							writer.Write(Define.MAP_TOOL_WALL);
					}
					writer.WriteLine();
				}
			}
		}

		Debug.Log("Map Collision Generation Complete");
	}

    /// <summary>
    /// 데이터로 CustomTile용 파일 만들어주는 함수
    ///  ctrl + shift + o
    /// </summary>
    [MenuItem("Tools/Create Object Tile Asset %#o")]
    public static void CreateObjectTile()
    {
        // Monster
        Dictionary<int, Data.MonsterData> MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        foreach (var data in MonsterDic.Values)
        {
            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>();
            customTile.Name = data.DescriptionTextID;
            customTile.DataTemplateID = data.DataId;
            customTile.ObjectType = Define.EObjectType.Creature;
            customTile.CreatureType = Define.ECreatureType.Monster;

            string name = $"{data.DataId}_{data.DescriptionTextID}";
            string path = "Assets/@Resources/TileMaps/Tiles/Dev/Monster";
            path = Path.Combine(path, $"{name}.Asset");

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }

        // Env
        Dictionary<int, Data.EnvData> Env = LoadJson<Data.EnvDataLoader, int, Data.EnvData>("EnvData").MakeDict();
        foreach (var data in Env.Values)
        {

            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>();
            customTile.Name = data.DescriptionTextID;
            customTile.DataTemplateID = data.DataId;
            customTile.ObjectType = Define.EObjectType.Env;
            customTile.CreatureType = Define.ECreatureType.None;

            string name = $"{data.DataId}_{data.DescriptionTextID}";
            string path = "Assets/@Resources/TileMaps/Tiles/Dev/Env";
            path = Path.Combine(path, $"{name}.Asset");

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }
    }

    private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
	{
		TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/@Resources/Data/JsonData/{path}.json");
		return JsonConvert.DeserializeObject<Loader>(textAsset.text);
	}
#endif
}
