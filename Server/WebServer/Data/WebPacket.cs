
// Require
// 클라 -> 서버
[Serializable] // 유니티에서는 붙여야 된다
public class TestPackReq
{
    public string userId { get; set; }
    public string token { get; set; }

}

// Respone
// 서버 -> 클라
[Serializable]
public class TestPackRes
{
    public bool success { get; set; }
}