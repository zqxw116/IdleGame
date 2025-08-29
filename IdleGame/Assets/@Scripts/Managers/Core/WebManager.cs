
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
    //public string ip = "127.0.0.1";


    // **** 매번 실행전 확인 필요!!!!! ****
    //public string ip = "192.168.0.4"; // PC의 LAN IP  서울대입구 집
    //public string ip = "192.168.35.9"; // PC의 LAN IP   문정 파스쿠찌
    public string ip = "192.168.219.105"; // PC의 LAN IP   다미 집

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
        //BaseUrl = $"http://{ipv4.ToString()}:{port}";
        BaseUrl = $"http://{ip}:{port}";
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

        // URL 안전 결합
        string sendUrl = $"{BaseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
        Debug.Log($"[HTTP] {method} {sendUrl}에  접속 할 예정");

        byte[] jsonBytes = null;
        if (obj != null && method != UnityWebRequest.kHttpVerbGET)
        {
            string json = JsonUtility.ToJson(obj);
            Debug.Log($"[HTTP JSON OUT] {json}");   // {"userId":"...","token":"..."} 가 찍혀야 정상
            jsonBytes = Encoding.UTF8.GetBytes(json);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            uwr.downloadHandler = new DownloadHandlerBuffer();

            if (method == UnityWebRequest.kHttpVerbGET)
            {
                uwr.uploadHandler = null; // GET에는 바디 금지
            }
            else
            {
                uwr.uploadHandler = new UploadHandlerRaw(jsonBytes ?? Array.Empty<byte>());
                uwr.SetRequestHeader("Content-Type", "application/json");
            }

            // HTTPS 테스트할 때만 사용 (HTTP에서는 의미 없음)
            // 반드시 보내기 전에 설정!
            // uwr.certificateHandler = new CertificateWhore();
            // uwr.disposeCertificateHandlerOnDispose = true;

            // 응답이 느릴 수 있으면 늘리세요 (기본 0=무제한)
            uwr.timeout = 10;

            Debug.Log($"[HTTP] {method} {sendUrl}에  접속 시도");
            yield return uwr.SendWebRequest();
            Debug.Log($"[HTTP] {method} {sendUrl}에  접속 시도 결과 수신");

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[HTTP] {method} {sendUrl}에  접속 실패 {uwr.error} (code:{uwr.responseCode}");
                yield break;
            }

            var text = uwr.downloadHandler.text;

            // 204 No Content 대비
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("Empty response");
                yield break;
            }

            // JsonUtility 제한: 루트 배열/Dictionary 미지원
            // 서버가 배열을 주면 래퍼 클래스를 사용하거나 Json.NET 사용 검토
            T resObj = JsonUtility.FromJson<T>(text);
            res?.Invoke(resObj);
            Debug.Log($"[HTTP] {method} {sendUrl}에  접속 성공");
        }
    }

}
