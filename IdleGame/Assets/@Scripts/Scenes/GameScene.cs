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

		Hero hero = Managers.Object.Spawn<Hero>(new Vector3Int(-10, -5, 0));
		hero.CreatureState = ECreatureState.Move;

		CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
		camera.Target = hero;


		Managers.UI.ShowBaseUI<UI_Joystick>();


        Monster monster = Managers.Object.Spawn<Monster>(new Vector3Int(0, 1, 0));
        monster.CreatureState = ECreatureState.Idle;

        // TODO

        return true;
	}

	public override void Clear()
	{

	}
}
