#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace WorldSystem.Language
{
    [Serializable]
// [CreateAssetMenu(fileName = "UniversalTranslationFunction.asset", menuName = "世界系统/通用翻译函数库")]
    public class UniversalTranslationFunction : ScriptableObject
    {
        private static UniversalTranslationFunction _instance;
        
        private static readonly object LockObj = new();
        
        private static bool _isShuttingDown;

        public static UniversalTranslationFunction Instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(UniversalTranslationFunction) +
                                     "' 已经被摧毁了。返回null。");
                    return null;
                }

                lock (LockObj)
                {
                    if (_instance == null)
                    {
                        _instance = AssetDatabase.LoadAssetAtPath<UniversalTranslationFunction>("Packages/com.worldsystem/Language/UniversalTranslationFunction.asset");
                    }
                    return _instance;
                }
            }
        }
        private void OnDestroy()
        {
            _isShuttingDown = true;
        }
    
        [FormerlySerializedAs("UniversalSearchFuncAPI")] [LabelText("通用正向搜索")][HorizontalGroup("01")]
        public List<string> universalSearchFuncAPI = new List<string>();
        [FormerlySerializedAs("UniversalExcludeFuncAPI")] [LabelText("通用负向排除")][HorizontalGroup("01")]
        public List<string> universalExcludeFuncAPI = new List<string>();
        
        private void OnValidate()
        {
            Language.RemoveFuncListRepeatElement(universalSearchFuncAPI);
            Language.RemoveFuncListRepeatElement(universalExcludeFuncAPI);
            Language[] languages = Resources.FindObjectsOfTypeAll<Language>();
            foreach (var variable in languages)
            {
                variable.OnValidate();
                EditorUtility.SetDirty(variable);
            }
        }
        
    }
}

#endif