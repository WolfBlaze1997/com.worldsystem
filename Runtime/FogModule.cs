using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace WorldSystem.Runtime
{
    
    public partial class FogModule
    {
        //此处编写枚举帮助函数等
        public enum HeightFogType
        {
            Linear = 1,
            Exponential = 2
        }
        public enum NoiseAffect
        {
            HeightOnly,
            DistanceOnly,
            Both
        }
        
#if UNITY_EDITOR
        
        [PropertyOrder(-100)]
        [ShowIf("@property.useSunLight")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "太阳散射雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseSunLight_Off()
        {
            property.useSunLight = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.useSunLight")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "太阳散射雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseSunLight_On()
        {
            property.useSunLight = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("@property.useDistanceFog")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Medium, Name = "距离雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseDistanceFog_Off()
        {
            property.useDistanceFog = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.useDistanceFog")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Medium, Name = "距离雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseDistanceFog_On()
        {
            property.useDistanceFog = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("@property.useSkyboxHeightFog")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Medium, Name = "大气雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseSkyboxHeightFog_Off()
        {
            property.useSkyboxHeightFog = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.useSkyboxHeightFog")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Medium, Name = "大气雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseSkyboxHeightFog_On()
        {
            property.useSkyboxHeightFog = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("@property.useHeightFog")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/04")]
        [Button(ButtonSizes.Medium, Name = "高度雾"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseHeightFog_Off()
        {
            property.useHeightFog = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.useHeightFog")]
        [VerticalGroup("Split/04")]
        [Button(ButtonSizes.Medium, Name = "高度雾"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseHeightFog_On()
        {
            property.useHeightFog = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("@property.useNoise")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/05")]
        [Button(ButtonSizes.Medium, Name = "噪波"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseNoise_Off()
        {
            property.useNoise = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.useNoise")]
        [VerticalGroup("Split/05")]
        [Button(ButtonSizes.Medium, Name = "噪波"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseNoise_On()
        {
            property.useNoise = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("@property.useSsms")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/06")]
        [Button(ButtonSizes.Medium, Name = "SSMS"), GUIColor(0.5f, 0.5f, 1f)]
        public void _UseSSMS_Off()
        {
            property.useSsms = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.useSsms")]
        [VerticalGroup("Split/06")]
        [Button(ButtonSizes.Medium, Name = "SSMS"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void _UseSSMS_On()
        {
            property.useSsms = true;
            OnValidate();
        }
        
#endif
        
    }
    
    
    [ExecuteAlways]
    public partial class FogModule : BaseModule
    {
        
        
        #region 字段
        
        [Serializable]
        public class Property
        {
            
            [FoldoutGroup("配置")][LabelText("雾材质")][ReadOnly]
            [ShowIf("@WorldManager.Instance?.fogModule?.hideFlags == HideFlags.None")]
            public Material fogMaterial;
            
            [FoldoutGroup("配置")][LabelText("雾因子材质")][ReadOnly]
            [ShowIf("@WorldManager.Instance?.fogModule?.hideFlags == HideFlags.None")]
            public Material fogFactorMaterial;
            
            [FoldoutGroup("配置")][LabelText("雾因子材质")][ReadOnly]
            [ShowIf("@WorldManager.Instance?.fogModule?.hideFlags == HideFlags.None")]
            public Material fogApplyMaterial;
            
            [FoldoutGroup("配置")][LabelText("雾因子材质")][ReadOnly]
            [ShowIf("@WorldManager.Instance?.fogModule?.hideFlags == HideFlags.None")]
            public Material ssmsMaterial;
            
            [FoldoutGroup("颜色")][LabelText("雾强度")] [GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色/_FogIntensity", 0.9f, DisableAutomaticLabelWidth = true)]
            [ShowIf("@useSunLight || useDistanceFog || useSkyboxHeightFog || useHeightFog")]
            public AnimationCurve fogIntensity = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
            
            [HorizontalGroup("颜色/_FogIntensity")][HideLabel][ReadOnly]
            [ShowIf("@useSunLight || useDistanceFog || useSkyboxHeightFog || useHeightFog")]
            public float fogIntensityExecute;
            
            [FoldoutGroup("颜色")] [LabelText("雾颜色")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色/_FogColor", 0.9f, DisableAutomaticLabelWidth = true)]
            [ShowIf("@useSunLight || useDistanceFog || useSkyboxHeightFog || useHeightFog")]
            public Gradient fogColor = new Gradient();
            
            [HorizontalGroup("颜色/_FogColor")][HideLabel][ReadOnly]
            [ShowIf("@useSunLight || useDistanceFog || useSkyboxHeightFog || useHeightFog")]
            public Color fogColorExecute = Color.white;
            
            [FoldoutGroup("颜色")] [LabelText("闪电雾颜色")][GUIColor(1f,0.7f,1f)]
            [HorizontalGroup("颜色/_FogLightningColor", 0.9f, DisableAutomaticLabelWidth = true)]
            [ShowIf("@(useSunLight || useDistanceFog || useSkyboxHeightFog || useHeightFog) && (WorldManager.Instance.weatherEffectModule?.lightningEffect?.property?.lightningSpawnRate > 0 ?? false)")]
            public Gradient fogLightningColor = new Gradient();
            
            [HorizontalGroup("颜色/_FogLightningColor")][HideLabel][ReadOnly]
            [ShowIf("@(useSunLight || useDistanceFog || useSkyboxHeightFog || useHeightFog) && (WorldManager.Instance.weatherEffectModule?.lightningEffect?.property?.lightningSpawnRate > 0 ?? false)")]
            public Color fogLightningColorExecute = Color.white;
            
            [FoldoutGroup("太阳散射雾")] [LabelText("使用太阳散射雾")][HideInInspector][GUIColor(0.7f,0.7f,1f)]
            public bool useSunLight;
            
            [FoldoutGroup("太阳散射雾")][ShowIf("useSunLight")] [GUIColor(1f,0.7f,0.7f)][LabelText("太阳雾强度")]
            [HorizontalGroup("太阳散射雾/_SunIntensity", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve sunIntensity = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,0));
            
            [HorizontalGroup("太阳散射雾/_SunIntensity")][HideLabel][ReadOnly][ShowIf("useSunLight")]
            public float sunIntensityExecute;
            
            [FoldoutGroup("太阳散射雾")] [LabelText("太阳雾权值")] [ShowIf("useSunLight")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("太阳散射雾/_SunPower", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve sunPower = new AnimationCurve(new Keyframe(0,2), new Keyframe(1,2));
            
            [HorizontalGroup("太阳散射雾/_SunPower")][HideLabel][ReadOnly][ShowIf("useSunLight")]
            public float sunPowerExecute;
            
            [FoldoutGroup("太阳散射雾")] [LabelText("太阳雾开始距离")] [ShowIf("useSunLight")][GUIColor(0.7f,0.7f,1f)]
            public float sunStartDistance = 20;
            
            [FoldoutGroup("太阳散射雾")] [LabelText("太阳雾衰减")] [ShowIf("useSunLight")][GUIColor(0.7f,0.7f,1f)]
            public float sunAtten = 50;
            
            [FoldoutGroup("距离雾")] [LabelText("使用距离雾")][HideInInspector][GUIColor(0.7f,0.7f,1f)]
            public bool useDistanceFog = true;

            [FoldoutGroup("距离雾")] [LabelText("使用径向距离")] [ShowIf("useDistanceFog")][GUIColor(0.7f,0.7f,1f)]
            public bool useRadialDistance;

            [FoldoutGroup("距离雾")] [LabelText("距离雾偏移")] [ShowIf("useDistanceFog")] [GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("距离雾/_DistanceFogOffset", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve distanceFogOffset = new AnimationCurve(new Keyframe(0,0),new Keyframe(0,1));
            
            [HorizontalGroup("距离雾/_DistanceFogOffset")] [ShowIf("useDistanceFog")][HideLabel][ReadOnly]
            public float distanceFogOffsetExecute;
            
            [FoldoutGroup("距离雾")] [LabelText("距离雾类型")] [ShowIf("useDistanceFog")][GUIColor(0.7f,0.7f,1f)]
            public FogMode fogType = FogMode.ExponentialSquared;
            
            [FoldoutGroup("距离雾")] [LabelText("    距离雾开始")] [ShowIf("@useDistanceFog && fogType == FogMode.Linear")][GUIColor(1f,0.7f,0.7f)]
            public float sceneStart = 10;

            [FoldoutGroup("距离雾")] [LabelText("    距离雾结束")] [ShowIf("@useDistanceFog && fogType == FogMode.Linear")][GUIColor(1f,0.7f,0.7f)]
            public float sceneEnd = 100;

            [FoldoutGroup("距离雾")] [LabelText("距离雾密度[0,1]")][ShowIf("useDistanceFog")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("距离雾/_FogDensity", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve fogDensity = new AnimationCurve(new Keyframe(0,0.2f), new Keyframe(1,0.2f));
            
            [HorizontalGroup("距离雾/_FogDensity")] [ShowIf("useDistanceFog")][HideLabel][ReadOnly]
            public float fogDensityExecute;
            
            [FoldoutGroup("大气雾")] [LabelText("使用大气雾")][HideInInspector][GUIColor(0.7f,0.7f,1f)]
            public bool useSkyboxHeightFog;

            [FoldoutGroup("大气雾")] [LabelText("大气雾偏移[-0.5,0.5]")] [HorizontalGroup("大气雾/_SkyboxFogOffset", 0.9f, DisableAutomaticLabelWidth = true)]
            [ShowIf("useSkyboxHeightFog")][GUIColor(1f,0.7f,0.7f)]
            public AnimationCurve skyboxFogOffset = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,0));
            
            [HorizontalGroup("大气雾/_SkyboxFogOffset")] [ShowIf("useSkyboxHeightFog")][HideLabel][ReadOnly]
            public float skyboxFogOffsetExecute;

            [FoldoutGroup("大气雾")] [Range(0f, 0.999f)] [LabelText("大气雾硬度")] [ShowIf("useSkyboxHeightFog")][GUIColor(0.7f,0.7f,1f)]
            public float skyboxFogHardness = 0.75f;

            [FoldoutGroup("大气雾")][LabelText("大气雾强度[0,1]")] [ShowIf("useSkyboxHeightFog")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("大气雾/_SkyboxFogIntensity", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve skyboxFogIntensity = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
            
            [HorizontalGroup("大气雾/_SkyboxFogIntensity")] [ShowIf("useSkyboxHeightFog")][HideLabel][ReadOnly]
            public float skyboxFogIntensityExecute;

            [FoldoutGroup("大气雾")] [Range(0f, 1f)] [LabelText("大气雾填充")] [ShowIf("useSkyboxHeightFog")][GUIColor(0.7f,0.7f,1f)]
            public float skyboxFill;
            
            [FoldoutGroup("高度雾")] [LabelText("使用高度雾")][HideInInspector][GUIColor(0.7f,0.7f,1f)]
            public bool useHeightFog;

            [FoldoutGroup("高度雾")] [LabelText("高度雾高度")] [ShowIf("useHeightFog")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("高度雾/_Height", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve height = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,0));
            
            [HorizontalGroup("高度雾/_Height")][ShowIf("useHeightFog")][HideLabel][ReadOnly]
            public float heightExecute;

            [FoldoutGroup("高度雾")] [LabelText("高度雾密度[0,0.5]")] [ShowIf("useHeightFog")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("高度雾/_HeightDensity", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve heightDensity = new AnimationCurve(new Keyframe(0,0.2f), new Keyframe(1,0.2f));
            
            [HorizontalGroup("高度雾/_HeightDensity")][ShowIf("useHeightFog")][HideLabel][ReadOnly]
            public float heightDensityExecute;
            
            [FoldoutGroup("高度雾")] [LabelText("高度雾类型")] [ShowIf("useHeightFog")][GUIColor(0.7f,0.7f,1f)]
            public HeightFogType heightFogType = HeightFogType.Exponential;
            
            [FoldoutGroup("噪波")] [LabelText("使用噪波")][HideInInspector][GUIColor(0.7f,0.7f,1f)]
            public bool useNoise;

            [FoldoutGroup("噪波")] [LabelText("噪波影响")] [ShowIf("useNoise")][GUIColor(0.7f,0.7f,1f)]
            public NoiseAffect noiseAffect = NoiseAffect.Both;

            [FoldoutGroup("噪波")] [Range(0, 2f)] [LabelText("风速影响")] [ShowIf("useNoise")][GUIColor(0.7f,0.7f,1f)]
            public float noiseWindCoeff = 0.1f;
            
            [FoldoutGroup("噪波")] [LabelText("噪波强度[0,1]")] [HorizontalGroup("噪波/_NoiseIntensity", 0.9f, DisableAutomaticLabelWidth = true)]
            [ShowIf("useNoise")][GUIColor(1f,0.7f,0.7f)]
            public AnimationCurve noiseIntensity = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,0));
            
            [HorizontalGroup("噪波/_NoiseIntensity")][ShowIf("useNoise")][HideLabel][ReadOnly]
            public float noiseIntensityExecute;

            [FoldoutGroup("噪波")] [LabelText("噪波结束距离")] [ShowIf("useNoise")][GUIColor(0.7f,0.7f,1f)]
            public float noiseDistanceEnd = 80;
            
            [FoldoutGroup("噪波")] [Range(0.0001f, 140f)] [LabelText("噪波比例")] [ShowIf("useNoise")][GUIColor(0.7f,0.7f,1f)]
            public float scale1 = 40;

            [FoldoutGroup("噪波")] [Range(0f, 1f)] [LabelText("噪波插值")] [ShowIf("useNoise")][GUIColor(0.7f,0.7f,1f)]
            public float lerp1 = 0.5f;
            
            [FoldoutGroup("SSMS")] [LabelText("使用SSMS")][HideInInspector][GUIColor(0.7f,0.7f,1f)]
            public bool useSsms;
            
            [InfoBox("注意SSMS模糊的性能消耗,会使用较多的额外RT")] [FoldoutGroup("SSMS")] [Range(-1, 1)] [LabelText("阈值")][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public float threshold;
            
            [FoldoutGroup("SSMS")] [LabelText("柔和")][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public float softKnee = 0.5f;
            
            [FoldoutGroup("SSMS")] [LabelText("模糊半径")] [Range(1,7)][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public int radius = 7;
            
            [FoldoutGroup("SSMS")] [LabelText("模糊权重")] [Range(0.1f, 100f)][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public float blurWeight = 1;
            
            [FoldoutGroup("SSMS")] [LabelText("模糊强度")] [Range(0,1)][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public float intensity = 1;
            
            [FoldoutGroup("SSMS")] [LabelText("高质量")][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public bool highQuality;
            
            [FoldoutGroup("SSMS")] [LabelText("防止闪烁")][ShowIf("useSsms")][GUIColor(0.7f,0.7f,1f)]
            public bool antiFlicker;
            
            [FoldoutGroup("SSMS")] [LabelText("淡入淡出")][ShowIf("useSsms")][ReadOnly] [ShowIf("@WorldManager.Instance?.fogModule?.hideFlags == HideFlags.None")]
            public Texture2D fadeRamp ;
            
            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) return;
                if (!UseLerp)
                {
                    //未插值时
                    fogIntensityExecute = fogIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    
                    if (!VFXLightningEffect.IsBeInLightning)
                    {
                        fogColorExecute = fogColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    }
                    
                    distanceFogOffsetExecute = distanceFogOffset.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    fogDensityExecute = fogDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    skyboxFogOffsetExecute = skyboxFogOffset.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    skyboxFogIntensityExecute = skyboxFogIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    heightExecute = height.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    heightDensityExecute = heightDensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    noiseIntensityExecute = noiseIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    sunIntensityExecute = sunIntensity.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    sunPowerExecute = sunPower.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
#if UNITY_EDITOR
                    fogLightningColorExecute = fogLightningColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
#endif
                }

                //如果处于闪电状态,调整雾颜色
                if (VFXLightningEffect.IsBeInLightning)
                {
                    fogColorExecute = fogLightningColor.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                }
                
            }
        }
        
        [HideLabel]
        public Property property = new Property();
        
        public static bool UseLerp = false;
        
        [HideInInspector]
        public bool update;
        
        #endregion
        
        
        
        #region 安装参数
        
        private static readonly int FogIntensity = Shader.PropertyToID("_FogIntensity");
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");
        private static readonly int SunIntensity = Shader.PropertyToID("_SunIntensity");
        private static readonly int DistanceFogOffset = Shader.PropertyToID("_DistanceFogOffset");
        private static readonly int SceneFogParams = Shader.PropertyToID("_SceneFogParams");
        private static readonly int SkyboxFogOffset = Shader.PropertyToID("_SkyboxFogOffset");
        private static readonly int SkyboxFogIntensity = Shader.PropertyToID("_SkyboxFogIntensity");
        private static readonly int Height = Shader.PropertyToID("_Height");
        private static readonly int HeightDensity = Shader.PropertyToID("_HeightDensity");
        private static readonly int NoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int NoiseAddMotionVector = Shader.PropertyToID("_NoiseAddMotionVector");
        private static readonly int SunPower = Shader.PropertyToID("_SunPower");
        private static readonly int SkyboxFogHardness = Shader.PropertyToID("_SkyboxFogHardness");
        private static readonly int SkyboxFill = Shader.PropertyToID("_SkyboxFill");
        private static readonly int NoiseDistanceEnd = Shader.PropertyToID("_NoiseDistanceEnd");
        private static readonly int Scale1 = Shader.PropertyToID("_Scale1");
        private static readonly int Lerp1 = Shader.PropertyToID("_Lerp1");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int Curve = Shader.PropertyToID("_Curve");
        private static readonly int PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int FadeTex = Shader.PropertyToID("_FadeTex");
        private static readonly int BlurWeight = Shader.PropertyToID("_BlurWeight");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int SampleScale = Shader.PropertyToID("_SampleScale");

        private static readonly int Usedistancefog = Shader.PropertyToID("_USEDISTANCEFOG");
        private static readonly int Useradialdistance = Shader.PropertyToID("_USERADIALDISTANCE");
        private static readonly int Fogtype = Shader.PropertyToID("_FOGTYPE");
        private static readonly int Useskyboxheightfog = Shader.PropertyToID("_USESKYBOXHEIGHTFOG");
        private static readonly int Useheightfog = Shader.PropertyToID("_USEHEIGHTFOG");
        private static readonly int Heightfogtype = Shader.PropertyToID("_HEIGHTFOGTYPE");
        private static readonly int Usenoise = Shader.PropertyToID("_USENOISE");
        private static readonly int Noiseaffect = Shader.PropertyToID("_NOISEAFFECT");
        private static readonly int AntiFlicker = Shader.PropertyToID("ANTI_FLICKER");
        private static readonly int HighQuality = Shader.PropertyToID("_HIGH_QUALITY");
        
        private void SetupStaticProperty()
        { 
            if (!property.useSsms)
            {
                property.fogMaterial.SetFloat(Usedistancefog, property.useDistanceFog ? 1 : 0);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_USEDISTANCEFOG_ON"), property.useDistanceFog);
                
                property.fogMaterial.SetFloat(Useradialdistance, property.useRadialDistance ? 1 : 0);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_USERADIALDISTANCE_ON"), property.useRadialDistance);
                
                property.fogMaterial.SetFloat(Fogtype, (int)property.fogType - 1);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_FOGTYPE_LINEAR"), property.fogType == FogMode.Linear);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_FOGTYPE_EXP"), property.fogType == FogMode.Exponential);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_FOGTYPE_EXP2"), property.fogType == FogMode.ExponentialSquared);
                
                property.fogMaterial.SetFloat(Useskyboxheightfog, property.useSkyboxHeightFog ? 1 : 0);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_USESKYBOXHEIGHTFOG_ON"), property.useSkyboxHeightFog);
                
                property.fogMaterial.SetFloat(Useheightfog, property.useHeightFog ? 1 : 0);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_USEHEIGHTFOG_ON"), property.useHeightFog);
                
                property.fogMaterial.SetFloat(Heightfogtype, (int)property.heightFogType - 1);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_HEIGHTFOGTYPE_LINEAR"), property.heightFogType == HeightFogType.Linear);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_HEIGHTFOGTYPE_EXP"), property.heightFogType == HeightFogType.Exponential);
                
                property.fogMaterial.SetFloat(Usenoise, property.useNoise ? 1 : 0);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_USENOISE_ON"), property.useNoise);
                
                property.fogMaterial.SetFloat(Noiseaffect, (int)property.noiseAffect);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_NOISEAFFECT_HEIGHTONLY"), property.noiseAffect == NoiseAffect.HeightOnly);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_NOISEAFFECT_DISTANCEONLY"), property.noiseAffect == NoiseAffect.DistanceOnly);
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_NOISEAFFECT_BOTH"), property.noiseAffect == NoiseAffect.Both);

                property.fogMaterial.SetFloat(SkyboxFogHardness, property.skyboxFogHardness);
                property.fogMaterial.SetFloat(SkyboxFill, property.skyboxFill);
                property.fogMaterial.SetFloat(NoiseDistanceEnd, property.noiseDistanceEnd);
                property.fogMaterial.SetFloat(Scale1, property.scale1);
                property.fogMaterial.SetFloat(Lerp1, property.lerp1);
                
                property.fogMaterial.SetFloat(SunStartDistance, property.sunStartDistance);
                property.fogMaterial.SetFloat(SunAtten, property.sunAtten);
            }
            else
            {
                property.fogFactorMaterial.SetFloat(Usedistancefog, property.useDistanceFog ? 1 : 0);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_USEDISTANCEFOG_ON"), property.useDistanceFog);
                
                property.fogFactorMaterial.SetFloat(Useradialdistance, property.useRadialDistance ? 1 : 0);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_USERADIALDISTANCE_ON"), property.useRadialDistance);
                
                property.fogFactorMaterial.SetFloat(Fogtype, (int)property.fogType - 1);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_FOGTYPE_LINEAR"), property.fogType == FogMode.Linear);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_FOGTYPE_EXP"), property.fogType == FogMode.Exponential);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_FOGTYPE_EXP2"), property.fogType == FogMode.ExponentialSquared);
                
                property.fogFactorMaterial.SetFloat(Useskyboxheightfog, property.useSkyboxHeightFog ? 1 : 0);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_USESKYBOXHEIGHTFOG_ON"), property.useSkyboxHeightFog);
                
                property.fogFactorMaterial.SetFloat(Useheightfog, property.useHeightFog ? 1 : 0);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_USEHEIGHTFOG_ON"), property.useHeightFog);
                
                property.fogFactorMaterial.SetFloat(Heightfogtype, (int)property.heightFogType - 1);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_HEIGHTFOGTYPE_LINEAR"), property.heightFogType == HeightFogType.Linear);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_HEIGHTFOGTYPE_EXP"), property.heightFogType == HeightFogType.Exponential);
                
                property.fogFactorMaterial.SetFloat(Usenoise, property.useNoise ? 1 : 0);
                property.fogFactorMaterial.SetKeyword(
                    new LocalKeyword(property.fogFactorMaterial.shader, "_USENOISE_ON"), property.useNoise);
                    
                property.fogFactorMaterial.SetFloat(Noiseaffect, (int)property.noiseAffect);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_NOISEAFFECT_HEIGHTONLY"), property.noiseAffect == NoiseAffect.HeightOnly);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_NOISEAFFECT_DISTANCEONLY"), property.noiseAffect == NoiseAffect.DistanceOnly);
                property.fogFactorMaterial.SetKeyword(new LocalKeyword(property.fogFactorMaterial.shader, "_NOISEAFFECT_BOTH"), property.noiseAffect == NoiseAffect.Both);
                
                property.fogFactorMaterial.SetFloat(SkyboxFogHardness, property.skyboxFogHardness);
                property.fogFactorMaterial.SetFloat(SkyboxFill, property.skyboxFill);
                property.fogFactorMaterial.SetFloat(NoiseDistanceEnd, property.noiseDistanceEnd);
                property.fogFactorMaterial.SetFloat(Scale1, property.scale1);
                property.fogFactorMaterial.SetFloat(Lerp1, property.lerp1);
                
                property.fogApplyMaterial.SetFloat(SunStartDistance, property.sunStartDistance);
                property.fogApplyMaterial.SetFloat(SunAtten, property.sunAtten);
                
                property.ssmsMaterial.SetFloat(AntiFlicker, property.antiFlicker ? 1 : 0);
                property.ssmsMaterial.SetKeyword(new LocalKeyword(property.ssmsMaterial.shader, "ANTI_FLICKER_ON"), property.antiFlicker);
                property.ssmsMaterial.SetFloat(HighQuality, property.highQuality ? 1 : 0);
                property.ssmsMaterial.SetKeyword(new LocalKeyword(property.ssmsMaterial.shader, "_HIGH_QUALITY_ON"), property.highQuality);
                
                var lthresh = property.threshold;
                property.ssmsMaterial.SetFloat(Threshold, lthresh);
                var knee = lthresh * property.softKnee + 1e-5f;
                var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
                property.ssmsMaterial.SetVector(Curve, curve);
                var pfo = !property.highQuality && property.antiFlicker;
                property.ssmsMaterial.SetFloat(PrefilterOffs, pfo ? -0.5f : 0.0f);
                property.ssmsMaterial.SetFloat(Intensity, property.intensity);
                var fadeRampTexture = property.fadeRamp;
                if(fadeRampTexture is not null) property.ssmsMaterial.SetTexture(FadeTex, fadeRampTexture);
                property.ssmsMaterial.SetFloat(BlurWeight, property.blurWeight);
                property.ssmsMaterial.SetFloat(Radius, property.radius);
            }
        }
        
        Vector3 _noiseAddMotionVector = new float3(0, 0, 0);
        private void SetupDynamicProperty()
        {
            if (WorldManager.Instance.timeModule is null) return;
            
            if (WorldManager.Instance.windZoneModule is not null)
            {
                _noiseAddMotionVector += -WorldManager.Instance.windZoneModule.property.WindDirection * 
                                         (WorldManager.Instance.timeModule.deltaTime * property.noiseWindCoeff * WorldManager.Instance.windZoneModule.property.WindSpeed);
            }
            else
            {
                _noiseAddMotionVector += -Vector3.forward *
                                         (WorldManager.Instance.timeModule.deltaTime * property.noiseWindCoeff * 1f);
            }
            
            if (!property.useSsms)
            {
                if (property.fogMaterial is null) return;
                property.fogMaterial.SetFloat(FogIntensity, property.fogIntensityExecute);
                property.fogMaterial.SetColor(FogColor, property.fogColorExecute);
                
                property.fogMaterial.SetKeyword(new LocalKeyword(property.fogMaterial.shader, "_USESUNLIGHT_ON"), property.useSunLight && property.sunIntensityExecute > 0.05f);
                property.fogMaterial.SetFloat(SunIntensity, property.sunIntensityExecute);
                property.fogMaterial.SetFloat(SunPower, property.sunPowerExecute);

                // 距离雾值
                property.fogMaterial.SetFloat(DistanceFogOffset, property.distanceFogOffsetExecute);
                Vector4 sceneParams = default;
                sceneParams.x = property.fogDensityExecute * 0.12011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
                sceneParams.y = property.fogDensityExecute * 0.14426950408f; // density / ln(2), used by Exp fog mode
                if (property.fogType == FogMode.Linear)
                {
                    float diff = property.sceneEnd - property.sceneStart;
                    float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
                    sceneParams.z = -invDiff;
                    sceneParams.w = property.sceneEnd * invDiff;
                }
                property.fogMaterial.SetVector(SceneFogParams, sceneParams);
                
                // 大气雾
                property.fogMaterial.SetFloat(SkyboxFogOffset, property.skyboxFogOffsetExecute);
                property.fogMaterial.SetFloat(SkyboxFogIntensity, property.skyboxFogIntensityExecute);

                // 高度雾
                property.fogMaterial.SetFloat(Height, property.heightExecute);
                property.fogMaterial.SetFloat(HeightDensity, property.heightDensityExecute);
                
                // 噪波
                property.fogMaterial.SetFloat(NoiseIntensity, property.noiseIntensityExecute);
                property.fogMaterial.SetVector(NoiseAddMotionVector, new float4(_noiseAddMotionVector,0));
            }
            else
            {
                property.fogFactorMaterial.SetFloat(FogIntensity, property.fogIntensityExecute);
                property.fogFactorMaterial.SetFloat(DistanceFogOffset, property.distanceFogOffsetExecute);
                Vector4 sceneParams = default;
                
                sceneParams.x = property.fogDensityExecute * 0.12011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
                sceneParams.y = property.fogDensityExecute * 0.14426950408f; // density / ln(2), used by Exp fog mode
                if (property.fogType == FogMode.Linear)
                {
                    float diff = property.sceneEnd - property.sceneStart;
                    float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
                    sceneParams.z = -invDiff;
                    sceneParams.w = property.sceneEnd * invDiff;
                }
                property.fogFactorMaterial.SetVector(SceneFogParams, sceneParams);
                
                // 大气雾
                property.fogFactorMaterial.SetFloat(SkyboxFogOffset, property.skyboxFogOffsetExecute);
                property.fogFactorMaterial.SetFloat(SkyboxFogIntensity, property.skyboxFogIntensityExecute);
                // 高度雾
                property.fogFactorMaterial.SetFloat(Height, property.heightExecute);
                property.fogFactorMaterial.SetFloat(HeightDensity, property.heightDensityExecute);
                // 噪波
                property.fogFactorMaterial.SetFloat(NoiseIntensity, property.noiseIntensityExecute);
                property.fogFactorMaterial.SetVector(NoiseAddMotionVector, new float4(_noiseAddMotionVector,0));
                
                property.fogApplyMaterial.SetKeyword(new LocalKeyword(property.fogApplyMaterial.shader, "_USESUNLIGHT_ON"), property.useSunLight && property.sunIntensityExecute > 0.05f);
                property.fogApplyMaterial.SetFloat(SunIntensity, property.sunIntensityExecute);

                property.fogApplyMaterial.SetColor(FogColor, property.fogColorExecute);
                
                property.fogApplyMaterial.SetVector(FogSunDir, WorldManager.Instance.celestialBodyManager?.sun?.direction ?? Vector3.zero);
                property.fogApplyMaterial.SetFloat(SunPower, property.sunPowerExecute);
            }
        }
        
        #endregion
        
        
        
        #region 事件函数
        
        private void NoSSMS_Unload()
        {
            //销毁卸载数据
            if(property.fogMaterial != null)
                Resources.UnloadAsset(property.fogMaterial);
            property.fogMaterial = null;
        }
        private void NoSSMS_Load()
        {
#if UNITY_EDITOR
            //载入数据
            if (property.fogMaterial == null)
                property.fogMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.worldsystem/Shader/Fog/FogShader.mat");
#endif
        }
        private void UseSSMS_Unload()
        {
            if(property.fadeRamp != null)
                Resources.UnloadAsset(property.fadeRamp);
            if(property.fogFactorMaterial != null)
                Resources.UnloadAsset(property.fogFactorMaterial);
            if(property.fogApplyMaterial != null)
                Resources.UnloadAsset(property.fogApplyMaterial);
            if(property.ssmsMaterial != null)
                Resources.UnloadAsset(property.ssmsMaterial);
            property.fogFactorMaterial = null;
            property.fogApplyMaterial = null;
            property.ssmsMaterial = null;
            property.fadeRamp = null;
                
            _fogRT?.Release();
            _fogFactorRT?.Release();
            _prefilteredRT?.Release();
            _fogRT = null;
            _fogFactorRT = null;
            _prefilteredRT = null;
                
            // 释放临时缓冲区
            for (var i = 0; i < KMaxIterations / 2; i++)
            {
                if (_blurBuffer1[i] != null)
                {
                    RenderTexture.ReleaseTemporary(_blurBuffer1[i].rt);
                    _blurBuffer1[i] = null;
                }
                if (_blurBuffer2[i] != null)
                {
                    RenderTexture.ReleaseTemporary(_blurBuffer2[i].rt);
                    _blurBuffer2[i] = null;
                }
            }
        }
        private void UseSSMS_Load()
        {
#if UNITY_EDITOR
            //载入数据
            if(property.fadeRamp == null)
                property.fadeRamp = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Fog/SSMS_nonLinear2.png");
            if (property.fogFactorMaterial == null)
                property.fogFactorMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.worldsystem/Shader/Fog/FogFactor.mat");
            if (property.fogApplyMaterial == null)
                property.fogApplyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.worldsystem/Shader/Fog/FogApply.mat");
            if (property.ssmsMaterial == null)
                property.ssmsMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.worldsystem/Shader/Fog/SSMS.mat");
#endif
        }
        
        
        private void OnEnable()
        {
            OnValidate();
        }
        
        
        private void OnDisable()
        {
            if (Time.frameCount == 0) return;
            
            UseSSMS_Unload();
            NoSSMS_Unload();
        }

        public void OnValidate()
        {
            if (!property.useSsms)
            {
                NoSSMS_Load();
                UseSSMS_Unload();
            }
            else
            {
                UseSSMS_Load();
                NoSSMS_Unload();
            }
            SetupStaticProperty();
        }
        
        
        void Update()
        {
            if (!update) return;
            
            property.ExecuteProperty();
            
            //没搞懂,没有这个,编辑模式unity保存时有点问题,运行打包没有问题,这应该是unity的BUG
#if UNITY_EDITOR
            SetupStaticProperty();
#endif
            SetupDynamicProperty();
        }
        
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        #endregion

        

        #region 渲染函数

        private RTHandle _fogFactorRT;
        private RTHandle _fogRT;
        private RTHandle _prefilteredRT;
        private const int KMaxIterations = 16;
        RTHandle[] _blurBuffer1 = new RTHandle[KMaxIterations / 2];
        RTHandle[] _blurBuffer2 = new RTHandle[KMaxIterations / 2];
        private readonly int _FogCameraTex = Shader.PropertyToID("_FogCameraTex");
        private readonly int _FogFactorTex = Shader.PropertyToID("_FogFactorTex");
        private readonly int _SSMSBaseTex = Shader.PropertyToID("_SSMSBaseTex");
        private static readonly int SunStartDistance = Shader.PropertyToID("_SunStartDistance");
        private static readonly int SunAtten = Shader.PropertyToID("_SunAtten");
        private static readonly int FogSunDir = Shader.PropertyToID("_FogSunDir");

        public void RenderFog(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraColorRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (cameraColorRT == null) return;
            
            if (!property.useSsms)
            {
                RenderingUtils.ReAllocateIfNeeded(ref _fogRT, 
                    new RenderTextureDescriptor(cameraColorRT.rt.descriptor.width, cameraColorRT.rt.descriptor.height, cameraColorRT.rt.descriptor.colorFormat),
                    FilterMode.Bilinear, TextureWrapMode.Clamp, name: "FogRT");
                cmd.SetGlobalTexture(_FogCameraTex, cameraColorRT);
                cmd.SetRenderTarget(_fogRT);
                Blitter.BlitTexture(cmd, new Vector4(1,1,0,0),property.fogMaterial, 0);
                Blitter.BlitCameraTexture(cmd, _fogRT, cameraColorRT);
            }
            else
            {
                RenderingUtils.ReAllocateIfNeeded(ref _fogRT, 
                    new RenderTextureDescriptor(cameraColorRT.rt.descriptor.width, cameraColorRT.rt.descriptor.height, cameraColorRT.rt.descriptor.colorFormat),
                    FilterMode.Bilinear, TextureWrapMode.Clamp, name: "FogRT");
                RenderingUtils.ReAllocateIfNeeded(ref _fogFactorRT, 
                    new RenderTextureDescriptor(cameraColorRT.rt.descriptor.width, cameraColorRT.rt.descriptor.height, RenderTextureFormat.RHalf), 
                    FilterMode.Point, TextureWrapMode.Clamp, name: "FogFactorRT");
                
                cmd.SetGlobalTexture(_FogCameraTex, cameraColorRT);
                cmd.SetRenderTarget(_fogFactorRT);
                Blitter.BlitTexture(cmd, new Vector4(1,1,0,0),property.fogFactorMaterial, 0);
                cmd.SetGlobalTexture(_FogFactorTex, _fogFactorRT);
                
                cmd.SetRenderTarget(_fogRT);
                Blitter.BlitTexture(cmd, new Vector4(1,1,0,0),property.fogApplyMaterial, 0);
                
              
				// 源纹理大小
				var tw = cameraColorRT.rt.descriptor.width;
				var th = cameraColorRT.rt.descriptor.height;

				// 在半分辨率上做雾，全分辨率不会带来太多效果
				tw /= 2;
				th /= 2;

                // 确定迭代次数
                var logh = Mathf.Log(th, 2) + property.radius - 8;
                var logh_i = (int)logh;
                var iterations = Mathf.Clamp(logh_i, 1, KMaxIterations);
                property.ssmsMaterial.SetFloat(SampleScale, 0.5f + logh - logh_i);

				var rtFormat = RenderTextureFormat.ARGBHalf;
				var rtFilterMode = FilterMode.Bilinear;

				// prefilter pass
				RenderTextureDescriptor prefilteredDesc = 
                    new RenderTextureDescriptor(cameraColorRT.rt.descriptor.width, cameraColorRT.rt.descriptor.height,rtFormat,0,0);
				RenderingUtils.ReAllocateIfNeeded(ref _prefilteredRT, prefilteredDesc, rtFilterMode, TextureWrapMode.Clamp, name : "prefilteredRT");
				
				var pass = 0;
				cmd.SetGlobalTexture(_FogCameraTex, _fogRT);
				Blitter.BlitCameraTexture(cmd, _fogRT, _prefilteredRT, property.ssmsMaterial, pass);
				
				
				// 构建mip金字塔(下采样)
                RTHandle last = _prefilteredRT;
				
				for (var level = 0; level < iterations; level++)
				{
					RenderTextureDescriptor blurBufferDesc = new RenderTextureDescriptor(tw, th, rtFormat, 0, 0);
                    _blurBuffer1[level] = RTHandles.Alloc(RenderTexture.GetTemporary(blurBufferDesc));
                    _blurBuffer2[level] = RTHandles.Alloc(RenderTexture.GetTemporary(blurBufferDesc));
                    
					tw = Mathf.Max(tw / 2, 1);
					th = Mathf.Max(th / 2, 1);
				
					pass = (level == 0) ?  1 : 2;
					cmd.SetGlobalTexture(_FogCameraTex, last);
					Blitter.BlitCameraTexture(cmd, last, _blurBuffer1[level], property.ssmsMaterial, pass);

					last = _blurBuffer1[level];
				}
				
				// 上采样和组合循环
				for (var level = iterations - 2; level >= 0; level--)
				{
					var basetex = _blurBuffer1[level];
					cmd.SetGlobalTexture(_SSMSBaseTex, basetex);
				
					pass = 3;
					cmd.SetGlobalTexture(_FogCameraTex, last);
					Blitter.BlitCameraTexture(cmd, last, _blurBuffer2[level], property.ssmsMaterial, pass);
				
					last = _blurBuffer2[level];
				}
                
				// 完成
				cmd.SetGlobalTexture(_SSMSBaseTex, _fogRT);
                cmd.SetGlobalTexture(_FogCameraTex, last);

				pass = 4;
				Blitter.BlitCameraTexture(cmd, last, renderingData.cameraData.renderer.cameraColorTargetHandle, property.ssmsMaterial, pass);
                
                // 释放临时缓冲区
                for (var i = 0; i < KMaxIterations / 2; i++)
                {
                    if (_blurBuffer1[i] != null)
                    {
                        RenderTexture.ReleaseTemporary(_blurBuffer1[i].rt);
                    }
                    if (_blurBuffer2[i] != null)
                    {
                        RenderTexture.ReleaseTemporary(_blurBuffer2[i].rt);
                    }
                }
            }
            
        }
        
        #endregion
        
        
    }
    
}
