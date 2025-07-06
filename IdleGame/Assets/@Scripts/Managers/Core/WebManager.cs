
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 임시로 사용할 가짜로 무조건 true 주는 것
/// </summary>
public class CertificateWhore : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}

public class WebManager
{
    public string BaseUrl { get; set; }
    // 루프백 주소는 네트워크상에서 자신을 나타내는 가상적인 주소이며, 자신에게 다시 네트워크 입력이 들어온다고 하여 루프백 (Loopback) 주소
    // 나중에는 도메인 기반으로 바꿔야 한다. www.naver.com DNS -> ?.?.?.?
    public string ip = "127.0.0.1";
    public int port = 7777;

    public void Init()
    {
        IPAddress ipv4 = Util.GetIpv4Address(ip); // 추후도메인 주소 넣으면 ip주소 받을 수 있게 미리 로직 적용
        if (ipv4 == null)
        {
            Debug.LogError("WebServer IPv4 Failed");
            return;
        }

        //  http://127.0.0.1:7777??  -> 나중에 https로 변경 필요.
        BaseUrl = $"http://{ipv4.ToString()}:{port}";
        Debug.Log($"WebServer BaseUrl : {BaseUrl}");
    }

    /// <summary>
    /// 이 프로젝트에서는 PostRequest로 할 예정
    /// </summary>
    public void SendPostRequest<T>(string url, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, res));
    }

    public void SendGetRequest<T>(string url, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbGET, obj, res));
    }

    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> res)
    {
        if (string.IsNullOrEmpty(BaseUrl))
            Init();

        // http://127.0.0.1:7777/test/hello
        string sendUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;
        if (obj != null)
        {
            string jsonStr = JsonUtility.ToJson(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        // 유니티에서 제공하는 웹 기능
        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            // 보내는 부분
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.certificateHandler = new CertificateWhore(); // 임시로 사용할 가짜로 무조건 true 주는 것
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            // 받는 부분
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"CoSendWebRequest Failed : {uwr.error}");
            }
            else
            {
                T resObj = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
                res.Invoke(resObj);
            }
        }
    }
}
