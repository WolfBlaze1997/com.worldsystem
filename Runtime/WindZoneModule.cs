using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming

namespace WorldSystem.Runtime
{
    public partial class WindZoneModule
    {
        
        public enum DynamicMode
        {
            CurveMode,
            RandomMode
        }

        
        #region Gizmos相关
        
#if UNITY_EDITOR
        
        protected override void DrawGizmos()
        {
            if (property.windZone == null) return;
            Color IconColor = HelpFunc.Remap(property.windZone.windMain, property.minSpeed,property.maxSpeed,0f,1f) * Color.white;
            Gizmos.DrawIcon(transform.position, "Packages/com.worldsystem//Textures/Icon/fan-icon.png",true, IconColor);
        }
        
        protected override void DrawGizmosSelected()
        {
            float Scale = 0.8f;
            Vector3[] points01 = {
                transform.position + transform.forward * -2 * Scale,
                transform.position + transform.forward * -2 * Scale + transform.up * Scale,
                
                transform.position + transform.forward * -2 * Scale + transform.up * Scale,
                transform.position + transform.forward * -2 * Scale + transform.up * Scale + transform.forward * 3 * Scale,
                
                transform.position + transform.forward * -2 * Scale + transform.up * Scale + transform.forward * 3 * Scale,
                transform.position + transform.forward * -2 * Scale + transform.up * Scale + transform.forward * 3 * Scale + transform.up * Scale,
                
                transform.position + transform.forward * -2  * Scale+ transform.up * Scale + transform.forward * Scale * 3 + transform.up * Scale,
                transform.position + transform.forward * 3 * Scale
            };
            
            Color Cache = Gizmos.color;
            Gizmos.color = Color.gray;

            Gizmos.DrawLineList(points01);
            
            Vector3[] points02 = new Vector3[8];
            for (int i = 0; i < points01.Length; i++)
            {
                points02[i] = Quaternion.AngleAxis(90, transform.forward) * (points01[i] - transform.position) + transform.position;
            }
            Gizmos.DrawLineList(points02);

            Vector3[] points03 = new Vector3[8];
            for (int i = 0; i < points01.Length; i++)
            {
                points03[i] = Quaternion.AngleAxis(90, transform.forward) * (points02[i] - transform.position) + transform.position;
            }
            Gizmos.DrawLineList(points03);
            
            Vector3[] points04 = new Vector3[8];
            for (int i = 0; i < points01.Length; i++)
            {
                points04[i] = Quaternion.AngleAxis(90, transform.forward) * (points03[i] - transform.position) + transform.position;
            }
            Gizmos.DrawLineList(points04);

            Gizmos.color = Cache;
        }
        
#endif  
        
        #endregion
        

    }

    [ExecuteAlways]
    public partial class WindZoneModule : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [LabelText("动态模式")] [GUIColor(0.7f,0.7f,1f)]
            public DynamicMode dynamicMode = DynamicMode.CurveMode;
            
            [LabelText("风向旋转曲线")] [GUIColor(1f,0.7f,1f)] [ShowIf("@dynamicMode == DynamicMode.CurveMode")]
            [HorizontalGroup("directionRotateCurve", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve directionRotateCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,0));
            
            [ShowIf("@dynamicMode == DynamicMode.CurveMode")] [HorizontalGroup("directionRotateCurve")][HideLabel][ReadOnly]
            public float directionRotateCurve_Execute;
            
            [LabelText("风速曲线")] [GUIColor(1f,0.7f,0.7f)] [ShowIf("@dynamicMode == DynamicMode.CurveMode")]
            [HorizontalGroup("WindSpeedCurve", 0.9f, DisableAutomaticLabelWidth = true)]
            public AnimationCurve WindSpeedCurve = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
            
            [ShowIf("@dynamicMode == DynamicMode.CurveMode")] [HorizontalGroup("WindSpeedCurve")][HideLabel][ReadOnly]
            public float WindSpeedCurve_Execute;
            
            [LabelText("风向改变频率")] [GUIColor(0.7f,0.7f,1f)][ShowIf("@dynamicMode == DynamicMode.RandomMode")]
            public int directionVaryingFreq = 6;
            
            [LabelText("速度改变频率")][GUIColor(0.7f,0.7f,1f)][ShowIf("@dynamicMode == DynamicMode.RandomMode")]
            public int SpeedVaryingFreq = 12;
            
            [LabelText("    最小速度")][MinValue(0)] [GUIColor(1f,0.7f,0.7f)][ShowIf("@dynamicMode == DynamicMode.RandomMode")]
            public float minSpeed;
            
            [LabelText("    最大速度")][MinValue(0)] [GUIColor(1f,0.7f,0.7f)][ShowIf("@dynamicMode == DynamicMode.RandomMode")]
            public float maxSpeed = 2.0f;
            
            [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
            public WindZone windZone;
            
            [HideInInspector]
            public float WindSpeed;
            
            [HideInInspector]
            public Vector3 WindDirection;
            
            public void LimitProperty()
            {
                directionVaryingFreq = math.max(directionVaryingFreq, 2);
                SpeedVaryingFreq = math.max(SpeedVaryingFreq, 2);
                minSpeed = math.max(minSpeed, 0);
                maxSpeed = math.max(maxSpeed, 0);
            }
            
            public void ExecuteProperty()
            {
                if (WorldManager.Instance.timeModule is null) return;
                if (!useLerp)
                {
                    WindSpeedCurve_Execute = WindSpeedCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
                }
                directionRotateCurve_Execute = directionRotateCurve.Evaluate(WorldManager.Instance.timeModule.CurrentTime01);
            }
        }
        
        [HideLabel]
        public Property property = new();
        
        public static bool useLerp = false;

        #endregion

        
        
        #region 事件函数
        
        private void OnEnable()
        {
            if(gameObject.GetComponent<WindZone>() == null)
                property.windZone = gameObject.AddComponent<WindZone>();
            else
            {
                property.windZone = gameObject.GetComponent<WindZone>();
            }
            _randomSpeed = Random.Range(property.minSpeed, property.maxSpeed);
            _previousSpeed = property.windZone.windMain;
        }
        
        private void OnDestroy()
        {
            if(GetComponent<WindZone>() != null && gameObject.activeSelf && Time.frameCount != 0)
                CoreUtils.Destroy(GetComponent<WindZone>());
            property.windZone = null;
        }
        
#if UNITY_EDITOR
        private void Start()
        {
            WorldManager.Instance?.weatherListModule?.weatherList?.SetupPropertyFromActive();
        }
#endif
        
        [HideInInspector] public bool _Update;
        
        private void Update()
        {
            if (!_Update) 
                return;
            
            property.ExecuteProperty();
            DynamicWindDirection();
            DynamicWindSpeed();
            property.WindDirection = property.windZone.transform.forward;
            property.WindSpeed = property.windZone.windMain;
            
        }
        
        #endregion

        
        
        #region 重要函数
        
        //动态随机风方向
        private float _previousTimeFactorDirection;
        private float _randomYRotation;
        private Quaternion _previousRotation = Quaternion.identity;
        //动态随机风速度
        private float _previousTimeFactorSpeed;
        private float _randomSpeed;
        private float _previousSpeed;
        
        private void DynamicWindDirection()
        {
            if (WorldManager.Instance?.timeModule is null) return;

            if (property.dynamicMode == DynamicMode.RandomMode)
            {
                //将一天中的时间分为 directionVaryingFreq 份 0到1的值, 每一份为一个周期
                float CurrentTimeFactor = WorldManager.Instance.timeModule.CurrentTime01 * property.directionVaryingFreq - (int)(WorldManager.Instance.timeModule.CurrentTime01 * property.directionVaryingFreq);
                //当 当前的时间因子 小于 上一帧的时间因子 说明 周期改变进入下一个周期 ,进入下一个周期重新获得一个随机值, 并记录当前方向进行插值
                if (CurrentTimeFactor < _previousTimeFactorDirection)
                {
                    _randomYRotation = Random.Range(-90, 90);
                    _previousRotation = transform.rotation;
                }
                //当前周期的插值
                transform.rotation = Quaternion.Lerp(_previousRotation,Quaternion.AngleAxis(_randomYRotation, transform.up), CurrentTimeFactor);
                //将 当前时间因子 记录为 上一帧时间因子 退出函数
                _previousTimeFactorDirection = CurrentTimeFactor;
            }
            else
            {
                transform.rotation = Quaternion.AngleAxis(property.directionRotateCurve_Execute, transform.up);
            }
        }
        
        private void DynamicWindSpeed()
        {
            if (WorldManager.Instance?.timeModule is null ) return;

            if (property.dynamicMode == DynamicMode.RandomMode)
            {
                float CurrentTimeFactor = WorldManager.Instance.timeModule.CurrentTime01 * property.SpeedVaryingFreq - (int)(WorldManager.Instance.timeModule.CurrentTime01 * property.SpeedVaryingFreq);
                if (CurrentTimeFactor < _previousTimeFactorSpeed)
                {
                    _randomSpeed = Random.Range(property.minSpeed, property.maxSpeed);
                    _previousSpeed = property.windZone.windMain;
                }
                property.windZone.windMain = math.lerp(_previousSpeed, _randomSpeed, CurrentTimeFactor);
                _previousTimeFactorSpeed = CurrentTimeFactor;
            }
            else
            {
                property.windZone.windMain = property.WindSpeedCurve_Execute;
            }
            
        }
        
        
        #endregion
        
    }
}