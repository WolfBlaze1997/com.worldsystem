#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Pinyin4net;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using WorldSystem.Language;
using File = System.IO.File;
using Formatting = Unity.Plastic.Newtonsoft.Json.Formatting;
using JsonConvert = Unity.Plastic.Newtonsoft.Json.JsonConvert;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace WorldSystem.Runtime
{
    
    
    [ExecuteAlways]
    public class BaiduInputMethodPhrase : BaseModule
    {
        [Serializable]
        public class Phrase
        {
            public enum PhraseType
            {
                 Word,Keyword,NoteFunc,
            }
            
            [TableColumnWidth(50, Resizable = false)]
            public int priority;
            
            [TableColumnWidth(80, Resizable = false)]
            public PhraseType type = PhraseType.Word;
            
            [TableColumnWidth(35, Resizable = false)]
            public int num = 1;
            
            [TableColumnWidth(30, Resizable = true)]
            public string pinyin = "";
            
            [TableColumnWidth(35, Resizable = false)]
            public string flag = "";

            public string chinese = "";

            public string english = "";
            
            public string phonetic = "";

            public Phrase()
            {
            }
            
            public Phrase(Phrase ph)
            {
                priority = ph.priority;
                type = ph.type;
                num = ph.num;
                pinyin = ph.pinyin;
                flag = ph.flag;
                chinese = ph.chinese;
                english = ph.english;
                phonetic = ph.phonetic;
            }
            
            public string CombinePhrase()
            {
                string finalstring = $"{num},{pinyin}=#{{{flag + (phonetic == "" ? "" : "[" + phonetic + "]") + english + chinese}}}{english}\n";
                return finalstring;
            }

            public List<Phrase> SplitPhrase()
            {
                List<Phrase> outList = new List<Phrase>();
                outList.Add(new Phrase(this));
                if (type == PhraseType.Keyword || type == PhraseType.Word)
                {
                    char[] englishcChars = english.ToLower().ToCharArray();
                    string enPinyin = "";
                    for (var index = 0; index < (englishcChars.Length > 8 ? 8 : englishcChars.Length); index++)
                    {
                        Phrase phrase = new Phrase(this);
                        enPinyin += englishcChars[index];
                        phrase.pinyin = enPinyin;
                        outList.Add(phrase);
                    }
                }
                return outList;
            }
            
            public async void Translation()
            {
                flag = type switch
                {
                    PhraseType.Keyword => "(K)",
                    PhraseType.Word => "",
                    _ => flag
                };
                BaiduTranslationAPI.AppId = "20221018001399306";
                BaiduTranslationAPI.SecretKey = "pAmts8VwuxpdvvvhvSUA";
                
                if (string.IsNullOrEmpty(english) && !string.IsNullOrEmpty(chinese))
                {
                    english = await BaiduTranslationAPI.Translate(chinese);
                    Debug.Log("百度翻译: " + chinese + " = " + english + " ......");
                }
                if (!string.IsNullOrEmpty(english) && string.IsNullOrEmpty(chinese))
                {
                    string chineseCache = await BaiduTranslationAPI.Translate(english);
                    if (IsChinese(chineseCache))
                    {
                        chinese = chineseCache;
                        Debug.Log("百度翻译: " + chinese + " = " + english + " ......");
                    }
                }
                
                if (!string.IsNullOrEmpty(chinese) && type != PhraseType.NoteFunc)
                {
                    pinyin = ToPinyin(Regex.Replace(chinese, @"\s*\(.*?\)\s*", ""));
                }

                if (!string.IsNullOrEmpty(english) && type == PhraseType.Word)
                {
                    var temporary = english.ToCharArray();
                    temporary[0] = temporary[0].ToString().ToUpper().ToCharArray()[0];
                    string newEnglish = "";
                    foreach (var variable in temporary)
                    {
                        newEnglish += variable;
                    }

                    english = newEnglish;
                }
            }

            private bool IsChinese(string query)
            {
                return Regex.IsMatch(query, @"[\u4e00-\u9fa5]");
            }
            
            private static string ToPinyin(string hanzi)
            {
                var hanzis = hanzi.ToCharArray();
                var pinyin = "\'";
                foreach (var variable in hanzis)
                {
                    string py = PinyinHelper.ToHanyuPinyinStringArray(variable)[0];
                    py = py.Remove(py.Length - 1);
                    pinyin += (py + "\'");
                }
                return pinyin;
            }
            
            public override bool Equals(object obj)
            {
                if (obj is Phrase other)
                {
                    if (
                        english == other.english && 
                        pinyin == other.pinyin 
                    )
                    {
                        return true;
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(english, pinyin);
            }
            
            public static bool operator ==(Phrase a, Phrase b)
            {
                // 如果两个对象都为 null，则相等
                if (ReferenceEquals(a, b)) return true;
                // 如果其中一个为 null，则不相等
                if (a is null || b is null) return false;
                // 否则比较它们的内容
                return a.Equals(b);
            }

            public static bool operator !=(Phrase a, Phrase b)
            {
                return !(a == b);
            }
            
        }

        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [LabelText("关键字列表")]
        public List<Phrase> keywordPhraseList = new List<Phrase>();
        
        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [LabelText("单词列表")]
        public List<Phrase> wordPhraseList = new List<Phrase>();
        
        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [LabelText("函数API&笔记列表")]
        public List<Phrase> noteFuncPhraseList = new List<Phrase>();
        
        [ShowInInspector][TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [LabelText("短语列表")][ReadOnly]
        private List<Phrase> _phraseList = new List<Phrase>();
        
        private readonly List<TextAsset> _textAsset = new List<TextAsset>();

        [Button("添加新元素", ButtonSizes.Large)]
        private void AddNewToWordPhraseList()
        {
            wordPhraseList.Add(new Phrase());
        }
        
        [Button("获取短语优先级")]
        private void FindKeywordPriority()
        {
            _textAsset.Clear();
            foreach (var variable in keywordPhraseList)
            {
                variable.priority = 10000;
            }
            foreach (var variable in wordPhraseList)
            {
                variable.priority = 10;
            }
            foreach (var variable in noteFuncPhraseList)
            {
                variable.priority = 0;
            }
            string path = Application.dataPath.Replace("Assets", "");
            FindCsFilesInDirectory(path);
            foreach (var variable in _textAsset)
            {
                string text = variable.text;

                foreach (var variable0 in keywordPhraseList)
                {
                    MatchCollection matchCollection = Regex.Matches(text, $@"\b{variable0.english}\b", RegexOptions.IgnoreCase);
                    variable0.priority += matchCollection.Count;
                }

                foreach (var variable1 in wordPhraseList)
                {
                    MatchCollection matchCollection = Regex.Matches(text, $@"{variable1.english}", RegexOptions.IgnoreCase);
                    variable1.priority += matchCollection.Count;
                }
            }
            
            
            Debug.Log("获取短语优先级成功!");
        }
        
        [Button("刷新关键字列表")]
        private void KeywordRefreshPhraseList()
        {
            foreach (var variable in keywordPhraseList)
            {
                variable.type = Phrase.PhraseType.Keyword;
                variable.Translation();
            }
            // keywordPhraseList.Sort((p1, p2) => p2.priority.CompareTo(p1.priority));
            keywordPhraseList = keywordPhraseList.OrderBy(p => p.english, StringComparer.Ordinal).ToList();
            
            foreach (var variable in wordPhraseList)
            {
                variable.type = Phrase.PhraseType.Word;
                variable.Translation();
            }
            // wordPhraseList.Sort((p1, p2) => p2.priority.CompareTo(p1.priority));
            wordPhraseList = wordPhraseList.OrderBy(p => p.english, StringComparer.Ordinal).ToList();

            foreach (var variable in noteFuncPhraseList)
            {
                variable.type = Phrase.PhraseType.NoteFunc;
                variable.Translation();
            }
            noteFuncPhraseList = noteFuncPhraseList.OrderBy(p => p.pinyin, StringComparer.Ordinal).ToList();
            
        }

        [Button("应用到短语列表")]
        private void ApplyToPhraseList()
        {
            
            _phraseList.Clear();
            foreach (var variable in keywordPhraseList)
            {
                var splitPhrase = variable.SplitPhrase();
                foreach (var variable0 in splitPhrase)
                {
                    if(!_phraseList.Contains(variable0))
                        _phraseList.Add(variable0);
                }
            }
            foreach (var variable in wordPhraseList)
            {
                var splitPhrase = variable.SplitPhrase();
                foreach (var variable0 in splitPhrase)
                {
                    if(!_phraseList.Contains(variable0))
                        _phraseList.Add(variable0);
                }
            }
            foreach (var variable in noteFuncPhraseList)
            {
                var splitPhrase = variable.SplitPhrase();
                foreach (var variable0 in splitPhrase)
                {
                    if(!_phraseList.Contains(variable0))
                        _phraseList.Add(variable0);
                }
            }
            
            _phraseList = _phraseList
                .OrderBy(p => p.pinyin, StringComparer.Ordinal)             // 按 Name 字段升序
                .ThenByDescending(p => p.priority)     // 如果 Name 相同，则按 Age 降序
                .ToList();

            string  previousPinyin = null;
            int num = 0;
            for (int i = 0; i < _phraseList.Count; i++)
            {
                if (_phraseList[i].pinyin == previousPinyin)
                {
                    num++; 
                }
                else
                {
                    num = 1; 
                    previousPinyin = _phraseList[i].pinyin;
                }
                _phraseList[i].num = num;
            }
            
        }
        
        [Button("创建短语文件")]
        private void CreatePhraseFile()
        {
            string PhraseText = "";
            foreach (var variable in _phraseList)
            {
                PhraseText += variable.CombinePhrase();
            }
            File.WriteAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "短语.txt", PhraseText);
            
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\Baidu\BaiduPinyin\Account\rTrdnXpOmd\userphrase.dat";
            using (StreamWriter sw = new StreamWriter(appDataPath, false, Encoding.Unicode))  
            {  
                sw.Write(PhraseText);  
            } 
        }
        
        private void FindCsFilesInDirectory(string path)
        {
            // 获取目录中的所有文件
            string[] files = Directory.GetFiles(path);
            // List<TextAsset> textAsset = new List<TextAsset>();
            // 查找 .cs 文件并加载为 TextAsset
            foreach (string file in files)
            {
                if (file.EndsWith(".cs") || file.EndsWith(".shader") || file.EndsWith(".hlsl"))
                {
                    // 获取相对路径以加载 TextAsset
                    string relativePath = file.Replace(Application.dataPath.Replace("Assets", ""), "").Replace("\\", "/");
                    TextAsset csFile = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
                    // 如果成功加载为 TextAsset，则添加到列表中
                    if (csFile != null)
                    {
                        _textAsset.Add(csFile);
                    }
                }
            }
            // 获取所有子文件夹并递归处理
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                FindCsFilesInDirectory(directory); // 递归查找子文件夹中的 .cs 文件
            }
        }
        
        
        #region 事件函数

        private void OnEnable()
        {
            OnValidate();
        }

        public void OnValidate()
        {
            keywordPhraseList = keywordPhraseList.GroupBy(n => n).Select(g => g.First()).ToList();
            if ((keywordPhraseList == null || keywordPhraseList.Count == 0) && File.Exists(
                    Application.dataPath.Replace("Assets", "") + "Packages/com.worldsystem/短语/" +
                    "keywordPhraseList.txt"))
            {
                keywordPhraseList = JsonConvert.DeserializeObject<List<Phrase>>(
                    File.ReadAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "keywordPhraseList.txt"));
            }
            
            if ((wordPhraseList == null || wordPhraseList.Count == 0) && File.Exists(
                    Application.dataPath.Replace("Assets", "") + "Packages/com.worldsystem/短语/" +
                    "wordPhraseList.txt"))
            {
                wordPhraseList = JsonConvert.DeserializeObject<List<Phrase>>(
                    File.ReadAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "wordPhraseList.txt"));
            }
            
            if ((noteFuncPhraseList == null || noteFuncPhraseList.Count == 0) && File.Exists(
                    Application.dataPath.Replace("Assets", "") + "Packages/com.worldsystem/短语/" +
                    "noteFuncPhraseList.txt"))
            {
                noteFuncPhraseList = JsonConvert.DeserializeObject<List<Phrase>>(
                    File.ReadAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "noteFuncPhraseList.txt"));
            }
            
            if(keywordPhraseList != null && keywordPhraseList.Count > 0)
                File.WriteAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "keywordPhraseList.txt", JsonConvert.SerializeObject(keywordPhraseList, Formatting.Indented));
            if(wordPhraseList != null && wordPhraseList.Count > 0)
                File.WriteAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "wordPhraseList.txt", JsonConvert.SerializeObject(wordPhraseList, Formatting.Indented));
            if(noteFuncPhraseList != null && noteFuncPhraseList.Count > 0)
                File.WriteAllText(Application.dataPath.Replace("Assets", "")+ "Packages/com.worldsystem/短语/" + "noteFuncPhraseList.txt", JsonConvert.SerializeObject(noteFuncPhraseList, Formatting.Indented));

        }

        
        #endregion
        
    }
}
#endif
