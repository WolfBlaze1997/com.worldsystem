using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class ApproxRealtimeGIModule : BaseModule
    {
        
        #region 字段
        
        [Serializable]
        public class Property
        {
            [FoldoutGroup("模拟实时GI")] [LabelText("模拟实时GI强度")] 
            [GUIColor(1f,0.7f,1f)]
            public AnimationCurve realtimeGIStrengthCurve = new(new Keyframe(0, 1.0f), new Keyframe(1,1.0f));
            
            [FoldoutGroup("模拟实时GI")] [LabelText("闪电模拟实时GI强度")] [ShowIf("@WorldManager.Instance.weatherEffectModule?.lightningEffect?.property?.lightningSpawnRate > 0 ?? false")]
            [GUIColor(1f,0.7f,1f)]
            public AnimationCurve lightningRealtimeGIStrengthCurve = new(new Keyframe(0, 1.0f), new Keyframe(1,1.0f));
            
            [FoldoutGroup("模拟实时GI")][LabelText("光照贴图对比度")][Range(0,1)][GUIColor(0.7f,0.7f,1)]
            public float lightingMapContrast = 0.75f;
            
            [FoldoutGroup("模拟实时反射")][LabelText("反射立方体纹理")][GUIColor(1f,0.7f,0.7f)][PreviewField(100)]
            public Cubemap reflectionCubeTexture;
            
            [FoldoutGroup("模拟实时反射")][LabelText("主反射探针")][GUIColor(0.7f,0.7f,1f)]
            public ReflectionProbe mainReflectionProbe;
            
            [FoldoutGroup("模拟实时反射")][LabelText("混合立方体纹理")][GUIColor(0.7f,0.7f,1f)]
            public RenderTexture blendCubeTexture;
            
            [FoldoutGroup("模拟实时反射")][LabelText("反射探针天空颜色")][GUIColor(1f,0.7f,0.7f)]
            public Gradient reflectionSkyColor = new Gradient();
            
            [FoldoutGroup("模拟实时反射")][LabelText("反射探针强度曲线")][GUIColor(1f,0.7f,1f)]
            public AnimationCurve reflectionStrengthCurve = new(new Keyframe(0,0),new Keyframe(1,0));
            
            [FoldoutGroup("模拟实时反射")][LabelText("反射探针基准混合曲线")][GUIColor(1f,0.7f,1f)]
            public AnimationCurve mixCoeffCurve = new(new Keyframe(0,1f),new Keyframe(0.5f,0),new Keyframe(1,1f));
            
            [FoldoutGroup("模拟实时反射")][LabelText("光照贴图转为遮蔽-Min")][Range(-1,1)][GUIColor(0.7f,0.7f,1)]
            public float lightingMapToAoMin = -0.02f;
            
            [FoldoutGroup("模拟实时反射")][LabelText("光照贴图转为遮蔽-Max")][Range(-1,1)][GUIColor(0.7f,0.7f,1)]
            public float lightingMapToAoMan = 0.25f;
            
            [FoldoutGroup("屏幕空间反射")] [LabelText("采样")][GUIColor(0.7f,0.7f,1)]
            public float ssrSamples = 16;
            
            [FoldoutGroup("屏幕空间反射")] [LabelText("光线长度")][GUIColor(0.7f,0.7f,1)]
            public float ssrRayLength = 6;
            
            [FoldoutGroup("屏幕空间反射")] [LabelText("厚度")][GUIColor(0.7f,0.7f,1)]
            public float ssrThickness = 0.5f;
            
            [FoldoutGroup("屏幕空间反射")] [LabelText("抖动噪音")] 
            [ReadOnly] [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D ssrNoiseTex;
            
            [FoldoutGroup("屏幕空间反射")] [LabelText("抖动")][GUIColor(0.7f,0.7f,1)]
            public float ssrJitter = 0.3f;
        }
        
        [HideLabel]
        public Property property = new Property();
        
        [HideInInspector]
        public Color reflectionSkyColorExecute;
        
        [HideInInspector]
        public bool update;
        
        public static bool UseLerp = false;
        
        private static readonly int _ApproxRealtimeGI_LightingMapContrast = Shader.PropertyToID("_ApproxRealtimeGI_LightingMapContrast");
        private static readonly int _ApproxRealtimeGI_AOMin = Shader.PropertyToID("_ApproxRealtimeGI_AOMin");
        private static readonly int _ApproxRealtimeGI_AOMax = Shader.PropertyToID("_ApproxRealtimeGI_AOMax");
        private static readonly int _SSR_NoiseTex = Shader.PropertyToID("_SSR_NoiseTex");
        private static readonly int _SSR_Settings = Shader.PropertyToID("_SSR_Settings");
        private static readonly int _ApproxRealtimeGI_SkyColor = Shader.PropertyToID("_ApproxRealtimeGI_SkyColor");
        private static readonly int _ApproxRealtimeGI_ReflectionStrength = Shader.PropertyToID("_ApproxRealtimeGI_ReflectionStrength");
        private static readonly int _ApproxRealtimeGI_MixCoeff = Shader.PropertyToID("_ApproxRealtimeGI_MixCoeff");
        private static readonly int _RealtimeGIStrength = Shader.PropertyToID("_RealtimeGIStrength");
        
        #endregion
        
        
        #region 安装参数
        
        private void SetupStaticProperty()
        {
            property.mainReflectionProbe.customBakedTexture = property.reflectionCubeTexture;
            Shader.SetGlobalFloat(_ApproxRealtimeGI_LightingMapContrast, property.lightingMapContrast);
            Shader.SetGlobalFloat(_ApproxRealtimeGI_AOMin, property.lightingMapToAoMin);
            Shader.SetGlobalFloat(_ApproxRealtimeGI_AOMax, property.lightingMapToAoMan);
            Shader.SetGlobalTexture(_SSR_NoiseTex, property.ssrNoiseTex);
            Shader.SetGlobalVector(_SSR_Settings, new Vector4(property.ssrSamples, property.ssrRayLength, property.ssrThickness, property.ssrJitter));
        }
        
        private void SetupDynamicProperty()
        {
            if (WorldManager.Instance.timeModule is null) return;
            Shader.SetGlobalColor(_ApproxRealtimeGI_SkyColor, reflectionSkyColorExecute);
            Shader.SetGlobalFloat(_ApproxRealtimeGI_ReflectionStrength, property.reflectionStrengthCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            Shader.SetGlobalFloat(_ApproxRealtimeGI_MixCoeff, property.mixCoeffCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            if (!VFXLightningEffect.IsBeInLightning)
            {
                Shader.SetGlobalFloat(_RealtimeGIStrength, property.realtimeGIStrengthCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            }
            else
            {
                Shader.SetGlobalFloat(_RealtimeGIStrength, property.lightningRealtimeGIStrengthCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            }
        }
        
        #endregion
        

        #region 事件函数
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (property.ssrNoiseTex == null)
                property.ssrNoiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Noise Textures/blueNoiseSSR64.png");
            if (property.blendCubeTexture == null)
                property.blendCubeTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>("Packages/com.worldsystem/Runtime/BlendCubeTexture/BlendCubeTexture.renderTexture");
#endif
           property.mainReflectionProbe = FindAnyObjectByType<ReflectionProbe>();
           property.mainReflectionProbe.mode = ReflectionProbeMode.Custom;
           
           OnValidate();
        }
        
        private void OnDisable()
        {
            if(property.ssrNoiseTex != null)
                Resources.UnloadAsset(property.ssrNoiseTex);
            if(property.blendCubeTexture != null)
                Resources.UnloadAsset(property.blendCubeTexture);
            
            property.ssrNoiseTex = null;
            property.blendCubeTexture = null;
        }

        public void OnValidate()
        {
            SetupStaticProperty();
        }
        
        void Update()
        {
            if (!update) return;
            
            if (!UseLerp)
            {
                reflectionSkyColorExecute =
                property.reflectionSkyColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                property.mainReflectionProbe.customBakedTexture = property.reflectionCubeTexture;
            }
            else
            {
                property.mainReflectionProbe.customBakedTexture = property.blendCubeTexture;
            }
            SetupDynamicProperty();
        }
        
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        #endregion
        

    }
}
