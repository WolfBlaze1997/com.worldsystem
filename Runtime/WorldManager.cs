using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// using WorldSystem.Editor;

namespace WorldSystem.Runtime
{
    public partial class WorldManager
    {
        
#if UNITY_EDITOR
        
        #region 模块开关
        
        [PropertyOrder(-100)]
        [ShowIf("timModuleToggle")]
        [FoldoutGroup("昼夜与天气")]
        [HorizontalGroup("昼夜与天气/Split")]
        [VerticalGroup("昼夜与天气/Split/01")]
        [Button(ButtonSizes.Large, Name = "时间模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void TimModuleToggle_Off()
        {
            timModuleToggle = false;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [HideIf("timModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/01")]
        [Button(ButtonSizes.Large, Name = "时间模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void TimModuleToggle_On()
        {
            timModuleToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("universeBackgroundModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/02")]
        [Button(ButtonSizes.Large, Name = "渲染设置与背景"), GUIColor(0.3f, 1f, 0.3f)]
        private void UniverseBackgroundModuleToggle_Off()
        {
            StarModuleToggle_Off();
            CelestialBodyModuleToggle_Off();
            AtmosphereModuleToggle_Off();
            VolumeCloudOptimizeModuleToggle_Off();
            universeBackgroundModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("universeBackgroundModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/02")]
        [Button(ButtonSizes.Large, Name = "渲染设置与背景"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void UniverseBackgroundModuleToggle_On()
        {
            universeBackgroundModuleToggle = true;
            OnValidate();
        }
        
                
        [PropertyOrder(-100)]
        [ShowIf("weatherSystemModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/03")]
        [Button(ButtonSizes.Large, Name = "天气列表模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void WeatherSystemModuleToggle_Off()
        {
            weatherSystemModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("weatherSystemModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/03")]
        [Button(ButtonSizes.Large, Name = "天气列表模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WeatherSystemModuleToggle_On()
        {
            weatherSystemModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("atmosphereModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/03")]
        [Button(ButtonSizes.Large, Name = "大气模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void AtmosphereModuleToggle_Off()
        {
            atmosphereModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("atmosphereModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/03")]
        [Button(ButtonSizes.Large, Name = "大气模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void AtmosphereModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            atmosphereModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("volumeCloudOptimizeModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/04")]
        [Button(ButtonSizes.Large, Name = "体积云模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void VolumeCloudOptimizeModuleToggle_Off()
        {
            volumeCloudOptimizeModuleToggle = false;
            starModuleToggle = false;
            celestialBodyManagerToggle = false;

            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("volumeCloudOptimizeModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/04")]
        [Button(ButtonSizes.Large, Name = "体积云模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void VolumeCloudOptimizeModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            volumeCloudOptimizeModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("starModuleToggle")]
        [HorizontalGroup("昼夜与天气/Split02")]
        [VerticalGroup("昼夜与天气/Split02/01")]
        [Button(ButtonSizes.Large, Name = "星星模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void StarModuleToggle_Off()
        {
            starModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("starModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/01")]
        [Button(ButtonSizes.Large, Name = "星星模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void StarModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            starModuleToggle = true;
            volumeCloudOptimizeModuleToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("celestialBodyManagerToggle")]
        [VerticalGroup("昼夜与天气/Split02/02")]
        [Button(ButtonSizes.Large, Name = "天体模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void CelestialBodyModuleToggle_Off()
        {
            celestialBodyManagerToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("celestialBodyManagerToggle")]
        [VerticalGroup("昼夜与天气/Split02/02")]
        [Button(ButtonSizes.Large, Name = "天体模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void CelestialBodyModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            celestialBodyManagerToggle = true;
            volumeCloudOptimizeModuleToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("windZoneModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/05")]
        [Button(ButtonSizes.Large, Name = "风场模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void WindZoneModuleToggle_Off()
        {
            windZoneModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("windZoneModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/05")]
        [Button(ButtonSizes.Large, Name = "风场模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WindZoneModuleToggle_On()
        {
            windZoneModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("weatherEffectModuleToggle")]
        [HorizontalGroup("昼夜与天气/Split03")]
        [VerticalGroup("昼夜与天气/Split03/01")]
        [Button(ButtonSizes.Large, Name = "天气特效模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void WeatherEffectModuleToggle_Off()
        {
            weatherEffectModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/01")]
        [Button(ButtonSizes.Large, Name = "天气特效模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WeatherEffectModuleToggle_On()
        {
            weatherEffectModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("moistAccumulatedWaterModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/02")]
        [Button(ButtonSizes.Large, Name = "湿润积水模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void MoistAccumulatedWaterModuleToggle_Off()
        {
            moistAccumulatedWaterModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("moistAccumulatedWaterModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/02")]
        [Button(ButtonSizes.Large, Name = "湿润积水模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void MoistAccumulatedWaterModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            moistAccumulatedWaterModuleToggle = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("approxRealtimeGIModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/03")]
        [Button(ButtonSizes.Large, Name = "近似实时全局光照模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void ApproxRealtimeGIModuleToggle_Off()
        {
            approxRealtimeGIModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("approxRealtimeGIModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/03")]
        [Button(ButtonSizes.Large, Name = "近似实时全局光照模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void ApproxRealtimeGIModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            approxRealtimeGIModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("fogModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/04")]
        [Button(ButtonSizes.Large, Name = "雾模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void FogModuleToggle_Off()
        {
            fogModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("fogModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/04")]
        [Button(ButtonSizes.Large, Name = "雾模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void FogModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            fogModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("lightingScatterModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/05")]
        [Button(ButtonSizes.Large, Name = "光照散射模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void LightingScatterModuleToggle_Off()
        {
            lightingScatterModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("lightingScatterModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/05")]
        [Button(ButtonSizes.Large, Name = "光照散射雾模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void LightingScatterModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            lightingScatterModuleToggle = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("postprocessAdjustModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/06")]
        [Button(ButtonSizes.Large, Name = "后处理调整模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void PostprocessAdjustModuleToggle_Off()
        {
            postprocessAdjustModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("postprocessAdjustModuleToggle")]
        [VerticalGroup("昼夜与天气/Split03/06")]
        [Button(ButtonSizes.Large, Name = "后处理调整模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void PostprocessAdjustModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            postprocessAdjustModuleToggle = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("fpsDisplayModuleToggle")]
        [FoldoutGroup("实用工具")]
        [HorizontalGroup("实用工具/Split03",0.195f)]
        [VerticalGroup("实用工具/Split03/01")]
        [Button(ButtonSizes.Large, Name = "FPS显示模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void FpsDisplayModuleToggle_Off()
        {
            fpsDisplayModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("fpsDisplayModuleToggle")]
        [VerticalGroup("实用工具/Split03/01")]
        [Button(ButtonSizes.Large, Name = "FPS显示模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void FpsDisplayModuleToggle_On()
        {
            fpsDisplayModuleToggle = true;
            OnValidate();
        }
        
#if UNITY_EDITOR
        
        [PropertyOrder(-100)] [DisableIf("@true")]
        [ShowIf("baiduInputMethodPhraseToggle")]
        [FoldoutGroup("实用工具")]
        [HorizontalGroup("实用工具/Split03",0.195f)]
        [VerticalGroup("实用工具/Split03/04")]
        [Button(ButtonSizes.Large, Name = "百度输入法短语"), GUIColor(0.3f, 1f, 0.3f)]
        private void BaiduInputMethodPhraseToggle_Off()
        {
            // baiduInputMethodPhraseToggle = false;
            // OnValidate();
        }
        [PropertyOrder(-100)] [DisableIf("@true")]
        [HideIf("baiduInputMethodPhraseToggle")]
        [VerticalGroup("实用工具/Split03/04")]
        [Button(ButtonSizes.Large, Name = "百度输入法短语"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void BaiduInputMethodPhraseToggle_On()
        {
            // baiduInputMethodPhraseToggle = true;
            // OnValidate();
        }
        
        [PropertyOrder(-100)] [DisableIf("@true")]
        [ShowIf("packageManagerToggle")]
        [HorizontalGroup("实用工具/Split03",0.195f)]
        [VerticalGroup("实用工具/Split03/02")]
        [Button(ButtonSizes.Large, Name = "包管理器模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void PackageManagerToggle_Off()
        {
            // packageManagerToggle = false;
            // OnValidate();
        }
        [PropertyOrder(-100)] [DisableIf("@true")]
        [HideIf("packageManagerToggle")]
        [VerticalGroup("实用工具/Split03/02")]
        [Button(ButtonSizes.Large, Name = "包管理器模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void PackageManagerToggle_On()
        {
            // packageManagerToggle = true;
            // OnValidate();
        }
        
#endif
        
        
        #endregion

        
        #region GUI帮助函数
        
        [HideInInspector] 
        public bool hideFlagToggle;
        
        [Button(ButtonSizes.Medium, Name = "开发者模式已开启")] [ShowIf("hideFlagToggle")] [GUIColor("white")]
        private void HideFlagToggleChild()
        {
            if (timeModule != null ||
                universeBackgroundModule != null ||
                starModule != null ||
                celestialBodyManager != null ||
                gameObject.transform.childCount > 0)
            {
                hideFlagToggle = !hideFlagToggle;
                BroadcastMessage("HideFlagToggle");
            }
            else
            {
                Debug.Log("没有模块!");
            }
        }
        
        [Button(ButtonSizes.Medium, Name = "开发者模式已关闭")] [HideIf("hideFlagToggle")] [GUIColor("gray")]
        private void HideFlagToggleChild_Ref()
        {
            HideFlagToggleChild();
        }

        
        #endregion

        
        #region 绘制Gizmos
        
        private void OnDrawGizmosSelected()
        {
            if (transform.childCount > 0)
                BroadcastMessage("DrawGizmosSelected");
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem/Textures/Icon/day-night-icon.png");

            if (transform.childCount > 0)
                BroadcastMessage("DrawGizmos");
            
        }
        
        #endregion
        
#endif
        
        #region 帮助函数
        
        private T AppendOrDestroyModule<T>(bool moduleToggle, bool useChildObject = false, Vector3 offset = default) where T : MonoBehaviour
        {
            if (moduleToggle)
            {
                if (useChildObject)
                {
                    if (gameObject.GetComponentInChildren<T>() != null)
                        return gameObject.GetComponentInChildren<T>();
                    GameObject child = new GameObject();
                    child.transform.position = transform.position + offset;
                    child.transform.parent = transform;
                    string[] strings = typeof(T).ToString().Split(".");
                    child.name = strings[^1];
                    return child.AddComponent<T>();
                }

                if (gameObject.GetComponent<T>() != null)
                    return gameObject.GetComponent<T>();
                return gameObject.AddComponent<T>();
            }

            if (useChildObject)
            {
                if(gameObject.GetComponentInChildren<T>() != null)
                    CoreUtils.Destroy(gameObject.GetComponentInChildren<T>().gameObject);
                return null;
            }

            if(gameObject.GetComponent<T>() != null)
                CoreUtils.Destroy(gameObject.GetComponent<T>());
            return null;
        }
        
        #endregion

        
    }
    
    [AddComponentMenu("WorldSystem/WorldManager")]
    [ExecuteAlways]
    public partial class WorldManager : MonoBehaviour
    {
        /// 备忘录!
        /// 1- UI改进,需要将部分模块聚合成一个模块,子模块用蓝色标识
        /// 2- 注意CPU时间, 将最大可能改为异步执行
        /// 3- 以当前天空盒渲染循环所需的 FrameCount 来决定下一个天空盒渲染循环将分散到N帧, 如果在N帧还未渲染完成,则等待渲染完成在发出渲染命令(会重新计算分散的N帧数)

        
        #region 字段
        
        public static WorldManager Instance { get; set; }
        
        [HideInInspector] public bool timModuleToggle;
        [FoldoutGroup("昼夜与天气/时间模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("timModuleToggle")]
        public TimeModule timeModule;

        [HideInInspector] public bool universeBackgroundModuleToggle;
        [FoldoutGroup("昼夜与天气/渲染设置与背景")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(universeBackgroundModuleToggle && !weatherSystemModuleToggle) || (universeBackgroundModuleToggle && (universeBackgroundModule.hideFlags == HideFlags.None))")]
        public UniverseBackgroundModule universeBackgroundModule;

        [HideInInspector] public bool starModuleToggle;
        [FoldoutGroup("昼夜与天气/星星模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(starModuleToggle && !weatherSystemModuleToggle) || (starModuleToggle && (starModule.hideFlags == HideFlags.None))")]
        public StarModule starModule;

        [HideInInspector] public bool celestialBodyManagerToggle;
        [FoldoutGroup("昼夜与天气/星体模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(celestialBodyManagerToggle && !weatherSystemModuleToggle) || (celestialBodyManagerToggle && (celestialBodyManager.hideFlags == HideFlags.None))")]
        public CelestialBodyManager celestialBodyManager;

        
        [HideInInspector] public bool atmosphereModuleToggle;
        [FoldoutGroup("昼夜与天气/大气模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(atmosphereModuleToggle && !weatherSystemModuleToggle) || (atmosphereModuleToggle && (atmosphereModule.hideFlags == HideFlags.None))")]
        public AtmosphereModule atmosphereModule;
        
        
        [HideInInspector] public bool volumeCloudOptimizeModuleToggle;
        [FoldoutGroup("昼夜与天气/体积云模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(volumeCloudOptimizeModuleToggle && !weatherSystemModuleToggle) || (volumeCloudOptimizeModuleToggle && (volumeCloudOptimizeModule.hideFlags == HideFlags.None))")]
        public VolumeCloudOptimizeModule volumeCloudOptimizeModule;
        
        
        [HideInInspector] public bool windZoneModuleToggle;
        [FoldoutGroup("昼夜与天气/风场模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(windZoneModuleToggle && !weatherSystemModuleToggle) || (windZoneModuleToggle && (windZoneModule.hideFlags == HideFlags.None))")]
        public WindZoneModule windZoneModule;
        
        [HideInInspector] public bool weatherEffectModuleToggle;
        [FoldoutGroup("昼夜与天气/天气特效模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(weatherEffectModuleToggle && !weatherSystemModuleToggle) || (weatherEffectModuleToggle && (weatherEffectModule.hideFlags == HideFlags.None))")]
        public WeatherEffectModule weatherEffectModule;
        
        
        [HideInInspector] public bool weatherSystemModuleToggle;
        [FoldoutGroup("昼夜与天气/天气列表模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("weatherSystemModuleToggle")]
        public WeatherListModule weatherListModule;

        [HideInInspector] public bool moistAccumulatedWaterModuleToggle;
        [FoldoutGroup("昼夜与天气/湿润积水模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("moistAccumulatedWaterModuleToggle")]
        [ShowIf("@(moistAccumulatedWaterModuleToggle && !weatherSystemModuleToggle) || (moistAccumulatedWaterModuleToggle && (moistAccumulatedWaterModule.hideFlags == HideFlags.None))")]
        public MoistAccumulatedWaterModule moistAccumulatedWaterModule;
        
        [HideInInspector] public bool approxRealtimeGIModuleToggle;
        [FoldoutGroup("昼夜与天气/近似实时光照模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("approxRealtimeGIModuleToggle")]
        [ShowIf("@(approxRealtimeGIModuleToggle && !weatherSystemModuleToggle) || (approxRealtimeGIModuleToggle && (approxRealtimeGIModule.hideFlags == HideFlags.None))")]
        public ApproxRealtimeGIModule approxRealtimeGIModule;
        
        [HideInInspector] public bool fogModuleToggle;
        [FoldoutGroup("昼夜与天气/雾模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("fogModuleToggle")]
        [ShowIf("@(fogModuleToggle && !weatherSystemModuleToggle) || (fogModuleToggle && (fogModule.hideFlags == HideFlags.None))")]
        public FogModule fogModule;
        
        [HideInInspector] public bool lightingScatterModuleToggle;
        [FoldoutGroup("昼夜与天气/光照散射模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("lightingScatterModuleToggle")]
        [ShowIf("@(lightingScatterModuleToggle && !weatherSystemModuleToggle) || (lightingScatterModuleToggle && (lightingScatterModule.hideFlags == HideFlags.None))")]
        public LightingScatterModule lightingScatterModule;
        
        [HideInInspector] public bool postprocessAdjustModuleToggle;
        [FoldoutGroup("昼夜与天气/后处理调整模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("postprocessAdjustModuleToggle")]
        [ShowIf("@(postprocessAdjustModuleToggle && !weatherSystemModuleToggle) || (postprocessAdjustModuleToggle && (postprocessAdjustModule.hideFlags == HideFlags.None))")]
        public PostprocessAdjustModule postprocessAdjustModule;
        
        [HideInInspector] public bool fpsDisplayModuleToggle;
        [FoldoutGroup("实用工具/FPS显示")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("fpsDisplayModuleToggle")]
        public FPSDisplayModule fpsDisplayModule;
        
#if UNITY_EDITOR
        private const bool baiduInputMethodPhraseToggle = true;
        [ShowInInspector][FoldoutGroup("实用工具/百度输入法短语")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("baiduInputMethodPhraseToggle")]
        private BaiduInputMethodPhrase baiduInputMethodPhraseModule;
        
        private const bool packageManagerToggle = true;
        [ShowInInspector][FoldoutGroup("实用工具/包管理器")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("packageManagerToggle")]
        private PackageManager packageManager;
#endif
        
        private float _timeCount;
        
        private static bool _update;
        
        private static bool _isSplitFrameRender;
        
        // private static int _SplitFrameMaxCount = 2;
        
        #endregion

        
        
        #region 事件函数
        
        private void OnEnable()
        {
            gameObject.name = "WorldManager";
            if (Instance == null)
                Instance = this;
            if (Instance != null && Instance != this)
            {
                CoreUtils.Destroy(gameObject);
                Debug.Log("世界管理器只能有一个!");
            }
            
            _skyRenderPass ??= new SkyRenderPass();
            // atmosphereBlendPass ??= new AtmosphereBlendPass();
            _volumeCloudOptimizeShadowRenderPass ??= new VolumeCloudOptimizeShadowRenderPass();
            _postprocessPass ??= new PostprocessPass(); 
            OnValidate();
            
            RenderPipelineManager.beginCameraRendering -= AddRenderPasses;
            RenderPipelineManager.beginCameraRendering += AddRenderPasses;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= AddRenderPasses;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (Time.frameCount < 2)
                return;
#endif
            _skyRenderPass = null;
            // atmosphereBlendPass = null;
            _postprocessPass = null;
            _volumeCloudOptimizeShadowRenderPass = null;
            
            timModuleToggle = false;
            universeBackgroundModuleToggle = false;
            starModuleToggle = false;
            celestialBodyManagerToggle = false;
            atmosphereModuleToggle = false;
            weatherEffectModuleToggle = false;
            windZoneModuleToggle = false;
            fpsDisplayModuleToggle = false;
            moistAccumulatedWaterModuleToggle = false;
            approxRealtimeGIModuleToggle = false;
            fogModuleToggle = false;
            lightingScatterModuleToggle = false;
            postprocessAdjustModuleToggle = false;
// #if UNITY_EDITOR
//             packageManagerToggle = false;
// #endif
            OnValidate();
            
            timeModule = null;
            universeBackgroundModule = null;
            starModule = null;
            celestialBodyManager = null;
            atmosphereModule = null;
            windZoneModule = null;
            weatherEffectModule = null;
            fpsDisplayModule = null;
            moistAccumulatedWaterModule = null;
            approxRealtimeGIModule = null;
            fogModule = null;
            lightingScatterModule = null;
            postprocessAdjustModule = null;
#if UNITY_EDITOR
            baiduInputMethodPhraseModule = null;
            packageManager = null;
#endif
            Instance = null;
        }
        
        private void OnValidate()
        {
            timeModule = AppendOrDestroyModule<TimeModule>(timModuleToggle);
            universeBackgroundModule = AppendOrDestroyModule<UniverseBackgroundModule>(universeBackgroundModuleToggle);
            starModule = AppendOrDestroyModule<StarModule>(starModuleToggle);
            celestialBodyManager = AppendOrDestroyModule<CelestialBodyManager>(celestialBodyManagerToggle);
            atmosphereModule = AppendOrDestroyModule<AtmosphereModule>(atmosphereModuleToggle);
            volumeCloudOptimizeModule = AppendOrDestroyModule<VolumeCloudOptimizeModule>(volumeCloudOptimizeModuleToggle);
            weatherEffectModule = AppendOrDestroyModule<WeatherEffectModule>(weatherEffectModuleToggle);
            windZoneModule = AppendOrDestroyModule<WindZoneModule>(windZoneModuleToggle, true, new Vector3(0,-2,0));
            weatherListModule = AppendOrDestroyModule<WeatherListModule>(weatherSystemModuleToggle);
            moistAccumulatedWaterModule = AppendOrDestroyModule<MoistAccumulatedWaterModule>(moistAccumulatedWaterModuleToggle);
            approxRealtimeGIModule = AppendOrDestroyModule<ApproxRealtimeGIModule>(approxRealtimeGIModuleToggle);
            fogModule = AppendOrDestroyModule<FogModule>(fogModuleToggle);
            lightingScatterModule = AppendOrDestroyModule<LightingScatterModule>(lightingScatterModuleToggle);
            postprocessAdjustModule = AppendOrDestroyModule<PostprocessAdjustModule>(postprocessAdjustModuleToggle);
            fpsDisplayModule = AppendOrDestroyModule<FPSDisplayModule>(fpsDisplayModuleToggle);
#if UNITY_EDITOR
            baiduInputMethodPhraseModule = AppendOrDestroyModule<BaiduInputMethodPhrase>(baiduInputMethodPhraseToggle);
            packageManager = AppendOrDestroyModule<PackageManager>(packageManagerToggle);
#endif
        }
        
        private void Update()
        {
            if (universeBackgroundModule is null) return;
#if UNITY_EDITOR
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
#endif
            
            _timeCount += Time.deltaTime;
            if (_timeCount >= 1f / (float)Instance.universeBackgroundModule.property.renderAsyncUpdateRate && !_isSplitFrameRender)
            {
                _update = true;
                _timeCount = 0;
                _isSplitFrameRender = true;
            }
            else
            {
                _update = false;
            }
            if(!Instance.universeBackgroundModule.property.renderUseAsyncRender)
                _update = true;
            
            if (Instance.timeModule is not null)
                Instance.timeModule.update = _update;
            if (Instance.starModule is not null)
                Instance.starModule.update = _update;
            if(Instance.celestialBodyManager is not null)
                Instance.celestialBodyManager.update = _update;
            if (Instance.atmosphereModule is not null)
                Instance.atmosphereModule.update = _update;
            if (Instance.volumeCloudOptimizeModule is not null)
                Instance.volumeCloudOptimizeModule.update = _update;
            if (Instance.windZoneModule is not null)
                Instance.windZoneModule._Update = _update;
            if (Instance.weatherEffectModule is not null)
                Instance.weatherEffectModule.update = _update;
            if(Instance.weatherEffectModule?.rainEffect is not null)
                Instance.weatherEffectModule.rainEffect.update = _update;
            if(Instance.weatherEffectModule?.rainSpatterEffect is not null)
                Instance.weatherEffectModule.rainSpatterEffect.update = _update;
            if(Instance.weatherEffectModule?.snowEffect is not null)
                Instance.weatherEffectModule.snowEffect.update = _update;
            if(Instance.weatherEffectModule?.lightningEffect is not null)
                Instance.weatherEffectModule.lightningEffect.update = _update;
            if (Instance.weatherListModule is not null)
                Instance.weatherListModule.update = _update;
            if (Instance.moistAccumulatedWaterModule is not null)
                Instance.moistAccumulatedWaterModule.update = _update;
            if (Instance.approxRealtimeGIModule is not null)
                Instance.approxRealtimeGIModule.update = _update;
            if(Instance.fogModule is not null)
                Instance.fogModule.update = _update;
            if(Instance.lightingScatterModule is not null)
                Instance.lightingScatterModule.update = _update;
            if(Instance.postprocessAdjustModule is not null)
                Instance.postprocessAdjustModule.update = _update;

        }
        
        #endregion


        
        #region 渲染通道
        
        private SkyRenderPass _skyRenderPass;
        
        private VolumeCloudOptimizeShadowRenderPass _volumeCloudOptimizeShadowRenderPass;
        
        private PostprocessPass _postprocessPass;
        
        private void AddRenderPasses(ScriptableRenderContext context,Camera cam)
        {
            if (cam.cameraType != CameraType.Game &&
                cam.cameraType != CameraType.SceneView &&
                cam.cameraType != CameraType.Reflection ||
                cam.name == "OcclusionCamera" || 
                !isActiveAndEnabled)
                return;
            
            ScriptableRenderer scriptableRenderer = cam.GetUniversalAdditionalCameraData().scriptableRenderer;
            scriptableRenderer.EnqueuePass(_skyRenderPass);
            scriptableRenderer.EnqueuePass(_volumeCloudOptimizeShadowRenderPass);
            scriptableRenderer.EnqueuePass(_postprocessPass);
        }
        
        private class SkyRenderPass : ScriptableRenderPass
        {

            #region 字段

            private static int _splitFrameCount;
            
            private Matrix4x4 _viewMatrix;
            
            private float3 _cameraPosition;
            
            private RTHandle _activeRT;

            // private Matrix4x4 _ViewMatrix_Inv;
            
            #endregion


            #region 事件函数
            
            public SkyRenderPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            }
            
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (_update)
                    Instance?.volumeCloudOptimizeModule?.RenderCloudMap();
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                
                if (Instance?.universeBackgroundModule is null) 
                    return;

                CommandBuffer cmd = CommandBufferPool.Get("WorldSystem: SkyRender");
                var dataCamera = renderingData.cameraData.camera;
                
                if (Instance.universeBackgroundModule.property.renderUseAsyncRender)
                {
                    //我现在希望分帧渲染只分为两帧,而不是分为多个帧,这样可以避免运动矢量的累加,降低复杂度
                    //我现在希望分帧渲染不止分为两帧,使用多帧分帧渲染,继续提高性能
                    
                    // profilingSampler.Begin(cmd);
                    //修改矩阵以扩大视野
                    float cameraAspect = dataCamera.pixelRect.width / dataCamera.pixelRect.height;
                    
                    //缓存分帧渲染需要的ViewMatrix,Position,保证它在分帧渲染的过程中保持不变
                    if (_splitFrameCount == 0)
                    {
                        _viewMatrix = dataCamera.worldToCameraMatrix;
                        _cameraPosition = dataCamera.transform.position;
                    }
                    
                    //重新设置矩阵,用于扩大视野和维持分帧渲染过程中,ViewMatrix不变
                    var projectionMatrix = Matrix4x4.Perspective(Instance.universeBackgroundModule.property.renderAsyncFOV, cameraAspect, dataCamera.nearClipPlane, dataCamera.farClipPlane);
                    projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);
                    RenderingUtils.SetViewAndProjectionMatrices(cmd,_viewMatrix, projectionMatrix, true);
                    
                    Instance.universeBackgroundModule.SetupTaaMatrices_PerFrame(cmd,dataCamera.worldToCameraMatrix, projectionMatrix);

                    if (_splitFrameCount < 2 && _isSplitFrameRender)
                    {
                        //初始化分帧缓存RT,分帧渲染的结果储存在此RT
                        RenderingUtils.ReAllocateIfNeeded(ref Instance.universeBackgroundModule.SkyRT, 
                                new RenderTextureDescriptor(
                                    renderingData.cameraData.cameraTargetDescriptor.width >> (int)Instance.universeBackgroundModule.property.renderResolutionOptions,
                                    renderingData.cameraData.cameraTargetDescriptor.height >> (int)Instance.universeBackgroundModule.property.renderResolutionOptions,
                                    RenderTextureFormat.ARGBHalf), 
                                name: "SkyRT");
                        
                        //计算当前渲染的矩形
                        Rect renderTargetRect = new Rect(0,0,Instance.universeBackgroundModule.SkyRT.rt.width, Instance.universeBackgroundModule.SkyRT.rt.height);
                        float Width = renderTargetRect.width / 2;
                        Rect currentRect = new Rect(_splitFrameCount * Width, 0, Width, renderTargetRect.height);
                        cmd.EnableScissorRect(currentRect);
                        
                        //渲染天空盒内容到分帧缓存RT
                        //渲染背景
                        _activeRT = Instance.universeBackgroundModule.RenderBackground(cmd, ref renderingData, Instance.universeBackgroundModule.SkyRT);
                        //渲染大气
                        Instance.atmosphereModule?.RenderAtmosphere(cmd, ref renderingData, _activeRT, currentRect);
                        //体积云
                        Instance.volumeCloudOptimizeModule?.RenderVolumeCloud(cmd, ref renderingData, _activeRT);
                        
                        _activeRT = Instance.universeBackgroundModule.RenderUpScaleAndTaa(cmd,ref renderingData, _activeRT,_viewMatrix, projectionMatrix, _splitFrameCount);
                        //我们这里将星星和星体放在后面渲染,使用特别的方式正确混合,这是因为,大气体积云我们可以降低分辨率渲染,而星星星体为保持清晰度不可降低分辨率
                        //渲染星星
                        Instance.starModule?.RenderStar(cmd, ref renderingData);
                        
                        // //渲染星体
                        Instance.celestialBodyManager?.RenderCelestialBodyList(cmd, _cameraPosition);
                        
                        //取消分帧矩形
                        cmd.DisableScissorRect();
                        
                        // EditorApplication.isPaused = true;
                        //累加计数器
#if UNITY_EDITOR
                        if (!EditorApplication.isPaused)
                        {
                            _splitFrameCount++;
                        }
#else
                            _splitFrameCount++;
#endif
                        
                        //当分裂计数器等于最大计数时,说明已经完成分帧渲染, 注意: 这个分帧渲染的结果是1帧之前的结果,必须将其修正到当前帧
                        if (_splitFrameCount == 2)
                        {
                            _activeRT = Instance.universeBackgroundModule?.RenderFixupLate(cmd, _activeRT);
                            
                            RenderingUtils.ReAllocateIfNeeded(ref Instance.universeBackgroundModule.SplitFrameRT, _activeRT.rt.descriptor, name: "SplitFrameRT");
                            cmd.CopyTexture(_activeRT, Instance.universeBackgroundModule.SplitFrameRT);
                            _activeRT = Instance.universeBackgroundModule.SplitFrameRT;
                            
                            _splitFrameCount = 0;
                            _isSplitFrameRender = false;
                        }
                        else
                        {
                            //修正由于异步渲染造成的不跟手
                            if (Instance.universeBackgroundModule.SplitFrameRT != null)
                                _activeRT = Instance.universeBackgroundModule.RenderFixupLate(cmd, Instance.universeBackgroundModule.SplitFrameRT);
                        }
                        
                        
                    }
                    else
                    {
                        //修正由于异步渲染造成的不跟手
                        if (Instance.universeBackgroundModule.SplitFrameRT != null)
                            _activeRT = Instance.universeBackgroundModule.RenderFixupLate(cmd, Instance.universeBackgroundModule.SplitFrameRT);
                    }
                    
                    //恢复矩阵
                    projectionMatrix = GL.GetGPUProjectionMatrix(dataCamera.projectionMatrix, true); 
                    RenderingUtils.SetViewAndProjectionMatrices(cmd,  dataCamera.worldToCameraMatrix, projectionMatrix, true); 
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    
                    Instance.volumeCloudOptimizeModule?.RenderAddCloudMaskToDepth(cmd,ref renderingData, Instance.universeBackgroundModule.SplitFrameRT);
                    
                    //修正视野范围
                    if(_activeRT != null) 
                        Instance.universeBackgroundModule.RenderFixupLateBlit(cmd,ref renderingData ,_activeRT, renderingData.cameraData.renderer.cameraColorTargetHandle);
                    
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
                else
                {
                    cmd.DisableScissorRect();

                    RenderingUtils.ReAllocateIfNeeded(ref Instance.universeBackgroundModule.SkyRT, 
                            new RenderTextureDescriptor(
                                renderingData.cameraData.cameraTargetDescriptor.width >> (int)Instance.universeBackgroundModule.property.renderResolutionOptions,
                                renderingData.cameraData.cameraTargetDescriptor.height >> (int)Instance.universeBackgroundModule.property.renderResolutionOptions,
                                RenderTextureFormat.ARGBHalf), 
                            name: "SkyRT");
                    //渲染背景
                    _activeRT = Instance.universeBackgroundModule.RenderBackground(cmd, ref renderingData, Instance.universeBackgroundModule.SkyRT);
                    //渲染大气
                    Instance.atmosphereModule?.RenderAtmosphere(cmd, ref renderingData, _activeRT);
                    //体积云
                    Instance.volumeCloudOptimizeModule?.RenderVolumeCloud(cmd, ref renderingData, _activeRT);

                    _activeRT = Instance.universeBackgroundModule.RenderUpScaleAndTaa(cmd,ref renderingData, _activeRT, dataCamera.worldToCameraMatrix, dataCamera.projectionMatrix, _splitFrameCount);
                    
                    Instance.volumeCloudOptimizeModule?.RenderAddCloudMaskToDepth(cmd,ref renderingData, _activeRT);
                    cmd.SetRenderTarget(_activeRT);
                    //我们这里将星星和星体放在后面渲染,使用特别的方式正确混合,这是因为,大气体积云我们可以降低分辨率渲染,而星星星体为保持清晰度不可降低分辨率
                    //渲染星星
                    Instance.starModule?.RenderStar(cmd, ref renderingData);
                    //渲染星体
                    Instance.celestialBodyManager?.RenderCelestialBodyList(cmd, ref renderingData);
                    
                    Blitter.BlitCameraTexture(cmd, _activeRT, renderingData.cameraData.renderer.cameraColorTargetHandle);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
                
                CommandBufferPool.Release(cmd);
            }
            
            #endregion

            
        }
        
        private class VolumeCloudOptimizeShadowRenderPass : ScriptableRenderPass
        {
            public VolumeCloudOptimizeShadowRenderPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            }
                
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("WorldSystem: VolumeCloudOptimizeShadowRender");
                    
                Instance?.volumeCloudOptimizeModule?.RenderVolumeCloudShadow(cmd,ref renderingData);
                    
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
        
	    public class PostprocessPass : ScriptableRenderPass
		{
            public PostprocessPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }
            
            private ProfilingSampler _fogProfilingSampler = new ProfilingSampler("WorldSystem: Fog");
            private ProfilingSampler _scatterProfilingSampler = new ProfilingSampler("WorldSystem: LightingScatter");

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
                CommandBuffer cmd = CommandBufferPool.Get("WorldSystem: Postprocess");
                _fogProfilingSampler.Begin(cmd);
                WorldManager.Instance.fogModule?.RenderFog(cmd, ref renderingData);
                _fogProfilingSampler.End(cmd);
                
                _scatterProfilingSampler.Begin(cmd);
                WorldManager.Instance.lightingScatterModule?.RenderLightingScatter(cmd, ref renderingData);
                _scatterProfilingSampler.End(cmd);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
        
        #endregion
        
        
    }
}