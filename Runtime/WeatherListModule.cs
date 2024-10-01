using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class WeatherListModule : BaseModule
    {
        
        
        #region 字段
        
#if UNITY_EDITOR
        [FormerlySerializedAs("RefreshSerializeData")] [ToggleLeft][LabelText(" \u261a 打包之前请点击此处刷新序列化数据, 否则部分参数可能不生效")]
        public bool refreshSerializeData;
#endif
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)][Title("$_info")][HideLabel]
        public WeatherList weatherList;
        
#if UNITY_EDITOR
        private string _info;
#endif
        
        private float _previousTime;
        
        [FormerlySerializedAs("i")] [HideInInspector]
        public int weatherListIndex;
        
        [FormerlySerializedAs("_Update")] [HideInInspector] public bool update;
        
        #endregion

        
        #region 事件函数
        
        private void Update()
        {
            if (!update) return;
            
#if UNITY_EDITOR
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            WeatherDefine.UpdateDynamicDisplayProperty();
#endif
            
            if (WorldManager.Instance?.timeModule is null || weatherList?.weatherList is null || weatherList?.weatherList?.Count == 0
#if UNITY_EDITOR
                || (weatherList?.selectedWeatherDefine?.IsActive ?? false)
#endif
                ) return;
            
            //计算增量时间(小时为单位)
            float DeltaTime = _previousTime == 0 ? 0 : WorldManager.Instance.timeModule.initTime.hour - _previousTime;
            
            //按列表循环天气
            //避免某些情况下索引越界
            weatherListIndex %= weatherList.weatherList.Count;

            if (DeltaTime < 0) //时间后退时
            {
                weatherList.weatherList[weatherListIndex].sustainedTime = weatherList.weatherList[weatherListIndex].sustainedTimeCache;
                weatherList.weatherList[weatherListIndex].varyingTime = weatherList.weatherList[weatherListIndex].varyingTimeCache;
            }
            else //时间前进时
            {
                //在持续时间之内
                if ((weatherList.weatherList[weatherListIndex].sustainedTime -= DeltaTime) > 0)
                {
                    //如果为激活则激活
                    if (weatherList.weatherList[weatherListIndex].IsActive == false) weatherList.weatherList[weatherListIndex].IsActive = true;
                }
                //经过持续时间,进入切换时间需要在下一个天气状态之间插值
                else
                {
                    ////修正溢出
                    weatherList.weatherList[weatherListIndex].varyingTime += weatherList.weatherList[weatherListIndex].sustainedTime;
                    weatherList.weatherList[weatherListIndex].sustainedTime = 0;

                    //在变换时间之内
                    if ((weatherList.weatherList[weatherListIndex].varyingTime -= DeltaTime) > 0)
                    {
                        //下一个天气状态之间插值
                        weatherList.weatherList[weatherListIndex].SetupLerpProperty(weatherList.weatherList[(weatherListIndex + 1) % weatherList.weatherList.Count],
                            math.remap(weatherList.weatherList[weatherListIndex].varyingTimeCache, 0, 0, 1,
                                weatherList.weatherList[weatherListIndex].varyingTime));
                    }
                    //经过变换时间退出当前天气进入下一个天气
                    else
                    {
                        //修正溢出
                        weatherList.weatherList[(weatherListIndex + 1) % weatherList.weatherList.Count].sustainedTime +=
                            weatherList.weatherList[weatherListIndex].varyingTime;
                        //进入下一个天气时将上一个天气的时间恢复
                        weatherList.weatherList[weatherListIndex].sustainedTime = weatherList.weatherList[weatherListIndex].sustainedTimeCache;
                        weatherList.weatherList[weatherListIndex].varyingTime = weatherList.weatherList[weatherListIndex].varyingTimeCache;
                        //索引前进,将在下一帧激活下一个天气
                        weatherListIndex = (weatherListIndex + 1) % weatherList.weatherList.Count;
                    }
                }
            }
            
#if UNITY_EDITOR
            float totalTime = 0;
            foreach (var VARIABLE in weatherList.weatherList)
            {
                totalTime += VARIABLE.sustainedTimeCache + VARIABLE.varyingTimeCache;
            }
            
            // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
            if(weatherList != null && weatherList.weatherList.Count != 0)
                _info = "激活的索引: " + weatherListIndex + "    激活的天气: " + weatherList.weatherList[weatherListIndex].name
                    + "    当前天气列表的总时间: " + math.trunc(totalTime/24) + "天/" + WorldManager.Instance.timeModule.HoursToTimeString(totalTime);
#endif
            _previousTime = WorldManager.Instance.timeModule.initTime.hour;
            
        }
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (weatherList == null)
                weatherList =
                    AssetDatabase.LoadAssetAtPath<WeatherList>(
                        "Packages/com.worldsystem/Preset/WeatherList/PresetWeatherList.asset");
#endif
            if (weatherList != null)
                weatherList.ResetWeatherListTime();
        }
        
        private void OnDestroy()
        {
            if (weatherList != null)
            {
                weatherList.ResetWeatherListTime();
                Resources.UnloadAsset(weatherList);
            }
            weatherList = null;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (refreshSerializeData)
            {
                refreshSerializeData = false;
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.Log("已刷新序列化数据");
            }
        }
#endif
        
        #endregion

        
    } 
}