using System;
using System.IO;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif


namespace WorldSystem.Runtime
{
    public partial class CelestialBody
    {
        
        #region 枚举与帮助函数
        
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
            if (!property.lightComponent)
                return property.objectColorExecute;
            
            return Mathf.CorrelatedColorTemperatureToRGB(property.lightComponent.colorTemperature) * property.lightComponent.color;
        }
        
        #endregion

        
        #region Gizmos相关
        
#if UNITY_EDITOR
        private Color _iconColor;
        protected override void DrawGizmos()
        {
            float Strength01 = Vector3.Dot((transform.position - WorldManager.Instance.transform.position).normalized, Vector3.up);
            Strength01 = Math.Clamp(Strength01,0,1);
            
            if (property.lightComponent == null)
                _iconColor = Mathf.CorrelatedColorTemperatureToRGB(6500) * Color.white * Strength01;
            else
                _iconColor = Mathf.CorrelatedColorTemperatureToRGB(property.lightComponent.colorTemperature) * property.lightComponent.color * property.lightComponent.intensity;
            
            DrawDirectLine(_iconColor);
            if (property.type == ObjectType.Sun)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem/Textures/Icon/sun-icon.png", true, _iconColor);
            }
            if (property.type == ObjectType.Moon)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem/Textures/Icon/moon-icon.png", true, _iconColor);
            }
            if (property.type == ObjectType.Other)
            {
                Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem/Textures/Icon/other-icon.png",true, _iconColor);
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
            Handles.color = _iconColor * 0.8f;
            DrawUpperArc();
            Handles.color = _iconColor * 0.3f;
            DrawLowerArc();
            Handles.color = cache;
        }
        
        private void DrawDirectLine(Color color)
        {
            Color cache = Handles.color;
            Handles.color = color;
            Handles.DrawDottedLine(property.geocentricTheory.position, transform.position, 2f);
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
            Handles.DrawWireArc(center + property.geocentricTheory.position, normal, from, 180f, radius, 2f);
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
            Handles.DrawWireArc(center + property.geocentricTheory.position, normal, from, -180f, radius, 2f);
        }
#endif
        
        #endregion
        
    }

    
    [ExecuteAlways]
    public partial class CelestialBody : BaseModule
    {
        /// 备忘录! 
        ///  1- 现在每个星体都是一昼夜环绕一圈, 我希望有一个参数可以控制星体, 每N个昼夜环绕一圈
        ///  2- 我希望在灯光强度为零时, 禁用灯光组件
        ///  3- 给随机种加一个随机按钮
        ///  4- 需要一个参数来设置名字
        
        #region 字段

        [Serializable]
        public class Property
        {
            [LabelText("星体类型")] 
            public ObjectType type = ObjectType.Other;
            
            [ProgressBar(0f, 1f, ColorGetter = "gray")] [HideLabel]
            public float executeCoeff;
            
            [FoldoutGroup("基础")] [GradientUsage(true)] [GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("基础/objectColor", 0.9f, DisableAutomaticLabelWidth = true)]
            public Gradient objectColor = new();
            
            [HorizontalGroup("基础/objectColor")] [HideLabel] [ReadOnly]
            public Color objectColorExecute;
            
            [FoldoutGroup("配置")] [LabelText("地心学说 ! 小子 !")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Transform geocentricTheory;

            [FoldoutGroup("配置")] [LabelText("天体网格")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Mesh skyObjectMesh;

            [FoldoutGroup("配置")] [LabelText("着色器")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Shader shader;

            [FoldoutGroup("配置")] [LabelText("材质")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.celestialBodyManager?.hideFlags == HideFlags.None")]
            public Material material;

            [FoldoutGroup("基础")] [LabelText("角直径")] [Tooltip("角直径越大天体越大")] [GUIColor(1f,0.7f,1f)]
            [HorizontalGroup("基础/angularDiameterDegrees", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve angularDiameterDegrees = new( new Keyframe(0,2.35f), new Keyframe(1,2.35f));
            
            [HorizontalGroup("基础/angularDiameterDegrees")][HideLabel][ReadOnly]
            public float angularDiameterDegreesExecute;
            
            [FoldoutGroup("基础")] [LabelText("纹理")] [GUIColor(0.7f,0.7f,1f)]
            [Tooltip("如果未设置纹理对象仍渲染为圆形")]
            public Texture2D texture;

            [FoldoutGroup("位置")] [LabelText("轨道序列")] [Range(1, 10)] [GUIColor(0.7f,0.7f,1f)]
            [Tooltip("排序顺序较高的对象被认为离得更远,并且将在排序顺序较低的对象后面渲染")]
            public int sortOrder = 5;

            [FoldoutGroup("位置")] [LabelText("环绕偏移")] [GUIColor(0.7f,0.7f,1f)]
            public float orbitOffsetP;

            [FoldoutGroup("位置")] [LabelText("方向偏移")] [GUIColor(0.7f,0.7f,1f)]
            public float orientationOffset;

            [FoldoutGroup("位置")] [LabelText("倾角偏移")] [GUIColor(0.7f,0.7f,1f)]
            public float inclinationOffset;

            [FoldoutGroup("位置")] [LabelText("静止位置")] [GUIColor(0.7f,0.7f,1f)]
            [Tooltip("启用后,此天空对象的位置将在整个昼夜循环中保持不变")]
            public bool positionIsStatic;

            [FoldoutGroup("光照")] [LabelText("大气散射")] [GUIColor(1f,0.7f,0.7f)] [Tooltip("天体颜色渗入大气")]
            [HorizontalGroup("光照/falloff", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve falloff = new(new Keyframe(0, 0.5f), new Keyframe(1,0.5f));
            
            [HorizontalGroup("光照/falloff")][HideLabel][ReadOnly]
            public float falloffExecute;
            
            [FoldoutGroup("光照")] [LabelText("启用光照组件")] [GUIColor(0f,1f,0f)]
            public bool useLight;
            
            [FoldoutGroup("光照")] [LabelText("    光照过滤器")] [GUIColor(1f,0.7f,0.7f)] [ShowIf("useLight")]
            [HorizontalGroup("光照/lightingColorMask", 0.9f, DisableAutomaticLabelWidth = true)]
            public Gradient lightingColorMask = new() ;
            
            [HorizontalGroup("光照/lightingColorMask")] [ShowIf("useLight")] [HideLabel] [ReadOnly]
            public Color lightingColorMaskExecute;
            
            [FoldoutGroup("光照")] [LabelText("    色温曲线")] [GUIColor(1f,0.7f,0.7f)] [ShowIf("useLight")]
            [HorizontalGroup("光照/colorTemperatureCurve", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve colorTemperatureCurve =
                new(new Keyframe(0, 2000), new Keyframe(0.45f, 2000), new Keyframe(1f, 6500));
            
            [HorizontalGroup("光照/colorTemperatureCurve")] [ShowIf("useLight")] [HideLabel] [ReadOnly]
            public float colorTemperatureCurveExecute;
            
            [FoldoutGroup("光照")] [LabelText("    强度曲线")] [Tooltip("[0,0.5]地平线以下 [0.5,1.0]地平线之上")] [GUIColor(1f,0.7f,0.7f)] [ShowIf("useLight")]
            [HorizontalGroup("光照/intensityCurve", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve intensityCurve = new(new Keyframe(0, 0), new Keyframe(0.45f, 0f),
                new Keyframe(0.5f, 1f), new Keyframe(1f, 1.5f));
            
            [HorizontalGroup("光照/intensityCurve")] [ShowIf("useLight")] [HideLabel] [ReadOnly]
            public float intensityCurveExecute;
            
            [LabelText("灯光组件")] [InlineEditor] [ShowIf("useLight")]
            public Light lightComponent;
            
            [FoldoutGroup("光照")] [LabelText("启用镜头光斑")]
#if UNITY_EDITOR
            [InlineButton("OverlayLensFlareCommonFile", "覆盖LensFlareCommon.hlsl文件", ShowIf = "@!IsMatchInFile")]
#endif
            [GUIColor(0f,1f,0f)]
            public bool useLensFlare;
            
            [FoldoutGroup("光照")] [LabelText("    强度")] [GUIColor(1f,0.7f,0.7f)][ShowIf("useLensFlare")]
            [HorizontalGroup("光照/lensFlareStrength", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve lensFlareStrength;
            
            [HorizontalGroup("光照/lensFlareStrength")][ShowIf("useLensFlare")][HideLabel][ReadOnly]
            public float lensFlareStrengthExecute;
            
            [FoldoutGroup("光照")] [LabelText("    缩放")] [GUIColor(0.7f,0.7f,1f)][ShowIf("useLensFlare")]
            public float lensFlareScale;
            
            [InfoBox("文件: LensFlareCommon.hlsl中未检测到_CameraDepthTextureAddCloudMask! 镜头光晕无法与体积云交互! 请点击 [覆盖LensFlareCommon.hlsl文件] 按钮!", InfoMessageType.Error, "@!IsMatchInFile")]
            [FoldoutGroup("光照")] [LabelText("镜头光斑数据")][InlineEditor(InlineEditorObjectFieldModes.Hidden)] [ShowIf("useLensFlare")]
            public LensFlareDataSRP lensFlareData;
            
            [LabelText("镜头光斑组件")] [InlineEditor] [ShowIf("useLensFlare")]
            public LensFlareComponentSRP lensFlare;
            
            public void LimitProperty()
            {
                if (orbitOffsetP < 0f) orbitOffsetP = 360f;
                if (orbitOffsetP > 360f) orbitOffsetP = 0f;
                
                if (inclinationOffset > 90f) inclinationOffset = -90f;
                if (inclinationOffset < -90f) inclinationOffset = 90f;

                if (orientationOffset < 0f) orientationOffset = 360f;
                if (orientationOffset > 360f) orientationOffset = 0f;
            }

            public void ExecuteProperty()
            {
                if (!UseLerp)
                {
                    lensFlareStrengthExecute = lensFlareStrength?.Evaluate(executeCoeff) ?? 0;
                    objectColorExecute = objectColor?.Evaluate(executeCoeff) ?? Color.black;
                    falloffExecute = falloff?.Evaluate(executeCoeff) ?? 0;
                    lightingColorMaskExecute = lightingColorMask?.Evaluate(executeCoeff) ?? Color.black;
                    colorTemperatureCurveExecute = colorTemperatureCurve?.Evaluate(executeCoeff) ?? 0;
                    intensityCurveExecute = intensityCurve?.Evaluate(executeCoeff) ?? 0;
                }
                angularDiameterDegreesExecute = angularDiameterDegrees?.Evaluate(executeCoeff) ?? 0;
            }
            
#if UNITY_EDITOR
            public static bool IsMatchInFile;
            public void OverlayLensFlareCommonFile()
            {
                string SourcePath = PackageInfo.FindForPackageName("com.worldsystem").resolvedPath + @"\Packages~\Library\LensFlareCommon.hlsl";
                string TargetPath = PackageInfo.FindForPackageName("com.unity.render-pipelines.core").resolvedPath +
                                    @"\Runtime\PostProcessing\Shaders\LensFlareCommon.hlsl";
                try
                {
                    // 复制文件，并设置overwrite参数为true表示允许覆盖同名文件
                    File.Copy(SourcePath, TargetPath, true);
                    Debug.Log("LensFlareCommon.hlsl 文件已成功复制并覆盖至: " + TargetPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("复制文件时发生错误: " + ex.Message);
                }
                IsMatchInFile = HelpFunc.CheckForStringInFile(TargetPath, "_CameraDepthTextureAddCloudMask");
                AssetDatabase.ImportAsset(PackageInfo.FindForPackageName("com.unity.render-pipelines.core").assetPath + @"\Runtime\PostProcessing\Shaders\LensFlareCommon.hlsl");
            }
#endif
        }
        
        [HideLabel]
        public Property property = new();
        
        [HideInInspector] public Vector3 direction;
        
        [HideInInspector] public Vector3 positionRelative;

        private float _orbitOffset;
        
        public static bool UseLerp = false;
        
        private readonly int _MainTex = Shader.PropertyToID("_MainTex");
        private readonly int _Color = Shader.PropertyToID("_Color");

        #endregion
        
        
        
        #region 安装属性

        private void SetupStaticProperty()
        {
            //设置静态参数
            if (property.material == null || property.texture == null) return;
            property.material.SetTexture(_MainTex, property.texture);
            if(property.lensFlare != null)
                property.lensFlare.scale = property.lensFlareScale;
        }

        private void SetupDynamicProperty()
        {
            //设置动态参数
            property.material.SetColor(_Color, property.objectColorExecute);
            if(property.lensFlare is not null)
                property.lensFlare.intensity = property.lensFlareStrengthExecute;
        }
        
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
            if (property.geocentricTheory == null) property.geocentricTheory = gameObject.transform.parent;
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

        private void OnEnable_LensFlare()
        {
            if (gameObject.GetComponent<LensFlareComponentSRP>() == null)
            {
                property.lensFlare = gameObject.AddComponent<LensFlareComponentSRP>();
            }
            else
            {
                property.lensFlare = gameObject.GetComponent<LensFlareComponentSRP>();
            }
#if UNITY_EDITOR
            if (property.lensFlareData == null)
                property.lensFlareData =
                    AssetDatabase.LoadAssetAtPath<LensFlareDataSRP>(
                        "Packages/com.worldsystem/Shader/LensFlares/OasisSun_Flare.asset");
#endif
            property.lensFlare.lensFlareData = property.lensFlareData;

            property.lensFlare.radialScreenAttenuationCurve =
                new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
            property.lensFlare.useOcclusion = true;
            property.lensFlare.occlusionRadius = 0.5f;
            property.lensFlare.occlusionOffset = 300;
            property.lensFlare.sampleCount = 16;
#if UNITY_EDITOR
            EditorUtility.SetDirty(property.lensFlare);

            string TargetPath = PackageInfo.FindForPackageName("com.unity.render-pipelines.core").resolvedPath +
                                "/Runtime/PostProcessing/Shaders/LensFlareCommon.hlsl";
            Property.IsMatchInFile = HelpFunc.CheckForStringInFile(TargetPath, "_CameraDepthTextureAddCloudMask");
#endif
        }
        
        private void OnDisable_LensFlare()
        {
            if (gameObject.GetComponent<LensFlareComponentSRP>() != null)
            {
                CoreUtils.Destroy(gameObject.GetComponent<LensFlareComponentSRP>());
            }
            if(property.lensFlareData != null)
                Resources.UnloadAsset(property.lensFlareData);
            property.lensFlareData = null;
            property.lensFlare = null;
            
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
            
            if(property.useLensFlare)
                OnEnable_LensFlare();
            else
                OnDisable_LensFlare();
            
            SetupStaticProperty();
            
            WorldManager.Instance?.celestialBodyManager?.LensFlareOperations();
        }
        
        private void Update()
        {
            if (!(WorldManager.Instance?.celestialBodyManager?.update ?? false)) return;
            
            if (property.lightComponent)
            {
                property.lightComponent.enabled = property.angularDiameterDegreesExecute > 0.01;
            }
            
            UpdateRotations();
            
            UpdateLightProperties();
            
            property.ExecuteProperty();
            
            SetupDynamicProperty();
        }
        
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

            transform.position = property.geocentricTheory.position + c * Vector3.down * property.sortOrder;
            transform.LookAt(property.geocentricTheory, transform.up);
            
            positionRelative = transform.position - property.geocentricTheory.position;
            direction = positionRelative.normalized;
        }
        
        private void UpdateLightProperties()
        {
            float lightAngle = direction.y * 180f;
            property.executeCoeff = math.remap( -180f, 180f,0f,1f,lightAngle);
            
            if (!property.useLight || property.lightComponent is null)
                return;
            
            property.lightComponent.color =  property.lightingColorMaskExecute;
            property.lightComponent.intensity =  property.intensityCurveExecute;
            property.lightComponent.colorTemperature =  property.colorTemperatureCurveExecute;
        }

        
        #endregion

        
        #region 渲染函数
        
        public void RenderCelestialBody(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!isActiveAndEnabled || property.angularDiameterDegreesExecute < 1) return;
            
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(
                positionRelative + renderingData.cameraData.worldSpaceCameraPos,
                transform.rotation,
                Vector3.one * (Mathf.Tan(property.angularDiameterDegreesExecute * Mathf.Deg2Rad) * property.sortOrder)
            );
            cmd.DrawMesh(property.skyObjectMesh, m, property.material);
        }
        
        public void RenderCelestialBody(CommandBuffer cmd, Vector3 Position)
        {
            if (!isActiveAndEnabled || property.angularDiameterDegreesExecute < 1) return;
            
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(
                positionRelative + Position,
                transform.rotation,
                Vector3.one * (Mathf.Tan(property.angularDiameterDegreesExecute * Mathf.Deg2Rad) * property.sortOrder)
            );
            cmd.DrawMesh(property.skyObjectMesh, m, property.material);
        }
        
        #endregion


        
    }
    
}