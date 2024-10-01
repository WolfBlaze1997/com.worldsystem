using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif

namespace WorldSystem.Runtime
{
    
    public partial class AtmosphereModule
    {
        /// <summary>
        /// 定义一天中的时段
        /// </summary>
        [Serializable]
        public struct DayPeriods
        {
            [LabelText("时段")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public string description;

            [LabelText("开始时间")] [PropertyRange(0f, 24f)] [Unit(Units.Hour)]
            [GUIColor(0.7f, 0.7f, 1f)]
            public float startTime;

            [LabelText("天空颜色")] [ColorUsage(true, true)]
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color skyColor;

            [LabelText("赤道颜色")] [ColorUsage(true, true)]
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color equatorColor;

            [LabelText("地面颜色")] [ColorUsage(true, true)]
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color groundColor;

            public DayPeriods(string desc, float start, Color sky, Color equator, Color ground)
            {
                description = desc;
                startTime = start;
                skyColor = sky;
                equatorColor = equator;
                groundColor = ground;
            }
        }

        /// <summary>
        /// 定义大气颜色
        /// </summary>
        [Serializable]
        public struct AtmosphereColor
        {
            [HorizontalGroup("大气颜色")] [LabelText("天空颜色")][ColorUsage(true,true)]
            public Color skyColor;

            [HorizontalGroup("大气颜色")] [LabelText("赤道颜色")][ColorUsage(true,true)]
            public Color equatorColor;

            [HorizontalGroup("大气颜色")] [LabelText("地面颜色")][ColorUsage(true,true)]
            public Color groundColor;
        }
    }
    
    
    [ExecuteAlways]
    public partial class AtmosphereModule : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("网格")] 
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Mesh mesh;

            [FoldoutGroup("配置")] [LabelText("着色器")] 
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Shader shader;

            [FoldoutGroup("配置")] [LabelText("材质")] 
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Material material;

            [FoldoutGroup("配置")] [LabelText("大气混合着色器")] 
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Shader atmosphereBlendShader;

            [FoldoutGroup("配置")] [LabelText("大气混合材质")] 
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Material atmosphereBlendMaterial;

            [FoldoutGroup("颜色")] [LabelText("时段")]
            [GUIColor(1f,0.7f,0.7f)]
            [ListDrawerSettings(CustomAddFunction = "DayPeriodsListAddFunc", CustomRemoveIndexFunction = "DayPeriodsListRemoveFunc",OnTitleBarGUI = "DrawRefreshButton")]
            public List<DayPeriods> dayPeriodsList = new()
            {
                new DayPeriods("Night", 6f, new Color32(35, 4, 46, 255), new Color32(36, 21, 36, 255),
                    new Color32(26, 23, 26, 255)),
                new DayPeriods("Day", 7f, new Color32(18, 25, 59, 255), new Color32(123, 153, 173, 255),
                    new Color32(14, 12, 10, 255)),
                new DayPeriods("Day", 18f, new Color32(18, 25, 59, 255), new Color32(123, 153, 173, 255),
                    new Color32(14, 12, 10, 255)),
                new DayPeriods("Night", 19f, new Color32(35, 4, 46, 255), new Color32(36, 21, 36, 255),
                    new Color32(26, 23, 26, 255))
            };
            
            [FoldoutGroup("颜色")] [LabelText("当前大气颜色")] [ReadOnly]
            public AtmosphereColor currentAtmosphereColor;
            
            [FoldoutGroup("光照")] [LabelText("当前云量")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public float currentCloudiness;

            [FoldoutGroup("光照")] [LabelText("当前降雨")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public float currentPrecipitation;
            
            private protected virtual void DayPeriodsListAddFunc()
            {
                DayPeriods newDayPeriods = new("Day", 7f,
                    new Color32(18, 25, 59, 255),
                    new Color32(123, 153, 173, 255),
                    new Color32(14, 12, 10, 255));
                dayPeriodsList.Add(newDayPeriods);
            }
            private protected virtual void DayPeriodsListRemoveFunc(int Index)
            {
                dayPeriodsList.RemoveAt(Index);
            }
            
#if UNITY_EDITOR
            private protected virtual void DrawRefreshButton()
            {
                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
                {
                    //根据开始时间进行升序排序
                    dayPeriodsList = dayPeriodsList.OrderBy(o => o.startTime).ToList();
                }
            }
#endif
            
        }
        
        [HideLabel]
        public Property property  = new();

        [HideInInspector] 
        public bool update;
        
        private readonly int _HasSkyTexture = Shader.PropertyToID("_HasSkyTexture");
        private readonly int _HorizonColor = Shader.PropertyToID("_HorizonColor");
        private readonly int _ZenithColor = Shader.PropertyToID("_ZenithColor");
        private readonly int _SkyTexture = Shader.PropertyToID("_SkyTexture");

        private RTHandle _atmosphereMixRT;
        
        #endregion
        

        #region 安装参数

        private void SetupStaticProperty()
        {
            Shader.SetGlobalInt(_HasSkyTexture, 1);
        }

        private void SetupDynamicProperty()
        {
            Shader.SetGlobalColor(_HorizonColor, property.currentAtmosphereColor.equatorColor);
            Shader.SetGlobalColor(_ZenithColor, property.currentAtmosphereColor.skyColor);
        }

        #endregion
        
        
        #region 事件函数
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (property.mesh == null) property.mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.worldsystem//Meshes/SkySphere.mesh");
            if (property.shader == null)
                property.shader = AssetDatabase.LoadAssetAtPath<Shader>(
                    "Packages/com.worldsystem//Shader/Skybox/AtmosphereShader.shader");
            if (property.atmosphereBlendShader == null)
                property.atmosphereBlendShader =
                    AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem//Shader/Skybox/BlendAtmosphere.shader");
#endif
            if (property.material == null) property.material = CoreUtils.CreateEngineMaterial(property.shader);
            if (property.atmosphereBlendMaterial == null) property.atmosphereBlendMaterial = CoreUtils.CreateEngineMaterial(property.atmosphereBlendShader);
            
            OnValidate();
        }
        
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (property.mesh != null)
                Resources.UnloadAsset(property.mesh);
            if (property.shader != null)
                Resources.UnloadAsset(property.shader);
            if(property.atmosphereBlendShader != null)
                Resources.UnloadAsset(property.atmosphereBlendShader);
#endif
            if (property.material != null)
                CoreUtils.Destroy(property.material);
            if(property.atmosphereBlendMaterial != null)
                CoreUtils.Destroy(property.atmosphereBlendMaterial);
            
            _atmosphereMixRT?.Release();

            property.mesh = null;
            property.shader = null;
            property.material = null;
            property.atmosphereBlendShader = null;
            property.atmosphereBlendMaterial = null;
            _atmosphereMixRT = null;
            
            OnValidate();
        }

        public void OnValidate()
        {
            //固定渲染设置
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            
            SetupStaticProperty();
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
            
            if (property.dayPeriodsList.Count == 0 || property.dayPeriodsList == null) return;
            UpdatePeriod();
            UpdateAtmosphereColor();
            SetupDynamicProperty();
        }

        private int _currentPeriod;
        private int _nextPeriod;
        private void UpdatePeriod()
        {
            //根据时间获取当前时段
            int currentPeriod = property.dayPeriodsList.Count - 1;
            for (int i = 0; i < property.dayPeriodsList.Count; i++)
            {
                if ((WorldManager.Instance?.timeModule?.CurrentTime ?? 10) >= property.dayPeriodsList[i].startTime)
                {
                    currentPeriod = i;
                }
            }

            //更新时段参数
            if (currentPeriod != _currentPeriod)
            {
                _currentPeriod = currentPeriod;
                _nextPeriod = (currentPeriod + 1) % property.dayPeriodsList.Count;
            }

            //避免某些情况下索引超出范围
            _currentPeriod %= property.dayPeriodsList.Count;
            _nextPeriod %= property.dayPeriodsList.Count;
        }

        private void UpdateAtmosphereColor()
        {
            float currentTime = WorldManager.Instance?.timeModule?.CurrentTime ?? 10;

            //时间循环,当时段位于列表最后一位(即夜晚将要过渡到第二天白天)时,进行矫正
            float startTime = property.dayPeriodsList[_currentPeriod].startTime;
            if (startTime > property.dayPeriodsList[_nextPeriod].startTime)
            {
                startTime -= 24f;
            }

            if (currentTime > property.dayPeriodsList[_nextPeriod].startTime)
            {
                currentTime -= 24f;
            }

            //计算插值因子
            float lerp = HelpFunc.Remap(currentTime, startTime, property.dayPeriodsList[_nextPeriod].startTime, 0, 1);

            //线性插值
            property.currentAtmosphereColor.skyColor = Color.Lerp(property.dayPeriodsList[_currentPeriod].skyColor, property.dayPeriodsList[_nextPeriod].skyColor, lerp);
            property.currentAtmosphereColor.equatorColor = Color.Lerp(property.dayPeriodsList[_currentPeriod].equatorColor, property.dayPeriodsList[_nextPeriod].equatorColor, lerp);
            property.currentAtmosphereColor.groundColor = Color.Lerp(property.dayPeriodsList[_currentPeriod].groundColor, property.dayPeriodsList[_nextPeriod].groundColor, lerp);
            
            //应用大气颜色到环境光照
            RenderSettings.ambientSkyColor = property.currentAtmosphereColor.skyColor;
            RenderSettings.ambientEquatorColor = property.currentAtmosphereColor.equatorColor;
            RenderSettings.ambientGroundColor = property.currentAtmosphereColor.groundColor;
        }

        

        
        #endregion

        
        #region 渲染函数
        
        public void RenderAtmosphere(CommandBuffer cmd, ref RenderingData renderingData, RTHandle activeRT, Rect renderRect)
        {
            if (!isActiveAndEnabled) return;
            
            //设置矩阵
            var transformMatrix = Matrix4x4.identity;
            transformMatrix.SetTRS(renderingData.cameraData.camera.transform.position, Quaternion.identity,
                    Vector3.one * renderingData.cameraData.camera.farClipPlane);
            //渲染大气
            cmd.DrawMesh(property.mesh, transformMatrix, property.material, 0,0);
            
            // if (!property.useAtmosphereBlend) return;
            RenderingUtils.ReAllocateIfNeeded(ref _atmosphereMixRT, activeRT.rt.descriptor, name: "AtmosphereMix");
            cmd.CopyTexture(activeRT, 0,0, (int)renderRect.x, (int)renderRect.y, (int)renderRect.width, (int)renderRect.height, 
                _atmosphereMixRT,0,0, (int)renderRect.x, (int)renderRect.y);
            cmd.SetGlobalTexture(_SkyTexture, _atmosphereMixRT);
        }
        
        public void RenderAtmosphere(CommandBuffer cmd, ref RenderingData renderingData, RTHandle activeRT)
        {
            if (!isActiveAndEnabled) return;
            
            //设置矩阵
            var transformMatrix = Matrix4x4.identity;
            transformMatrix.SetTRS(renderingData.cameraData.camera.transform.position, Quaternion.identity,
                Vector3.one * renderingData.cameraData.camera.farClipPlane);
            //渲染大气
            cmd.DrawMesh(property.mesh, transformMatrix, property.material, 0,0);
            
            // if (!property.useAtmosphereBlend) return;
            RenderingUtils.ReAllocateIfNeeded(ref _atmosphereMixRT, activeRT.rt.descriptor, name: "AtmosphereMix");
            cmd.CopyTexture(activeRT,_atmosphereMixRT);
            cmd.SetGlobalTexture(_SkyTexture, _atmosphereMixRT);
        }
        
        
        #endregion
        
        
    }
}