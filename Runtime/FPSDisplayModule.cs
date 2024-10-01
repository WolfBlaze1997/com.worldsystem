using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class FPSDisplayModule : BaseModule
    {
        
        #region 字段
        
        [LabelText("大小")]
        public int size = 50;
        
        [LabelText("频率")]
        public int freq = 100;
        
        [LabelText("偏移")]
        public int bias = 200;
        
        private float _deltaTime;
        
        private string _text;
        
        private bool _startFPS;
        
        private float _averageFps;
        
        private float _totalFps;
        
        private float _maxFps;
        
        private float _minFps;
        
        private int _frameCount;
        
        #endregion
        
        
        #region 事件函数
        
        private void OnEnable()
        {
            _startFPS = true;
            _ = GetFPS();
        }

        void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        private void OnValidate()
        {
            size = Math.Max(30, size);
            size = Math.Min(80, size);
            freq = Math.Max(10, freq);
        }

        private void OnDisable()
        {
            _startFPS = false;
        }
        
        void OnGUI()
        {
            Rect rect = new Rect(Screen.width-bias, 0, 0, 0);
            GUI.Label(rect, _text, GetGUIStyle());
        }
        private GUIStyle GetGUIStyle()
        {
            int h = Screen.height;
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / size;
            style.normal.textColor = new Color(1f, 1f, 1f, 1.0f);
            return style;
        }

        #endregion
        
        
        #region 渲染函数
        
        private async Task GetFPS()
        {
            if (!_startFPS)
            {
                _text = " - FPS | - ms";
                return;
            }
            await Task.Delay(freq);
            {
                _frameCount++;
                float fps = 1.0f / _deltaTime;
                float ms = _deltaTime * 1000.0f;
                _totalFps += fps;
                _averageFps = _totalFps / _frameCount;

                if (_frameCount == 1)
                    _maxFps = fps;
                else
                    _maxFps = Math.Max(_maxFps, fps);

                //正确的最小帧率需要预热
                if (_frameCount < 28)
                {
                    _minFps = math.INFINITY;
                }
                else
                {
                    if(_frameCount == 28)
                        _minFps = fps;
                    else
                    {
                        if(fps > _averageFps * 0.1f)
                            _minFps = Math.Min(_minFps, fps);
                    }
                }
                
                _text = string.Format("{0:0} FPS | {1:0.000} ms" +
                                      "\n平均帧率: {2:0}" +
                                      "\n最大帧率: {3:0}" +
                                      "\n最小帧率: {4:0}", 
                    fps, ms, _averageFps, _maxFps, _minFps);
                
                _ = GetFPS();
            }

        }
        
        #endregion

        
    }
}