using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSystem.Runtime
{
    public partial class LightingScatterModule
    {
        //此处编写枚举帮助函数等
        public enum FalloffDirection
        {
            RayDirection, CameraForward
        }
    }
    
    
    [ExecuteAlways]
    public partial class LightingScatterModule : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("遮蔽着色器")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Shader occlusionShader;
            
            [FoldoutGroup("配置")] [LabelText("遮蔽材质")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Material occlusionMaterial;
            
            [FoldoutGroup("配置")] [LabelText("散射着色器")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Shader scatterShader;
            
            [FoldoutGroup("配置")] [LabelText("散射材质")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Material scatterMaterial;
            
            [FoldoutGroup("配置")] [LabelText("合并着色器")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Shader mergeShader;
            
            [FoldoutGroup("配置")] [LabelText("合并材质")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Material mergeMaterial;
            
            [Tooltip("控制效果的质量。更高的值在计算上更昂贵，但可以提供更平滑的结果。")] 
            [Range(4,64)][LabelText("采样数")][GUIColor(0.7f,0.7f,1f)]
            public int lightingScatterNumSamples = 16;
            
            [Tooltip("设置光衰减是基于单个像素的光线方向还是基于相机的前进方向（每个像素都是相同的）。")] 
            [LabelText("基础衰减模式")][GUIColor(0.7f,0.7f,1f)]
            public FalloffDirection lightingScatterFalloffDirective = FalloffDirection.RayDirection;
            
            [Tooltip("设置雾密度。较高的值会导致更强烈的光散射结果。")] [HorizontalGroup("fogDensity", 0.9f, DisableAutomaticLabelWidth = true)] 
            [LabelText("密度")][GUIColor(1f,0.7f,0.7f)]
            public AnimationCurve lightingScatterDensity = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
            
            [HorizontalGroup("fogDensity")][HideLabel][ReadOnly]
            public float lightingScatterDensityExecute;
            
            [Tooltip("控制衰减效果的强度。较高的值会导致光散射效果更快地减弱。")] [LabelText("衰减强度[1,10]")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("falloffIntensity", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve lightingScatterFalloffIntensity = new AnimationCurve(new Keyframe(0,3), new Keyframe(1,3));
            
            [HorizontalGroup("falloffIntensity")][HideLabel][ReadOnly]
            public float lightingScatterFalloffIntensityExecute;

            [Range(0.0f, 1.0f)][LabelText("散射饱和度")][GUIColor(0.7f,0.7f,1f)]
            public float lightingScatterSaturation = 0.5f;
            
            [Tooltip("确定对遮挡纹理进行采样的距离（屏幕空间距离）")][Range(0.01f, 1.0f)][LabelText("最大射线距离")][GUIColor(0.7f,0.7f,1f)]
            public float lightingScatterMaxRayDistance = 0.5f;
            
            [Tooltip("LSPP假设随着光线越来越远，遮挡器阻挡的光线越来越少。这个选项改变了这个假设。1=远处的遮挡物阻挡不了光线。0=远处的遮挡物阻挡了全光。")]
            [Range(0,1)] [LabelText("遮蔽距离量")][GUIColor(0.7f,0.7f,1f)]
            public float lightingScatterOccOverDistanceAmount;
            
            [Tooltip("启用后，我们假设屏幕边界外的区域未被遮挡。")]
            [LabelText("软化屏幕边缘")][GUIColor(0.7f,0.7f,1f)]
            public bool lightingScatterUseSoftEdge = true;
            
            [Tooltip("禁用时，每个像素都有一个固定的随机偏移，用噪声代替条带。启用后，每个像素都有一个随机的每帧偏移，用可变噪声替换静态噪声。")]
            [LabelText("动画采样偏移")][GUIColor(0.7f,0.7f,1f)]
            public bool lightingScatterUseDynamicNoise;
            
            public void LimitProperty()
            {
            }
            
            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) return;
                if (!UseLerp)
                {
                    //未插值时
                    lightingScatterDensityExecute = lightingScatterDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    lightingScatterFalloffIntensityExecute = lightingScatterFalloffIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                }

            }
        }
        
        [HideLabel] public Property property = new Property();

        public static bool UseLerp = false;

        [HideInInspector] public bool update;
        
        #endregion


        #region 安装参数
        
        private static readonly int _LightingScatter_UseDynamicNoise = Shader.PropertyToID("_LightingScatter_UseDynamicNoise");
        private static readonly int _LightingScatter_UseSoftEdge = Shader.PropertyToID("_LightingScatter_UseSoftEdge");
        private static readonly int _LightingScatter_FalloffDirective = Shader.PropertyToID("_LightingScatter_FalloffDirective");
        private static readonly int _LightingScatter_MaxRayDistance = Shader.PropertyToID("_LightingScatter_MaxRayDistance");
        private static readonly int _LightingScatter_OccOverDistanceAmount = Shader.PropertyToID("_LightingScatter_OccOverDistanceAmount");
        private static readonly int _LightingScatter_NumSamples = Shader.PropertyToID("_LightingScatter_NumSamples");
        private static readonly int _LightingScatter_Saturation = Shader.PropertyToID("_LightingScatter_Saturation");
        private static readonly int _LightingScatter_Density = Shader.PropertyToID("_LightingScatter_Density");
        private static readonly int _LightingScatter_FalloffIntensity = Shader.PropertyToID("_LightingScatter_FalloffIntensity");
        private static readonly int _LightingScatter_OcclusionTex = Shader.PropertyToID("_LightingScatter_OcclusionTex");
        private static readonly int _LightingScatter_ScatterTex = Shader.PropertyToID("_LightingScatter_ScatterTex");
        private static readonly int _LightingScatter_CameraTex = Shader.PropertyToID("_LightingScatter_CameraTex");
        
        private void SetupStaticProperty()
        {
            Shader.SetGlobalFloat(_LightingScatter_UseDynamicNoise, property.lightingScatterUseDynamicNoise ? 1f : 0f);
            Shader.SetGlobalFloat(_LightingScatter_UseSoftEdge, property.lightingScatterUseSoftEdge ? 1f : 0f);
            Shader.SetGlobalFloat(_LightingScatter_FalloffDirective, (int)property.lightingScatterFalloffDirective);
            Shader.SetGlobalFloat(_LightingScatter_MaxRayDistance, property.lightingScatterMaxRayDistance);
            Shader.SetGlobalFloat(_LightingScatter_OccOverDistanceAmount, property.lightingScatterOccOverDistanceAmount);
            Shader.SetGlobalFloat(_LightingScatter_NumSamples, property.lightingScatterNumSamples);
            Shader.SetGlobalFloat(_LightingScatter_Saturation, property.lightingScatterSaturation);
        }

        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_LightingScatter_Density, property.lightingScatterDensityExecute * 0.1f);
            Shader.SetGlobalFloat(_LightingScatter_FalloffIntensity, property.lightingScatterFalloffIntensityExecute);
        }
        
        #endregion

        

        #region 事件函数

        private void OnEnable()
        {
#if UNITY_EDITOR
            //载入数据
            if(property.occlusionShader == null)
                property.occlusionShader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/LightingScatter/Occluders.shader");
            if (property.scatterShader == null)
                property.scatterShader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/LightingScatter/LightScatter.shader");
            if (property.mergeShader == null)
                property.mergeShader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/LightingScatter/Merge.shader");
#endif
            if (property.occlusionMaterial == null)
                property.occlusionMaterial = CoreUtils.CreateEngineMaterial(property.occlusionShader);
            if (property.scatterMaterial == null)
                property.scatterMaterial = CoreUtils.CreateEngineMaterial(property.scatterShader);
            if (property.mergeMaterial == null)
                property.mergeMaterial = CoreUtils.CreateEngineMaterial(property.mergeShader);
            
            OnValidate();
        }


        private void OnDisable()
        {
            //销毁卸载数据
            if(property.occlusionShader != null)
                Resources.UnloadAsset(property.occlusionShader);
            if(property.scatterShader != null)
                Resources.UnloadAsset(property.scatterShader);
            if(property.mergeShader != null)
                Resources.UnloadAsset(property.mergeShader);
            
            if(property.occlusionMaterial != null)
                CoreUtils.Destroy(property.occlusionMaterial);
            if(property.scatterMaterial != null)
                CoreUtils.Destroy(property.scatterMaterial);
            if(property.mergeMaterial != null)
                CoreUtils.Destroy(property.mergeMaterial);
            
            _occlusionRT?.Release();
            _scatterRT?.Release();
            _occlusionRT = null;
            _scatterRT = null;

            property.occlusionShader = null;
            property.scatterShader = null;
            property.mergeShader = null;
            property.occlusionMaterial = null;
            property.scatterMaterial = null;
            property.mergeMaterial = null;
        }

        public void OnValidate()
        {
            property.LimitProperty();
            SetupStaticProperty();
        }


        void Update()
        {
            if (!update) return;

            property.ExecuteProperty();
            if (property.lightingScatterDensityExecute < 0.01) return;
            SetupDynamicProperty();
        }


#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif

        #endregion

        

        #region 渲染函数

        private RTHandle _occlusionRT;
        private RTHandle _scatterRT;
        public void RenderLightingScatter(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!isActiveAndEnabled || property.lightingScatterDensityExecute < 0.01) return;
            
            RTHandle CameraColorRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RenderTextureDescriptor CamRTDescriptor = CameraColorRT.rt.descriptor;
            CamRTDescriptor.msaaSamples = 1;

            RenderTextureDescriptor OcclusionRTDescriptor = CamRTDescriptor;
            OcclusionRTDescriptor.width >>= 1;
            OcclusionRTDescriptor.height >>= 1;
            OcclusionRTDescriptor.colorFormat = RenderTextureFormat.R16;
            RenderingUtils.ReAllocateIfNeeded(ref _occlusionRT, OcclusionRTDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "OcclusionRT");
            cmd.SetRenderTarget(_occlusionRT);
            Blitter.BlitTexture(cmd , new Vector4(1,1,0,0), property.occlusionMaterial, 0);
            
            RenderTextureDescriptor ScatterRTDescriptor = CamRTDescriptor;
            ScatterRTDescriptor.width >>= 1;
            ScatterRTDescriptor.height >>= 1;
            RenderingUtils.ReAllocateIfNeeded(ref _scatterRT, ScatterRTDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "ScatterRT");
            cmd.SetRenderTarget(_scatterRT);
            cmd.SetGlobalTexture(_LightingScatter_OcclusionTex, _occlusionRT);
            Blitter.BlitTexture(cmd , new Vector4(1,1,0,0), property.scatterMaterial, 0);

            cmd.SetRenderTarget(CameraColorRT);
            cmd.SetGlobalTexture(_LightingScatter_ScatterTex, _scatterRT);
            RTHandle Temporary = RTHandles.Alloc(RenderTexture.GetTemporary(CamRTDescriptor));
            cmd.CopyTexture(CameraColorRT, Temporary);
            cmd.SetGlobalTexture(_LightingScatter_CameraTex, Temporary);
            Blitter.BlitTexture(cmd , new Vector4(1,1,0,0), property.mergeMaterial, 0);
            RenderTexture.ReleaseTemporary(Temporary);
        }

        #endregion
        
        
    }
    
}