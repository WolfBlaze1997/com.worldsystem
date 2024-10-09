using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace WorldSystem.Runtime
{
    
    [CreateAssetMenu(fileName = "天气定义", menuName = "世界系统/天气定义")]
    [Serializable]
    public partial class WeatherDefine : ScriptableObject
    {
        
        #region 字段

        [HideInInspector]
        public bool isActive;

        [ShowInInspector] [HorizontalGroup("Split",0.3f)] [ToggleLeft] [HideLabel] [PropertyOrder(-100)]
        public bool IsActive
        {
            get => isActive;
            set
            {
                List<WeatherDefine> weatherDefineList = WorldManager.Instance?.weatherListModule?.weatherList?.weatherList;
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
                            VARIABLE._modifTime = false;
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
                        WorldManager.Instance.weatherListModule.weatherListIndex = weatherDefineList.IndexOf(this);
                    }
#endif
                    //一旦设置Active, 说明必定退出插值了, 将CelestialBody.useLerp 设置为false
                    CelestialBody.UseLerp = false;
                    ApproxRealtimeGIModule.UseLerp = false;
                    FogModule.UseLerp = false;
                    LightingScatterModule.UseLerp = false;
                    WindZoneModule.useLerp = false;
                    PostprocessAdjustModule.UseLerp = false;
                    VolumeCloudOptimizeModule.UseLerp = false;
                    _celestialBodySingleExecute = true;
                    _approxRealtimeGISingleExecute = true;
                    _fogModuleSingleExecute = true;
                    _lightingScatterSingleExecute = true;
                    _volumeCloudSingleExecute = true;
                    _windZoneSingleExecute = true;
                    _postprocessAdjustSingleExecute = true;
                }
            }
        }

#if UNITY_EDITOR
        [HorizontalGroup("Split",0.05f)][ShowInInspector] [EnableIf("isActive")][HideLabel][ToggleLeft]
        private bool _modifTime;
#endif
        
        [HorizontalGroup("Split")] [LabelText("持续时间")] [EnableIf("@isActive && _modifTime")]
        public float sustainedTime;

        [HorizontalGroup("Split")] [LabelText("切换时间")] [EnableIf("@isActive && _modifTime")]
        public float varyingTime;

        [HideInInspector] public float sustainedTimeCache;
        
        [HideInInspector] public float varyingTimeCache;

        #endregion
        
        
        #region 事件函数
        
        private void OnValidate()
        {
            if (!isActive) return;
            
#if UNITY_EDITOR
            if (_modifTime)
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
                UniformStaticProperty_UniverseBackgroundModule(item);
                UniformStaticProperty_StarModule(item);
                UniformStaticProperty_CelestialBodyManager(item);
                UniformStaticProperty_AtmosphereModule(item);
                UniformStaticProperty_VolumeCloudOptimizeModule(item);
                UniformStaticProperty_WindZoneModule(item);
                UniformStaticProperty_WeatherEffectModule(item);
                UniformStaticProperty_MoistAccumulatedWaterModule(item);
                UniformStaticProperty_ApproxRealtimeGIModule(item);
                UniformStaticProperty_FogModule(item);
                UniformStaticProperty_LightingScatterModule(item);
                UniformStaticProperty_PostprocessAdjustModule(item);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(item);
#endif
            }
            SetupProperty();

        }

#if UNITY_EDITOR
        //修正参数
        private void Awake()
        {
            WeatherDefine[] allWeatherDefine = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            WeatherDefine activeWeatherDefine = allWeatherDefine.ToList().Find(o => o.isActive);
        }
#endif
        
        
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
            SetupProperty_MoistAccumulatedWaterModule();
            SetupProperty_ApproxRealtimeGIModule();
            SetupProperty_FogModule();
            SetupProperty_LightingScatterModule();
            SetupProperty_PostprocessAdjustModule();
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
            SetupLerpProperty_MoistAccumulatedWaterModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_ApproxRealtimeGIModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_FogModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_LightingScatterModule(weatherDefine, lerpCoeff);
            SetupLerpProperty_PostprocessAdjustModule(weatherDefine, lerpCoeff);
        }

#if UNITY_EDITOR
        public static void UpdateDynamicDisplayProperty()
        {
            //仅显示信息
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                UpdateDynamicDisplayProperty_FogModule(VARIABLE);
                UpdateDynamicDisplayProperty_VolumeCloudOptimizeModule(VARIABLE);
                UpdateDynamicDisplayProperty_LightingScatterModule(VARIABLE);
                UpdateDynamicDisplayProperty_WindZoneModule(VARIABLE);
                UpdateDynamicDisplayProperty_WeatherEffectModule(VARIABLE);
                UpdateDynamicDisplayProperty_PostprocessAdjustModule(VARIABLE);
                UpdateDynamicDisplayProperty_CelestialBodyManager(VARIABLE);
            }
        }
#endif
        
        #endregion
        
    }

    
    /// <summary>
    /// 宇宙背景模块
    /// </summary>
    public partial class WeatherDefine
    {
        [FoldoutGroup("昼夜与天气")] [FoldoutGroup("昼夜与天气/渲染设置与背景")] [HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.universeBackgroundModuleToggle")]
        public UniverseBackgroundModule.Property backgroundProperty = new();
        
        public void SetupProperty_UniverseBackgroundModule()
        {
            if (WorldManager.Instance?.universeBackgroundModule is null)
                return;
            WorldManager.Instance.universeBackgroundModule.property.renderResolutionOptions = backgroundProperty.renderResolutionOptions;
            WorldManager.Instance.universeBackgroundModule.property.renderTemporalAAFactor = backgroundProperty.renderTemporalAAFactor;
            WorldManager.Instance.universeBackgroundModule.property.renderUseAsyncRender = backgroundProperty.renderUseAsyncRender;
            WorldManager.Instance.universeBackgroundModule.property.renderAsyncFOV = backgroundProperty.renderAsyncFOV;
            WorldManager.Instance.universeBackgroundModule.property.renderAsyncUpdateRate = backgroundProperty.renderAsyncUpdateRate;
            WorldManager.Instance.universeBackgroundModule.property.renderTargetFps = backgroundProperty.renderTargetFps;
            WorldManager.Instance.universeBackgroundModule.OnValidate();
            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.backgroundProperty.skyMesh = WorldManager.Instance.universeBackgroundModule.property.skyMesh;
                VARIABLE.backgroundProperty.backgroundShader = WorldManager.Instance.universeBackgroundModule.property.backgroundShader;
                VARIABLE.backgroundProperty.backgroundMaterial = WorldManager.Instance.universeBackgroundModule.property.backgroundMaterial;
                VARIABLE.backgroundProperty.taaShader = WorldManager.Instance.universeBackgroundModule.property.taaShader;
                VARIABLE.backgroundProperty.taaMaterial = WorldManager.Instance.universeBackgroundModule.property.taaMaterial;

            }
#endif
        }
        
        public void UniformStaticProperty_UniverseBackgroundModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.universeBackgroundModule is null)
                return;
            item.backgroundProperty.renderResolutionOptions = backgroundProperty.renderResolutionOptions;
            item.backgroundProperty.renderTemporalAAFactor = backgroundProperty.renderTemporalAAFactor;
            item.backgroundProperty.renderUseAsyncRender = backgroundProperty.renderUseAsyncRender;
            item.backgroundProperty.renderAsyncFOV = backgroundProperty.renderAsyncFOV;
            item.backgroundProperty.renderAsyncUpdateRate = backgroundProperty.renderAsyncUpdateRate;
            item.backgroundProperty.renderTargetFps = backgroundProperty.renderTargetFps;
        }
        
    }
    
    
    /// <summary>
    /// 星星模块
    /// </summary>
    public partial class WeatherDefine
    {
        
        [FoldoutGroup("昼夜与天气/星星模块")][HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.starModuleToggle")]
        public StarModule.Property starProperty = new();
        
        public void SetupProperty_StarModule()
        {
            if (WorldManager.Instance?.starModule is null)
                return;
            starProperty.LimitProperty();
            WorldManager.Instance.starModule.property.count = starProperty.count;
            WorldManager.Instance.starModule.property.size = starProperty.size;
            WorldManager.Instance.starModule.property.automaticColor = starProperty.automaticColor;
            WorldManager.Instance.starModule.property.automaticBrightness = starProperty.automaticBrightness;
            WorldManager.Instance.starModule.property.starColor = starProperty.starColor;
            WorldManager.Instance.starModule.property.brightness = starProperty.brightness;
            WorldManager.Instance.starModule.property.flickerFrequency = starProperty.flickerFrequency;
            WorldManager.Instance.starModule.property.flickerStrength = starProperty.flickerStrength;
            WorldManager.Instance.starModule.property.initialSeed = starProperty.initialSeed;
            WorldManager.Instance.starModule.property.inclination = starProperty.inclination;

            WorldManager.Instance.starModule.OnValidate();

#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.starProperty.starMesh = WorldManager.Instance.starModule.property.starMesh;
                VARIABLE.starProperty.starShader = WorldManager.Instance.starModule.property.starShader;
                VARIABLE.starProperty.starMaterial = WorldManager.Instance.starModule.property.starMaterial;
                VARIABLE.starProperty.starTexture = WorldManager.Instance.starModule.property.starTexture;
            }
#endif
        }

        public void UniformStaticProperty_StarModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.starModule is null)
                return;
            item.starProperty.count = starProperty.count;
            item.starProperty.automaticColor = starProperty.automaticColor;
            item.starProperty.automaticBrightness = starProperty.automaticBrightness;
            item.starProperty.starColor = starProperty.starColor;
            item.starProperty.flickerFrequency = starProperty.flickerFrequency;
            item.starProperty.flickerStrength = starProperty.flickerStrength;
            item.starProperty.initialSeed = starProperty.initialSeed;
            item.starProperty.inclination = starProperty.inclination;
        }

        public void SetupLerpProperty_StarModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.starModule is null)
                return;
            WorldManager.Instance.starModule.property.size = math.lerp(starProperty.size, weatherDefine.starProperty.size, lerpCoeff);
            WorldManager.Instance.starModule.property.brightness = math.lerp(starProperty.brightness, weatherDefine.starProperty.brightness, lerpCoeff);
        }
    }


    /// <summary>
    /// 星体模块
    /// </summary>
    public partial class WeatherDefine
    {
        
        #region 字段
        
        [FoldoutGroup("昼夜与天气/星体模块")][LabelText("星体列表")]
        [ListDrawerSettings(CustomAddFunction = "CreateCelestialBody", CustomRemoveIndexFunction = "DestroyCelestialBody",OnTitleBarGUI = "DrawRefreshButton")]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.celestialBodyManagerToggle")]
        public List<CelestialBody.Property> celestialBodyList;
        
        [FoldoutGroup("昼夜与天气/星体模块")][LabelText("星体数量限制")] [ShowIf("@WorldManager.Instance?.celestialBodyManagerToggle")][ReadOnly]
        public int maxCelestialBodyCount = 4;
        
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
                
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.useLensFlare = celestialBodyList[i].useLensFlare;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lensFlareStrength = celestialBodyList[i].lensFlareStrength;
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lensFlareScale = celestialBodyList[i].lensFlareScale;

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
                        VARIABLE.celestialBodyList[i].geocentricTheory = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.geocentricTheory;
                        VARIABLE.celestialBodyList[i].skyObjectMesh = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.skyObjectMesh;
                        VARIABLE.celestialBodyList[i].shader = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.shader;
                        VARIABLE.celestialBodyList[i].material = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.material;
                        VARIABLE.celestialBodyList[i].lightComponent = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lightComponent;
                        VARIABLE.celestialBodyList[i].lensFlareData = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lensFlareData;
                        VARIABLE.celestialBodyList[i].lensFlare = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lensFlare;

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
                    item.celestialBodyList.Add(new CelestialBody.Property());
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
                
                item.celestialBodyList[i].useLensFlare = celestialBodyList[i].useLensFlare;
                item.celestialBodyList[i].lensFlareScale = celestialBodyList[i].lensFlareScale;
                item.celestialBodyList[i].lensFlareData = celestialBodyList[i].lensFlareData;
                item.celestialBodyList[i].lensFlare = celestialBodyList[i].lensFlare;

            }
            
        }
        
        private static bool _celestialBodySingleExecute = true;

        public void SetupLerpProperty_CelestialBodyManager(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.celestialBodyManager is null) 
                return;
            //当进入插值时将 CelestialBody.useLerp 设置为true,停止星体的属性的根据昼夜的变换, 而是由外部多个天气进行插值
            CelestialBody.UseLerp = true;
            
            for (int i = 0; i < celestialBodyList.Count; i++)
            {
                //只执行一次
                if (_celestialBodySingleExecute)
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
                    WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lensFlareStrength =
                        weatherDefine.celestialBodyList[i].lensFlareStrength;
                }

                //根据昼夜和两个天气之间进行插值
                var curveTime = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.executeCoeff;
                
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.objectColorExecute = 
                    Color.Lerp(celestialBodyList[i].objectColor.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].objectColor.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.falloffExecute = 
                    math.lerp(celestialBodyList[i].falloff.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].falloff.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lightingColorMaskExecute = 
                    Color.Lerp(celestialBodyList[i].lightingColorMask.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].lightingColorMask.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.colorTemperatureCurveExecute = 
                    math.lerp(celestialBodyList[i].colorTemperatureCurve.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].colorTemperatureCurve.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.intensityCurveExecute = 
                    math.lerp(celestialBodyList[i].intensityCurve.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].intensityCurve.Evaluate(curveTime), lerpCoeff);
                WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.lensFlareStrengthExecute = 
                    math.lerp(celestialBodyList[i].lensFlareStrength.Evaluate(curveTime),
                        weatherDefine.celestialBodyList[i].lensFlareStrength.Evaluate(curveTime), lerpCoeff);
                
            }
            _celestialBodySingleExecute = false;
        }
        
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_CelestialBodyManager(WeatherDefine variable)
        {
            if(WorldManager.Instance.celestialBodyManager == null)
                return;
            
            for (var i = 0; i < variable.celestialBodyList.Count; i++)
            {
                variable.celestialBodyList[i].executeCoeff = WorldManager.Instance.celestialBodyManager.property.celestialBodyList[i].property.executeCoeff;
            }
            
            foreach (var VARIABLE in variable.celestialBodyList)
            {
                VARIABLE.ExecuteProperty();
            }
        }
#endif
        
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
        
        [FoldoutGroup("昼夜与天气/大气模块")] [HideLabel] [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.atmosphereModuleToggle")]
        public AtmosphereModuleProperty atmosphereProperty = new();
        
        #endregion
        
        public void SetupProperty_AtmosphereModule()
        {
            if (WorldManager.Instance?.atmosphereModule is null)
                return;
            WorldManager.Instance.atmosphereModule.property.dayPeriodsList =
                new List<AtmosphereModule.DayPeriods>(atmosphereProperty.dayPeriodsList);
            
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
        }
        
    }


    /// <summary>
    /// 体积云模块
    /// </summary>
    public partial class WeatherDefine
    {
        
        #region 体积云模块

        [FoldoutGroup("昼夜与天气/体积云模块")] [HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModuleToggle")]
        public VolumeCloudOptimizeModule.Property cloudProperty = new();
        
        private static bool _volumeCloudSingleExecute;
        
        #endregion


        public void SetupProperty_VolumeCloudOptimizeModule()
        {
            if (WorldManager.Instance?.volumeCloudOptimizeModule is null)
                return;
            cloudProperty.LimitProperty();
            WorldManager.Instance.volumeCloudOptimizeModule.property.renderMaxRenderDistance = cloudProperty.renderMaxRenderDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property.renderCoarseSteps = cloudProperty.renderCoarseSteps;
            WorldManager.Instance.volumeCloudOptimizeModule.property.renderDetailSteps = cloudProperty.renderDetailSteps;
            WorldManager.Instance.volumeCloudOptimizeModule.property.renderBlueNoise = cloudProperty.renderBlueNoise;
            WorldManager.Instance.volumeCloudOptimizeModule.property.renderMipmapDistance = cloudProperty.renderMipmapDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountCloudAmount = cloudProperty.modelingAmountCloudAmount;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountUseFarOverlay =
                cloudProperty.modelingAmountUseFarOverlay;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountOverlayStartDistance =
                cloudProperty.modelingAmountOverlayStartDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountOverlayCloudAmount =
                cloudProperty.modelingAmountOverlayCloudAmount;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeBaseOctaves = cloudProperty.modelingShapeBaseOctaves;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeBaseGain = cloudProperty.modelingShapeBaseGain;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeBaseFreq = cloudProperty.modelingShapeBaseFreq;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeBaseScale = cloudProperty.modelingShapeBaseScale;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeDetailType = cloudProperty.modelingShapeDetailType;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeDetailQuality =
                cloudProperty.modelingShapeDetailQuality;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeDetailScale = cloudProperty.modelingShapeDetailScale;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionRadiusPreset =
                cloudProperty.modelingPositionRadiusPreset;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionCloudHeight =
                cloudProperty.modelingPositionCloudHeight;
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionCloudThickness =
                cloudProperty.modelingPositionCloudThickness;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseUseDynamicCloud = cloudProperty.motionBaseUseDynamicCloud;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseWindSpeedCoeff = cloudProperty.motionBaseWindSpeedCoeff;

            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseDirection = cloudProperty.motionBaseDirection;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseSpeed = cloudProperty.motionBaseSpeed;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseUseDirectionRandom =
                cloudProperty.motionBaseUseDirectionRandom;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseDirectionRandomRange =
                cloudProperty.motionBaseDirectionRandomRange;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseDirectionRandomFreq =
                cloudProperty.motionBaseDirectionRandomFreq;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailDirection = cloudProperty.motionDetailDirection;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailSpeed = cloudProperty.motionDetailSpeed;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailUseRandomDirection =
                cloudProperty.motionDetailUseRandomDirection;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailDirectionRandomRange =
                cloudProperty.motionDetailDirectionRandomRange;
            WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailDirectionRandomFreq =
                cloudProperty.motionDetailDirectionRandomFreq;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingAlbedoColor = cloudProperty.lightingAlbedoColor;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingLightColorFilter = cloudProperty.lightingLightColorFilter;
            
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingExtinctionCoeff = cloudProperty.lightingExtinctionCoeff;
            
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingDensityInfluence = cloudProperty.lightingDensityInfluence;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHeightDensityInfluence =
                cloudProperty.lightingHeightDensityInfluence;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingCheapAmbient = cloudProperty.lightingCheapAmbient;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingAmbientExposure = cloudProperty.lightingAmbientExposure;
            // WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_UseAtmosphereVisibilityOverlay =
            //     cloudProperty._Lighting_UseAtmosphereVisibilityOverlay;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingAtmosphereVisibility =
                cloudProperty.lightingAtmosphereVisibility;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHgStrength = cloudProperty.lightingHgStrength;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHgEccentricityForward =
                cloudProperty.lightingHgEccentricityForward;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHgEccentricityBackward =
                cloudProperty.lightingHgEccentricityBackward;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingMaxLightingDistance =
                cloudProperty.lightingMaxLightingDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingShadingStrengthFalloff =
                cloudProperty.lightingShadingStrengthFalloff;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingScatterMultiplier = cloudProperty.lightingScatterMultiplier;
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingScatterStrength = cloudProperty.lightingScatterStrength;
            WorldManager.Instance.volumeCloudOptimizeModule.property.shadowUseCastShadow = cloudProperty.shadowUseCastShadow;
            WorldManager.Instance.volumeCloudOptimizeModule.property.shadowDistance = cloudProperty.shadowDistance;
            WorldManager.Instance.volumeCloudOptimizeModule.property.shadowStrength = cloudProperty.shadowStrength;
            WorldManager.Instance.volumeCloudOptimizeModule.property.shadowResolution = cloudProperty.shadowResolution;
            WorldManager.Instance.volumeCloudOptimizeModule.property.shadowUseShadowTaa = cloudProperty.shadowUseShadowTaa;
            if (cloudProperty.modelingPositionRadiusPreset == VolumeCloudOptimizeModule.CelestialBodySelection.Custom)
                WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionPlanetRadius =
                    cloudProperty.modelingPositionPlanetRadius;

            WorldManager.Instance.volumeCloudOptimizeModule.OnValidate();

            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.cloudProperty.volumeCloudBaseTexShader = WorldManager.Instance.volumeCloudOptimizeModule.property.volumeCloudBaseTexShader;
                VARIABLE.cloudProperty.volumeCloudBaseTexMaterial = WorldManager.Instance.volumeCloudOptimizeModule.property.volumeCloudBaseTexMaterial;
                VARIABLE.cloudProperty.volumeCloudMainShader = WorldManager.Instance.volumeCloudOptimizeModule.property.volumeCloudMainShader;
                VARIABLE.cloudProperty.volumeCloudMainMaterial = WorldManager.Instance.volumeCloudOptimizeModule.property.volumeCloudMainMaterial;
                VARIABLE.cloudProperty.modelingShapeDetailNoiseTexture3D = WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeDetailNoiseTexture3D;
                VARIABLE.cloudProperty.motionBaseDynamicVector = WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseDynamicVector;
                VARIABLE.cloudProperty.motionDetailDynamicVector = WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailDynamicVector;
                VARIABLE.cloudProperty.cloudShadowsTemporalAAShader = WorldManager.Instance.volumeCloudOptimizeModule.property.cloudShadowsTemporalAAShader;
                VARIABLE.cloudProperty.cloudShadowsTemporalAAMaterial = WorldManager.Instance.volumeCloudOptimizeModule.property.cloudShadowsTemporalAAMaterial;
                VARIABLE.cloudProperty.cloudShadowsScreenShadowShader = WorldManager.Instance.volumeCloudOptimizeModule.property.cloudShadowsScreenShadowShader;
                VARIABLE.cloudProperty.cloudShadowsScreenShadowMaterial = WorldManager.Instance.volumeCloudOptimizeModule.property.cloudShadowsScreenShadowMaterial;
                
                if (VARIABLE.cloudProperty.modelingPositionRadiusPreset != VolumeCloudOptimizeModule.CelestialBodySelection.Custom)
                    VARIABLE.cloudProperty.modelingPositionPlanetRadius = WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionPlanetRadius;
                
            }
#endif
            
        }

        private void UniformStaticProperty_VolumeCloudOptimizeModule(WeatherDefine item)
        {
            item.cloudProperty.renderMaxRenderDistance = cloudProperty.renderMaxRenderDistance;
            item.cloudProperty.renderCoarseSteps = cloudProperty.renderCoarseSteps;
            item.cloudProperty.renderDetailSteps = cloudProperty.renderDetailSteps;
            item.cloudProperty.renderBlueNoise = cloudProperty.renderBlueNoise;
            item.cloudProperty.renderMipmapDistance = cloudProperty.renderMipmapDistance;
            item.cloudProperty.modelingAmountUseFarOverlay = cloudProperty.modelingAmountUseFarOverlay;
            item.cloudProperty.modelingShapeBaseOctaves = cloudProperty.modelingShapeBaseOctaves;
            item.cloudProperty.modelingShapeBaseFreq = cloudProperty.modelingShapeBaseFreq;
            item.cloudProperty.modelingShapeDetailNoiseTexture3D = cloudProperty.modelingShapeDetailNoiseTexture3D;
            item.cloudProperty.modelingShapeDetailType = cloudProperty.modelingShapeDetailType;
            item.cloudProperty.modelingShapeDetailQuality = cloudProperty.modelingShapeDetailQuality;
            item.cloudProperty.modelingPositionRadiusPreset = cloudProperty.modelingPositionRadiusPreset;
            item.cloudProperty.modelingPositionPlanetRadius = cloudProperty.modelingPositionPlanetRadius;
            item.cloudProperty.motionBaseUseDynamicCloud = cloudProperty.motionBaseUseDynamicCloud;
            item.cloudProperty.motionBaseWindSpeedCoeff = cloudProperty.motionBaseWindSpeedCoeff;
            item.cloudProperty.motionBaseDynamicVector = cloudProperty.motionBaseDynamicVector;
            item.cloudProperty.motionBaseDirection = cloudProperty.motionBaseDirection;
            item.cloudProperty.motionBaseUseDirectionRandom = cloudProperty.motionBaseUseDirectionRandom;
            item.cloudProperty.motionBaseDirectionRandomRange = cloudProperty.motionBaseDirectionRandomRange;
            item.cloudProperty.motionBaseDirectionRandomFreq = cloudProperty.motionBaseDirectionRandomFreq;
            item.cloudProperty.motionDetailDynamicVector = cloudProperty.motionDetailDynamicVector;
            item.cloudProperty.motionDetailDirection = cloudProperty.motionDetailDirection;
            item.cloudProperty.motionDetailUseRandomDirection = cloudProperty.motionDetailUseRandomDirection;
            item.cloudProperty.motionDetailDirectionRandomRange = cloudProperty.motionDetailDirectionRandomRange;
            item.cloudProperty.motionDetailDirectionRandomFreq = cloudProperty.motionDetailDirectionRandomFreq;
            item.cloudProperty.lightingCheapAmbient = cloudProperty.lightingCheapAmbient;
            // item.cloudProperty._Lighting_UseAtmosphereVisibilityOverlay = cloudProperty._Lighting_UseAtmosphereVisibilityOverlay;
            item.cloudProperty.shadowUseCastShadow = cloudProperty.shadowUseCastShadow;
            item.cloudProperty.shadowDistance = cloudProperty.shadowDistance;
            item.cloudProperty.shadowStrength = cloudProperty.shadowStrength;
            item.cloudProperty.shadowResolution = cloudProperty.shadowResolution;
            item.cloudProperty.shadowUseShadowTaa = cloudProperty.shadowUseShadowTaa;
        }

        public void SetupLerpProperty_VolumeCloudOptimizeModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.volumeCloudOptimizeModule is null)
                return;
            
            //当进入插值时将 useLerp 设置为true,停止属性的根据昼夜的变换, 而是由外部多个天气进行插值
            VolumeCloudOptimizeModule.UseLerp = true;
            
            //只执行一次
            if (_volumeCloudSingleExecute)
            {
                //输入下一个天气的动态属性
                WorldManager.Instance.volumeCloudOptimizeModule.property.lightingExtinctionCoeff = weatherDefine.cloudProperty.lightingExtinctionCoeff;
            }
            
            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingExtinctionCoeffExecute = 
                math.lerp(cloudProperty.lightingExtinctionCoeff.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.cloudProperty.lightingExtinctionCoeff.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            
            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountCloudAmount =
                math.lerp(cloudProperty.modelingAmountCloudAmount, weatherDefine.cloudProperty.modelingAmountCloudAmount, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountOverlayStartDistance =
                math.lerp(cloudProperty.modelingAmountOverlayStartDistance, weatherDefine.cloudProperty.modelingAmountOverlayStartDistance,
                    lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingAmountOverlayCloudAmount =
                math.lerp(cloudProperty.modelingAmountOverlayCloudAmount, weatherDefine.cloudProperty.modelingAmountOverlayCloudAmount,
                    lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeBaseGain =
                math.lerp(cloudProperty.modelingShapeBaseGain, weatherDefine.cloudProperty.modelingShapeBaseGain, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeBaseScale =
                math.lerp(cloudProperty.modelingShapeBaseScale, weatherDefine.cloudProperty.modelingShapeBaseScale, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingShapeDetailScale =
                math.lerp(cloudProperty.modelingShapeDetailScale, weatherDefine.cloudProperty.modelingShapeDetailScale, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionCloudHeight =
                math.lerp(cloudProperty.modelingPositionCloudHeight, weatherDefine.cloudProperty.modelingPositionCloudHeight, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionCloudThickness =
                math.lerp(cloudProperty.modelingPositionCloudThickness, weatherDefine.cloudProperty.modelingPositionCloudThickness,
                    lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.motionBaseSpeed =
                math.lerp(cloudProperty.motionBaseSpeed, weatherDefine.cloudProperty.motionBaseSpeed, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.motionDetailSpeed =
                math.lerp(cloudProperty.motionDetailSpeed, weatherDefine.cloudProperty.motionDetailSpeed, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingAlbedoColor =
                Color.Lerp(cloudProperty.lightingAlbedoColor, weatherDefine.cloudProperty.lightingAlbedoColor, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingLightColorFilter =
                Color.Lerp(cloudProperty.lightingLightColorFilter, weatherDefine.cloudProperty.lightingLightColorFilter, lerpCoeff);

            // WorldManager.Instance.volumeCloudOptimizeModule.property._Lighting_ExtinctionCoeff_Execute =
            //     math.lerp(cloudProperty._Lighting_ExtinctionCoeff_Execute, weatherDefine.cloudProperty._Lighting_ExtinctionCoeff_Execute, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingDensityInfluence =
                math.lerp(cloudProperty.lightingDensityInfluence, weatherDefine.cloudProperty.lightingDensityInfluence, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHeightDensityInfluence =
                math.lerp(cloudProperty.lightingHeightDensityInfluence, weatherDefine.cloudProperty.lightingHeightDensityInfluence, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingAmbientExposure =
                math.lerp(cloudProperty.lightingAmbientExposure, weatherDefine.cloudProperty.lightingAmbientExposure, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingAtmosphereVisibility =
                math.lerp(cloudProperty.lightingAtmosphereVisibility, weatherDefine.cloudProperty.lightingAtmosphereVisibility, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHgStrength =
                math.lerp(cloudProperty.lightingHgStrength, weatherDefine.cloudProperty.lightingHgStrength, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHgEccentricityForward =
                math.lerp(cloudProperty.lightingHgEccentricityForward, weatherDefine.cloudProperty.lightingHgEccentricityForward, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingHgEccentricityBackward =
                math.lerp(cloudProperty.lightingHgEccentricityBackward, weatherDefine.cloudProperty.lightingHgEccentricityBackward, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingMaxLightingDistance =
                (int)math.lerp(cloudProperty.lightingMaxLightingDistance, weatherDefine.cloudProperty.lightingMaxLightingDistance, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingShadingStrengthFalloff =
                math.lerp(cloudProperty.lightingShadingStrengthFalloff, weatherDefine.cloudProperty.lightingShadingStrengthFalloff, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingScatterMultiplier =
                math.lerp(cloudProperty.lightingScatterMultiplier, weatherDefine.cloudProperty.lightingScatterMultiplier, lerpCoeff);

            WorldManager.Instance.volumeCloudOptimizeModule.property.lightingScatterStrength =
                math.lerp(cloudProperty.lightingScatterStrength, weatherDefine.cloudProperty.lightingScatterStrength, lerpCoeff);

            _volumeCloudSingleExecute = false;
        }
        
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_VolumeCloudOptimizeModule(WeatherDefine variable)
        {
            variable.cloudProperty.ExecuteProperty();
        }
#endif
        
        
    }


    /// <summary>
    /// 风场模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 风场模块
        
        [FoldoutGroup("昼夜与天气/风场模块")] [HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.windZoneModuleToggle")]
        public WindZoneModule.Property windZoneProperty = new();

        #endregion

        private static bool _windZoneSingleExecute = true;

        public void SetupProperty_WindZoneModule()
        {
            if (WorldManager.Instance?.windZoneModule is null)
                return;
            windZoneProperty.LimitProperty();
            WorldManager.Instance.windZoneModule.property.dynamicMode = windZoneProperty.dynamicMode;
            WorldManager.Instance.windZoneModule.property.directionRotateCurve = windZoneProperty.directionRotateCurve;
            WorldManager.Instance.windZoneModule.property.WindSpeedCurve = windZoneProperty.WindSpeedCurve;

            WorldManager.Instance.windZoneModule.property.directionVaryingFreq = windZoneProperty.directionVaryingFreq;
            WorldManager.Instance.windZoneModule.property.SpeedVaryingFreq = windZoneProperty.SpeedVaryingFreq;
            WorldManager.Instance.windZoneModule.property.minSpeed = windZoneProperty.minSpeed;
            WorldManager.Instance.windZoneModule.property.maxSpeed = windZoneProperty.maxSpeed;

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
            item.windZoneProperty.directionRotateCurve = windZoneProperty.directionRotateCurve;
            item.windZoneProperty.directionVaryingFreq = windZoneProperty.directionVaryingFreq;
            item.windZoneProperty.SpeedVaryingFreq = windZoneProperty.SpeedVaryingFreq;
            item.windZoneProperty.windZone = windZoneProperty.windZone;
        }

        public void SetupLerpProperty_WindZoneModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.windZoneModule is null)
                return;
            
            WindZoneModule.useLerp = true;
            
            if (WorldManager.Instance.windZoneModule.property.dynamicMode == WindZoneModule.DynamicMode.RandomMode)
            {
                WorldManager.Instance.windZoneModule.property.minSpeed =
                    math.lerp(windZoneProperty.minSpeed, weatherDefine.windZoneProperty.minSpeed, lerpCoeff);
                WorldManager.Instance.windZoneModule.property.maxSpeed =
                    math.lerp(windZoneProperty.maxSpeed, weatherDefine.windZoneProperty.maxSpeed, lerpCoeff);
            }
            else
            {
                if (_windZoneSingleExecute)
                {
                    //输入下一个天气的动态属性
                    WorldManager.Instance.windZoneModule.property.WindSpeedCurve = weatherDefine.windZoneProperty.WindSpeedCurve;
                }
                WorldManager.Instance.windZoneModule.property.WindSpeedCurve_Execute = 
                    math.lerp(windZoneProperty.WindSpeedCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                        weatherDefine.windZoneProperty.WindSpeedCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);

                _windZoneSingleExecute = false;
            }
        }
        
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_WindZoneModule(WeatherDefine variable)
        {
            variable.windZoneProperty.ExecuteProperty();
        }
        
#endif
        
    }

    
    /// <summary>
    /// 天气特效模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 天气特效模块
        
#if UNITY_EDITOR
        [ShowIf("@weatherEffectProperty.rainEnable && WorldManager.Instance?.weatherEffectModuleToggle")]
        [FoldoutGroup("昼夜与天气/天气特效模块")]
        [HorizontalGroup("昼夜与天气/天气特效模块/Split01")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块"), GUIColor(0.5f, 0.5f, 1f)]
        [EnableIf("isActive")]
        public void RainToggle_Off()
        {
            weatherEffectProperty.rainEnable = false;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.RainToggle_Off();
        }
    
        [HideIf("@weatherEffectProperty.rainEnable || !WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        [EnableIf("isActive")]
        public void RainToggle_On()
        {
            weatherEffectProperty.rainEnable = true;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.RainToggle_On();
        }
        
        [ShowIf("@weatherEffectProperty.rainSpatterEnable && WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/02")]
        [EnableIf("isActive")]
        [Button(ButtonSizes.Medium, Name = "雨滴飞溅模块"), GUIColor(0.5f, 0.5f, 1f)]
        public void RainSpatterToggle_Off()
        {
            weatherEffectProperty.rainSpatterEnable = false;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.RainSpatterToggle_Off();
        }
        [HideIf("@weatherEffectProperty.rainSpatterEnable || !WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/02")]
        [EnableIf("isActive")]
        [Button(ButtonSizes.Medium, Name = "雨滴飞溅模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void RainSpatterToggle_On()
        {
            weatherEffectProperty.rainSpatterEnable = true;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.RainSpatterToggle_On();
        }
        
        [ShowIf("@weatherEffectProperty.snowEnable && WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/03")]
        [Button(ButtonSizes.Medium, Name = "雪模块"), GUIColor(0.5f, 0.5f, 1f)]
        [EnableIf("isActive")]
        public void SnowToggle_Off()
        {
            weatherEffectProperty.snowEnable = false;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.SnowToggle_Off();
        }
    
        [HideIf("@weatherEffectProperty.snowEnable || !WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/03")]
        [Button(ButtonSizes.Medium, Name = "雪模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        [EnableIf("isActive")]
        public void SnowToggle_On()
        {
            weatherEffectProperty.snowEnable = true;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.SnowToggle_On();
        }
    
        [ShowIf("@weatherEffectProperty.lightningEnable && WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/04")]
        [Button(ButtonSizes.Medium, Name = "闪电模块"), GUIColor(0.5f, 0.5f, 1f)]
        [EnableIf("isActive")]
        public void LightningToggle_Off()
        {
            weatherEffectProperty.lightningEnable = false;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.LightningToggle_Off();
        }
    
        [HideIf("@weatherEffectProperty.lightningEnable || !WorldManager.Instance?.weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/天气特效模块/Split01/04")]
        [Button(ButtonSizes.Medium, Name = "闪电模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        [EnableIf("isActive")]
        public void LightningToggle_On()
        {
            weatherEffectProperty.lightningEnable = true;
            OnValidate();
            WorldManager.Instance?.weatherEffectModule?.LightningToggle_On();
        }
#endif
        
        
        [FoldoutGroup("昼夜与天气/天气特效模块")][HideLabel]
        [EnableIf("isActive")][PropertyOrder(1)]
        public WeatherEffectModule.Property weatherEffectProperty = new();
        
        [FoldoutGroup("昼夜与天气/天气特效模块/雨")][HideLabel]
        [EnableIf("isActive")][ShowIf("@weatherEffectProperty.rainEnable")][PropertyOrder(1)]
        public VFXRainEffect.Property rainEffectProperty = new();
        
        [FoldoutGroup("昼夜与天气/天气特效模块/雨滴飞溅")][HideLabel]
        [EnableIf("isActive")][ShowIf("@weatherEffectProperty.rainSpatterEnable")][PropertyOrder(1)]
        public VFXRainSpatterEffect.Property rainSpatterEffectProperty = new();
        
        [FoldoutGroup("昼夜与天气/天气特效模块/雪")][HideLabel]
        [EnableIf("isActive")][ShowIf("@weatherEffectProperty.snowEnable")][PropertyOrder(1)]
        public VFXSnowEffect.Property snowEffectProperty = new();
        
        [FoldoutGroup("昼夜与天气/天气特效模块/闪电")][HideLabel]
        [EnableIf("isActive")][ShowIf("@weatherEffectProperty.lightningEnable")][PropertyOrder(1)]
        public VFXLightningEffect.Property lightningEffectProperty = new();
        
        [FoldoutGroup("昼夜与天气/天气特效模块/遮蔽渲染器")]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [EnableIf("isActive")][ShowIf("@weatherEffectProperty.useOcclusion")][PropertyOrder(1)]
        public OcclusionRenderer occlusionRenderer;
        

    
        public void SetupProperty_WeatherEffectModule()
        {
            if (WorldManager.Instance?.weatherEffectModule is null)
                return;
            weatherEffectProperty.LimitProperty();
            WorldManager.Instance.weatherEffectModule.property.useOcclusion = weatherEffectProperty.useOcclusion;
            WorldManager.Instance.weatherEffectModule.property.rainEnable = weatherEffectProperty.rainEnable;
            WorldManager.Instance.weatherEffectModule.property.rainSpatterEnable = weatherEffectProperty.rainSpatterEnable;
            WorldManager.Instance.weatherEffectModule.property.snowEnable = weatherEffectProperty.snowEnable;
            WorldManager.Instance.weatherEffectModule.property.lightningEnable = weatherEffectProperty.lightningEnable;
            WorldManager.Instance.weatherEffectModule.property.windSpeedCoeff = weatherEffectProperty.windSpeedCoeff;
            WorldManager.Instance.weatherEffectModule.property.effectRadius = weatherEffectProperty.effectRadius;
            WorldManager.Instance.weatherEffectModule.property.particleBright = weatherEffectProperty.particleBright;

            WorldManager.Instance.weatherEffectModule.OnValidate();

            if (WorldManager.Instance.weatherEffectModule.rainEffect != null)
            {
                rainEffectProperty.LimitProperty();
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainPrecipitation = rainEffectProperty.rainPrecipitation;
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainSize = rainEffectProperty.rainSize;
                WorldManager.Instance.weatherEffectModule.rainEffect.property.rainDropLength = rainEffectProperty.rainDropLength;
                WorldManager.Instance.weatherEffectModule.rainEffect.OnValidate();
            }
            
            if (WorldManager.Instance.weatherEffectModule.rainSpatterEffect != null)
            {
                rainSpatterEffectProperty.LimitProperty();
                if (rainSpatterEffectProperty.rangeMesh == null)
                {
                    rainSpatterEffectProperty.rangeMesh = VFXRainSpatterEffect.TemporaryQuad;
                    WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.rangeMesh = VFXRainSpatterEffect.TemporaryQuad;
                }
                else
                    WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.rangeMesh = rainSpatterEffectProperty.rangeMesh;
                WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.debugRangeMesh = rainSpatterEffectProperty.debugRangeMesh;
                rainSpatterEffectProperty.meshTransform = WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.meshTransform;
                WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.spawnRate = rainSpatterEffectProperty.spawnRate;
                WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.particleSize = rainSpatterEffectProperty.particleSize;
                
                WorldManager.Instance.weatherEffectModule.rainSpatterEffect.OnValidate();
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

                if (WorldManager.Instance.weatherEffectModule.rainSpatterEffect != null)
                {
                    VARIABLE.rainSpatterEffectProperty.rainSpatterEffect = WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.rainSpatterEffect;
                    VARIABLE.rainSpatterEffectProperty.rainSpatterFlipbook = WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.rainSpatterFlipbook;
                    VARIABLE.rainSpatterEffectProperty.rainSpatterFlipbookSize = WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.rainSpatterFlipbookSize;
                }
                
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
            item.weatherEffectProperty.useOcclusion = weatherEffectProperty.useOcclusion;
            item.weatherEffectProperty.rainEnable = weatherEffectProperty.rainEnable;
            item.weatherEffectProperty.rainSpatterEnable = weatherEffectProperty.rainSpatterEnable;
            item.weatherEffectProperty.snowEnable = weatherEffectProperty.snowEnable;
            item.weatherEffectProperty.lightningEnable = weatherEffectProperty.lightningEnable;
            item.weatherEffectProperty.windSpeedCoeff = weatherEffectProperty.windSpeedCoeff;
            item.weatherEffectProperty.effectRadius = weatherEffectProperty.effectRadius;
            item.weatherEffectProperty.particleBright = weatherEffectProperty.particleBright;
            
            item.rainSpatterEffectProperty.rangeMesh = rainSpatterEffectProperty.rangeMesh;
            item.rainSpatterEffectProperty.debugRangeMesh = rainSpatterEffectProperty.debugRangeMesh;
            item.rainSpatterEffectProperty.meshTransform = rainSpatterEffectProperty.meshTransform;
            
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
            
            if (WorldManager.Instance.weatherEffectModule.rainSpatterEffect is not null)
            {
                WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.spawnRate =
                    math.lerp(rainSpatterEffectProperty.spawnRate, weatherDefine.rainSpatterEffectProperty.spawnRate, lerpCoeff);
                WorldManager.Instance.weatherEffectModule.rainSpatterEffect.property.particleSize =
                    math.lerp(rainSpatterEffectProperty.particleSize, weatherDefine.rainSpatterEffectProperty.particleSize, lerpCoeff);
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
    
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_WeatherEffectModule(WeatherDefine variable)
        {
            variable.weatherEffectProperty.ExecuteProperty();
        }
        
#endif
        
        #endregion
    }
    
    
    /// <summary>
    /// 湿润积水模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 湿润积水模块
        
        [FoldoutGroup("昼夜与天气/湿润积水模块")][HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.moistAccumulatedWaterModuleToggle")]
        public MoistAccumulatedWaterModule.Property moistAccumulatedWaterProperty = new();
        
        #endregion
        
        public void SetupProperty_MoistAccumulatedWaterModule()
        {
            if (WorldManager.Instance?.moistAccumulatedWaterModule is null)
                return;
            
            WorldManager.Instance.moistAccumulatedWaterModule.property.globalMoist =
                moistAccumulatedWaterProperty.globalMoist;
            WorldManager.Instance.moistAccumulatedWaterModule.property.raindropsTiling =
                moistAccumulatedWaterProperty.raindropsTiling;
            WorldManager.Instance.moistAccumulatedWaterModule.property.raindropsSplashSpeed =
                moistAccumulatedWaterProperty.raindropsSplashSpeed;
            WorldManager.Instance.moistAccumulatedWaterModule.property.raindropsSize =
                moistAccumulatedWaterProperty.raindropsSize;
            WorldManager.Instance.moistAccumulatedWaterModule.property.accumulatedWaterMaskTiling =
                moistAccumulatedWaterProperty.accumulatedWaterMaskTiling;
            WorldManager.Instance.moistAccumulatedWaterModule.property.accumulatedWaterContrast =
                moistAccumulatedWaterProperty.accumulatedWaterContrast;
            WorldManager.Instance.moistAccumulatedWaterModule.property.accumulatedWaterSteepHillExtinction =
                moistAccumulatedWaterProperty.accumulatedWaterSteepHillExtinction;
            WorldManager.Instance.moistAccumulatedWaterModule.property.accumulatedWaterParallaxStrength =
                moistAccumulatedWaterProperty.accumulatedWaterParallaxStrength;
            WorldManager.Instance.moistAccumulatedWaterModule.property.xColumnsYRowsZSpeedWStrartFrame =
                moistAccumulatedWaterProperty.xColumnsYRowsZSpeedWStrartFrame;
            WorldManager.Instance.moistAccumulatedWaterModule.property.ripplesMainTiling =
                moistAccumulatedWaterProperty.ripplesMainTiling;
            WorldManager.Instance.moistAccumulatedWaterModule.property.ripplesMainStrength =
                moistAccumulatedWaterProperty.ripplesMainStrength;
            WorldManager.Instance.moistAccumulatedWaterModule.property.flowStrength =
                moistAccumulatedWaterProperty.flowStrength;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveRotate =
                moistAccumulatedWaterProperty.waterWaveRotate;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveMainTiling =
                moistAccumulatedWaterProperty.waterWaveMainTiling;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveMainSpeed =
                moistAccumulatedWaterProperty.waterWaveMainSpeed;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveMainStrength =
                moistAccumulatedWaterProperty.waterWaveMainStrength;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveDetailTiling =
                moistAccumulatedWaterProperty.waterWaveDetailTiling;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveDetailSpeed =
                moistAccumulatedWaterProperty.waterWaveDetailSpeed;
            WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveDetailStrength =
                moistAccumulatedWaterProperty.waterWaveDetailStrength;
            WorldManager.Instance.moistAccumulatedWaterModule.property.flowTiling =
                moistAccumulatedWaterProperty.flowTiling;
            
            WorldManager.Instance.moistAccumulatedWaterModule.OnValidate();
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.moistAccumulatedWaterProperty.accumulatedWaterMask = WorldManager.Instance.moistAccumulatedWaterModule.property.accumulatedWaterMask;
                VARIABLE.moistAccumulatedWaterProperty.waterWaveNormal = WorldManager.Instance.moistAccumulatedWaterModule.property.waterWaveNormal;
                VARIABLE.moistAccumulatedWaterProperty.raindropsGradientMap = WorldManager.Instance.moistAccumulatedWaterModule.property.raindropsGradientMap;
                VARIABLE.moistAccumulatedWaterProperty.flowMap = WorldManager.Instance.moistAccumulatedWaterModule.property.flowMap;
                VARIABLE.moistAccumulatedWaterProperty.ripplesNormalAtlas = WorldManager.Instance.moistAccumulatedWaterModule.property.ripplesNormalAtlas;
            }
#endif
        }

        private void UniformStaticProperty_MoistAccumulatedWaterModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.atmosphereModule is null)
                return;
            
            item.moistAccumulatedWaterProperty.raindropsGradientMap = moistAccumulatedWaterProperty.raindropsGradientMap;
            item.moistAccumulatedWaterProperty.raindropsTiling = moistAccumulatedWaterProperty.raindropsTiling;
            item.moistAccumulatedWaterProperty.raindropsSplashSpeed = moistAccumulatedWaterProperty.raindropsSplashSpeed;
            item.moistAccumulatedWaterProperty.raindropsSize = moistAccumulatedWaterProperty.raindropsSize;
            
            item.moistAccumulatedWaterProperty.accumulatedWaterMask = moistAccumulatedWaterProperty.accumulatedWaterMask;
            item.moistAccumulatedWaterProperty.accumulatedWaterMaskTiling = moistAccumulatedWaterProperty.accumulatedWaterMaskTiling;
            item.moistAccumulatedWaterProperty.accumulatedWaterContrast = moistAccumulatedWaterProperty.accumulatedWaterContrast;
            item.moistAccumulatedWaterProperty.accumulatedWaterSteepHillExtinction = moistAccumulatedWaterProperty.accumulatedWaterSteepHillExtinction;
            item.moistAccumulatedWaterProperty.accumulatedWaterParallaxStrength = moistAccumulatedWaterProperty.accumulatedWaterParallaxStrength;
            
            item.moistAccumulatedWaterProperty.ripplesNormalAtlas = moistAccumulatedWaterProperty.ripplesNormalAtlas;
            item.moistAccumulatedWaterProperty.xColumnsYRowsZSpeedWStrartFrame = moistAccumulatedWaterProperty.xColumnsYRowsZSpeedWStrartFrame;
            item.moistAccumulatedWaterProperty.ripplesMainTiling = moistAccumulatedWaterProperty.ripplesMainTiling;
            // item.moistAccumulatedWaterProperty.ripplesMainStrength = moistAccumulatedWaterProperty.ripplesMainStrength;
            
            item.moistAccumulatedWaterProperty.waterWaveNormal = moistAccumulatedWaterProperty.waterWaveNormal;
            item.moistAccumulatedWaterProperty.waterWaveRotate = moistAccumulatedWaterProperty.waterWaveRotate;
            item.moistAccumulatedWaterProperty.waterWaveMainTiling = moistAccumulatedWaterProperty.waterWaveMainTiling;
            item.moistAccumulatedWaterProperty.waterWaveMainSpeed = moistAccumulatedWaterProperty.waterWaveMainSpeed;
            item.moistAccumulatedWaterProperty.waterWaveMainStrength = moistAccumulatedWaterProperty.waterWaveMainStrength;
            item.moistAccumulatedWaterProperty.waterWaveDetailTiling = moistAccumulatedWaterProperty.waterWaveDetailTiling;
            item.moistAccumulatedWaterProperty.waterWaveDetailSpeed = moistAccumulatedWaterProperty.waterWaveDetailSpeed;
            item.moistAccumulatedWaterProperty.waterWaveDetailStrength = moistAccumulatedWaterProperty.waterWaveDetailStrength;
            
            item.moistAccumulatedWaterProperty.flowMap = moistAccumulatedWaterProperty.flowMap;
            item.moistAccumulatedWaterProperty.flowTiling = moistAccumulatedWaterProperty.flowTiling;
            
        }

        public void SetupLerpProperty_MoistAccumulatedWaterModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.moistAccumulatedWaterModule is null)
                return;
            
            WorldManager.Instance.moistAccumulatedWaterModule.property.globalMoist =
                math.lerp(moistAccumulatedWaterProperty.globalMoist, weatherDefine.moistAccumulatedWaterProperty.globalMoist, lerpCoeff);
            WorldManager.Instance.moistAccumulatedWaterModule.property.ripplesMainStrength =
                math.lerp(moistAccumulatedWaterProperty.ripplesMainStrength, weatherDefine.moistAccumulatedWaterProperty.ripplesMainStrength, lerpCoeff);
            WorldManager.Instance.moistAccumulatedWaterModule.property.flowStrength =
                math.lerp(moistAccumulatedWaterProperty.flowStrength, weatherDefine.moistAccumulatedWaterProperty.flowStrength, lerpCoeff);
        }
    }
    
    
    /// <summary>
    /// 近似实时光照模块
    /// </summary>
    public partial class WeatherDefine
    {
        
        #region 字段
        
        [FoldoutGroup("昼夜与天气/近似实时光照模块")][HideLabel] [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.starModuleToggle")]
        public ApproxRealtimeGIModule.Property approxRealtimeGIProperty = new();
        
        #endregion

        private static bool _approxRealtimeGISingleExecute = true;

        public void SetupProperty_ApproxRealtimeGIModule()
        {
            if (WorldManager.Instance?.approxRealtimeGIModule is null) 
                return;
            WorldManager.Instance.approxRealtimeGIModule.property.realtimeGIStrengthCurve =
                approxRealtimeGIProperty.realtimeGIStrengthCurve;
            WorldManager.Instance.approxRealtimeGIModule.property.lightningRealtimeGIStrengthCurve =
                approxRealtimeGIProperty.lightningRealtimeGIStrengthCurve;
            WorldManager.Instance.approxRealtimeGIModule.property.lightingMapContrast =
                approxRealtimeGIProperty.lightingMapContrast;
            WorldManager.Instance.approxRealtimeGIModule.property.reflectionSkyColor =
                approxRealtimeGIProperty.reflectionSkyColor;
            WorldManager.Instance.approxRealtimeGIModule.property.reflectionStrengthCurve =
                approxRealtimeGIProperty.reflectionStrengthCurve;
            WorldManager.Instance.approxRealtimeGIModule.property.mixCoeffCurve =
                approxRealtimeGIProperty.mixCoeffCurve;
            WorldManager.Instance.approxRealtimeGIModule.property.lightingMapToAoMin =
                approxRealtimeGIProperty.lightingMapToAoMin;
            WorldManager.Instance.approxRealtimeGIModule.property.lightingMapToAoMan =
                approxRealtimeGIProperty.lightingMapToAoMan;
            WorldManager.Instance.approxRealtimeGIModule.property.ssrSamples =
                approxRealtimeGIProperty.ssrSamples;
            WorldManager.Instance.approxRealtimeGIModule.property.ssrRayLength =
                approxRealtimeGIProperty.ssrRayLength;
            WorldManager.Instance.approxRealtimeGIModule.property.ssrThickness =
                approxRealtimeGIProperty.ssrThickness;
            WorldManager.Instance.approxRealtimeGIModule.property.ssrJitter =
                approxRealtimeGIProperty.ssrJitter;
            
            WorldManager.Instance.approxRealtimeGIModule.property.reflectionCubeTexture =
                approxRealtimeGIProperty.reflectionCubeTexture;
            // WorldManager.Instance.approxRealtimeGIModule.property._SSR_BlendCurve =
            //     approxRealtimeGIProperty._SSR_BlendCurve;
            
            WorldManager.Instance.approxRealtimeGIModule.OnValidate();
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.approxRealtimeGIProperty.ssrNoiseTex = 
                    WorldManager.Instance.approxRealtimeGIModule.property.ssrNoiseTex;
                VARIABLE.approxRealtimeGIProperty.mainReflectionProbe = 
                    WorldManager.Instance.approxRealtimeGIModule.property.mainReflectionProbe;
                VARIABLE.approxRealtimeGIProperty.blendCubeTexture = 
                    WorldManager.Instance.approxRealtimeGIModule.property.blendCubeTexture;
            }
#endif

        }
        
        private void UniformStaticProperty_ApproxRealtimeGIModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.approxRealtimeGIModule is null)
                return;
            
            item.approxRealtimeGIProperty.realtimeGIStrengthCurve = approxRealtimeGIProperty.realtimeGIStrengthCurve;
            item.approxRealtimeGIProperty.lightningRealtimeGIStrengthCurve = approxRealtimeGIProperty.lightningRealtimeGIStrengthCurve;

            item.approxRealtimeGIProperty.lightingMapContrast = approxRealtimeGIProperty.lightingMapContrast;
            item.approxRealtimeGIProperty.reflectionStrengthCurve = approxRealtimeGIProperty.reflectionStrengthCurve;
            item.approxRealtimeGIProperty.mixCoeffCurve = approxRealtimeGIProperty.mixCoeffCurve;
            item.approxRealtimeGIProperty.lightingMapToAoMin = approxRealtimeGIProperty.lightingMapToAoMin;
            item.approxRealtimeGIProperty.lightingMapToAoMan = approxRealtimeGIProperty.lightingMapToAoMan;
            item.approxRealtimeGIProperty.ssrSamples = approxRealtimeGIProperty.ssrSamples;
            item.approxRealtimeGIProperty.ssrRayLength = approxRealtimeGIProperty.ssrRayLength;
            item.approxRealtimeGIProperty.ssrThickness = approxRealtimeGIProperty.ssrThickness;
            item.approxRealtimeGIProperty.ssrNoiseTex = approxRealtimeGIProperty.ssrNoiseTex;
            item.approxRealtimeGIProperty.ssrJitter = approxRealtimeGIProperty.ssrJitter;
            
            item.approxRealtimeGIProperty.mainReflectionProbe = approxRealtimeGIProperty.mainReflectionProbe;
            item.approxRealtimeGIProperty.blendCubeTexture = approxRealtimeGIProperty.blendCubeTexture;
            
            // item.approxRealtimeGIProperty._SSR_BlendCurve = approxRealtimeGIProperty._SSR_BlendCurve;

        }
        
        //动态属性
        public void SetupLerpProperty_ApproxRealtimeGIModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.approxRealtimeGIModule is null || WorldManager.Instance.timeModule is null) 
                return;
            //当进入插值时将 ApproxRealtimeGIModule.useLerp 设置为true,停止属性的根据昼夜的变换, 而是由外部多个天气进行插值
            ApproxRealtimeGIModule.UseLerp = true;
            
            //只执行一次
            if (_approxRealtimeGISingleExecute)
            {
                //输入下一个天气的动态属性
                WorldManager.Instance.approxRealtimeGIModule.property.reflectionSkyColor =
                    weatherDefine.approxRealtimeGIProperty.reflectionSkyColor;
                WorldManager.Instance.approxRealtimeGIModule.property.reflectionCubeTexture =
                    weatherDefine.approxRealtimeGIProperty.reflectionCubeTexture;
            }
            
            WorldManager.Instance.approxRealtimeGIModule.reflectionSkyColorExecute = 
                Color.Lerp(approxRealtimeGIProperty.reflectionSkyColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.approxRealtimeGIProperty.reflectionSkyColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);

            ReflectionProbe.BlendCubemap(approxRealtimeGIProperty.reflectionCubeTexture,
                weatherDefine.approxRealtimeGIProperty.reflectionCubeTexture, lerpCoeff,
                WorldManager.Instance.approxRealtimeGIModule.property.blendCubeTexture);
            
            _approxRealtimeGISingleExecute = false;
            
        }
        
    }
    
    
    /// <summary>
    /// 雾模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 字段
        
#if UNITY_EDITOR
        
        [ShowIf("@fogProperty.useSunLight && WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [FoldoutGroup("昼夜与天气/雾模块")]
        [HorizontalGroup("昼夜与天气/雾模块/Split")]
        [VerticalGroup("昼夜与天气/雾模块/Split/01")]
        [Button(ButtonSizes.Medium, Name = "太阳散射雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseSunLight_Off()
        {
            fogProperty.useSunLight = false;
            OnValidate();
            WorldManager.Instance.fogModule?._UseSunLight_Off();
        }
        
        [HideIf("@fogProperty.useSunLight || !WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [VerticalGroup("昼夜与天气/雾模块/Split/01")]
        [Button(ButtonSizes.Medium, Name = "太阳散射雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseSunLight_On()
        {
            fogProperty.useSunLight = true;
            OnValidate();
            WorldManager.Instance.fogModule?._UseSunLight_On();
        }
        
        [ShowIf("@fogProperty.useDistanceFog && WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [HorizontalGroup("昼夜与天气/雾模块/Split")]
        [VerticalGroup("昼夜与天气/雾模块/Split/02")]
        [Button(ButtonSizes.Medium, Name = "距离雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseDistanceFog_Off()
        {
            fogProperty.useDistanceFog = false;
            OnValidate();
            WorldManager.Instance.fogModule?._UseDistanceFog_Off();
        }
        
        [HideIf("@fogProperty.useDistanceFog || !WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [VerticalGroup("昼夜与天气/雾模块/Split/02")]
        [Button(ButtonSizes.Medium, Name = "距离雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseDistanceFog_On()
        {
            fogProperty.useDistanceFog = true;
            OnValidate();
            WorldManager.Instance.fogModule?._UseDistanceFog_On();
        }
        
        [ShowIf("@fogProperty.useSkyboxHeightFog && WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [HorizontalGroup("昼夜与天气/雾模块/Split")]
        [VerticalGroup("昼夜与天气/雾模块/Split/03")]
        [Button(ButtonSizes.Medium, Name = "大气雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseSkyboxHeightFog_Off()
        {
            fogProperty.useSkyboxHeightFog = false;
            OnValidate();
            WorldManager.Instance.fogModule?._UseSkyboxHeightFog_Off();
        }
        
        [HideIf("@fogProperty.useSkyboxHeightFog || !WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [VerticalGroup("昼夜与天气/雾模块/Split/03")]
        [Button(ButtonSizes.Medium, Name = "大气雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseSkyboxHeightFog_On()
        {
            fogProperty.useSkyboxHeightFog = true;
            OnValidate();
            WorldManager.Instance.fogModule?._UseSkyboxHeightFog_On();
        }
        
        [ShowIf("@fogProperty.useHeightFog && WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [HorizontalGroup("昼夜与天气/雾模块/Split")]
        [VerticalGroup("昼夜与天气/雾模块/Split/04")]
        [Button(ButtonSizes.Medium, Name = "高度雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseHeightFog_Off()
        {
            fogProperty.useHeightFog = false;
            OnValidate();
            WorldManager.Instance.fogModule?._UseHeightFog_Off();
        }
        
        [HideIf("@fogProperty.useHeightFog || !WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [VerticalGroup("昼夜与天气/雾模块/Split/04")]
        [Button(ButtonSizes.Medium, Name = "高度雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseHeightFog_On()
        {
            fogProperty.useHeightFog = true;
            OnValidate();
            WorldManager.Instance.fogModule?._UseHeightFog_On();
        }
        
        [ShowIf("@fogProperty.useNoise && WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [HorizontalGroup("昼夜与天气/雾模块/Split")]
        [VerticalGroup("昼夜与天气/雾模块/Split/05")]
        [Button(ButtonSizes.Medium, Name = "噪波"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseNoise_Off()
        {
            fogProperty.useNoise = false;
            OnValidate();
            WorldManager.Instance.fogModule?._UseNoise_Off();
        }
        
        [HideIf("@fogProperty.useNoise || !WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [VerticalGroup("昼夜与天气/雾模块/Split/05")]
        [Button(ButtonSizes.Medium, Name = "噪波"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseNoise_On()
        {
            fogProperty.useNoise = true;
            OnValidate();
            WorldManager.Instance.fogModule?._UseNoise_On();
        }
        
        [ShowIf("@fogProperty.useSsms && WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [HorizontalGroup("昼夜与天气/雾模块/Split")]
        [VerticalGroup("昼夜与天气/雾模块/Split/06")]
        [Button(ButtonSizes.Medium, Name = "SSMS"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseSSMS_Off()
        {
            fogProperty.useSsms = false;
            OnValidate();
            WorldManager.Instance.fogModule?._UseSSMS_Off();
        }
        
        [HideIf("@fogProperty.useSsms || !WorldManager.Instance?.fogModuleToggle")]
        [EnableIf("isActive")] 
        [VerticalGroup("昼夜与天气/雾模块/Split/06")]
        [Button(ButtonSizes.Medium, Name = "SSMS"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseSSMS_On()
        {
            fogProperty.useSsms = true;
            OnValidate();
            WorldManager.Instance.fogModule?._UseSSMS_On();
        }
#endif
        
        [FoldoutGroup("昼夜与天气/雾模块")][HideLabel][PropertyOrder(1)]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.fogModuleToggle")]
        public FogModule.Property fogProperty = new();
        
        #endregion

        private static bool _fogModuleSingleExecute = true;

        public void SetupProperty_FogModule()
        {
            if (WorldManager.Instance?.fogModule is null) 
                return;
            WorldManager.Instance.fogModule.property.fogIntensity = fogProperty.fogIntensity;
            WorldManager.Instance.fogModule.property.fogColor = fogProperty.fogColor;
            WorldManager.Instance.fogModule.property.useSunLight = fogProperty.useSunLight;
            WorldManager.Instance.fogModule.property.sunIntensity = fogProperty.sunIntensity;
            WorldManager.Instance.fogModule.property.sunPower = fogProperty.sunPower;
            WorldManager.Instance.fogModule.property.useDistanceFog = fogProperty.useDistanceFog;
            WorldManager.Instance.fogModule.property.useRadialDistance = fogProperty.useRadialDistance;
            WorldManager.Instance.fogModule.property.distanceFogOffset = fogProperty.distanceFogOffset;
            WorldManager.Instance.fogModule.property.fogType = fogProperty.fogType;
            WorldManager.Instance.fogModule.property.sceneStart = fogProperty.sceneStart;
            WorldManager.Instance.fogModule.property.sceneEnd = fogProperty.sceneEnd;
            WorldManager.Instance.fogModule.property.fogDensity = fogProperty.fogDensity;
            WorldManager.Instance.fogModule.property.useSkyboxHeightFog = fogProperty.useSkyboxHeightFog;
            WorldManager.Instance.fogModule.property.skyboxFogOffset = fogProperty.skyboxFogOffset;
            WorldManager.Instance.fogModule.property.skyboxFogHardness = fogProperty.skyboxFogHardness;
            WorldManager.Instance.fogModule.property.skyboxFogIntensity = fogProperty.skyboxFogIntensity;
            WorldManager.Instance.fogModule.property.skyboxFill = fogProperty.skyboxFill;
            WorldManager.Instance.fogModule.property.useHeightFog = fogProperty.useHeightFog;
            WorldManager.Instance.fogModule.property.height = fogProperty.height;
            WorldManager.Instance.fogModule.property.heightDensity = fogProperty.heightDensity;
            WorldManager.Instance.fogModule.property.heightFogType = fogProperty.heightFogType;
            WorldManager.Instance.fogModule.property.useNoise = fogProperty.useNoise;
            WorldManager.Instance.fogModule.property.noiseAffect = fogProperty.noiseAffect;
            WorldManager.Instance.fogModule.property.noiseIntensity = fogProperty.noiseIntensity;
            WorldManager.Instance.fogModule.property.noiseDistanceEnd = fogProperty.noiseDistanceEnd;
            WorldManager.Instance.fogModule.property.scale1 = fogProperty.scale1;
            WorldManager.Instance.fogModule.property.lerp1 = fogProperty.lerp1;
            WorldManager.Instance.fogModule.property.noiseWindCoeff = fogProperty.noiseWindCoeff;
            WorldManager.Instance.fogModule.property.useSsms = fogProperty.useSsms;
            WorldManager.Instance.fogModule.property.threshold = fogProperty.threshold;
            WorldManager.Instance.fogModule.property.softKnee = fogProperty.softKnee;
            WorldManager.Instance.fogModule.property.radius = fogProperty.radius;
            WorldManager.Instance.fogModule.property.blurWeight = fogProperty.blurWeight;
            WorldManager.Instance.fogModule.property.intensity = fogProperty.intensity;
            WorldManager.Instance.fogModule.property.highQuality = fogProperty.highQuality;
            WorldManager.Instance.fogModule.property.antiFlicker = fogProperty.antiFlicker;
            
            WorldManager.Instance.fogModule.property.sunStartDistance = fogProperty.sunStartDistance;
            WorldManager.Instance.fogModule.property.sunAtten = fogProperty.sunAtten;
            
            WorldManager.Instance.fogModule.property.fogLightningColor = fogProperty.fogLightningColor;

            WorldManager.Instance.fogModule.OnValidate();
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                // VARIABLE.fogProperty.fogShader = WorldManager.Instance.fogModule.property.fogShader;
                VARIABLE.fogProperty.fogMaterial = WorldManager.Instance.fogModule.property.fogMaterial;
                // VARIABLE.fogProperty.fogFactorShader = WorldManager.Instance.fogModule.property.fogFactorShader;
                VARIABLE.fogProperty.fogFactorMaterial = WorldManager.Instance.fogModule.property.fogFactorMaterial;
                // VARIABLE.fogProperty.fogApplyShader = WorldManager.Instance.fogModule.property.fogApplyShader;
                VARIABLE.fogProperty.fogApplyMaterial = WorldManager.Instance.fogModule.property.fogApplyMaterial;
                // VARIABLE.fogProperty.ssmsShader = WorldManager.Instance.fogModule.property.ssmsShader;
                VARIABLE.fogProperty.ssmsMaterial = WorldManager.Instance.fogModule.property.ssmsMaterial;
                VARIABLE.fogProperty.fadeRamp = WorldManager.Instance.fogModule.property.fadeRamp;
            }
#endif

        }
        
        private void UniformStaticProperty_FogModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.fogModule is null)
                return;
            
            item.fogProperty.useSunLight = fogProperty.useSunLight;
            // item.fogProperty._SunPower = fogProperty._SunPower;
            item.fogProperty.useDistanceFog = fogProperty.useDistanceFog;
            item.fogProperty.useRadialDistance = fogProperty.useRadialDistance;
            item.fogProperty.fogType = fogProperty.fogType;
            item.fogProperty.useSkyboxHeightFog = fogProperty.useSkyboxHeightFog;
            item.fogProperty.skyboxFogHardness = fogProperty.skyboxFogHardness;
            item.fogProperty.skyboxFill = fogProperty.skyboxFill;
            item.fogProperty.useHeightFog = fogProperty.useHeightFog;
            item.fogProperty.heightFogType = fogProperty.heightFogType;
            item.fogProperty.useNoise = fogProperty.useNoise;
            item.fogProperty.noiseAffect = fogProperty.noiseAffect;
            item.fogProperty.noiseDistanceEnd = fogProperty.noiseDistanceEnd;
            item.fogProperty.scale1 = fogProperty.scale1;
            item.fogProperty.lerp1 = fogProperty.lerp1;
            // item.fogProperty._NoiseDirection1 = fogProperty._NoiseDirection1;
            item.fogProperty.noiseWindCoeff = fogProperty.noiseWindCoeff;
            item.fogProperty.useSsms = fogProperty.useSsms;
            item.fogProperty.threshold = fogProperty.threshold;
            item.fogProperty.softKnee = fogProperty.softKnee;
            item.fogProperty.radius = fogProperty.radius;
            item.fogProperty.blurWeight = fogProperty.blurWeight;
            item.fogProperty.intensity = fogProperty.intensity;
            item.fogProperty.highQuality = fogProperty.highQuality;
            item.fogProperty.antiFlicker = fogProperty.antiFlicker;
            
            item.fogProperty.sunStartDistance = fogProperty.sunStartDistance;
            item.fogProperty.sunAtten = fogProperty.sunAtten;
            item.fogProperty.fogLightningColor = fogProperty.fogLightningColor;

        }
        
        //动态属性
        public void SetupLerpProperty_FogModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.fogModule is null || WorldManager.Instance.timeModule is null) 
                return;
            //当进入插值时将 useLerp 设置为true,停止属性的根据昼夜的变换, 而是由外部多个天气进行插值
            FogModule.UseLerp = true;
            
            //只执行一次
            if (_fogModuleSingleExecute)
            {
                //输入下一个天气的动态属性
                WorldManager.Instance.fogModule.property.fogIntensity = weatherDefine.fogProperty.fogIntensity;
                WorldManager.Instance.fogModule.property.fogColor = weatherDefine.fogProperty.fogColor;
                WorldManager.Instance.fogModule.property.sunIntensity = weatherDefine.fogProperty.sunIntensity;
                WorldManager.Instance.fogModule.property.sunPower = weatherDefine.fogProperty.sunPower;
                WorldManager.Instance.fogModule.property.distanceFogOffset = weatherDefine.fogProperty.distanceFogOffset;
                // WorldManager.Instance.fogModule.property._SceneStart = weatherDefine.fogProperty._SceneStart;
                // WorldManager.Instance.fogModule.property._SceneEnd = weatherDefine.fogProperty._SceneEnd;
                WorldManager.Instance.fogModule.property.fogDensity = weatherDefine.fogProperty.fogDensity;
                WorldManager.Instance.fogModule.property.skyboxFogOffset = weatherDefine.fogProperty.skyboxFogOffset;
                WorldManager.Instance.fogModule.property.skyboxFogIntensity = weatherDefine.fogProperty.skyboxFogIntensity;
                WorldManager.Instance.fogModule.property.height = weatherDefine.fogProperty.height;
                WorldManager.Instance.fogModule.property.heightDensity = weatherDefine.fogProperty.heightDensity;
                WorldManager.Instance.fogModule.property.noiseIntensity = weatherDefine.fogProperty.noiseIntensity;
            }
            
            WorldManager.Instance.fogModule.property.fogIntensityExecute = 
                math.lerp(fogProperty.fogIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.fogIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.fogColorExecute = 
                Color.Lerp(fogProperty.fogColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.fogColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.sunIntensityExecute = 
                math.lerp(fogProperty.sunIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), 
                    weatherDefine.fogProperty.sunIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.sunPowerExecute = 
                math.lerp(fogProperty.sunPower.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), 
                    weatherDefine.fogProperty.sunPower.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.distanceFogOffsetExecute = 
                math.lerp(fogProperty.distanceFogOffset.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.distanceFogOffset.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.sceneStart = 
                math.lerp(fogProperty.sceneStart, weatherDefine.fogProperty.sceneStart, lerpCoeff);
            WorldManager.Instance.fogModule.property.sceneEnd = 
                math.lerp(fogProperty.sceneEnd, weatherDefine.fogProperty.sceneEnd, lerpCoeff);
            
            WorldManager.Instance.fogModule.property.fogDensityExecute = 
                math.lerp(fogProperty.fogDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.fogDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.skyboxFogOffsetExecute = 
                math.lerp(fogProperty.skyboxFogOffset.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.skyboxFogOffset.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.skyboxFogIntensityExecute = 
                math.lerp(fogProperty.skyboxFogIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.skyboxFogIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.heightDensityExecute = 
                math.lerp(fogProperty.heightDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.heightDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.fogModule.property.noiseIntensityExecute = 
                math.lerp(fogProperty.noiseIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.fogProperty.noiseIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            _fogModuleSingleExecute = false;
        }
        
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_FogModule(WeatherDefine variable)
        {
            variable.fogProperty.ExecuteProperty();
            // variable.fogProperty._NoiseDirection1 =
            //     WorldManager.Instance.fogModule?.property._NoiseDirection1 ?? new float3(0, 0, 0);
        }
        
#endif
    }
    
    
    /// <summary>
    /// 光照散射模块
    /// </summary>
    public partial class WeatherDefine
    {
        
        #region 字段
        
        [FoldoutGroup("昼夜与天气/光照散射模块")][HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.lightingScatterModuleToggle")]
        public LightingScatterModule.Property lightingScatterProperty = new();
        
        #endregion

        private static bool _lightingScatterSingleExecute = true;

        public void SetupProperty_LightingScatterModule()
        {
            if (WorldManager.Instance?.lightingScatterModule is null) 
                return;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterNumSamples = lightingScatterProperty.lightingScatterNumSamples;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterFalloffDirective = lightingScatterProperty.lightingScatterFalloffDirective;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterDensity = lightingScatterProperty.lightingScatterDensity;
            // WorldManager.Instance.lightingScatterModule.property._LightingScatter_Density_Execute = lightingScatterProperty._LightingScatter_Density_Execute;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterFalloffIntensity = lightingScatterProperty.lightingScatterFalloffIntensity;
            // WorldManager.Instance.lightingScatterModule.property._LightingScatter_FalloffIntensity_Execute = lightingScatterProperty._LightingScatter_FalloffIntensity_Execute;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterSaturation = lightingScatterProperty.lightingScatterSaturation;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterMaxRayDistance = lightingScatterProperty.lightingScatterMaxRayDistance;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterOccOverDistanceAmount = lightingScatterProperty.lightingScatterOccOverDistanceAmount;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterUseSoftEdge = lightingScatterProperty.lightingScatterUseSoftEdge;
            WorldManager.Instance.lightingScatterModule.property.lightingScatterUseDynamicNoise = lightingScatterProperty.lightingScatterUseDynamicNoise;
            
            WorldManager.Instance.lightingScatterModule.OnValidate();
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.lightingScatterProperty.occlusionShader = WorldManager.Instance.lightingScatterModule.property.occlusionShader;
                VARIABLE.lightingScatterProperty.occlusionMaterial = WorldManager.Instance.lightingScatterModule.property.occlusionMaterial;
                VARIABLE.lightingScatterProperty.scatterShader = WorldManager.Instance.lightingScatterModule.property.scatterShader;
                VARIABLE.lightingScatterProperty.scatterMaterial = WorldManager.Instance.lightingScatterModule.property.scatterMaterial;
                VARIABLE.lightingScatterProperty.mergeShader = WorldManager.Instance.lightingScatterModule.property.mergeShader;
                VARIABLE.lightingScatterProperty.mergeMaterial = WorldManager.Instance.lightingScatterModule.property.mergeMaterial;
            }
#endif

        }
        
        private void UniformStaticProperty_LightingScatterModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.lightingScatterModule is null)
                return;
            
            item.lightingScatterProperty.lightingScatterNumSamples = lightingScatterProperty.lightingScatterNumSamples;
            item.lightingScatterProperty.lightingScatterFalloffDirective = lightingScatterProperty.lightingScatterFalloffDirective;
            item.lightingScatterProperty.lightingScatterSaturation = lightingScatterProperty.lightingScatterSaturation;
            item.lightingScatterProperty.lightingScatterMaxRayDistance = lightingScatterProperty.lightingScatterMaxRayDistance;
            item.lightingScatterProperty.lightingScatterOccOverDistanceAmount = lightingScatterProperty.lightingScatterOccOverDistanceAmount;
            item.lightingScatterProperty.lightingScatterUseSoftEdge = lightingScatterProperty.lightingScatterUseSoftEdge;
            item.lightingScatterProperty.lightingScatterUseDynamicNoise = lightingScatterProperty.lightingScatterUseDynamicNoise;

        }
        
        //动态属性
        public void SetupLerpProperty_LightingScatterModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.lightingScatterModule is null || WorldManager.Instance.timeModule is null) 
                return;
            
            //当进入插值时将 useLerp 设置为true,停止属性的根据昼夜的变换, 而是由外部多个天气进行插值
            LightingScatterModule.UseLerp = true;
            
            //只执行一次
            if (_lightingScatterSingleExecute)
            {
                //输入下一个天气的动态属性
                WorldManager.Instance.lightingScatterModule.property.lightingScatterDensity = weatherDefine.lightingScatterProperty.lightingScatterDensity;
                WorldManager.Instance.lightingScatterModule.property.lightingScatterFalloffIntensity = weatherDefine.lightingScatterProperty.lightingScatterFalloffIntensity;
            }
            
            WorldManager.Instance.lightingScatterModule.property.lightingScatterDensityExecute = 
                math.lerp(lightingScatterProperty.lightingScatterDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.lightingScatterProperty.lightingScatterDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.lightingScatterModule.property.lightingScatterFalloffIntensityExecute = 
                math.lerp(lightingScatterProperty.lightingScatterFalloffIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.lightingScatterProperty.lightingScatterFalloffIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            _lightingScatterSingleExecute = false;
        }
        
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_LightingScatterModule(WeatherDefine variable)
        {
            variable.lightingScatterProperty.ExecuteProperty();
        }
#endif
        
        
    }
    
    
    /// <summary>
    /// 后处理调整模块
    /// </summary>
    public partial class WeatherDefine
    {
        #region 字段
        
#if UNITY_EDITOR
        
        //请总是对一类效果制作开关
        [ShowIf("@postprocessAdjustProperty.useColorAdjust")]
        [HorizontalGroup("昼夜与天气/后处理调整模块/Split", 0.25f)]
        [VerticalGroup("昼夜与天气/后处理调整模块/Split/01")]
        [Button(ButtonSizes.Medium, Name = "颜色调整"), GUIColor(0.5f, 0.5f, 1f)]
        public void ToggleFunction_Off()
        {
            postprocessAdjustProperty.useColorAdjust = false;
            WorldManager.Instance.postprocessAdjustModule?.ColorAdjust_Off();
        }
        [HideIf("@postprocessAdjustProperty.useColorAdjust")]
        [VerticalGroup("昼夜与天气/后处理调整模块/Split/01")]
        [Button(ButtonSizes.Medium, Name = "颜色调整"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void ToggleFunction_On()
        {
            postprocessAdjustProperty.useColorAdjust = true;
            WorldManager.Instance.postprocessAdjustModule?.ColorAdjust_On();
        }
        
#endif
        
        
        [FoldoutGroup("昼夜与天气/后处理调整模块")][HideLabel]
        [EnableIf("isActive")] [ShowIf("@WorldManager.Instance?.lightingScatterModuleToggle")][PropertyOrder(1)]
        public PostprocessAdjustModule.Property postprocessAdjustProperty = new();
        
        #endregion

        private static bool _postprocessAdjustSingleExecute = true;

        public void SetupProperty_PostprocessAdjustModule()
        {
            if (WorldManager.Instance?.postprocessAdjustModule is null) 
                return;
            WorldManager.Instance.postprocessAdjustModule.property.useColorAdjust = postprocessAdjustProperty.useColorAdjust;
            WorldManager.Instance.postprocessAdjustModule.property.exposeCurve = postprocessAdjustProperty.exposeCurve;
            WorldManager.Instance.postprocessAdjustModule.property.contrastCurve = postprocessAdjustProperty.contrastCurve;
            WorldManager.Instance.postprocessAdjustModule.property.colorFilter = postprocessAdjustProperty.colorFilter;
            WorldManager.Instance.postprocessAdjustModule.property.hueShift = postprocessAdjustProperty.hueShift;
            WorldManager.Instance.postprocessAdjustModule.property.saturationCurve = postprocessAdjustProperty.saturationCurve;
            WorldManager.Instance.postprocessAdjustModule.OnValidate();
            
#if UNITY_EDITOR
            //仅显示信息
            WeatherDefine[] weatherDefineAll = Resources.FindObjectsOfTypeAll<WeatherDefine>();
            foreach (var VARIABLE in weatherDefineAll)
            {
                VARIABLE.postprocessAdjustProperty.globalVolume = WorldManager.Instance.postprocessAdjustModule.property.globalVolume;
                VARIABLE.postprocessAdjustProperty.colorAdjustments = WorldManager.Instance.postprocessAdjustModule.property.colorAdjustments;

            }
#endif

        }

        
        private void UniformStaticProperty_PostprocessAdjustModule(WeatherDefine item)
        {
            if (WorldManager.Instance?.postprocessAdjustModule is null)
                return;
            item.postprocessAdjustProperty.hueShift = postprocessAdjustProperty.hueShift;
            item.postprocessAdjustProperty.useColorAdjust = postprocessAdjustProperty.useColorAdjust;

        }
        
        //动态属性
        public void SetupLerpProperty_PostprocessAdjustModule(WeatherDefine weatherDefine, float lerpCoeff)
        {
            if (WorldManager.Instance?.postprocessAdjustModule is null || WorldManager.Instance.timeModule is null) 
                return;
            
            //当进入插值时将 useLerp 设置为true,停止属性的根据昼夜的变换, 而是由外部多个天气进行插值
            PostprocessAdjustModule.UseLerp = true;
            
            //只执行一次
            if (_postprocessAdjustSingleExecute)
            {
                //输入下一个天气的动态属性
                WorldManager.Instance.postprocessAdjustModule.property.exposeCurve = weatherDefine.postprocessAdjustProperty.exposeCurve;
                WorldManager.Instance.postprocessAdjustModule.property.contrastCurve = weatherDefine.postprocessAdjustProperty.contrastCurve;
                WorldManager.Instance.postprocessAdjustModule.property.colorFilter = weatherDefine.postprocessAdjustProperty.colorFilter;
                WorldManager.Instance.postprocessAdjustModule.property.saturationCurve = weatherDefine.postprocessAdjustProperty.saturationCurve;
            }
            
            WorldManager.Instance.postprocessAdjustModule.property.exposeCurveExecute = 
                math.lerp(postprocessAdjustProperty.exposeCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.postprocessAdjustProperty.exposeCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.postprocessAdjustModule.property.contrastCurveExecute = 
                math.lerp(postprocessAdjustProperty.contrastCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.postprocessAdjustProperty.contrastCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.postprocessAdjustModule.property.colorFilterExecute = 
                Color.Lerp(postprocessAdjustProperty.colorFilter.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.postprocessAdjustProperty.colorFilter.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            WorldManager.Instance.postprocessAdjustModule.property.saturationCurveExecute = 
                math.lerp(postprocessAdjustProperty.saturationCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01),
                    weatherDefine.postprocessAdjustProperty.saturationCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01), lerpCoeff);
            
            _postprocessAdjustSingleExecute = false;
        }
        
#if UNITY_EDITOR
        private static void UpdateDynamicDisplayProperty_PostprocessAdjustModule(WeatherDefine variable)
        {
            variable.postprocessAdjustProperty.ExecuteProperty();
        }
        
#endif
        
    }
    
    
    
       
    
}