using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace WorldSystem.Runtime
{
    public partial class VFXRainSpatterEffect
    {
        
        #region 帮助函数
        
        private Mesh CreateQuadMesh(float edgeLength = 2)
        {
            // 创建一个新的Mesh  
            Mesh mesh = new Mesh();

            mesh.name = "TemporaryQuad";
            float edgeLengthHalf = edgeLength * 0.5f;
            // 设置顶点数据  
            // 我们需要四个顶点来形成一个四边形  
            Vector3[] vertices = new Vector3[4];  
            vertices[0] = new Vector3(-edgeLengthHalf, 0, -edgeLengthHalf); // 左下角  
            vertices[1] = new Vector3(edgeLengthHalf, 0, -edgeLengthHalf); // 右下角  
            vertices[2] = new Vector3(edgeLengthHalf, 0, edgeLengthHalf);  // 右上角  
            vertices[3] = new Vector3(-edgeLengthHalf, 0, edgeLengthHalf); // 左上角  
  
            // 设置UV坐标（用于纹理映射）  
            Vector2[] uvs = new Vector2[4];  
            uvs[0] = new Vector2(0, 0);  
            uvs[1] = new Vector2(1, 0);  
            uvs[2] = new Vector2(1, 1);  
            uvs[3] = new Vector2(0, 1);  
  
            // 设置三角形（两个三角形组成一个四边形）  
            // 注意，三角形的顶点顺序决定了面的朝向  
            int[] triangles = new int[6];  
            triangles[0] = 0; triangles[1] = 1; triangles[2] = 2; // 第一个三角形  
            triangles[3] = 0; triangles[4] = 2; triangles[5] = 3; // 第二个三角形  
  
            // 将数据应用到Mesh  
            mesh.vertices = vertices;  
            mesh.uv = uvs;  
            mesh.triangles = triangles;  
            
            return mesh;
        }
        
        #endregion

    }
    
    
    [ExecuteAlways]
    public partial class VFXRainSpatterEffect : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("雨滴飞溅特效")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public VisualEffect rainSpatterEffect;
            
            [FoldoutGroup("配置")] [LabelText("飞溅序列帧")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Texture2D rainSpatterFlipbook;
            
            [FoldoutGroup("配置")] [LabelText("序列帧大小")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public float2 rainSpatterFlipbookSize = new float2(5,5);
            
            [LabelText("范围网格")] [GUIColor(0.7f,0.7f,1f)]
            [HorizontalGroup("RangeMesh", 0.96f, DisableAutomaticLabelWidth = true)]
            public Mesh rangeMesh;
            
            [HorizontalGroup("RangeMesh")][HideLabel][GUIColor(0.7f,0.7f,1f)] [Space(2f)]
            public bool debugRangeMesh;
            
            [LabelText("网格变换")] [GUIColor(0.7f, 0.7f, 1f)] [InlineEditor(InlineEditorObjectFieldModes.Hidden)] [ShowIf("debugRangeMesh")]
            public Transform meshTransform;
            
            [LabelText("繁殖率")] [GUIColor(1f,0.7f,0.7f)]
            public float spawnRate = 2000;
            
            [LabelText("粒子大小")] [GUIColor(1f,0.7f,0.7f)]
            public float particleSize = 0.05f;
            
            public void LimitProperty()
            {
                spawnRate = math.max(spawnRate, 0);
                particleSize = math.clamp(particleSize, 0f, 0.5f);
            }
        }
        
        [HideLabel] public Property property = new Property();

        [HideInInspector] public bool update;

        public static Mesh TemporaryQuad;

        private bool _isActive;

#if UNITY_EDITOR
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
#endif
        
        #endregion

        

        #region 安装参数
        
        private void SetupStaticProperty()
        {
            if (property.rainSpatterEffect == null || property.rainSpatterEffect.visualEffectAsset == null ||  property.rangeMesh == null) 
                return;
            property.rainSpatterEffect.SetVector3("SpatterStatic_BoundSize", property.rangeMesh.bounds.size);
            property.rainSpatterEffect.SetVector3("SpatterStatic_BoundCenter", property.rangeMesh.bounds.center);
            property.rainSpatterEffect.SetMesh("SplatterStatic_GenerateMesh", property.rangeMesh);
            property.rainSpatterEffect.SetTexture("SplatterStatic_FlipbookTex", property.rainSpatterFlipbook);
            property.rainSpatterEffect.SetVector2("SplatterStatic_FlipbookSize", property.rainSpatterFlipbookSize);
            
        }

        private void SetupDynamicProperty()
        {
            if (property.rainSpatterEffect?.visualEffectAsset is null) 
                return;
            property.rainSpatterEffect.SetFloat("SpatterDynamic_SpawnRate", property.spawnRate);
            property.rainSpatterEffect.SetFloat("SpatterDynamic_Size", property.particleSize);
            property.rainSpatterEffect.SetFloat("Dynamic_ParticleBright", WorldManager.Instance.weatherEffectModule?.property.particleBrightExecute ?? 1);
        }
        
        #endregion

        

        #region 事件函数

        private void OnEnable()
        {
#if UNITY_EDITOR
            //载入数据
            if(property.rainSpatterFlipbook == null)
                property.rainSpatterFlipbook = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem/Textures/Visual Effects/Splash_TextureSheet.tif");
            property.rainSpatterFlipbookSize = new float2(5, 5);
#endif
            if (gameObject.GetComponent<VisualEffect>() == null)
            {
                property.rainSpatterEffect = gameObject.AddComponent<VisualEffect>();
#if UNITY_EDITOR
                property.rainSpatterEffect.visualEffectAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>("Packages/com.worldsystem//Visual Effects/RainSpatterVFX.vfx");
#endif
            }
            else
            {
                property.rainSpatterEffect = gameObject.GetComponent<VisualEffect>();
            }
            
            TemporaryQuad = CreateQuadMesh(10);
            if(property.rangeMesh == null)
                property.rangeMesh = TemporaryQuad;

            if (property.meshTransform == null)
                property.meshTransform = gameObject.transform;
            
#if UNITY_EDITOR
            // 将Mesh组件添加到当前GameObject上  
            // MeshFilter meshFilter;
            if (gameObject.GetComponent<MeshFilter>() == null)
                _meshFilter = gameObject.AddComponent<MeshFilter>();  
            else
                _meshFilter = gameObject.GetComponent<MeshFilter>();
            _meshFilter.sharedMesh = property.rangeMesh;  
  
            // 为了看到Mesh，还需要一个MeshRenderer组件 
            // MeshRenderer meshRenderer;
            if(gameObject.GetComponent<MeshRenderer>() == null)
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            else
                _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            _meshRenderer.material =
                AssetDatabase.LoadAssetAtPath<Material>("Packages/com.worldsystem/Shader/Debug/RainSpatterMeshPreview.mat");
#endif
            OnValidate();
        }
        
        private void OnDisable()
        {
            //销毁卸载数据
            if(property.rainSpatterFlipbook != null)
                Resources.UnloadAsset(property.rainSpatterFlipbook);
            property.rainSpatterFlipbook = null;
            if (gameObject.GetComponent<VisualEffect>() != null)
            {
                if(gameObject.GetComponent<VisualEffect>().visualEffectAsset != null)
                    Resources.UnloadAsset(gameObject.GetComponent<VisualEffect>().visualEffectAsset);
                if(gameObject.activeSelf && Time.frameCount != 0)
                    CoreUtils.Destroy(gameObject.GetComponent<VisualEffect>());
                property.rainSpatterEffect = null;
            }

            if (TemporaryQuad != null)
                CoreUtils.Destroy(TemporaryQuad);
            
            if (property.rangeMesh != null)
            {
                if (property.rangeMesh.name == "TemporaryQuad")
                    CoreUtils.Destroy(property.rangeMesh);
                else
                    Resources.UnloadAsset(property.rangeMesh);
                property.rangeMesh = null;
            }
            property.meshTransform = null;
        }

        public void OnValidate()
        {
            property.LimitProperty();
            SetupStaticProperty();
            
#if UNITY_EDITOR
            if (_meshRenderer != null)
                _meshRenderer.enabled = property.debugRangeMesh;
            if (_meshFilter != null && _meshFilter.sharedMesh != property.rangeMesh)
                _meshFilter.sharedMesh = property.rangeMesh;
#endif
        }
        
        void Update()
        {
            if (!update) return;
            
            //确定是否激活, 如果没有激活则跳出函数, 节约资源
            _isActive = !(property.spawnRate < 5 && property.rainSpatterEffect.aliveParticleCount < 50);
            property.rainSpatterEffect.enabled = _isActive;
            if (!_isActive) return;
            
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