using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace WorldSystem.Runtime
{
    public partial class VFXLightningEffect
    {
        
        #region Gizmos相关
        
#if UNITY_EDITOR

        protected void DrawGizmos()
        {
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmosSelected();
        }

        protected void DrawGizmosSelected()
        {
            if (property.lightningDataObjectArray == null) return;
            foreach (var variable in property.lightningDataObjectArray)
            {
                if (variable != null && variable.activeSelf)
                {
                    Color cache = Gizmos.color;
                    Gizmos.color = new Color(1, 1, 1, 0.5f);
                    Gizmos.DrawLine(variable.transform.position, property.mainCamera.transform.position);
                    Gizmos.color = cache;
                }
            }
        }
#endif
        
        #endregion
        
    }


    [ExecuteAlways]
    public partial class VFXLightningEffect : VFXOutputEventAbstractHandler
    {
        
        
        #region 字段
        
        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("闪电数据预制件")]
            [ShowIf("@WorldManager.Instance?.weatherEffectModule?.hideFlags == HideFlags.None")]
            public GameObject prefab;

            [FoldoutGroup("配置")][LabelText("主摄像机")]
            [ShowIf("@WorldManager.Instance?.weatherEffectModule?.hideFlags == HideFlags.None")]
            public Camera mainCamera;

            [FoldoutGroup("配置")][LabelText("闪电照明")]
            [ShowIf("@WorldManager.Instance?.weatherEffectModule?.hideFlags == HideFlags.None")]
            public Light lightningLit;

            [FoldoutGroup("配置")][LabelText("闪电数据对象数组")]
            [ShowIf("@WorldManager.Instance?.weatherEffectModule?.hideFlags == HideFlags.None")]
            public GameObject[] lightningDataObjectArray = new GameObject[LightningData.MaxLightningDataCount];

            [FoldoutGroup("配置")][LabelText("闪电寿命数组")]
            [ShowIf("@WorldManager.Instance?.weatherEffectModule?.hideFlags == HideFlags.None")]
            public float[] lightningLifetimeArray = new float[LightningData.MaxLightningDataCount];

            [LabelText("光照强度")][GUIColor(0.7f,0.7f,1f)]
            public float lightningLightStrength = 2;

            [LabelText("闪电长度")][GUIColor(0.7f,0.7f,1f)]
            public float lightningLength = 2000;

            [LabelText("最小寿命")][GUIColor(0.7f,0.7f,1f)]
            public float lightningMinLifetime = 0.3f;

            [LabelText("最大寿命")][GUIColor(0.7f,0.7f,1f)]
            public float lightningMaxLifetime = 0.75f;

            [LabelText("繁殖率(频率)")][GUIColor(1f,0.7f,0.7f)]
            public float lightningSpawnRate;
            
            public void LimitProperty()
            {
                //限制属性
                lightningLightStrength = Math.Max(lightningLightStrength, 0);
                lightningLength =  Math.Max(lightningLength, 0);
                lightningMinLifetime = Math.Max(lightningMinLifetime, 0.1f);
                lightningMaxLifetime = Math.Max(lightningMaxLifetime, 0.3f);
                lightningSpawnRate = Math.Max(lightningSpawnRate, 0.0f);
            }
            
        }
        
        [HideLabel]
        public Property property = new();

        public static bool IsBeInLightning;
        public override bool canExecuteInEditor => true;
        
        [FormerlySerializedAs("_Update")] [HideInInspector] 
        public bool update;
        
        private float _previousTime;
        
        private Vector4[] _lightningArray = new Vector4[2];

        private static readonly int position = Shader.PropertyToID("position");
        private static readonly int lifetime = Shader.PropertyToID("lifetime");
        private readonly int SpawnRate = Shader.PropertyToID("Spawn Rate");
        private readonly int Length = Shader.PropertyToID("Length");
        private readonly int MinLifetime = Shader.PropertyToID("MinLifetime");
        private readonly int MaxLifetime = Shader.PropertyToID("MaxLifetime");
        private readonly int AltosLightningArray = Shader.PropertyToID("altos_LightningArray");
        private readonly int AltosLightningArraySize = Shader.PropertyToID("altos_LightningArraySize");
        
        #endregion
        
        
        
        #region 安装属性
        
        private void SetupStaticProperty()
        {
            if (property.lightningLit != null) property.lightningLit.intensity = property.lightningLightStrength;
            if (m_VisualEffect != null && m_VisualEffect.isActiveAndEnabled)
            {
                m_VisualEffect.SetFloat(Length, property.lightningLength);
                m_VisualEffect.SetFloat(MinLifetime, property.lightningMinLifetime);
                m_VisualEffect.SetFloat(MaxLifetime, property.lightningMaxLifetime);
            }
        }
        
        private void SetupDynamicProperty()
        {
            int lightningArraySize = 0;
            foreach (LightningData data in LightningData.LightningDataHashList)
            {
                _lightningArray[lightningArraySize] = new Vector4(data.Position.x, data.Position.y,
                    data.Position.z, data.Intensity);
                lightningArraySize++;
            }
            Shader.SetGlobalVectorArray(AltosLightningArray, _lightningArray);
            Shader.SetGlobalInt(AltosLightningArraySize, lightningArraySize);
            m_VisualEffect?.SetFloat(SpawnRate, property.lightningSpawnRate);
        }
        
        private void ClearDynamicLightningProperty()
        {
            Shader.SetGlobalInt(AltosLightningArraySize, 0);
        }
        
        
        #endregion
        
        
        
        #region 事件函数
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if(gameObject.GetComponent<BaseModule>() == null)
                gameObject.AddComponent<BaseModule>();
            property.mainCamera = Camera.main;
            
            //安装数据
#if UNITY_EDITOR
            if (m_VisualEffect.visualEffectAsset == null)
                m_VisualEffect.visualEffectAsset =
                    AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(
                        "Packages/com.worldsystem//Visual Effects/Lightning.vfx");

            property.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Packages/com.worldsystem//Prefaces/Light_Lightning Point Light Prefab.prefab");
#endif
            
            if (m_VisualEffect != null)
                m_VisualEffect.enabled = true;
            
            //创建闪电数据数组
            CreateLightningDataObjectArray();
            
            //创建闪电灯光
            if (gameObject.GetComponentInChildren<Light>(true) == null)
            {
                GameObject lightningObject = new GameObject("LightningLight");
                lightningObject.transform.position = transform.position;
                lightningObject.transform.parent = transform;
                property.lightningLit = lightningObject.AddComponent<Light>();
                property.lightningLit.type = LightType.Directional;
                property.lightningLit.intensity = property.lightningLightStrength;
                property.lightningLit.renderMode = LightRenderMode.Auto;
                property.lightningLit.shadows = LightShadows.Hard;
                property.lightningLit.enabled = false;
            }
            else
            {
                property.lightningLit = gameObject.GetComponentInChildren<Light>(true);
                property.lightningLit.enabled = false;
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            property.mainCamera = null;
            if(gameObject.GetComponent<BaseModule>() != null && gameObject.activeSelf && Time.frameCount != 0)
                CoreUtils.Destroy(gameObject.GetComponent<BaseModule>());
            
            //卸载数据
            if (m_VisualEffect.visualEffectAsset != null)
            {
                Resources.UnloadAsset(m_VisualEffect.visualEffectAsset);
                m_VisualEffect.visualEffectAsset = null;
            }

            property.prefab = null;
            
            if (m_VisualEffect != null)
                m_VisualEffect.enabled = false;
            
            DestroyLightningDataObjectArray();
            
            if(gameObject.GetComponentInChildren<Light>(true) != null)
                CoreUtils.Destroy(gameObject.GetComponentInChildren<Light>(true).gameObject);
            property.lightningLit = null;
        }
        
        public void OnValidate()
        {
            property.LimitProperty();
            SetupStaticProperty();
        }
        
        void Update()
        {
            if (!update) return;

#if UNITY_EDITOR
            if (property.lightningDataObjectArray[0] == null ||
                property.lightningDataObjectArray[property.lightningDataObjectArray.Length - 1] == null)
            {
                LightningData[] LightningDataObjectArray = GetComponentsInChildren<LightningData>(true);
                GameObject[] LightningData = new GameObject[LightningDataObjectArray.Length];
                for (var index = 0; index < LightningDataObjectArray.Length; index++)
                {
                    LightningData[index] = LightningDataObjectArray[index].gameObject;
                }
                property.lightningDataObjectArray = LightningData;
            }
#endif
            
            if (property.lightningSpawnRate > 0.1) 
                m_VisualEffect.enabled = true;
            else
            {
                 property.lightningLit.enabled = false;
                 m_VisualEffect.enabled = false;
                 IsBeInLightning = false;
                 ClearDynamicLightningProperty();
                 return;
            }
            
            if (WorldManager.Instance?.volumeCloudOptimizeModule is not null)
            {
                var vector3 = transform.position;
                vector3.y = WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionCloudThickness * 150 / 1000 +
                            WorldManager.Instance.volumeCloudOptimizeModule.property.modelingPositionCloudHeight;
                transform.position = vector3;
            }

            var dt = Time.time - _previousTime;
            for (int i = 0; i < property.lightningDataObjectArray.Length; i++)
            {
                // 非时间管理的负无穷大
                if (float.IsNegativeInfinity(property.lightningLifetimeArray[i]))
                    continue;

                // 否则，管理时间
                if (property.lightningLifetimeArray[i] <= 0.0f && property.lightningDataObjectArray[i].activeSelf)
                {
                    property.lightningDataObjectArray[i].SetActive(false);
                    property.lightningLifetimeArray[i] = float.NegativeInfinity;
                    ClearDynamicLightningProperty();
                    property.lightningLit.enabled = false;
                    IsBeInLightning = false;
                }
                else
                {
                    property.lightningLifetimeArray[i] -= dt;
                    IsBeInLightning = true;
                }
            }
            _previousTime = Time.time;
        }
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        public override void OnVFXOutputEvent(VFXEventAttribute eventAttribute)
        {
            int availableInstanceId = -1;
            for (int i = 0; i < property.lightningDataObjectArray.Length; i++)
            {
                if (!property.lightningDataObjectArray[i].activeSelf)
                {
                    availableInstanceId = i;
                    break;
                }
            }
        
            if (availableInstanceId != -1)
            {
                var availableInstance = property.lightningDataObjectArray[availableInstanceId];
                availableInstance.SetActive(true);
                if (eventAttribute.HasVector3(position))
                {
                    availableInstance.transform.localPosition = eventAttribute.GetVector3(position);
                    //旋转闪电照明并激活
                    if (property.mainCamera != null) property.lightningLit.transform.forward = (property.mainCamera.transform.position - availableInstance.transform.position).normalized;
                    float dis = (property.mainCamera.transform.position - availableInstance.transform.position).magnitude;
                    property.lightningLit.intensity = property.lightningLightStrength * (float)Math.Pow(Math.E, -dis / 3000);
                    property.lightningLit.enabled = true;
                }
        
                if (eventAttribute.HasFloat(lifetime)) property.lightningLifetimeArray[availableInstanceId] = eventAttribute.GetFloat(lifetime);
        
                SetupDynamicProperty();
            }
        }
        
        #endregion
        
        
        
        #region 重要函数
        
        void CreateLightningDataObjectArray()
        {
            if (property.prefab == null) return;
            
            //如果有LightningData的残留,则将其销毁
            if (GetComponentsInChildren<LightningData>(true).Length > 0)
                DestroyLightningDataObjectArray();

            property.lightningDataObjectArray = new GameObject[LightningData.MaxLightningDataCount];
            property.lightningLifetimeArray = new float[LightningData.MaxLightningDataCount];

            for (int i = 0; i < LightningData.MaxLightningDataCount; i++)
            {
                GameObject newInstance = Instantiate(property.prefab, transform);
                newInstance.name = $"{name} - #{i} - {property.prefab.name}";
                newInstance.SetActive(false);
                newInstance.hideFlags = HideFlags.None;

                property.lightningDataObjectArray[i] = newInstance;
                property.lightningLifetimeArray[i] = float.NegativeInfinity;
            }
        }
        
        void DestroyLightningDataObjectArray()
        {
            foreach (var instance in GetComponentsInChildren<LightningData>(true))
            {
                CoreUtils.Destroy(instance.gameObject);
            }

            property.lightningDataObjectArray = null;
            property.lightningLifetimeArray = null;
            
        }
        
        
        #endregion



    }

    
}