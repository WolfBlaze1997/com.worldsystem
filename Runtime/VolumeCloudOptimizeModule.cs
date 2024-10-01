using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;
// ReSharper disable UselessBinaryOperation

namespace WorldSystem.Runtime
{

    public partial class VolumeCloudOptimizeModule
    {

        #region 枚举与帮助函数
        
        public enum CloudShadowResolution
        {
            Low = 256,
            Medium = 512,
            High = 1024,
        }

        public enum Scale
        {
            Full,
            Half,
            Quarter
        }

        public enum Noise3DTextureId
        {
            None,
            Perlin,
            PerlinWorley,
            PerlinWorleyMix,
            Worley,
            Billow
        }

        public enum TextureQuality
        {
            Medium,
            High,
            Ultra
        }

        public enum CelestialBodySelection
        {
            Earth,
            Mars,
            Venus,
            Luna,
            Titan,
            Enceladus,
            Custom
        }
        
        public enum ResolutionOptions
        {
            Full,
            Half
        }
        
        /// <summary>
        /// 加载体积纹理
        /// </summary>
        private Texture3D LoadVolumeTexture(Noise3DTextureId id, TextureQuality quality, Texture3D currentTexture)
        {
#if UNITY_EDITOR
            string loadTarget = "Packages/com.worldsystem//Textures/Noise Textures/VolumeTextures/";
            switch (id)
            {
                case Noise3DTextureId.None:
                    return CoreUtils.blackVolumeTexture;
                case Noise3DTextureId.Perlin:
                    loadTarget += "Perlin/Perlin";
                    break;
                case Noise3DTextureId.PerlinWorley:
                    loadTarget += "PerlinWorley/PerlinWorley";
                    break;
                case Noise3DTextureId.PerlinWorleyMix:
                    loadTarget += "PerlinWorleyMix/PerlinWorleyMix";
                    break;
                case Noise3DTextureId.Worley:
                    loadTarget += "Worley/Worley";
                    break;
                case Noise3DTextureId.Billow:
                    loadTarget += "Billow/Billow";
                    break;
                default:
                    return CoreUtils.blackVolumeTexture;
            }

            switch (quality)
            {
                case TextureQuality.Medium:
                    loadTarget += "32";
                    break;
                case TextureQuality.High:
                    loadTarget += "64";
                    break;
                case TextureQuality.Ultra:
                    loadTarget += "128";
                    break;
                default:
                    return CoreUtils.blackVolumeTexture;
            }

            loadTarget += ".asset";
            return AssetDatabase.LoadAssetAtPath<Texture3D>(loadTarget);
#else
            return currentTexture;
#endif
        }
        
        /// <summary>
        /// 一种辅助方法，我们将每个天体的半径编码为KM。
        /// </summary>
        private int GetRadiusFromCelestialBodySelection(CelestialBodySelection celestialBodySelection, int currentVal)
        {
            switch (celestialBodySelection)
            {
                case CelestialBodySelection.Earth:
                    return 6378;
                case CelestialBodySelection.Mars:
                    return 3389;
                case CelestialBodySelection.Venus:
                    return 6052;
                case CelestialBodySelection.Luna:
                    return 1737;
                case CelestialBodySelection.Titan:
                    return 2575;
                case CelestialBodySelection.Enceladus:
                    return 252;
                default:
                    return Mathf.Max(0, currentVal);
            }
        }

        public float GetAtmosphereVisibility()
        {
            //此处需要大气配置 大气混合 模块
            // float visibility = WorldManager.Instance?.atmosphereModule?.property?.end ?? 20000;
            // float visibility = 20000;
            
            // if (property._Lighting_UseAtmosphereVisibilityOverlay)
            // {
            float visibility = property.lightingAtmosphereVisibility;
            // }

            const float factor = 3.912023005f;
            return factor / visibility;
        }
        
        private Vector3 Div(Vector3 a, float b)
        {
            return new Vector3(a.x / b, a.y / b, a.z / b);
        }

        private Vector3 Floor(Vector3 i)
        {
            return new Vector3(Mathf.Floor(i.x), Mathf.Floor(i.y), Mathf.Floor(i.z));
        }
        
#if UNITY_EDITOR
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            direction += new Vector3(0.00001f, 0.00001f, 0.00001f);
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        
        private void OnDrawGizmosSelected()
        {
            Vector3 pos;

            Color Cache = Gizmos.color;
            Gizmos.color = Color.gray;
            pos = SetupGizmosColorAndPosition();
            Vector3 destination =
                new Vector3(property.motionBaseDynamicVector.x, 0, property.motionBaseDynamicVector.y).normalized
                * (float)((1 - Math.Pow(Math.E, -property.motionBaseSpeed)) * 2);
            ForGizmo(pos, destination);

            _ = SetupGizmosColorAndPosition();
            Vector3 destination01 = property.motionDetailDynamicVector.normalized *
                                    (float)((1 - Math.Pow(Math.E, -property.motionDetailSpeed)) * 2);
            ForGizmo(pos, destination01);

            Gizmos.color = Cache;
        }

        private Vector3 SetupGizmosColorAndPosition()
        {
            Vector3 pos;
            if (WorldManager.Instance?.windZoneModule != null)
            {
                pos = WorldManager.Instance.windZoneModule.transform.position;
            }
            else
            {
                pos = transform.position;
            }

            return pos;
        }

#endif
        // ReSharper disable once UnusedMember.Local
        private Texture2DArray Texture2DToTexture2DArray(Texture2D[] blue)
        {
            Texture2DArray blueArray =
                new Texture2DArray(blue[0].width, blue[0].height, blue.Length, blue[0].format, false);
            for (int i = 0; i < blue.Length; i++)
            {
                blueArray.SetPixels(blue[i].GetPixels(), i);
            }

            blueArray.Apply(false);
            return blueArray;
        }
        
        #endregion
        
    }

    
    /// <summary>
    /// 基础云纹理
    /// </summary>
    public partial class VolumeCloudOptimizeModule
    {
        
        #region 字段
        
        public partial class Property
        {
            [Title("云图")] [FoldoutGroup("配置")] [LabelText("云图着色器")] [ReadOnly] [PropertyOrder(-20)] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader volumeCloudBaseTexShader;
            
            [FoldoutGroup("配置")] [LabelText("云图材质")] [ReadOnly] [PropertyOrder(-20)] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material volumeCloudBaseTexMaterial;
            
            [FoldoutGroup("渲染")] [LabelText("最大渲染距离(m)")] [GUIColor(0.7f, 0.7f, 1.0f)] [PropertyOrder(-10)] 
            public float renderMaxRenderDistance = 20000f;

            [FoldoutGroup("建模")] [TitleGroup("建模/云量")] [LabelText("云量")][PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)] [PropertyOrder(-10)]
            public float modelingAmountCloudAmount = 0.6f;

            [TitleGroup("建模/云量")] [LabelText("覆盖远程云量")] [GUIColor(0.7f, 0.7f, 1.0f)]
            public bool modelingAmountUseFarOverlay;

            [TitleGroup("建模/云量")] [LabelText("    开始距离(m)")] [GUIColor(1f, 0.7f, 0.7f)] [ShowIf("modelingAmountUseFarOverlay")]
            public float modelingAmountOverlayStartDistance = 20000f;

            [TitleGroup("建模/云量")] [LabelText("    云量")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)] [ShowIf("modelingAmountUseFarOverlay")]
            public float modelingAmountOverlayCloudAmount = 0.8f;

            [TitleGroup("建模/位置")] [LabelText("星球半径预设")] [GUIColor(0.7f, 0.7f, 1f)]
            public CelestialBodySelection modelingPositionRadiusPreset;

            [TitleGroup("建模/位置")] [LabelText("    星球半径(km)")] [MinValue(0)] [GUIColor(0.7f, 0.7f, 1f)] [EnableIf("@modelingPositionRadiusPreset == CelestialBodySelection.Custom")]
            public int modelingPositionPlanetRadius = 6378;

            [TitleGroup("建模/位置")] [LabelText("云层海拔(m)")] [MinValue(0)] [GUIColor(1f, 0.7f, 0.7f)]
            public float modelingPositionCloudHeight = 600f;

            [TitleGroup("建模/位置")] [LabelText("云层厚度(m)")] [PropertyRange(100, 8000)] [GUIColor(1f, 0.7f, 0.7f)]
            public float modelingPositionCloudThickness = 4000f;
            
            [TitleGroup("建模/基础(云图)")] [LabelText("八度音程")] [PropertyRange(1, 6)] [GUIColor(0.7f, 0.7f, 1f)]
            public int modelingShapeBaseOctaves = 3;
            
            [TitleGroup("建模/基础(云图)")] [LabelText("增益")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)]
            public float modelingShapeBaseGain = 0.5f;
            
            [TitleGroup("建模/基础(云图)")] [LabelText("频率")] [PropertyRange(2, 5)] [GUIColor(0.7f, 0.7f, 1f)]
            public float modelingShapeBaseFreq = 2f;
            
            [TitleGroup("建模/基础(云图)")] [LabelText("比例")] [GUIColor(1f, 0.7f, 0.7f)]
            public float modelingShapeBaseScale = 5f;

            [Title("基础(云图)")] [FoldoutGroup("运动")] [LabelText("动态矢量")] [PropertyOrder(-1)] [GUIColor(0.7f, 0.7f, 0.7f)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Vector2 motionBaseDynamicVector = Vector2.zero;

            [FoldoutGroup("运动")] [LabelText("方向")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule == null")]
            public float motionBaseDirection;

            [FoldoutGroup("运动")] [LabelText("速度")] [PropertyRange(0, 5)]
            [GUIColor(1f, 0.7f, 0.7f)] [PropertyOrder(-1)] [ShowIf("@WorldManager.Instance?.timeModule != null")]
            public float motionBaseSpeed = 0.25f;

            [FoldoutGroup("运动")] [LabelText("使用方向随机")]
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-1)] [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null")]
            public bool motionBaseUseDirectionRandom = true;

            [FoldoutGroup("运动")] [LabelText("    随机范围")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && motionBaseUseDirectionRandom")]
            public float motionBaseDirectionRandomRange = 60;

            [FoldoutGroup("运动")] [LabelText("    随机频率")] [GUIColor(0.7f, 0.7f, 1f)][PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && motionBaseUseDirectionRandom")]
            public float motionBaseDirectionRandomFreq = 12;
            
        }
        
        
        #endregion


        
        #region 安装属性
        
        private readonly int _Render_MaxRenderDistance_ID = Shader.PropertyToID("_Render_MaxRenderDistance");
        private readonly int _Modeling_Amount_UseFarOverlay_ID = Shader.PropertyToID("_Modeling_Amount_UseFarOverlay");
        private readonly int _Modeling_ShapeBase_Octaves_ID = Shader.PropertyToID("_Modeling_ShapeBase_Octaves");
        private readonly int _Modeling_ShapeBase_Freq_ID = Shader.PropertyToID("_Modeling_ShapeBase_Freq");
        private readonly int _Modeling_Amount_CloudAmount_ID = Shader.PropertyToID("_Modeling_Amount_CloudAmount");
        private readonly int _Modeling_Amount_OverlayStartDistance_ID = Shader.PropertyToID("_Modeling_Amount_OverlayStartDistance");
        private readonly int _Modeling_Amount_OverlayCloudAmount_ID = Shader.PropertyToID("_Modeling_Amount_OverlayCloudAmount");
        private readonly int _Modeling_ShapeBase_Gain_ID = Shader.PropertyToID("_Modeling_ShapeBase_Gain");
        private readonly int _Modeling_ShapeBase_Scale_ID = Shader.PropertyToID("_Modeling_ShapeBase_Scale");
        private readonly int _MotionBase_Position_ID = Shader.PropertyToID("_MotionBase_Position");
        
        private void SetupStaticProperty_CloudBaseTex()
        {
            Shader.SetGlobalFloat(_Render_MaxRenderDistance_ID, property.renderMaxRenderDistance);
            Shader.SetGlobalFloat(_Modeling_Amount_UseFarOverlay_ID, property.modelingAmountUseFarOverlay ? 1 : 0);
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Octaves_ID, property.modelingShapeBaseOctaves);
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Freq_ID, property.modelingShapeBaseFreq);
        }
        
        private void SetupDynamicProperty_CloudBaseTex()
        {
            Shader.SetGlobalFloat(_Modeling_Amount_CloudAmount_ID, property.modelingAmountCloudAmount);
            if (property.modelingAmountUseFarOverlay)
            {
                Shader.SetGlobalFloat(_Modeling_Amount_OverlayStartDistance_ID, property.modelingAmountOverlayStartDistance);
                Shader.SetGlobalFloat(_Modeling_Amount_OverlayCloudAmount_ID, property.modelingAmountOverlayCloudAmount);
            }
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Gain_ID, property.modelingShapeBaseGain);
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Scale_ID, property.modelingShapeBaseScale);
            Shader.SetGlobalVector(_MotionBase_Position_ID, _motionBasePosition);
        }
        
        #endregion


        
        #region 事件函数

        private void OnEnable_CloudMap()
        {
#if UNITY_EDITOR
            if (property.volumeCloudBaseTexShader == null)
                property.volumeCloudBaseTexShader = AssetDatabase.LoadAssetAtPath<Shader>(
                    "Packages/com.worldsystem//Shader/VolumeClouds_V1_1_20240604/VolumeCloudBaseTex_V1_1_20240604.shader");
#endif
            if (property.volumeCloudBaseTexMaterial == null)
                property.volumeCloudBaseTexMaterial =
                    CoreUtils.CreateEngineMaterial(property.volumeCloudBaseTexShader);

            _cloudBaseTexRT ??= RTHandles.Alloc(new RenderTextureDescriptor(512, 512, GraphicsFormat.B10G11R11_UFloatPack32,0),
                name: "CloudMapRT");
        }

        private void OnDisable_CloudMap()
        {
            if (property.volumeCloudBaseTexShader != null)
                Resources.UnloadAsset(property.volumeCloudBaseTexShader);
            if (property.volumeCloudBaseTexMaterial != null)
                CoreUtils.Destroy(property.volumeCloudBaseTexMaterial);
            _cloudBaseTexRT?.Release();

            property.volumeCloudBaseTexShader = null;
            _cloudBaseTexRT = null;
            property.volumeCloudBaseTexMaterial = null;
        }

        private void OnValidate_CloudMap()
        {
            SetupStaticProperty_CloudBaseTex();
        }
        
        private void Update_CloudMap()
        {
            SetupDynamicProperty_CloudBaseTex();
        }
        
        #endregion


        
        #region 渲染函数
        
        private RTHandle _cloudBaseTexRT;
        
        private readonly int CloudBaseTexRT_ID = Shader.PropertyToID("CloudBaseTexRT");
        
        public void RenderCloudMap()
        {
            if (!isActiveAndEnabled || property.modelingAmountCloudAmount < 0.25f) return;
            
            //渲染云图
            Graphics.Blit(null, _cloudBaseTexRT, property.volumeCloudBaseTexMaterial, 0);

            //将云图设置为全局参数
            Shader.SetGlobalTexture(CloudBaseTexRT_ID, _cloudBaseTexRT);
        }
        
        
        #endregion
        
    }


    [ExecuteAlways]
    public partial class VolumeCloudOptimizeModule : BaseModule
    {
        /// 备忘录
        ///  - 可以将每个天气状态下的所有参数打包成一个个常量缓冲区,然后只需要向 GUP 传递当前天气 和 下一个天气, 在着色器中完成插值, 就可以极大降低
        ///  - 植物可以使用 Graphics.DrawMeshInstancedIndirect + structBuffer 实现每个实例的精细化变形, 将blender中顶点的偏移记录成数组, 使用JSON传输到unity
        ///  - 大气 天空颜色 赤道颜色 地面颜色 一天的昼夜变化可以生成 Lut表
        ///  - 使用第二个摄像机 和 摄像机运动矢量 来优化云渲染
        ///  - 时间调整只允许向前 不允许向后 添加一个重置按钮归零时间

        #region 字段
        
        [Serializable]
        public partial class Property
        {
            [Title("体积云")] [FoldoutGroup("配置")] [LabelText("体积云着色器")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader volumeCloudMainShader;
            
            [FoldoutGroup("配置")] [LabelText("体积云材质")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material volumeCloudMainMaterial;
            
            [FoldoutGroup("配置")] [LabelText("添加云遮罩到深度着色器")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader volumeCloudAddCloudMaskToDepthShader;
            
            [FoldoutGroup("配置")] [LabelText("添加云遮罩到深度材质")] [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material volumeCloudAddCloudMaskToDepthMaterial;
            
            [FoldoutGroup("渲染")] [LabelText("粗略步进")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)] 
            public int renderCoarseSteps = 32;

            [FoldoutGroup("渲染")] [LabelText("细节步进")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)]
            public int renderDetailSteps = 16;
            
            [FoldoutGroup("渲染")] [LabelText("Blue噪音纹理数组")] [GUIColor(0.7f, 0.7f, 1f)] [ReadOnly] [PropertyOrder(-10)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Texture2DArray renderBlueNoiseArray;

            [FoldoutGroup("渲染")] [LabelText("噪音")] [PropertyRange(0, 1)] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)] 
            public float renderBlueNoise = 1.0f;

            [FoldoutGroup("渲染")] [LabelText("Mipmap距离")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)] 
            public Vector2 renderMipmapDistance = new Vector2(4000, 8000);
            
            [TitleGroup("建模/细节")] [LabelText("3D噪音")] [GUIColor(0.7f, 0.7f, 0.7f)] [ReadOnly]
            public Texture3D modelingShapeDetailNoiseTexture3D;

            [TitleGroup("建模/细节")] [LabelText("类型")] [GUIColor(0.7f, 0.7f, 1f)]
            public Noise3DTextureId modelingShapeDetailType = Noise3DTextureId.Perlin;

            [TitleGroup("建模/细节")] [LabelText("质量")] [GUIColor(0.7f, 0.7f, 1f)]
            public TextureQuality modelingShapeDetailQuality = TextureQuality.High;

            [TitleGroup("建模/细节")] [LabelText("比例")] [GUIColor(1f, 0.7f, 0.7f)]
            public Vector3 modelingShapeDetailScale = new(5, 5, 5);

            [FoldoutGroup("运动")] [LabelText("使用动态体积云")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-2)]
            public bool motionBaseUseDynamicCloud = true;

            [FoldoutGroup("运动")] [LabelText("风速影响")] [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-2)]
            public float motionBaseWindSpeedCoeff = 1;
            
            [Title("细节")] [FoldoutGroup("运动")] [LabelText("动态矢量")] [GUIColor(0.7f, 0.7f, 0.7f)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Vector3 motionDetailDynamicVector;
            
            [FoldoutGroup("运动")] [LabelText("方向")] [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule == null")]
            public Vector2 motionDetailDirection;

            [FoldoutGroup("运动")] [LabelText("速度")] [GUIColor(1f, 0.7f, 0.7f)] [ShowIf("@WorldManager.Instance?.timeModule != null")]
            public float motionDetailSpeed = 1;
            
            [FoldoutGroup("运动")] [LabelText("使用随机")] [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null")]
            public bool motionDetailUseRandomDirection = true;
            
            [FoldoutGroup("运动")] [LabelText("    随机范围")] [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && motionDetailUseRandomDirection")]
            public Vector2 motionDetailDirectionRandomRange = new Vector2(40, 20);
            
            [FoldoutGroup("运动")] [LabelText("    随机频率")] [MinValue(2)] [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && motionDetailUseRandomDirection")]
            public float motionDetailDirectionRandomFreq = 12;

            [Title("基础")] [FoldoutGroup("光照")] [LabelText("反照率颜色")] [GUIColor(1f, 0.7f, 0.7f)]
            public Color lightingAlbedoColor = new Color(1.0f, 0.964f, 0.92f);

            [FoldoutGroup("光照")] [LabelText("光照颜色过滤")] [GUIColor(1f, 0.7f, 0.7f)]
            public Color lightingLightColorFilter = Color.white;
            
            [TitleGroup("光照/密度")] [LabelText("消光系数")] [GUIColor(1f, 0.7f, 0.7f)] 
            [HorizontalGroup("光照/密度/_Lighting_ExtinctionCoeff", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve lightingExtinctionCoeff = new AnimationCurve(new Keyframe(0,15f), new Keyframe(1,15f));
            
            [HorizontalGroup("光照/密度/_Lighting_ExtinctionCoeff")][HideLabel][ReadOnly]
            public float lightingExtinctionCoeffExecute = 15f;
            
            [FoldoutGroup("光照")] [LabelText("密度影响")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingDensityInfluence = 1.0f;

            [FoldoutGroup("光照")] [LabelText("海拔密度影响")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingHeightDensityInfluence = 1.0f;

            [Title("环境")] [FoldoutGroup("光照")] [LabelText("廉价环境光照")] [GUIColor(0.7f, 0.7f, 1f)]
            public bool lightingCheapAmbient = true;

            [FoldoutGroup("光照")] [LabelText("环境照明强度")] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingAmbientExposure = 1.0f;
            
            [FoldoutGroup("光照")] [LabelText("可见度")] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingAtmosphereVisibility = 30000f;

            [Title("照明")] [FoldoutGroup("光照")] [LabelText("HG强度")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingHgStrength = 1.0f;

            [FoldoutGroup("光照")] [LabelText("HG偏心度向前")] [PropertyRange(0f, 0.99f)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingHgEccentricityForward = 0.6f;

            [FoldoutGroup("光照")] [LabelText("HG偏心度向后")] [PropertyRange(-0.99f, 0.99f)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingHgEccentricityBackward = -0.2f;

            [FoldoutGroup("光照")] [LabelText("最大光照距离")] [GUIColor(1f, 0.7f, 0.7f)]
            public int lightingMaxLightingDistance = 2000;
            
            [FoldoutGroup("光照")] [LabelText("着色强度衰减")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingShadingStrengthFalloff = 0.2f;

            [FoldoutGroup("光照")] [LabelText("散射乘数")] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingScatterMultiplier = 100;

            [FoldoutGroup("光照")] [LabelText("散射强度")] [PropertyRange(0, 1)] [GUIColor(1f, 0.7f, 0.7f)]
            public float lightingScatterStrength = 1.0f;

            public void LimitProperty()
            {
                renderMaxRenderDistance = math.max(renderMaxRenderDistance, 0);
                modelingAmountCloudAmount = math.clamp(modelingAmountCloudAmount, 0,1);
                modelingAmountOverlayStartDistance = math.max(modelingAmountOverlayStartDistance, 0);
                modelingAmountOverlayCloudAmount = math.clamp(modelingAmountOverlayCloudAmount, 0,1);
                modelingAmountOverlayCloudAmount = math.clamp(modelingAmountOverlayCloudAmount, 0,1);
                modelingShapeBaseOctaves = math.clamp(modelingShapeBaseOctaves, 1,6);
                modelingShapeBaseGain = math.clamp(modelingShapeBaseGain, 0,1);
                modelingShapeBaseFreq = math.clamp(modelingShapeBaseFreq, 2,5);
                modelingShapeBaseScale = math.max(modelingShapeBaseScale, 0);
                motionBaseDirection = math.clamp(motionBaseDirection, 0,360);
                motionBaseSpeed  = math.clamp(motionBaseSpeed, 0,5);
                motionBaseDirectionRandomRange  = math.clamp(motionBaseDirectionRandomRange, 0,90);
                motionBaseDirectionRandomFreq  = math.max(motionBaseDirectionRandomFreq, 2);
                
                renderCoarseSteps = math.clamp(renderCoarseSteps, 0,128);
                renderDetailSteps = math.clamp(renderDetailSteps, 0,256);
                renderBlueNoise = math.clamp(renderBlueNoise, 0,1);
                renderMipmapDistance = math.clamp(renderMipmapDistance, 0,new Vector2(10000, 20000));
                modelingPositionPlanetRadius = math.max(modelingPositionPlanetRadius, 0);
                modelingPositionCloudHeight = math.max(modelingPositionCloudHeight, 0);
                modelingPositionCloudThickness = math.clamp(modelingPositionCloudThickness, 100,8000);
                modelingShapeDetailScale = math.max(modelingShapeDetailScale, 0);
                motionDetailDirection = math.clamp(motionDetailDirection, new float2(-90,0),new float2(90,360));
                motionDetailSpeed = math.max(motionDetailSpeed, 0);
                motionDetailDirectionRandomRange = math.clamp(motionDetailDirectionRandomRange, 0,90);
                motionDetailDirectionRandomFreq = math.max(motionDetailDirectionRandomFreq, 2);
                lightingDensityInfluence = math.clamp(lightingDensityInfluence, 0,1);
                lightingHeightDensityInfluence = math.clamp(lightingHeightDensityInfluence, 0,1);
                lightingAmbientExposure = math.max(lightingAmbientExposure, 0);
                lightingAtmosphereVisibility = math.max(lightingAtmosphereVisibility, 0);
                lightingHgStrength = math.clamp(lightingHgStrength, 0,1);
                lightingHgEccentricityForward = math.clamp(lightingHgEccentricityForward, 0,0.99f);
                lightingHgEccentricityBackward = math.clamp(lightingHgEccentricityBackward, -0.99f,0.99f);
                lightingMaxLightingDistance = math.max(lightingMaxLightingDistance, 0);
                lightingShadingStrengthFalloff = math.clamp(lightingShadingStrengthFalloff, 0,1);
                lightingScatterMultiplier = math.max(lightingScatterMultiplier, 0);
                lightingScatterStrength = math.clamp(lightingScatterStrength, 0,1);
                
                shadowDistance = math.max(shadowDistance, 0);
            }

            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) return;
                if (!UseLerp)
                {
                    //未插值时
                    lightingExtinctionCoeffExecute =
                        lightingExtinctionCoeff.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                }
            }
            
        }
        
        [HideLabel]
        public Property property = new();

        public static bool UseLerp = false;
        
        public bool useAddCloudMaskToDepth;
        
        #endregion

        

        #region 安装属性
        
        private readonly int _Render_BlueNoiseArray_ID = Shader.PropertyToID("_Render_BlueNoiseArray");
        private readonly int _Render_BlueNoiseArrayIndices_ID = Shader.PropertyToID("_Render_BlueNoiseArrayIndices");
        private readonly int _Modeling_ShapeDetail_NoiseTexture3D_ID = Shader.PropertyToID("_Modeling_ShapeDetail_NoiseTexture3D");
        private readonly int _Lighting_CheapAmbient_ID = Shader.PropertyToID("_Lighting_CheapAmbient");
        private readonly int _Lighting_AmbientExposure_ID = Shader.PropertyToID("_Lighting_AmbientExposure");
        private readonly int _Render_BlueNoise_ID = Shader.PropertyToID("_Render_BlueNoise");
        private readonly int _Lighting_DensityInfluence_ID = Shader.PropertyToID("_Lighting_DensityInfluence");
        private readonly int _Lighting_ExtinctionCoeff_ID = Shader.PropertyToID("_Lighting_ExtinctionCoeff");
        private readonly int _Lighting_AtmosphereVisibility_ID = Shader.PropertyToID("_Lighting_AtmosphereVisibility");
        private readonly int _Lighting_HeightDensityInfluence_ID = Shader.PropertyToID("_Lighting_HeightDensityInfluence");
        private readonly int _Lighting_HGEccentricityBackward_ID = Shader.PropertyToID("_Lighting_HGEccentricityBackward");
        private readonly int _Lighting_HGEccentricityForward_ID = Shader.PropertyToID("_Lighting_HGEccentricityForward");
        private readonly int _Lighting_HGStrength_ID = Shader.PropertyToID("_Lighting_HGStrength");
        private readonly int _Modeling_Position_CloudHeight_ID = Shader.PropertyToID("_Modeling_Position_CloudHeight");
        private readonly int _Modeling_Position_CloudThickness_ID = Shader.PropertyToID("_Modeling_Position_CloudThickness");
        private readonly int _Lighting_MaxLightingDistance_ID = Shader.PropertyToID("_Lighting_MaxLightingDistance");
        private readonly int _Modeling_Position_PlanetRadius_ID = Shader.PropertyToID("_Modeling_Position_PlanetRadius");
        private readonly int _Render_CoarseSteps_ID = Shader.PropertyToID("_Render_CoarseSteps");
        private readonly int _Render_DetailSteps_ID = Shader.PropertyToID("_Render_DetailSteps");
        private readonly int _Lighting_ScatterMultiplier_ID = Shader.PropertyToID("_Lighting_ScatterMultiplier");
        private readonly int _Lighting_ScatterStrength_ID = Shader.PropertyToID("_Lighting_ScatterStrength");
        private readonly int _Lighting_ShadingStrengthFalloff_ID = Shader.PropertyToID("_Lighting_ShadingStrengthFalloff");
        private readonly int _ShadowPass = Shader.PropertyToID("_ShadowPass");
        private readonly int _Lighting_AlbedoColor_ID = Shader.PropertyToID("_Lighting_AlbedoColor");
        private readonly int _Modeling_ShapeDetail_Scale_ID = Shader.PropertyToID("_Modeling_ShapeDetail_Scale");
        private readonly int _Lighting_LightColorFilter_ID = Shader.PropertyToID("_Lighting_LightColorFilter");
        private readonly int _Render_MipmapDistance_ID = Shader.PropertyToID("_Render_MipmapDistance");
        private readonly int _RenderTextureDimensions = Shader.PropertyToID("_RenderTextureDimensions");
        private readonly int _MotionDetail_Position_ID = Shader.PropertyToID("_MotionDetail_Position");
        
        private void SetupStaticProperty_VolumeCloud()
        {
            Shader.SetGlobalFloat(_Render_CoarseSteps_ID, property.renderCoarseSteps);
            Shader.SetGlobalFloat(_Render_DetailSteps_ID, property.renderDetailSteps);
            Shader.SetGlobalTexture(_Render_BlueNoiseArray_ID, property.renderBlueNoiseArray);
            Shader.SetGlobalFloat(_Render_BlueNoise_ID, property.renderBlueNoise);
            Shader.SetGlobalVector(_Render_MipmapDistance_ID, property.renderMipmapDistance);
            Shader.SetGlobalFloat(_Modeling_Position_PlanetRadius_ID, property.modelingPositionPlanetRadius);
            Shader.SetGlobalTexture(_Modeling_ShapeDetail_NoiseTexture3D_ID, property.modelingShapeDetailNoiseTexture3D);
            Shader.SetGlobalFloat(_Lighting_CheapAmbient_ID, property.lightingCheapAmbient ? 1 : 0);
        }
        
        private void SetupDynamicProperty_VolumeCloud()
        {
            Shader.SetGlobalFloat(_Render_BlueNoiseArrayIndices_ID, Time.renderedFrameCount % 64);
            Shader.SetGlobalFloat(_Modeling_Position_CloudHeight_ID, property.modelingPositionCloudHeight);
            Shader.SetGlobalFloat(_Modeling_Position_CloudThickness_ID, property.modelingPositionCloudThickness);
            Shader.SetGlobalVector(_Modeling_ShapeDetail_Scale_ID, property.modelingShapeDetailScale);
            Shader.SetGlobalFloat(_Lighting_AmbientExposure_ID, property.lightingAmbientExposure);
            Shader.SetGlobalVector(_MotionDetail_Position_ID, _motionDetailPosition);
            Shader.SetGlobalVector(_Lighting_AlbedoColor_ID, property.lightingAlbedoColor);
            Shader.SetGlobalVector(_Lighting_LightColorFilter_ID, property.lightingLightColorFilter);
            Shader.SetGlobalFloat(_Lighting_ExtinctionCoeff_ID, property.lightingExtinctionCoeffExecute);
            Shader.SetGlobalFloat(_Lighting_DensityInfluence_ID, property.lightingDensityInfluence);
            Shader.SetGlobalFloat(_Lighting_HeightDensityInfluence_ID, property.lightingHeightDensityInfluence);
            Shader.SetGlobalFloat(_Lighting_AtmosphereVisibility_ID, GetAtmosphereVisibility()); //大气可见度
            Shader.SetGlobalFloat(_Lighting_HGStrength_ID, property.lightingHgStrength);
            Shader.SetGlobalFloat(_Lighting_HGEccentricityForward_ID, property.lightingHgEccentricityForward);
            Shader.SetGlobalFloat(_Lighting_HGEccentricityBackward_ID, property.lightingHgEccentricityBackward);
            Shader.SetGlobalFloat(_Lighting_MaxLightingDistance_ID, property.lightingMaxLightingDistance);
            Shader.SetGlobalFloat(_Lighting_ShadingStrengthFalloff_ID, property.lightingShadingStrengthFalloff);
            Shader.SetGlobalFloat(_Lighting_ScatterMultiplier_ID, property.lightingScatterMultiplier);
            Shader.SetGlobalFloat(_Lighting_ScatterStrength_ID, property.lightingScatterStrength);
        }

        
        #endregion

        

        #region 事件函数
        
        [HideInInspector] public bool update;

        private void OnEnable()
        {
#if UNITY_EDITOR

            if (property.volumeCloudMainShader == null)
                property.volumeCloudMainShader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/VolumeCloudMain_V1_1_20240604.shader");

            if (property.renderBlueNoiseArray == null)
                property.renderBlueNoiseArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>("Packages/com.worldsystem/Textures/Noise Textures/BlueNoise/_Render_BlueNoiseArray.asset");

#endif

            if (property.volumeCloudMainMaterial == null)
                property.volumeCloudMainMaterial = CoreUtils.CreateEngineMaterial(property.volumeCloudMainShader);
            
            
            OnEnable_CloudMap();
            OnEnable_VolumeCloudShadow();
            OnValidate();
        }
        
        private void OnDisable()
        {
            if (property.volumeCloudMainShader != null)
                Resources.UnloadAsset(property.volumeCloudMainShader);
            if (property.renderBlueNoiseArray != null)
                Resources.UnloadAsset(property.renderBlueNoiseArray);
            
            if (property.volumeCloudMainMaterial != null)
                CoreUtils.Destroy(property.volumeCloudMainMaterial);
            
            if (property.volumeCloudAddCloudMaskToDepthShader != null)
                Resources.UnloadAsset(property.volumeCloudAddCloudMaskToDepthShader);
            if (property.volumeCloudAddCloudMaskToDepthMaterial != null)
                CoreUtils.Destroy(property.volumeCloudAddCloudMaskToDepthMaterial);
            
            property.renderBlueNoiseArray = null;
            property.volumeCloudMainShader = null;
            property.volumeCloudMainMaterial = null;
            property.volumeCloudAddCloudMaskToDepthShader = null;
            property.volumeCloudAddCloudMaskToDepthMaterial = null;
            
            _temporaryRTAddCloudMaskToDepth?.Release();
            _temporaryRTAddCloudMaskToDepth = null;

            OnDisable_CloudMap();
            OnDisable_VolumeCloudShadow();
        }

        public void OnValidate()
        {
            property.LimitProperty();
            
            property.modelingShapeDetailNoiseTexture3D = LoadVolumeTexture(property.modelingShapeDetailType, property.modelingShapeDetailQuality, property.modelingShapeDetailNoiseTexture3D);
            property.modelingPositionPlanetRadius = GetRadiusFromCelestialBodySelection(property.modelingPositionRadiusPreset, property.modelingPositionPlanetRadius);
            
            SetupStaticProperty_VolumeCloud();
            
            OnValidate_CloudMap();
            OnValidate_VolumeCloudShadow();

            if (useAddCloudMaskToDepth)
            {
#if UNITY_EDITOR
            if (property.volumeCloudAddCloudMaskToDepthShader == null)
                property.volumeCloudAddCloudMaskToDepthShader =
                    AssetDatabase.LoadAssetAtPath<Shader>(
                        "Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/AddCloudMaskToDepth.shader");
#endif
            if (property.volumeCloudAddCloudMaskToDepthMaterial == null)
                property.volumeCloudAddCloudMaskToDepthMaterial =
                    CoreUtils.CreateEngineMaterial(property.volumeCloudAddCloudMaskToDepthShader);
            }
            else
            {
                if (property.volumeCloudAddCloudMaskToDepthShader != null)
                    Resources.UnloadAsset(property.volumeCloudAddCloudMaskToDepthShader);
                if (property.volumeCloudAddCloudMaskToDepthMaterial != null)
                    CoreUtils.Destroy(property.volumeCloudAddCloudMaskToDepthMaterial);
                
                property.volumeCloudAddCloudMaskToDepthShader = null;
                property.volumeCloudAddCloudMaskToDepthMaterial = null;
                _temporaryRTAddCloudMaskToDepth?.Release();
                _temporaryRTAddCloudMaskToDepth = null;
            }
            
            
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
            property.ExecuteProperty();
            UpdatePositions();
            SetupDynamicProperty_VolumeCloud();
            Update_CloudMap();
            Update_VolumeCloudShadow();
        }
        
        
        
        #region 描述体积云的动态

        private Vector2 _motionBasePosition = Vector2.zero;
        private Vector2 _detailRandomValue;
        private Vector3 _motionDetailPosition = Vector3.zero;
        private float _baseTimeAdd;
        private float _baseRandomValue;
        private float _detailTimeAdd;
        private float _previousTime;
        
        private void UpdatePositions()
        {
            if (!property.motionBaseUseDynamicCloud || WorldManager.Instance?.timeModule is null)
                return;

            float deltaTime = Time.time - _previousTime;
            
            if (WorldManager.Instance?.windZoneModule is not null)
            {
                float windSpeed = WorldManager.Instance.windZoneModule.property.WindSpeed * property.motionBaseWindSpeedCoeff;
                Vector3 windDir3 = WorldManager.Instance.windZoneModule.property.WindDirection;
                Vector2 windDir2 = new Vector2(windDir3.x, windDir3.z);

                if (property.motionBaseUseDirectionRandom)
                {
                    float period = WorldManager.Instance.timeModule.dayNightCycleDurationMinute * 60 /
                                   property.motionBaseDirectionRandomFreq;
                    _baseTimeAdd += deltaTime;
                    //当累加的时间大于周期时,更新随机数,并重置累加
                    if (_baseTimeAdd > period)
                    {
                        _baseRandomValue = Random.Range(0, property.motionBaseDirectionRandomRange);
                        _baseTimeAdd = 0;
                    }

                    //根据随机数在一个周期内旋转风方向
                    Vector3 cloudTexRandomVector = Quaternion.Euler(0,
                        (float)Math.Sin(_baseTimeAdd * 2 * Math.PI / period) * _baseRandomValue, 0) * windDir3;
                    //根据随机的风方向和速度计算最终的动态矢量
                    property.motionBaseDynamicVector = new Vector2(cloudTexRandomVector.x, cloudTexRandomVector.z) *
                                                         (property.motionBaseSpeed * windSpeed);
                }
                else
                {
                    property.motionBaseDynamicVector = windDir2 * (property.motionBaseSpeed * windSpeed);
                }

                //应用动态矢量到位置
                _motionBasePosition += property.motionBaseDynamicVector * (deltaTime * 0.1f);


                //注释见上,同理
                if (property.motionDetailUseRandomDirection)
                {
                    float period = WorldManager.Instance.timeModule.dayNightCycleDurationMinute * 60 /
                                   property.motionDetailDirectionRandomFreq;
                    _detailTimeAdd += deltaTime;
                    if (_detailTimeAdd > period)
                    {
                        _detailRandomValue = new Vector2(
                            Random.Range(-property.motionDetailDirectionRandomRange.x,
                                property.motionDetailDirectionRandomRange.x),
                            Random.Range(-property.motionDetailDirectionRandomRange.y,
                                property.motionDetailDirectionRandomRange.y));
                        _detailTimeAdd = 0;
                    }

                    var sin = (float)Math.Sin(_detailTimeAdd * 2 * Math.PI / period);
                    var baseTexRandomVector = Quaternion.Euler(sin * _detailRandomValue.x,
                        sin * _detailRandomValue.y, 0) * windDir3;
                    property.motionDetailDynamicVector =
                        baseTexRandomVector * (property.motionDetailSpeed * windSpeed);
                }
                else
                {
                    property.motionDetailDynamicVector = windDir3 * (property.motionDetailSpeed * windSpeed);
                }

                _motionDetailPosition += property.motionDetailDynamicVector * (deltaTime * 0.05f);
            }
            else
            {
#if UNITY_EDITOR
                var rotation = Quaternion.Euler(0, property.motionBaseDirection, 0) * Vector3.forward;
                property.motionBaseDynamicVector = new Vector2(rotation.x, rotation.z) * property.motionBaseSpeed;
#endif
                _motionBasePosition += property.motionBaseDynamicVector * (deltaTime * 0.1f);

#if UNITY_EDITOR
                property.motionDetailDynamicVector = Quaternion.Euler(property.motionDetailDirection.x,
                    property.motionDetailDirection.y,
                    0) * Vector3.forward * property.motionDetailSpeed;
#endif
                _motionDetailPosition += property.motionDetailDynamicVector * (deltaTime * 0.05f);
            }

            _previousTime = Time.time;
        }

        #endregion

        
        
        #endregion

        

        #region 渲染函数
        
        private RTHandle _temporaryRTAddCloudMaskToDepth;

        public void RenderVolumeCloud(CommandBuffer cmd, ref RenderingData renderingData, RTHandle activeRT)
        {
            //绘制体积云
            cmd.SetGlobalVector(_RenderTextureDimensions, new Vector4(
                1f / activeRT.GetScaledSize().x,
                1f / activeRT.GetScaledSize().y,
                activeRT.GetScaledSize().x,
                activeRT.GetScaledSize().y));
            cmd.SetGlobalFloat(_ShadowPass, 0);
            Blitter.BlitTexture(cmd, new Vector4(1f, 1f, 0f, 0f), property.volumeCloudMainMaterial, 0);
            
        }

        public void RenderAddCloudMaskToDepth(CommandBuffer cmd, ref RenderingData renderingData, RTHandle activeRT)
        {
            if (!useAddCloudMaskToDepth) return;
             RenderingUtils.ReAllocateIfNeeded(ref _temporaryRTAddCloudMaskToDepth,
                new RenderTextureDescriptor(renderingData.cameraData.renderer.cameraColorTargetHandle.rt.width,
                    renderingData.cameraData.renderer.cameraColorTargetHandle.rt.height, GraphicsFormat.R16_SFloat, 0), FilterMode.Point, TextureWrapMode.Clamp, name : "TemporaryRTAddCloudMaskToDepth");
            cmd.SetGlobalTexture("_CloudNoFixupTex",activeRT);
            cmd.SetRenderTarget(_temporaryRTAddCloudMaskToDepth);
            Blitter.BlitTexture(cmd,new Vector4(1,1,0,0),property.volumeCloudAddCloudMaskToDepthMaterial,0);
            cmd.SetGlobalTexture("_CameraDepthTextureAddCloudMask", _temporaryRTAddCloudMaskToDepth);
        }
        
        
        #endregion
        
        
    }


    /// <summary>
    /// 体积云阴影
    /// </summary>
    public partial class VolumeCloudOptimizeModule
    {

        #region 字段

        public partial class Property
        {
            
            [Title("阴影")] [FoldoutGroup("配置")] [LabelText("云阴影TAA着色器")] [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader cloudShadowsTemporalAAShader;
            
            [FoldoutGroup("配置")] [LabelText("云阴影TAA材质")] [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material cloudShadowsTemporalAAMaterial;
            
            [FoldoutGroup("配置")] [LabelText("屏幕阴影着色器")] [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader cloudShadowsScreenShadowShader;
            
            [FoldoutGroup("配置")] [LabelText("屏幕阴影材质")] [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material cloudShadowsScreenShadowMaterial;
            
            [FoldoutGroup("阴影")] [LabelText("开启阴影铸造")] [GUIColor(0.3f, 1f, 0.3f)]
            public bool shadowUseCastShadow;
            
            [FoldoutGroup("阴影")] [LabelText("阴影距离")] [MinValue(0)] [GUIColor(0.7f, 0.7f, 1f)] [ShowIf("shadowUseCastShadow")]
            public int shadowDistance = 10000;
            
            [FoldoutGroup("阴影")] [LabelText("阴影强度")] [GUIColor(1f, 0.7f, 1f)] [ShowIf("shadowUseCastShadow")]
            public AnimationCurve shadowStrength = new();
            
            [FoldoutGroup("阴影")] [LabelText("阴影分辨率")] [GUIColor(0.7f, 0.7f, 1f)] [ShowIf("shadowUseCastShadow")]
            public CloudShadowResolution shadowResolution = CloudShadowResolution.Medium;
            
            [FoldoutGroup("阴影")] [LabelText("使用阴影TAA")] [GUIColor(0.7f, 0.7f, 1f)] [ShowIf("shadowUseCastShadow")] 
            public bool shadowUseShadowTaa = true;
            
        }


        #endregion
        
        
        
        #region 安装属性
        
        private readonly int _CloudShadowOrthoParams = Shader.PropertyToID("_CloudShadowOrthoParams");
        private readonly int _ShadowmapResolution = Shader.PropertyToID("_ShadowmapResolution");
        private readonly int _CloudShadowDistance = Shader.PropertyToID("_CloudShadowDistance");
        private readonly int _CloudShadow_WorldToShadowMatrix = Shader.PropertyToID("_CloudShadow_WorldToShadowMatrix");
        private readonly int _ShadowCasterCameraForward = Shader.PropertyToID("_ShadowCasterCameraForward");
        private readonly int _ShadowCasterCameraPosition = Shader.PropertyToID("_ShadowCasterCameraPosition");
        private readonly int _ShadowCasterCameraUp = Shader.PropertyToID("_ShadowCasterCameraUp");
        private readonly int _ShadowCasterCameraRight = Shader.PropertyToID("_ShadowCasterCameraRight");
        private readonly int _CloudShadowStrength = Shader.PropertyToID("_CloudShadowStrength");
        
        private void SetupStaticProperty_Shadow()
        {
            //设置静态全局参数
            const float zFar = 60000f;
            Shader.SetGlobalVector(_CloudShadowOrthoParams, new Vector4(property.shadowDistance * 2, property.shadowDistance * 2, zFar, 0));
            Shader.SetGlobalVector(_ShadowmapResolution, new Vector4((int)property.shadowResolution, (int)property.shadowResolution, 1f / (int)property.shadowResolution, 1f / (int)property.shadowResolution));
            Shader.SetGlobalFloat(_CloudShadowDistance, property.shadowDistance);
        }
        
        private void SetupDynamicProperty_Shadow(CelestialBody celestialBodyShadowCast)
        {
            //计算全局参数
            const float zFar = 60000f;
            Vector3 sourcePosition = Vector3.zero;
            Vector3 shadowCasterCameraPosition =
                sourcePosition - celestialBodyShadowCast.transform.forward * (zFar * 0.5f);
            Vector3 min = shadowCasterCameraPosition -
                          property.shadowDistance * celestialBodyShadowCast.transform.right -
                          property.shadowDistance * celestialBodyShadowCast.transform.up;
            Vector3 max = shadowCasterCameraPosition +
                          celestialBodyShadowCast.transform.forward * zFar +
                          property.shadowDistance * celestialBodyShadowCast.transform.right +
                          property.shadowDistance * celestialBodyShadowCast.transform.up;
            float radius = (new Vector2(max.x, max.z) - new Vector2(min.x, min.z)).magnitude / 2f;
            float texelSize = radius / (0.25f * (int)property.shadowResolution);

            sourcePosition = Floor(Div(sourcePosition, texelSize));
            sourcePosition *= texelSize;

            shadowCasterCameraPosition = sourcePosition - celestialBodyShadowCast.transform.forward * (zFar * 0.5f);

            Matrix4x4 viewMatrix = HelpFunc.SetupViewMatrix(shadowCasterCameraPosition,
                celestialBodyShadowCast.transform.forward,
                zFar, celestialBodyShadowCast.transform.up);
            Matrix4x4 projectionMatrix = HelpFunc.SetupProjectionMatrix(property.shadowDistance, zFar);
            Matrix4x4 worldToShadowMatrix = HelpFunc.ConvertToWorldToShadowMatrix(projectionMatrix, viewMatrix);

            //设置动态全局参数
            Shader.SetGlobalMatrix(_CloudShadow_WorldToShadowMatrix, worldToShadowMatrix);
            Shader.SetGlobalVector(_ShadowCasterCameraPosition, shadowCasterCameraPosition);
            Shader.SetGlobalVector(_ShadowCasterCameraForward, celestialBodyShadowCast.transform.forward);
            Shader.SetGlobalVector(_ShadowCasterCameraUp, celestialBodyShadowCast.transform.up);
            Shader.SetGlobalVector(_ShadowCasterCameraRight, celestialBodyShadowCast.transform.right);
            
            Shader.SetGlobalFloat(_CloudShadowStrength, property.shadowStrength.Evaluate(celestialBodyShadowCast.property.executeCoeff));
        }

        #endregion


        
        #region 事件函数
        
        private CloudShadowResolution _cloudShadowResolutionCache;

        private void OnEnable_VolumeCloudShadow()
        {
            if (!property.shadowUseCastShadow) return;
#if UNITY_EDITOR

            if (property.cloudShadowsTemporalAAShader == null)
                property.cloudShadowsTemporalAAShader =
                    AssetDatabase.LoadAssetAtPath<Shader>(
                        "Packages/com.worldsystem//Shader/VolumeClouds_V1_1_20240604/CloudShadowsTemporalAA_V1_1_20240604.shader");
            if (property.cloudShadowsScreenShadowShader == null)
                property.cloudShadowsScreenShadowShader =
                    AssetDatabase.LoadAssetAtPath<Shader>(
                        "Packages/com.worldsystem//Shader/VolumeClouds_V1_1_20240604/CloudShadowsScreenShadows_V1_1_20240604.shader");
#endif

            if (property.cloudShadowsTemporalAAMaterial == null)
                property.cloudShadowsTemporalAAMaterial =
                    CoreUtils.CreateEngineMaterial(property.cloudShadowsTemporalAAShader);

            if (property.cloudShadowsScreenShadowMaterial == null)
                property.cloudShadowsScreenShadowMaterial =
                    CoreUtils.CreateEngineMaterial(property.cloudShadowsScreenShadowShader);
            
            _cloudShadowResolutionCache = property.shadowResolution;
        }
        
        private void OnDisable_VolumeCloudShadow()
        {
            if (property.cloudShadowsTemporalAAShader != null)
                Resources.UnloadAsset(property.cloudShadowsTemporalAAShader);
            if (property.cloudShadowsScreenShadowShader != null)
                Resources.UnloadAsset(property.cloudShadowsScreenShadowShader);
            
            if (property.cloudShadowsTemporalAAMaterial != null)
                CoreUtils.Destroy(property.cloudShadowsTemporalAAMaterial);
            if (property.cloudShadowsScreenShadowMaterial != null)
                CoreUtils.Destroy(property.cloudShadowsScreenShadowMaterial);

            _previousCloudShadowRT?.Release();
            _cloudShadowMapRT?.Release();
            _cloudShadowTaaRT?.Release();
            
            property.cloudShadowsTemporalAAShader = null;
            property.cloudShadowsScreenShadowShader = null;
            property.cloudShadowsTemporalAAMaterial = null;
            property.cloudShadowsScreenShadowMaterial = null;
            _previousCloudShadowRT = null;
            _cloudShadowMapRT = null;
            _cloudShadowTaaRT = null;
        }

        private void OnValidate_VolumeCloudShadow()
        {
            SetupStaticProperty_Shadow();

            if (!property.shadowUseCastShadow)
                OnDisable_VolumeCloudShadow();
            else
                OnEnable_VolumeCloudShadow();
        }

        private void Update_VolumeCloudShadow()
        {
            if (IsDisableVolumeCloudShadow(out var celestialBodyShadowCast)) return;
            
            SetupDynamicProperty_Shadow(celestialBodyShadowCast);
        }

        
        #endregion


        
        #region 渲染函数

        private RTHandle _cloudShadowMapRT;
        private RTHandle _previousCloudShadowRT;
        private RTHandle _cloudShadowTaaRT;
        private readonly int _CloudShadowmap = Shader.PropertyToID("_CloudShadowmap");
        private readonly int _CURRENT_TAA_CLOUD_SHADOW = Shader.PropertyToID("_CURRENT_TAA_CLOUD_SHADOW");
        private readonly int _PREVIOUS_TAA_CLOUD_SHADOW = Shader.PropertyToID("_PREVIOUS_TAA_CLOUD_SHADOW");
        
        private bool IsDisableVolumeCloudShadow(out CelestialBody celestialBodyShadowCast)
        {
            //获取进行阴影铸造的星体
            celestialBodyShadowCast = WorldManager.Instance?.celestialBodyManager?.GetShadowCastCelestialBody();
            if (celestialBodyShadowCast is null) return true;
            
            if (!isActiveAndEnabled || property.modelingAmountCloudAmount < 0.25f || !property.shadowUseCastShadow) 
                return true;
            
            return false;
        }
        
        public void RenderVolumeCloudShadow(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (IsDisableVolumeCloudShadow(out _)) return;
            
            cmd.SetGlobalFloat(_ShadowPass, 1);
            cmd.SetGlobalVector(_RenderTextureDimensions,
                new Vector4(1f / (int)property.shadowResolution, 1f / (int)property.shadowResolution,
                    (int)property.shadowResolution, (int)property.shadowResolution));
            
            //渲染体积云阴影贴图
            //初始化RT
            if (property.shadowResolution != _cloudShadowResolutionCache || _cloudShadowMapRT == null)
            {
                RenderTextureDescriptor rtDescriptor =
                    new RenderTextureDescriptor((int)property.shadowResolution, (int)property.shadowResolution,
                        GraphicsFormat.B10G11R11_UFloatPack32, 0);
                _cloudShadowMapRT?.Release();
                _cloudShadowMapRT = RTHandles.Alloc(rtDescriptor, name: "CloudShadowMapRT", filterMode: FilterMode.Bilinear);
                _cloudShadowResolutionCache = property.shadowResolution;
            }
            cmd.SetRenderTarget(_cloudShadowMapRT);
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), property.volumeCloudMainMaterial, 0);

            
            if (property.shadowUseShadowTaa)
            {
                //将 原始体积云阴影贴图 设置为全局参数
                cmd.SetGlobalTexture(_CURRENT_TAA_CLOUD_SHADOW, _cloudShadowMapRT);
                //在没有进行新的复制之前拿到的上一帧的TAA结果
                if (_previousCloudShadowRT == null)
                {
                    _previousCloudShadowRT = RTHandles.Alloc(_cloudShadowMapRT.rt.descriptor, name: "PreviousCloudShadowRT", filterMode: FilterMode.Bilinear);
                    cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_SHADOW, _cloudShadowMapRT);
                }
                else
                {
                    cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_SHADOW, _previousCloudShadowRT);
                }
                
                _cloudShadowTaaRT ??= RTHandles.Alloc(_cloudShadowMapRT.rt.descriptor, name: "CloudShadowTaaRT", filterMode: FilterMode.Bilinear);
                //渲染体积云阴影贴图TAA
                cmd.SetRenderTarget(_cloudShadowTaaRT);
                Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), property.cloudShadowsTemporalAAMaterial, 0);
                //将这一帧的TAA结果赋值给 cloudShadowTaaTexture 
                cmd.CopyTexture(_cloudShadowTaaRT, _previousCloudShadowRT);
                //将 TAA之后的体积云阴影贴图 设置为全局参数
                cmd.SetGlobalTexture(_CloudShadowmap, _cloudShadowTaaRT);
            }
            else
            {
                _previousCloudShadowRT?.Release();
                _previousCloudShadowRT = null;
                _cloudShadowTaaRT?.Release();
                _cloudShadowTaaRT = null;
                cmd.SetGlobalTexture(_CloudShadowmap, _cloudShadowMapRT);
            }

            
            //渲染屏幕空间云shadow
            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), property.cloudShadowsScreenShadowMaterial, 0);
        }

        
        #endregion
        
    }

    
    
}