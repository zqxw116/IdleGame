using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_TitleScene : UI_Scene
{
    enum GameObjects
    {
        Image_StartTouch,
    }

    enum Texts
    {
        Text_Display
    }

	public Text text_Display;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

		// bind 우선 적용
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

		GetObject((int)GameObjects.Image_StartTouch).BindEvent((evt) =>
		{
			Debug.Log("ChangeScene");
			Managers.Scene.LoadScene(EScene.GameScene);
		});

		GetObject((int)GameObjects.Image_StartTouch).gameObject.SetActive(false);
		GetText((int)Texts.Text_Display).text = $"";

		StartLoadAssets();

		return true;
    }

	void StartLoadAssets()
	{
		Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
		{
			Debug.Log($"{key} {count}/{totalCount}");

			if (count == totalCount)
			{
				Managers.Data.Init();

				GetObject((int)GameObjects.Image_StartTouch).gameObject.SetActive(true);
				GetText((int)Texts.Text_Display).text = "Touch To Start~!";

                //Managers.Data.TestDic;

            }
        });
	}
}
