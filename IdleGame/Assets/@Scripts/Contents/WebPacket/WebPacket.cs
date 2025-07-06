using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 서버와 같은 형식
namespace WebPacket
{
	[Serializable]
	public class TestPacketReq
	{
		public string userId;
		public string token;
	}

	[Serializable]
	public class TestPacketRes
	{
		public bool success;
	}
}