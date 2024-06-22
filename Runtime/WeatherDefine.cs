using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace WorldSystem.Runtime
{
    [CreateAssetMenu(fileName = "天气定义", menuName = "世界系统/天气定义")]
    [Serializable]
    public partial class WeatherDefine : ScriptableObject
    {
        #region 字段

        private bool isActive;

        [ShowInInspector]
        [HorizontalGroup("Split",0.3f)]
        [ToggleLeft]
        [HideLabel]
        [PropertyOrder(-100)]
        public bool IsActive
        {
            get => isActive;
            set
            {
                List<WeatherDefine> weatherDefineList = WorldManager.Instance?.weatherSystemModule?.weatherList?.list;
#if UNITY_EDITOR
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
                if (weatherDefineAll != null && weatherDefineAll.Length != 0)
                {
                    foreach (var VARIABLE in weatherDefineAll)
                    {
                        if (VARIABLE is not null)
                        {
                            VARIABLE.isActive = false;
    #if UNITY_EDITOR
                            VARIABLE.modifTime = false;
    #endif
                        }
                    }
                }

#else
                //保证只有一个实例处于激活
                if (weatherDefineList != null && weatherDefineList.Count != 0)
                {
                    foreach (var VARIABLE in weatherDefineList)
                    {
                        if (VARIABLE is not null) VARIABLE.isActive = false;
                    }
                }
#endif
                isActive = value;

                if (isActive)
                {
#if UNITY_EDITOR
                    //如果实例位于天气队列中,且处于激活状态,则设置索引
                    if (weatherDefineList != null && weatherDefineList.Contains(this))
                    {
                        WorldManager.Instance.weatherSystemModule.i = weatherDefineList.IndexOf(this);
                    }
#endif
                    //一旦设置Active, 说明必定退出插值了, 将CelestialBody.useLerp 设置为false
                    CelestialBody.useLerp = false;
                    _singleExecute = true;
                }
            }
        }

#if UNITY_EDITOR
        [HorizontalGroup("Split",0.05f)][ShowInInspector]
        [EnableIf("isActive")][HideLabel][ToggleLeft]
        private bool modifTime;
#endif
        
        [HorizontalGroup("Split")] [LabelText("持续时间")]
        [EnableIf("@isActive && modifTime")]
        public float sustainedTime;

        [HorizontalGroup("Split")] [LabelText("切换时间")]
        [EnableIf("@isActive && modifTime")]
        public float varyingTime;

        [HideInInspector] public float sustainedTimeCache;
        [HideInInspector] public float varyingTimeCache;

        #endregion
        
        
        #region 事件函数
        
        private void OnValidate()
        {
            if (!isActive) return;
            
#if UNITY_EDITOR
            if (modifTime)
            {
                sustainedTimeCache = sustainedTime;
                varyingTimeCache = varyingTime;
            }
#endif
            //统一静态属性 #0B0B1600
            //(由于unity不支持序列化静态字段,无奈只能这样进行统一静态字段了)
            WeatherDefine[] allWeatherDefine = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var item in allWeatherDefine)
            {
                UniformStaticProperty_StarModule(item);
                UniformStaticProperty_CelestialBodyManager(item);
                UniformStaticProperty_AtmosphereModule(item);
                UniformStaticProperty_VolumeCloudOptimizeModule(item);
                UniformStaticProperty_WindZoneModule(item);
                UniformStaticProperty_WeatherEffectModule(item);
            }

            SetupProperty();
        }

        #endregion


        #region 安装属性

        public void SetupProperty()
        {
            SetupProperty_UniverseBackgroundModule();
            SetupProperty_StarModule();
            SetupProperty_CelestialBodyManager();
            SetupProperty_AtmosphereModule();
            SetupProperty_VolumeCloudOptimizeModule();
            SetupProperty_WindZoneModule();
            SetupProperty_WeatherEffectModule();
        }

        // 动态属性
        public void SetupLerpProperty(WeatherDefine weatherDefine, float lerpCoeff)
        {
            SetupLerpProperty_StarModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_CelestialBodyManager(weatherDefine, lerpCoeff);
            SetupLerpProperty_AtmosphereModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_VolumeCloudOptimizeModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_WindZoneModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_WeatherEffectModule(weatherDefine, lerpCoeff);
        }

        #endregion
    }

    
    /// <summary>
    /// 宇宙背景模块
    /// </summary>
    public partial class WeatherDefine
    {
        [FoldoutGroup("天气系统")]
        [FoldoutGroup("天气系统/宇宙背景模块")] [HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.universeBackgroundModuleToggle")]
        public UniverseBackgroundModule.Property backgroundProperty = new();
        
        
        public void SetupProperty_UniverseBackgroundModule()
        {
            if (WorldManager.Instance?.universeBackgroundModule is null)
                return;
            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.backgroundProperty.skyMesh = WorldManager.Instance.universeBackgroundModule.property.skyMesh;
                VARIABLE.backgroundProperty.backgroundShader = WorldManager.Instance.universeBackgroundModule.property.backgroundShader;
                VARIABLE.backgroundProperty.backgroundMaterial = WorldManager.Instance.universeBackgroundModule.property.backgroundMaterial;
                
            }
#endif

        }
        
    }
    
    
    /// <summary>
    /// 星星模块
    /// </summary>
    public partial class WeatherDefine
    {
        
        [FoldoutGroup("天气系统/星星模块")][HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.starModuleToggle")]
        public StarModule.Property StarProperty = new();
        
        public void SetupProperty_StarModule()
        {
            if (WorldManager.Instance?.starModule is null)
                return;
            StarProperty.LimitProperty();
            WorldManager.Instance.starModule.property.count = StarProperty.count;
            WorldManager.Instance.starModule.property.size = StarProperty.size;
            WorldManager.Instance.starModule.property.automaticColor = StarProperty.automaticColor;
            WorldManager.Instance.starModule.property.automaticBrightness = StarProperty.automaticBrightness;
            WorldManager.Instance.starModule.property.starColor = StarProperty.starColor;
            WorldManager.Instance.starModule.property.brightness = StarProperty.brightness;
            WorldManager.Instance.starModule.property.flickerFrequency = StarProperty.flickerFrequency;
            WorldManager.Instance.starModule.property.flickerStrength = StarProperty.flickerStrength;
            WorldManager.Instance.starModule.property.initialSeed = StarProperty.initialSeed;
            WorldManager.Instance.starModule.property.inclination = StarProperty.inclination;

            WorldManager.Instance.starModule.OnValidate();

#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.StarProperty.starMesh = WorldManager.Instance.starModule.property.starMesh;
                VARIABLE.StarProperty.starShader = WorldManager.Instance.starModule.property.starShader;
                VARIABLE.StarProperty.starMaterial = WorldManager.Instance.starModule.property.starMaterial;
                VARIABLE.StarProperty.starTexture = WorldManager.Instance.starModule.property.starTexture;
            }
#endif
        }

        public void UniformStaticProperty_StarModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.starModule is null)
                return;
            item.StarProperty.count = StarProperty.count;
            item.StarProperty.automaticColor = StarProperty.automaticColor;
            item.StarProperty.automaticBrightness = StarProperty.automaticBrightness;
            item.StarProperty.starColor = StarProperty.starColor;
            item.StarProperty.flickerFrequency = StarProperty.flickerFrequency;
            item.StarProperty.flickerStrength = StarProperty.flickerStrength;
            item.StarProperty.initialSeed = StarProperty.initialSeed;
            item.StarProperty.inclination = StarProperty.inclination;
        }

        public void SetupLerpProperty_StarModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.starModule is null)
                return;
            WorldManager.Instance.starModule.property.size = math.lerp(StarProperty.size, weatherDefine.StarProperty.size, lerpCoeff);
            WorldManager.Instance.starModule.property.brightness = math.lerp(StarProperty.brightness, weatherDefine.StarProperty.brightness, lerpCoeff);
        }
    }


    /// <summary>
    /// 星体模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 字段
        
        [FoldoutGroup("天气系统/星体模块")][LabelText("星体列表")]
        [ListDrawerSettings(CustomAddFunction = "CreateCelestialBody", CustomRemoveIndexFunction = "DestroyCelestialBody",OnTitleBarGUI = "DrawRefreshButton")]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.celestialBodyManagerToggle")]
        public List<CelestialBody.Property> celestialBodyList;
        
        [FoldoutGroup("天气系统/星体模块")][LabelText("星体数量限制")]
        [ShowInInspector][DisableInInlineEditors][ShowIf("@WorldManager.Instance?.celestialBodyManagerToggle")]
        private int maxCelestialBodyCount = 4;
        
        public void CreateCelestialBody()
        {
            if (celestialBodyList.Count == maxCelestialBodyCount)
            {
                Debug.Log($"最多支持到{maxCelestialBodyCount}个星体! 请与TA沟通!");
                return;
            }
            
            celestialBodyList.Add(new CelestialBody.Property());
            WorldManager.Instance?.celestialBodyManager?.property?.CreateCelestialBody();
            
            OnValidate();
        }
        
        private void DestroyCelestialBody(int index)
        {
            celestialBodyList.RemoveAt(index);
            WorldManager.Instance?.celestialBodyManager?.property?.DestroyCelestialBody(index);
            OnValidate();
        }


#if UNITY_EDITOR
        private  void DrawRefreshButton()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                celestialBodyList = celestialBodyList.OrderByDescending(o => o.sortOrder).ToList();
                
                var list = WorldManager.Instance?.celestialBodyManager?.property?.celestialBodyList;
                if (list != null)
                    list = list.OrderByDescending(o => o.property.sortOrder).ToList();
                
                OnValidate();
            }
        }
#endif
        
        
        #endregion

        
        public void SetupProperty_CelestialBodyManager()
        {
            if (WorldManager.Instance?.celestialBodyManager is null) 
                return;
            
            //保证列表中元素数量一致
            while (WorldManager.Instance.celestialBodyManager.property.celestialBodyList.Count != celestialBodyList.Count)
            {
                if(WorldManager.Instance.celestialBodyManager.property.celestialBodyList.Count > celestialBodyList.Count)
                    WorldManager.Instance.celestialBodyManager.property.DestroyCelestialBody(
                        WorldManager.Instance.celestialBodyManager.property.celestialBodyList.Count - 1);
                if (WorldManager.Instance.celestialBodyManager.property.celestialBodyList.Count < celestialBodyList.Count)
                    WorldManager.Instance.celestialBodyManager.property.CreateCelestialBody();
            }

            for (var i = 0; i < celestialBodyList.Count; i++)
            {
                celestialBodyList[i].LimitProperty();
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.type = celestialBodyList[i].type;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.objectColor = celestialBodyList[i].objectColor;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.angularDiameterDegrees = celestialBodyList[i].angularDiameterDegrees;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.texture = celestialBodyList[i].texture;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.sortOrder = celestialBodyList[i].sortOrder;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.orbitOffsetP = celestialBodyList[i].orbitOffsetP;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.orientationOffset = celestialBodyList[i].orientationOffset;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.inclinationOffset = celestialBodyList[i].inclinationOffset;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.positionIsStatic = celestialBodyList[i].positionIsStatic;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.falloff = celestialBodyList[i].falloff;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.useLight = celestialBodyList[i].useLight;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lightingColorMask = celestialBodyList[i].lightingColorMask;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.colorTemperatureCurve = celestialBodyList[i].colorTemperatureCurve;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.intensityCurve = celestialBodyList[i].intensityCurve;
                
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].OnValidate();
            }
            
            
#if UNITY_EDITOR
                //仅显示信息
                WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
                foreach (var VARIABLE in weatherDefineAll)
                {
                    for (var i = 0; i < VARIABLE.celestialBodyList.Count; i++)
                    {
                        VARIABLE.maxCelestialBodyCount = WorldManager.Instance.celestialBodyManager.property.maxCelestialBodyCount;
                        VARIABLE.celestialBodyList[i].GeocentricTheory = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.GeocentricTheory;
                        VARIABLE.celestialBodyList[i].skyObjectMesh = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.skyObjectMesh;
                        VARIABLE.celestialBodyList[i].shader = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.shader;
                        VARIABLE.celestialBodyList[i].material = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.material;
                        VARIABLE.celestialBodyList[i].lightComponent = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lightComponent;
                    }
                }
#endif
            
        }
        
        
        private void UniformStaticProperty_CelestialBodyManager(WeatherDefine item)
        {
            if (WorldManager.Instance?.celestialBodyManager is null)
                return;
            
            //统一星体数量
            while (item.celestialBodyList.Count != celestialBodyList.Count)
            {
                if(item.celestialBodyList.Count > celestialBodyList.Count)
                    item.celestialBodyList.RemoveAt(item.celestialBodyList.Count - 1);
                if (item.celestialBodyList.Count < celestialBodyList.Count)
                    item.celestialBodyList.Add(new CelestialBody.Property());;
            }

            for (var i = 0; i < item.celestialBodyList.Count; i++)
            {
                item.celestialBodyList[i].type = celestialBodyList[i].type;
                item.celestialBodyList[i].texture = celestialBodyList[i].texture;
                item.celestialBodyList[i].sortOrder = celestialBodyList[i].sortOrder;
                item.celestialBodyList[i].orbitOffsetP = celestialBodyList[i].orbitOffsetP;
                item.celestialBodyList[i].orientationOffset = celestialBodyList[i].orientationOffset;
                item.celestialBodyList[i].inclinationOffset = celestialBodyList[i].inclinationOffset;
                item.celestialBodyList[i].positionIsStatic = celestialBodyList[i].positionIsStatic;
                item.celestialBodyList[i].useLight = celestialBodyList[i].useLight;
                item.celestialBodyList[i].angularDiameterDegrees = celestialBodyList[i].angularDiameterDegrees;
            }
            
        }

        private static bool _singleExecute = true;
        //动态属性
        public void SetupLerpProperty_CelestialBodyManager(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.celestialBodyManager is null) 
                return;
            //当进入插值时将 CelestialBody.useLerp 设置为true,停止星体的属性的根据昼夜的变换, 而是由外部多个天气进行插值
            CelestialBody.useLerp = true;
            
            for (int i = 0; i < celestialBodyList.Count; i++)
            {
                //只执行一次
                if (_singleExecute)
                {
                    //输入下一个天气的动态属性
                    WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.objectColor =
                        weatherDefine.celestialBodyList[i].objectColor;
                    WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.falloff =
                        weatherDefine.celestialBodyList[i].falloff;
                    WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lightingColorMask =
                        weatherDefine.celestialBodyList[i].lightingColorMask;
                    WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.colorTemperatureCurve =
                        weatherDefine.celestialBodyList[i].colorTemperatureCurve;
                    WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.intensityCurve =
                        weatherDefine.celestialBodyList[i].intensityCurve;
                }

                //根据昼夜和两个天气之间进行插值
                var curveTime = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].curveTime;
                
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].objectColorEvaluate = 
                    Color.Lerp(celestialBodyList[i].objectColor.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].objectColor.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].falloffEvaluate = 
                    math.lerp(celestialBodyList[i].falloff.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].falloff.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].lightingColorMaskEvaluate = 
                    Color.Lerp(celestialBodyList[i].lightingColorMask.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].lightingColorMask.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].colorTemperatureCurveEvaluate = 
                    math.lerp(celestialBodyList[i].colorTemperatureCurve.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].colorTemperatureCurve.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].intensityCurveEvaluate = 
                    math.lerp(celestialBodyList[i].intensityCurve.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].intensityCurve.Evaluate(curveTime), lerpCoeff);
            }
            _singleExecute = false;
        }
        
    }


    /// <summary>
    /// 大气模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 大气模块
        
        [Serializable]
        public class AtmosphereModuleProperty : AtmosphereModule.Property
        {
            private protected override void DayPeriodsListAddFunc()
            {
                var newDayPeriods = new AtmosphereModule.DayPeriods("Day", 7f,
                    new Color32(18, 25, 59, 255),
                    new Color32(123, 153, 173, 255),
                    new Color32(14, 12, 10, 255));
                
                WorldManager.Instance?.atmosphereModule?.property?.dayPeriodsList?.Add(newDayPeriods);

                WeatherDefine[] WeatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
                foreach (var VARIABLE in WeatherDefineAll)
                {
                    VARIABLE.atmosphereProperty.dayPeriodsList.Add(newDayPeriods);
                }
            }
            private protected override void DayPeriodsListRemoveFunc(int Index)
            {
                WorldManager.Instance?.atmosphereModule?.property?.dayPeriodsList?.RemoveAt(Index);

                WeatherDefine[] WeatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
                foreach (var VARIABLE in WeatherDefineAll)
                {
                    VARIABLE.atmosphereProperty.dayPeriodsList.RemoveAt(Index);
                }
            }

#if UNITY_EDITOR
            private protected override void DrawRefreshButton()
            {
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
                {
                    var list = WorldManager.Instance?.atmosphereModule?.property?.dayPeriodsList;
                    if (list != null) WorldManager.Instance.atmosphereModule.property.dayPeriodsList = list.OrderBy(o => o.startTime).ToList();

                    WeatherDefine[] WeatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
                    foreach (var VARIABLE in WeatherDefineAll)
                    {
                        VARIABLE.atmosphereProperty.dayPeriodsList = VARIABLE.atmosphereProperty.dayPeriodsList
                            .OrderBy(o => o.startTime).ToList();
                    }
                }
            }
#endif
            
        }
        
        [FoldoutGroup("天气系统/大气模块")][HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.atmosphereModuleToggle")]
        public AtmosphereModuleProperty atmosphereProperty = new();
        
        #endregion
        
        public void SetupProperty_AtmosphereModule()
        {
            if (WorldManager.Instance?.atmosphereModule is null)
                return;
            // atmosphereProperty.LimitProperty();
            WorldManager.Instance.atmosphereModule.property.dayPeriodsList =
                new List<AtmosphereModule.DayPeriods>(atmosphereProperty.dayPeriodsList);
            WorldManager.Instance.atmosphereModule.property.useAtmosphereMap = atmosphereProperty.useAtmosphereMap;
            WorldManager.Instance.atmosphereModule.property.useAtmosphereBlend = atmosphereProperty.useAtmosphereBlend;
            WorldManager.Instance.atmosphereModule.property.start = atmosphereProperty.start;
            WorldManager.Instance.atmosphereModule.property.end = atmosphereProperty.end;

            WorldManager.Instance.atmosphereModule.OnValidate();
            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.atmosphereProperty.currentAtmosphereColor = WorldManager.Instance.atmosphereModule.property.currentAtmosphereColor;
                VARIABLE.atmosphereProperty.currentCloudiness = WorldManager.Instance.atmosphereModule.property.currentCloudiness;
                VARIABLE.atmosphereProperty.currentPrecipitation = WorldManager.Instance.atmosphereModule.property.currentPrecipitation;
                VARIABLE.atmosphereProperty.mesh = WorldManager.Instance.atmosphereModule.property.mesh;
                VARIABLE.atmosphereProperty.shader = WorldManager.Instance.atmosphereModule.property.shader;
                VARIABLE.atmosphereProperty.material = WorldManager.Instance.atmosphereModule.property.material;
                VARIABLE.atmosphereProperty.atmosphereBlendShader = WorldManager.Instance.atmosphereModule.property.atmosphereBlendShader;
                VARIABLE.atmosphereProperty.atmosphereBlendMaterial = WorldManager.Instance.atmosphereModule.property.atmosphereBlendMaterial;
            }
#endif
        }

        private void UniformStaticProperty_AtmosphereModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.atmosphereModule is null)
                return;
            
            AtmosphereModule.DayPeriods newDayPeriods = new AtmosphereModule.DayPeriods("Day", 7f,
                new Color32(18, 25, 59, 255),
                new Color32(123, 153, 173, 255),
                new Color32(14, 12, 10, 255));
            
            //保证所有实例的dayPeriodsList列表拥有相同数量的元素
            if (item.atmosphereProperty.dayPeriodsList.Count < atmosphereProperty.dayPeriodsList.Count)
            {
                for (int i = 0; i < atmosphereProperty.dayPeriodsList.Count - item.atmosphereProperty.dayPeriodsList.Count; i++)
                {
                    item.atmosphereProperty.dayPeriodsList.Add(newDayPeriods);
                }
            }
            else if (item.atmosphereProperty.dayPeriodsList.Count > atmosphereProperty.dayPeriodsList.Count)
            {
                for (int i = 0; i < item.atmosphereProperty.dayPeriodsList.Count - atmosphereProperty.dayPeriodsList.Count; i++)
                {
                    item.atmosphereProperty.dayPeriodsList.RemoveAt(item.atmosphereProperty.dayPeriodsList.Count - 1);
                }
            }

            for (int i = 0; i < atmosphereProperty.dayPeriodsList.Count; i++)
            {
                var itemDayPeriods = item.atmosphereProperty.dayPeriodsList[i];
                itemDayPeriods.description = atmosphereProperty.dayPeriodsList[i].description;
                itemDayPeriods.startTime = atmosphereProperty.dayPeriodsList[i].startTime;
                item.atmosphereProperty.dayPeriodsList[i] = itemDayPeriods;
            }

            item.atmosphereProperty.useAtmosphereMap = atmosphereProperty.useAtmosphereMap;
            item.atmosphereProperty.useAtmosphereBlend = atmosphereProperty.useAtmosphereBlend;
        }

        public void SetupLerpProperty_AtmosphereModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.atmosphereModule is null)
                return;
            
            //插值dayPeriodsList中的所有颜色
            for (int i = 0; i < WorldManager.Instance.atmosphereModule.property.dayPeriodsList.Count; i++)
            {
                AtmosphereModule.DayPeriods atmosphereModuleDayPeriods =
                    WorldManager.Instance.atmosphereModule.property.dayPeriodsList[i];
                atmosphereModuleDayPeriods.skyColor = Color.Lerp(atmosphereProperty.dayPeriodsList[i].skyColor,
                    weatherDefine.atmosphereProperty.dayPeriodsList[i].skyColor, lerpCoeff);
                atmosphereModuleDayPeriods.equatorColor = Color.Lerp(atmosphereProperty.dayPeriodsList[i].equatorColor,
                    weatherDefine.atmosphereProperty.dayPeriodsList[i].equatorColor, lerpCoeff);
                atmosphereModuleDayPeriods.groundColor = Color.Lerp(atmosphereProperty.dayPeriodsList[i].groundColor,
                    weatherDefine.atmosphereProperty.dayPeriodsList[i].groundColor, lerpCoeff);
                WorldManager.Instance.atmosphereModule.property.dayPeriodsList[i] = atmosphereModuleDayPeriods;
            }
            
            WorldManager.Instance.atmosphereModule.property.start = 
                math.lerp(atmosphereProperty.start, weatherDefine.atmosphereProperty.start, lerpCoeff);
            WorldManager.Instance.atmosphereModule.property.end = 
                math.lerp(atmosphereProperty.end, weatherDefine.atmosphereProperty.end, lerpCoeff);
        }
    }


    /// <summary>
    /// 体积云模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 体积云模块

        [FoldoutGroup("天气系统/体积云模块")] [HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModuleToggle")]
        public VolumeCloudOptimizeModule.Property cloudProperty = new();
        
        #endregion


        public void SetupProperty_VolumeCloudOptimizeModule()
        {
            if (WorldManager.Instance?.volumeCloudOptimizeModule is null)
                return;
            cloudProperty.LimitProperty();
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_MaxRenderDistance = cloudProperty._Render_MaxRenderDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_CoarseSteps = cloudProperty._Render_CoarseSteps;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_DetailSteps = cloudProperty._Render_DetailSteps;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_BlueNoise = cloudProperty._Render_BlueNoise;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_MipmapDistance = cloudProperty._Render_MipmapDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_DepthOptions = cloudProperty._Render_DepthOptions;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_UseReprojection = cloudProperty._Render_UseReprojection;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_UseTemporalAA = cloudProperty._Render_UseTemporalAA;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_TemporalAAFactor = cloudProperty._Render_TemporalAAFactor;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Render_ResolutionOptions = cloudProperty._Render_ResolutionOptions;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_CloudAmount = cloudProperty._Modeling_Amount_CloudAmount;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_UseFarOverlay =
                cloudProperty._Modeling_Amount_UseFarOverlay;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_OverlayStartDistance =
                cloudProperty._Modeling_Amount_OverlayStartDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_OverlayCloudAmount =
                cloudProperty._Modeling_Amount_OverlayCloudAmount;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Octaves = cloudProperty._Modeling_ShapeBase_Octaves;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Gain = cloudProperty._Modeling_ShapeBase_Gain;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Freq = cloudProperty._Modeling_ShapeBase_Freq;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Scale = cloudProperty._Modeling_ShapeBase_Scale;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeDetail_Type = cloudProperty._Modeling_ShapeDetail_Type;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeDetail_Quality =
                cloudProperty._Modeling_ShapeDetail_Quality;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeDetail_Scale = cloudProperty._Modeling_ShapeDetail_Scale;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_RadiusPreset =
                cloudProperty._Modeling_Position_RadiusPreset;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_CloudHeight =
                cloudProperty._Modeling_Position_CloudHeight;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_CloudThickness =
                cloudProperty._Modeling_Position_CloudThickness;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_UseDynamicCloud = cloudProperty._MotionBase_UseDynamicCloud;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_Direction = cloudProperty._MotionBase_Direction;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_Speed = cloudProperty._MotionBase_Speed;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_UseDirectionRandom =
                cloudProperty._MotionBase_UseDirectionRandom;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_DirectionRandomRange =
                cloudProperty._MotionBase_DirectionRandomRange;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_DirectionRandomFreq =
                cloudProperty._MotionBase_DirectionRandomFreq;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_Direction = cloudProperty._MotionDetail_Direction;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_Speed = cloudProperty._MotionDetail_Speed;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_UseRandomDirection =
                cloudProperty._MotionDetail_UseRandomDirection;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_DirectionRandomRange =
                cloudProperty._MotionDetail_DirectionRandomRange;
            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_DirectionRandomFreq =
                cloudProperty._MotionDetail_DirectionRandomFreq;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_AlbedoColor = cloudProperty._Lighting_AlbedoColor;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_LightColorFilter = cloudProperty._Lighting_LightColorFilter;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ExtinctionCoeff = cloudProperty._Lighting_ExtinctionCoeff;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_DensityInfluence = cloudProperty._Lighting_DensityInfluence;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HeightDensityInfluence =
                cloudProperty._Lighting_HeightDensityInfluence;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_CheapAmbient = cloudProperty._Lighting_CheapAmbient;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_AmbientExposure = cloudProperty._Lighting_AmbientExposure;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_UseAtmosphereVisibilityOverlay =
                cloudProperty._Lighting_UseAtmosphereVisibilityOverlay;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_AtmosphereVisibility =
                cloudProperty._Lighting_AtmosphereVisibility;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HGStrength = cloudProperty._Lighting_HGStrength;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HGEccentricityForward =
                cloudProperty._Lighting_HGEccentricityForward;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HGEccentricityBackward =
                cloudProperty._Lighting_HGEccentricityBackward;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_MaxLightingDistance =
                cloudProperty._Lighting_MaxLightingDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ShadingStrengthFalloff =
                cloudProperty._Lighting_ShadingStrengthFalloff;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ScatterMultiplier = cloudProperty._Lighting_ScatterMultiplier;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ScatterStrength = cloudProperty._Lighting_ScatterStrength;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Shadow_UseCastShadow = cloudProperty._Shadow_UseCastShadow;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Shadow_Distance = cloudProperty._Shadow_Distance;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Shadow_Strength = cloudProperty._Shadow_Strength;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Shadow_Resolution = cloudProperty._Shadow_Resolution;
            WorldManager.Instance.volumeCloudOptimizeModule.property._Shadow_UseShadowTaa = cloudProperty._Shadow_UseShadowTaa;
            if (cloudProperty._Modeling_Position_RadiusPreset == VolumeCloudOptimizeModule.CelestialBodySelection.Custom)
                WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_PlanetRadius =
                    cloudProperty._Modeling_Position_PlanetRadius;

            WorldManager.Instance.volumeCloudOptimizeModule.OnValidate();

            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.cloudProperty.VolumeCloud_BaseTex_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_BaseTex_Shader;
                VARIABLE.cloudProperty.VolumeCloud_BaseTex_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_BaseTex_Material;
                VARIABLE.cloudProperty.VolumeCloud_DitherDepth_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_DitherDepth_Shader;
                VARIABLE.cloudProperty.VolumeCloud_DitherDepth_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_DitherDepth_Material;
                VARIABLE.cloudProperty.VolumeCloud_Main_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_Main_Shader;
                VARIABLE.cloudProperty.VolumeCloud_Main_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_Main_Material;
                VARIABLE.cloudProperty.VolumeCloud_Reproject_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_Reproject_Shader;
                VARIABLE.cloudProperty.VolumeCloud_Reproject_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_Reproject_Material;
                VARIABLE.cloudProperty.VolumeCloud_UpScale_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_UpScale_Shader;
                VARIABLE.cloudProperty.VolumeCloud_UpScale_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_UpScale_Material;
                VARIABLE.cloudProperty.VolumeCloud_Merge_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_Merge_Shader;
                VARIABLE.cloudProperty.VolumeCloud_Merge_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.VolumeCloud_Merge_Material;
                VARIABLE.cloudProperty.Halton = WorldManager.Instance.volumeCloudOptimizeModule.property.Halton;
                VARIABLE.cloudProperty._Modeling_ShapeDetail_NoiseTexture3D = WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeDetail_NoiseTexture3D;
                VARIABLE.cloudProperty._MotionBase_DynamicVector = WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_DynamicVector;
                VARIABLE.cloudProperty._MotionDetail_DynamicVector = WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_DynamicVector;
                VARIABLE.cloudProperty.CloudShadows_TemporalAA_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.CloudShadows_TemporalAA_Shader;
                VARIABLE.cloudProperty.CloudShadows_TemporalAA_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.CloudShadows_TemporalAA_Material;
                VARIABLE.cloudProperty.CloudShadows_ScreenShadow_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.CloudShadows_ScreenShadow_Shader;
                VARIABLE.cloudProperty.CloudShadows_ScreenShadow_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.CloudShadows_ScreenShadow_Material;
                VARIABLE.cloudProperty.CloudShadows_ToScreen_Shader = WorldManager.Instance.volumeCloudOptimizeModule.property.CloudShadows_ToScreen_Shader;
                VARIABLE.cloudProperty.CloudShadows_ToScreen_Material = WorldManager.Instance.volumeCloudOptimizeModule.property.CloudShadows_ToScreen_Material;
                
                if (VARIABLE.cloudProperty._Modeling_Position_RadiusPreset != VolumeCloudOptimizeModule.CelestialBodySelection.Custom)
                    VARIABLE.cloudProperty._Modeling_Position_PlanetRadius = WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_PlanetRadius;
                
            }
#endif
            
        }

        private void UniformStaticProperty_VolumeCloudOptimizeModule(WeatherDefine item)
        {
            item.cloudProperty._Render_MaxRenderDistance = cloudProperty._Render_MaxRenderDistance;
            item.cloudProperty._Render_CoarseSteps = cloudProperty._Render_CoarseSteps;
            item.cloudProperty._Render_DetailSteps = cloudProperty._Render_DetailSteps;
            item.cloudProperty._Render_BlueNoise = cloudProperty._Render_BlueNoise;
            item.cloudProperty._Render_MipmapDistance = cloudProperty._Render_MipmapDistance;
            item.cloudProperty._Render_DepthOptions = cloudProperty._Render_DepthOptions;
            item.cloudProperty._Render_UseReprojection = cloudProperty._Render_UseReprojection;
            item.cloudProperty._Render_UseTemporalAA = cloudProperty._Render_UseTemporalAA;
            item.cloudProperty._Render_TemporalAAFactor = cloudProperty._Render_TemporalAAFactor;
            item.cloudProperty._Render_ResolutionOptions = cloudProperty._Render_ResolutionOptions;
            item.cloudProperty._Modeling_Amount_UseFarOverlay = cloudProperty._Modeling_Amount_UseFarOverlay;
            item.cloudProperty._Modeling_ShapeBase_Octaves = cloudProperty._Modeling_ShapeBase_Octaves;
            item.cloudProperty._Modeling_ShapeBase_Freq = cloudProperty._Modeling_ShapeBase_Freq;
            item.cloudProperty._Modeling_ShapeDetail_NoiseTexture3D = cloudProperty._Modeling_ShapeDetail_NoiseTexture3D;
            item.cloudProperty._Modeling_ShapeDetail_Type = cloudProperty._Modeling_ShapeDetail_Type;
            item.cloudProperty._Modeling_ShapeDetail_Quality = cloudProperty._Modeling_ShapeDetail_Quality;
            item.cloudProperty._Modeling_Position_RadiusPreset = cloudProperty._Modeling_Position_RadiusPreset;
            item.cloudProperty._Modeling_Position_PlanetRadius = cloudProperty._Modeling_Position_PlanetRadius;
            item.cloudProperty._MotionBase_UseDynamicCloud = cloudProperty._MotionBase_UseDynamicCloud;
            item.cloudProperty._MotionBase_DynamicVector = cloudProperty._MotionBase_DynamicVector;
            item.cloudProperty._MotionBase_Direction = cloudProperty._MotionBase_Direction;
            item.cloudProperty._MotionBase_UseDirectionRandom = cloudProperty._MotionBase_UseDirectionRandom;
            item.cloudProperty._MotionBase_DirectionRandomRange = cloudProperty._MotionBase_DirectionRandomRange;
            item.cloudProperty._MotionBase_DirectionRandomFreq = cloudProperty._MotionBase_DirectionRandomFreq;
            item.cloudProperty._MotionDetail_DynamicVector = cloudProperty._MotionDetail_DynamicVector;
            item.cloudProperty._MotionDetail_Direction = cloudProperty._MotionDetail_Direction;
            item.cloudProperty._MotionDetail_UseRandomDirection = cloudProperty._MotionDetail_UseRandomDirection;
            item.cloudProperty._MotionDetail_DirectionRandomRange = cloudProperty._MotionDetail_DirectionRandomRange;
            item.cloudProperty._MotionDetail_DirectionRandomFreq = cloudProperty._MotionDetail_DirectionRandomFreq;
            item.cloudProperty._Lighting_CheapAmbient = cloudProperty._Lighting_CheapAmbient;
            item.cloudProperty._Lighting_UseAtmosphereVisibilityOverlay = cloudProperty._Lighting_UseAtmosphereVisibilityOverlay;
            item.cloudProperty._Shadow_UseCastShadow = cloudProperty._Shadow_UseCastShadow;
            item.cloudProperty._Shadow_Distance = cloudProperty._Shadow_Distance;
            item.cloudProperty._Shadow_Strength = cloudProperty._Shadow_Strength;
            item.cloudProperty._Shadow_Resolution = cloudProperty._Shadow_Resolution;
            item.cloudProperty._Shadow_UseShadowTaa = cloudProperty._Shadow_UseShadowTaa;
        }

        public void SetupLerpProperty_VolumeCloudOptimizeModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.volumeCloudOptimizeModule is null)
                return;
            // WorldManager.Instance.volumeCloudOptimizeModule.property._Render_DetailSteps =
            //     (int)math.lerp(cloudProperty._Render_DetailSteps, weatherDefine.cloudProperty._Render_DetailSteps, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_CloudAmount =
                math.lerp(cloudProperty._Modeling_Amount_CloudAmount, weatherDefine.cloudProperty._Modeling_Amount_CloudAmount, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_OverlayStartDistance =
                math.lerp(cloudProperty._Modeling_Amount_OverlayStartDistance, weatherDefine.cloudProperty._Modeling_Amount_OverlayStartDistance,
                    lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Amount_OverlayCloudAmount =
                math.lerp(cloudProperty._Modeling_Amount_OverlayCloudAmount, weatherDefine.cloudProperty._Modeling_Amount_OverlayCloudAmount,
                    lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Gain =
                math.lerp(cloudProperty._Modeling_ShapeBase_Gain, weatherDefine.cloudProperty._Modeling_ShapeBase_Gain, lerpCoeff);

            // WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Freq =
            //     math.lerp(cloudProperty._Modeling_ShapeBase_Freq, weatherDefine.cloudProperty._Modeling_ShapeBase_Freq, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeBase_Scale =
                math.lerp(cloudProperty._Modeling_ShapeBase_Scale, weatherDefine.cloudProperty._Modeling_ShapeBase_Scale, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_ShapeDetail_Scale =
                math.lerp(cloudProperty._Modeling_ShapeDetail_Scale, weatherDefine.cloudProperty._Modeling_ShapeDetail_Scale, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_CloudHeight =
                math.lerp(cloudProperty._Modeling_Position_CloudHeight, weatherDefine.cloudProperty._Modeling_Position_CloudHeight, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Modeling_Position_CloudThickness =
                math.lerp(cloudProperty._Modeling_Position_CloudThickness, weatherDefine.cloudProperty._Modeling_Position_CloudThickness,
                    lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionBase_Speed =
                math.lerp(cloudProperty._MotionBase_Speed, weatherDefine.cloudProperty._MotionBase_Speed, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._MotionDetail_Speed =
                math.lerp(cloudProperty._MotionDetail_Speed, weatherDefine.cloudProperty._MotionDetail_Speed, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_AlbedoColor =
                Color.Lerp(cloudProperty._Lighting_AlbedoColor, weatherDefine.cloudProperty._Lighting_AlbedoColor, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_LightColorFilter =
                Color.Lerp(cloudProperty._Lighting_LightColorFilter, weatherDefine.cloudProperty._Lighting_LightColorFilter, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ExtinctionCoeff =
                math.lerp(cloudProperty._Lighting_ExtinctionCoeff, weatherDefine.cloudProperty._Lighting_ExtinctionCoeff, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_DensityInfluence =
                math.lerp(cloudProperty._Lighting_DensityInfluence, weatherDefine.cloudProperty._Lighting_DensityInfluence, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HeightDensityInfluence =
                math.lerp(cloudProperty._Lighting_HeightDensityInfluence, weatherDefine.cloudProperty._Lighting_HeightDensityInfluence, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_AmbientExposure =
                math.lerp(cloudProperty._Lighting_AmbientExposure, weatherDefine.cloudProperty._Lighting_AmbientExposure, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_AtmosphereVisibility =
                math.lerp(cloudProperty._Lighting_AtmosphereVisibility, weatherDefine.cloudProperty._Lighting_AtmosphereVisibility, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HGStrength =
                math.lerp(cloudProperty._Lighting_HGStrength, weatherDefine.cloudProperty._Lighting_HGStrength, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HGEccentricityForward =
                math.lerp(cloudProperty._Lighting_HGEccentricityForward, weatherDefine.cloudProperty._Lighting_HGEccentricityForward, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_HGEccentricityBackward =
                math.lerp(cloudProperty._Lighting_HGEccentricityBackward, weatherDefine.cloudProperty._Lighting_HGEccentricityBackward, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_MaxLightingDistance =
                (int)math.lerp(cloudProperty._Lighting_MaxLightingDistance, weatherDefine.cloudProperty._Lighting_MaxLightingDistance, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ShadingStrengthFalloff =
                math.lerp(cloudProperty._Lighting_ShadingStrengthFalloff, weatherDefine.cloudProperty._Lighting_ShadingStrengthFalloff, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ScatterMultiplier =
                math.lerp(cloudProperty._Lighting_ScatterMultiplier, weatherDefine.cloudProperty._Lighting_ScatterMultiplier, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ScatterStrength =
                math.lerp(cloudProperty._Lighting_ScatterStrength, weatherDefine.cloudProperty._Lighting_ScatterStrength, lerpCoeff);
        }
    }


    /// <summary>
    /// 风区模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 风区模块
        
        [FoldoutGroup("天气系统/风区模块")] [HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.windZoneModuleToggle")]
        public WindZoneModule.Property windZoneProperty = new();

        #endregion


        public void SetupProperty_WindZoneModule()
        {
            if (WorldManager.Instance?.windZoneModule is null)
                return;
            windZoneProperty.LimitProperty();
            WorldManager.Instance.windZoneModule.property.dynamicDirection = windZoneProperty.dynamicDirection;
            WorldManager.Instance.windZoneModule.property.directionVaryingFreq = windZoneProperty.directionVaryingFreq;
            WorldManager.Instance.windZoneModule.property.dynamicSpeed = windZoneProperty.dynamicSpeed;
            WorldManager.Instance.windZoneModule.property.SpeedVaryingFreq = windZoneProperty.SpeedVaryingFreq;
            WorldManager.Instance.windZoneModule.property.minSpeed = windZoneProperty.minSpeed;
            WorldManager.Instance.windZoneModule.property.maxSpeed = windZoneProperty.maxSpeed;
            WorldManager.Instance.windZoneModule.property.vfxModifier = windZoneProperty.vfxModifier;

#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.windZoneProperty.windZone = WorldManager.Instance.windZoneModule.property.windZone;
            }
#endif
        }

        private void UniformStaticProperty_WindZoneModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.windZoneModule is null)
                return;
            item.windZoneProperty.dynamicDirection = windZoneProperty.dynamicDirection;
            item.windZoneProperty.directionVaryingFreq = windZoneProperty.directionVaryingFreq;
            item.windZoneProperty.dynamicSpeed = windZoneProperty.dynamicSpeed;
            item.windZoneProperty.SpeedVaryingFreq = windZoneProperty.SpeedVaryingFreq;
            item.windZoneProperty.vfxModifier = windZoneProperty.vfxModifier;
            item.windZoneProperty.windZone = windZoneProperty.windZone;
        }

        public void SetupLerpProperty_WindZoneModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.windZoneModule is null)
                return;
            WorldManager.Instance.windZoneModule.property.minSpeed =
                math.lerp(windZoneProperty.minSpeed, weatherDefine.windZoneProperty.minSpeed, lerpCoeff);
            WorldManager.Instance.windZoneModule.property.maxSpeed =
                math.lerp(windZoneProperty.maxSpeed, weatherDefine.windZoneProperty.maxSpeed, lerpCoeff);
        }
    }

    
    
    /// <summary>
    /// 天气效果模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 天气效果模块


#if UNITY_EDITOR
        [ShowIf("@WorldManager.Instance?.weatherEffectModule?.rainEnable")]
        [HorizontalGroup("天气系统/天气效果模块/Split01")]
        [VerticalGroup("天气系统/天气效果模块/Split01/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块已开启"), GUIColor(0.5f, 0.5f, 1f)]
        [EnableIf("isActive")]
        public void RainToggle_Off()
        {
            WorldManager.Instance?.weatherEffectModule?.RainToggle_Off();
        }
    
        [HideIf("@WorldManager.Instance?.weatherEffectModule?.rainEnable")]
        [VerticalGroup("天气系统/天气效果模块/Split01/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        [EnableIf("isActive")]
        public void RainToggle_On()
        {
            WorldManager.Instance?.weatherEffectModule?.RainToggle_On();
        }
    
        [ShowIf("@WorldManager.Instance?.weatherEffectModule?.snowEnable")]
        [VerticalGroup("天气系统/天气效果模块/Split01/02")]
        [Button(ButtonSizes.Medium, Name = "雪模块已开启"), GUIColor(0.5f, 0.5f, 1f)]
        [EnableIf("isActive")] 
        public void SnowToggle_Off()
        {
            WorldManager.Instance?.weatherEffectModule?.SnowToggle_Off();
        }
    
        [HideIf("@WorldManager.Instance?.weatherEffectModule?.snowEnable")]
        [VerticalGroup("天气系统/天气效果模块/Split01/02")]
        [Button(ButtonSizes.Medium, Name = "雪模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        [EnableIf("isActive")] 
        public void SnowToggle_On()
        {
            WorldManager.Instance?.weatherEffectModule?.SnowToggle_On();
        }
    
        [ShowIf("@WorldManager.Instance?.weatherEffectModule?.lightningEnable")]
        [VerticalGroup("天气系统/天气效果模块/Split01/03")]
        [Button(ButtonSizes.Medium, Name = "闪电模块已开启"), GUIColor(0.5f, 0.5f, 1f)]
        [EnableIf("isActive")]
        public void LightningToggle_Off()
        {
            WorldManager.Instance?.weatherEffectModule?.LightningToggle_Off();
        }
    
        [HideIf("@WorldManager.Instance?.weatherEffectModule?.lightningEnable")]
        [VerticalGroup("天气系统/天气效果模块/Split01/03")]
        [Button(ButtonSizes.Medium, Name = "闪电模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        [EnableIf("isActive")]
        public void LightningToggle_On()
        {
            WorldManager.Instance?.weatherEffectModule?.LightningToggle_On();
        }
#endif
    
        
        
        [PropertyOrder(1)] [FoldoutGroup("天气系统/天气效果模块")] [LabelText("使用遮蔽")] [GUIColor(0, 1, 0)]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.weatherEffectModuleToggle")]
        public bool useOcclusion;
    
        [PropertyOrder(1)] [FoldoutGroup("天气系统/天气效果模块")] [LabelText("范围半径")] [GUIColor(0.7f, 0.7f, 1f)]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.weatherEffectModuleToggle")]
        public float effectRadius = 40;
    
        [PropertyOrder(1)]
        [FoldoutGroup("天气系统/天气效果模块/雨")][HideLabel]
        [EnableIf("isActive")][ShowIf("@WorldManager.Instance?.weatherEffectModule?.rainEnable")]
        public VFXRainEffect.Property rainEffectProperty = new();
    
        [PropertyOrder(1)]
        [FoldoutGroup("天气系统/天气效果模块/雪")][HideLabel]
        [EnableIf("isActive")][ShowIf("@WorldManager.Instance?.weatherEffectModule?.snowEnable")]
        public VFXSnowEffect.Property snowEffectProperty = new();
    
        [PropertyOrder(1)]
        [FoldoutGroup("天气系统/天气效果模块/闪电")][HideLabel]
        [EnableIf("isActive")][ShowIf("@WorldManager.Instance?.weatherEffectModule?.lightningEnable")]
        public VFXLightningEffect.Property lightningEffectProperty = new();
    
        [PropertyOrder(1)]
        [FoldoutGroup("天气系统/天气效果模块/遮蔽渲染器")]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [EnableIf("isActive")][ShowIf("@WorldManager.Instance?.weatherEffectModule?.useOcclusion")]
        public OcclusionRenderer occlusionRenderer;
    
        public void SetupProperty_WeatherEffectModule()
        {
            if (WorldManager.Instance?.weatherEffectModule is null)
                return;
            effectRadius = Math.Max(effectRadius, 10);
            WorldManager.Instance.weatherEffectModule.useOcclusion = useOcclusion;
            WorldManager.Instance.weatherEffectModule.effectRadius = effectRadius;
            WorldManager.Instance.weatherEffectModule.OnValidate();

            if (WorldManager.Instance.weatherEffectModule.rainEffect != null)
            {
                rainEffectProperty.LimitProperty();
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainPrecipitation = rainEffectProperty.rainPrecipitation;
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainSize = rainEffectProperty.rainSize;
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainDropLength = rainEffectProperty.rainDropLength;
                WorldManager.Instance.weatherEffectModule.rainEffect.OnValidate();
            }
            
            if (WorldManager.Instance.weatherEffectModule.snowEffect != null)
            {
                snowEffectProperty.LimitProperty();
                WorldManager.Instance.weatherEffectModule.snowEffect.property.snowPrecipitation = snowEffectProperty.snowPrecipitation;
                WorldManager.Instance.weatherEffectModule.snowEffect.property.snowSize = snowEffectProperty.snowSize;
                WorldManager.Instance.weatherEffectModule.snowEffect.OnValidate();
            }
            
            if (WorldManager.Instance.weatherEffectModule.lightningEffect != null)
            {
                lightningEffectProperty.LimitProperty();
                WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningLightStrength = lightningEffectProperty.lightningLightStrength;
                WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningLength = lightningEffectProperty.lightningLength;
                WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningMinLifetime = lightningEffectProperty.lightningMinLifetime;
                WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningMaxLifetime = lightningEffectProperty.lightningMaxLifetime;
                WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningSpawnRate = lightningEffectProperty.lightningSpawnRate;
                WorldManager.Instance.weatherEffectModule.lightningEffect.OnValidate();
            }
            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                if (WorldManager.Instance.weatherEffectModule.rainEffect != null)
                    VARIABLE.rainEffectProperty.rainRadius = WorldManager.Instance.weatherEffectModule.rainEffect.property.rainRadius;
                
                if (WorldManager.Instance.weatherEffectModule.snowEffect != null)
                    VARIABLE.snowEffectProperty.snowRadius = WorldManager.Instance.weatherEffectModule.snowEffect.property.snowRadius;

                if (WorldManager.Instance.weatherEffectModule.lightningEffect != null)
                {
                    VARIABLE.lightningEffectProperty.prefab = WorldManager.Instance.weatherEffectModule.lightningEffect.property.prefab;
                    VARIABLE.lightningEffectProperty.mainCamera = WorldManager.Instance.weatherEffectModule.lightningEffect.property.mainCamera;
                    VARIABLE.lightningEffectProperty.lightningLit = WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningLit;
                    VARIABLE.lightningEffectProperty.lightningDataObjectArray = WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningDataObjectArray;
                    VARIABLE.lightningEffectProperty.lightningLifetimeArray = WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningLifetimeArray;
                }
                
                if(WorldManager.Instance.weatherEffectModule.occlusionRenderer != null)
                    VARIABLE.occlusionRenderer = WorldManager.Instance.weatherEffectModule.occlusionRenderer;
            }
#endif

        }
        private void UniformStaticProperty_WeatherEffectModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.weatherEffectModule is null)
                return;
            item.useOcclusion = useOcclusion;
            item.effectRadius = effectRadius;

            item.lightningEffectProperty.lightningLightStrength = lightningEffectProperty.lightningLightStrength;
            item.lightningEffectProperty.lightningLength = lightningEffectProperty.lightningLength;
            item.lightningEffectProperty.lightningMinLifetime = lightningEffectProperty.lightningMinLifetime;
            item.lightningEffectProperty.lightningMaxLifetime = lightningEffectProperty.lightningMaxLifetime;
        }
        
        public void SetupLerpProperty_WeatherEffectModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.weatherEffectModule is null)
                return;
            
            if (WorldManager.Instance.weatherEffectModule.rainEffect is not null)
            {
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainPrecipitation =
                    math.lerp(rainEffectProperty.rainPrecipitation, weatherDefine.rainEffectProperty.rainPrecipitation, lerpCoeff);
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainSize =
                    math.lerp(rainEffectProperty.rainSize, weatherDefine.rainEffectProperty.rainSize, lerpCoeff);
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainDropLength =
                    math.lerp(rainEffectProperty.rainDropLength, weatherDefine.rainEffectProperty.rainDropLength, lerpCoeff);
            }
            
            if (WorldManager.Instance.weatherEffectModule.snowEffect is not null)
            {
                WorldManager.Instance.weatherEffectModule.snowEffect.property.snowPrecipitation =
                    math.lerp(snowEffectProperty.snowPrecipitation, weatherDefine.snowEffectProperty.snowPrecipitation, lerpCoeff);
                WorldManager.Instance.weatherEffectModule.snowEffect.property.snowSize =
                    math.lerp(snowEffectProperty.snowSize, weatherDefine.snowEffectProperty.snowSize, lerpCoeff);
            }
            
            if (WorldManager.Instance.weatherEffectModule.lightningEffect is not null)
            {
                WorldManager.Instance.weatherEffectModule.lightningEffect.property.lightningSpawnRate =
                    math.lerp(lightningEffectProperty.lightningSpawnRate, weatherDefine.lightningEffectProperty.lightningSpawnRate, lerpCoeff);
            }
            
        }
    
        #endregion
    }
    
    
    
}