using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace WorldSystem.Runtime
{
    public partial class TemplateModule
    {
        //此处编写枚举帮助函数等

        
#if UNITY_EDITOR
        
        //请总是对一类效果制作开关
        [PropertyOrder(-100)]
        // [ShowIf("_bool")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "开启"), GUIColor(0.5f, 0.5f, 1f)]
        public void ToggleFunction_Off()
        {
            // _bool = false;
            // OnValidate();
        }
        
        [PropertyOrder(-100)]
        // [HideIf("_bool")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "关闭"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void ToggleFunction_On()
        {
            // _bool = true;
            // OnValidate();
        }


        #region 绘制Gizmos

        protected override void DrawGizmos()
        {
            base.DrawGizmos();
        }

        protected override void DrawGizmosSelected()
        {
            base.DrawGizmosSelected();
        }

        #endregion
        
        
#endif
    }
    
    
    [ExecuteAlways]
    public partial class TemplateModule : BaseModule
    {
        #region 字段

        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")]
            [LabelText("着色器")]
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Shader shader;

            [FoldoutGroup("配置")]
            [LabelText("材质")]
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Material material;

            [FoldoutGroup("配置")]
            [LabelText("纹理")]
            [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Texture2D texture;
            
            [FoldoutGroup("颜色")][LabelText("强度")] [GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色/_Curve", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve curve = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
            [HorizontalGroup("颜色/_Curve")][HideLabel][ReadOnly]
            public float curveExecute;
            
            [FoldoutGroup("颜色")] [LabelText("颜色")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色/_Gradient", 0.9f, DisableAutomaticLabelWidth = true)]
            public Gradient gradient = new Gradient();
            [HorizontalGroup("颜色/_Gradient")][HideLabel][ReadOnly]
            public Color gradientExecute = Color.white;
            
            public void LimitProperty()
            {
            }

            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) return;
                if (!UseLerp)
                {
                    //未插值时
                    curveExecute = curve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    gradientExecute = gradient.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                }
                else
                {
                    //插值时

                }
            }
        }


        [HideLabel] public Property property = new Property();

        public static bool UseLerp = false;

        [HideInInspector] public bool update;
        
        #endregion


        #region 安装参数
        
        private void SetupStaticProperty()
        {
        }

        private void SetupDynamicProperty()
        {
        }
        
        #endregion

        

        #region 事件函数

        private void OnEnable()
        {
#if UNITY_EDITOR
            //载入数据

#endif
            OnValidate();
        }


        private void OnDisable()
        {
            //销毁卸载数据
        }

        public void OnValidate()
        {
            property.LimitProperty();
            SetupStaticProperty();
        }


        void Update()
        {
            if (!update) return;

            property.ExecuteProperty();
            SetupDynamicProperty();
        }


#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif

        #endregion


        
        #region 渲染函数

        public void RenderFunction(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!isActiveAndEnabled) return;
        }

        #endregion
        
    }
    
}