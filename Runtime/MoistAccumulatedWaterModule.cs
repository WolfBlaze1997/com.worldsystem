using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WorldSystem.Runtime
{
    public class MoistAccumulatedWaterModule : BaseModule
    {

        #region 字段

        [Serializable]
        public class Property
        {
            [InfoBox("需要着色器支持,如果无效请为着色器添加提供的ASE节点!")]
            [PropertyRange(0,2)][LabelText("全局湿润")]
            [GUIColor(1f,0.7f,0.7f)]
            public float globalMoist;

            [FoldoutGroup("雨点")][LabelText("雨点渐变纹理")]
            [ReadOnly] [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D raindropsGradientMap;

            [FoldoutGroup("雨点")][LabelText("雨点平铺")]
            [GUIColor(0.7f,0.7f,1f)]
            public float raindropsTiling = 20f;

            [FoldoutGroup("雨点")][LabelText("雨点飞溅速度")]
            [GUIColor(0.7f,0.7f,1f)]
            public float raindropsSplashSpeed = 0.3f;

            [FoldoutGroup("雨点")][LabelText("雨点大小")]
            [GUIColor(0.7f,0.7f,1f)]
            public float raindropsSize = 0.4f;

            [FoldoutGroup("积水")][LabelText("积水贴图")]
            [ReadOnly] [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D accumulatedWaterMask;

            [FoldoutGroup("积水")][LabelText("积水贴图平铺")]
            [GUIColor(0.7f,0.7f,1f)]
            public float accumulatedWaterMaskTiling = 0.7f;

            [FoldoutGroup("积水")][LabelText("积水贴图对比度")]
            [GUIColor(0.7f,0.7f,1f)]
            public float accumulatedWaterContrast = 2f;

            [FoldoutGroup("积水")][LabelText("积水斜坡消除")][PropertyRange(0.9,0.999)]
            [GUIColor(0.7f,0.7f,1f)]
            public float accumulatedWaterSteepHillExtinction = 0.97f;

            [FoldoutGroup("积水")][LabelText("积水视差强度")][PropertyRange(0,1)]
            [GUIColor(0.7f,0.7f,1f)]
            public float accumulatedWaterParallaxStrength = 0.2f;

            [FoldoutGroup("涟漪")][LabelText("涟漪序列帧")]
            [ReadOnly][ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D ripplesNormalAtlas;

            [FoldoutGroup("涟漪")][LabelText("X(列)-Y(排)-Z(速度)-W(开始帧)")]
            [GUIColor(0.7f,0.7f,1f)]
            public float4 xColumnsYRowsZSpeedWStrartFrame = new(8,8,12,0);

            [FoldoutGroup("涟漪")][LabelText("涟漪平铺")]
            [GUIColor(0.7f,0.7f,1f)]
            public float ripplesMainTiling = 1f;

            [FoldoutGroup("涟漪")][LabelText("涟漪强度")]
            [GUIColor(0.7f,0.7f,1f)]
            public float ripplesMainStrength = 0.6f;

            [FoldoutGroup("水波")][LabelText("水波法线")]
            [ReadOnly][ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D waterWaveNormal;

            [FoldoutGroup("水波")][LabelText("水波旋转")]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveRotate;

            [FoldoutGroup("水波")][LabelText("主水波平铺")]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveMainTiling = 1f;

            [FoldoutGroup("水波")][LabelText("主水波速度")][Range(0.01f,0.2f)]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveMainSpeed = 0.05f;

            [FoldoutGroup("水波")][LabelText("主水波强度")][Range(0.01f,0.2f)]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveMainStrength = 0.05f;

            [FoldoutGroup("水波")][LabelText("细节水波平铺")]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveDetailTiling = 1.5f;

            [FoldoutGroup("水波")][LabelText("细节水波速度")][Range(0.01f,0.2f)]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveDetailSpeed = 0.03f;

            [FoldoutGroup("水波")][LabelText("细节水波强度")][Range(0.01f,0.2f)]
            [GUIColor(0.7f,0.7f,1f)]
            public float waterWaveDetailStrength = 0.03f;

            [FoldoutGroup("水流")][LabelText("水流贴图")]
            [ReadOnly] [ShowIf("@WorldManager.Instance?.atmosphereModule?.hideFlags == HideFlags.None")]
            public Texture2D flowMap;

            [FoldoutGroup("水流")][LabelText("水流贴图平铺")]
            [GUIColor(0.7f,0.7f,1f)]
            public float3 flowTiling = new(1.5f,8f,1.5f);
        }
        [HideLabel]
        public Property property = new Property();
        
        #endregion
        
        
        
        #region 安装属性

        private void SetupStaticProperty()
        {
            Shader.SetGlobalVector(_FlowTiling, new float4(property.flowTiling,0));
            Shader.SetGlobalTexture(_FlowMap, property.flowMap);
            Shader.SetGlobalTexture(_RaindropsGradientMap, property.raindropsGradientMap);
            Shader.SetGlobalFloat(_RaindropsTiling, property.raindropsTiling);
            Shader.SetGlobalFloat(_RaindropsSplashSpeed, property.raindropsSplashSpeed);
            Shader.SetGlobalFloat(_RaindropsSize, property.raindropsSize);
            Shader.SetGlobalTexture(_AccumulatedWaterMask, property.accumulatedWaterMask);
            Shader.SetGlobalFloat(_AccumulatedWaterMaskTiling, property.accumulatedWaterMaskTiling);
            Shader.SetGlobalFloat(_AccumulatedWaterContrast, property.accumulatedWaterContrast);
            Shader.SetGlobalFloat(_AccumulatedWaterSteepHillExtinction, property.accumulatedWaterSteepHillExtinction);
            Shader.SetGlobalFloat(_AccumulatedWaterParallaxStrength, property.accumulatedWaterParallaxStrength);
            Shader.SetGlobalTexture(_RipplesNormalAtlas, property.ripplesNormalAtlas);
            Shader.SetGlobalVector(_XColumnsYRowsZSpeedWStrartFrame, property.xColumnsYRowsZSpeedWStrartFrame);
            Shader.SetGlobalFloat(_RipplesMainTiling, property.ripplesMainTiling);
            Shader.SetGlobalFloat(_RipplesMainStrength, property.ripplesMainStrength);
            Shader.SetGlobalTexture(_WaterWaveNormal, property.waterWaveNormal);
            Shader.SetGlobalFloat(_WaterWaveRotate, property.waterWaveRotate);
            Shader.SetGlobalFloat(_WaterWaveMainTiling, property.waterWaveMainTiling);
            Shader.SetGlobalFloat(_WaterWaveMainSpeed, property.waterWaveMainSpeed);
            Shader.SetGlobalFloat(_WaterWaveMainStrength, property.waterWaveMainStrength);
            Shader.SetGlobalFloat(_WaterWaveDetailTiling, property.waterWaveDetailTiling);
            Shader.SetGlobalFloat(_WaterWaveDetailSpeed, property.waterWaveDetailSpeed);
            Shader.SetGlobalFloat(_WaterWaveDetailStrength, property.waterWaveDetailStrength);
        }
        private static readonly int _FlowTiling = Shader.PropertyToID("_FlowTiling");
        private static readonly int _FlowMap = Shader.PropertyToID("_FlowMap");
        private static readonly int _RaindropsGradientMap = Shader.PropertyToID("_RaindropsGradientMap");
        private static readonly int _RaindropsTiling = Shader.PropertyToID("_RaindropsTiling");
        private static readonly int _RaindropsSplashSpeed = Shader.PropertyToID("_RaindropsSplashSpeed");
        private static readonly int _RaindropsSize = Shader.PropertyToID("_RaindropsSize");
        private static readonly int _AccumulatedWaterMask = Shader.PropertyToID("_AccumulatedWaterMask");
        private static readonly int _AccumulatedWaterMaskTiling = Shader.PropertyToID("_AccumulatedWaterMaskTiling");
        private static readonly int _AccumulatedWaterContrast = Shader.PropertyToID("_AccumulatedWaterContrast");
        private static readonly int _AccumulatedWaterSteepHillExtinction = Shader.PropertyToID("_AccumulatedWaterSteepHillExtinction");
        private static readonly int _AccumulatedWaterParallaxStrength = Shader.PropertyToID("_AccumulatedWaterParallaxStrength");
        private static readonly int _RipplesNormalAtlas = Shader.PropertyToID("_RipplesNormalAtlas");
        private static readonly int _XColumnsYRowsZSpeedWStrartFrame = Shader.PropertyToID("_XColumnsYRowsZSpeedWStrartFrame");
        private static readonly int _RipplesMainTiling = Shader.PropertyToID("_RipplesMainTiling");
        private static readonly int _RipplesMainStrength = Shader.PropertyToID("_RipplesMainStrength");
        private static readonly int _WaterWaveNormal = Shader.PropertyToID("_WaterWaveNormal");
        private static readonly int _WaterWaveRotate = Shader.PropertyToID("_WaterWaveRotate");
        private static readonly int _WaterWaveMainTiling = Shader.PropertyToID("_WaterWaveMainTiling");
        private static readonly int _WaterWaveMainSpeed = Shader.PropertyToID("_WaterWaveMainSpeed");
        private static readonly int _WaterWaveMainStrength = Shader.PropertyToID("_WaterWaveMainStrength");
        private static readonly int _WaterWaveDetailTiling = Shader.PropertyToID("_WaterWaveDetailTiling");
        private static readonly int _WaterWaveDetailSpeed = Shader.PropertyToID("_WaterWaveDetailSpeed");
        private static readonly int _WaterWaveDetailStrength = Shader.PropertyToID("_WaterWaveDetailStrength");

        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_GlobalMoist, property.globalMoist);
        }
        private static readonly int _GlobalMoist = Shader.PropertyToID("_GlobalMoist");

        #endregion

        
        
        #region 事件函数
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if(property.raindropsGradientMap == null)
                property.raindropsGradientMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Moist/Gradient.png");
            if(property.flowMap == null)
                property.flowMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Moist/WaterFlowMap.tga");
            if(property.accumulatedWaterMask == null)
                property.accumulatedWaterMask = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Moist/AccumulatedwaterMask.png");
            if(property.ripplesNormalAtlas == null)
                property.ripplesNormalAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Moist/RipplesNormalFlipbook.png");
            if(property.waterWaveNormal == null)
                property.waterWaveNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Moist/WaterWaveNormal.png");
#endif
            
            // this.hideFlags = HideFlags.None;
            OnValidate();
        }

        private void OnDisable()
        {
            Shader.SetGlobalFloat(_GlobalMoist, 0);
            
            if(property.raindropsGradientMap != null)
                Resources.UnloadAsset(property.raindropsGradientMap);
            if(property.flowMap != null)
                Resources.UnloadAsset(property.flowMap);
            if(property.accumulatedWaterMask != null)
                Resources.UnloadAsset(property.accumulatedWaterMask);
            if(property.ripplesNormalAtlas != null)
                Resources.UnloadAsset(property.ripplesNormalAtlas);
            if(property.waterWaveNormal != null)
                Resources.UnloadAsset(property.waterWaveNormal);

            property.raindropsGradientMap = null;
            property.flowMap = null;
            property.accumulatedWaterMask = null;
            property.ripplesNormalAtlas = null;
            property.waterWaveNormal = null;
        }
        
        public void OnValidate()
        {
            SetupStaticProperty();
        }
        
        
        [HideInInspector]
        public bool _Update;
        private void Update()
        {
            if (!_Update) return;
            SetupDynamicProperty();
        }
        
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        #endregion
        
        
    }
}
