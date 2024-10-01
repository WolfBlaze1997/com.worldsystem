using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
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
            Full, Half, Quarter
        }
        
        public enum AsyncUpdateRate
        {
            Fps60 = 60,Fps50 = 50,Fps40 = 40,Fps30 = 30,Fps20 = 20,Fps10 = 10
        }
        
        public enum TargetFps
        {
            TargetFpsUnLimit = -1,TargetFps240 = 240,TargetFps180 = 180,TargetFps144 = 144,TargetFps120 = 120,TargetFps90 = 90,TargetFps60 = 60,TargetFps30 = 30
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
            
            [FoldoutGroup("配置")] [LabelText("TAA着色器")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader taaShader;
            
            [FoldoutGroup("配置")] [LabelText("TAA材质")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material taaMaterial;
            
            [FoldoutGroup("配置")] [LabelText("修正延迟着色器")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader volumeCloudFixupLateShader;
            
            [FoldoutGroup("配置")] [LabelText("修正延迟材质")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material volumeCloudFixupLateMaterial;
            
            [FoldoutGroup("配置")] [LabelText("修正延迟块移着色器")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader volumeCloudFixupLateBlitShader;
            
            [FoldoutGroup("配置")] [LabelText("修正延迟块移材质")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material volumeCloudFixupLateBlitMaterial;
            
            [LabelText("分辨率选项")] [GUIColor(0, 1, 0)] [PropertyOrder(-10)]
            public Scale renderResolutionOptions = Scale.Full;
            
            [LabelText("    TAA降噪减弱")] [PropertyRange(0, 1)] [GUIColor(0f, 0.7f, 0f)] [PropertyOrder(-10)]
            [ShowIf("@renderResolutionOptions != Scale.Full")]
            public float renderTemporalAAFactor;
            
            [LabelText("目标帧率")] [GUIColor(0.7f, 0.7f, 1f)]
            public TargetFps renderTargetFps = TargetFps.TargetFps120;
            
            [LabelText("使用异步分帧渲染")] [GUIColor(0.7f, 0.7f, 1f)]
            public bool renderUseAsyncRender = true;
            
            [LabelText("    异步更新率(fps/s)")] [GUIColor(0.7f, 0.7f, 1f)] [ShowIf("renderUseAsyncRender")]
            public AsyncUpdateRate renderAsyncUpdateRate = AsyncUpdateRate.Fps20;
            
            [LabelText("    扩大的视野")] [GUIColor(0.7f, 0.7f, 1f)] [ShowIf("renderUseAsyncRender")]
            public float renderAsyncFOV = 80;
        }
        
        [HideLabel]
        public Property property = new();
        
        #endregion

        

        #region 安装参数

        private void SetupstaticProperty()
        {
            Shader.SetGlobalFloat(_TAA_BLEND_FACTOR, property.renderTemporalAAFactor);
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
#endif
            if (property.backgroundMaterial == null) property.backgroundMaterial = CoreUtils.CreateEngineMaterial(property.backgroundShader);
            
            OnValidate();
        }
        
        private void OnDisable()
        {
            if (property.skyMesh != null)
                Resources.UnloadAsset(property.skyMesh);
            if (property.backgroundShader != null)
                Resources.UnloadAsset(property.backgroundShader);
            if (property.backgroundMaterial != null)
                CoreUtils.Destroy(property.backgroundMaterial);
            
            if(property.volumeCloudFixupLateShader != null)
                Resources.UnloadAsset(property.volumeCloudFixupLateShader);
            if (property.volumeCloudFixupLateMaterial != null)
                CoreUtils.Destroy(property.volumeCloudFixupLateMaterial);
            if(property.volumeCloudFixupLateBlitShader != null)
                Resources.UnloadAsset(property.volumeCloudFixupLateBlitShader);
            if (property.volumeCloudFixupLateBlitMaterial != null)
                CoreUtils.Destroy(property.volumeCloudFixupLateBlitMaterial);
            
            property.volumeCloudFixupLateShader = null;
            property.volumeCloudFixupLateMaterial = null;
            property.volumeCloudFixupLateBlitShader = null;
            property.volumeCloudFixupLateBlitMaterial = null;
            PreviousRT?.Release();
            PreviousRT = null;
            SkyRT?.Release();
            SkyRT = null;
            property.skyMesh = null;
            property.backgroundShader = null;
            property.backgroundMaterial = null;
            TaaRT1?.Release();
            TaaRT1 = null;
        }

        public void OnValidate()
        {
            SetupstaticProperty();

            if (property.renderResolutionOptions != Scale.Full)
            {
#if UNITY_EDITOR
                if (property.taaShader == null)
                    property.taaShader =
                        AssetDatabase.LoadAssetAtPath<Shader>(
                            "Packages/com.worldsystem/Shader/ShaderLibrary/TemporalAA.shader");
#endif
                if (property.taaMaterial == null)
                    property.taaMaterial = CoreUtils.CreateEngineMaterial(property.taaShader);
            }
            else
            {
                if(property.taaShader != null)
                    Resources.UnloadAsset(property.taaShader);
                if(property.taaMaterial != null)
                    CoreUtils.Destroy(property.taaMaterial);
                property.taaShader = null;
                property.taaMaterial = null;
                PreviousRT?.Release();
                PreviousRT = null;
                TaaRT1?.Release();
                TaaRT1 = null;
            }

            if (property.renderUseAsyncRender)
            {
#if UNITY_EDITOR
                if (property.volumeCloudFixupLateShader == null)
                    property.volumeCloudFixupLateShader =
                        AssetDatabase.LoadAssetAtPath<Shader>(
                            "Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/FixupLate.shader");
                if (property.volumeCloudFixupLateBlitShader == null)
                    property.volumeCloudFixupLateBlitShader =
                        AssetDatabase.LoadAssetAtPath<Shader>(
                            "Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/FixupLateBlit.shader");
#endif
                if (property.volumeCloudFixupLateMaterial == null)
                    property.volumeCloudFixupLateMaterial =
                        CoreUtils.CreateEngineMaterial(property.volumeCloudFixupLateShader);
                if (property.volumeCloudFixupLateBlitMaterial == null)
                    property.volumeCloudFixupLateBlitMaterial =
                        CoreUtils.CreateEngineMaterial(property.volumeCloudFixupLateBlitShader);
            }
            else
            {
                SplitFrameRT?.Release();
                SplitFrameRT = null;
                if (property.volumeCloudFixupLateShader != null)
                    Resources.UnloadAsset(property.volumeCloudFixupLateShader);
                if (property.volumeCloudFixupLateMaterial != null)
                    CoreUtils.Destroy(property.volumeCloudFixupLateMaterial);
                if (property.volumeCloudFixupLateBlitShader != null)
                    Resources.UnloadAsset(property.volumeCloudFixupLateBlitShader);
                if (property.volumeCloudFixupLateBlitMaterial != null)
                    CoreUtils.Destroy(property.volumeCloudFixupLateBlitMaterial);
                property.volumeCloudFixupLateShader = null;
                property.volumeCloudFixupLateMaterial = null;
                property.volumeCloudFixupLateBlitShader = null;
                property.volumeCloudFixupLateBlitMaterial = null;
            }
            
            Application.targetFrameRate = (int)property.renderTargetFps;
            
        }

#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        #endregion

        

        #region 渲染函数
        
        public RTHandle SkyRT;
        public RTHandle SplitFrameRT;
        public RTHandle TaaRT1;
        public RTHandle PreviousRT;
        
        private Matrix4x4 _viewProjection;
        private Matrix4x4 _prevViewProjection;
        private Matrix4x4 _inverseViewProjection;
        private Matrix4x4 _viewProjectionPerFrame;
        private Matrix4x4 _prevViewProjectionPerFrame;
        private Matrix4x4 _inverseViewProjectionPerFrame;
        
        private static readonly int _PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");
        private static readonly int _ViewProjM = Shader.PropertyToID("_ViewProjM");
        private static readonly int _InverseViewProjM = Shader.PropertyToID("_InverseViewProjM");
        private static readonly int _PrevViewProjM_PerFrame = Shader.PropertyToID("_PrevViewProjM_PerFrame");
        private static readonly int _ViewProjM_PerFrame = Shader.PropertyToID("_ViewProjM_PerFrame");
        private static readonly int _InverseViewProjM_PerFrame = Shader.PropertyToID("_InverseViewProjM_PerFrame");
        private static readonly int _CURRENT_TAA_FRAME = Shader.PropertyToID("_CURRENT_TAA_FRAME");
        private static readonly int _PREVIOUS_TAA_CLOUD_RESULTS = Shader.PropertyToID("_PREVIOUS_TAA_CLOUD_RESULTS");
        private static readonly int _TAA_BLEND_FACTOR = Shader.PropertyToID("_TAA_BLEND_FACTOR");
        private static readonly int _FOVScale = Shader.PropertyToID("_FOVScale");
        private static readonly int _FixupLateTarget = Shader.PropertyToID("_FixupLateTarget");
        
        public RTHandle RenderBackground(CommandBuffer cmd, ref RenderingData renderingData, RTHandle dstRT)
        {
            cmd.SetRenderTarget(dstRT);
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(renderingData.cameraData.worldSpaceCameraPos, Quaternion.identity,
                Vector3.one * renderingData.cameraData.camera.farClipPlane);
            cmd.DrawMesh(property.skyMesh, m, property.backgroundMaterial, 0);
            
            return dstRT;
        }
        
        public void SetupTaaMatrices(CommandBuffer cmd,Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            //设置TAA需要的矩阵信息
            if(_viewProjection != Matrix4x4.identity)
                _prevViewProjection = _viewProjection;
            else
                _prevViewProjection = Matrix4x4.identity;
            _viewProjection = projectionMatrix * viewMatrix;
            
            _inverseViewProjection = _viewProjection.inverse;
            
            cmd.SetGlobalMatrix(_PrevViewProjM, _prevViewProjection);
            cmd.SetGlobalMatrix(_ViewProjM, _viewProjection);
            cmd.SetGlobalMatrix(_InverseViewProjM, _inverseViewProjection);
        }
        
        public void SetupTaaMatrices_PerFrame(CommandBuffer cmd,Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            //设置TAA需要的矩阵信息
            if(_viewProjectionPerFrame != Matrix4x4.identity)
                _prevViewProjectionPerFrame = _viewProjectionPerFrame;
            else
                _prevViewProjectionPerFrame = Matrix4x4.identity;

            _viewProjectionPerFrame = projectionMatrix * viewMatrix;
            
            _inverseViewProjectionPerFrame = _viewProjectionPerFrame.inverse;
            
            cmd.SetGlobalMatrix(_PrevViewProjM_PerFrame, _prevViewProjectionPerFrame);
            cmd.SetGlobalMatrix(_ViewProjM_PerFrame, _viewProjectionPerFrame);
            cmd.SetGlobalMatrix(_InverseViewProjM_PerFrame, _inverseViewProjectionPerFrame);
        }
        
        public RTHandle RenderUpScaleAndTaa(CommandBuffer cmd,ref RenderingData renderingData, RTHandle currentRT, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, int splitFrameCount)
        {
            if (property.renderResolutionOptions == Scale.Full) 
                return currentRT;

            var TaaDescriptor = new RenderTextureDescriptor(renderingData.cameraData.cameraTargetDescriptor.width,
                renderingData.cameraData.cameraTargetDescriptor.height, currentRT.rt.descriptor.colorFormat);
            
            if (property.renderUseAsyncRender)
            {
                float width = renderingData.cameraData.cameraTargetDescriptor.width / 2f;
                cmd.EnableScissorRect(new Rect(splitFrameCount * width, 0, width, renderingData.cameraData.cameraTargetDescriptor.height));
            }
            
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
            Blitter.BlitTexture(cmd, new Vector4(1,1,0,0), property.taaMaterial, 0);
            
            cmd.CopyTexture(TaaRT1, PreviousRT);
            
            return TaaRT1;
        }
        
        public RTHandle RenderFixupLate(CommandBuffer cmd, RTHandle activeRT)
        {
            RenderTexture TemporaryRT =  RenderTexture.GetTemporary(activeRT.rt.descriptor);
            cmd.CopyTexture(activeRT,TemporaryRT);
            cmd.SetGlobalTexture("_ActiveTarget",TemporaryRT);
            cmd.SetRenderTarget(activeRT);
            Blitter.BlitTexture(cmd,new Vector4(1,1,0,0),property.volumeCloudFixupLateMaterial,0);
            RenderTexture.ReleaseTemporary(TemporaryRT);
            return activeRT;
        }
        
        public void RenderFixupLateBlit(CommandBuffer cmd, ref RenderingData renderingData, RTHandle SrcRT, RTHandle DstRT)
        {
            var dataCamera = renderingData.cameraData.camera;
            var frustumHeight1 = 2.0f * dataCamera.farClipPlane * Mathf.Tan(property.renderAsyncFOV * 0.5f * Mathf.Deg2Rad);
            var frustumHeight2 = 2.0f * dataCamera.farClipPlane * Mathf.Tan(dataCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            cmd.SetGlobalFloat(_FOVScale, frustumHeight2 / frustumHeight1);
            cmd.SetGlobalTexture(_FixupLateTarget,SrcRT);
            Blitter.BlitCameraTexture(cmd, SrcRT, DstRT,property.volumeCloudFixupLateBlitMaterial,0);
        }
        

        #endregion
        
    }
}