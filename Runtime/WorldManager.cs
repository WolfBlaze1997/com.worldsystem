using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSystem.Runtime
{
    public partial class WorldManager
    {
#if UNITY_EDITOR

        #region 编辑器
        
        #region 模块开关
        [PropertyOrder(-100)]
        [ShowIf("timModuleToggle")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Large, Name = "时间模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void TimModuleToggle_Off()
        {
            timModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("timModuleToggle")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Large, Name = "时间模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void TimModuleToggle_On()
        {
            timModuleToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("universeBackgroundModuleToggle")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Large, Name = "宇宙背景模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void UniverseBackgroundModuleToggle_Off()
        {
            starModuleToggle = false;
            celestialBodyManagerToggle = false;
            atmosphereModuleToggle = false;
            universeBackgroundModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("universeBackgroundModuleToggle")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Large, Name = "宇宙背景模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void UniverseBackgroundModuleToggle_On()
        {
            universeBackgroundModuleToggle = true;
            OnValidate();
        }
        

        [PropertyOrder(-100)]
        [ShowIf("starModuleToggle")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Large, Name = "星星模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void StarModuleToggle_Off()
        {
            starModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("starModuleToggle")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Large, Name = "星星模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void StarModuleToggle_On()
        {
            universeBackgroundModuleToggle = true;
            starModuleToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("celestialBodyManagerToggle")]
        [VerticalGroup("Split/04")]
        [Button(ButtonSizes.Large, Name = "星体模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void CelestialBodyModuleToggle_Off()
        {
            celestialBodyManagerToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("celestialBodyManagerToggle")]
        [VerticalGroup("Split/04")]
        [Button(ButtonSizes.Large, Name = "星体模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void CelestialBodyModuleToggle_On()
        {
            universeBackgroundModuleToggle = true;
            celestialBodyManagerToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("atmosphereModuleToggle")]
        [VerticalGroup("Split/05")]
        [Button(ButtonSizes.Large, Name = "大气模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void AtmosphereModuleToggle_Off()
        {
            atmosphereModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("atmosphereModuleToggle")]
        [VerticalGroup("Split/05")]
        [Button(ButtonSizes.Large, Name = "大气模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void AtmosphereModuleToggle_On()
        {
            universeBackgroundModuleToggle = true;
            atmosphereModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("volumeCloudOptimizeModuleToggle")]
        [HorizontalGroup("Split02")]
        [VerticalGroup("Split02/01")]
        [Button(ButtonSizes.Large, Name = "简化体积云_V1_1_20240604已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void VolumeCloudOptimizeModuleToggle_Off()
        {
            volumeCloudOptimizeModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("volumeCloudOptimizeModuleToggle")]
        [VerticalGroup("Split02/01")]
        [Button(ButtonSizes.Large, Name = "简化体积云_V1_1_20240604已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void VolumeCloudOptimizeModuleToggle_On()
        {
            volumeCloudOptimizeModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("windZoneModuleToggle")]
        [VerticalGroup("Split02/02")]
        [Button(ButtonSizes.Large, Name = "风区模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void WindZoneModuleToggle_Off()
        {
            windZoneModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("windZoneModuleToggle")]
        [VerticalGroup("Split02/02")]
        [Button(ButtonSizes.Large, Name = "风区模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WindZoneModuleToggle_On()
        {
            windZoneModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("weatherEffectModuleToggle")]
        [VerticalGroup("Split02/03")]
        [Button(ButtonSizes.Large, Name = "天气效果模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void WeatherEffectModuleToggle_Off()
        {
            weatherEffectModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("weatherEffectModuleToggle")]
        [VerticalGroup("Split02/03")]
        [Button(ButtonSizes.Large, Name = "天气效果模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WeatherEffectModuleToggle_On()
        {
            weatherEffectModuleToggle = true;
            OnValidate();
        }
        
        
        
        [PropertyOrder(-100)]
        [ShowIf("weatherSystemModuleToggle")]
        [VerticalGroup("Split02/04")]
        [Button(ButtonSizes.Large, Name = "天气系统模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void WeatherSystemModuleToggle_Off()
        {
            weatherSystemModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("weatherSystemModuleToggle")]
        [VerticalGroup("Split02/04")]
        [Button(ButtonSizes.Large, Name = "天气系统模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WeatherSystemModuleToggle_On()
        {
            weatherSystemModuleToggle = true;
            OnValidate();
        }
        
        
        
        [PropertyOrder(-100)]
        [ShowIf("fpsDisplayModuleToggle")]
        [HorizontalGroup("Split03",0.2f)]
        [VerticalGroup("Split03/01")]
        [Button(ButtonSizes.Large, Name = "FPS模块已开启"), GUIColor(0.3f, 1f, 0.3f)]
        private void FpsDisplayModuleToggle_Off()
        {
            fpsDisplayModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("fpsDisplayModuleToggle")]
        [VerticalGroup("Split03/01")]
        [Button(ButtonSizes.Large, Name = "FPS模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void FpsDisplayModuleToggle_On()
        {
            fpsDisplayModuleToggle = true;
            OnValidate();
        }
        
        #endregion

        #region GUI帮助函数
        [HideInInspector] public bool hideFlagToggle;

        
        [Button(ButtonSizes.Medium, Name = "开发者模式已开启")]
        [ShowIf("hideFlagToggle")]
        [GUIColor("white")]
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
        
        [Button(ButtonSizes.Medium, Name = "开发者模式已关闭")]
        [HideIf("hideFlagToggle")]
        [GUIColor("gray")]
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
            Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem//Textures/Icon/day-night-icon.png");

            if (transform.childCount > 0)
                BroadcastMessage("DrawGizmos");
            
        }
        #endregion
        
        #endregion
        
#endif

    }
    
    
    [AddComponentMenu("WorldSystem/WorldManager")]
    [ExecuteAlways]
    public partial class WorldManager : MonoBehaviour
    {
        /// 备忘录!
        /// 1- UI改进,需要将部分模块聚合成一个模块,子模块用蓝色标识
        /// 2- 注意CPU时间, 将最大可能改为异步执行
        ///

        #region 字段
        public static WorldManager Instance { get; set; }
        
        [HideInInspector] public bool timModuleToggle;
        [FoldoutGroup("时间模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("timModuleToggle")]
        public TimeModule timeModule;

        [HideInInspector] public bool universeBackgroundModuleToggle;
        [FoldoutGroup("宇宙背景模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(universeBackgroundModuleToggle && !weatherSystemModuleToggle) || (universeBackgroundModuleToggle && (universeBackgroundModule.hideFlags == HideFlags.None))")]
        public UniverseBackgroundModule universeBackgroundModule;

        [HideInInspector] public bool starModuleToggle;
        [FoldoutGroup("星星模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(starModuleToggle && !weatherSystemModuleToggle) || (starModuleToggle && (starModule.hideFlags == HideFlags.None))")]
        public StarModule starModule;

        [HideInInspector] public bool celestialBodyManagerToggle;
        [FoldoutGroup("星体模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(celestialBodyManagerToggle && !weatherSystemModuleToggle) || (celestialBodyManagerToggle && (celestialBodyManager.hideFlags == HideFlags.None))")]
        public CelestialBodyManager celestialBodyManager;

        
        [HideInInspector] public bool atmosphereModuleToggle;
        [FoldoutGroup("大气模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(atmosphereModuleToggle && !weatherSystemModuleToggle) || (atmosphereModuleToggle && (atmosphereModule.hideFlags == HideFlags.None))")]
        public AtmosphereModule atmosphereModule;
        
        
        [HideInInspector] public bool volumeCloudOptimizeModuleToggle;
        [FoldoutGroup("体积云模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(volumeCloudOptimizeModuleToggle && !weatherSystemModuleToggle) || (volumeCloudOptimizeModuleToggle && (volumeCloudOptimizeModule.hideFlags == HideFlags.None))")]
        public VolumeCloudOptimizeModule volumeCloudOptimizeModule;
        
        
        [HideInInspector] public bool windZoneModuleToggle;
        [FoldoutGroup("风区模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(windZoneModuleToggle && !weatherSystemModuleToggle) || (windZoneModuleToggle && (windZoneModule.hideFlags == HideFlags.None))")]
        public WindZoneModule windZoneModule;
        
        [HideInInspector] public bool weatherEffectModuleToggle;
        [FoldoutGroup("天气效果模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("@(weatherEffectModuleToggle && !weatherSystemModuleToggle) || (weatherEffectModuleToggle && (weatherEffectModule.hideFlags == HideFlags.None))")]
        public WeatherEffectModule weatherEffectModule;
        
        
        [HideInInspector] public bool weatherSystemModuleToggle;
        [FoldoutGroup("天气系统模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("weatherSystemModuleToggle")]
        public WeatherSystemModule weatherSystemModule;

        
        [HideInInspector] public bool fpsDisplayModuleToggle;
        [FoldoutGroup("FPS显示")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("fpsDisplayModuleToggle")]
        public FPSDisplayModule fpsDisplayModule;
        
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
            
            skyRenderPass ??= new SkyRenderPass();
            atmosphereBlendPass ??= new AtmosphereBlendPass();
            
            volumeCloudOptimizeShadowRenderPass ??= new VolumeCloudOptimizeShadowRenderPass();
            // volumeCloudOptimizeRenderPass ??= new VolumeCloudOptimizeRenderPass();
          
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
            skyRenderPass = null;
            atmosphereBlendPass = null;
            volumeCloudOptimizeShadowRenderPass = null;
            // volumeCloudOptimizeRenderPass = null;
            
            timModuleToggle = false;
            universeBackgroundModuleToggle = false;
            starModuleToggle = false;
            celestialBodyManagerToggle = false;
            atmosphereModuleToggle = false;
            weatherEffectModuleToggle = false;
            windZoneModuleToggle = false;
            fpsDisplayModuleToggle = false;
            OnValidate();
            
            timeModule = null;
            universeBackgroundModule = null;
            starModule = null;
            celestialBodyManager = null;
            atmosphereModule = null;
            windZoneModule = null;
            weatherEffectModule = null;
            fpsDisplayModule = null;
            
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
            weatherSystemModule = AppendOrDestroyModule<WeatherSystemModule>(weatherSystemModuleToggle);
            
            fpsDisplayModule = AppendOrDestroyModule<FPSDisplayModule>(fpsDisplayModuleToggle);
            
        }
        #endregion

        
        private int _frameID;
        private int _updateCount;
#if UNITY_EDITOR
        private void Update()
        {
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            if (Application.isPlaying) return;
            
        }
        private void FixedUpdate()
        {
            if (Time.frameCount == _frameID) return;
            
            
            //分帧器,将不同的操作分散到不同的帧,提高帧率稳定性
            if (_updateCount % 4 == 0)
            {
                _Render_Atmosphere_VolumeCloud = true;
            }
            if (_updateCount % 4 == 1)
            {
                _Render_Taa_Star_CelestialBody = true;
            }
            _updateCount++;
            
            
            _frameID = Time.frameCount;
        }
#else

#endif
        
        

        private void AddRenderPasses(ScriptableRenderContext context,Camera cam)
        {
            if (cam.cameraType != CameraType.Game &&
                cam.cameraType != CameraType.SceneView &&
                cam.cameraType != CameraType.Reflection ||
                cam.name == "OcclusionCamera" || 
                !isActiveAndEnabled)
                return;
            
            ScriptableRenderer scriptableRenderer = cam.GetUniversalAdditionalCameraData().scriptableRenderer;
            scriptableRenderer.EnqueuePass(skyRenderPass);
            scriptableRenderer.EnqueuePass(volumeCloudOptimizeShadowRenderPass);
            scriptableRenderer.EnqueuePass(atmosphereBlendPass);
        }
        
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

        
        private SkyRenderPass skyRenderPass;
        private VolumeCloudOptimizeShadowRenderPass volumeCloudOptimizeShadowRenderPass;
        private AtmosphereBlendPass atmosphereBlendPass;

        private static bool _Render_Atmosphere_VolumeCloud;
        private static bool _Render_Taa_Star_CelestialBody;
        
        private class SkyRenderPass : ScriptableRenderPass
        {
            public SkyRenderPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            }
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                Instance?.volumeCloudOptimizeModule?.RenderCloudMap();
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Instance.universeBackgroundModule.SetupTaaMatrices(renderingData);

                if (Instance?.universeBackgroundModule is null) return;
                var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
                CommandBuffer cmd = CommandBufferPool.Get("Test: SkyRender");

                if (_Render_Atmosphere_VolumeCloud || Time.frameCount < 2 
#if UNITY_EDITOR                 
                    || !Application.isPlaying
#endif
                    )
                {
                    _Render_Atmosphere_VolumeCloud = false;
                    //渲染背景
                    _ActiveRT = Instance.universeBackgroundModule.RenderBackground(cmd, ref renderingData);
                    //渲染大气
                    Instance?.atmosphereModule?.RenderAtmosphere(cmd, ref renderingData, _ActiveRT);
                    //体积云
                    Instance?.volumeCloudOptimizeModule?.RenderVolumeCloud(cmd,ref renderingData, _ActiveRT);

               
                    if (_Render_Taa_Star_CelestialBody || Time.frameCount < 2
    #if UNITY_EDITOR
                        || !Application.isPlaying
    #endif
                       )
                    {
                        Debug.Log(_Render_Atmosphere_VolumeCloud + "     " + _Render_Taa_Star_CelestialBody);
                        _Render_Taa_Star_CelestialBody = false;
                        var TaaDescriptor = new RenderTextureDescriptor(source.rt.descriptor.width,
                            source.rt.descriptor.height, RenderTextureFormat.ARGBHalf);
                        _ActiveRT = Instance.universeBackgroundModule.RenderUpScaleAndTaa_1(cmd, ref renderingData, _ActiveRT, TaaDescriptor);
                        _ActiveRT = Instance.universeBackgroundModule.RenderUpScaleAndTaa_2(cmd, _ActiveRT, TaaDescriptor);
                        //我们这里将星星和星体放在后面渲染,使用特别的方式正确混合,这是因为,大气体积云我们可以降低分辨率渲染,而星星星体为保持清晰度不可降低分辨率
                        //渲染星星
                        Instance?.starModule?.RenderStar(cmd, ref renderingData);
                        //渲染星体
                        Instance?.celestialBodyManager?.RenderCelestialBodyList(cmd, ref renderingData);
                    }
                    
                    
                }
                else
                {
                    _ActiveRT = Instance?.volumeCloudOptimizeModule?.RenderFixupLate(cmd,ref renderingData, _ActiveRT);
                }



                //输出到Camera
                Blitter.BlitCameraTexture(cmd, _ActiveRT, source);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            private RTHandle _ActiveRT;
        }
        
        private class AtmosphereBlendPass : ScriptableRenderPass
        {

            public AtmosphereBlendPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Test: AtmosphereBlend");
                
                Instance?.atmosphereModule?.RenderAtmosphereBlend(cmd, ref renderingData);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
        
        private class VolumeCloudOptimizeShadowRenderPass : ScriptableRenderPass
        {
            public VolumeCloudOptimizeShadowRenderPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            }
                
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Test: VolumeCloudOptimizeShadowRender");
                    
                Instance?.volumeCloudOptimizeModule?.RenderVolumeCloudShadow(cmd,ref renderingData);
                    
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        
    }

        
    
        
    
}