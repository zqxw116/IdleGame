using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightMotion : ProjectileMotionBase
{

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		return true;
	}


	public new void SetInfo(int projectileTemplateID, Vector3 spawnPosition, Vector3 targetPosition, Action endCallback = null)
	{
		base.SetInfo(projectileTemplateID, spawnPosition, targetPosition, endCallback);
	}
	protected override IEnumerator CoLaunchProjectile()
    {
		float journeyLengh = Vector3.Distance(StartPosition, TargetPosition);   // 얼마나 멀리 가야 하는가
		float totalTime = journeyLengh / _speed;
		float elapsedTime = 0; // 얼마나 시간이 흘렀는가

        while (elapsedTime < totalTime)
        {
			elapsedTime += Time.deltaTime;

			float nomalizedTime = elapsedTime / totalTime;
			transform.position = Vector3.Lerp(StartPosition, TargetPosition, nomalizedTime);

			if (LookAtTarget)
				LookAt2D(TargetPosition - transform.position);


			yield return null;
		}
		transform.position = TargetPosition;
		EndCallback?.Invoke();

	}
}
