using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSystem.Runtime
{
    public partial class OcclusionRenderer
    {
        
#if UNITY_EDITOR
        protected override void DrawGizmosSelected()
        {
            if (occlusionCamera != null)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem//Textures/Icon/occProbes-icon.png");

                Color Cache = Gizmos.color;
                Gizmos.color = new Color(0, 0, 0, 0.2f);
                Gizmos.DrawCube(transform.position + transform.forward * (occlusionCamera.farClipPlane + occlusionCamera.nearClipPlane) / 2,
                new Vector3(occlusionCamera.orthographicSize * 2, occlusionCamera.farClipPlane - occlusionCamera.nearClipPlane, occlusionCamera.orthographicSize * 2));
                Gizmos.color = new Color(1, 1, 1, 0.3f);
                Gizmos.DrawWireCube(transform.position + transform.forward * (occlusionCamera.farClipPlane + occlusionCamera.nearClipPlane) / 2,
                    new Vector3(occlusionCamera.orthographicSize * 2, occlusionCamera.farClipPlane - occlusionCamera.nearClipPlane, occlusionCamera.orthographicSize * 2));

                Vector3[] Positions01 = new[]
                {
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    (transform.right * 0.5f + transform.up * 0.5f),
                    transform.position + transform.forward * occlusionCamera.nearClipPlane + 
                    transform.right * occlusionCamera.orthographicSize + transform.up * occlusionCamera.orthographicSize,
                    
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    (transform.right * 0.5f + transform.up * 0.5f),
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    (transform.right * 0.5f + transform.up * 0.5f) + transform.forward * 2,
                    
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    (transform.right * 0.5f + transform.up * 0.5f) + transform.forward * 2,
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    (transform.right * 0.5f + transform.up * 0.5f) + transform.forward * 2 +
                    (transform.right + transform.up),
                    
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    (transform.right * 0.5f + transform.up * 0.5f) + transform.forward * 2 +
                    (transform.right + transform.up),
                    transform.position + transform.forward * occlusionCamera.nearClipPlane +
                    transform.forward * 5,
                };
                Gizmos.DrawLineList(Positions01);
                
                Vector3[] Positions02 = new Vector3[Positions01.Length];
                for (int i = 0; i < Positions01.Length; i++)
                {
                    Positions02[i] = Quaternion.AngleAxis(90, transform.forward) * (Positions01[i] - transform.position) + transform.position;
                }
                Gizmos.DrawLineList(Positions02);
                
                Vector3[] Positions03 = new Vector3[Positions02.Length];
                for (int i = 0; i < Positions01.Length; i++)
                {
                    Positions03[i] = Quaternion.AngleAxis(90, transform.forward) * (Positions02[i] - transform.position) + transform.position;
                }
                Gizmos.DrawLineList(Positions03);
                
                Vector3[] Positions04 = new Vector3[Positions03.Length];
                for (int i = 0; i < Positions01.Length; i++)
                {
                    Positions04[i] = Quaternion.AngleAxis(90, transform.forward) * (Positions03[i] - transform.position) + transform.position;
                }
                Gizmos.DrawLineList(Positions04);
                
                Gizmos.color = Cache;
            }
        }
#endif
        
    }
    
    [ExecuteAlways]
    public partial class OcclusionRenderer : BaseModule
    {
        #region 字段
        [LabelText("范围半径")][ReadOnly]
        public float effectRadius;
        
        [LabelText("位置偏移")]
        public Vector3 occlusionPositionOffset = new Vector3(0, 0, 0);
        
        [LabelText("深度渲染器数据")][ReadOnly]
        public ScriptableRendererData occlusionRendererData;

        [LabelText("渲染纹理分辨率")] [MinValue(0)]
        public int renderTextureResolution = 128;

        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public Camera occlusionCamera;

        private RenderTexture _occlusionRT;
        
        #endregion

        
        #region 事件函数
        private void OnEnable()
        {
            //调整游戏对象
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            gameObject.name = "OcclusionCamera";
            
            if (gameObject.GetComponent<Camera>() == null)
            {
                occlusionCamera = gameObject.AddComponent<Camera>();
                occlusionCamera.orthographic = true;
                occlusionCamera.forceIntoRenderTexture = true;
                occlusionCamera.GetUniversalAdditionalCameraData().renderShadows = false;
            }
            else
            {
                occlusionCamera = gameObject.GetComponent<Camera>();
            }
            
#if UNITY_EDITOR
            if (occlusionRendererData == null)
                occlusionRendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(
                    "Packages/com.worldsystem//Renderers/PrecipitationOccluderRendererData.asset");
#endif
            //安装遮蔽渲染器
            SetupOcclusionRenderer();
            //创建摄像机纹理
            CreateRenderTexture();
        }
        public void OnDestroy()
        {
            CleanupRenderTexture();
            RemoveOcclusionRenderer();

            if (occlusionRendererData != null)
                Resources.UnloadAsset(occlusionRendererData);
            occlusionRendererData = null;

            if (gameObject.GetComponent<Camera>() != null && gameObject.activeSelf && Time.frameCount != 0)
            {
                CoreUtils.Destroy(gameObject.GetComponent<Camera>().GetUniversalAdditionalCameraData());
                CoreUtils.Destroy(gameObject.GetComponent<Camera>());
            }
            occlusionCamera = null;
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            //调整遮蔽摄像机数据
            transform.position = transform.parent.position + occlusionPositionOffset + new Vector3(0,effectRadius * 0.9f,0);
            if (occlusionCamera != null)
            {
                occlusionCamera.nearClipPlane = 0;
                occlusionCamera.farClipPlane = effectRadius * 0.9f * 2 + occlusionPositionOffset.y;
                occlusionCamera.orthographicSize = effectRadius * 0.9f;
            }
                
        }
#endif
        #endregion

        
        #region 重要函数
        
        private void SetupOcclusionRenderer()
        {
            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            ScriptableRendererData[] rendererDataList = GetRendererDataList(asset);

            int dataIndex = FindRendererDataIndex(rendererDataList, occlusionRendererData);

            //如果管线没有 遮蔽渲染器 则使用反射添加
            if (dataIndex == -1)
            {
                dataIndex = AddRendererDataToList(asset, rendererDataList, occlusionRendererData);
            }

            //如果管线有 遮蔽渲染器 则设置摄像机的渲染器为 遮蔽渲染器
            if (dataIndex != -1)
            {
                occlusionCamera.GetUniversalAdditionalCameraData().SetRenderer(dataIndex);
            }
        }

        /// <summary>
        /// 移除遮蔽渲染器
        /// </summary>
        private void RemoveOcclusionRenderer()
        {
            if (occlusionRendererData == null)
                return;

            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            ScriptableRendererData[] rendererDataList = GetRendererDataList(asset);

            int dataIndex = FindRendererDataIndex(rendererDataList, occlusionRendererData);

            if (dataIndex != -1)
            {
                // 从列表中删除添加的渲染器数据
                List<ScriptableRendererData> datas = new List<ScriptableRendererData>(rendererDataList);
                datas.RemoveAt(dataIndex);

                // 将修改后的列表设置回资源
                typeof(UniversalRenderPipelineAsset)
                    .GetField("m_RendererDataList",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(asset, datas.ToArray());
            }

            occlusionRendererData = null;
            occlusionCamera.GetUniversalAdditionalCameraData().SetRenderer(0);
        }

        /// <summary>
        /// 使用反射获取 非公开字段 m_RendererDataList
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private ScriptableRendererData[] GetRendererDataList(UniversalRenderPipelineAsset asset)
        {
            return (ScriptableRendererData[])
                typeof(UniversalRenderPipelineAsset)
                    .GetField("m_RendererDataList",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(asset);
        }

        /// <summary>
        /// 查找渲染器位于列表的索引
        /// </summary>
        /// <param name="rendererDataList"></param>
        /// <param name="targetData"></param>
        /// <returns></returns>
        private int FindRendererDataIndex(ScriptableRendererData[] rendererDataList, ScriptableRendererData targetData)
        {
            for (int i = 0; i < rendererDataList.Length; i++)
            {
                if (rendererDataList[i] == targetData)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 使用反射将渲染器 添加到 管线渲染器列表的最后
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="rendererDataList"></param>
        /// <param name="newData"></param>
        /// <returns></returns>
        private int AddRendererDataToList(
            UniversalRenderPipelineAsset asset,
            ScriptableRendererData[] rendererDataList,
            ScriptableRendererData newData
        )
        {
            List<ScriptableRendererData> datas = new List<ScriptableRendererData>(rendererDataList);
            datas.Add(newData);

            typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(asset, datas.ToArray());

            return datas.Count - 1; // Index of the added data
        }

        /// <summary>
        /// 创建渲染纹理
        /// </summary>
        private void CreateRenderTexture()
        {
            CleanupRenderTexture();
            _occlusionRT = new RenderTexture(renderTextureResolution, renderTextureResolution, 0,
                RenderTextureFormat.RHalf)
            {
                name = "PrecipitationCollisionTexture"
            };
            _occlusionRT.Create();
            occlusionCamera.targetTexture = _occlusionRT;
        }

        /// <summary>
        /// 清理摄像机渲染纹理
        /// </summary>
        private void CleanupRenderTexture()
        {
            if (_occlusionRT != null)
            {
                occlusionCamera.targetTexture.Release();
                occlusionCamera.targetTexture = null;
                _occlusionRT.Release();
                _occlusionRT = null;
            }
        }
        
        #endregion
        
    }
}