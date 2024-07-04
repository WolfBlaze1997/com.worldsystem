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
#if UNITY_EDITOR
        protected override void DrawGizmos()
        {
            if (property.windZone == null) return;
            Color IconColor;
            if(property.dynamicSpeed) 
                IconColor = HelpFunc.Remap(property.windZone.windMain, property.minSpeed,property.maxSpeed,0f,1f) * Color.white;
            else 
                IconColor = Math.Clamp(property.windZone.windMain * 0.5f, 0, 1) * Color.white;
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
    }

    [ExecuteAlways]
    public partial class WindZoneModule : BaseModule
    {
        
        #region 字段

        [Serializable]
        public class Property
        {
            [LabelText("动态风向")][GUIColor(0.7f,0.7f,1f)]
            public bool dynamicDirection = true;
            [LabelText("    风向改变频率")] [ShowIf("dynamicDirection")][GUIColor(0.7f,0.7f,1f)]
            public int directionVaryingFreq = 6;
            [LabelText("动态速度")][GUIColor(0.7f,0.7f,1f)]
            public bool dynamicSpeed = true;
            [LabelText("    速度改变频率")][ShowIf("dynamicSpeed")][GUIColor(0.7f,0.7f,1f)]
            public int SpeedVaryingFreq = 12;
            [LabelText("    最小速度")][MinValue(0)] [ShowIf("dynamicSpeed")][GUIColor(1f,0.7f,0.7f)]
            public float minSpeed = 0.0f;
            [LabelText("    最大速度")][MinValue(0)] [ShowIf("dynamicSpeed")][GUIColor(1f,0.7f,0.7f)]
            public float maxSpeed = 2.0f;
            [LabelText("粒子影响")][MinValue(0)] [GUIColor(0.7f,0.7f,1f)]
            public float vfxModifier = 1.0f;
            [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
            public WindZone windZone;
            
            public WindData cloudWindData;
            public WindData vfxWindData;
            public struct WindData
            {
                public float speed;
                public Vector3 direction;
            }

            public void LimitProperty()
            {
                directionVaryingFreq = math.max(directionVaryingFreq, 2);
                SpeedVaryingFreq = math.max(SpeedVaryingFreq, 2);
                minSpeed = math.max(minSpeed, 0);
                maxSpeed = math.max(maxSpeed, 0);
                vfxModifier = math.max(vfxModifier, 0);
            }
        }
        
        [HideLabel]
        public Property property = new();

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
            
            _RandomSpeed = Random.Range(property.minSpeed, property.maxSpeed);
            _PreviousSpeed = property.windZone.windMain;
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
            if (!_Update) return;
            
            AutoWindDirection();
            AutoWindSpeed();
            property.cloudWindData = SetWindData(1);
            property.vfxWindData = SetWindData(property.vfxModifier);
        }
        
        #endregion

        
        
        #region 重要函数
        
        //动态随机风方向
        private float _PreviousTimeFactorDirection;
        private float _RandomYRotation;
        private Quaternion _PreviousRotation = Quaternion.identity;
        private void AutoWindDirection()
        {
            if (WorldManager.Instance?.timeModule is null || !property.dynamicDirection) return;
            //将一天中的时间分为 directionVaryingFreq 份 0到1的值, 每一份为一个周期
            float CurrentTimeFactor = WorldManager.Instance.timeModule.CurrentTime01 * property.directionVaryingFreq - (int)(WorldManager.Instance.timeModule.CurrentTime01 * property.directionVaryingFreq);
            //当 当前的时间因子 小于 上一帧的时间因子 说明 周期改变进入下一个周期 ,进入下一个周期重新获得一个随机值, 并记录当前方向进行插值
            if (CurrentTimeFactor < _PreviousTimeFactorDirection)
            {
                _RandomYRotation = Random.Range(-90, 90);
                _PreviousRotation = transform.rotation;
            }
            //当前周期的插值
            transform.rotation = Quaternion.Lerp(_PreviousRotation,Quaternion.AngleAxis(_RandomYRotation, transform.up), CurrentTimeFactor);
            //将 当前时间因子 记录为 上一帧时间因子 退出函数
            _PreviousTimeFactorDirection = CurrentTimeFactor;
        }
        
        //动态随机风速度
        private float _PreviousTimeFactorSpeed;
        private float _RandomSpeed;
        private float _PreviousSpeed;
        private void AutoWindSpeed()
        {
            if (WorldManager.Instance?.timeModule is null || !property.dynamicSpeed) return;

            float CurrentTimeFactor = WorldManager.Instance.timeModule.CurrentTime01 * property.SpeedVaryingFreq - (int)(WorldManager.Instance.timeModule.CurrentTime01 * property.SpeedVaryingFreq);
            if (CurrentTimeFactor < _PreviousTimeFactorSpeed)
            {
                _RandomSpeed = Random.Range(property.minSpeed, property.maxSpeed);
                _PreviousSpeed = property.windZone.windMain;
            }
            
            property.windZone.windMain = math.lerp(_PreviousSpeed, _RandomSpeed, CurrentTimeFactor);
            _PreviousTimeFactorSpeed = CurrentTimeFactor;
        }
        
        private Property.WindData SetWindData(float modifier)
        {
            Property.WindData windData = new Property.WindData
            {
                speed = property.windZone.windMain * modifier,
                direction = transform.forward
            };
            return windData;
        }
        #endregion
        
    }
}