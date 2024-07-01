using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

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
            
            [FoldoutGroup("配置")] [LabelText("修正延迟着色器")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader VolumeCloud_FixupLate_Shader;

            [FoldoutGroup("配置")] [LabelText("修正延迟材质")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material VolumeCloud_FixupLate_Material;
            
            [FoldoutGroup("配置")] [LabelText("修正延迟块移着色器")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader VolumeCloud_FixupLateBlit_Shader;

            [FoldoutGroup("配置")] [LabelText("修正延迟块移材质")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material VolumeCloud_FixupLateBlit_Material;
            
            [LabelText("分辨率选项")] 
            [GUIColor(0, 1, 0)] [PropertyOrder(-10)]
            public Scale _Render_ResolutionOptions = Scale.Full;
            
            [LabelText("    TAA降噪减弱")] [PropertyRange(0, 1)]
            [GUIColor(0f, 0.7f, 0f)] [PropertyOrder(-10)]
            [ShowIf("@_Render_ResolutionOptions != Scale.Full")]
            public float _Render_TemporalAAFactor = 0f;
            
            [LabelText("使用异步渲染")]
            [GUIColor(0.7f, 0.7f, 1f)]
            public bool _Render_UseAsyncRender = true;
            
            [LabelText("    异步更新率(fps/s)")]
            [GUIColor(0.7f, 0.7f, 1f)]
            public AsyncUpdateRate _Render_AsyncUpdateRate = AsyncUpdateRate.Fps20;
            
            [LabelText("    扩大的视野")]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("_Render_UseAsyncRender")]
            public float _Render_AsyncFOV = 90;
            
        }
        [HideLabel]
        public Property property = new();
        
        public enum AsyncUpdateRate
        {
            Fps60 = 60,Fps50 = 50,Fps40 = 40,Fps30 = 30,Fps20 = 20,Fps10 = 10
        }
        
        
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
            if (property.VolumeCloud_FixupLate_Shader == null)
                property.VolumeCloud_FixupLate_Shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/FixupLate.shader");
            if (property.VolumeCloud_FixupLateBlit_Shader == null)
                property.VolumeCloud_FixupLateBlit_Shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/FixupLateBlit.shader");
#endif
            if (property.backgroundMaterial == null) property.backgroundMaterial = CoreUtils.CreateEngineMaterial(property.backgroundShader);
            if (property.TaaMaterial == null) property.TaaMaterial = CoreUtils.CreateEngineMaterial(property.TaaShader);
            if (property.VolumeCloud_FixupLate_Material == null)
                property.VolumeCloud_FixupLate_Material = CoreUtils.CreateEngineMaterial(property.VolumeCloud_FixupLate_Shader);
            if (property.VolumeCloud_FixupLateBlit_Material == null)
                property.VolumeCloud_FixupLateBlit_Material = CoreUtils.CreateEngineMaterial(property.VolumeCloud_FixupLateBlit_Shader);
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
            
            if(property.VolumeCloud_FixupLate_Shader != null)
                Resources.UnloadAsset(property.VolumeCloud_FixupLate_Shader);
            if (property.VolumeCloud_FixupLate_Material != null)
                CoreUtils.Destroy(property.VolumeCloud_FixupLate_Material);
            if(property.VolumeCloud_FixupLateBlit_Shader != null)
                Resources.UnloadAsset(property.VolumeCloud_FixupLateBlit_Shader);
            if (property.VolumeCloud_FixupLateBlit_Material != null)
                CoreUtils.Destroy(property.VolumeCloud_FixupLateBlit_Material);
            
            property.VolumeCloud_FixupLate_Shader = null;
            property.VolumeCloud_FixupLate_Material = null;
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
            TaaRT1 = null;
            
            _fixupLateRTCache?.Release();
            _fixupLateRTCache = null;
        }

        public void OnValidate()
        {
            SetupstaticProperty();
            if (property._Render_ResolutionOptions == Scale.Full)
            {
                PreviousRT?.Release();
                PreviousRT = null;
                TaaRT1?.Release();
                TaaRT1 = null;
            }

            if (property._Render_UseAsyncRender)
            {
                if (_fixupLateRTCache == null)
                    _fixupLateRTCache = RTHandles.Alloc(1, 1);
            }
            else
            {
                _fixupLateRTCache?.Release();
                _fixupLateRTCache = null;
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
        
        
        public void SetupTaaMatrices(CommandBuffer cmd,Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            //设置TAA需要的矩阵信息
            if(viewProjection != Matrix4x4.identity)
                prevViewProjection = viewProjection;
            else
                prevViewProjection = Matrix4x4.identity;
            viewProjection = projectionMatrix * viewMatrix;
            
            inverseViewProjection = viewProjection.inverse;
            
            cmd.SetGlobalMatrix(_PrevViewProjM, prevViewProjection);
            cmd.SetGlobalMatrix(_ViewProjM, viewProjection);
            cmd.SetGlobalMatrix(_InverseViewProjM, inverseViewProjection);
        }
        private readonly int _PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");
        private readonly int _ViewProjM = Shader.PropertyToID("_ViewProjM");
        private readonly int _InverseViewProjM = Shader.PropertyToID("_InverseViewProjM");
        private Matrix4x4 viewProjection;
        private Matrix4x4 prevViewProjection;
        private Matrix4x4 inverseViewProjection;
        
        public void SetupTaaMatrices_PerFrame(CommandBuffer cmd,Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            //设置TAA需要的矩阵信息
            if(viewProjection_PerFrame != Matrix4x4.identity)
                prevViewProjection_PerFrame = viewProjection_PerFrame;
            else
                prevViewProjection_PerFrame = Matrix4x4.identity;

            viewProjection_PerFrame = projectionMatrix * viewMatrix;
            
            inverseViewProjection_PerFrame = viewProjection_PerFrame.inverse;
            
            cmd.SetGlobalMatrix(_PrevViewProjM_PerFrame, prevViewProjection_PerFrame);
            cmd.SetGlobalMatrix(_ViewProjM_PerFrame, viewProjection_PerFrame);
            cmd.SetGlobalMatrix(_InverseViewProjM_PerFrame, inverseViewProjection_PerFrame);
        }
        private readonly int _PrevViewProjM_PerFrame = Shader.PropertyToID("_PrevViewProjM_PerFrame");
        private readonly int _ViewProjM_PerFrame = Shader.PropertyToID("_ViewProjM_PerFrame");
        private readonly int _InverseViewProjM_PerFrame = Shader.PropertyToID("_InverseViewProjM_PerFrame");
        private Matrix4x4 viewProjection_PerFrame;
        private Matrix4x4 prevViewProjection_PerFrame;
        private Matrix4x4 inverseViewProjection_PerFrame;
        
        public RTHandle RenderUpScaleAndTaa_1(CommandBuffer cmd,ref RenderingData renderingData, RTHandle currentRT, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            if (property._Render_ResolutionOptions == Scale.Full) 
                return currentRT;

            var TaaDescriptor = new RenderTextureDescriptor(renderingData.cameraData.cameraTargetDescriptor.width,
                renderingData.cameraData.cameraTargetDescriptor.height, currentRT.rt.descriptor.colorFormat);
            
            SetupTaaMatrices(cmd, viewMatrix, projectionMatrix);
            if (PreviousRT == null || 
                PreviousRT.rt.descriptor.height != TaaDescriptor.height || 
                PreviousRT.rt.descriptor.width != TaaDescriptor.width)
            {
                RenderingUtils.ReAllocateIfNeeded(ref PreviousRT, TaaDescriptor, name: "PreviousRT");
                cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, currentRT);
            }
            else
            {
                cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, PreviousRT);
            }
            cmd.SetGlobalTexture(_CURRENT_TAA_FRAME, currentRT);
            
            RenderingUtils.ReAllocateIfNeeded(ref TaaRT1, TaaDescriptor, name: "TaaRT1");
            cmd.SetRenderTarget(TaaRT1);
            Blitter.BlitTexture(cmd, new Vector4(1,1,0,0), property.TaaMaterial, 0);
            
            cmd.CopyTexture(TaaRT1, PreviousRT);
            
            return TaaRT1;
        }

        private static int _CURRENT_TAA_FRAME = Shader.PropertyToID("_CURRENT_TAA_FRAME");
        private static int _PREVIOUS_TAA_CLOUD_RESULTS = Shader.PropertyToID("_PREVIOUS_TAA_CLOUD_RESULTS");
        private static int _TAA_BLEND_FACTOR = Shader.PropertyToID("_TAA_BLEND_FACTOR");
        public RTHandle TaaRT1;
        public RTHandle PreviousRT;
        public RTHandle skyRT;

        
        public RTHandle RenderFixupLate(CommandBuffer cmd, RTHandle activeRT)
        {
            RenderingUtils.ReAllocateIfNeeded(ref _fixupLateRTCache, activeRT.rt.descriptor, name: "FixupLateRTCache", wrapMode: TextureWrapMode.MirrorOnce);
            cmd.CopyTexture(activeRT,_fixupLateRTCache);
            cmd.SetGlobalTexture("_ActiveTarget",_fixupLateRTCache);
            cmd.SetRenderTarget(activeRT);
            Blitter.BlitTexture(cmd,new Vector4(1,1,0,0),property.VolumeCloud_FixupLate_Material,0);
            return activeRT;
        }
        public RTHandle _fixupLateRTCache;
        
        public void RenderFixupLateBlit(CommandBuffer cmd, ref RenderingData renderingData, RTHandle SrcRT, RTHandle DstRT)
        {
            var dataCamera = renderingData.cameraData.camera;
            var frustumHeight1 = 2.0f * dataCamera.farClipPlane * Mathf.Tan(property._Render_AsyncFOV * 0.5f * Mathf.Deg2Rad);
            var frustumHeight2 = 2.0f * dataCamera.farClipPlane * Mathf.Tan(dataCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Shader.SetGlobalFloat(FOVScale, frustumHeight2 / frustumHeight1);
            cmd.SetGlobalTexture(FixupLateTarget,SrcRT);
            Blitter.BlitCameraTexture(cmd, SrcRT, DstRT,property.VolumeCloud_FixupLateBlit_Material,0);
        }
        private static readonly int FOVScale = Shader.PropertyToID("_FOVScale");
        private static readonly int FixupLateTarget = Shader.PropertyToID("_FixupLateTarget");

        #endregion
    }
}