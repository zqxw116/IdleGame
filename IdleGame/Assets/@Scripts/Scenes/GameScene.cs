using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = EScene.GameScene;

		var map = Managers.Resource.Instantiate("BaseMap");
		map.name = "@BaseMap";
		map.transform.position = Vector3.zero;

        HeroCamp camp = Managers.Object.Spawn<HeroCamp>(new Vector3Int(-10, -5, 0), 0);


		for (int i = 0; i < 5; i++)
		{
            Hero hero = Managers.Object.Spawn<Hero>(new Vector3Int(-10 + Random.Range(-5, 5), -5 + Random.Range(-5, 5), 0), HERO_KNIGHT_ID);
            hero.CreatureState = ECreatureState.Move;
        }
		CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
		camera.Target = camp;

		Managers.UI.ShowBaseUI<UI_Joystick>();

		{ 
			Managers.Object.Spawn<Monster>(new Vector3Int(0, 1, 0), MONSTER_BEAR_ID);
			Managers.Object.Spawn<Monster>(new Vector3Int(0, 1, 0), MONSTER_SLIME_ID);
		}
		
		{ 
			Env env = Managers.Object.Spawn<Env>(new Vector3Int(0, 1, 0), ENV_TREE1_ID);
			env.EnvState = EEnvState.Idle;
		}

        // TODO

        return true;
	}

	public override void Clear()
	{

	}
}
