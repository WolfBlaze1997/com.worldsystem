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
            [FoldoutGroup("模拟实时GI")][LabelText("光照贴图对比度")][Range(0,1)][GUIColor(0.7f,0.7f,1)]
            public float LightingMapContrast = 0.75f;
        
            
            [FoldoutGroup("模拟实时反射")][LabelText("反射立方体纹理")][GUIColor(1f,0.7f,0.7f)][PreviewField(100)]
            public Cubemap ReflectionCubeTexture;
            [FoldoutGroup("模拟实时反射")][LabelText("主反射探针")][GUIColor(0.7f,0.7f,1f)]
            public ReflectionProbe MainReflectionProbe;
            [FoldoutGroup("模拟实时反射")][LabelText("混合立方体纹理")][GUIColor(0.7f,0.7f,1f)]
            public RenderTexture BlendCubeTexture;
            [FoldoutGroup("模拟实时反射")][LabelText("反射探针天空颜色")][GUIColor(1f,0.7f,0.7f)]
            public Gradient ReflectionSkyColor = new Gradient();
            [FoldoutGroup("模拟实时反射")][LabelText("反射探针强度曲线")][GUIColor(1f,0.7f,1f)]
            public AnimationCurve ReflectionStrengthCurve = new(new Keyframe(0,0),new Keyframe(1,0));
            [FoldoutGroup("模拟实时反射")][LabelText("反射探针基准混合曲线")][GUIColor(1f,0.7f,1f)]
            public AnimationCurve MixCoeffCurve = new(new Keyframe(0,1f),new Keyframe(0.5f,0),new Keyframe(1,1f));
            [FoldoutGroup("模拟实时反射")][LabelText("光照贴图转为遮蔽-Min")][Range(-1,1)][GUIColor(0.7f,0.7f,1)]
            public float _ApproxRealtimeGI_AOMin = -0.02f;
            [FoldoutGroup("模拟实时反射")][LabelText("光照贴图转为遮蔽-Max")][Range(-1,1)][GUIColor(0.7f,0.7f,1)]
            public float _ApproxRealtimeGI_AOMax = 0.25f;

            
            [FoldoutGroup("屏幕空间反射")] [LabelText("混合曲线")][GUIColor(1f,0.7f,1)]
            public AnimationCurve _SSR_BlendCurve = new AnimationCurve( new Keyframe(0,0), new Keyframe(1,0));
            [FoldoutGroup("屏幕空间反射")] [LabelText("采样")][GUIColor(0.7f,0.7f,1)]
            public float _SSR_Samples = 16;
            [FoldoutGroup("屏幕空间反射")] [LabelText("光线长度")][GUIColor(0.7f,0.7f,1)]
            public float _SSR_RayLength = 6;
            [FoldoutGroup("屏幕空间反射")] [LabelText("厚度")][GUIColor(0.7f,0.7f,1)]
            public float _SSR_Thickness = 0.5f;
            [FoldoutGroup("屏幕空间反射")] [LabelText("抖动噪音")] 
            [ReadOnly] [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D _SSR_NoiseTex;
            [FoldoutGroup("屏幕空间反射")] [LabelText("抖动")][GUIColor(0.7f,0.7f,1)]
            public float _SSR_Jitter = 0.3f;
        }
        [HideLabel]
        public Property property = new Property();
        
        #endregion
        
        
        public static bool useLerp = false;
        [HideInInspector]
        public Color ReflectionSkyColorExecute;


        #region 安装参数
        
        private void SetupStaticProperty()
        {
            Shader.SetGlobalFloat(_ApproxRealtimeGI_LightingMapContrast, property.LightingMapContrast);
            
            Shader.SetGlobalFloat(_ApproxRealtimeGI_AOMin, property._ApproxRealtimeGI_AOMin);
            Shader.SetGlobalFloat(_ApproxRealtimeGI_AOMax, property._ApproxRealtimeGI_AOMax);
            Shader.SetGlobalTexture(_SSR_NoiseTex, property._SSR_NoiseTex);
            Shader.SetGlobalVector(_SSR_Settings, new Vector4(property._SSR_Samples, property._SSR_RayLength, property._SSR_Thickness, property._SSR_Jitter));
        }
        private static readonly int _ApproxRealtimeGI_LightingMapContrast = Shader.PropertyToID("_ApproxRealtimeGI_LightingMapContrast");
        private static readonly int _ApproxRealtimeGI_AOMin = Shader.PropertyToID("_ApproxRealtimeGI_AOMin");
        private static readonly int _ApproxRealtimeGI_AOMax = Shader.PropertyToID("_ApproxRealtimeGI_AOMax");
        private static readonly int _SSR_NoiseTex = Shader.PropertyToID("_SSR_NoiseTex");
        private static readonly int _SSR_Settings = Shader.PropertyToID("_SSR_Settings");
        private void SetupDynamicProperty()
        {
            if (WorldManager.Instance.timeModule is null) return;
            Shader.SetGlobalColor(_ApproxRealtimeGI_SkyColor, ReflectionSkyColorExecute);
            Shader.SetGlobalFloat(_ApproxRealtimeGI_ReflectionStrength, property.ReflectionStrengthCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            Shader.SetGlobalFloat(_ApproxRealtimeGI_MixCoeff, property.MixCoeffCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            Shader.SetGlobalFloat(_RealtimeGIStrength, property.realtimeGIStrengthCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
            Shader.SetGlobalFloat("_SSR_BlendCoeff", property._SSR_BlendCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01));
        }
        private static readonly int _ApproxRealtimeGI_SkyColor = Shader.PropertyToID("_ApproxRealtimeGI_SkyColor");
        private static readonly int _ApproxRealtimeGI_ReflectionStrength = Shader.PropertyToID("_ApproxRealtimeGI_ReflectionStrength");
        private static readonly int _ApproxRealtimeGI_MixCoeff = Shader.PropertyToID("_ApproxRealtimeGI_MixCoeff");
        private static readonly int _RealtimeGIStrength = Shader.PropertyToID("_RealtimeGIStrength");
        
        #endregion
        

        #region 事件函数
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (property._SSR_NoiseTex == null)
                property._SSR_NoiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Noise Textures/blueNoiseSSR64.png");
            if (property.BlendCubeTexture == null)
                property.BlendCubeTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>("Packages/com.worldsystem/Runtime/BlendCubeTexture/BlendCubeTexture.renderTexture");
#endif
           property.MainReflectionProbe = FindAnyObjectByType<ReflectionProbe>();
           property.MainReflectionProbe.mode = ReflectionProbeMode.Custom;
           
           OnValidate();
        }
        
        

        private void OnDisable()
        {
            if(property._SSR_NoiseTex != null)
                Resources.UnloadAsset(property._SSR_NoiseTex);
            if(property.BlendCubeTexture != null)
                Resources.UnloadAsset(property.BlendCubeTexture);
            
            property._SSR_NoiseTex = null;
            property.BlendCubeTexture = null;
        }

        public void OnValidate()
        {
            property.MainReflectionProbe.customBakedTexture = property.ReflectionCubeTexture;
            SetupStaticProperty();
        }

        // [HideInInspector]
        // public Cubemap NextWeatherCubeTexture;
        
        [HideInInspector]
        public bool _Update;
        void Update()
        {
            if (!_Update) return;
            
            if (!useLerp)
            {
                ReflectionSkyColorExecute =
                property.ReflectionSkyColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                property.MainReflectionProbe.customBakedTexture = property.ReflectionCubeTexture;
            }
            else
            {
                property.MainReflectionProbe.customBakedTexture = property.BlendCubeTexture;
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
