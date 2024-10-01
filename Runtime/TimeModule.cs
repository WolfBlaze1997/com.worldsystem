using System;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace WorldSystem.Runtime
{
    
    public partial class TimeModule
    {
        [Serializable]
        public struct Date
        {
            [FormerlySerializedAs("Hour")] [HorizontalGroup] [LabelText("时")] [GUIColor(0.5f, 1.0f, 0.5f)] [Unit(Units.Hour)]
            public float hour;

            [FormerlySerializedAs("Year")] [HorizontalGroup] [LabelText("年")] 
            public int year;

            [FormerlySerializedAs("Month")] [HorizontalGroup] [LabelText("月")] 
            public int month;

            [FormerlySerializedAs("Day")] [HorizontalGroup] [LabelText("日")]
            public int day;
        }
        
        private string ConvertDecimalHoursToDateTimeString(double decimalHours)
        {
            // 假设小数部分的前两位是分钟，剩下的部分是秒（需要四舍五入到最接近的秒）  
            int hours = (int)decimalHours;
            double minutesAndSeconds = decimalHours - hours;
            int minutes = (int)(minutesAndSeconds * 60);
            double seconds = (minutesAndSeconds * 60) % 60; // 取出秒的部分  

            // 四舍五入到最接近的秒  
            int roundedSeconds = (int)Math.Round(seconds);

            // 起始日期设为 Unix 时间戳的起始日期：1970年1月1日  
            DateTime startDate = new DateTime(initTime.year, initTime.month, initTime.day);

            // 添加小时、分钟和秒到起始日期  
            DateTime dateTime = startDate.AddHours(hours).AddMinutes(minutes).AddSeconds(roundedSeconds);

            // 格式化日期时间为字符串  
            string formattedDateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            return formattedDateTime;
        }

        public string HoursToTimeString(double decimalHours)
        {
            // 假设小数部分的前两位是分钟，剩下的部分是秒（需要四舍五入到最接近的秒）  
            int hours = (int)decimalHours;
            double minutesAndSeconds = decimalHours - hours;
            int minutes = (int)(minutesAndSeconds * 60);
            double seconds = (minutesAndSeconds * 60) % 60; // 取出秒的部分  

            // 四舍五入到最接近的秒  
            int roundedSeconds = (int)Math.Round(seconds);

            // 起始日期设为 Unix 时间戳的起始日期：1970年1月1日  
            DateTime startDate = new DateTime(1, 1, 1);

            // 添加小时、分钟和秒到起始日期  
            DateTime dateTime = startDate.AddHours(hours).AddMinutes(minutes).AddSeconds(roundedSeconds);

            // 格式化日期时间为字符串  
            string formattedDateTime = dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            return formattedDateTime;
        }
        
    }
    
    [ExecuteAlways]
    public partial class TimeModule : BaseModule
    {
        
        #region 字段
        
        [InlineProperty(LabelWidth = 14)] [LabelText("初始时间")]
        public Date initTime = new Date()
        {
            year = 1997, month = 1, day = 1, hour = 10
        };

        [ReadOnly] [LabelText("游戏时间")]
        public string timeString;

        [LabelText("使用昼夜循环")]
        public bool useDayNightCycle;
        
        //分钟为单位
        [LabelText("    昼夜循环(分钟)")] [Unit(Units.Minute)] 
        public float dayNightCycleDurationMinute = 20;
        
        [LabelText("    打包后开启昼夜循环")]
        public bool packUseDayNightCycle = true;
        
        public float CurrentTime => initTime.hour % 24;

        [ShowInInspector]
        [ProgressBar(0f, 1f, ColorGetter = "gray")]
        [HideLabel] [EnableIf("useDayNightCycle")]
        public float CurrentTime01 => CurrentTime / 24f;

        /// <summary>
        /// 返回当前日间系数。1=白天，0=夜晚。
        /// </summary>
        public float DaytimeFactor => 1.0f - Mathf.Abs((CurrentTime01 - 0.5f) * 2.0f);

        private float _managedTime;
        
        private int _frameCount;
        
        [FormerlySerializedAs("_Update")] [HideInInspector] 
        public bool update;
        
        private float _previousTime;
        
        [HideInInspector]
        public float deltaTime;
        
        #endregion
        
        
        #region 限制/安装属性

        private void LimitProperty()
        {
            dayNightCycleDurationMinute = Math.Max(dayNightCycleDurationMinute, 0);
            initTime.hour = Math.Max(initTime.hour, 0);
            initTime.year = Math.Max(initTime.year, 1);
            initTime.month = Math.Max(initTime.month, 1);
            initTime.day = Math.Max(initTime.day, 1);
        }
        
        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_EarthTime, CurrentTime);
            Shader.SetGlobalFloat(_FrameId, _frameCount % 4);
        }
        private readonly int _EarthTime = Shader.PropertyToID("_EarthTime");
        private readonly int _FrameId = Shader.PropertyToID("_FrameId");

        #endregion
        
        
        #region 事件函数
#if !UNITY_EDITOR
        private void OnEnable()
        {
            useDayNightCycle = packUseDayNightCycle;
        }
#endif
        private void OnValidate()
        {
            LimitProperty();
        }

        
        private void Update()
        {
            if (!update) return;
            
            deltaTime = _previousTime == 0 ? 0 : Time.time - _previousTime;
            _previousTime = Time.time;
            float dayNightCycleDuration = dayNightCycleDurationMinute / 60;
            if (dayNightCycleDuration > 0f && useDayNightCycle)
            {
                const float CONVERSION_FACTOR = 24f * 1f / 3600f;
                float t = deltaTime * CONVERSION_FACTOR / dayNightCycleDuration;
                initTime.hour += t;
            }

            timeString = ConvertDecimalHoursToDateTimeString(initTime.hour);
            
            _frameCount++;
            
            SetupDynamicProperty();
        }
        
        
        #endregion
        
    }
    
}