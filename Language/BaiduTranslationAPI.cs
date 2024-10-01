using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace WorldSystem.Language
{
    public static class BaiduTranslationAPI
    {
        public static string AppId = "";
        
        public static string SecretKey = "";

        private const string URL = "https://fanyi-api.baidu.com/api/trans/vip/translate";
        
        public static async Task<string> Translate(string query)
        {
            if (AppId == "" || SecretKey == "")
            {
                Debug.Log("请设置您的百度AppID与秘钥!");
                return null;
            }
            string from, to;
            // 简单的语言检测
            if (Regex.IsMatch(query, @"[\u4e00-\u9fa5]"))
            {
                from = "zh"; to = "en";
            }
            else
            {
                from = "en"; to = "zh";
            }
            
            System.Random random = new System.Random();
            string salt = random.Next(10000, 99999).ToString();
            string sign;
            string sign1 = AppId + query + salt + SecretKey;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(sign1);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                sign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            string requestUrl = $"{URL}?q={UnityWebRequest.EscapeURL(query)}&from={from}&to={to}&appid={AppId}&salt={salt}&sign={sign}";
            
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                // 解析JSON以获取翻译结果
                string translationResult = "";
                string pattern = "\"dst\":\"(.*?)\"";
                Match match = Regex.Match(json, pattern);
                if (match.Success)
                {
                    string translatedText = match.Groups[1].Value;
                    if (translatedText.Contains("\\u"))
                    {
                        translatedText = Regex.Unescape(translatedText);
                    }
                    translationResult = translatedText;
                }
                return translationResult;
            }
        }
        
    }
}