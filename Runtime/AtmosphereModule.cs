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

namespace WorldSystem.Runtime
{
    
    
    public partial class AtmosphereModule
    {
        // 包含关于给定的一天关键帧的所有信息。
        [Serializable]
        public struct DayPeriods
        {
            [LabelText("时段")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public string description;

            [LabelText("开始时间")] [PropertyRange(0f, 24f)] [Unit(Units.Hour)]
            [GUIColor(0.7f, 0.7f, 1f)]
            public float startTime;

            [LabelText("天空颜色")] [ColorUsage(false, true)]
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color skyColor;

            [LabelText("赤道颜色")] [ColorUsage(false, true)]
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color equatorColor;

            [LabelText("地面颜色")] [ColorUsage(false, true)]
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

        [Serializable]
        public struct AtmosphereColor
        {
            [HorizontalGroup("大气颜色")] [LabelText("天空颜色")][ColorUsage(false,true)]
            public Color skyColor;

            [HorizontalGroup("大气颜色")] [LabelText("赤道颜色")][ColorUsage(false,true)]
            public Color equatorColor;

            [HorizontalGroup("大气颜色")] [LabelText("地面颜色")][ColorUsage(false,true)]
            public Color groundColor;
        }
        
        
        private Vector3 ToV3(float v)
        {
            return new Vector3(v, v, v);
        }

        private static readonly Vector3 RGB_LUMINANCE = new Vector3(0.2126f, 0.7152f, 0.0722f);

        private Color Saturation(Color color, float saturation)
        {
            if (saturation == 1)
                return color;

            Vector3 c = new Vector3(color.r, color.g, color.b);
            float luma = Vector3.Dot(c, RGB_LUMINANCE);
            c = ToV3(luma) + (saturation * (c - ToV3(luma)));
            return new Color(c.x, c.y, c.z);
        }

        private Color Exposure(Color color, float exposure)
        {
            return color * exposure;
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
            [FoldoutGroup("颜色")] [LabelText("当前大气颜色")] 
            [ReadOnly]
            public AtmosphereColor currentAtmosphereColor;
            
            [FoldoutGroup("光照")] [LabelText("当前云量")]
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public float currentCloudiness;

            [FoldoutGroup("光照")] [LabelText("当前降雨")]
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public float currentPrecipitation;

            [FoldoutGroup("大气混合")] [LabelText("渲染大气图")]
            [GUIColor(0.3f, 1f, 0.3f)]
            public bool useAtmosphereMap = true;

            [FoldoutGroup("大气混合")][LabelText("使用大气混合")] 
            [GUIColor(0.3f, 1f, 0.3f)]
            public bool useAtmosphereBlend = true;

            [FoldoutGroup("大气混合")][LabelText("    开始")] 
            [Tooltip("设置大气混合的开始距离")][ShowIf("useAtmosphereBlend")][GUIColor(1f,0.7f,0.7f)]
            public float start;

            [FoldoutGroup("大气混合")][LabelText("    结束")] 
            [Tooltip("设置大气混合结束距离")][ShowIf("useAtmosphereBlend")][GUIColor(1f,0.7f,0.7f)]
            public float end = 20000;
            
        }
        
        [HideLabel]
        public Property property  = new();

        #endregion



        #region 安装参数

        private void SetupStaticProperty()
        {

            if (property.useAtmosphereMap)
            {
                Shader.SetGlobalInt(_HasSkyTexture, 1);
            }
            else
            {
                Shader.SetGlobalInt(_HasSkyTexture, 0);
                Shader.SetGlobalTexture(_SkyTexture, Texture2D.linearGrayTexture);
            }
        }
        private readonly int _HasSkyTexture = Shader.PropertyToID("_HasSkyTexture");

        private void SetupDynamicProperty()
        {
            Shader.SetGlobalColor(_HorizonColor, property.currentAtmosphereColor.equatorColor);
            Shader.SetGlobalColor(_ZenithColor, property.currentAtmosphereColor.skyColor);
            Shader.SetGlobalFloat(_BlendStart, property.start);
            Shader.SetGlobalFloat(_Density, HelpFunc.GetDensityFromVisibilityDistance(property.end));
        }
        private readonly int _HorizonColor = Shader.PropertyToID("_HorizonColor");
        private readonly int _ZenithColor = Shader.PropertyToID("_ZenithColor");
        private readonly int _BlendStart = Shader.PropertyToID("_BlendStart");
        private readonly int _Density = Shader.PropertyToID("_Density");
        
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
            
            _atmosphereMapRT?.Release();
            _atmosphereBlendRT?.Release();

            property.mesh = null;
            property.shader = null;
            property.material = null;
            property.atmosphereBlendShader = null;
            property.atmosphereBlendMaterial = null;
            _atmosphereBlendRT = null;
            _atmosphereMapRT = null;
            
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
            WorldManager.Instance?.weatherSystemModule?.weatherList?.SetupPropertyFromActive();
        }
#endif

        
        private int _frameID;
        private int _updateCount;
#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying) return;
            UpdateFunc();
        }
        private void FixedUpdate()
        {
            if (Time.frameCount == _frameID) return;
            
            //分帧器,将不同的操作分散到不同的帧,提高帧率稳定性
            if (_updateCount % 1 == 0)
            {
                _RenderAtmosphereMap = true;
                _RenderAtmosphereBlend = true;
            }
            if (_updateCount % 2 == 0)
            {
                UpdateFunc();
            }
            _updateCount++;
            
            
            _frameID = Time.frameCount;
        }
#else
        private void FixedUpdate()
        {
            if (Time.frameCount == _frameID) return;
            
            //分帧器,将不同的操作分散到不同的帧,提高帧率稳定性
            if (_updateCount % 1 == 0)
            {
                _RenderAtmosphereMap = true;
                _RenderAtmosphereBlend = true;
            }
            if (_updateCount % 2 == 0)
            {
                UpdateFunc();
            }
            _updateCount++;
            
            
            _frameID = Time.frameCount;
        }
#endif
        private void UpdateFunc()
        {
            if (property.dayPeriodsList.Count == 0 || property.dayPeriodsList == null) return;

            UpdatePeriod();
            UpdateAtmosphereColor();
            
            SetupDynamicProperty();
        }
        private int _currentPeriod = 0;
        private int _nextPeriod = 0;
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
            

            //根据参数修饰大气颜色
            // float exposure = property.environmentLightingExposure;
            // property.currentCloudiness = WorldManager.Instance?.volumeCloudOptimizeModule?.property._Modeling_Amount_CloudAmount ?? 0;
            // property.currentPrecipitation = Math.Max(WorldManager.Instance?.weatherEffectModule?.rainEffect?.property.rainPrecipitation ?? 0,
            //                                 WorldManager.Instance?.weatherEffectModule?.snowEffect?.property.snowPrecipitation ?? 0);
            // exposure *= (1.0f - Mathf.Max(property.currentPrecipitation, property.currentCloudiness) * 0.8f);
            // property.currentAtmosphereColor.skyColor =
            //     Exposure(Saturation(property.currentAtmosphereColor.skyColor, property.environmentLightingSaturation), exposure);
            // property.currentAtmosphereColor.equatorColor =
            //     Exposure(Saturation(property.currentAtmosphereColor.equatorColor, property.environmentLightingSaturation), exposure);
            // property.currentAtmosphereColor.groundColor =
            //     Exposure(Saturation(property.currentAtmosphereColor.groundColor, property.environmentLightingSaturation), exposure);

            
        }

        

        
        #endregion

        
        #region 渲染函数
        
        public void RenderAtmosphere(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!isActiveAndEnabled) return;
            
            //设置矩阵
            _transformMatrix.SetTRS(renderingData.cameraData.camera.transform.position, Quaternion.identity,
                    Vector3.one * renderingData.cameraData.camera.farClipPlane);

            //渲染大气
            cmd.DrawMesh(property.mesh, _transformMatrix, property.material, 0,0);
        }
        
        
        
        private Matrix4x4 _transformMatrix = Matrix4x4.identity;
        private bool _RenderAtmosphereMap;
        public void RenderAtmosphereMap(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
//             if (!_RenderAtmosphereMap && Time.renderedFrameCount > 2
// #if UNITY_EDITOR  
//                         && Application.isPlaying
// #endif
//                )
//             {
//                 return;
//             }
            if (!property.useAtmosphereMap || !isActiveAndEnabled) return;
            _RenderAtmosphereMap = false;
            
            //配置RT
            RenderTextureDescriptor atmosphereMapDescriptor = new RenderTextureDescriptor(
                renderingData.cameraData.cameraTargetDescriptor.height >> 3,
                renderingData.cameraData.cameraTargetDescriptor.height >> 3,
                RenderTextureFormat.DefaultHDR);
            RenderingUtils.ReAllocateIfNeeded(ref _atmosphereMapRT, atmosphereMapDescriptor, name: "AtmosphereMap");
            
            //渲染大气图
            cmd.SetRenderTarget(_atmosphereMapRT);
            cmd.DrawMesh(property.mesh, _transformMatrix, property.material, 0, 1);

            //输出大气图为全局参数
            cmd.SetGlobalTexture(_SkyTexture, _atmosphereMapRT);
        }
        private RTHandle _atmosphereMapRT;
        private readonly int _SkyTexture = Shader.PropertyToID("_SkyTexture");
        private bool _RenderAtmosphereBlend;
        public void RenderAtmosphereBlend(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
//             if (!_RenderAtmosphereBlend && Time.renderedFrameCount > 2
// #if UNITY_EDITOR  
//                                         && Application.isPlaying
// #endif
//                )
//             {
//                 Blitter.BlitCameraTexture(cmd, _atmosphereBlendRT, source);
//                 return;
//             }
            if (!property.useAtmosphereBlend || !isActiveAndEnabled) return;
            // _RenderAtmosphereBlend = false;
            
            //配置RT
            RenderTextureDescriptor rtDescriptor = new RenderTextureDescriptor(
                renderingData.cameraData.cameraTargetDescriptor.width,
                renderingData.cameraData.cameraTargetDescriptor.height,
                colorFormat: RenderTextureFormat.DefaultHDR);
            RenderingUtils.ReAllocateIfNeeded(ref _atmosphereBlendRT, rtDescriptor);

            
            
            cmd.SetGlobalTexture(_ScreenTexture, source);
            cmd.SetRenderTarget(_atmosphereBlendRT);
            Blitter.BlitTexture(cmd,new Vector4(1,1,0,0), property.atmosphereBlendMaterial,0);
            Blitter.BlitCameraTexture(cmd, _atmosphereBlendRT, source);
        }
        private RTHandle _atmosphereBlendRT;
        private readonly int _ScreenTexture = Shader.PropertyToID("_ScreenTexture");
        
        #endregion
        
    }
}