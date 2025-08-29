using AccountServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AccountServer.Services
{
    public class FacebookService
    {
        HttpClient _httpClient; // 개발 쉬운 http

        // {app_id}|{app_secret} 
        readonly string _accessToken = "GG|540435154335782|9oGyj8cigWaMXoppGFjE8faw4mI"; // TODO Secret

        public FacebookService()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri("https://graph.facebook.com/") }; // facebook에 요청할 주소
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FacebookTokenData?> GetUserTokenData(string inputToken)
        {
            // 3. 다른 서버(페이스북 구글)에 유저 정보 맞는지 요청.   4. await으로 정보 수신 대기
            HttpResponseMessage response = await _httpClient.GetAsync($"debug_token?input_token={inputToken}&access_token={_accessToken}");//우리가 볼 수 있다고 주장

            if (!response.IsSuccessStatusCode)
                return null;

            // 4. 수신받은 정보를 JSON으로 DATA 변환 후 전달
            string resultStr = await response.Content.ReadAsStringAsync();

            FacebookResponseJsonData? result = JsonConvert.DeserializeObject<FacebookResponseJsonData>(resultStr);
            if (result == null)
                return null;

            return result.data;
        }
    }
}
