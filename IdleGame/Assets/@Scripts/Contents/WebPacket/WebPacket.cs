using System;
using static Define;

[Serializable]
public class LoginAccountPacketReq
{
	public string userId;// { get; set; } = String.Empty;
	public string token ;//{ get; set; } = string.Empty;
}

[Serializable]
public class LoginAccountPacketRes
{
	public EProviderType providerType; // { get; set; }
	public bool success; // { get; set; } = false;
	public long accountDbId; // { get; set; }
	public string jwt; // { get; set; } = string.Empty;
}