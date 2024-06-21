using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class FPSDisplayModule : BaseModule
    {
        private float deltaTime = 0.0f;
        [LabelText("大小")]
        public int size = 36;
        [LabelText("频率")]
        public int freq = 100;

        private void OnEnable()
        {
            Application.targetFrameRate = 2000;
            // Debug.Log("XXXQQQ");
            StartFPS = true;
            _ = GetFPS();
        }

        void Update()
        {
            // 计算每帧之间的时间差  
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        private void OnValidate()
        {
            size = Math.Max(15, size);
            size = Math.Min(50, size);
            freq = Math.Max(10, freq);

        }

        private void OnDisable()
        {
            StartFPS = false;
        }

        private string _text;

        void OnGUI()
        {
            Rect rect = new Rect(0, 0, Screen.width, Screen.height * 2 / size);
            GUI.Label(rect, _text, GetGUIStyle());
        }

        private GUIStyle GetGUIStyle()
        {
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();
            //Rect rect = new Rect(0, 0, 200, 100); // 设置帧率显示区域的位置和大小  
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / size;
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

            return style;
        }

        private bool StartFPS = false;

        [Button(name: "开始/暂停")]
        private void FPS()
        {
            StartFPS = !StartFPS;
            _ = GetFPS();
        }
        private async Task GetFPS()
        {
            if (!StartFPS)
            {
                _text = " - FPS | - ms";
                return;
            }
            await Task.Delay(freq);
            {
                float fps = 1.0f / deltaTime;
                float ms = deltaTime * 1000.0f;
                _text = string.Format("{0:0} FPS | {1:0.000} ms", fps, ms);
                _ = GetFPS();
            }
        }
        
    }
}