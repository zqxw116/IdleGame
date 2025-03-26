using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BaseObject : InitBase
{
	public EObjectType ObjectType { get; protected set; } = EObjectType.None;
	public CircleCollider2D Collider { get; private set; }
	public SkeletonAnimation SkeletonAnim { get; private set; }
	public Rigidbody2D RigidBody { get; private set; }

	// 아래와 같은 내용public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
	public float ColliderRadius { get { return Collider?.radius ?? 0.0f; } }
	public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }

	// 온라인 게임에서는 왼쪽 오른쪽을 알아야 상태설정을 할 수 있기 때문이다.
	bool _lookLeft = true;
	public bool LookLeft
	{
		get { return _lookLeft; }
		set
		{
			_lookLeft = value;
			Flip(!value);
		}
	}

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
		SkeletonAnim = GetComponent<SkeletonAnimation>();
		RigidBody = GetComponent<Rigidbody2D>();

		return true;
	}

	public void TranslateEx(Vector3 dir)
	{
		transform.Translate(dir);

		if (dir.x < 0)
			LookLeft = true;
		else if (dir.x > 0)
			LookLeft = false;
	}

	#region Spine
	protected virtual void UpdateAnimation()
	{
	}

	public void PlayAnimation(int trackIndex, string AnimName, bool loop)
	{
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.AnimationState.SetAnimation(trackIndex, AnimName, loop);
	}

	public void AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
	{
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.AnimationState.AddAnimation(trackIndex, AnimName, loop, delay);
	}

	// true면 오른쪽을 보겠다
	public void Flip(bool flag)
	{
		if (SkeletonAnim == null)
			return;

		SkeletonAnim.Skeleton.ScaleX = flag ? -1 : 1;
	}
	#endregion
}
