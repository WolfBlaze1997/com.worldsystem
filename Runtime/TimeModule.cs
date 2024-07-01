using System;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WorldSystem.Runtime
{
    public partial class TimeModule
    {
        [Serializable]
        public struct Date
        {
            [HorizontalGroup] [LabelText("时")] [GUIColor(0.5f, 1.0f, 0.5f)] [Unit(Units.Hour)]
            public float Hour;

            [HorizontalGroup] [LabelText("年")] public int Year;

            [HorizontalGroup] [LabelText("月")] public int Month;

            [HorizontalGroup] [LabelText("日")] public int Day;
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
            DateTime startDate = new DateTime(initTime.Year, initTime.Month, initTime.Day);

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
            Year = 1997, Month = 1, Day = 1, Hour = 10
        };

        [ReadOnly] [LabelText("游戏时间")]
        public string timeString;

        [LabelText("使用昼夜循环")]
        public bool useDayNightCycle;

        //分钟为单位
        [LabelText("昼夜循环(分钟)")] [Unit(Units.Minute)] [EnableIf("useDayNightCycle")]
        public float dayNightCycleDurationMinute = 20;
        
        public float CurrentTime
        {
            get => initTime.Hour % 24;
        }
        
        [ShowInInspector]
        [ProgressBar(0f, 1f, ColorGetter = "gray")]
        [HideLabel] [EnableIf("useDayNightCycle")]
        public float CurrentTime01
        {
            get => CurrentTime / 24f;
        }

        /// <summary>
        /// 返回当前日间系数。1=白天，0=夜晚。
        /// </summary>
        public float DaytimeFactor
        {
            get => 1.0f - Mathf.Abs((CurrentTime01 - 0.5f) * 2.0f);
        }


        
        #endregion
        
        private float ManagedTime;
        private int FrameCount;
        
        
        #region 限制/安装属性

        private void LimitProperty()
        {
            dayNightCycleDurationMinute = Math.Max(dayNightCycleDurationMinute, 0);
            initTime.Hour = Math.Max(initTime.Hour, 0);
            initTime.Year = Math.Max(initTime.Year, 1);
            initTime.Month = Math.Max(initTime.Month, 1);
            initTime.Day = Math.Max(initTime.Day, 1);
        }
        
        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_EarthTime, CurrentTime);
            Shader.SetGlobalFloat(_FrameId, FrameCount % 4);
        }
        private readonly int _EarthTime = Shader.PropertyToID("_EarthTime");
        private readonly int _FrameId = Shader.PropertyToID("_FrameId");

        #endregion
        
        
        #region 事件函数
#if !UNITY_EDITOR
        private void OnEnable()
        {
            useDayNightCycle = true;
        }
#endif
        private void OnValidate()
        {
            LimitProperty();
        }

        [HideInInspector] public bool _Update;
        private float _previousTime;
        private void Update()
        {
            if (!_Update) return;
            
            float deltaTime = _previousTime == 0 ? 0 : Time.time - _previousTime;
            _previousTime = Time.time;
            float dayNightCycleDuration = dayNightCycleDurationMinute / 60;
            if (dayNightCycleDuration > 0f && useDayNightCycle)
            {
                const float CONVERSION_FACTOR = 24f * 1f / 3600f;
                float t = deltaTime * CONVERSION_FACTOR / dayNightCycleDuration;
                initTime.Hour += t;
            }

            timeString = ConvertDecimalHoursToDateTimeString(initTime.Hour);
            
            FrameCount++;
            
            SetupDynamicProperty();
        }
        
        #endregion


    }
    
}