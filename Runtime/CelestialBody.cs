using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// ReSharper disable InconsistentNaming

namespace WorldSystem.Runtime
{
    public partial class CelestialBody
    {
        public enum ObjectType
        {
            Sun,
            Moon,
            Other
        }
        
        private void CreateOrDestroyLightComponent()
        {
            if (property.useLight)
            {
                if (gameObject.GetComponent<Light>() != null)
                {
                    property.lightComponent = gameObject.GetComponent<Light>();
                }
                else
                {
                    property.lightComponent = gameObject.AddComponent<Light>();
                    property.lightComponent.type = LightType.Directional;
                    property.lightComponent.useColorTemperature = true;
                }
            }
            else
            {
                if (gameObject.GetComponent<Light>() == null) return;
                CoreUtils.Destroy(transform.gameObject.GetComponent<UniversalAdditionalLightData>());
                CoreUtils.Destroy(transform.gameObject.GetComponent<Light>());
                property.lightComponent = null;
            }
        }

        public Color GetAtmosphereScatterColor()
        {
            if (property.lightComponent is null)
                return objectColorEvaluate;

            return Mathf.CorrelatedColorTemperatureToRGB(property.lightComponent.colorTemperature) * property.lightComponent.color;
        }

        
#if UNITY_EDITOR
        
        private Color iconColor;
        protected override void DrawGizmos()
        {
            float Strength01 = (float)Vector3.Dot((transform.position - WorldManager.Instance.transform.position).normalized, Vector3.up);
            Strength01 = Math.Clamp(Strength01,0,1);
            
            if (property.lightComponent == null)
                iconColor = Mathf.CorrelatedColorTemperatureToRGB(6500) * Color.white * Strength01;
            else
                iconColor = Mathf.CorrelatedColorTemperatureToRGB(property.lightComponent.colorTemperature) * property.lightComponent.color * property.lightComponent.intensity;
            
            DrawDirectLine(iconColor);
            if (property.type == ObjectType.Sun)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem//Textures/Icon/sun-icon.png", true, iconColor);
            }
            if (property.type == ObjectType.Moon)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem//Textures/Icon/moon-icon.png", true, iconColor);
            }
            if (property.type == ObjectType.Other)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem//Textures/Icon/other-icon.png",true, iconColor);
            }
            
            Color Cache = Handles.color;
            Handles.color = new Color(0, 0, 0, 0.2f);
            Handles.DrawSolidDisc(WorldManager.Instance.transform.position,WorldManager.Instance.transform.up, property.sortOrder);
            Handles.color = Cache;
            
            Color Cache01 = Gizmos.color;
            Gizmos.color = new Color(0, 0, 0, 0.2f);
            
            if(WorldManager.Instance?.universeBackgroundModule?.property.skyMesh != null)
                Gizmos.DrawMesh(WorldManager.Instance.universeBackgroundModule.property.skyMesh,WorldManager.Instance.transform.position, 
                Quaternion.identity, new Vector3(property.sortOrder, property.sortOrder, property.sortOrder) * 2);
            Gizmos.color = Cache01;
            
        }

        protected override void DrawGizmosSelected()
        {
            
            Color cache = Handles.color;
            Handles.color = iconColor * 0.8f;
            DrawUpperArc();
            Handles.color = iconColor * 0.3f;
            DrawLowerArc();
            Handles.color = cache;
        }
        
        private void DrawDirectLine(Color color)
        {
            Color cache = Handles.color;
            Handles.color = color;
            UnityEditor.Handles.DrawDottedLine(property.GeocentricTheory.position, transform.position, 2f);
            Handles.color = cache;

        }
        
        private void DrawUpperArc()
        {
            Vector3 center = new Vector3(0, 0, property.sortOrder * Mathf.Sin(Mathf.Deg2Rad * property.inclinationOffset));
            Quaternion rot = Quaternion.Euler(0, property.orientationOffset, 0);
            center = rot * center;
            Vector3 normal = Vector3.forward;
            normal = rot * normal;
            Vector3 from = Vector3.right;
            from = rot * from;
            float radius = property.sortOrder * Mathf.Cos(Mathf.Deg2Rad * property.inclinationOffset);
            UnityEditor.Handles.DrawWireArc(center + property.GeocentricTheory.position, normal, from, 180f, radius, 2f);
        }

        private void DrawLowerArc()
        {
            Vector3 center = new Vector3(0, 0, property.sortOrder * Mathf.Sin(Mathf.Deg2Rad * property.inclinationOffset));
            Quaternion rot = Quaternion.Euler(0, property.orientationOffset, 0);
            center = rot * center;
            Vector3 normal = Vector3.forward;
            normal = rot * normal;
            Vector3 from = Vector3.right;
            from = rot * from;
            float radius = property.sortOrder * Mathf.Cos(Mathf.Deg2Rad * property.inclinationOffset);
            UnityEditor.Handles.DrawWireArc(center + property.GeocentricTheory.position, normal, from, -180f, radius, 2f);
        }
        
#endif
        
    }

    [ExecuteAlways]
    public partial class CelestialBody : BaseModule
    {
        /// 备忘录! 
        ///  1- 现在每个星体都是一昼夜环绕一圈, 我希望有一个参数可以控制星体, 每N个昼夜环绕一圈
        ///  2- 我希望在灯光强度为零时, 禁用灯光组件
        ///  3- 给随机种加一个随机按钮
        ///  4- 需要一个参数来设置名字
        ///
        ///  5- 
        ///

        #region 字段

        [Serializable]
        public class Property
        {
            
            [LabelText("星体类型")] 
            public ObjectType type = ObjectType.Other;
            
            [FoldoutGroup("基础")]
            [GradientUsage(true)]
            [GUIColor(1f,0.7f,0.7f)]
            public Gradient objectColor = new();
            
            [FoldoutGroup("配置")] [LabelText("地心学说 ! 小子 !")] 
            [Sirenix.OdinInspector.ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Transform GeocentricTheory;

            [FoldoutGroup("配置")] [LabelText("天体网格")] 
            [Sirenix.OdinInspector.ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Mesh skyObjectMesh;

            [FoldoutGroup("配置")] [LabelText("着色器")] 
            [Sirenix.OdinInspector.ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Shader shader;

            [FoldoutGroup("配置")] [LabelText("材质")] 
            [Sirenix.OdinInspector.ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Material material;

            [FoldoutGroup("基础")] [LabelText("角直径")]
            [Tooltip("角直径越大天体越大")]
            [GUIColor(1f,0.7f,1f)]
            public AnimationCurve angularDiameterDegrees = new( new Keyframe(0,2.35f), new Keyframe(1,2.35f));

            [FoldoutGroup("基础")] [LabelText("纹理")]
            [Tooltip("如果未设置纹理对象仍渲染为圆形")]
            [GUIColor(0.7f,0.7f,1f)]
            public Texture2D texture;

            [FoldoutGroup("位置")] [LabelText("轨道序列")]  [Range(1, 10)]
            [Tooltip("排序顺序较高的对象被认为离得更远,并且将在排序顺序较低的对象后面渲染")]
            [GUIColor(0.7f,0.7f,1f)]
            public int sortOrder = 5;

            [FoldoutGroup("位置")] [LabelText("环绕偏移")]
            [GUIColor(0.7f,0.7f,1f)]
            public float orbitOffsetP = 0f;

            [FoldoutGroup("位置")] [LabelText("方向偏移")]
            [GUIColor(0.7f,0.7f,1f)]
            public float orientationOffset = 0f;

            [FoldoutGroup("位置")] [LabelText("倾角偏移")]
            [GUIColor(0.7f,0.7f,1f)]
            public float inclinationOffset = 0f;

            [FoldoutGroup("位置")] [LabelText("静止位置")] 
            [Tooltip("启用后,此天空对象的位置将在整个昼夜循环中保持不变")][GUIColor(0.7f,0.7f,1f)]
            public bool positionIsStatic = false;

            [FoldoutGroup("光照")] [LabelText("大气散射")] 
            [Tooltip("天体颜色渗入大气")]
            [GUIColor(1f,0.7f,0.7f)]
            public AnimationCurve falloff = new(new Keyframe(0, 0.5f), new Keyframe(1,0.5f));

            [FoldoutGroup("光照")] [LabelText("启用光照组件")]
            [GUIColor(0.7f,0.7f,1f)]
            public bool useLight;
            
            [FoldoutGroup("光照")] [LabelText("    光照过滤器")]
            [GUIColor(1f,0.7f,0.7f)]
            [ShowIf("useLight")]
            public Gradient lightingColorMask = new() ;

            [FoldoutGroup("光照")] [LabelText("    色温曲线")]
            [GUIColor(1f,0.7f,0.7f)]
            [ShowIf("useLight")]
            public AnimationCurve colorTemperatureCurve =
                new(new Keyframe(0, 2000), new Keyframe(0.45f, 2000), new Keyframe(1f, 6500));

            [FoldoutGroup("光照")] [LabelText("    强度曲线")]
            [Tooltip("[0,0.5]地平线以下 [0.5,1.0]地平线之上")]
            [GUIColor(1f,0.7f,0.7f)]
            [ShowIf("useLight")]
            public AnimationCurve intensityCurve = new(new Keyframe(0, 0), new Keyframe(0.45f, 0f),
                new Keyframe(0.5f, 1f), new Keyframe(1f, 1.5f));

            [LabelText("灯光组件")] [InlineEditor] 
            [ShowIf("useLight")]
            public Light lightComponent;

            public void LimitProperty()
            {
                if (orbitOffsetP < 0f) orbitOffsetP = 360f;
                if (orbitOffsetP > 360f) orbitOffsetP = 0f;
                
                if (inclinationOffset > 90f) inclinationOffset = -90f;
                if (inclinationOffset < -90f) inclinationOffset = 90f;

                if (orientationOffset < 0f) orientationOffset = 360f;
                if (orientationOffset > 360f) orientationOffset = 0f;
            }
        }
        
        [HideLabel]
        public Property property = new();
        

        #endregion
        
        
        [HideInInspector] public Vector3 direction;
        [HideInInspector] public Vector3 positionRelative;
        
        private float _orbitOffset = 0f;
        [HideInInspector] public float curveTime;
        [HideInInspector] public Color objectColorEvaluate;
        [HideInInspector] public float angularDiameterDegreesEvaluate;
        [HideInInspector] public float falloffEvaluate;
        [HideInInspector] public Color lightingColorMaskEvaluate;
        [HideInInspector] public float colorTemperatureCurveEvaluate;
        [HideInInspector] public float intensityCurveEvaluate;
        
        public static bool useLerp = false;


        #region 安装属性

        private void SetupStaticProperty()
        {
            //设置静态参数
            if (property.material == null || property.texture == null) return;
            property.material.SetTexture(_MainTex, property.texture);
        }
        private readonly int _MainTex = Shader.PropertyToID("_MainTex");

        private void SetupDynamicProperty()
        {
            //设置动态参数
            property.material.SetColor(_Color, objectColorEvaluate);
        }
        private readonly int _Color = Shader.PropertyToID("_Color");

        
        #endregion
        
        
        
        #region 事件函数
        public void OnEnable()
        {
#if UNITY_EDITOR
            if (property.shader == null)
                property.shader = AssetDatabase.LoadAssetAtPath<Shader>(
                    "Packages/com.worldsystem//Shader/Skybox/SkyObjectShader.shader");
            if (property.texture == null)
                property.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Packages/com.worldsystem//Textures/Sun/SunTex_16x16.png");
#endif
            if (property.GeocentricTheory == null) property.GeocentricTheory = gameObject.transform.parent;
            if (property.skyObjectMesh == null) property.skyObjectMesh = HelpFunc.CreateQuad();
            if (property.material == null) property.material = CoreUtils.CreateEngineMaterial(property.shader);
            
            OnValidate();
        }
        private void OnDestroy()
        {
            if (property.skyObjectMesh != null)
                CoreUtils.Destroy(property.skyObjectMesh);
            if (property.shader != null)
                Resources.UnloadAsset(property.shader);
            if (property.material != null)
                CoreUtils.Destroy(property.material);
            if (property.texture != null)
                Resources.UnloadAsset(property.texture);

            property.skyObjectMesh = null;
            property.shader = null;
            property.material = null;
            property.texture = null;
        }
        public void OnValidate()
        {
            property.LimitProperty();
            
            _orbitOffset = property.type == ObjectType.Moon ? property.orbitOffsetP + 180 : property.orbitOffsetP;
            gameObject.name = property.type switch
            {
                ObjectType.Sun => "SUN",
                ObjectType.Moon => "MOON",
                _ => "CelestialBody",
            };
            
            CreateOrDestroyLightComponent();
            
            SetupStaticProperty();
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
        private void UpdateRotations()
        {
            Quaternion a = Quaternion.Euler(-property.inclinationOffset, 0, 0);

            float timeOfDayOffset = 0f;
            if (!property.positionIsStatic)
            {
                timeOfDayOffset = (WorldManager.Instance?.timeModule?.CurrentTime ?? 10) * 15;
            }

            Quaternion b = Quaternion.Euler(0, 0, _orbitOffset + timeOfDayOffset) * a;
            Quaternion c = Quaternion.Euler(0, property.orientationOffset, 0) * b;

            transform.position = property.GeocentricTheory.position + c * Vector3.down * property.sortOrder;
            transform.LookAt(property.GeocentricTheory, transform.up);
            
            positionRelative = transform.position - property.GeocentricTheory.position;
            direction = positionRelative.normalized;
        }
        
        private void UpdateLightProperties()
        {
            if (!property.useLight || property.lightComponent is null)
                return;
            
            float lightAngle = direction.y * 180f;
            curveTime = math.remap( -180f, 180f,0f,1f,lightAngle);

            property.lightComponent.color = lightingColorMaskEvaluate;
            property.lightComponent.intensity = intensityCurveEvaluate;
            property.lightComponent.colorTemperature = colorTemperatureCurveEvaluate;
        }

        void UpdateFunc()
        {
            UpdateRotations();
            UpdateLightProperties();

            if (!useLerp)
            {
                objectColorEvaluate = property.objectColor.Evaluate(curveTime);
                falloffEvaluate = property.falloff.Evaluate(curveTime);
                lightingColorMaskEvaluate = property.lightingColorMask.Evaluate(curveTime);
                colorTemperatureCurveEvaluate = property.colorTemperatureCurve.Evaluate(curveTime);
                intensityCurveEvaluate = property.intensityCurve.Evaluate(curveTime);
            }
            angularDiameterDegreesEvaluate = property.angularDiameterDegrees.Evaluate(curveTime);
            
            SetupDynamicProperty();
        }
        
        #endregion

        
        #region 渲染函数
        public void RenderCelestialBody(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!isActiveAndEnabled || angularDiameterDegreesEvaluate < 1) return;
            
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(
                positionRelative + renderingData.cameraData.worldSpaceCameraPos,
                transform.rotation,
                Vector3.one * (Mathf.Tan(angularDiameterDegreesEvaluate * Mathf.Deg2Rad) * property.sortOrder)
            );
            cmd.DrawMesh(property.skyObjectMesh, m, property.material);

        }



        #endregion


        
    }
    
}