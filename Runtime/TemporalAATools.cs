// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
//
// namespace WorldSystem.Runtime
// {
//     internal static class TemporalAATools
//     {
//         public static Shader TaaShader = Shader.Find("Hidden/WorldSystem/TemporalAA");
//         public static Material TaaMaterial = CoreUtils.CreateEngineMaterial(TaaShader);
//         public static RTHandle PreviousRT;
//         public static RTHandle TaaRT;
//         
//         public static void Initialize()
//         {
//             if(TaaShader == null)
//                 TaaShader = Shader.Find("Hidden/WorldSystem/TemporalAA");
//             if(TaaMaterial == null)
//                 TaaMaterial = CoreUtils.CreateEngineMaterial(TaaShader);
//         }
//
//         public static void StartTAA(CommandBuffer cmd,RenderingData renderingData)
//         {
//             SetupMatrices(renderingData);
//             cmd.SetGlobalMatrix(_PrevViewProjM, projectionMatrices.prevViewProjection);
//             cmd.SetGlobalMatrix(_ViewProjM, projectionMatrices.viewProjection);
//             cmd.SetGlobalMatrix(_InverseViewProjM, projectionMatrices.inverseViewProjection);
//             cmd.SetGlobalInt(_IsFirstFrame, PreviousRT == null ? 1 : 0);
//             
//             // Debug.Log(projectionMatrices.prevViewProjection == projectionMatrices.viewProjection);
//         }
//         private static readonly int _PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");
//         private static readonly int _ViewProjM = Shader.PropertyToID("_ViewProjM");
//         private static readonly int _InverseViewProjM = Shader.PropertyToID("_InverseViewProjM");
//         private static readonly int _IsFirstFrame = Shader.PropertyToID("_IsFirstFrame");
//
//         private static void SetupMatrices(RenderingData renderingData)
//         {
//             if(projectionMatrices.viewProjection != Matrix4x4.identity)
//                 projectionMatrices.prevViewProjection = projectionMatrices.viewProjection;
//             else
//                 projectionMatrices.prevViewProjection = Matrix4x4.identity;
//             
//             projectionMatrices.projection = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.nonJitteredProjectionMatrix, true);
//             projectionMatrices.viewProjection = projectionMatrices.projection * renderingData.cameraData.camera.worldToCameraMatrix;
//
//             projectionMatrices.inverseViewProjection = projectionMatrices.viewProjection.inverse;
//             
//         }
//         
//         
//         public static RTHandle ExecuteTAA(CommandBuffer cmd, RTHandle currentRT, float taaBlendFactor)
//         {
//             if (PreviousRT != null)
//             {
//                 RenderingUtils.ReAllocateIfNeeded(ref TaaRT, currentRT.rt.descriptor, name: "TemporalAART");
//                 cmd.SetGlobalTexture(_CURRENT_TAA_FRAME, currentRT);
//                 cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_RESULTS, PreviousRT);
//                 cmd.SetGlobalFloat(_TAA_BLEND_FACTOR, taaBlendFactor);
//                 cmd.SetRenderTarget(TaaRT);
//                 Blitter.BlitTexture(cmd, new Vector4(1,1,0,0), TaaMaterial, 0);
//             }
//             
//             RenderingUtils.ReAllocateIfNeeded(ref PreviousRT,currentRT.rt.descriptor, name: "PreviousRT");
//             if (TaaRT == null)
//             {
//                 cmd.CopyTexture(currentRT, PreviousRT);
//             }
//             else
//             {
//                 cmd.CopyTexture(TaaRT, PreviousRT); 
//             }
//
//             return TaaRT ?? currentRT;
//         }
//         private static int _TAA_BLEND_FACTOR = Shader.PropertyToID("_TAA_BLEND_FACTOR");
//
//         
//         public static void Dispose()
//         {
//             if(TaaShader != null)
//                 Resources.UnloadAsset(TaaShader);
//             if(TaaMaterial != null)
//                 CoreUtils.Destroy(TaaMaterial);
//             PreviousRT?.Release();
//             TaaRT?.Release();
//
//             TaaShader = null;
//             TaaMaterial = null;
//             PreviousRT = null;
//             TaaRT = null;
//         }
//
//         public static ProjectionMatrices projectionMatrices;
//
//         public struct ProjectionMatrices
//         {
//             public Matrix4x4 viewProjection;
//             public Matrix4x4 prevViewProjection;
//             public Matrix4x4 projection;
//             public Matrix4x4 inverseViewProjection;
//         }
//
//
//         
//         private static int _CURRENT_TAA_FRAME = Shader.PropertyToID("_CURRENT_TAA_FRAME");
//         private static int _PREVIOUS_TAA_CLOUD_RESULTS = Shader.PropertyToID("_PREVIOUS_TAA_CLOUD_RESULTS");
//
//     }
// }
