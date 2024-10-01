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
    
    public partial class StarModule
    {
        private struct StarMeshProperties
        {
            public Matrix4x4 Mat;
            public Vector3 Color;
            public float Brightness;
            public float ID;
            public static int Size()
            {
                return sizeof(float) * 4 * 4
                       + // matrix
                       sizeof(float) * 3
                       + // color
                       sizeof(float)
                       + // brightness
                       sizeof(float); // id
            }
            
        }
        
        /// <summary>
        /// 获取星星色温,取[0,1]范围内的值，根据真实世界的恒星温度分布，返回[240040000]之间的色温。
        /// <br/>
        /// 基于VizieR:
        /// http://vizier.u-strasbg.fr/viz-bin/VizieR-4
        /// </summary>
        /// <param name="v01">介于0和1之间的值。像“概率”掩模一样用于对抗潜在的恒星温度算法。</param>
        /// <returns>Kelvins中[240040000]范围内的值</returns>
        public static float ComputeStarTemperature(float v01)
        {
            float EPSILON = 0.0001f;
            v01 = Mathf.Clamp(v01, EPSILON, 1.0f);
            v01 = 2400 * Mathf.Pow(v01, -0.378f);
            return Mathf.Clamp(v01, 2400, 40000);
        }

        public static float ComputeStarBrightness(float v01)
        {
            v01 = Mathf.Pow(100, v01) * 0.01f;
            return v01;
        }

        /// <summary>
        /// 获取黑体颜色,从范围[2400,40000]的输入色温（开尔文）返回黑体颜色（RGB范围[0,1]）。
        /// <br/>
        /// 我们使用一种r平方在[0.916，0.9966]范围内的新模型进行准确、低成本的黑体计算。
        /// <br/>
        /// 基于Mitchell Charity提供的黑体颜色源数据进行建模：
        /// http://www.vendian.org/mncharity/dir3/blackbody/UnstableURLs/bbr_color.html
        /// </summary>
        /// <param name="temperature">输入温度，单位为Kelvins。夹紧至[2400,400000]K</param>
        /// <returns>黑体颜色RGB值在[0,1]范围内。</returns>
        public static Vector3 ComputeBlackbodyColor(float temperature)
        {
            Vector3 col = Vector3.one;

            Vector2 VALID_RANGE = new Vector2(2400, 40000);
            temperature = Mathf.Clamp(temperature, VALID_RANGE.x, VALID_RANGE.y);

            // Handle Red
            // (R^2 = 0.943) across full range
            col.x = 74.4f * Mathf.Pow(temperature, -0.522f);

            // Handle Green
            if (temperature <= 6600)
            {
                // R^2 = 0.987
                col.y = 0.000146f * temperature + 0.0327f;
            }
            else if (temperature > 6600 && temperature < 10500)
            {
                // Roll off adjustment factor towards 10.5K
                col.y = 9.51f * Mathf.Pow(temperature, -0.283f) + HelpFunc.Remap(temperature, 6600, 10500, 0.2069f, 0);
            }
            else
            {
                // R^2 = 0.916
                col.y = 9.51f * Mathf.Pow(temperature, -0.283f);
            }

            // Handle Blue (R^2 = 0.998)
            // Returns 0.9966 at 6600
            if (temperature < 6600)
            {
                col.z = 0.000236f * temperature + -0.561f;
            }

            return HelpFunc.Saturate(col);
        }
        
    }


    [ExecuteAlways]
    public partial class StarModule : BaseModule
    {
        
        
        #region 字段
        
        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("星星网格")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Mesh starMesh;

            [FoldoutGroup("配置")] [LabelText("星星着色器")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Shader starShader;

            [FoldoutGroup("配置")] [LabelText("星星材质")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Material starMaterial;

            [FoldoutGroup("配置")] [LabelText("星星纹理")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Texture2D starTexture;

            [TitleGroup("数量与大小")] [LabelText("星星计数")]
            [GUIColor(0.7f,0.7f,1f)]
            public int count = 20000;

            [TitleGroup("数量与大小")] [LabelText("星星大小")]
            [GUIColor(1f,0.7f,0.7f)]
            public float size = 1;

            [TitleGroup("颜色与亮度")] [LabelText("自动亮度")] 
            [Tooltip("自动为每颗星星指定一种颜色,这些颜色是基于对可见恒星的科学测量")]
            [GUIColor(0.7f,0.7f,1f)]
            public bool automaticColor = true;

            [TitleGroup("颜色与亮度")] [LabelText("自动颜色")] 
            [Tooltip("自动为每颗星星指定亮度,这些亮度值是基于对可见恒星的科学测量")]
            [GUIColor(0.7f,0.7f,1f)]
            public bool automaticBrightness = true;

            [TitleGroup("颜色与亮度")] [LabelText("星星颜色")] [ColorUsage(false, true)]
            [GUIColor(0.7f,0.7f,1f)]
            public Color starColor = Color.white;

            [TitleGroup("颜色与亮度")] [LabelText("亮度")]
            [GUIColor(1f,0.7f,0.7f)]
            public float brightness = 1;

            [TitleGroup("闪烁")] [LabelText("闪烁频率")]
            [GUIColor(0.7f,0.7f,1f)]
            public float flickerFrequency = 20;

            [TitleGroup("闪烁")] [LabelText("闪烁强度")]
            [GUIColor(0.7f,0.7f,1f)]
            public float flickerStrength = 0.1f;

            [TitleGroup("位置")] [LabelText("随机种")]
            [GUIColor(0.7f,0.7f,1f)]
            public int initialSeed = 1;

            [TitleGroup("位置")] [LabelText("倾斜度")]
            [GUIColor(0.7f,0.7f,1f)]
            public float inclination;

            public void LimitProperty()
            {
                count = math.max(count, 1000);
                size = math.max(size, 0.2f);
                brightness = math.max(brightness, 0f);
                flickerFrequency = math.clamp(flickerFrequency, 0f,40f);
                flickerStrength = math.clamp(flickerStrength, 0.1f,1f);
                inclination = math.clamp(inclination, -180f,180f);
            }
        }
        
        [HideLabel]
        public Property property = new();

        [HideInInspector] public bool update;
        
        #endregion
        
        

        #region 安装参数
        
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _meshPropertiesBuffer;
        private readonly int altos_StarBuffer = Shader.PropertyToID("altos_StarBuffer");
        private readonly int _FlickerFrequency = Shader.PropertyToID("_FlickerFrequency");
        private readonly int _FlickerStrength = Shader.PropertyToID("_FlickerStrength");
        private readonly int _Inclination = Shader.PropertyToID("_Inclination");
        private readonly int _Star_MainTex = Shader.PropertyToID("_Star_MainTex");
        private readonly int _StarColor = Shader.PropertyToID("_StarColor");
        private readonly int _Brightness = Shader.PropertyToID("_Brightness");

        private void ComputeStarBuffers()
        {
            if (property.starMesh == null)
                return;

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = property.starMesh.GetIndexCount(0);
            args[1] = (uint)property.count;

            _argsBuffer?.Release();
            _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);

            // 使用给定的填充初始化缓冲区。
            StarModule.StarMeshProperties[] meshPropertiesArray = new StarModule.StarMeshProperties[property.count];
            UnityEngine.Random.InitState(property.initialSeed);
            for (int i = 0; i < property.count; i++)
            {
                StarModule.StarMeshProperties starMeshProperties = new StarModule.StarMeshProperties();
                Vector3 position = UnityEngine.Random.onUnitSphere * 100f;
                Quaternion rotation = Quaternion.LookRotation(Vector3.zero - position, UnityEngine.Random.onUnitSphere);
                Vector3 scale = Vector3.one * UnityEngine.Random.Range(1f, 2f) * 0.1f * property.size;

                starMeshProperties.Mat = Matrix4x4.TRS(position, rotation, scale);

                //设置星星的颜色
                if (property.automaticColor)
                {
                    float temperature = ComputeStarTemperature(UnityEngine.Random.Range(0f, 1f));
                    starMeshProperties.Color = ComputeBlackbodyColor(temperature);
                }
                else
                {
                    starMeshProperties.Color = new Vector3(1, 1, 1);
                }

                //设置星星的亮度
                if (property.automaticBrightness)
                {
                    starMeshProperties.Brightness = ComputeStarBrightness(UnityEngine.Random.Range(0f, 1f));
                }
                else
                {
                    starMeshProperties.Brightness = 1f;
                }


                starMeshProperties.ID = UnityEngine.Random.Range(0f, 1f);
                meshPropertiesArray[i] = starMeshProperties;
            }

            _meshPropertiesBuffer?.Release();
            _meshPropertiesBuffer = new ComputeBuffer(property.count, StarMeshProperties.Size());
            _meshPropertiesBuffer.SetData(meshPropertiesArray);
        }
        
        private void SetupStaticProperty()
        {
            Shader.SetGlobalBuffer(altos_StarBuffer, _meshPropertiesBuffer);
            Shader.SetGlobalFloat(_FlickerFrequency, property.flickerFrequency);
            Shader.SetGlobalFloat(_FlickerStrength, property.flickerStrength);
            Shader.SetGlobalFloat(_Inclination, -property.inclination);
            Shader.SetGlobalTexture(_Star_MainTex, property.starTexture);
            Shader.SetGlobalColor(_StarColor, property.starColor);
            
        }
        
        private void SetupDynamicProperty()
        {
            Shader.SetGlobalFloat(_Brightness, Mathf.Lerp(property.brightness, 0,
                WorldManager.Instance?.timeModule?.DaytimeFactor ?? 0.833333f));
        }
        
        #endregion

        
        
        #region 事件函数
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (property.starShader == null)
                property.starShader =
                    AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.worldsystem/Shader/Skybox/StarShader.shader");
            if (property.starTexture == null)
                property.starTexture =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        "Packages/com.worldsystem/Textures/Stars/star-texture-2@64px.png");
#endif
            if (property.starMesh == null) property.starMesh = HelpFunc.CreateQuad();
            if (property.starMaterial == null) property.starMaterial = CoreUtils.CreateEngineMaterial(property.starShader);

            OnValidate();
        }
        
        private void OnDisable()
        {
            if (property.starShader != null)
                Resources.UnloadAsset(property.starShader);
            if (property.starTexture != null)
                Resources.UnloadAsset(property.starTexture);
            if (property.starMesh != null)
                CoreUtils.Destroy(property.starMesh);
            if (property.starMaterial != null)
                CoreUtils.Destroy(property.starMaterial);

            _argsBuffer?.Release();
            _meshPropertiesBuffer?.Release();
            
            property.starShader = null;
            property.starTexture = null;
            property.starMesh = null;
            property.starMaterial = null;
            _argsBuffer = null;
            _meshPropertiesBuffer = null;
        }
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        public void OnValidate()
        {
            property.LimitProperty();
            ComputeStarBuffers();
            SetupStaticProperty();
        }
        

        
        private void Update()
        {
            if (!update) return;
            if (!isActiveAndEnabled || (8 < WorldManager.Instance?.timeModule?.CurrentTime && WorldManager.Instance?.timeModule?.CurrentTime < 16) ||
                property.brightness < 0.01f)
                return;
            
            SetupDynamicProperty();
        }

        #endregion
        
        
        
        #region 渲染函数
        
        public void RenderStar(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!isActiveAndEnabled || (8 < WorldManager.Instance?.timeModule?.CurrentTime && WorldManager.Instance?.timeModule?.CurrentTime < 16) ||
                property.brightness < 0.01f)
                return;
            
            //渲染星星
            cmd.DrawMeshInstancedIndirect(property.starMesh, 0, property.starMaterial, -1, _argsBuffer);
        }

        #endregion

        
    }


}