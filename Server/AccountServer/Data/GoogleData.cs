namespace AccountServer.Models
{
    public class GoogleTokenData
    {
        public string iss { get; set; } = string.Empty; // "https://accounts.google.com", (토큰 발급자)
        public string azp { get; set; } = string.Empty; // "앱의 client_id",
        public string aud { get; set; } = string.Empty; // "앱의 client_id",
        public string sub { get; set; } = string.Empty; // "123456789012345678901", 구글 유저 고유 ID
        public string email { get; set; } = string.Empty; // "user@gmail.com"
        public bool email_verified { get; set; }            // true,  이메일 검증 여부
        public string name { get; set; } = string.Empty;    // "홍길동",
        public string picture { get; set; } = string.Empty; // "https://lh3.googleusercontent.com/a-/profilepic",
        public string given_name { get; set; } = string.Empty;  // "길동",
        public string family_name { get; set; } = string.Empty; // 홍",
        public long iat { get; set; }                           // 1692432000, (발급/만료 시간)
        public long exp { get; set; }                           // 1692435600  (발급/만료 시간)
    }
}
