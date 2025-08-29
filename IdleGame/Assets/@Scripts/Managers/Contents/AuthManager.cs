using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Facebook.Unity;
using System;
using static Define;
using Google;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using System.Threading.Tasks;

public class AuthResult
{
	public EProviderType providerType;
	public string uniqueId;
	public string token;
	public bool isSuccess = true; // 기본값은 true로 설정
}

public class AuthManager
{
	const string FACEBOOK_APPID = "540435154335782";
	const string GOOGLE_WEBID = "658110665747-sopd9opmeb49k54ub83ck88iip9s9sdp.apps.googleusercontent.com";

	Action<AuthResult> _onLoginSucess;

    #region Google

    // Firebase 인증 인스턴스
    private FirebaseUser _user; // 로그인된 사용자 정보
    #endregion


    #region Facebook

    public void Logout()
	{
		Debug.Log("Facebook Logout");
		//FB.LogOut();
	}
	public void TryFacebookLogin(Action<AuthResult> onLoginSucess)
	{
		_onLoginSucess = onLoginSucess;

		//if (FB.IsInitialized == false)
		//{
		//	FB.Init(FACEBOOK_APPID, onInitComplete: OnFacebookInitComplete);
		//	return;
		//}

		FacebookLogin();
	}

	void OnFacebookInitComplete()
	{
		//if (FB.IsInitialized == false)
		//	return;

		//Debug.Log("OnFacebookInitComplete");
		//FB.ActivateApp();
		//FacebookLogin();
	}

	void FacebookLogin()
	{
		//Debug.Log("FacebookLogin");
		//List<string> permissions = new List<string>() { "gaming_profile", "email" };
		//FB.LogInWithReadPermissions(permissions, FacebookAuthCallback);
	}

	void FacebookAuthCallback(/*ILoginResult result*/)
	{
		//if (FB.IsLoggedIn)
		//{
		//	// 페이스북 토큰 획득
		//	AccessToken token = Facebook.Unity.AccessToken.CurrentAccessToken;

		//	AuthResult authResult = new AuthResult()
		//	{
		//		providerType = EProviderType.Facebook,
		//		uniqueId = token.UserId,
		//		token = token.TokenString,
		//	};

		//	_onLoginSucess?.Invoke(authResult);
		//}
		//else
		//{
		//	if (result.Error != null)
		//		Debug.Log($"FacebookAuthCallback Failed (ErrorCode: {result.Error})");
		//	else
		//		Debug.Log("FacebookAuthCallback Failed");
		//}
	}
	#endregion


	#region Google
	public void InitGoogle()
    {
#if UNITY_EDITOR
        Debug.Log("Google Sign-In is not supported in Unity Editor. Please test on Android device.");
        return;
#endif
        try
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                //WebClientId = "여기에_WebClientID_입력하세요", // Firebase 콘솔에서 발급받은 WebClientId
                WebClientId = GOOGLE_WEBID,
                RequestIdToken = true,                        // ID Token 요청 (Firebase 인증에 필요
                RequestEmail = true,                          // 사용자 이메일 요청
                UseGameSignIn = false                         // 게임 서비스 연동은 사용하지 않음 (일반 로그인용)
            };

            //// Firebase 의존성 확인 및 인증 인스턴스 초기화
            //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            //{
            //    if (task.Result == DependencyStatus.Available)
            //    {
            //        Debug.Log("InitGoogle Complete");
            //    }
            //    else
            //    {
            //        Debug.LogError($"X Firebase dependency error: {task.Result}");
            //    }
            //});

            EnsureFirebaseReadyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Google Sign-In configuration failed: {e.Message}\n{e.StackTrace}");
        }
    }
    private Task _firebaseReadyTask;

    // 어디서든 한 번만 호출해도 됨 (Awake에서 호출해도 되고, TryGoogleLogin에서 바로 await해도 됩니다)
    public Task EnsureFirebaseReadyAsync()
    {
        if (_firebaseReadyTask != null) return _firebaseReadyTask;

        _firebaseReadyTask = FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.Result != DependencyStatus.Available)
                    throw new Exception($"Firebase deps not available: {task.Result}");

                // ⚠️ Auth 모듈을 여기서 한 번 Touch 해서 “생성”되도록
                var _ = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Ready (Auth DefaultInstance created)");
            });

        return _firebaseReadyTask;
    }

    public void GoogleLogout()
	{
		Debug.Log("Google Logout");
#if UNITY_EDITOR
        Debug.LogWarning("Google Sign-In is not supported in Unity Editor.");
        return;
#endif
        GoogleSignIn.DefaultInstance.SignOut();
    }

	public async void TryGoogleLogin(Action<AuthResult> onLoginSucess)
	{
		_onLoginSucess = onLoginSucess;
#if UNITY_EDITOR
        // 에디터에서는 더미 로그인으로 테스트
        Debug.Log("Editor mode: Using dummy Google login for testing");
        var dummyResult = new AuthResult()
        {
            providerType = EProviderType.Google,
            uniqueId = "EDITOR_TEST_USER_" + System.DateTime.Now.Ticks,
            token = "EDITOR_DUMMY_TOKEN",
            isSuccess = true
        };

        Debug.Log("Editor mode: Dummy Google login completed");
        _onLoginSucess?.Invoke(dummyResult);
        return;
#endif
        try
        {
            // 🔸 여기서 초기화 완료 보장
            await EnsureFirebaseReadyAsync();

            GoogleLogin(); // 이제 실행해도 안전
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase init failed: {e}");
            _onLoginSucess?.Invoke(new AuthResult { providerType = EProviderType.Google, isSuccess = false });
        }

    }

	void GoogleLogin()
	{
		Debug.Log("GoogleLogin");

#if UNITY_EDITOR
        Debug.LogWarning("Google Sign-In is not supported in Unity Editor. Please test on Android device.");
        _onLoginSucess?.Invoke(new AuthResult() { providerType = EProviderType.Google, isSuccess = false });
        return;
#endif
        try
        {
            // Google Sign-In 인스턴스 초기화 시도
            var googleSignIn = GoogleSignIn.DefaultInstance;
            if (googleSignIn != null)
            {
                Debug.Log("GoogleLogin googleSignIn null이 아니다");
                // 실제 Google 계정으로 로그인 요청을 보냄. 결과는 비동기 Task<GoogleSignInUser>로 반환됨.
                googleSignIn.SignIn().ContinueWith(OnGoogleSignInCompleted);
            }
            else
            {
                Debug.LogError("Google Sign-In instance is null. Check if the plugin is properly initialized.");
                // 로그인 실패 콜백 호출
                _onLoginSucess?.Invoke(new AuthResult() { providerType = EProviderType.Google, isSuccess = false });
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Google Sign-In initialization failed: {e.Message}\n{e.StackTrace}");
            // 로그인 실패 콜백 호출
            _onLoginSucess?.Invoke(new AuthResult() { providerType = EProviderType.Google, isSuccess = false });
        }
    }

    /// <summary>
    /// Google Sign-In 결과 처리
    /// </summary>
    private void OnGoogleSignInCompleted(Task<GoogleSignInUser> task)
    {
        Debug.Log("OnGoogleSignInCompleted Start");
        if (task.IsCanceled)
        {
            Debug.LogError("! Google Sign-In was canceled.");
            return;
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"X Google Sign-In failed:\n{task.Exception}");
            return;
        }

        if (task.Exception != null)
        {
            //foreach (var ex in task.Exception.Flatten().InnerExceptions)
            //{
            //    LogError($"Google Sign-In Exception: {ex.GetType()} - {ex.Message}\n{ex.StackTrace}");
            //}

            // 디테일하게
            foreach (var ex in task.Exception.Flatten().InnerExceptions)
            {
                Debug.LogError($"[Exception TYPE] {ex.GetType()}");
                Debug.LogError($"[Exception MESSAGE] {ex.Message}");

                if (ex is Google.GoogleSignIn.SignInException gEx)
                {
                    // 리플렉션으로 모든 속성 출력
                    var props = gEx.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        try
                        {
                            var value = prop.GetValue(gEx);
                            Debug.LogError($"[PROP] {prop.Name} = {value}");
                        }
                        catch { }
                    }
                }
            }
        }
        else
        {
            Debug.Log("GoogleSignInUser Exception 없다~");
        }

        // GoogleSignInUser:
        // Google 로그인에 성공했을 때 반환되는 사용자 정보 객체.
        // 이 객체에서 Firebase 인증에 필요한 IdToken을 가져올 수 있음.
        GoogleSignInUser googleUser = task.Result; // Google 사용자 인증용 토큰

        Debug.Log($"googleUser.AuthCode : {googleUser.AuthCode}");
        Debug.Log($"googleUser.Email : {googleUser.Email}");
        Debug.Log($"googleUser.IdToken : {googleUser.IdToken}");
        Debug.Log($"googleUser.DisplayName : {googleUser.DisplayName}");
        Debug.Log($"googleUser.GivenName : {googleUser.GivenName}");
        Debug.Log($"googleUser.FamilyName : {googleUser.FamilyName}");
        Debug.Log($"googleUser.UserId : {googleUser.UserId}");


        try
        {

            // Google IdToken을 Firebase에서 사용할 수 있는 Credential 객체로 변환.
            // 이후 FirebaseAuth로 이 Credential을 사용해 로그인 가능.
            var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            // Firebase 인증 시스템에 위에서 생성한 Credential로 로그인 시도.
            Debug.Log($"GetCredential googleUser.IdToken:{googleUser.IdToken}로 시도!");
            FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(OnFirebaseSignInCompleted);


            AuthResult authResult = new AuthResult()
            {
                providerType = EProviderType.Google,
                uniqueId = googleUser.UserId,
                token = googleUser.IdToken,
            };

            _onLoginSucess?.Invoke(authResult);
        }
        catch (Exception ex)
        {
            Debug.Log($"Credential로 로그인 시도 실패. {ex}");
        }
    }

    /// <summary>
    /// Firebase 인증 결과 처리
    /// </summary>
    private void OnFirebaseSignInCompleted(Task<FirebaseUser> task)
    {
        Debug.Log($"OnFirebaseSignInCompleted Firebase 인증 결과 처리");
        if (task.IsCanceled)
        {
            Debug.LogError("! Firebase sign-in was canceled.");
            return;
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"X Firebase sign-in failed:\n{task.Exception}");
            return;
        }
        

        // 로그인 유저 정보 가져와서 설정.
        _user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (_user != null)
        {
           //_user.id
            //usernameText.text = _user.DisplayName ?? "No Name";
            //userEmailText.text = _user.Email ?? "No Email";

            //loginScreen.SetActive(false);
            //profileScreen.SetActive(true);


            Debug.Log($"O Logged in as: {_user.DisplayName} ({_user.Email})");
        }
        else
        {

            Debug.LogError($"OnFirebaseSignInCompleted CurrentUser Null !! ");

        }
    }
	#endregion



	#region Guest
	public void TryGuestLogin(Action<AuthResult> onLoginSucess)
	{
		_onLoginSucess = onLoginSucess;

		AuthResult result = new AuthResult()
		{
			//providerType = EProviderType.Guest,
			uniqueId = SystemInfo.deviceUniqueIdentifier,
			token = ""
		};

		_onLoginSucess?.Invoke(result);
	}
	#endregion
}
