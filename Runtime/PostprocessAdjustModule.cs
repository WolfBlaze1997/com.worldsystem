using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSystem.Runtime
{
    public partial class PostprocessAdjustModule
    {
        
        #region 模块开关
        
#if UNITY_EDITOR
        [PropertyOrder(-100)]
        [ShowIf("@property.useColorAdjust")]
        [HorizontalGroup("Split", 0.25f)]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "颜色调整"), GUIColor(0.5f, 0.5f, 1f)]
        public void ColorAdjust_Off()
        {
            property.useColorAdjust = false;
            OnValidate();
        }
        
        [PropertyOrder(-100)]
        [HideIf("@property.useColorAdjust")]
        [VerticalGroup("Split/01")]
        [Button(ButtonSizes.Medium, Name = "颜色调整"), GUIColor(0.5f, 0.2f, 0.2f)]
        public void ColorAdjust_On()
        {
            property.useColorAdjust = true;
            OnValidate();
        }
#endif
        
        #endregion
        
    }
    
    
    [ExecuteAlways]
    public partial class PostprocessAdjustModule : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [FoldoutGroup("配置")] [LabelText("全局后处理")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public Volume globalVolume;

            [FoldoutGroup("配置")] [LabelText("材质")] [ReadOnly]
            [ShowIf("@WorldManager.Instance?.starModule?.hideFlags == HideFlags.None")]
            public ColorAdjustments colorAdjustments;
            
            [HideInInspector]
            public bool useColorAdjust;
            
            [FoldoutGroup("颜色调整")][LabelText("曝光")] [GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色调整/_ExposeCurve", 0.9f, DisableAutomaticLabelWidth = true)] [ShowIf("useColorAdjust")]
            public AnimationCurve exposeCurve = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
            
            [HorizontalGroup("颜色调整/_ExposeCurve")][HideLabel][ReadOnly][ShowIf("useColorAdjust")]
            public float exposeCurveExecute;
            
            [FoldoutGroup("颜色调整")][LabelText("对比度[-100,100]")] [GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色调整/_ContrastCurve", 0.9f, DisableAutomaticLabelWidth = true)][ShowIf("useColorAdjust")]
            public AnimationCurve contrastCurve = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,0));
            
            [HorizontalGroup("颜色调整/_ContrastCurve")][HideLabel][ReadOnly][ShowIf("useColorAdjust")]
            public float contrastCurveExecute;
            
            [FoldoutGroup("颜色调整")] [LabelText("颜色过滤器")][GUIColor(1f,0.7f,0.7f)]
            [HorizontalGroup("颜色调整/_ColorFilter", 0.9f, DisableAutomaticLabelWidth = true)][ShowIf("useColorAdjust")]
            public Gradient colorFilter = new Gradient();
            
            [HorizontalGroup("颜色调整/_ColorFilter")][HideLabel][ReadOnly][ShowIf("useColorAdjust")]
            public Color colorFilterExecute = Color.white;
            
            [FoldoutGroup("颜色调整")][LabelText("色调改变[-100,100]")] [GUIColor(0.7f,0.7f,1f)][Range(-180,180)] [ShowIf("useColorAdjust")]
            public float hueShift;
            
            [FoldoutGroup("颜色调整")][LabelText("饱和度[-100,100]")] [GUIColor(1f,0.7f,0.7f)] 
            [HorizontalGroup("颜色调整/_SaturationCurve", 0.9f, DisableAutomaticLabelWidth = true)][ShowIf("useColorAdjust")]
            public AnimationCurve saturationCurve = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,0));
            
            [HorizontalGroup("颜色调整/_SaturationCurve")][HideLabel][ReadOnly][ShowIf("useColorAdjust")]
            public float saturationCurveExecute;
            
            public void LimitProperty()
            {
            }

            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) return;
                if (!UseLerp)
                {
                    //未插值时
                    if (useColorAdjust)
                    {
                        exposeCurveExecute = exposeCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                        contrastCurveExecute = contrastCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                        colorFilterExecute = colorFilter.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                        saturationCurveExecute = saturationCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                    }
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
            property.colorAdjustments.active = property.useColorAdjust;
            property.colorAdjustments.hueShift.value = property.hueShift;
        }

        private void SetupDynamicProperty()
        {
            if (property.useColorAdjust) 
            {
                property.colorAdjustments.postExposure.value = property.exposeCurveExecute;
                property.colorAdjustments.contrast.value = property.contrastCurveExecute;
                property.colorAdjustments.colorFilter.value = property.colorFilterExecute;
                property.colorAdjustments.saturation.value = property.saturationCurveExecute;
            }
            
        }
        
        #endregion


        #region 事件函数

        private void OnEnable()
        {
#if UNITY_EDITOR
            //载入数据
#endif
            property.globalVolume = FindObjectsOfType<Volume>().ToList().Find(o => o.isGlobal);
            if (property.globalVolume == null)
            {
                property.globalVolume = new GameObject("Global Volume").AddComponent<Volume>();
            }

            if (property.globalVolume.profile == null)
            {
                property.globalVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }
            
            if (!property.globalVolume.profile.Has<ColorAdjustments>())
            {
                property.colorAdjustments = property.globalVolume.profile.Add<ColorAdjustments>();
            }
            else
            {
                property.colorAdjustments =
                    (ColorAdjustments)property.globalVolume.profile.components.Find(o => o.name.Contains("ColorAdjustments"));
            }
            
            foreach (var VARIABLE in property.colorAdjustments.parameters)
            {
                VARIABLE.overrideState = true;
            }
            
            OnValidate();
        }
        
        // private void OnDisable()
        // {
        //     //销毁卸载数据
        // }

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
        
        
    }
    
}