using System.Collections;
using System.Collections.Generic;
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

		Hero hero = Managers.Object.Spawn<Hero>(Vector3.zero);
		hero.CreatureState = ECreatureState.Move;

		Managers.UI.ShowBaseUI<UI_Joystick>();


        // TODO

        return true;
	}

	public override void Clear()
	{

	}
}
