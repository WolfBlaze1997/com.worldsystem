using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

// using WorldSystem.Editor;

namespace WorldSystem.Runtime
{
    public partial class WorldManager
    {
#if UNITY_EDITOR

        #region 编辑器
        
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
        [ShowIf("windZoneModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/02")]
        [Button(ButtonSizes.Large, Name = "风场模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void WindZoneModuleToggle_Off()
        {
            windZoneModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("windZoneModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/02")]
        [Button(ButtonSizes.Large, Name = "风场模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WindZoneModuleToggle_On()
        {
            windZoneModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/03")]
        [Button(ButtonSizes.Large, Name = "天气特效模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void WeatherEffectModuleToggle_Off()
        {
            weatherEffectModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("weatherEffectModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/03")]
        [Button(ButtonSizes.Large, Name = "天气特效模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WeatherEffectModuleToggle_On()
        {
            weatherEffectModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("weatherSystemModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/04")]
        [Button(ButtonSizes.Large, Name = "天气列表模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void WeatherSystemModuleToggle_Off()
        {
            weatherSystemModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("weatherSystemModuleToggle")]
        [VerticalGroup("昼夜与天气/Split/04")]
        [Button(ButtonSizes.Large, Name = "天气列表模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void WeatherSystemModuleToggle_On()
        {
            weatherSystemModuleToggle = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("universeBackgroundModuleToggle")]
        [HorizontalGroup("昼夜与天气/Split02")]
        [VerticalGroup("昼夜与天气/Split02/01")]
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
        [VerticalGroup("昼夜与天气/Split02/01")]
        [Button(ButtonSizes.Large, Name = "渲染设置与背景"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void UniverseBackgroundModuleToggle_On()
        {
            universeBackgroundModuleToggle = true;
            OnValidate();
        }
        

        [PropertyOrder(-100)]
        [ShowIf("starModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/02")]
        [Button(ButtonSizes.Large, Name = "星星模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void StarModuleToggle_Off()
        {
            starModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("starModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/02")]
        [Button(ButtonSizes.Large, Name = "星星模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void StarModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            starModuleToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("celestialBodyManagerToggle")]
        [VerticalGroup("昼夜与天气/Split02/03")]
        [Button(ButtonSizes.Large, Name = "天体模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void CelestialBodyModuleToggle_Off()
        {
            celestialBodyManagerToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("celestialBodyManagerToggle")]
        [VerticalGroup("昼夜与天气/Split02/03")]
        [Button(ButtonSizes.Large, Name = "天体模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void CelestialBodyModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            celestialBodyManagerToggle = true;
            OnValidate();
        }

        
        [PropertyOrder(-100)]
        [ShowIf("atmosphereModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/04")]
        [Button(ButtonSizes.Large, Name = "大气模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void AtmosphereModuleToggle_Off()
        {
            atmosphereModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("atmosphereModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/04")]
        [Button(ButtonSizes.Large, Name = "大气模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void AtmosphereModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            atmosphereModuleToggle = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("volumeCloudOptimizeModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/05")]
        [Button(ButtonSizes.Large, Name = "体积云模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void VolumeCloudOptimizeModuleToggle_Off()
        {
            volumeCloudOptimizeModuleToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("volumeCloudOptimizeModuleToggle")]
        [VerticalGroup("昼夜与天气/Split02/05")]
        [Button(ButtonSizes.Large, Name = "体积云模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void VolumeCloudOptimizeModuleToggle_On()
        {
            UniverseBackgroundModuleToggle_On();
            volumeCloudOptimizeModuleToggle = true;
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
        [PropertyOrder(-100)]
        [ShowIf("packageManagerToggle")]
        [HorizontalGroup("实用工具/Split03",0.195f)]
        [VerticalGroup("实用工具/Split03/02")]
        [Button(ButtonSizes.Large, Name = "包管理器模块"), GUIColor(0.3f, 1f, 0.3f)]
        private void PackageManagerToggle_Off()
        {
            packageManagerToggle = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("packageManagerToggle")]
        [VerticalGroup("实用工具/Split03/02")]
        [Button(ButtonSizes.Large, Name = "包管理器模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void PackageManagerToggle_On()
        {
            packageManagerToggle = true;
            OnValidate();
        }
#endif
        
        
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
            Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem/Textures/Icon/day-night-icon.png");

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
        [FormerlySerializedAs("weatherSystemModule")] [FoldoutGroup("昼夜与天气/天气列表模块")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("weatherSystemModuleToggle")]
        public WeatherListModule weatherListModule;

        
        [HideInInspector] public bool fpsDisplayModuleToggle;
        [FoldoutGroup("实用工具/FPS显示")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("fpsDisplayModuleToggle")]
        public FPSDisplayModule fpsDisplayModule;
        
#if UNITY_EDITOR
        [HideInInspector] public bool packageManagerToggle;
        [FoldoutGroup("实用工具/包管理器")][InlineEditor(InlineEditorObjectFieldModes.Hidden)][ShowIf("packageManagerToggle")]
        public PackageManager packageManager;
#endif
        
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
            
            timModuleToggle = false;
            universeBackgroundModuleToggle = false;
            starModuleToggle = false;
            celestialBodyManagerToggle = false;
            atmosphereModuleToggle = false;
            weatherEffectModuleToggle = false;
            windZoneModuleToggle = false;
            fpsDisplayModuleToggle = false;
#if UNITY_EDITOR
            packageManagerToggle = false;
#endif
            OnValidate();
            
            timeModule = null;
            universeBackgroundModule = null;
            starModule = null;
            celestialBodyManager = null;
            atmosphereModule = null;
            windZoneModule = null;
            weatherEffectModule = null;
            fpsDisplayModule = null;
#if UNITY_EDITOR
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
            
            fpsDisplayModule = AppendOrDestroyModule<FPSDisplayModule>(fpsDisplayModuleToggle);
            
#if UNITY_EDITOR
            packageManager = AppendOrDestroyModule<PackageManager>(packageManagerToggle);
#endif
        }
        #endregion

        private float _timeCount;
        private static bool _Update;
        private static bool _IsSplitFrameRender;
        // private static int _SplitFrameMaxCount = 2;
        private void Update()
        {
            if (universeBackgroundModule is null) return;
#if UNITY_EDITOR
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
#endif
            
            _timeCount += Time.deltaTime;
            if (_timeCount >= 1f / (float)Instance.universeBackgroundModule.property._Render_AsyncUpdateRate && !_IsSplitFrameRender)
            {
                _Update = true;
                _timeCount = 0;
                _IsSplitFrameRender = true;
            }
            else
            {
                _Update = false;
            }
            if(!Instance.universeBackgroundModule.property._Render_UseAsyncRender)
                _Update = true;
            
            if (Instance.timeModule is not null)
                Instance.timeModule._Update = _Update;
            if (Instance.starModule is not null)
                Instance.starModule._Update = _Update;
            if(Instance.celestialBodyManager is not null)
                Instance.celestialBodyManager._Update = _Update;
            if (Instance.atmosphereModule is not null)
                Instance.atmosphereModule._Update = _Update;
            if (Instance.volumeCloudOptimizeModule is not null)
                Instance.volumeCloudOptimizeModule._Update = _Update;
            if (Instance.windZoneModule is not null)
                Instance.windZoneModule._Update = _Update;
            if (Instance.weatherEffectModule is not null)
                Instance.weatherEffectModule._Update = _Update;
            if(Instance.weatherEffectModule?.rainEffect is not null)
                Instance.weatherEffectModule.rainEffect._Update = _Update;
            if(Instance.weatherEffectModule?.snowEffect is not null)
                Instance.weatherEffectModule.snowEffect._Update = _Update;
            if(Instance.weatherEffectModule?.lightningEffect is not null)
                Instance.weatherEffectModule.lightningEffect._Update = _Update;
            if (Instance.weatherListModule is not null)
                Instance.weatherListModule._Update = _Update;
        }

        
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
        
        private class SkyRenderPass : ScriptableRenderPass
        {
            public SkyRenderPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            }
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (_Update)
                    Instance?.volumeCloudOptimizeModule?.RenderCloudMap();
            }

            public static int _SplitFrameCount;
            private Matrix4x4 _ViewMatrix;
            private Matrix4x4 _ViewMatrix_Inv;
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                
                if (Instance?.universeBackgroundModule is null) 
                    return;

                CommandBuffer cmd = CommandBufferPool.Get("Test: SkyRender");
                var dataCamera = renderingData.cameraData.camera;
                
                if (Instance.universeBackgroundModule.property._Render_UseAsyncRender)
                {
                    //我现在希望分帧渲染只分为两帧,而不是分为多个帧,这样可以避免运动矢量的累加,降低复杂度
                    //我现在希望分帧渲染不止分为两帧,使用多帧分帧渲染,继续提高性能
                    
                    profilingSampler.Begin(cmd);
                    //修改矩阵以扩大视野
                    float cameraAspect = dataCamera.pixelRect.width / dataCamera.pixelRect.height;
                    
                    //缓存分帧渲染需要的ViewMatrix,保证它在分帧渲染的过程中保持不变
                    if (_SplitFrameCount == 0)
                        _ViewMatrix = dataCamera.worldToCameraMatrix;
                    
                    //重新设置矩阵,用于扩大视野和维持分帧渲染过程中,ViewMatrix不变
                    var projectionMatrix = Matrix4x4.Perspective(Instance.universeBackgroundModule.property._Render_AsyncFOV, cameraAspect, dataCamera.nearClipPlane, dataCamera.farClipPlane);
                    projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);
                    RenderingUtils.SetViewAndProjectionMatrices(cmd,_ViewMatrix, projectionMatrix, true);
                    
                    
                    Instance.universeBackgroundModule.SetupTaaMatrices_PerFrame(cmd,dataCamera.worldToCameraMatrix, projectionMatrix);

                    if (_SplitFrameCount < 2 && _IsSplitFrameRender)
                    {
                        //初始化分帧缓存RT,分帧渲染的结果储存在此RT
                        RenderingUtils.ReAllocateIfNeeded(ref Instance.universeBackgroundModule.skyRT, 
                                new RenderTextureDescriptor(
                                    renderingData.cameraData.cameraTargetDescriptor.width >> (int)Instance.universeBackgroundModule.property._Render_ResolutionOptions,
                                    renderingData.cameraData.cameraTargetDescriptor.height >> (int)Instance.universeBackgroundModule.property._Render_ResolutionOptions,
                                    RenderTextureFormat.ARGBHalf), 
                                name: "SkyRT");
                        
                        //计算当前渲染的矩形
                        Rect renderTargetRect = new Rect(0,0,Instance.universeBackgroundModule.skyRT.rt.width, Instance.universeBackgroundModule.skyRT.rt.height);
                        float Width = renderTargetRect.width / 2;
                        Rect currentRect = new Rect(_SplitFrameCount * Width, 0, Width, renderTargetRect.height);
                        cmd.EnableScissorRect(currentRect);
                        
                        //渲染天空盒内容到分帧缓存RT
                        //渲染背景
                        _ActiveRT = Instance.universeBackgroundModule.RenderBackground(cmd, ref renderingData, Instance.universeBackgroundModule.skyRT);
                        //渲染大气
                        Instance.atmosphereModule?.RenderAtmosphere(cmd, ref renderingData, _ActiveRT, currentRect);
                        //体积云
                        Instance.volumeCloudOptimizeModule?.RenderVolumeCloud(cmd, ref renderingData, _ActiveRT);
                        
                        _ActiveRT = Instance.universeBackgroundModule.RenderUpScaleAndTaa_1(cmd,ref renderingData, _ActiveRT,_ViewMatrix, projectionMatrix, _SplitFrameCount);
                        //我们这里将星星和星体放在后面渲染,使用特别的方式正确混合,这是因为,大气体积云我们可以降低分辨率渲染,而星星星体为保持清晰度不可降低分辨率
                        //渲染星星
                        Instance.starModule?.RenderStar(cmd, ref renderingData);
                        //渲染星体
                        Instance.celestialBodyManager?.RenderCelestialBodyList(cmd, ref renderingData);
                        
                        //取消分帧矩形
                        cmd.DisableScissorRect();
                        // EditorApplication.isPaused = true;
                        //累加计数器
#if UNITY_EDITOR
                        if (!EditorApplication.isPaused)
                        {
                            _SplitFrameCount++;
                        }
#else
                            _SplitFrameCount++;
#endif
                        
                        //当分裂计数器等于最大计数时,说明已经完成分帧渲染, 注意: 这个分帧渲染的结果是1帧之前的结果,必须将其修正到当前帧
                        if (_SplitFrameCount == 2)
                        {
                            _ActiveRT = Instance.universeBackgroundModule?.RenderFixupLate(cmd, _ActiveRT);
                            RenderingUtils.ReAllocateIfNeeded(ref Instance.universeBackgroundModule.splitFrameRT, _ActiveRT.rt.descriptor, name: "SplitFrameRT");
                            cmd.CopyTexture(_ActiveRT, Instance.universeBackgroundModule.splitFrameRT);
                            _ActiveRT = Instance.universeBackgroundModule.splitFrameRT;
                            
                            _SplitFrameCount = 0;
                            _IsSplitFrameRender = false;
                        }
                        else
                        {
                            //修正由于异步渲染造成的不跟手
                            if (Instance.universeBackgroundModule.splitFrameRT != null)
                                _ActiveRT = Instance.universeBackgroundModule?.RenderFixupLate(cmd, Instance.universeBackgroundModule.splitFrameRT);
                        }
                    }
                    else
                    {
                        //修正由于异步渲染造成的不跟手
                        if (Instance.universeBackgroundModule.splitFrameRT != null)
                            _ActiveRT = Instance.universeBackgroundModule?.RenderFixupLate(cmd, Instance.universeBackgroundModule.splitFrameRT);
                    }
                    
                    profilingSampler.End(cmd);
                    
                    
                    //恢复矩阵
                    projectionMatrix = GL.GetGPUProjectionMatrix(dataCamera.projectionMatrix, true); 
                    RenderingUtils.SetViewAndProjectionMatrices(cmd,  dataCamera.worldToCameraMatrix, projectionMatrix, true); 
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    
                    //修正视野范围
                    if(_ActiveRT != null) 
                        Instance.universeBackgroundModule.RenderFixupLateBlit(cmd,ref renderingData ,_ActiveRT, renderingData.cameraData.renderer.cameraColorTargetHandle);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    
                }
                else
                {
                    cmd.DisableScissorRect();

                    RenderingUtils.ReAllocateIfNeeded(ref Instance.universeBackgroundModule.skyRT, 
                            new RenderTextureDescriptor(
                                renderingData.cameraData.cameraTargetDescriptor.width >> (int)Instance.universeBackgroundModule.property._Render_ResolutionOptions,
                                renderingData.cameraData.cameraTargetDescriptor.height >> (int)Instance.universeBackgroundModule.property._Render_ResolutionOptions,
                                RenderTextureFormat.ARGBHalf), 
                            name: "SkyRT");
                    //渲染背景
                    _ActiveRT = Instance.universeBackgroundModule.RenderBackground(cmd, ref renderingData, Instance.universeBackgroundModule.skyRT);
                    //渲染大气
                    Instance.atmosphereModule?.RenderAtmosphere(cmd, ref renderingData, _ActiveRT);
                    //体积云
                    Instance.volumeCloudOptimizeModule?.RenderVolumeCloud(cmd, ref renderingData, _ActiveRT);

                    _ActiveRT = Instance.universeBackgroundModule.RenderUpScaleAndTaa_1(cmd,ref renderingData, _ActiveRT, dataCamera.worldToCameraMatrix, dataCamera.projectionMatrix, _SplitFrameCount);
                        
                    //我们这里将星星和星体放在后面渲染,使用特别的方式正确混合,这是因为,大气体积云我们可以降低分辨率渲染,而星星星体为保持清晰度不可降低分辨率
                    //渲染星星
                    Instance.starModule?.RenderStar(cmd, ref renderingData);
                    //渲染星体
                    Instance.celestialBodyManager?.RenderCelestialBodyList(cmd, ref renderingData);
                    
                    Blitter.BlitCameraTexture(cmd, _ActiveRT, renderingData.cameraData.renderer.cameraColorTargetHandle);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
                
                CommandBufferPool.Release(cmd);
            }
            private RTHandle _ActiveRT;
        }
        
        private class AtmosphereBlendPass : ScriptableRenderPass
        {
            public AtmosphereBlendPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Test: AtmosphereBlend");
                
                Instance.atmosphereModule?.RenderAtmosphereBlend(cmd, ref renderingData);
                
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