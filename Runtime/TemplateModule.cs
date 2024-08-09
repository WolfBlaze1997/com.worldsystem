using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class TemplateModule : BaseModule
    {
        
        #region 字段
        
        [Serializable]
        public class Property
        {


            public void LimitProperty()
            {
                
            }
        }
        
        
        [HideLabel]
        public Property property = new Property();
        
        public static bool useLerp = false;
        
        [HideInInspector]
        public bool _Update;
        
        // 插值执行缓存
        // [HideInInspector]
        // public Color ReflectionSkyColorExecute;
        
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
            if (!_Update) return;
            
            if (!useLerp)
            {
                //未插值时

            }
            else
            {
                //插值时

            }
            
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

        

        #endregion
        
        
        
    }
    
}
