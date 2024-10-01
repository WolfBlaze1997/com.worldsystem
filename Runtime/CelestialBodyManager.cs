using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace WorldSystem.Runtime
{
    public partial class CelestialBodyManager
    {

        #region 枚举或帮助函数
        
        public CelestialBody GetShadowCastCelestialBody()
        {
            CelestialBody sunFind = property.celestialBodyList.Find(o=> o.property.type == CelestialBody.ObjectType.Sun);
            CelestialBody moonFind = property.celestialBodyList.Find(o => o.property.type == CelestialBody.ObjectType.Moon);
            if (sunFind is null && moonFind is null) return null;
            if (sunFind is not null && moonFind is null) return sunFind;
            if (sunFind is null) return moonFind;
            if (sunFind.property.lightComponent is null && moonFind.property.lightComponent is null) return null;
            if (sunFind.property.lightComponent is not null && moonFind.property.lightComponent is null) return sunFind;
            if (moonFind.property.lightComponent is not null && sunFind.property.lightComponent is null) return moonFind;
            if (moonFind.property.lightComponent is not null && sunFind.property.lightComponent is not null)
                return sunFind.direction.y >= moonFind.direction.y ? sunFind : moonFind;

            return null;
        }

        public CelestialBody GetSunCelestialBody()
        { 
            return property.celestialBodyList.Find(o => o.property.type == CelestialBody.ObjectType.Sun);
        }
        
        public void LensFlareOperations()
        {
            if (WorldManager.Instance.volumeCloudOptimizeModule != null)
            {
                WorldManager.Instance.volumeCloudOptimizeModule.useAddCloudMaskToDepth = 
                    property.celestialBodyList.Find(o => o.property.useLensFlare) != null;
                WorldManager.Instance.volumeCloudOptimizeModule.OnValidate();
            }
        }

        #endregion


        #region Gizmos相关
        
#if UNITY_EDITOR
        protected override void DrawGizmosSelected()
        {
            if (property.celestialBodyList.Count == 0 || property.celestialBodyList == null) return;

            Color Cache = Gizmos.color;
            Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            Handles.color = Gizmos.color;
            
            int maxTrack = 0;
            foreach (var variable in property.celestialBodyList)
            {
                if (variable.property.sortOrder > maxTrack) maxTrack = variable.property.sortOrder;
            }
            Vector3[] Positions = new[]
            {
                transform.position + Quaternion.Euler(0,5,0) * transform.forward * maxTrack,
                transform.position + Quaternion.Euler(0,5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f,
                
                transform.position + Quaternion.Euler(0,5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f + transform.right * maxTrack * 0.2f * 0.5f,

                transform.position + Quaternion.Euler(0,5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f + transform.right * maxTrack * 0.2f * 0.5f,
                transform.position + transform.forward * maxTrack + transform.forward * maxTrack * 0.2f * 1.8f,
                
                transform.position + Quaternion.Euler(0,-5,0) * transform.forward * maxTrack,
                transform.position + Quaternion.Euler(0,-5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f,

                transform.position + Quaternion.Euler(0,-5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,-5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f + -transform.right * maxTrack * 0.2f * 0.5f,
                
                transform.position + Quaternion.Euler(0,-5,0) * transform.forward * maxTrack + transform.forward * maxTrack * 0.2f + -transform.right * maxTrack * 0.2f * 0.5f,
                transform.position + transform.forward * maxTrack + transform.forward * maxTrack * 0.2f * 1.8f,
                
                
                transform.position + Quaternion.Euler(0,5,0) * -transform.forward * maxTrack,
                transform.position + Quaternion.Euler(0,5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f,
                
                transform.position + Quaternion.Euler(0,5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f + -transform.right * maxTrack * 0.2f * 0.5f,

                transform.position + Quaternion.Euler(0,5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f + -transform.right * maxTrack * 0.2f * 0.5f,
                transform.position + -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f * 1.8f,
                
                transform.position + Quaternion.Euler(0,-5,0) * -transform.forward * maxTrack,
                transform.position + Quaternion.Euler(0,-5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f,

                transform.position + Quaternion.Euler(0,-5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,-5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f + transform.right * maxTrack * 0.2f * 0.5f,
                
                transform.position + Quaternion.Euler(0,-5,0) * -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f + transform.right * maxTrack * 0.2f * 0.5f,
                transform.position + -transform.forward * maxTrack + -transform.forward * maxTrack * 0.2f * 1.8f,
                
                
                transform.position + Quaternion.Euler(0,5,0) * transform.right * maxTrack,
                transform.position + Quaternion.Euler(0,5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f,
                
                transform.position + Quaternion.Euler(0,5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f + -transform.forward * maxTrack * 0.2f * 0.5f,

                transform.position + Quaternion.Euler(0,5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f + -transform.forward * maxTrack * 0.2f * 0.5f,
                transform.position + transform.right * maxTrack + transform.right * maxTrack * 0.2f * 1.8f,
                
                transform.position + Quaternion.Euler(0,-5,0) * transform.right * maxTrack,
                transform.position + Quaternion.Euler(0,-5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f,

                transform.position + Quaternion.Euler(0,-5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,-5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f + transform.forward * maxTrack * 0.2f * 0.5f,
                
                transform.position + Quaternion.Euler(0,-5,0) * transform.right * maxTrack + transform.right * maxTrack * 0.2f + transform.forward * maxTrack * 0.2f * 0.5f,
                transform.position + transform.right * maxTrack + transform.right * maxTrack * 0.2f * 1.8f,
                
                
                transform.position + Quaternion.Euler(0,5,0) * -transform.right * maxTrack,
                transform.position + Quaternion.Euler(0,5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f,
                
                transform.position + Quaternion.Euler(0,5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f + transform.forward * maxTrack * 0.2f * 0.5f,

                transform.position + Quaternion.Euler(0,5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f + transform.forward * maxTrack * 0.2f * 0.5f,
                transform.position + -transform.right * maxTrack + -transform.right * maxTrack * 0.2f * 1.8f,
                
                transform.position + Quaternion.Euler(0,-5,0) * -transform.right * maxTrack,
                transform.position + Quaternion.Euler(0,-5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f,

                transform.position + Quaternion.Euler(0,-5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f,
                transform.position + Quaternion.Euler(0,-5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f + -transform.forward * maxTrack * 0.2f * 0.5f,
                
                transform.position + Quaternion.Euler(0,-5,0) * -transform.right * maxTrack + -transform.right * maxTrack * 0.2f + -transform.forward * maxTrack * 0.2f * 0.5f,
                transform.position + -transform.right * maxTrack + -transform.right * maxTrack * 0.2f * 1.8f,

            };
            Handles.DrawWireArc(transform.position, transform.up, Quaternion.Euler(0,5,0) * transform.forward, 80, maxTrack,2);
            Handles.DrawWireArc(transform.position, transform.up, Quaternion.Euler(0,5,0) * -transform.forward, 80, maxTrack,2);
            Handles.DrawWireArc(transform.position, transform.up, Quaternion.Euler(0,5,0) * transform.right, 80, maxTrack,2);
            Handles.DrawWireArc(transform.position, transform.up, Quaternion.Euler(0,5,0) * -transform.right, 80, maxTrack,2);
            Gizmos.DrawLineList(Positions);
            
            Mesh North = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.worldsystem//Textures/Icon/North.mesh");
            Mesh South = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.worldsystem//Textures/Icon/South.mesh");
            Mesh East = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.worldsystem//Textures/Icon/East.mesh");
            Mesh West = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.worldsystem//Textures/Icon/West.mesh");
            
            Gizmos.DrawWireMesh(North, 
                transform.position + transform.forward * maxTrack + transform.forward * 0.5f, 
                Quaternion.Euler(-90,180,0), new Vector3(80,80,80) * maxTrack * 0.2f);
            Gizmos.DrawWireMesh(South, 
                transform.position + -transform.forward * maxTrack + -transform.forward * 0.5f, 
                Quaternion.Euler(-90,0,0), new Vector3(80,80,80) * maxTrack * 0.2f);
            Gizmos.DrawWireMesh(East, 
                transform.position + transform.right * maxTrack + transform.right * 0.5f, 
                Quaternion.Euler(-90,-90,0), new Vector3(80,80,80) * maxTrack * 0.2f);
            Gizmos.DrawWireMesh(West, 
                transform.position + -transform.right * maxTrack + -transform.right * 0.5f, 
                Quaternion.Euler(-90,90,0), new Vector3(80,80,80) * maxTrack * 0.2f);
            
            Gizmos.color = Cache;
            Handles.color = Cache;
        }
#endif
        
        #endregion

    }


    [ExecuteAlways]
    public partial class CelestialBodyManager : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [LabelText("星体列表")]
            [ListDrawerSettings(CustomAddFunction = "CreateCelestialBody", CustomRemoveIndexFunction = "DestroyCelestialBody",OnTitleBarGUI = "DrawRefreshButton")]
            [InlineEditor]
            public List<CelestialBody> celestialBodyList = new();
            
            [LabelText("星体数量限制")] [ReadOnly]
            public int maxCelestialBodyCount = 4;
            
            /// <summary>
            /// 销毁不在列表中的星体
            /// </summary>
            private void DestroyNotInListCelestialBody()
            {
                
                CelestialBody[] CelestialBodyAll =  WorldManager.Instance?.GetComponentsInChildren<CelestialBody>();
                if (CelestialBodyAll != null && CelestialBodyAll.Length > 0)
                {
                    foreach (var VARIABLE in CelestialBodyAll)
                    {
                        if(!celestialBodyList.Contains(VARIABLE))
                            CoreUtils.Destroy(VARIABLE.gameObject);
                    }
                }
            }
            
            public void CreateCelestialBody()
            {
                DestroyNotInListCelestialBody();

                if (celestialBodyList.Count == maxCelestialBodyCount)
                {
                    Debug.Log($"最多支持到{maxCelestialBodyCount}个星体! 请与TA沟通!");
                    return;
                }

                GameObject gameObject = new GameObject("Celestial Body");
                gameObject.transform.position = WorldManager.Instance.transform.position;
                gameObject.transform.rotation = WorldManager.Instance.transform.rotation;
                gameObject.transform.localScale = WorldManager.Instance.transform.localScale;
                gameObject.transform.SetParent(WorldManager.Instance.transform);
                celestialBodyList.Add(gameObject.AddComponent<CelestialBody>());
                
                Shader.SetGlobalInteger(_SkyObjectCount, celestialBodyList?.Count ?? 0);

            }
            
            public void DestroyCelestialBody(int index)
            {
                DestroyNotInListCelestialBody();
                
                CoreUtils.Destroy(celestialBodyList[index].gameObject);
                celestialBodyList.RemoveAt(index);
                
                Shader.SetGlobalInteger(_SkyObjectCount, celestialBodyList?.Count ?? 0);

            }
            
#if UNITY_EDITOR
            public  void DrawRefreshButton()
            {
                DestroyNotInListCelestialBody();
                
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
                {
                    celestialBodyList = celestialBodyList.OrderByDescending(o => o.property.sortOrder).ToList();
                }
                
                Shader.SetGlobalInteger(_SkyObjectCount, celestialBodyList?.Count ?? 0);
            }
#endif
            
            private readonly int _SkyObjectCount = Shader.PropertyToID("_SkyObjectCount");
            
        }
        
        [HideLabel]
        public Property property = new();

        [HideInInspector]
        public CelestialBody sun;
        
        [HideInInspector] public bool update;

        private readonly int _SkyObjectCount = Shader.PropertyToID("_SkyObjectCount");
        private readonly int _Direction = Shader.PropertyToID("_Direction");
        private readonly int _Color = Shader.PropertyToID("_Color");
        private readonly int _Falloff = Shader.PropertyToID("_Falloff");
        
        #endregion
        
        
        #region 安装参数
        
        private void SetupStaticProperty()
        {
            Shader.SetGlobalInteger(_SkyObjectCount, property.celestialBodyList?.Count ?? 0);
        }
        
        private void SetupDynamicProperty()
        {
            //设置动态全局参数
            Vector4[] directions = new Vector4[property.maxCelestialBodyCount];
            Vector4[] colors = new Vector4[property.maxCelestialBodyCount];
            float[] falloffs = new float[property.maxCelestialBodyCount];
            if (property.celestialBodyList == null) return;
            for (int i = 0; i < property.celestialBodyList.Count; i++)
            {
                directions[i] = property.celestialBodyList[i].direction;
                colors[i] = property.celestialBodyList[i].GetAtmosphereScatterColor();
                falloffs[i] = property.celestialBodyList[i].property.falloffExecute;
            }
            Shader.SetGlobalVectorArray(_Direction, directions);
            Shader.SetGlobalVectorArray(_Color, colors);
            Shader.SetGlobalFloatArray(_Falloff, falloffs);
        }
        
        #endregion
        

        #region 事件函数
        private void OnEnable()
        {
            OnValidate();
        }
        
        public void OnValidate()
        {
            sun = GetSunCelestialBody();
            SetupStaticProperty();
        }
        
        private void OnDestroy()
        {
            foreach (var variable in property.celestialBodyList)
            {
                CoreUtils.Destroy(variable.gameObject);
            }
            property.celestialBodyList.Clear();
            property.celestialBodyList = null;
        }
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif

        
        private void Update()
        {
            if (!update) return;

            SetupDynamicProperty();
        }

        
        #endregion
        
        
        #region 渲染函数
        
        public void RenderCelestialBodyList(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (property.celestialBodyList == null || property.celestialBodyList.Count == 0 || !isActiveAndEnabled) return;
            foreach (var variable in property.celestialBodyList)
            {
                variable?.RenderCelestialBody(cmd, ref renderingData);
            }
        }
        
        public void RenderCelestialBodyList(CommandBuffer cmd,  Vector3 cameraPosition)
        {
            if (property.celestialBodyList == null || property.celestialBodyList.Count == 0 || !isActiveAndEnabled) return;
            foreach (var variable in property.celestialBodyList)
            {
                variable?.RenderCelestialBody(cmd, cameraPosition);
            }
        }
        
        #endregion
        
    }

}