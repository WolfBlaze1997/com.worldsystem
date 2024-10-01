using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace WorldSystem.Runtime
{
    public partial class WeatherEffectModule
    {
        
#if UNITY_EDITOR
        
        protected override void DrawGizmos()
        {
            if (WorldManager.Instance != null && mainCamera != null)
            {
                Color cache = Handles.color;
                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.DrawDottedLine(WorldManager.Instance.transform.position, mainCamera.transform.position,2);
                Handles.color = cache;
            }
        }
        
        [PropertyOrder(-100)]
        [ShowIf("@property.rainEnable")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块"), GUIColor(0.5f, 0.5f, 1f)]
        public void RainToggle_Off()
        {
            property.rainEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.rainEnable")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void RainToggle_On()
        {
            property.rainEnable = true;
            OnValidate();
        }
        
        
        [PropertyOrder(-100)]
        [ShowIf("@property.rainSpatterEnable")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Medium, Name = "雨滴飞溅模块"), GUIColor(0.5f, 0.5f, 1f)]
        public void RainSpatterToggle_Off()
        {
            property.rainSpatterEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.rainSpatterEnable")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Medium, Name = "雨滴飞溅模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void RainSpatterToggle_On()
        {
            property.rainSpatterEnable = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("@property.snowEnable")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Medium, Name = "雪模块"), GUIColor(0.5f, 0.5f, 1f)]
        public void SnowToggle_Off()
        {
            property.snowEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.snowEnable")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Medium, Name = "雪模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void SnowToggle_On()
        {
            property.snowEnable = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("@property.lightningEnable")]
        [VerticalGroup("Split/04")]
        [Button(ButtonSizes.Medium, Name = "闪电模块"), GUIColor(0.5f, 0.5f, 1f)]
        public void LightningToggle_Off()
        {
            property.lightningEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("@property.lightningEnable")]
        [VerticalGroup("Split/04")]
        [Button(ButtonSizes.Medium, Name = "闪电模块"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void LightningToggle_On()
        {
            property.lightningEnable = true;
            OnValidate();
        }
        
        
#endif
    }

    
    [ExecuteAlways]
    public partial class WeatherEffectModule : BaseModule
    {
        
        #region 字段
        
        [Serializable]
        public class Property
        {
            [LabelText("使用遮蔽")][GUIColor(0, 1, 0)]
            public bool useOcclusion;

            [LabelText("风速影响")][GUIColor(0.7f,0.7f,1f)]
            public float windSpeedCoeff = 1f;

            [LabelText("范围半径")][GUIColor(0.7f,0.7f,1f)]
            public float effectRadius = 40;
            
            [LabelText("粒子亮度(雨滴,雪花,水滴,水流,水花等)")][GUIColor(1f,0.7f,1f)] [HorizontalGroup("ParticleBrightGroup", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve particleBright = new AnimationCurve(new Keyframe(0f,1.0f), new Keyframe(1.0f,1.0f));
            
            [HorizontalGroup("ParticleBrightGroup")][HideLabel][ReadOnly]
            public float particleBrightExecute;
            
            [HideInInspector]
            public bool rainEnable;
            
            [HideInInspector]
            public bool rainSpatterEnable;
            
            [HideInInspector]
            public bool snowEnable;
            
            [HideInInspector]
            public bool lightningEnable;
            
            public void LimitProperty()
            {
                effectRadius = Math.Max(effectRadius, 10);
            }

            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) 
                    return;
                particleBrightExecute = particleBright.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
            }
        }
        
        [HideLabel]
        public Property property = new Property();
        
        [FoldoutGroup("雨")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("@property.rainEnable")]
        public VFXRainEffect rainEffect;
        
        [FoldoutGroup("雨滴飞溅")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("@property.rainSpatterEnable")]
        public VFXRainSpatterEffect rainSpatterEffect;
        
        [FoldoutGroup("雪")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("@property.snowEnable")]
        public VFXSnowEffect snowEffect;
        
        [FoldoutGroup("闪电")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("@property.lightningEnable")]
        public VFXLightningEffect lightningEffect;
        
        [FoldoutGroup("遮蔽渲染器")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("@property.useOcclusion")]
        public OcclusionRenderer occlusionRenderer;
        
        [HideInInspector]
        public Camera mainCamera;

        [HideInInspector] public bool update;
        
        #endregion

        
        #region 限制属性/安装属性
        
        private readonly int _PrecipitationGlobal = Shader.PropertyToID("_PrecipitationGlobal");
        private readonly int Dynamic_WindZone = Shader.PropertyToID("Dynamic_WindZone");
        private readonly int Dynamic_ParticleBright = Shader.PropertyToID("Dynamic_ParticleBright");
        private readonly int Dynamic_PrecipitationDepthEnabled = Shader.PropertyToID("Dynamic_PrecipitationDepthEnabled");
        private readonly int Dynamic_PrecipitationDepthBufferTexture = Shader.PropertyToID("Dynamic_PrecipitationDepthBufferTexture");
        private readonly int Dynamic_PrecipitationDepthCameraClipPlanes = Shader.PropertyToID("Dynamic_PrecipitationDepthCameraClipPlanes");
        private readonly int Dynamic_PrecipitationDepthWorldToCameraMatrix = Shader.PropertyToID("Dynamic_PrecipitationDepthWorldToCameraMatrix");
        private readonly int Dynamic_PrecipitationDepthCameraOrthographicSize = Shader.PropertyToID("Dynamic_PrecipitationDepthCameraOrthographicSize");
        
        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_PrecipitationGlobal, Math.Max(rainEffect?.property?.rainPrecipitation ?? 0, snowEffect?.property?.snowPrecipitation ?? 0));
        }

        public void SetupCommonDynamicProperty( VisualEffect effect)
        {
            if (WorldManager.Instance?.windZoneModule is not null)
            {
                effect.SetVector3(Dynamic_WindZone,
                    WorldManager.Instance.windZoneModule.property.WindDirection * 
                    (WorldManager.Instance.windZoneModule.property.WindSpeed * property.windSpeedCoeff));
            }
            
            effect.SetFloat(Dynamic_ParticleBright, property.particleBrightExecute);
            
            //遮蔽参数
            if (property.useOcclusion && occlusionRenderer?.occlusionCamera is not null)
            {
                effect.SetBool(Dynamic_PrecipitationDepthEnabled, property.useOcclusion);//静态
                
                if(occlusionRenderer.occlusionCamera.targetTexture is not null)
                    effect.SetTexture(Dynamic_PrecipitationDepthBufferTexture, occlusionRenderer.occlusionCamera.targetTexture);
                
                effect.SetVector2(Dynamic_PrecipitationDepthCameraClipPlanes,
                    new Vector2(occlusionRenderer.occlusionCamera.nearClipPlane, occlusionRenderer.occlusionCamera.farClipPlane));
                effect.SetMatrix4x4(Dynamic_PrecipitationDepthWorldToCameraMatrix, occlusionRenderer.occlusionCamera.worldToCameraMatrix);
                effect.SetFloat(Dynamic_PrecipitationDepthCameraOrthographicSize, occlusionRenderer.occlusionCamera.orthographicSize);
            }
        }
        
        #endregion
        
        
        #region 事件函数
        
        private void OnEnable()
        {
            mainCamera = Camera.main;
            OnValidate();
        }
        
        private void OnDestroy()
        {
            property.rainEnable = false;
            property.rainSpatterEnable = false;
            property.snowEnable = false;
            property.lightningEnable = false;
            property.useOcclusion = false;
            OnValidate();
            
            mainCamera = null;
        }
        
        public void OnValidate()
        {
            property.LimitProperty();
                
            //根据情况创建或销毁天气效果
            snowEffect = CreateOrDestroyEffect<VFXSnowEffect>("VFXSnowEffect", property.snowEnable);
            rainEffect = CreateOrDestroyEffect<VFXRainEffect>("VFXRainEffect", property.rainEnable);
            rainSpatterEffect = CreateOrDestroyEffect<VFXRainSpatterEffect>("VFXRainSpatterEffect", property.rainSpatterEnable, false);
            lightningEffect = CreateOrDestroyEffect<VFXLightningEffect>("VFXLightningEffect", property.lightningEnable);
            occlusionRenderer = CreateOrDestroyEffect<OcclusionRenderer>("OcclusionRenderer", property.useOcclusion);
            
            //统一半径
            if (rainEffect != null) rainEffect.property.rainRadius = property.effectRadius;
            if (snowEffect != null) snowEffect.property.snowRadius = property.effectRadius;
            if (occlusionRenderer != null) occlusionRenderer.effectRadius = property.effectRadius;
        }
        
        
        private void Update()
        {
            if (!update) return;
            
            property.ExecuteProperty();

            //固定位置到MainCamera上方
            if (mainCamera is not null)
                transform.position = mainCamera.transform.position + new Vector3(0, property.effectRadius * 0.6f,0);
            
            SetupDynamicProperty();
        }
        
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        #endregion

        
        #region 重要函数

        private T CreateOrDestroyEffect<T>(string objectName, bool isEnable, bool isChild = true) where T : MonoBehaviour
        {
            if (isChild)
            {
                switch (isEnable)
                {
                    case true when gameObject.GetComponentInChildren<T>() == null:
                    {
                        GameObject Object = new GameObject(objectName);
                        Object.transform.position = transform.position;
                        Object.transform.parent = transform;
                        return Object.AddComponent<T>();
                    }
                    case true when gameObject.GetComponentInChildren<T>() != null:
                        return gameObject.GetComponentInChildren<T>();
                    case false when gameObject.GetComponentInChildren<T>() != null:
                        CoreUtils.Destroy(gameObject.GetComponentInChildren<T>().gameObject);
                        return null;
                    default: 
                        return null;
                }
            }
            else
            {
                T[] Array = FindObjectsByType<T>(FindObjectsSortMode.None);
                switch (isEnable)
                {
                    case true when Array.Length == 0:
                    {
                        GameObject Object = new GameObject(objectName);
                        return Object.AddComponent<T>();
                    }
                    case true when Array.Length != 0:
                    {
                        return Array[0];
                    }
                    case false when Array.Length != 0:
                    {
                        for (int i = 0; i < Array.Length; i++)
                        {
                            CoreUtils.Destroy(Array[i].gameObject);
                        }
                        // CoreUtils.Destroy(gameObject.GetComponentInChildren<T>().gameObject);
                        return null;
                    }
                    default: 
                        return null;
                }
            }

            
        }
        
        #endregion
        
    }
    
}