using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
	#region Hero
	private Vector2 _moveDir;
	public Vector2 MoveDir
	{
		get { return _moveDir; }
		set
		{
			_moveDir = value;
			OnMoveDirChanged?.Invoke(value);
		}
	}

	private Define.EJoystickState _joystickState;
	public Define.EJoystickState JoystickState
	{
		get { return _joystickState; }
		set
		{
			_joystickState = value;
			OnJoystickStateChanged?.Invoke(_joystickState); 
		}
	}
	#endregion

	#region Action
	public event Action<Vector2> OnMoveDirChanged;						// 리스너 형태. 전체 브로드케스트를함
    public event Action<Define.EJoystickState> OnJoystickStateChanged;	// 리스너 형태. 전체 브로드케스트를함
    #endregion
}
