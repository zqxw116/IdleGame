
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_LoginScene : UI_Scene
{
	enum Buttons
	{
        GoogleButton,
		GuestButton,
	}

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		BindButtons(typeof(Buttons));

		GetButton((int)Buttons.GoogleButton).gameObject.BindEvent(OnClickGoogleButton);

		Managers.Auth.InitGoogle();

        return true;
	}

    #region OnClick
    public void OnClickGuestButton(PointerEventData evt)
	{
		Managers.Auth.TryGuestLogin((result) => OnLoginSucess(result, EProviderType.Guest));
	}

	//public void OnClickFacebookButton(PointerEventData evt)
	//{
	//	Managers.Auth.TryFacebookLogin((result) => OnLoginSucess(result, EProviderType.Facebook));
	//}

	public void OnClickGoogleButton(PointerEventData evt)
	{
		Managers.Auth.TryGoogleLogin((result) => OnLoginSucess(result, EProviderType.Google));
	}
    #endregion


    public void OnLoginSucess(AuthResult authResult, EProviderType providerType)
	{
		Debug.Log($"OnLoginSucess type : {providerType}");
		// 로그인 실패 체크
		if (!authResult.isSuccess)
		{
			Debug.LogError($"Login failed for provider: {providerType}");
			// TODO: 사용자에게 로그인 실패 메시지 표시
			return;
		}

		LoginAccountPacketReq req = new LoginAccountPacketReq()
		{
			userId = authResult.uniqueId,
			token = authResult.token
		};

		string url = "";

		switch (providerType)
		{
			case EProviderType.Guest:
				url = "guest";
				break;
			//case EProviderType.Facebook:
			//	url = "facebook";
			//	break;
			case EProviderType.Google:
				url = "google";
				break;
			default:
				return;
		}

		Managers.Web.SendPostRequest<LoginAccountPacketRes>($"api/account/login/{url}", req, (res) =>
		{
			if (res.success)
			{
				Debug.Log("Login Success");
				Debug.Log($"AccountDbId: {res.accountDbId}");
				Debug.Log($"JWT: {res.jwt}");

				// TODO
				Debug.Log("Try to Connect to GameServer...");
			}
			else
			{
				Debug.LogError("Login Failed");
			}
		});
	}
}
