using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace WorldSystem.Runtime
{
    
    [CreateAssetMenu(fileName = "天气列表", menuName = "世界系统/天气列表")]
    [Serializable]
    public class WeatherList : ScriptableObject
    {
        
#if UNITY_EDITOR
        [FormerlySerializedAs("SelectedWeatherDefine")] [InlineEditor][LabelText("选中天气")]
        public WeatherDefine selectedWeatherDefine;
#endif
        [FormerlySerializedAs("list")]
        [InlineEditor] [LabelText("天气列表")]
        [ListDrawerSettings(HideAddButton = false,CustomAddFunction = "AddWeatherDefine", CustomRemoveElementFunction = "RemoveWeatherDefine",OnTitleBarGUI = "DrawRefreshButton")]
        public List<WeatherDefine> weatherList;
        
#if UNITY_EDITOR
        
        private void AddWeatherDefine()
        {
            if (selectedWeatherDefine == null)
            {
                Debug.Log("请设置选择天气!将会把选择天气的拷贝添加到天气列表!");
                return;
            }
            WeatherDefine weatherDefineCopy = Instantiate(selectedWeatherDefine);
            weatherDefineCopy.name = "zCache_" + weatherList.Count + "_" + GetRandom16BitNumber() + "_" + selectedWeatherDefine.name;
            AssetDatabase.CreateAsset(weatherDefineCopy, 
                Regex.Replace(AssetDatabase.GetAssetPath(this),@"/([^/]+?\.asset)$", "/")
                                                         + weatherDefineCopy.name + ".asset");
            weatherList.Add(weatherDefineCopy);
        }

        private void RemoveWeatherDefine(WeatherDefine index)
        {
            weatherList.Remove(index);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(index));
        }

        private void DrawRefreshButton()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                ResetWeatherListTime();
            }
        }
        
        private void OnValidate()
        {
            foreach (var VARIABLE in weatherList)
            {
                string nameNew = Regex.Replace(VARIABLE.name, @"zCache_(\d+)", "zCache_" + weatherList.IndexOf(VARIABLE));
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(VARIABLE), nameNew);
            }
        }
        
        public void SetupPropertyFromActive()
        {
            weatherList?.Find(o => o.IsActive)?.SetupProperty();
            if(selectedWeatherDefine?.IsActive ?? false) selectedWeatherDefine.SetupProperty();
        }
        
        public string GetRandom16BitNumber()  
        {  
            // 假设我们想要一个4位的十六进制数（即一个介于0x0000和0xFFFF之间的数）  
            int numHexDigits = 4;  
          
            // 创建一个Random实例  
            Random rand = new Random();  
          
            // 计算随机数的最大值（16的numHexDigits次方 - 1）  
            int maxValue = (int)Math.Pow(16, numHexDigits) - 1;  
          
            // 生成一个随机整数  
            int randomInt = rand.Next(maxValue + 1); // 包含maxValue  
          
            // 将随机整数转换为十六进制字符串  
            string hexNumber = randomInt.ToString("X" + numHexDigits).ToUpper(); // 使用ToUpper()转换为大写  
            return hexNumber;
        } 
        
#endif
        
        public void ResetWeatherListTime()
        {
            foreach (var VARIABLE in weatherList)
            {
                VARIABLE.sustainedTime = VARIABLE.sustainedTimeCache;
                VARIABLE.varyingTime = VARIABLE.varyingTimeCache;
            }
        }
        
    }
}