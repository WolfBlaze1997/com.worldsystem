using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
        [ShowIf("rainEnable")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块已开启"), GUIColor(0.5f, 0.5f, 1f)]
        public void RainToggle_Off()
        {
            rainEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("rainEnable")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "雨模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void RainToggle_On()
        {
            rainEnable = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("snowEnable")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Medium, Name = "雪模块已开启"), GUIColor(0.5f, 0.5f, 1f)]
        public void SnowToggle_Off()
        {
            snowEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("snowEnable")]
        [VerticalGroup("Split/02")]
        [Button(ButtonSizes.Medium, Name = "雪模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void SnowToggle_On()
        {
            snowEnable = true;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [ShowIf("lightningEnable")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Medium, Name = "闪电模块已开启"), GUIColor(0.5f, 0.5f, 1f)]
        public void LightningToggle_Off()
        {
            lightningEnable = false;
            OnValidate();
        }
        [PropertyOrder(-100)]
        [HideIf("lightningEnable")]
        [VerticalGroup("Split/03")]
        [Button(ButtonSizes.Medium, Name = "闪电模块已关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void LightningToggle_On()
        {
            lightningEnable = true;
            OnValidate();
        }
        
        
#endif
    }
    
    
    [ExecuteAlways]
    public partial class WeatherEffectModule : BaseModule
    {
        
        #region 字段
        [HideInInspector]
        public Camera mainCamera;
        [LabelText("使用遮蔽")][GUIColor(0, 1, 0)]
        public bool useOcclusion;
        
        [LabelText("范围半径")][GUIColor(0.7f,0.7f,1f)]
        public float effectRadius = 40;
        
        [HideInInspector]
        public bool rainEnable;
        [FoldoutGroup("雨")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("rainEnable")]
        public VFXRainEffect rainEffect;
        
        [HideInInspector]
        public bool snowEnable;
        [FoldoutGroup("雪")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("snowEnable")]
        public VFXSnowEffect snowEffect;
        
        [HideInInspector]
        public bool lightningEnable;
        [FoldoutGroup("闪电")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("lightningEnable")]
        public VFXLightningEffect lightningEffect;
        
        [FoldoutGroup("遮蔽渲染器")][InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)][ShowIf("useOcclusion")]
        public OcclusionRenderer occlusionRenderer;
        
        #endregion

        
        #region 限制属性/安装属性

        private void LimitProperty()
        {
            effectRadius = Math.Max(effectRadius, 10);
        }
        
        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_PrecipitationGlobal, Math.Max(rainEffect?.property?.rainPrecipitation ?? 0, snowEffect?.property?.snowPrecipitation ?? 0));
        }
        private readonly int _PrecipitationGlobal = Shader.PropertyToID("_PrecipitationGlobal");

        public void SetupCommonDynamicProperty( VisualEffect effect)
        {
            if (WorldManager.Instance?.windZoneModule is not null)
            {
                effect.SetVector3(Dynamic_WindZone,
                    WorldManager.Instance.windZoneModule.property.vfxWindData.direction *
                    WorldManager.Instance.windZoneModule.property.vfxWindData.speed);
            }
            
            effect.SetFloat(Dynamic_DaytimeFactor, WorldManager.Instance?.timeModule?.DaytimeFactor ?? 0.83333f);
            
            //遮蔽参数
            if (useOcclusion && occlusionRenderer?.occlusionCamera is not null)
            {
                effect.SetBool(Dynamic_PrecipitationDepthEnabled, useOcclusion);//静态
                
                if(occlusionRenderer.occlusionCamera.targetTexture is not null)
                    effect.SetTexture(Dynamic_PrecipitationDepthBufferTexture,
                        occlusionRenderer.occlusionCamera.targetTexture);
                
                effect.SetVector2(Dynamic_PrecipitationDepthCameraClipPlanes,
                    new Vector2(occlusionRenderer.occlusionCamera.nearClipPlane,
                        occlusionRenderer.occlusionCamera.farClipPlane));
                effect.SetMatrix4x4(Dynamic_PrecipitationDepthWorldToCameraMatrix,
                    occlusionRenderer.occlusionCamera.worldToCameraMatrix);
                effect.SetFloat(Dynamic_PrecipitationDepthCameraOrthographicSize,
                    occlusionRenderer.occlusionCamera.orthographicSize);
            }
        }
        private readonly int Dynamic_WindZone = Shader.PropertyToID("Dynamic_WindZone");
        private readonly int Dynamic_DaytimeFactor = Shader.PropertyToID("Dynamic_DaytimeFactor");
        private readonly int Dynamic_PrecipitationDepthEnabled = Shader.PropertyToID("Dynamic_PrecipitationDepthEnabled");
        private readonly int Dynamic_PrecipitationDepthBufferTexture = Shader.PropertyToID("Dynamic_PrecipitationDepthBufferTexture");
        private readonly int Dynamic_PrecipitationDepthCameraClipPlanes = Shader.PropertyToID("Dynamic_PrecipitationDepthCameraClipPlanes");
        private readonly int Dynamic_PrecipitationDepthWorldToCameraMatrix = Shader.PropertyToID("Dynamic_PrecipitationDepthWorldToCameraMatrix");
        private readonly int Dynamic_PrecipitationDepthCameraOrthographicSize = Shader.PropertyToID("Dynamic_PrecipitationDepthCameraOrthographicSize");
        
        #endregion
        
        
        #region 事件函数
        
        private void OnEnable()
        {
            mainCamera = Camera.main;
            OnValidate();
        }
        
        private void OnDestroy()
        {
            rainEnable = false;
            snowEnable = false;
            lightningEnable = false;
            useOcclusion = false;
            OnValidate();
            
            mainCamera = null;
        }
        
        public void OnValidate()
        {
            LimitProperty();
                
            //根据情况创建或销毁天气效果
            snowEffect = CreateOrDestroyEffect<VFXSnowEffect>("VFXSnowEffect",snowEnable);
            rainEffect = CreateOrDestroyEffect<VFXRainEffect>("VFXRainEffect",rainEnable);
            lightningEffect = CreateOrDestroyEffect<VFXLightningEffect>("VFXLightningEffect",lightningEnable);
            occlusionRenderer = CreateOrDestroyEffect<OcclusionRenderer>("OcclusionRenderer",useOcclusion);
            
            //统一半径
            if (rainEffect != null) rainEffect.property.rainRadius = effectRadius;
            if (snowEffect != null) snowEffect.property.snowRadius = effectRadius;
            if (occlusionRenderer != null) occlusionRenderer.effectRadius = effectRadius;
        }
        
        
        
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
            //固定位置到MainCamera上方
            if (mainCamera is not null)
                transform.position = mainCamera.transform.position + new Vector3(0,effectRadius * 0.6f,0);
            
            SetupDynamicProperty();
        }
        
        
        
        #endregion

        
        #region 重要函数

        private T CreateOrDestroyEffect<T>(string objectName, bool isEnable) where T : MonoBehaviour
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
        
        #endregion
        
    }
    
}