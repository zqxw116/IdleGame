using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Hero : Creature
{
	Vector2 _moveDir = Vector2.zero;

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		CreatureType = ECreatureType.Hero;
		CreatureState = ECreatureState.Idle;
		Speed = 5.0f;

		// +만 해도 된다. 위에서 Init으로 예외처리를 하지만,
		// 실수로 혹여나 같은 개체가 두번 처리될 수 있기 때문에 습관적으로 - + 추가.
		Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
		Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;
		Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
		Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

		return true;
	}

	void Update()
	{
		transform.Translate(_moveDir * Time.deltaTime * Speed);
	}

	private void HandleOnMoveDirChanged(Vector2 dir)
	{
		_moveDir = dir;
		Debug.Log(dir);
	}

	private void HandleOnJoystickStateChanged(EJoystickState joystickState)
	{
		switch (joystickState)
		{
			case Define.EJoystickState.PointerDown:
				CreatureState = Define.ECreatureState.Move;
				break;
			case Define.EJoystickState.Drag:
				break;
			case Define.EJoystickState.PointerUp:
				CreatureState = Define.ECreatureState.Idle;
				break;
			default:
				break;
		}
	}
}
