using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class WeatherSystemModule : BaseModule
    {
        
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)][Title("$info")][HideLabel]
        // [LabelText("天气列表资产")]
        public WeatherList weatherList;
        
#if UNITY_EDITOR
        private string info;
#endif


        #region 事件函数
        
        private float previousTime;
        [HideInInspector]
        public int i = 0;
        
        
        
        private int _frameID;
        private int _updateCount;
#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying) return;
            UpdateFunc();
        }
        private void FixedUpdate()
        {
            if (Time.frameCount == _frameID) return;
            
            //分帧器,将不同的操作分散到不同的帧,提高帧率稳定性
            if (_updateCount % 2 == 0)
            {
                UpdateFunc();
            }
            _updateCount++;
            
            _frameID = Time.frameCount;
        }
#else
        private void FixedUpdate()
        {
            if (Time.frameCount == _frameID) return;
            
            //分帧器,将不同的操作分散到不同的帧,提高帧率稳定性
            if (_updateCount % 2 == 0)
            {
                UpdateFunc();
            }
            _updateCount++;
            
            _frameID = Time.frameCount;
        }
#endif
        private void UpdateFunc()
        {

            if (WorldManager.Instance?.timeModule is null || weatherList?.list is null || weatherList?.list?.Count == 0
#if UNITY_EDITOR
                || (weatherList?.weatherDefineNew?.IsActive ?? false)
#endif
                ) return;
            
            //计算增量时间(小时为单位)
            float DeltaTime = previousTime == 0 ? 0 : WorldManager.Instance.timeModule.initTime.Hour - previousTime;

            
            //按列表循环天气
            //避免某些情况下索引越界
            i %= weatherList.list.Count;

            if (DeltaTime < 0) //时间后退时
            {
                weatherList.list[i].sustainedTime = weatherList.list[i].sustainedTimeCache;
                weatherList.list[i].varyingTime = weatherList.list[i].varyingTimeCache;
            }
            else //时间前进时
            {
                //在持续时间之内
                if ((weatherList.list[i].sustainedTime -= DeltaTime) > 0)
                {
                    //如果为激活则激活
                    if (weatherList.list[i].IsActive == false) weatherList.list[i].IsActive = true;
                }
                //经过持续时间,进入切换时间需要在下一个天气状态之间插值
                else
                {
                    ////修正溢出
                    weatherList.list[i].varyingTime += weatherList.list[i].sustainedTime;
                    weatherList.list[i].sustainedTime = 0;

                    //在变换时间之内
                    if ((weatherList.list[i].varyingTime -= DeltaTime) > 0)
                    {
                        //下一个天气状态之间插值
                        weatherList.list[i].SetupLerpProperty(weatherList.list[(i + 1) % weatherList.list.Count],
                            math.remap(weatherList.list[i].varyingTimeCache, 0, 0, 1,
                                weatherList.list[i].varyingTime));
                    }
                    //经过变换时间退出当前天气进入下一个天气
                    else
                    {
                        //修正溢出
                        weatherList.list[(i + 1) % weatherList.list.Count].sustainedTime +=
                            weatherList.list[i].varyingTime;
                        //进入下一个天气时将上一个天气的时间恢复
                        weatherList.list[i].sustainedTime = weatherList.list[i].sustainedTimeCache;
                        weatherList.list[i].varyingTime = weatherList.list[i].varyingTimeCache;
                        //索引前进,将在下一帧激活下一个天气
                        i = (i + 1) % weatherList.list.Count;
                    }
                }
            }

        
#if UNITY_EDITOR
            float totalTime = 0;
            foreach (var VARIABLE in weatherList.list)
            {
                totalTime += VARIABLE.sustainedTimeCache + VARIABLE.varyingTimeCache;
            }
            
            // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
            if(weatherList != null && weatherList.list.Count != 0)
                info = "激活的索引: " + i + "    激活的天气: " + weatherList.list[i].name
                    + "    当前天气列表的总时间: " + math.trunc(totalTime/24) + "天/" + WorldManager.Instance.timeModule.HoursToTimeString(totalTime);
#endif
            
            previousTime = WorldManager.Instance.timeModule.initTime.Hour;
        }

        
        
        #endregion

        
    } 
}