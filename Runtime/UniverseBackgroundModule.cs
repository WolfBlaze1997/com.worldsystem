using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSystem.Runtime
{


    [ExecuteAlways]
    public class UniverseBackgroundModule : BaseModule
    {
        #region 字段
        
        public enum Scale
        {
            Full,
            Half,
            Quarter
        }
        
        
        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("网格")] [ReadOnly][ShowIf("@WorldManager.Instance?.universeBackgroundModule?.hideFlags == HideFlags.None")]
            public Mesh skyMesh;

            [FoldoutGroup("配置")] [LabelText("背景着色器")] [ReadOnly][ShowIf("@WorldManager.Instance?.universeBackgroundModule?.hideFlags == HideFlags.None")]
            public Shader backgroundShader;

            [FoldoutGroup("配置")] [LabelText("背景材质")] [ReadOnly][ShowIf("@WorldManager.Instance?.universeBackgroundModule?.hideFlags == HideFlags.None")]
            public Material backgroundMaterial;
            
            [FoldoutGroup("配置")] [LabelText("TAA着色器")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader TaaShader;

            [FoldoutGroup("配置")] [LabelText("TAA材质")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material TaaMaterial;
            
            [LabelText("分辨率选项")] 
            [GUIColor(0, 1, 0)] [PropertyOrder(-10)]
            public Scale _Render_ResolutionOptions = Scale.Full;
            
            [LabelText("    TAA降噪减弱")] [PropertyRange(0, 1)]
            [GUIColor(0f, 0.7f, 0f)] [PropertyOrder(-10)]
            [ShowIf("@_Render_ResolutionOptions != Scale.Full")]
            public float _Render_TemporalAAFactor = 0f;
            
        }
        [HideLabel]
        public Property property = new();
        
        
        
        #endregion


        #region 安装参数

        private void SetupstaticProperty()
        {
            Shader.SetGlobalFloat(_TAA_BLEND_FACTOR, property._Render_TemporalAAFactor);
        }
        

        #endregion
        

        #region 事件函数
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (property.skyMesh == null) property.skyMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.worldsystem/Meshes/SkySphere.mesh");
            if (property.backgroundShader == null)
                property.backgroundShader =
                    AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/Skybox/BackgroundShader.shader");
            if(property.TaaShader == null) 
                property.TaaShader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/ShaderLibrary/TemporalAA.shader");
#endif
            if (property.backgroundMaterial == null) property.backgroundMaterial = CoreUtils.CreateEngineMaterial(property.backgroundShader);
            if (property.TaaMaterial == null) property.TaaMaterial = CoreUtils.CreateEngineMaterial(property.TaaShader);
            OnValidate();
        }
        
        private void OnDisable()
        {
            if (property.skyMesh != null)
                Resources.UnloadAsset(property.skyMesh);
            if (property.backgroundShader != null)
                Resources.UnloadAsset(property.backgroundShader);
            if(property.TaaShader != null)
                Resources.UnloadAsset(property.TaaShader);
            
            if(property.TaaMaterial != null)
                CoreUtils.Destroy(property.TaaMaterial);
            if (property.backgroundMaterial != null)
                CoreUtils.Destroy(property.backgroundMaterial);
            PreviousRT?.Release();
            PreviousRT = null;
            skyRT?.Release();
            skyRT = null;
            property.skyMesh = null;
            property.backgroundShader = null;
            property.backgroundMaterial = null;
            property.TaaShader = null;
            property.TaaMaterial = null;
            TaaRT1?.Release();
            TaaRT2?.Release();
            TaaRT1 = null;
            TaaRT2 = null;
        }

        private void OnValidate()
        {
            SetupstaticProperty();
            if (property._Render_ResolutionOptions == Scale.Full)
            {
                PreviousRT?.Release();
                PreviousRT = null;
                TaaRT1?.Release();
                TaaRT2?.Release();
                TaaRT1 = null;
                TaaRT2 = null;
            }
            if (property._Render_ResolutionOptions == Scale.Half)
            {
                TaaRT2?.Release();
                TaaRT2 = null;
            }
                
        }

#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherSystemModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        #endregion


        #region 渲染函数
        public RTHandle RenderBackground(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderingUtils.ReAllocateIfNeeded(ref skyRT, 
                new RenderTextureDescriptor(
                    renderingData.cameraData.cameraTargetDescriptor.width >> (int)property._Render_ResolutionOptions,
                    renderingData.cameraData.cameraTargetDescriptor.height >> (int)property._Render_ResolutionOptions,
                    RenderTextureFormat.ARGBHalf), 
                name: "SkyRT");
            cmd.SetRenderTarget(skyRT);
            
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(renderingData.cameraData.worldSpaceCameraPos, Quaternion.identity,
                Vector3.one * renderingData.cameraData.camera.farClipPlane);
            cmd.DrawMesh(property.skyMesh, m, property.backgroundMaterial, 0);
            return skyRT;
        }
        
        private static readonly int _PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");
        private static readonly int _ViewProjM = Shader.PropertyToID("_ViewProjM");
        private static readonly int _InverseViewProjM = Shader.PropertyToID("_InverseViewProjM");
        private Matrix4x4 viewProjection;
        private Matrix4x4 prevViewProjection;
        private Matrix4x4 inverseViewProjection;
        public RTHandle RenderUpScaleAndTaa_1(CommandBuffer cmd, ref RenderingData  renderingData, RTHandle currentRT, RenderTextureDescriptor taaRTDescriptor)
        {
            if (property._Render_ResolutionOptions == Scale.Full) 
                return currentRT;
            
            
            if (PreviousRT == null)
            {
                if(property._Render_ResolutionOptions == Scale.Half)
                    RenderingUtils.ReAllocateIfNeeded(ref PreviousRT, taaRTDescriptor, name: "PreviousRT");
                cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, currentRT);
            }
            else
            {
                cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, PreviousRT);
            }
            cmd.SetGlobalTexture(_CURRENT_TAA_FRAME, currentRT);
            
            RenderingUtils.ReAllocateIfNeeded(ref TaaRT1, taaRTDescriptor, name: "TaaRT1");
            cmd.SetRenderTarget(TaaRT1);
            Blitter.BlitTexture(cmd, new Vector4(1,1,0,0), property.TaaMaterial, 0);
            
            if(property._Render_ResolutionOptions == Scale.Half)
                cmd.CopyTexture(TaaRT1, PreviousRT);
            
            return TaaRT1;
        }

        public void SetupTaaMatrices(RenderingData renderingData)
        {
            //设置TAA需要的矩阵信息
            if(viewProjection != Matrix4x4.identity)
                prevViewProjection = viewProjection;
            else
                prevViewProjection = Matrix4x4.identity;
            viewProjection = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.nonJitteredProjectionMatrix, true)
                             * renderingData.cameraData.camera.worldToCameraMatrix;
            inverseViewProjection = viewProjection.inverse;
            
            Shader.SetGlobalMatrix(_PrevViewProjM, prevViewProjection);
            Shader.SetGlobalMatrix(_ViewProjM, viewProjection);
            Shader.SetGlobalMatrix(_InverseViewProjM, inverseViewProjection);
        }

        public RTHandle RenderUpScaleAndTaa_2(CommandBuffer cmd, RTHandle currentRT, RenderTextureDescriptor taaRTDescriptor)
        {
            if (property._Render_ResolutionOptions != Scale.Quarter) 
                return currentRT;
            if (PreviousRT == null)
            {
                RenderingUtils.ReAllocateIfNeeded(ref PreviousRT, taaRTDescriptor, name: "PreviousRT");
                cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, currentRT);
            }
            // else
            // {
            //     cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, PreviousRT);
            // }
            cmd.SetGlobalTexture(_CURRENT_TAA_FRAME, currentRT);
            
            RenderingUtils.ReAllocateIfNeeded(ref TaaRT2, taaRTDescriptor, name: "TaaRT2");
            cmd.SetRenderTarget(TaaRT2);
            Blitter.BlitTexture(cmd, new Vector4(1,1,0,0), property.TaaMaterial, 0);
            
            cmd.CopyTexture(TaaRT2, PreviousRT);
            
            return TaaRT2;
        }
        private static int _CURRENT_TAA_FRAME = Shader.PropertyToID("_CURRENT_TAA_FRAME");
        private static int _PREVIOUS_TAA_CLOUD_RESULTS = Shader.PropertyToID("_PREVIOUS_TAA_CLOUD_RESULTS");
        private static int _TAA_BLEND_FACTOR = Shader.PropertyToID("_TAA_BLEND_FACTOR");
        public  RTHandle TaaRT1;
        public  RTHandle TaaRT2;
        public  RTHandle PreviousRT;
        public RTHandle skyRT;

        #endregion
    }
}