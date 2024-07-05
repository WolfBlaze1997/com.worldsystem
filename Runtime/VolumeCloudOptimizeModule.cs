﻿using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace WorldSystem.Runtime
{

    public partial class VolumeCloudOptimizeModule
    {
        
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
        /// 加载体积纹理使用的函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quality"></param>
        /// <param name="currentTexture"></param>
        /// <returns></returns>
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
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture3D>(loadTarget);
#else
            return currentTexture;
#endif
        }


        /// <summary>
        /// 一种辅助方法，我们将每个天体的半径编码为KM。
        /// </summary>
        /// <param name="celestialBodySelection"></param>
        /// <param name="currentVal"></param>
        /// <returns></returns>
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
            float visibility = WorldManager.Instance?.atmosphereModule?.property?.end ?? 20000;

            if (property._Lighting_UseAtmosphereVisibilityOverlay)
            {
                visibility = property._Lighting_AtmosphereVisibility;
            }

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
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
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
            pos = SetupGizmosColorAndPosition(property._MotionBase_Speed);
            Vector3 destination =
                new Vector3(property._MotionBase_DynamicVector.x, 0, property._MotionBase_DynamicVector.y).normalized
                * (float)((1 - Math.Pow(Math.E, -property._MotionBase_Speed)) * 2);
            ForGizmo(pos, destination);

            _ = SetupGizmosColorAndPosition(property._MotionDetail_Speed);
            Vector3 destination01 = property._MotionDetail_DynamicVector.normalized *
                                    (float)((1 - Math.Pow(Math.E, -property._MotionDetail_Speed)) * 2);
            ForGizmo(pos, destination01);

            Gizmos.color = Cache;
        }

        private Vector3 SetupGizmosColorAndPosition(float Speed)
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
    }

    
    /// <summary>
    /// 基础云纹理
    /// </summary>
    public partial class VolumeCloudOptimizeModule
    {
        #region 字段
        
        public partial class Property
        {
            [Title("云图")] 
            [FoldoutGroup("配置")] [LabelText("云图着色器")] 
            [ReadOnly] [PropertyOrder(-20)] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader VolumeCloud_BaseTex_Shader;

            [FoldoutGroup("配置")] [LabelText("云图材质")] 
            [ReadOnly] [PropertyOrder(-20)] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material VolumeCloud_BaseTex_Material;

            [FoldoutGroup("渲染")] [LabelText("最大渲染距离(m)")] 
            [GUIColor(0.7f, 0.7f, 1.0f)] [PropertyOrder(-10)] 
            public float _Render_MaxRenderDistance = 20000f;

            [FoldoutGroup("建模")]
            [FoldoutGroup("建模/云量")] [LabelText("云量")][PropertyRange(0, 1)]
            [GUIColor(1f, 0.7f, 0.7f)] [PropertyOrder(-10)]
            public float _Modeling_Amount_CloudAmount = 0.6f;

            [FoldoutGroup("建模/云量")] [LabelText("覆盖远程云量")] 
            [GUIColor(0.7f, 0.7f, 1.0f)]
            public bool _Modeling_Amount_UseFarOverlay = false;

            [FoldoutGroup("建模/云量")] [LabelText("    开始距离(m)")]
            [GUIColor(1f, 0.7f, 0.7f)]
            [ShowIf("_Modeling_Amount_UseFarOverlay")]
            public float _Modeling_Amount_OverlayStartDistance = 20000f;

            [FoldoutGroup("建模/云量")] [LabelText("    云量")] [PropertyRange(0, 1)]
            [GUIColor(1f, 0.7f, 0.7f)]
            [ShowIf("_Modeling_Amount_UseFarOverlay")]
            public float _Modeling_Amount_OverlayCloudAmount = 0.8f;

            [Title("基础(云图)")] 
            [FoldoutGroup("建模/形状")] [LabelText("八度音程")] [PropertyRange(1, 6)] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public int _Modeling_ShapeBase_Octaves = 3;

            [FoldoutGroup("建模/形状")] [LabelText("增益")] [PropertyRange(0, 1)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Modeling_ShapeBase_Gain = 0.5f;

            [FoldoutGroup("建模/形状")] [LabelText("频率")] [PropertyRange(2, 5)] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public float _Modeling_ShapeBase_Freq = 2f;

            [FoldoutGroup("建模/形状")] [LabelText("比例")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Modeling_ShapeBase_Scale = 5f;

            [Title("基础(云图)")] 
            [FoldoutGroup("运动")] [LabelText("动态矢量")] [PropertyOrder(-1)] 
            [GUIColor(0.7f, 0.7f, 0.7f)]
            public Vector2 _MotionBase_DynamicVector = Vector2.zero;

            [FoldoutGroup("运动")] [LabelText("方向")]
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule == null")]
            public float _MotionBase_Direction;

            [FoldoutGroup("运动")] [LabelText("速度")] [PropertyRange(0, 5)]
            [GUIColor(1f, 0.7f, 0.7f)] [PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null")]
            public float _MotionBase_Speed = 0.25f;

            [FoldoutGroup("运动")] [LabelText("使用方向随机")]
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null")]
            public bool _MotionBase_UseDirectionRandom = true;

            [FoldoutGroup("运动")] [LabelText("    随机范围")]
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && _MotionBase_UseDirectionRandom")]
            public float _MotionBase_DirectionRandomRange = 60;

            [FoldoutGroup("运动")] [LabelText("    随机频率")]
            [GUIColor(0.7f, 0.7f, 1f)][PropertyOrder(-1)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && _MotionBase_UseDirectionRandom")]
            public float _MotionBase_DirectionRandomFreq = 12;
            
        }
        
        
        #endregion


        #region 安装属性

        private void SetupStaticProperty_CloudBaseTex()
        {
            Shader.SetGlobalFloat(_Render_MaxRenderDistance_ID, property._Render_MaxRenderDistance);
            Shader.SetGlobalFloat(_Modeling_Amount_UseFarOverlay_ID, property._Modeling_Amount_UseFarOverlay ? 1 : 0);
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Octaves_ID, property._Modeling_ShapeBase_Octaves);
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Freq_ID, property._Modeling_ShapeBase_Freq);
        }
        private readonly int _Render_MaxRenderDistance_ID = Shader.PropertyToID("_Render_MaxRenderDistance");
        private readonly int _Modeling_Amount_UseFarOverlay_ID = Shader.PropertyToID("_Modeling_Amount_UseFarOverlay");
        private readonly int _Modeling_ShapeBase_Octaves_ID = Shader.PropertyToID("_Modeling_ShapeBase_Octaves");
        private readonly int _Modeling_ShapeBase_Freq_ID = Shader.PropertyToID("_Modeling_ShapeBase_Freq");

        
        private void SetupDynamicProperty_CloudBaseTex()
        {
            Shader.SetGlobalFloat(_Modeling_Amount_CloudAmount_ID, property._Modeling_Amount_CloudAmount);

            if (property._Modeling_Amount_UseFarOverlay)
            {
                Shader.SetGlobalFloat(_Modeling_Amount_OverlayStartDistance_ID, property._Modeling_Amount_OverlayStartDistance);
                Shader.SetGlobalFloat(_Modeling_Amount_OverlayCloudAmount_ID, property._Modeling_Amount_OverlayCloudAmount);
            }
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Gain_ID, property._Modeling_ShapeBase_Gain);
            Shader.SetGlobalFloat(_Modeling_ShapeBase_Scale_ID, property._Modeling_ShapeBase_Scale);
            Shader.SetGlobalVector(_MotionBase_Position_ID, _MotionBase_Position);
        }
        private readonly int _Modeling_Amount_CloudAmount_ID = Shader.PropertyToID("_Modeling_Amount_CloudAmount");
        private readonly int _Modeling_Amount_OverlayStartDistance_ID = Shader.PropertyToID("_Modeling_Amount_OverlayStartDistance");
        private readonly int _Modeling_Amount_OverlayCloudAmount_ID = Shader.PropertyToID("_Modeling_Amount_OverlayCloudAmount");
        private readonly int _Modeling_ShapeBase_Gain_ID = Shader.PropertyToID("_Modeling_ShapeBase_Gain");
        private readonly int _Modeling_ShapeBase_Scale_ID = Shader.PropertyToID("_Modeling_ShapeBase_Scale");
        private readonly int _MotionBase_Position_ID = Shader.PropertyToID("_MotionBase_Position");
        
        #endregion


        #region 事件函数

        private void OnEnable_CloudMap()
        {
#if UNITY_EDITOR
            if (property.VolumeCloud_BaseTex_Shader == null)
                property.VolumeCloud_BaseTex_Shader = AssetDatabase.LoadAssetAtPath<Shader>(
                    "Packages/com.worldsystem//Shader/VolumeClouds_V1_1_20240604/VolumeCloudBaseTex_V1_1_20240604.shader");
#endif
            if (property.VolumeCloud_BaseTex_Material == null)
                property.VolumeCloud_BaseTex_Material =
                    CoreUtils.CreateEngineMaterial(property.VolumeCloud_BaseTex_Shader);

            CloudBaseTexRT ??= RTHandles.Alloc(new RenderTextureDescriptor(512, 512, GraphicsFormat.B10G11R11_UFloatPack32,0),
                name: "CloudMapRT");
        }

        private void OnDisable_CloudMap()
        {
            if (property.VolumeCloud_BaseTex_Shader != null)
                Resources.UnloadAsset(property.VolumeCloud_BaseTex_Shader);
            if (property.VolumeCloud_BaseTex_Material != null)
                CoreUtils.Destroy(property.VolumeCloud_BaseTex_Material);
            CloudBaseTexRT?.Release();

            property.VolumeCloud_BaseTex_Shader = null;
            CloudBaseTexRT = null;
            property.VolumeCloud_BaseTex_Material = null;
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
        
        public void RenderCloudMap()
        {
            if (!isActiveAndEnabled || property._Modeling_Amount_CloudAmount < 0.25f) return;
            
            //渲染云图
            Graphics.Blit(null, CloudBaseTexRT, property.VolumeCloud_BaseTex_Material, 0);

            //将云图设置为全局参数
            Shader.SetGlobalTexture(CloudBaseTexRT_ID, CloudBaseTexRT);
        }
        private RTHandle CloudBaseTexRT;
        private readonly int CloudBaseTexRT_ID = Shader.PropertyToID("CloudBaseTexRT");
        
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
            [Title("体积云")]
            [FoldoutGroup("配置")] [LabelText("体积云着色器")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader VolumeCloud_Main_Shader;

            [FoldoutGroup("配置")] [LabelText("体积云材质")]
            [ReadOnly] [PropertyOrder(-20)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material VolumeCloud_Main_Material;
            
            [FoldoutGroup("渲染")] [LabelText("粗略步进")] 
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)] 
            public int _Render_CoarseSteps = 32;

            [FoldoutGroup("渲染")] [LabelText("细节步进")] 
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)]
            public int _Render_DetailSteps = 16;

            [FoldoutGroup("渲染")] [LabelText("Blue噪音纹理数组")]
            [GUIColor(0.7f, 0.7f, 1f)] [ReadOnly] [PropertyOrder(-10)]
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Texture2DArray _Render_BlueNoiseArray;

            [FoldoutGroup("渲染")] [LabelText("噪音")] [PropertyRange(0, 1)] 
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)] 
            public float _Render_BlueNoise = 1.0f;

            [FoldoutGroup("渲染")] [LabelText("Mipmap距离")] 
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-10)] 
            public Vector2 _Render_MipmapDistance = new Vector2(4000, 8000);
            
            [FoldoutGroup("建模/位置")] [LabelText("星球半径预设")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public CelestialBodySelection _Modeling_Position_RadiusPreset;

            [FoldoutGroup("建模/位置")] [LabelText("    星球半径(km)")] [MinValue(0)]
            [GUIColor(0.7f, 0.7f, 1f)]
            [EnableIf("@_Modeling_Position_RadiusPreset == CelestialBodySelection.Custom")]
            public int _Modeling_Position_PlanetRadius = 6378;

            [FoldoutGroup("建模/位置")] [LabelText("云层海拔(m)")] [MinValue(0)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Modeling_Position_CloudHeight = 600f;

            [FoldoutGroup("建模/位置")] [LabelText("云层厚度(m)")] [PropertyRange(100, 8000)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Modeling_Position_CloudThickness = 4000f;

            [Title("细节")] [FoldoutGroup("建模/形状")] [LabelText("3D噪音")] 
            [GUIColor(0.7f, 0.7f, 0.7f)] [ReadOnly]
            public Texture3D _Modeling_ShapeDetail_NoiseTexture3D = null;

            [FoldoutGroup("建模/形状")] [LabelText("类型")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public Noise3DTextureId _Modeling_ShapeDetail_Type = Noise3DTextureId.Perlin;

            [FoldoutGroup("建模/形状")] [LabelText("质量")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public TextureQuality _Modeling_ShapeDetail_Quality = TextureQuality.High;

            [FoldoutGroup("建模/形状")] [LabelText("比例")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public Vector3 _Modeling_ShapeDetail_Scale = new(5, 5, 5);

            [FoldoutGroup("运动")] [LabelText("使用动态体积云")] 
            [GUIColor(0.7f, 0.7f, 1f)] [PropertyOrder(-2)]
            public bool _MotionBase_UseDynamicCloud = true;

            [Title("细节")] 
            [FoldoutGroup("运动")] [LabelText("动态矢量")] 
            [GUIColor(0.7f, 0.7f, 0.7f)]
            public Vector3 _MotionDetail_DynamicVector;

            [FoldoutGroup("运动")] [LabelText("方向")]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule == null")]
            public Vector2 _MotionDetail_Direction;

            [FoldoutGroup("运动")] [LabelText("速度")]
            [GUIColor(1f, 0.7f, 0.7f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null")]
            public float _MotionDetail_Speed = 1;

            [FoldoutGroup("运动")] [LabelText("使用随机")]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null")]
            public bool _MotionDetail_UseRandomDirection = true;

            [FoldoutGroup("运动")] [LabelText("    随机范围")]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && _MotionDetail_UseRandomDirection")]
            public Vector2 _MotionDetail_DirectionRandomRange = new Vector2(40, 20);

            [FoldoutGroup("运动")] [LabelText("    随机频率")] [MinValue(2)]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("@WorldManager.Instance?.timeModule != null && WorldManager.Instance?.windZoneModule != null && _MotionDetail_UseRandomDirection")]
            public float _MotionDetail_DirectionRandomFreq = 12;

            [Title("基础")] 
            [FoldoutGroup("光照")] [LabelText("反照率颜色")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color _Lighting_AlbedoColor = new Color(1.0f, 0.964f, 0.92f);

            [FoldoutGroup("光照")] [LabelText("光照颜色过滤")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public Color _Lighting_LightColorFilter = Color.white;

            [Title("密度")] 
            [FoldoutGroup("光照")] [LabelText("消光系数")]
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_ExtinctionCoeff = 10f;

            [FoldoutGroup("光照")] [LabelText("密度影响")] [PropertyRange(0, 1)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_DensityInfluence = 1.0f;

            [FoldoutGroup("光照")] [LabelText("海拔密度影响")] [PropertyRange(0, 1)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_HeightDensityInfluence = 1.0f;

            [Title("环境")] 
            [FoldoutGroup("光照")] [LabelText("廉价环境光照")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public bool _Lighting_CheapAmbient = true;

            [FoldoutGroup("光照")] [LabelText("环境照明强度")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_AmbientExposure = 1.0f;

            [FoldoutGroup("光照")] [LabelText("覆盖大气可见度")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            public bool _Lighting_UseAtmosphereVisibilityOverlay;

            [FoldoutGroup("光照")] [LabelText("    可见度")]
            [GUIColor(1f, 0.7f, 0.7f)]
            [ShowIf("_Lighting_UseAtmosphereVisibilityOverlay")]
            public float _Lighting_AtmosphereVisibility = 30000f;

            [Title("照明")] 
            [FoldoutGroup("光照")] [LabelText("HG强度")] [PropertyRange(0, 1)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_HGStrength = 1.0f;

            [FoldoutGroup("光照")] [LabelText("HG偏心度向前")] [PropertyRange(0f, 0.99f)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_HGEccentricityForward = 0.6f;

            [FoldoutGroup("光照")] [LabelText("HG偏心度向后")] [PropertyRange(-0.99f, 0.99f)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_HGEccentricityBackward = -0.2f;

            [FoldoutGroup("光照")] [LabelText("最大光照距离")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public int _Lighting_MaxLightingDistance = 2000;

            [FoldoutGroup("光照")] [LabelText("着色强度衰减")] [PropertyRange(0, 1)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_ShadingStrengthFalloff = 0.2f;

            [FoldoutGroup("光照")] [LabelText("散射乘数")] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_ScatterMultiplier = 100;

            [FoldoutGroup("光照")] [LabelText("散射强度")] [PropertyRange(0, 1)] 
            [GUIColor(1f, 0.7f, 0.7f)]
            public float _Lighting_ScatterStrength = 1.0f;

            public void LimitProperty()
            {
                _Render_MaxRenderDistance = math.max(_Render_MaxRenderDistance, 0);
                _Modeling_Amount_CloudAmount = math.clamp(_Modeling_Amount_CloudAmount, 0,1);
                _Modeling_Amount_OverlayStartDistance = math.max(_Modeling_Amount_OverlayStartDistance, 0);
                _Modeling_Amount_OverlayCloudAmount = math.clamp(_Modeling_Amount_OverlayCloudAmount, 0,1);
                _Modeling_Amount_OverlayCloudAmount = math.clamp(_Modeling_Amount_OverlayCloudAmount, 0,1);
                _Modeling_ShapeBase_Octaves = math.clamp(_Modeling_ShapeBase_Octaves, 1,6);
                _Modeling_ShapeBase_Gain = math.clamp(_Modeling_ShapeBase_Gain, 0,1);
                _Modeling_ShapeBase_Freq = math.clamp(_Modeling_ShapeBase_Freq, 2,5);
                _Modeling_ShapeBase_Scale = math.max(_Modeling_ShapeBase_Scale, 0);
                _MotionBase_Direction = math.clamp(_MotionBase_Direction, 0,360);
                _MotionBase_Speed  = math.clamp(_MotionBase_Speed, 0,5);
                _MotionBase_DirectionRandomRange  = math.clamp(_MotionBase_DirectionRandomRange, 0,90);
                _MotionBase_DirectionRandomFreq  = math.max(_MotionBase_DirectionRandomFreq, 2);
                
                _Render_CoarseSteps = math.clamp(_Render_CoarseSteps, 0,128);
                _Render_DetailSteps = math.clamp(_Render_DetailSteps, 0,256);
                _Render_BlueNoise = math.clamp(_Render_BlueNoise, 0,1);
                _Render_MipmapDistance = math.clamp(_Render_MipmapDistance, 0,new Vector2(10000, 20000));
                // _Render_TemporalAAFactor = math.clamp(_Render_TemporalAAFactor, 0,1);
                _Modeling_Position_PlanetRadius = math.max(_Modeling_Position_PlanetRadius, 0);
                _Modeling_Position_CloudHeight = math.max(_Modeling_Position_CloudHeight, 0);
                _Modeling_Position_CloudThickness = math.clamp(_Modeling_Position_CloudThickness, 100,8000);
                _Modeling_ShapeDetail_Scale = math.max(_Modeling_ShapeDetail_Scale, 0);
                _MotionDetail_Direction = math.clamp(_MotionDetail_Direction, new float2(-90,0),new float2(90,360));
                _MotionDetail_Speed = math.max(_MotionDetail_Speed, 0);
                _MotionDetail_DirectionRandomRange = math.clamp(_MotionDetail_DirectionRandomRange, 0,90);
                _MotionDetail_DirectionRandomFreq = math.max(_MotionDetail_DirectionRandomFreq, 2);
                _Lighting_ExtinctionCoeff = math.max(_Lighting_ExtinctionCoeff, 0);
                _Lighting_DensityInfluence = math.clamp(_Lighting_DensityInfluence, 0,1);
                _Lighting_HeightDensityInfluence = math.clamp(_Lighting_HeightDensityInfluence, 0,1);
                _Lighting_AmbientExposure = math.max(_Lighting_AmbientExposure, 0);
                _Lighting_AtmosphereVisibility = math.max(_Lighting_AtmosphereVisibility, 0);
                _Lighting_HGStrength = math.clamp(_Lighting_HGStrength, 0,1);
                _Lighting_HGEccentricityForward = math.clamp(_Lighting_HGEccentricityForward, 0,0.99f);
                _Lighting_HGEccentricityBackward = math.clamp(_Lighting_HGEccentricityBackward, -0.99f,0.99f);
                _Lighting_MaxLightingDistance = math.max(_Lighting_MaxLightingDistance, 0);
                _Lighting_ShadingStrengthFalloff = math.clamp(_Lighting_ShadingStrengthFalloff, 0,1);
                _Lighting_ScatterMultiplier = math.max(_Lighting_ScatterMultiplier, 0);
                _Lighting_ScatterStrength = math.clamp(_Lighting_ScatterStrength, 0,1);
                
                _Shadow_Distance = math.max(_Shadow_Distance, 0);
            }
            
        }
        
        [HideLabel]
        public Property property = new();
        #endregion


        #region 安装属性

        private void SetupStaticProperty_VolumeCloud()
        {
            Shader.SetGlobalFloat(_Render_CoarseSteps_ID, property._Render_CoarseSteps);
            Shader.SetGlobalFloat(_Render_DetailSteps_ID, property._Render_DetailSteps);
            Shader.SetGlobalTexture(_Render_BlueNoiseArray_ID, property._Render_BlueNoiseArray);
            Shader.SetGlobalFloat(_Render_BlueNoise_ID, property._Render_BlueNoise);
            Shader.SetGlobalVector(_Render_MipmapDistance_ID, property._Render_MipmapDistance);
            Shader.SetGlobalFloat(_Modeling_Position_PlanetRadius_ID, property._Modeling_Position_PlanetRadius);
            Shader.SetGlobalTexture(_Modeling_ShapeDetail_NoiseTexture3D_ID, property._Modeling_ShapeDetail_NoiseTexture3D);
            Shader.SetGlobalFloat(_Lighting_CheapAmbient_ID, property._Lighting_CheapAmbient ? 1 : 0);

            
        }
        private void SetupDynamicProperty_VolumeCloud()
        {
            Shader.SetGlobalFloat(_Render_BlueNoiseArrayIndices_ID, Time.renderedFrameCount % 64);
            Shader.SetGlobalFloat(_Modeling_Position_CloudHeight_ID, property._Modeling_Position_CloudHeight);
            Shader.SetGlobalFloat(_Modeling_Position_CloudThickness_ID, property._Modeling_Position_CloudThickness);
            Shader.SetGlobalVector(_Modeling_ShapeDetail_Scale_ID, property._Modeling_ShapeDetail_Scale);
            Shader.SetGlobalFloat(_Lighting_AmbientExposure_ID, property._Lighting_AmbientExposure);
            Shader.SetGlobalVector(_MotionDetail_Position_ID, _MotionDetail_Position);
            Shader.SetGlobalVector(_Lighting_AlbedoColor_ID, property._Lighting_AlbedoColor);
            Shader.SetGlobalVector(_Lighting_LightColorFilter_ID, property._Lighting_LightColorFilter);
            Shader.SetGlobalFloat(_Lighting_ExtinctionCoeff_ID, property._Lighting_ExtinctionCoeff);
            Shader.SetGlobalFloat(_Lighting_DensityInfluence_ID, property._Lighting_DensityInfluence);
            Shader.SetGlobalFloat(_Lighting_HeightDensityInfluence_ID, property._Lighting_HeightDensityInfluence);
            Shader.SetGlobalFloat(_Lighting_AtmosphereVisibility_ID, GetAtmosphereVisibility()); //大气可见度
            Shader.SetGlobalFloat(_Lighting_HGStrength_ID, property._Lighting_HGStrength);
            Shader.SetGlobalFloat(_Lighting_HGEccentricityForward_ID, property._Lighting_HGEccentricityForward);
            Shader.SetGlobalFloat(_Lighting_HGEccentricityBackward_ID, property._Lighting_HGEccentricityBackward);
            Shader.SetGlobalFloat(_Lighting_MaxLightingDistance_ID, property._Lighting_MaxLightingDistance);
            Shader.SetGlobalFloat(_Lighting_ShadingStrengthFalloff_ID, property._Lighting_ShadingStrengthFalloff);
            Shader.SetGlobalFloat(_Lighting_ScatterMultiplier_ID, property._Lighting_ScatterMultiplier);
            Shader.SetGlobalFloat(_Lighting_ScatterStrength_ID, property._Lighting_ScatterStrength);
        }
        
        private readonly int _ScreenTexture = Shader.PropertyToID("_ScreenTexture");
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

        #endregion


        #region 事件函数

        private void OnEnable()
        {
#if UNITY_EDITOR

            if (property.VolumeCloud_Main_Shader == null)
                property.VolumeCloud_Main_Shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/VolumeClouds_V1_1_20240604/VolumeCloudMain_V1_1_20240604.shader");

            if (property._Render_BlueNoiseArray == null)
                property._Render_BlueNoiseArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>("Packages/com.worldsystem/Textures/Noise Textures/BlueNoise/_Render_BlueNoiseArray.asset");

#endif

            if (property.VolumeCloud_Main_Material == null)
                property.VolumeCloud_Main_Material = CoreUtils.CreateEngineMaterial(property.VolumeCloud_Main_Shader);
            
            
            OnEnable_CloudMap();
            OnEnable_VolumeCloudShadow();
            OnValidate();
        }
        
        private void OnDisable()
        {
            if (property.VolumeCloud_Main_Shader != null)
                Resources.UnloadAsset(property.VolumeCloud_Main_Shader);
            if (property._Render_BlueNoiseArray != null)
                Resources.UnloadAsset(property._Render_BlueNoiseArray);
            
            if (property.VolumeCloud_Main_Material != null)
                CoreUtils.Destroy(property.VolumeCloud_Main_Material);
            
            
            property._Render_BlueNoiseArray = null;
            property.VolumeCloud_Main_Shader = null;
            property.VolumeCloud_Main_Material = null;
            

            OnDisable_CloudMap();
            OnDisable_VolumeCloudShadow();
        }

        public void OnValidate()
        {
            property.LimitProperty();
            
            property._Modeling_ShapeDetail_NoiseTexture3D = LoadVolumeTexture(property._Modeling_ShapeDetail_Type, property._Modeling_ShapeDetail_Quality, property._Modeling_ShapeDetail_NoiseTexture3D);
            property._Modeling_Position_PlanetRadius = GetRadiusFromCelestialBodySelection(property._Modeling_Position_RadiusPreset, property._Modeling_Position_PlanetRadius);
            
            SetupStaticProperty_VolumeCloud();
            
            OnValidate_CloudMap();
            OnValidate_VolumeCloudShadow();
        }

#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        [HideInInspector] public bool _Update;
        private void Update()
        {
            if (!_Update) return;
            
            UpdatePositions();
            SetupDynamicProperty_VolumeCloud();

            Update_CloudMap();
            Update_VolumeCloudShadow();
        }


        #region 描述体积云的动态

        private Vector2 _MotionBase_Position = Vector2.zero;
        private Vector3 _MotionDetail_Position = Vector3.zero;

        private float BaseTimeAdd;
        private float BaseRandomValue;

        private float DetailTimeAdd;
        private Vector2 DetailRandomValue;

        private float _previousTime;
        private void UpdatePositions()
        {
            if (!property._MotionBase_UseDynamicCloud || WorldManager.Instance?.timeModule is null)
                return;

            float deltaTime = Time.time - _previousTime;
            
            if (WorldManager.Instance?.windZoneModule is not null)
            {
                float windSpeed = WorldManager.Instance.windZoneModule.property.cloudWindData.speed;
                Vector3 windDir3 = WorldManager.Instance.windZoneModule.property.cloudWindData.direction;
                Vector2 windDir2 = new Vector2(windDir3.x, windDir3.z);

                if (property._MotionBase_UseDirectionRandom)
                {
                    float period = WorldManager.Instance.timeModule.dayNightCycleDurationMinute * 60 /
                                   property._MotionBase_DirectionRandomFreq;
                    BaseTimeAdd += deltaTime;
                    //当累加的时间大于周期时,更新随机数,并重置累加
                    if (BaseTimeAdd > period)
                    {
                        BaseRandomValue = Random.Range(0, property._MotionBase_DirectionRandomRange);
                        BaseTimeAdd = 0;
                    }

                    //根据随机数在一个周期内旋转风方向
                    Vector3 cloudTexRandomVector = Quaternion.Euler(0,
                        (float)Math.Sin(BaseTimeAdd * 2 * Math.PI / period) * BaseRandomValue, 0) * windDir3;
                    //根据随机的风方向和速度计算最终的动态矢量
                    property._MotionBase_DynamicVector = new Vector2(cloudTexRandomVector.x, cloudTexRandomVector.z) *
                                                         (property._MotionBase_Speed * windSpeed);
                }
                else
                {
                    property._MotionBase_DynamicVector = windDir2 * (property._MotionBase_Speed * windSpeed);
                }

                //应用动态矢量到位置
                _MotionBase_Position += property._MotionBase_DynamicVector * (deltaTime * 0.1f);


                //注释见上,同理
                if (property._MotionDetail_UseRandomDirection)
                {
                    float period = WorldManager.Instance.timeModule.dayNightCycleDurationMinute * 60 /
                                   property._MotionDetail_DirectionRandomFreq;
                    DetailTimeAdd += deltaTime;
                    if (DetailTimeAdd > period)
                    {
                        DetailRandomValue = new Vector2(
                            Random.Range(-property._MotionDetail_DirectionRandomRange.x,
                                property._MotionDetail_DirectionRandomRange.x),
                            Random.Range(-property._MotionDetail_DirectionRandomRange.y,
                                property._MotionDetail_DirectionRandomRange.y));
                        DetailTimeAdd = 0;
                    }

                    var sin = (float)Math.Sin(DetailTimeAdd * 2 * Math.PI / period);
                    var baseTexRandomVector = Quaternion.Euler(sin * DetailRandomValue.x,
                        sin * DetailRandomValue.y, 0) * windDir3;
                    property._MotionDetail_DynamicVector =
                        baseTexRandomVector * (property._MotionDetail_Speed * windSpeed);
                }
                else
                {
                    property._MotionDetail_DynamicVector = windDir3 * (property._MotionDetail_Speed * windSpeed);
                }

                _MotionDetail_Position += property._MotionDetail_DynamicVector * (deltaTime * 0.05f);
            }
            else
            {
#if UNITY_EDITOR
                var rotation = Quaternion.Euler(0, property._MotionBase_Direction, 0) * Vector3.forward;
                property._MotionBase_DynamicVector = new Vector2(rotation.x, rotation.z) * property._MotionBase_Speed;
#endif
                _MotionBase_Position += property._MotionBase_DynamicVector * (deltaTime * 0.1f);

#if UNITY_EDITOR
                property._MotionDetail_DynamicVector = Quaternion.Euler(property._MotionDetail_Direction.x,
                    property._MotionDetail_Direction.y,
                    0) * Vector3.forward * property._MotionDetail_Speed;
#endif
                _MotionDetail_Position += property._MotionDetail_DynamicVector * (deltaTime * 0.05f);
            }

            _previousTime = Time.time;
        }

        #endregion

        
        
        #endregion


        #region 渲染函数
        public void RenderVolumeCloud(CommandBuffer cmd, ref RenderingData renderingData, RTHandle activeRT)
        {
            //绘制体积云
            cmd.SetGlobalVector(_RenderTextureDimensions, new Vector4(
                1f / activeRT.GetScaledSize().x,
                1f / activeRT.GetScaledSize().y,
                activeRT.GetScaledSize().x,
                activeRT.GetScaledSize().y));
            cmd.SetGlobalFloat(_ShadowPass, 0);
                
            Blitter.BlitTexture(cmd, new Vector4(1f, 1f, 0f, 0f), property.VolumeCloud_Main_Material, 0);
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
            [Title("阴影")] 
            [FoldoutGroup("配置")] [LabelText("云阴影TAA着色器")] 
            [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader CloudShadows_TemporalAA_Shader;

            [FoldoutGroup("配置")] [LabelText("云阴影TAA材质")] 
            [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material CloudShadows_TemporalAA_Material;

            [FoldoutGroup("配置")] [LabelText("屏幕阴影着色器")] 
            [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Shader CloudShadows_ScreenShadow_Shader;

            [FoldoutGroup("配置")] [LabelText("屏幕阴影材质")]
            [ReadOnly] 
            [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            public Material CloudShadows_ScreenShadow_Material;

            // [FoldoutGroup("配置")] [LabelText("阴影到屏幕着色器")] 
            // [ReadOnly] 
            // [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            // public Shader CloudShadows_ToScreen_Shader;
            //
            // [FoldoutGroup("配置")] [LabelText("阴影到屏幕材质")] 
            // [ReadOnly] 
            // [ShowIf("@WorldManager.Instance?.volumeCloudOptimizeModule?.hideFlags == HideFlags.None")]
            // public Material CloudShadows_ToScreen_Material;

            [FoldoutGroup("阴影")] [LabelText("开启阴影铸造")] 
            [GUIColor(0.3f, 1f, 0.3f)]
            public bool _Shadow_UseCastShadow = true;

            [FoldoutGroup("阴影")] [LabelText("阴影距离")] [MinValue(0)]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("_Shadow_UseCastShadow")]
            public int _Shadow_Distance = 10000;

            [FoldoutGroup("阴影")] [LabelText("阴影强度")]
            [GUIColor(1f, 0.7f, 1f)]
            [ShowIf("_Shadow_UseCastShadow")]
            public AnimationCurve _Shadow_Strength = new();
            
            [FoldoutGroup("阴影")] [LabelText("阴影分辨率")]
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("_Shadow_UseCastShadow")]
            public CloudShadowResolution _Shadow_Resolution = CloudShadowResolution.Medium;

            [FoldoutGroup("阴影")] [LabelText("使用阴影TAA")] 
            [GUIColor(0.7f, 0.7f, 1f)]
            [ShowIf("_Shadow_UseCastShadow")] 
            public bool _Shadow_UseShadowTaa = true;
            
        }


        #endregion
        
        
        #region 安装属性
        
        private void SetupStaticProperty_Shadow()
        {
            //设置静态全局参数
            const float zFar = 60000f;
            Shader.SetGlobalVector(_CloudShadowOrthoParams, new Vector4(property._Shadow_Distance * 2, property._Shadow_Distance * 2, zFar, 0));
            Shader.SetGlobalVector(_ShadowmapResolution, new Vector4((int)property._Shadow_Resolution, (int)property._Shadow_Resolution, 1f / (int)property._Shadow_Resolution, 1f / (int)property._Shadow_Resolution));
            Shader.SetGlobalFloat(_CloudShadowDistance, property._Shadow_Distance);
        }

        private readonly int _CloudShadowOrthoParams = Shader.PropertyToID("_CloudShadowOrthoParams");
        private readonly int _ShadowmapResolution = Shader.PropertyToID("_ShadowmapResolution");
        private readonly int _CloudShadowDistance = Shader.PropertyToID("_CloudShadowDistance");

        private void SetupDynamicProperty_Shadow(CelestialBody celestialBodyShadowCast)
        {
            //计算全局参数
            const float zFar = 60000f;
            Vector3 sourcePosition = Vector3.zero;
            Vector3 shadowCasterCameraPosition =
                sourcePosition - celestialBodyShadowCast.transform.forward * (zFar * 0.5f);
            Vector3 min = shadowCasterCameraPosition -
                          property._Shadow_Distance * celestialBodyShadowCast.transform.right -
                          property._Shadow_Distance * celestialBodyShadowCast.transform.up;
            Vector3 max = shadowCasterCameraPosition +
                          celestialBodyShadowCast.transform.forward * zFar +
                          property._Shadow_Distance * celestialBodyShadowCast.transform.right +
                          property._Shadow_Distance * celestialBodyShadowCast.transform.up;
            float radius = (new Vector2(max.x, max.z) - new Vector2(min.x, min.z)).magnitude / 2f;
            float texelSize = radius / (0.25f * (int)property._Shadow_Resolution);

            sourcePosition = Floor(Div(sourcePosition, texelSize));
            sourcePosition *= texelSize;

            shadowCasterCameraPosition = sourcePosition - celestialBodyShadowCast.transform.forward * (zFar * 0.5f);

            Matrix4x4 viewMatrix = HelpFunc.SetupViewMatrix(shadowCasterCameraPosition,
                celestialBodyShadowCast.transform.forward,
                zFar, celestialBodyShadowCast.transform.up);
            Matrix4x4 projectionMatrix = HelpFunc.SetupProjectionMatrix(property._Shadow_Distance, zFar);
            Matrix4x4 worldToShadowMatrix = HelpFunc.ConvertToWorldToShadowMatrix(projectionMatrix, viewMatrix);

            //设置动态全局参数
            Shader.SetGlobalMatrix(_CloudShadow_WorldToShadowMatrix, worldToShadowMatrix);
            Shader.SetGlobalVector(_ShadowCasterCameraPosition, shadowCasterCameraPosition);
            Shader.SetGlobalVector(_ShadowCasterCameraForward, celestialBodyShadowCast.transform.forward);
            Shader.SetGlobalVector(_ShadowCasterCameraUp, celestialBodyShadowCast.transform.up);
            Shader.SetGlobalVector(_ShadowCasterCameraRight, celestialBodyShadowCast.transform.right);
            
            Shader.SetGlobalFloat(_CloudShadowStrength, property._Shadow_Strength.Evaluate(celestialBodyShadowCast.curveTime));
        }

        private readonly int _CloudShadow_WorldToShadowMatrix = Shader.PropertyToID("_CloudShadow_WorldToShadowMatrix");
        private readonly int _ShadowCasterCameraForward = Shader.PropertyToID("_ShadowCasterCameraForward");
        private readonly int _ShadowCasterCameraPosition = Shader.PropertyToID("_ShadowCasterCameraPosition");
        private readonly int _ShadowCasterCameraUp = Shader.PropertyToID("_ShadowCasterCameraUp");
        private readonly int _ShadowCasterCameraRight = Shader.PropertyToID("_ShadowCasterCameraRight");
        private readonly int _CloudShadowStrength = Shader.PropertyToID("_CloudShadowStrength");

        #endregion


        #region 事件函数

        private void OnEnable_VolumeCloudShadow()
        {
            if (!property._Shadow_UseCastShadow) return;
#if UNITY_EDITOR

            if (property.CloudShadows_TemporalAA_Shader == null)
                property.CloudShadows_TemporalAA_Shader =
                    AssetDatabase.LoadAssetAtPath<Shader>(
                        "Packages/com.worldsystem//Shader/VolumeClouds_V1_1_20240604/CloudShadowsTemporalAA_V1_1_20240604.shader");
            if (property.CloudShadows_ScreenShadow_Shader == null)
                property.CloudShadows_ScreenShadow_Shader =
                    AssetDatabase.LoadAssetAtPath<Shader>(
                        "Packages/com.worldsystem//Shader/VolumeClouds_V1_1_20240604/CloudShadowsScreenShadows_V1_1_20240604.shader");
#endif

            if (property.CloudShadows_TemporalAA_Material == null)
                property.CloudShadows_TemporalAA_Material =
                    CoreUtils.CreateEngineMaterial(property.CloudShadows_TemporalAA_Shader);

            if (property.CloudShadows_ScreenShadow_Material == null)
                property.CloudShadows_ScreenShadow_Material =
                    CoreUtils.CreateEngineMaterial(property.CloudShadows_ScreenShadow_Shader);
            
            _cloudShadowResolutionCache = property._Shadow_Resolution;
        }
        private CloudShadowResolution _cloudShadowResolutionCache;
        
        private void OnDisable_VolumeCloudShadow()
        {
            if (property.CloudShadows_TemporalAA_Shader != null)
                Resources.UnloadAsset(property.CloudShadows_TemporalAA_Shader);
            if (property.CloudShadows_ScreenShadow_Shader != null)
                Resources.UnloadAsset(property.CloudShadows_ScreenShadow_Shader);
            
            if (property.CloudShadows_TemporalAA_Material != null)
                CoreUtils.Destroy(property.CloudShadows_TemporalAA_Material);
            if (property.CloudShadows_ScreenShadow_Material != null)
                CoreUtils.Destroy(property.CloudShadows_ScreenShadow_Material);

            PreviousCloudShadowRT?.Release();
            cloudShadowMapRT?.Release();
            cloudShadowTaaRT?.Release();
            
            property.CloudShadows_TemporalAA_Shader = null;
            property.CloudShadows_ScreenShadow_Shader = null;
            property.CloudShadows_TemporalAA_Material = null;
            property.CloudShadows_ScreenShadow_Material = null;
            PreviousCloudShadowRT = null;
            cloudShadowMapRT = null;
            cloudShadowTaaRT = null;
        }

        private void OnValidate_VolumeCloudShadow()
        {
            SetupStaticProperty_Shadow();

            if (!property._Shadow_UseCastShadow)
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

        private bool IsDisableVolumeCloudShadow(out CelestialBody celestialBodyShadowCast)
        {
            //获取进行阴影铸造的星体
            celestialBodyShadowCast = WorldManager.Instance?.celestialBodyManager?.GetShadowCastCelestialBody();
            if (celestialBodyShadowCast is null) return true;
            
            if (!isActiveAndEnabled || property._Modeling_Amount_CloudAmount < 0.25f || !property._Shadow_UseCastShadow) 
                return true;
            
            return false;
        }
        
        public void RenderVolumeCloudShadow(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (IsDisableVolumeCloudShadow(out _)) return;
            
            cmd.SetGlobalFloat(_ShadowPass, 1);
            cmd.SetGlobalVector(_RenderTextureDimensions,
                new Vector4(1f / (int)property._Shadow_Resolution, 1f / (int)property._Shadow_Resolution,
                    (int)property._Shadow_Resolution, (int)property._Shadow_Resolution));
            
            //渲染体积云阴影贴图
            //初始化RT
            if (property._Shadow_Resolution != _cloudShadowResolutionCache || cloudShadowMapRT == null)
            {
                RenderTextureDescriptor rtDescriptor =
                    new RenderTextureDescriptor((int)property._Shadow_Resolution, (int)property._Shadow_Resolution,
                        GraphicsFormat.B10G11R11_UFloatPack32, 0);
                cloudShadowMapRT?.Release();
                cloudShadowMapRT = RTHandles.Alloc(rtDescriptor, name: "CloudShadowMapRT", filterMode: FilterMode.Bilinear);
                _cloudShadowResolutionCache = property._Shadow_Resolution;
            }
            cmd.SetRenderTarget(cloudShadowMapRT);
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), property.VolumeCloud_Main_Material, 0);

            
            if (property._Shadow_UseShadowTaa)
            {
                //将 原始体积云阴影贴图 设置为全局参数
                cmd.SetGlobalTexture(_CURRENT_TAA_CLOUD_SHADOW, cloudShadowMapRT);
                //在没有进行新的复制之前拿到的上一帧的TAA结果
                if (PreviousCloudShadowRT == null)
                {
                    PreviousCloudShadowRT = RTHandles.Alloc(cloudShadowMapRT.rt.descriptor, name: "PreviousCloudShadowRT", filterMode: FilterMode.Bilinear);
                    cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_SHADOW, cloudShadowMapRT);
                }
                else
                {
                    cmd.SetGlobalTexture(_PREVIOUS_TAA_CLOUD_SHADOW, PreviousCloudShadowRT);
                }
                
                cloudShadowTaaRT ??= RTHandles.Alloc(cloudShadowMapRT.rt.descriptor, name: "CloudShadowTaaRT", filterMode: FilterMode.Bilinear);
                //渲染体积云阴影贴图TAA
                cmd.SetRenderTarget(cloudShadowTaaRT);
                Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), property.CloudShadows_TemporalAA_Material, 0);
                //将这一帧的TAA结果赋值给 cloudShadowTaaTexture 
                cmd.CopyTexture(cloudShadowTaaRT, PreviousCloudShadowRT);
                //将 TAA之后的体积云阴影贴图 设置为全局参数
                cmd.SetGlobalTexture(_CloudShadowmap, cloudShadowTaaRT);
            }
            else
            {
                PreviousCloudShadowRT?.Release();
                PreviousCloudShadowRT = null;
                cloudShadowTaaRT?.Release();
                cloudShadowTaaRT = null;
                cmd.SetGlobalTexture(_CloudShadowmap, cloudShadowMapRT);
            }

            
            //渲染屏幕空间云shadow
            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), property.CloudShadows_ScreenShadow_Material, 0);
        }

        private RTHandle cloudShadowMapRT;
        private RTHandle PreviousCloudShadowRT;
        private RTHandle cloudShadowTaaRT;
        private readonly int _CloudShadowmap = Shader.PropertyToID("_CloudShadowmap");
        private readonly int _CURRENT_TAA_CLOUD_SHADOW = Shader.PropertyToID("_CURRENT_TAA_CLOUD_SHADOW");
        private readonly int _PREVIOUS_TAA_CLOUD_SHADOW = Shader.PropertyToID("_PREVIOUS_TAA_CLOUD_SHADOW");
        

        #endregion
    }

    
    
}