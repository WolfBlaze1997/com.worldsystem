using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace WorldSystem.Runtime
{
    public class VFXSnowEffect : BaseModule
    {
        #region 字段

        [Serializable]
        public class Property
        {
            [LabelText("范围半径")][GUIColor(0.7f,0.7f,0.7f)][DisableInEditorMode]
            public float snowRadius = 40;

            [LabelText("降水量")][GUIColor(1f,0.7f,0.7f,0.7f)]
            public float snowPrecipitation;

            [LabelText("粒子大小")][GUIColor(1f,0.7f,0.7f,0.7f)]
            public float snowSize = 2;

            public void LimitProperty()
            {
                snowPrecipitation = Math.Clamp(snowPrecipitation, 0, 2);
                snowSize = Math.Clamp(snowSize, 1, 5);
            }
        }
        
        [HideLabel]
        public Property property = new();

        #endregion
        
        [HideInInspector]
        public VisualEffect snowEffect;
        private bool _isActive;
        
        #region 限制属性/安装属性
        
        private void SetupDynamicProperty()
        {
            if (snowEffect is null) return;
            
            snowEffect.SetFloat(SnowDynamic_Precipitation, property.snowPrecipitation);
            snowEffect.SetFloat(SnowDynamic_Size, property.snowSize);
            
            WorldManager.Instance?.weatherEffectModule?.SetupCommonDynamicProperty(snowEffect);
        }
        private void SetupStaticProperty()
        {
            if (snowEffect == null) return;
            snowEffect.SetFloat(Static_Radius, property.snowRadius);
        }
        private readonly int Static_Radius = Shader.PropertyToID("Static_Radius");
        private readonly int SnowDynamic_Precipitation = Shader.PropertyToID("SnowDynamic_Precipitation");
        private readonly int SnowDynamic_Size = Shader.PropertyToID("SnowDynamic_Size");

        #endregion


        #region 事件函数
        private void OnEnable()
        {
            if (gameObject.GetComponent<VisualEffect>() == null)
            {
                snowEffect = gameObject.AddComponent<VisualEffect>();
#if UNITY_EDITOR
                snowEffect.visualEffectAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>("Packages/com.worldsystem//Visual Effects/Snow.vfx");
#endif
            }
            else
            {
                snowEffect = gameObject.GetComponent<VisualEffect>();
            }
        }
        private void OnDisable()
        {
            if (gameObject.GetComponent<VisualEffect>() != null)
            {
                if(gameObject.GetComponent<VisualEffect>().visualEffectAsset != null)
                    Resources.UnloadAsset(gameObject.GetComponent<VisualEffect>().visualEffectAsset);
                if(gameObject.activeSelf && Time.frameCount != 0)
                    CoreUtils.Destroy(gameObject.GetComponent<VisualEffect>());
                snowEffect = null;
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
            _isActive = !(property.snowPrecipitation <=0 && snowEffect.aliveParticleCount < 100);
            snowEffect.enabled = _isActive;
            if (!_isActive) return;
            
            SetupDynamicProperty(); 
        }
        
        #endregion

        
    }
}