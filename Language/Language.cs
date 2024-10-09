#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = System.Random;


namespace WorldSystem.Language
{
    [Serializable]
    [CreateAssetMenu(fileName = "汉化注入器.asset", menuName = "世界系统/汉化注入器")]
    public class Language : ScriptableObject
    {

        // 搜索器类
        [Serializable]
        public class Searcher
        {
            public Searcher(string customRegex, RegexOptions regexOp, bool isFunctionInside, int nameIndex,
                string name, bool isBuiltin, SearchMode searchMode, List<string> searchFunctionOrAttributeAPI,
                List<string> excludeFunctionOrAttributeAPI, List<string> variableNameList, bool isBroadcast)
            {
                this.name = name;
                this.isBuiltin = isBuiltin;
                this.customRegex = customRegex;
                this.regexOp = regexOp;
                this.isFunctionInside = isFunctionInside;
                this.nameIndex = nameIndex;
                this.searchMode = searchMode;
                this.searchFunctionOrAttributeAPI = searchFunctionOrAttributeAPI;
                this.excludeFunctionOrAttributeAPI = excludeFunctionOrAttributeAPI;
                this.variableNameList = variableNameList;
                this.isBroadcast = isBroadcast;
            }

            public enum SearchMode
            {
                [LabelText("自定义")] Custom,
                [LabelText("数组列表或字典")] ArrayListOrDictionary,
                [LabelText("函数或特性")] FunctionOrAttributeSearch,
            }

            [HideInInspector] public bool isBuiltin;

            [LabelText("名字")] [DisableIf("isBuiltin")]
            public string name;

            [LabelText("正则表达式")] [DisableIf("isBuiltin")] [ShowIf("@searchMode == SearchMode.Custom")]
            public string customRegex;

            [ShowInInspector]
            [LabelText("正则表达式-匹配(...)")]
            [DisableIf("isBuiltin")]
            [ShowIf("@searchMode == SearchMode.FunctionOrAttributeSearch")]
            public const string FuncOrAttributeRegex = "{FunctionOrAttribute}" + BracketsMatch;

            [ShowInInspector]
            [LabelText("正则表达式-匹配{...}")]
            [DisableIf("isBuiltin")]
            [ShowIf("@searchMode == SearchMode.ArrayListOrDictionary")]
            public const string ArrayListOrDictionaryRegex = "{VariableName}" + BracketsMatch;

            [LabelText("正则搜索选项")] [DisableIf("isBuiltin")] [EnumPaging]
            public RegexOptions regexOp;

            [LabelText("搜索模式")] [DisableIf("isBuiltin")] [EnumPaging]
            public SearchMode searchMode;

            [LabelText("使用广播")] [DisableIf("isBuiltin")]
            public bool isBroadcast;

            [LabelText("限定至函数头")] [DisableIf("isBuiltin")] [ShowIf("@searchMode == SearchMode.Custom")]
            public bool isFunctionInside;

            [LabelText("名字索引")]
            [ShowIf("@isFunctionInside && searchMode == SearchMode.Custom")]
            [DisableIf("isBuiltin")]
            public int nameIndex;
            
            [LabelText("正向搜索{FunctionOrAttribute}")]
            [HorizontalGroup("01")]
            [ShowIf("@searchMode == SearchMode.FunctionOrAttributeSearch")]
            [ListDrawerSettings(CustomAddFunction = "AppendFunctionOrAttributeSearch",
                OnTitleBarGUI = "AppendUniversalFunctionOrAttributeSearch")]
            public List<string> searchFunctionOrAttributeAPI;
            
            [LabelText("负向排除")]
            [HorizontalGroup("01")]
            [ShowIf("@searchMode == SearchMode.FunctionOrAttributeSearch")]
            [ListDrawerSettings(CustomAddFunction = "AppendFunctionOrAttributeExclude",
                OnTitleBarGUI = "AppendUniversalFunctionOrAttributeExclude")]
            public List<string> excludeFunctionOrAttributeAPI;
            
            [FormerlySerializedAs("arrayListDictionaryName")]
            [LabelText("正向搜索{VariableName}")]
            [ShowIf("@searchMode == SearchMode.ArrayListOrDictionary")]
            [ListDrawerSettings(CustomAddFunction = "AppendVariableName")]
            public List<string> variableNameList;
            
            /// <summary>
            /// 添加通用函数或特性搜索器
            /// </summary>
            public void AppendUniversalFunctionOrAttributeSearch()
            {
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
                {
                    List<string> universalSearchFuncAPI = UniversalTranslationFunction.Instance.universalSearchFuncAPI;
                    foreach (var variable in universalSearchFuncAPI)
                    {
                        if (!searchFunctionOrAttributeAPI.Contains(variable))
                            searchFunctionOrAttributeAPI.Add(variable);
                    }
                }
            }

            /// <summary>
            /// 添加自定义函数或特性搜索器
            /// </summary>
            public void AppendFunctionOrAttributeSearch()
            {
                searchFunctionOrAttributeAPI.Add("");
                RemoveFuncListRepeatElement(searchFunctionOrAttributeAPI);
            }
            
            /// <summary>
            /// 添加通用函数或特性排除
            /// </summary>
            public void AppendUniversalFunctionOrAttributeExclude()
            {
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
                {
                    List<string> universalSearchFuncAPI = UniversalTranslationFunction.Instance.universalExcludeFuncAPI;
                    foreach (var variable in universalSearchFuncAPI)
                    {
                        if (!excludeFunctionOrAttributeAPI.Contains(variable))
                            excludeFunctionOrAttributeAPI.Add(variable);
                    }
                }
            }

            /// <summary>
            /// 添加自定义函数或特性排除
            /// </summary>
            public void AppendFunctionOrAttributeExclude()
            {
                excludeFunctionOrAttributeAPI.Add("");
                RemoveFuncListRepeatElement(excludeFunctionOrAttributeAPI);
            }
            
            /// <summary>
            /// 添加变量名
            /// </summary>
            public void AppendVariableName()
            {
                variableNameList.Add("");
                RemoveFuncListRepeatElement(variableNameList);
            }

            public override bool Equals(object obj)
            {
                if (obj is Searcher other)
                {
                    if (
                        this.isBuiltin == other.isBuiltin &&
                        this.name == other.name &&
                        this.customRegex == other.customRegex &&
                        this.regexOp == other.regexOp &&
                        this.searchMode == other.searchMode &&
                        this.isBroadcast == other.isBroadcast &&
                        this.isFunctionInside == other.isFunctionInside &&
                        this.nameIndex == other.nameIndex
                    )
                    {
                        return true;
                    }
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(isBuiltin, name, customRegex, regexOp, searchMode, isBroadcast,
                    isFunctionInside, nameIndex);
            }
            
            public static bool operator ==(Searcher a, Searcher b)
            {
                if (ReferenceEquals(a, b)) return true;
                if (a is null || b is null) return false;
                return a.Equals(b);
            }

            public static bool operator !=(Searcher a, Searcher b)
            {
                return !(a == b);
            }

        }
        
        /// <summary>
        /// 内置搜索器
        /// </summary>
        private static List<Searcher> _builtinSearcherList = new List<Searcher>()
        {
            new Searcher("",
                RegexOptions.None, false, 0, "搜索函数&特性", true,
                Searcher.SearchMode.FunctionOrAttributeSearch, new List<string>(), new List<string>(),
                new List<string>(), false),

            new Searcher("",
                RegexOptions.None, false, 0, "搜索字段-数组列表等", true,
                Searcher.SearchMode.ArrayListOrDictionary, new List<string>(), new List<string>(), new List<string>(),
                false),

            new Searcher("",
                RegexOptions.None, false, 0, "搜索字段-字典等", true,
                Searcher.SearchMode.ArrayListOrDictionary, new List<string>(), new List<string>(), new List<string>(),
                true),

            new Searcher(@"\b(?:public|private|protected|internal)(?:.*?)string\s*(\w+)\s*=(?:.*?)@?""(?:.*?)""\s*;",
                RegexOptions.None, true, 1, "搜索字段-字符串声明(单行)", true,
                Searcher.SearchMode.Custom, new List<string>(), new List<string>(), new List<string>(), false),
        };
        
        /// <summary>
        /// 匹配括号
        /// </summary>
        private const string BracketsMatch = @"\s*\((?>[^()]+|\((?>[^()]+|\([^()]*\))*\))*\)";

        /// <summary>
        /// 匹配双引号
        /// </summary>
        private const string DoubleQuotationMarksMatch = @"""(?:\\.|[^\\""])*""";
        
        [LabelText("文件夹")] [FoldoutGroup("注入目标")]
        public DefaultAsset folder;

        [LabelText("需要翻译的脚本")] [FoldoutGroup("注入目标")]
        public List<TextAsset> textAsset;

        [LabelText("通用函数库")] [InlineEditor(InlineEditorObjectFieldModes.Hidden)] [FoldoutGroup("通用函数库")]
        public UniversalTranslationFunction universalFunction;
        
        [LabelText("搜索器列表")] [FoldoutGroup("搜索器")] [ListDrawerSettings(OnTitleBarGUI = "AppendBuiltinSearcher")]
        public List<Searcher> searcherList = new List<Searcher>();

        public void AppendBuiltinSearcher()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                foreach (var variable in _builtinSearcherList)
                {
                    if (!searcherList.Contains(variable))
                    {
                        searcherList.Add(variable);
                    }
                }
            }
        }
        
        [LabelText("翻译目标函数头")] [ReadOnly] [FoldoutGroup("搜索器")]
        public List<string> functionHeadList = new List<string>();

        [LabelText(" 使用英汉双语")] [FoldoutGroup("搜索器")] [ToggleLeft] [HorizontalGroup("搜索器/02", 0.3f)]
        public bool useBilingual;

        [LabelText(" 反转双语位置")] [HorizontalGroup("搜索器/02")] [ToggleLeft] [ShowIf("useBilingual")]
        public bool reverseBilingual = true;
        
        [LabelText("排除词条")] [FoldoutGroup("搜索器")] [HorizontalGroup("搜索器/01")]
        public List<string> excludeList = new List<string>();

        [LabelText("搜索器缓存广播词条")] [HorizontalGroup("搜索器/01")] [ReadOnly]
        public List<string> broadcastCacheList = new List<string>();

        [LabelText("广播词条")] [HorizontalGroup("搜索器/01")]
        public List<string> broadcastList = new List<string>();

        [ShowInInspector] [LabelText("词条")] [FoldoutGroup("汉化字典")] [Searchable]
        public Dictionary<string, string> SinicizationDictionary = new Dictionary<string, string>();

        [ShowInInspector] [LabelText("未汉化的词条")] [FoldoutGroup("汉化字典")] [Searchable]
        public Dictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

        [HideLabel] [TextArea(4, 10)] [FoldoutGroup("汉化字典")]
        public string textBox = "我正在进行汉化工作, 这是Unity中的XXXX插件的词条, 请考虑使用环境和上下文翻译为最佳的中文, 不适合翻译的词条不进行汉化, 注意格式, 不要更改原来英文的字符, 使用代码块提交给我, 不要使用大括号, 最后的语句需要加上逗号, 这样我复制起来方便一些";
        
        [HideInInspector]
        public string appIdSerialize = ""; 
        
        [HideInInspector]
        public string secretKeySerialize = ""; 

        [FoldoutGroup("汉化字典")] [ShowInInspector] [LabelText("百度AppID")] 
        public static string AppId = ""; 
        
        [FoldoutGroup("汉化字典")] [ShowInInspector] [LabelText("百度SecretKey")]
        public static string SecretKey = ""; 
        
        public void OnValidate()
        {
            //序列化
            if(!string.IsNullOrEmpty(AppId))
                appIdSerialize = AppId;
            if(!string.IsNullOrEmpty(SecretKey))
                secretKeySerialize = SecretKey;
            //反序列化
            if(string.IsNullOrEmpty(AppId))
                AppId = appIdSerialize;
            if(string.IsNullOrEmpty(SecretKey))
                SecretKey = secretKeySerialize;
            
            universalFunction = UniversalTranslationFunction.Instance;
            if (universalFunction != null)
            {
                EditorUtility.SetDirty(universalFunction);
            }

            if (SinicizationDictionary.Count > 0)
            {
                foreach (var VARIABLE in excludeList)
                {
                    if (SinicizationDictionary.ContainsKey(VARIABLE))
                        SinicizationDictionary.Remove(VARIABLE);
                }

                if (EmptyDictionary.Count > 0)
                {
                    var list0 = EmptyDictionary.Values.ToList();
                    for (var index = 0; index < list0.Count; index++)
                    {
                        if (list0[index] != "" && !list0[index].Contains("未翻译!"))
                        {
                            SinicizationDictionary[EmptyDictionary.Keys.ToList()[index]] = list0[index];
                        }
                    }
                }

                var valueList = SinicizationDictionary.Values.ToList();
                Dictionary<string, string> Temporary = new Dictionary<string, string>();
                for (var index = 0; index < valueList.Count; index++)
                {
                    if (valueList[index] == "" || valueList[index].Contains("未翻译!"))
                    {
                        Temporary.Add(SinicizationDictionary.Keys.ToList()[index], valueList[index]);
                    }
                }

                EmptyDictionary = Temporary;

            }

            if (SinicizationDictionary.Count > 0)
            {
                File.WriteAllText(GetAssetAbsolutePath(this).Replace(this.name + ".asset", "Dictionary.json"),
                    JsonConvert.SerializeObject(SinicizationDictionary, Formatting.Indented));
            }

            EditorUtility.SetDirty(this);
        }

        private void OnEnable()
        {
            Awake();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("@UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(\"WB_LANGUAGE_CHINESE\")")]
        [Button(ButtonSizes.Large, Name = "语言设置: 中文"), GUIColor(0.5f, 0.5f, 1f)]
        private void SinicizationToggle_Off()
        {
            RemoveDefineSymbol();
        }

        [PropertyOrder(-100)]
        [HideIf("@UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(\"WB_LANGUAGE_CHINESE\")")]
        [Button(ButtonSizes.Large, Name = "Language Settings: English"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void SinicizationToggle_On()
        {
            AddDefineSymbol();
        }
        
        [Button("百度翻译未汉化词条", ButtonSizes.Large)] [GUIColor(0.5f, 0.5f, 1f)] [FoldoutGroup("汉化字典")]
        private async void BaiduTranslationDictionary()
        {
            BaiduTranslationAPI.AppId = AppId;
            BaiduTranslationAPI.SecretKey = SecretKey;

            Label : 
            List<Task<string>> translationTasks = new List<Task<string>>();
            // 一次只执行12个任务
            // 收集所有需要翻译的任务
            for (int i = 0; i < (EmptyDictionary.Keys.ToList().Count < 10 ? EmptyDictionary.Keys.ToList().Count : 10); i++)
            {
                if (EmptyDictionary[EmptyDictionary.Keys.ToList()[i]].Contains("未翻译!") || EmptyDictionary[EmptyDictionary.Keys.ToList()[i]] == "")
                {
                    string key = EmptyDictionary.Keys.ToList()[i];
                    translationTasks.Add(Task.Run(async () => 
                    {
                        await Task.Delay(120); // 等待250毫秒
                        string translation = await BaiduTranslationAPI.Translate(key) ?? "";
                        if (!string.IsNullOrEmpty(translation))
                        {
                            EmptyDictionary[key] = translation;
                            Debug.Log("百度翻译中: " + key + " = " + translation + " ......");
                        }
                        return translation;
                    }));
                }
            }
            // 等待所有翻译任务完成
            await Task.WhenAll(translationTasks);
            OnValidate();

            if (EmptyDictionary.Values.ToList().Find(o => o.Contains("未翻译!") || o == "") != null)
            {
                goto Label;
            }
            Debug.Log("翻译完成!");
        }
        
        [Button("查找脚本文件", 50)]
        private void FindScriptFile()
        {
            string Path = AssetDatabase.GetAssetPath(this);
            if (!IsAtWbLanguageInside(Path))
            {
                Debug.Log("汉化注入器需要在WB_Language文件夹内!请在需要注入汉化的插件根目录创建WB_Language文件夹!");
                return;
            }

            if (folder == null)
            {
                int lastSlashIndex = Path.LastIndexOf('/');
                int secondLastSlashIndex = Path.LastIndexOf('/', lastSlashIndex - 1);
                Path = Path.Substring(0, secondLastSlashIndex);
                folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(Path);
                CollectCsFiles();
            }
            else
            {
                CollectCsFiles();
            }
        }
        
        [Button("预处理", 50)]
        private async void PreprocessingText()
        {
            EditorApplication.LockReloadAssemblies();
            // 并行处理每个 textAsset 文件
            var tasks = textAsset.Select(async text =>
            {
                string preInputString = text.text;

                // 删除注释
                preInputString = RemoveNotes(preInputString);

                preInputString = preInputString.Replace("static readonly", "readonly static");

                MatchCollection matches1 = Regex.Matches(preInputString,
                    @"@?\$@?\s*\""(?=.*\{\w+\})([^\""\\]*(?:\\.[^\""\\]*)*)\""");
                foreach (Match matche1 in matches1)
                {
                    string newMatche = Regex.Replace(matche1.Value, @"^[^""]*(?="")", "").Replace("{", "\" + ")
                        .Replace("}", " + \"");
                    preInputString = preInputString.Replace(matche1.Value, newMatche);
                }

                await File.WriteAllTextAsync(GetAssetAbsolutePath(text), preInputString);
            });

            // 等待所有并行任务完成
            await Task.WhenAll(tasks);
            EditorApplication.UnlockReloadAssemblies();
            Debug.Log("预处理已完成!");
        }
        
        [Button("提取词条", 50)]
        private void ExtractEntryFromLocation_New()
        {
            //如果汉化未删除阻止进行
            foreach (var variable in textAsset)
            {
                string text = variable.text;
                if (text.Contains("!WB_LANGUAGE_CHINESE"))
                {
                    Debug.Log("请先删除汉化!");
                    return;
                }
            }

            if (!IsFunctionFormat())
            {
                Debug.Log("函数格式有错误!");
                return;
            }

            SearchFunctionHead();
            LocationSinicization();
            ExtractEntryFromLocation();
            OnValidate();
        }

        [Button("注入汉化", 50)]
        private void InjectSinicization()
        {
            List<string> keyList = SinicizationDictionary.Keys.ToList();
            List<string> valueList = SinicizationDictionary.Values.ToList();
            foreach (var text in textAsset)
            {
                string replaceString = text.text;
                if (text.text.Contains("#if !WB_LANGUAGE_CHINESE"))
                {
                    Debug.Log("请先删除汉化!");
                    return;
                }

                if (text.text.Contains("/*<!C>*/") == false)
                    continue;

                for (var index = 0; index < keyList.Count; index++)
                {
                    replaceString = InjectSinicization_Const(replaceString, keyList[index], valueList[index],
                        useBilingual, reverseBilingual);
                }

                File.WriteAllText(GetAssetAbsolutePath(text), replaceString);
            }

            Debug.Log("注入汉化成功!");
        }
        
        static string InjectSinicization_Const(string text, string key, string value, bool useBilingual,
            bool reverseBilingualPosition)
        {
            string replaceString = text;
            string keyDeleteBackSpace = key.TrimEnd(new[] { ' ' });
            string valueDeleteBackSpace = value.TrimEnd(new[] { ' ' });
            string keyDeleteFrontBackSpace = key.Trim(new[] { ' ' });
            string valueDeleteFrontBackSpace = value.Trim(new[] { ' ' });
            string pattern0 = $@"/*<!C>*/""{key}""/*<C!>*/";
            // 替换模板，使用你指定的原文和中文翻译
            string replacement0 = !useBilingual
                ? $@"
#if !WB_LANGUAGE_CHINESE
""{key}""
#else
""{value}""
#endif
"
                : (reverseBilingualPosition
                    ? $@"
#if !WB_LANGUAGE_CHINESE
""{key}""
#else
""{valueDeleteBackSpace} {keyDeleteFrontBackSpace}""
#endif
"
                    : $@"
#if !WB_LANGUAGE_CHINESE
""{key}""
#else
""{keyDeleteBackSpace} {valueDeleteFrontBackSpace}""
#endif
");
            replaceString = replaceString.Replace(pattern0, replacement0);

            string pattern1 = $@"/*<!C>*/@""{key}""/*<C!>*/";
            // 替换模板，使用你指定的原文和中文翻译
            string replacement1 = !useBilingual
                ? $@"
#if !WB_LANGUAGE_CHINESE
@""{key}""
#else
@""{value}""
#endif
"
                : (reverseBilingualPosition
                    ? $@"
#if !WB_LANGUAGE_CHINESE
@""{key}""
#else
@""{valueDeleteBackSpace} {keyDeleteFrontBackSpace}""
#endif
"
                    : $@"
#if !WB_LANGUAGE_CHINESE
@""{key}""
#else
@""{keyDeleteBackSpace} {valueDeleteFrontBackSpace}""
#endif
");
            replaceString = replaceString.Replace(pattern1, replacement1);
            return replaceString;
        }
        
        [Button("刷新字典", 50)]
        private void Awake()
        {
            universalFunction = UniversalTranslationFunction.Instance;
            string path = GetAssetAbsolutePath(this).Replace(this.name + ".asset", "Dictionary.json");
            // if (SinicizationDictionary == null && !File.Exists(path))
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(SinicizationDictionary));
            }

            SinicizationDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));

            if (searcherList == null)
                searcherList = new List<Searcher>();
            if (searcherList.Count == 0)
            {
                foreach (var variable in _builtinSearcherList)
                {
                    searcherList.Add(variable);
                }
            }

            OnValidate();
        }
        
        [Button("添加测试文本", ButtonSizes.Large)]
        [HorizontalGroup("02")]
        private void AppendTestText()
        {
            List<string> list = SinicizationDictionary.Values.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == "")
                {
                    Random random = new Random();
                    int randomNumber = random.Next(0x0000, 0xFFFF + 1);
                    string hexValue = randomNumber.ToString("X4");
                    SinicizationDictionary[SinicizationDictionary.Keys.ToList()[i]] = "未翻译!" + hexValue;
                }
            }

            OnValidate();
        }

        [Button("移除测试文本", ButtonSizes.Large)]
        [HorizontalGroup("02")]
        private void RemoveTsetText()
        {
            List<string> list = SinicizationDictionary.Values.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains("未翻译!"))
                {
                    SinicizationDictionary[SinicizationDictionary.Keys.ToList()[i]] = "";
                }
            }

            OnValidate();
        }
        
        [Button("导出未翻译的词条", ButtonSizes.Large)]
        [HorizontalGroup("03")]
        private void ExportEmptyDictionary()
        {
            if (EmptyDictionary.Count > 0)
            {
                File.WriteAllText(GetAssetAbsolutePath(this).Replace(this.name + ".asset", "EmptyDictionary.json"),
                    JsonConvert.SerializeObject(EmptyDictionary, Formatting.Indented));
            }
        }

        [Button("导入未翻译的词条", ButtonSizes.Large)]
        [HorizontalGroup("03")]
        private void ImportEmptyDictionary()
        {
            Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText(GetAssetAbsolutePath(this).Replace(this.name + ".asset", "EmptyDictionary.json")));
            List<string> list = dictionary.Keys.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (EmptyDictionary.ContainsKey(list[i]))
                {
                    EmptyDictionary[list[i]] = dictionary.Values.ToList()[i];
                }
            }
            OnValidate();
        }
        
        [Button("清除词条", ButtonSizes.Large)]
        [HorizontalGroup("04")]
        public void ClearDictionary()
        {
            SinicizationDictionary.Clear();
            EmptyDictionary.Clear();
            broadcastCacheList.Clear();
            OnValidate();
            EditorUtility.SetDirty(this);
        }
        
        [Button("清除函数头", ButtonSizes.Large)]
        [HorizontalGroup("04")]
        public void ClearFunctionHeadList()
        {
            functionHeadList.Clear();
            OnValidate();
            EditorUtility.SetDirty(this);
        }
        
        [Button("移除汉化", ButtonSizes.Large)]
        [HorizontalGroup("04")]
        private void RemoveSinicization()
        {
            // 1. 在主线程中预先读取所有 textAsset 的文本内容和绝对路径并缓存
            var textData = textAsset.Select(text => new
            {
                TextAsset = text,
                Content = text.text,
                FilePath = GetAssetAbsolutePath(text)
            }).ToList();

            // 2. 并行处理缓存的文本数据
            Parallel.ForEach(textData, data =>
            {
                var originalContent = data.Content;
                if (!originalContent.Contains("#if !WB_LANGUAGE_CHINESE"))
                    return;
                // 调用移除翻译的函数进行文本处理
                string replaceString = RemoveSinicization(originalContent);
                File.WriteAllText(data.FilePath, replaceString);
            });
            Debug.Log("移除汉化完成!");
        }
        
        private string RemoveSinicization(string text)
        {
            string replaceString = text;
            string pattern2 =
                @"\s*#if !WB_LANGUAGE_CHINESE\s*(@?)""((?:\\.|[^\\""])*)""\s*#else\s*@?""(?:\\.|[^\\""])*""\s*#endif\s*";
            List<Match> matches = RemoveRepeatElement(Regex.Matches(replaceString, pattern2, RegexOptions.None));
            foreach (var match in matches)
            {
                replaceString = replaceString.Replace(match.Value,
                    $" /*<!C>*/{match.Groups[1].Value}\"{match.Groups[2].Value}\"/*<C!>*/");
            }

            return replaceString;
        }
        
        [Button("移除定位", ButtonSizes.Large)]
        [HorizontalGroup("04")]
        public void ClearSinicizationLocation()
        {
            foreach (var text in textAsset)
            {
                string replaceString = text.text;
                replaceString = replaceString.Replace("/*<!C>*/", "").Replace("/*<C!>*/", "");
                File.WriteAllText(GetAssetAbsolutePath(text), replaceString);
            }

            Debug.Log("移除定位成功!");
        }
        
        /// <summary>
        /// 删除输入代码中的注释
        /// </summary>
        public static string RemoveNotes(string code)
        {
            string codeRemoveString = RemoveDoubleQuotationMarks(code);

            // 正则表达式匹配单行和多行注释
            string pattern = @"(//.*?$)|(/\*[\s\S]*?\*/)";

            // 使用正则表达式替换掉注释
            List<Match> matches = RemoveRepeatElement(Regex.Matches(codeRemoveString, pattern, RegexOptions.Multiline));
            foreach (var match in matches)
            {
                code = code.Replace(match.Value, "");
            }

            return code;
        }
        
        /// <summary>
        /// 搜索函数头
        /// </summary>
        private void SearchFunctionHead()
        {
            functionHeadList.Clear();

            var searchFunctionList = GetSearchFunctionList();

            foreach (var text in textAsset)
            {
                string inputString = text.text;
                if (inputString.Contains("!WB_LANGUAGE_CHINESE") || inputString.Contains("Language.WB_Translation"))
                {
                    Debug.Log("请先删除汉化!");
                    return;
                }

                foreach (var funcString in searchFunctionList)
                {
                    string funcStringRegexFixup = funcString.Remove(funcString.Length - 2, 2);

                    if (!inputString.Contains(funcStringRegexFixup))
                    {
                        continue;
                    }

                    string pattern = funcStringRegexFixup.Replace(".", @"\.") + BracketsMatch;

                    MatchCollection matches = Regex.Matches(inputString, pattern);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            string funcInsideString = match.Value;
                            if (!functionHeadList.Contains(funcInsideString))
                            {
                                functionHeadList.Add(funcInsideString.Replace("/*<!C>*/", "").Replace("/*<C!>*/", ""));
                            }
                        }
                    }
                }
            }

            Debug.Log("搜索函数头完毕!");
        }

        /// <summary>
        /// 获取搜索函数头列表
        /// </summary>
        private List<string> GetSearchFunctionList()
        {
            List<Searcher> functionOrAttributeSearcher = searcherList
                .FindAll(o => o.searchMode == Searcher.SearchMode.FunctionOrAttributeSearch);
            if (functionOrAttributeSearcher.Count == 0)
                return new List<string>();

            List<string> funcSearchList = new List<string>();
            foreach (var variable in functionOrAttributeSearcher)
            {
                funcSearchList.AddRange(variable.searchFunctionOrAttributeAPI);
            }

            return funcSearchList;
        }
        
        /// <summary>
        /// 定位汉化
        /// </summary>
        private void LocationSinicization()
        {
            List<string> excludeFuncAPI = GetExcludeFunctionList();

            var textAssetData = textAsset.Select(variable => new
            {
                Text = variable.text,
                Path = GetAssetAbsolutePath(variable)
            }).ToList();

            Parallel.ForEach(textAssetData, data =>
            {
                // 合并替换
                string text = Regex.Replace(data.Text, @"(/\*<!C>\*/|/\*<C!>\*/)", "");

                // 处理定位
                foreach (var searcher in searcherList)
                {
                    text = LocationExecute(text, searcher);
                }

                // 批量写入
                File.WriteAllText(data.Path, text);
            });

            // 广播定位
            List<string> broadcastListFinal = broadcastCacheList;
            foreach (var variable in broadcastList)
            {
                if (!broadcastListFinal.Contains(variable))
                {
                    broadcastListFinal.Add(variable);
                }
            }

            LocationBroadcast(broadcastListFinal);

            // 排除处理
            LocationClear(excludeFuncAPI, excludeList);

            Debug.Log("定位汉化完成!");
        }
        
        /// <summary>
        /// 获取排除函数列表
        /// </summary>
        private List<string> GetExcludeFunctionList()
        {
            List<Searcher> functionOrAttributeSearcher = searcherList
                .FindAll(o => o.searchMode == Searcher.SearchMode.FunctionOrAttributeSearch);
            if (functionOrAttributeSearcher.Count == 0)
                return new List<string>();

            List<string> funcExcludeList = new List<string>();
            foreach (var variable in functionOrAttributeSearcher)
            {
                funcExcludeList.AddRange(variable.excludeFunctionOrAttributeAPI);
            }

            return funcExcludeList;
        }

        /// <summary>
        /// 执行汉化定位
        /// </summary>
        private string LocationExecute(string text, string pattern, RegexOptions regexOp,
            bool isFunctionInside, int nameIndex, Searcher.SearchMode searchMode,
            List<string> searchFunctionOrAttributeAPI,
            List<string> variableNameList, bool isBroadcast)
        {
            //前置
            if (isFunctionInside && functionHeadList.Count == 0)
                return text;

            string replaceString = text;


            //函数或特性搜索
            if (searchMode == Searcher.SearchMode.FunctionOrAttributeSearch)
            {
                foreach (string funcString in searchFunctionOrAttributeAPI)
                {
                    string fixupFuncString = funcString.Remove(funcString.Length - 2, 2);
                    string patternFinal = fixupFuncString.Replace(".", @"\.") + BracketsMatch;
                    if (text.Contains(fixupFuncString))
                    {
                        List<Match> matches = RemoveRepeatElement(Regex.Matches(text, patternFinal, regexOp));
                        foreach (Match match in matches)
                        {
                            string contentString = match.Value;
                            string newFuncInsideString =
                                LocationDoubleQuotationMarks(contentString, "/*<!C>*/", "/*<C!>*/", isBroadcast);
                            replaceString = replaceString.Replace(contentString, newFuncInsideString);
                        }
                    }
                }
            }


            //自定义搜索
            if (searchMode == Searcher.SearchMode.Custom)
            {
                List<Match> matches = RemoveRepeatElement(Regex.Matches(text, pattern, regexOp));
                foreach (Match match in matches)
                {
                    if (isFunctionInside)
                    {
                        // Debug.Log(match.Value);
                        string variableName = match.Groups[nameIndex].Value;
                        // string contentString = match.Groups[contentIndex].Value;
                        // if (!IsEntryEffective(contentString)) continue;
                        if (IsAtFuncListInside(variableName))
                        {
                            string newContentString =
                                LocationDoubleQuotationMarks(match.Value, "/*<!C>*/", "/*<C!>*/", isBroadcast);
                            replaceString = replaceString.Replace(match.Value, newContentString);
                        }
                    }
                    else
                    {
                        string contentString = match.Value;
                        string newFuncInsideString =
                            LocationDoubleQuotationMarks(contentString, "/*<!C>*/", "/*<C!>*/", isBroadcast);
                        replaceString = replaceString.Replace(contentString, newFuncInsideString);
                    }
                }
            }


            //数组列表字典
            if (searchMode == Searcher.SearchMode.ArrayListOrDictionary)
            {
                foreach (string variableName in variableNameList)
                {
                    string patternFinal = $@"\b{variableName}" + @"\s*=\s*new\s*([^{]+)(\{(?:[^{}]*|\{[^{}]*\})*\})";
                    if (text.Contains(variableName))
                    {
                        List<Match> matches = RemoveRepeatElement(Regex.Matches(text, patternFinal, regexOp));
                        foreach (Match match in matches)
                        {
                            string contentString = match.Value;
                            string newContentString =
                                LocationDoubleQuotationMarks(contentString, "/*<!C>*/", "/*<C!>*/", isBroadcast);
                            replaceString = replaceString.Replace(contentString, newContentString);
                        }
                    }
                }
            }

            replaceString = replaceString.Replace("@/*<!C>*/", "/*<!C>*/@");

            return replaceString;
        }
        
        /// <summary>
        /// 执行汉化定位
        /// </summary>
        private string LocationExecute(string text, Searcher searcher)
        {
            string replaceText = text;
            replaceText = LocationExecute(replaceText,
                searcher.customRegex,
                searcher.regexOp, searcher.isFunctionInside, searcher.nameIndex, searcher.searchMode,
                searcher.searchFunctionOrAttributeAPI,
                searcher.variableNameList, searcher.isBroadcast);
            return replaceText;
        }

        /// <summary>
        /// 定位双引号字符串
        /// </summary>
        private string LocationDoubleQuotationMarks(string inputString, string leftLocation, string rightLocation,
            bool isBroadcast)
        {
            string replaceString = inputString;

            List<Match> matches1 = RemoveRepeatElement(Regex.Matches(inputString, DoubleQuotationMarksMatch));
            if (matches1.Count > 0)
            {
                foreach (Match matche1 in matches1)
                {
                    string entry = matche1.Value.Substring(1, matche1.Length - 2);
                    if (!IsEntryEffective(entry)) continue;
                    if (!replaceString.Contains(leftLocation + $"\"{entry}\"" + rightLocation) &&
                        !replaceString.Contains(leftLocation + "@" + $"\"{entry}\"" + rightLocation))
                    {
                        if (isBroadcast)
                        {
                            if (!broadcastCacheList.Contains(entry))
                            {
                                broadcastCacheList.Add(entry);
                            }
                        }
                        else
                        {
                            replaceString = replaceString.Replace(matche1.Value,
                                leftLocation + $"\"{entry}\"" + rightLocation);
                        }
                    }
                }
            }

            return replaceString;
        }

        /// <summary>
        /// 清除定位
        /// </summary>
        private void LocationClear(List<string> executeExcludeFuncAPI, List<string> excludeListArgs)
        {
            // 在主线程中缓存文件路径
            var textAssetData = textAsset.Select(text => new
            {
                Text = text.text,
                Path = GetAssetAbsolutePath(text)
            }).ToList();

            Parallel.ForEach(textAssetData, data =>
            {
                string replaceString = data.Text;
                foreach (string funcString in executeExcludeFuncAPI)
                {
                    if (!data.Text.Contains(funcString.Remove(funcString.Length - 2, 2)))
                    {
                        continue;
                    }

                    //定位函数内字符串
                    string funcStringRegexFixup = funcString.Remove(funcString.Length - 2, 2).Replace(".", @"\.");
                    string pattern = funcStringRegexFixup + BracketsMatch;
                    List<Match> matches = RemoveRepeatElement(Regex.Matches(data.Text, pattern, RegexOptions.None));
                    foreach (Match match in matches)
                    {
                        replaceString = replaceString.Replace(match.Value,
                            match.Value.Replace("/*<!C>*/", "").Replace("/*<C!>*/", ""));
                    }
                }

                foreach (var variable in excludeListArgs)
                {
                    replaceString = replaceString.Replace($"/*<!C>*/\"{variable}\"/*<C!>*/", $"\"{variable}\"");
                    replaceString = replaceString.Replace($"/*<!C>*/@\"{variable}\"/*<C!>*/", $"@\"{variable}\"");
                }

                File.WriteAllText(data.Path, replaceString);
            });
        }
        
        /// <summary>
        /// 广播定位
        /// </summary>
        private void LocationBroadcast(List<string> broadcastListArgs)
        {
            if (broadcastListArgs == null || broadcastListArgs.Count == 0)
                return;

            // 在主线程中缓存文件路径
            var textAssetData = textAsset.Select(text => new
            {
                Text = text.text,
                Path = GetAssetAbsolutePath(text)
            }).ToList();

            Parallel.ForEach(textAssetData, data =>
            {
                string replaceString = data.Text;

                foreach (var variable in broadcastListArgs)
                {
                    replaceString = replaceString.Replace($"\"{variable}\"", $"/*<!C>*/\"{variable}\"/*<C!>*/");
                    replaceString = replaceString.Replace($"@\"{variable}\"", $"/*<!C>*/@\"{variable}\"/*<C!>*/");
                }

                File.WriteAllText(data.Path, replaceString);
            });
        }
        
        /// <summary>
        /// 从定位提取词条
        /// </summary>
        private void ExtractEntryFromLocation()
        {
            foreach (var txet in textAsset)
            {
                string pattern0 = @"/\*<!C>\*/\s*@?""(.*?)""\s*/\*<C!>\*/";
                MatchCollection matches0 = Regex.Matches(txet.text, pattern0);
                foreach (Match match in matches0)
                {
                    string entry = match.Value.Replace("/*<!C>*/\"", "").Replace("/*<!C>*/@\"", "")
                        .Replace("\"/*<C!>*/", "");
                    SinicizationDictionary.TryAdd(entry, "");
                }
            }

            // OnValidate();
            Debug.Log("提取词条完成!");
        }
        
        /// <summary>
        /// 判断函数列表格式是否正确
        /// </summary>
        private bool IsFunctionFormat()
        {
            List<string> ExecuteSearchFuncAPI = searcherList
                .Find(o => o.searchMode == Searcher.SearchMode.FunctionOrAttributeSearch).searchFunctionOrAttributeAPI;
            foreach (var funcString in ExecuteSearchFuncAPI)
            {
                if (!(funcString.Substring(funcString.Length - 2) == "()" && funcString != ""))
                {
                    Debug.Log("函数字符串格式不正确!或有空元素!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 移除重复元素
        /// </summary>
        private static List<Match> RemoveRepeatElement(MatchCollection matches)
        {
            if (matches.Count == 0)
                return new List<Match>();
            List<Match> matchHash = new List<Match>();
            foreach (Match match in matches)
            {
                matchHash.Add(match);
            }

            List<Match> distinctMatches = matchHash
                .GroupBy(m => m.Value) // 根据 Value 属性分组
                .Select(g => g.First()) // 每组中取第一个元素
                .ToList();

            return distinctMatches;
        }

        /// <summary>
        /// 判断变量名是否在函数内部
        /// </summary>
        private bool IsAtFuncListInside(string variableName)
        {
            foreach (string variable in functionHeadList)
            {
                string variableModif = RemoveDoubleQuotationMarks(variable);
                if (variableModif.Contains(variableName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 移除双引号内的内容
        /// </summary>
        static string RemoveDoubleQuotationMarks(string input)
        {
            string replaceString = input;
            // 使用正则表达式匹配
            MatchCollection matches = Regex.Matches(input, DoubleQuotationMarksMatch);
            foreach (Match match in matches)
            {
                replaceString = replaceString.Replace(match.Value, "");
            }

            return replaceString;
        }
        
        /// <summary>
        /// 判断词条是否有效
        /// </summary>
        private bool IsEntryEffective(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            // 使用 Regex.Replace 替换匹配项
            string cleanedInput = Regex.Replace(input, @"\\u[0-9a-fA-F]{4}", "");
            // 检查去除后的字符串是否为空
            if (string.IsNullOrEmpty(cleanedInput))
            {
                return false;
            }
            // 使用正则表达式去除所有特殊字符、数字和空白字符
            cleanedInput = Regex.Replace(cleanedInput, @"[\t\r\n\\!@#$%^&*()_+\-=|{}[\];:'""/?.>,<`trnfabv~0-9A-Z\s]",
                "");
            // 检查去除后的字符串是否为空
            if (string.IsNullOrEmpty(cleanedInput))
            {
                return false;
            }
            // 检查是否为GUID
            if (Guid.TryParse(input, out _))
            {
                return false;
            }
            // 检查是否为文件后缀名
            if (Regex.IsMatch(input, @"^\.[a-zA-Z]+$"))
            {
                return false;
            }
            // 只有当所有条件都未触发时，才返回 true
            return true;
        }
        
        /// <summary>
        /// 移除列表中的重复元素
        /// </summary>
        public static void RemoveFuncListRepeatElement(List<string> funcAPI)
        {
            for (int i = 0; i < funcAPI.Count; i++)
            {
                for (int j = 0; j < funcAPI.Count; j++)
                {
                    if (i == j) continue;
                    if (funcAPI[i] == funcAPI[j])
                    {
                        Debug.Log("列表中已有元素: \"" + funcAPI[j] + "\"!");
                        funcAPI.RemoveAt(j);
                    }
                }
            }
        }

        /// <summary>
        /// 判断汉化注入器是否在WbLanguage文件内部
        /// </summary>
        private bool IsAtWbLanguageInside(string path)
        {
            int lastSlashIndex = path.LastIndexOf('/');
            int secondLastSlashIndex = path.LastIndexOf('/', lastSlashIndex - 1);
            string secondLastPart = path.Substring(secondLastSlashIndex + 1, lastSlashIndex - secondLastSlashIndex - 1);
            bool isWB_Language = secondLastPart == "WB_Language";
            return isWB_Language;
        }

        /// <summary>
        /// 收集.cs文件
        /// </summary>
        public void CollectCsFiles()
        {
            if (folder == null)
            {
                Debug.LogError("请在检查器中分配一个文件夹。");
                return;
            }
            // 获取文件夹路径
            string folderPath = AssetDatabase.GetAssetPath(folder);
            // 清空现有列表
            textAsset.Clear();
            // 递归遍历文件夹
            FindCsFilesInDirectory(folderPath);
            // 打印结果
            Debug.Log("收集 " + textAsset.Count + " 个文件!");
        }

        /// <summary>
        /// 输入路径递归查找子文件夹内的所有.cs文件
        /// </summary>
        private void FindCsFilesInDirectory(string path)
        {
            // 获取目录中的所有文件
            string[] files = Directory.GetFiles(path);
            // 查找 .cs 文件并加载为 TextAsset
            foreach (string file in files)
            {
                if (file.EndsWith(".cs"))
                {
                    // 获取相对路径以加载 TextAsset
                    string relativePath = file.Replace(Application.dataPath, "").Replace("\\", "/");
                    TextAsset csFile = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);

                    // 如果成功加载为 TextAsset，则添加到列表中
                    if (csFile != null)
                    {
                        textAsset.Add(csFile);
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

        /// <summary>
        /// 获取输入资产对象的绝对路径
        /// </summary>
        private string GetAssetAbsolutePath(Object Obj)
        {
            if (Obj == null)
            {
                return null;
            }
            else
            {
                return Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(Obj);
            }
        }
        
        /// <summary>
        /// 添加 WB_LANGUAGE_CHINESE 脚本定义符
        /// </summary>
        public static void AddDefineSymbol()
        {
            string symbol = "WB_LANGUAGE_CHINESE";
            // 获取当前平台的构建目标组 (如: Standalone, iOS, Android 等)
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            // 获取当前的脚本定义符列表
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            // 检查是否已包含该符号
            if (!defines.Contains(symbol))
            {
                // 如果没有，则添加新的符号
                defines += ";" + symbol;
                // 更新定义符
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
                Debug.Log($"已添加定义符: {symbol}");
            }
            else
            {
                Debug.Log($"定义符已存在: {symbol}");
            }
        }

        /// <summary>
        /// 移除 WB_LANGUAGE_CHINESE 脚本定义符
        /// </summary>
        public static void RemoveDefineSymbol()
        {
            string symbol = "WB_LANGUAGE_CHINESE";
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // 如果存在定义符，则将其删除
            if (defines.Contains(symbol))
            {
                // 删除符号并更新列表
                defines = defines.Replace(symbol, "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
                Debug.Log($"已删除定义符: {symbol}");
            }
            else
            {
                Debug.Log($"定义符不存在: {symbol}");
            }
        }
        
        
    }
}
#endif
