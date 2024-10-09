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
using UnityEngine.Serialization;
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
                
                if (!string.IsNullOrEmpty(chinese) && type != PhraseType.NoteFunc && pinyin != null)
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

        [FoldoutGroup("单词短语")] [FoldoutGroup("单词短语/配置")] [LabelText("优先值搜寻文件夹")]
        public List<DefaultAsset> foldersPriority;
        
        [ShowInInspector] [FoldoutGroup("单词短语/配置")] [LabelText("优先值搜寻目标")]
        private List<TextAsset> _priorityTextAsset = new List<TextAsset>();
        
        [ShowInInspector][TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [FoldoutGroup("单词短语/已完成短语")][LabelText("短语列表")][ReadOnly]
        private List<Phrase> _phraseList = new List<Phrase>();
        
        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [FoldoutGroup("单词短语/已完成短语")] [LabelText("关键字列表")] [Searchable]
        public List<Phrase> keywordPhraseList = new List<Phrase>();
        
        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [FoldoutGroup("单词短语/已完成短语")] [LabelText("函数API&笔记列表")] [Searchable]
        public List<Phrase> noteFuncPhraseList = new List<Phrase>();
        
        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [FoldoutGroup("单词短语/已完成短语")] [LabelText("单词列表")] [Searchable]
        public List<Phrase> wordPhraseList = new List<Phrase>();
        
        [TableList(ShowIndexLabels = true, HideToolbar = false, DrawScrollView = true)] [FoldoutGroup("单词短语")] [LabelText("单词列表(添加)")] [Searchable] [ShowInInspector]
        private List<Phrase> _wordPhraseAddList = new List<Phrase>();
        
        // [Button("单词列表添加新元素", ButtonSizes.Large)] [FoldoutGroup("单词短语")]
        // private void AddNewToWordPhraseList()
        // {
        //     wordPhraseList.Add(new Phrase());
        // }
        
        [Button("刷新短语列表", ButtonSizes.Large)] [FoldoutGroup("单词短语")] 
        private void KeywordRefreshPhraseList()
        {
            foreach (var variable in keywordPhraseList)
            {
                variable.type = Phrase.PhraseType.Keyword;
                variable.Translation();
            }
            
            foreach (var variable in wordPhraseList)
            {
                variable.type = Phrase.PhraseType.Word;
                variable.Translation();
            }

            foreach (var variable in noteFuncPhraseList)
            {
                variable.type = Phrase.PhraseType.NoteFunc;
                variable.Translation();
            }
            
            foreach (var variable in _wordPhraseAddList)
            {
                variable.type = Phrase.PhraseType.Word;
                variable.Translation();
            }
        }
        
        [Button("查找短语优先级(全部)", ButtonSizes.Large)] [ButtonGroup("单词短语/00")] 
        private void FindKeywordPriority()
        {
            foreach (var variable in keywordPhraseList)
            {
                variable.priority = 10000;
            }
            foreach (var variable in wordPhraseList)
            {
                variable.priority = 10;
            }
            foreach (var variable in _wordPhraseAddList)
            {
                variable.priority = 10;
            }
            foreach (var variable in noteFuncPhraseList)
            {
                variable.priority = 0;
            }
            
            
            if (foldersPriority == null || foldersPriority.Count == 0)
            {
                Debug.LogError("请在检查器中分配一个文件夹。");
                return;
            }
            // 清空现有列表
            _priorityTextAsset.Clear();
            foreach (var folder in foldersPriority)
            {
                // 获取文件夹路径
                string folderPath = AssetDatabase.GetAssetPath(folder);
                // Debug.Log(folderPath);
                // 递归遍历文件夹
                FindCsFilesInDirectoryToPriorityTextAsset(folderPath);
            }
            
            
            foreach (var variable in _priorityTextAsset)
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
                
                foreach (var variable1 in _wordPhraseAddList)
                {
                    MatchCollection matchCollection = Regex.Matches(text, $@"{variable1.english}", RegexOptions.IgnoreCase);
                    variable1.priority += matchCollection.Count;
                }
            }
            
            Debug.Log("获取短语优先级成功!");
        }
        
        [Button("查找短语优先级(添加)", ButtonSizes.Large)] [ButtonGroup("单词短语/00")] 
        private void FindKeywordPriorityAppend()
        {
            foreach (var variable in _wordPhraseAddList)
            {
                variable.priority = 10;
            }
            
            if (foldersPriority == null || foldersPriority.Count == 0)
            {
                Debug.LogError("请在检查器中分配一个文件夹。");
                return;
            }
            
            // 清空现有列表
            _priorityTextAsset.Clear();
            foreach (var folder in foldersPriority)
            {
                // 获取文件夹路径
                string folderPath = AssetDatabase.GetAssetPath(folder);
                // Debug.Log(folderPath);
                // 递归遍历文件夹
                FindCsFilesInDirectoryToPriorityTextAsset(folderPath);
            }
            
            foreach (var variable in _priorityTextAsset)
            {
                string text = variable.text;
                
                foreach (var variable1 in _wordPhraseAddList)
                {
                    MatchCollection matchCollection = Regex.Matches(text, $@"{variable1.english}", RegexOptions.IgnoreCase);
                    variable1.priority += matchCollection.Count;
                }
            }
            
            Debug.Log("获取短语优先级成功!");
        }
        
        private void FindCsFilesInDirectoryToPriorityTextAsset(string path)
        {
            // 获取目录中的所有文件
            string[] files = Directory.GetFiles(path);
            // Debug.Log(files[0]);

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
                        if (!_priorityTextAsset.Contains(csFile))
                        {
                            _priorityTextAsset.Add(csFile);
                        }
                    }
                }
            }
            // 获取所有子文件夹并递归处理
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                FindCsFilesInDirectoryToPriorityTextAsset(directory); // 递归查找子文件夹中的 .cs 文件
            }
        }
        
        
        
        [Button("排序短语(优先级)", ButtonSizes.Large)] [ButtonGroup("单词短语/01")] 
        private void SortByPriorityPhraseList()
        {
            keywordPhraseList = keywordPhraseList.OrderByDescending(p => p.priority).ToList();
            wordPhraseList = wordPhraseList.OrderByDescending(p => p.priority).ToList();
            noteFuncPhraseList = noteFuncPhraseList.OrderBy(p => p.pinyin, StringComparer.Ordinal).ToList();
        }
        [Button("排序短语(English)", ButtonSizes.Large)] [ButtonGroup("单词短语/01")]
        private void SortByEnglishPhraseList()
        {
            keywordPhraseList = keywordPhraseList.OrderBy(p => p.english, StringComparer.Ordinal).ToList();
            wordPhraseList = wordPhraseList.OrderBy(p => p.english, StringComparer.Ordinal).ToList();
            noteFuncPhraseList = noteFuncPhraseList.OrderBy(p => p.pinyin, StringComparer.Ordinal).ToList();
        }
        [Button("排序短语(拼音)", ButtonSizes.Large)] [ButtonGroup("单词短语/01")]
        private void SortByPinyinPhraseList()
        {
            keywordPhraseList = keywordPhraseList.OrderBy(p => p.pinyin, StringComparer.Ordinal).ToList();
            wordPhraseList = wordPhraseList.OrderBy(p => p.pinyin, StringComparer.Ordinal).ToList();
            noteFuncPhraseList = noteFuncPhraseList.OrderBy(p => p.pinyin, StringComparer.Ordinal).ToList();
        }
        
        [Button("应用到短语列表", ButtonSizes.Large)] [FoldoutGroup("单词短语")]
        private void ApplyToPhraseList()
        {
            foreach (var addPhrase in _wordPhraseAddList)
            {
                if (!wordPhraseList.Contains(addPhrase))
                {
                    wordPhraseList.Add(addPhrase);
                }
            }
            _wordPhraseAddList.Clear();
            
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

            _phraseList = _phraseList.Where(o => o.num <= 9).ToList();
            OnValidate();
        }
        
        [Button("创建短语文件", ButtonSizes.Large)] [FoldoutGroup("单词短语")]
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
        
        [Button("清除无效短语", ButtonSizes.Large)] [FoldoutGroup("单词短语")]
        private void ClearInvalidPhrase()
        {
            for (var index = 0; index < wordPhraseList.Count; index++)
            {
                if (string.IsNullOrEmpty(wordPhraseList[index].chinese))
                {
                    wordPhraseList.RemoveAt(index);
                }
            }
        }

        
        [FoldoutGroup("收集单词")] [LabelText("文件夹")] [ShowInInspector]
        private List<DefaultAsset> folders;
        
        [FoldoutGroup("收集单词")] [LabelText("收集脚本")] [ShowInInspector]
        private List<TextAsset> wordTextList = new List<TextAsset>();
        
        [FoldoutGroup("收集单词")] [LabelText("函数字符串")] [ShowInInspector]
        private List<string> functionStr = new List<string>();
         
        [FoldoutGroup("收集单词")] [LabelText("函数单词")] [ShowInInspector]
        private List<Phrase> functionWordStrList = new List<Phrase>();
        
        [FoldoutGroup("收集单词")][Button("查找脚本文件", ButtonSizes.Large)]
        public void CollectWordFindCsFiles()
        {
            if (folders == null)
            {
                Debug.LogError("请在检查器中分配一个文件夹。");
                return;
            }
            // 清空现有列表
            wordTextList.Clear();
            foreach (var folder in folders)
            {
                // 获取文件夹路径
                string folderPath = AssetDatabase.GetAssetPath(folder);
                // 递归遍历文件夹
                FindCsFilesInDirectoryToWordTextList(folderPath);
            }
            
            // 打印结果
            Debug.Log("收集 " + wordTextList.Count + " 个文件!");
        }
        
        private void FindCsFilesInDirectoryToWordTextList(string path)
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
                        if (!wordTextList.Contains(csFile))
                        {
                            wordTextList.Add(csFile);
                        }
                    }
                }
            }
            // 获取所有子文件夹并递归处理
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                FindCsFilesInDirectoryToWordTextList(directory); // 递归查找子文件夹中的 .cs 文件
            }
        }
        
        [FoldoutGroup("收集单词")] [Button("查找函数字符串", ButtonSizes.Large)]
        public void FindFunctionString()
        {
            foreach (var variable in wordTextList)
            {
                string text = variable.text;
                
                MatchCollection matches = Regex.Matches(text, @"(\w+)\(.*?\)");
                foreach (Match match in matches)
                {
                    string functionName = match.Groups[1].Value;
                    if (!functionStr.Contains(functionName))
                    {
                        functionStr.Add(functionName);
                    }
                }
            }
            Debug.Log("查找函数字符串完成!");
            
            foreach (var funStr in functionStr)
            {
                List<string> words = Regex.Matches(funStr, @"[A-Z][a-z]*")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .Where(w => w.Length >= 4)
                    .ToList();
                foreach (string variable in words)
                {
                    Phrase phrase = new Phrase() { english = variable };
                    if (!functionWordStrList.Contains(phrase))
                    {
                        functionWordStrList.Add(phrase);
                    }
                }
            }
            Debug.Log("查找函数单词完成!");
            
            foreach (var variable in wordTextList)
            {
                string text = variable.text;

                foreach (var phrase in functionWordStrList)
                {
                    MatchCollection matches = Regex.Matches(text, phrase.english);
                    phrase.priority += matches.Count;
                }
            }
            Debug.Log("查找函数字符串优先级完成!");
            
            // functionWordStrList.Sort((p1, p2) => p2.priority.CompareTo(p1.priority));
            functionWordStrList = new List<Phrase>(functionWordStrList.OrderByDescending(o => o.priority));
        }

        [FoldoutGroup("收集单词")] [Button("清除函数单词列表", ButtonSizes.Large)]
        public void ClearFunctionWordList()
        {
            functionStr.Clear();
            functionWordStrList.Clear();
        }

        [FoldoutGroup("收集单词")] [Button("应用到单词列表", ButtonSizes.Large)]
        public void ApplyToWordList()
        {
            foreach (var functionWord in functionWordStrList)
            {
                if (functionWord.priority < 5)
                {
                    continue;
                }
                bool isContains = false;
                foreach (var phrase in wordPhraseList)
                {
                    if (phrase.english == functionWord.english)
                    {
                        isContains = true;
                        break;
                    }
                }
                foreach (var phrase in keywordPhraseList)
                {
                    if (string.Equals(phrase.english, functionWord.english, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isContains = true;
                        break;
                    }
                }
                
                if (!isContains)
                {
                    wordPhraseList.Add(functionWord);
                }
            }
            Debug.Log("应用到单词列表完成!");
            
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
