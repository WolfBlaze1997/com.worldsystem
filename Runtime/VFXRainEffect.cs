using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace WorldSystem.Runtime
{
    
    public class VFXRainEffect : BaseModule
    {
        #region 字段
        
        [Serializable]
        public class Property
        {
            [LabelText("范围半径")][GUIColor(0.7f,0.7f,0.7f)][DisableInEditorMode]
            public float rainRadius = 40;

            [LabelText("降水量")][GUIColor(1f,0.7f,0.7f)]
            public float rainPrecipitation;

            [LabelText("粒子大小")][GUIColor(1f,0.7f,0.7f)]
            public float rainSize = 0.01f;

            [LabelText("水滴长度")][GUIColor(1f,0.7f,0.7f)]
            public float rainDropLength = 10;

            public void LimitProperty()
            {
                rainPrecipitation = Math.Clamp(rainPrecipitation, 0, 2);
                rainSize = Math.Clamp(rainSize, 0.01f, 0.1f);
                rainDropLength = Math.Max(rainDropLength, 5);
            }
        }
        
        [HideLabel]
        public Property property  = new();

        #endregion

        [HideInInspector]
        public VisualEffect rainEffect;
        
        private bool _isActive;
        
        
        #region 安装属性
        
        private void SetupDynamicProperty()
        {
            if (rainEffect is null) return;
            
            rainEffect.SetFloat(RainDynamic_Precipitation, property.rainPrecipitation);
            rainEffect.SetFloat(RainDynamic_Size, property.rainSize);
            rainEffect.SetFloat(RainDynamic_DropLength, property.rainDropLength);
            
            WorldManager.Instance?.weatherEffectModule?.SetupCommonDynamicProperty(rainEffect);
        }
        private void SetupStaticProperty()
        {
            if (rainEffect == null) return;
            rainEffect.SetFloat(Static_Radius, property.rainRadius);
        }
        private readonly int Static_Radius = Shader.PropertyToID("Static_Radius");
        private readonly int RainDynamic_Precipitation = Shader.PropertyToID("RainDynamic_Precipitation");
        private readonly int RainDynamic_Size = Shader.PropertyToID("RainDynamic_Size");
        private readonly int RainDynamic_DropLength = Shader.PropertyToID("RainDynamic_DropLength");

        #endregion

        
        #region 事件函数
        private void OnEnable()
        {
            if (gameObject.GetComponent<VisualEffect>() == null)
            {
                rainEffect = gameObject.AddComponent<VisualEffect>();
#if UNITY_EDITOR
                rainEffect.visualEffectAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>("Packages/com.worldsystem//Visual Effects/Rain.vfx");
#endif
            }
            else
            {
                rainEffect = gameObject.GetComponent<VisualEffect>();
            }
        }
        public void OnDisable()
        {
            if (gameObject.GetComponent<VisualEffect>() != null)
            {
                if(gameObject.GetComponent<VisualEffect>().visualEffectAsset != null)
                    Resources.UnloadAsset(gameObject.GetComponent<VisualEffect>().visualEffectAsset);
                if(gameObject.activeSelf && Time.frameCount != 0)
                    CoreUtils.Destroy(gameObject.GetComponent<VisualEffect>());
                rainEffect = null;
            }
            
        }
        public void OnValidate()
        {
            property.LimitProperty();
            SetupStaticProperty();
        }
        
        
        [HideInInspector] public bool _Update;
        private void Update()
        {
            if (!_Update) return;

            //确定是否激活, 如果没有激活则跳出函数, 节约资源
            _isActive = !(property.rainPrecipitation == 0 && rainEffect.aliveParticleCount < 100);
            rainEffect.enabled = _isActive;
            if (!_isActive) return;
            
            SetupDynamicProperty(); 
        }
        
        
        #endregion

    }
}