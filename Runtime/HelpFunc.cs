using System;
using System.IO;
using UnityEngine;

namespace WorldSystem.Runtime
{
    public static class HelpFunc
    {
#if UNITY_EDITOR
        public static void DrawText(Vector3 position, Vector3 offsetDir, int textSize, string inString)
        {
            float f = Vector3.Distance(Camera.current.transform.position, position);
            if (f > 20) return;
            f = Math.Clamp(f, 0.001f, 20);
            f = HelpFunc.Remap(f, 0.001f, 20, textSize, 5);
            
            GUIStyle s = new GUIStyle();
            s.fontSize = (int)f;
            s.normal.textColor = Color.white;
            s.alignment = TextAnchor.MiddleLeft;
            s.fontStyle = FontStyle.Bold;
            UnityEditor.Handles.color = new Color(1, 1, 1, 0.5f);
            Vector3 offset = offsetDir * 1f;

            s.fontSize = (int)Math.Round(f * 0.75);
            s.normal.textColor = new Color(1, 1, 1, 0.8f);
            UnityEditor.Handles.Label(
                position + offset,
                new GUIContent(
                    inString
                ),
                s
            );

            UnityEditor.Handles.DrawLine(position + offset, position, 0f);
        }
#endif
        public static float EaseIn(float x)
        {
            return x * x;
        }
        
        public static float EaseOut(float x)
        {
            return 1.0f - EaseIn(1.0f - x);
        }
        
        public static float Frac(float x)
        {
            return Mathf.Abs(x - (int)x);
        }
        
        public static float EaseInOut(float x)
        {
            float a = EaseIn(x);
            float b = EaseOut(x);
            return Mathf.Lerp(a, b, x);
        }
        
        public static float Random(float x, float seed = 0.546f)
        {
            return Frac(Mathf.Sin((x + 0.3804f) * seed) * 143758.5453f);
        }
        
        public static float Smoothstep(float a, float b, float t)
        {
            float x = EaseInOut(t);
            return RemapFrom01(x, a, b);
        }
        
        public static float RemapFrom01(float value, float oMin, float oMax)
        {
            value = Mathf.Clamp01(value);
            return oMin + (oMax - oMin) * value;
        }
        
        public static float GradientNoise(float x)
        {
            float f = Frac(x);
            float t = EaseInOut(f);

            float previousInclination = (Random(Mathf.Floor(x)) * 2f) - 1f;
            float previousPoint = previousInclination * f;

            float nextInclination = (Random(Mathf.Ceil(x)) * 2f) - 1f;
            float nextPoint = nextInclination * (f - 1f);
            return Mathf.Lerp(previousPoint, nextPoint, t);
        }
        
        public static float GradientNoiseLayered(float x, float frequency, int octaves, float lacunarity, float gain)
        {
            float v = 0;
            float amp = 1;
            for (int i = 0; i < octaves; i++)
            {
                v += GradientNoise(x * frequency + i * 0.16291f) * amp;
                x *= lacunarity;
                amp *= gain;
            }
            return v;
        }
        
        public static void ClearRenderTexture(RenderTexture textureToClear)
        {
            RenderTexture activeTexture = RenderTexture.active;
            RenderTexture.active = textureToClear;
            GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 1.0f));
            RenderTexture.active = activeTexture;
        }
        
        /// <summary>
        /// 返回衰减中使用的密度=（1.0/（exp（密度*距离）），这将产生在给定距离衰减的98%的衰减
        /// </summary>
        public static float GetDensityFromVisibilityDistance(float distance)
        {
            const float factor = 3.912023005f;
            return factor / distance;
        }
        
        public static void AssignDefaultDescriptorSettings(ref RenderTextureDescriptor desc, RenderTextureFormat format = RenderTextureFormat.DefaultHDR)
        {
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.width = Mathf.Max(1, desc.width);
            desc.height = Mathf.Max(1, desc.height);
            desc.useDynamicScale = false;
            desc.colorFormat = format;
        }
        
        public static Matrix4x4 SetupViewMatrix(Vector3 cameraPosition, Vector3 cameraForward, float zFar, Vector3 cameraUp)
        {
            // It is extremely important that the LookAt matrix uses real positions, not just relative vectors, for the "from" and "to" fields.
            Matrix4x4 lookMatrix = Matrix4x4.LookAt(cameraPosition, cameraPosition + cameraForward * zFar, cameraUp);
            Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            Matrix4x4 viewMatrix = scaleMatrix * lookMatrix.inverse;
            return viewMatrix;
        }

        public static Matrix4x4 SetupProjectionMatrix(float halfWidth, float zFar)
        {
            float s = halfWidth;
            Matrix4x4 proj = Matrix4x4.Ortho(-s, s, -s, s, 30, zFar);
            return proj;
        }

        public static Matrix4x4 ConvertToWorldToShadowMatrix(Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
        {
            var scaleOffset = Matrix4x4.identity;
            scaleOffset.m00 = scaleOffset.m11 = scaleOffset.m22 = 0.5f;
            scaleOffset.m03 = scaleOffset.m13 = scaleOffset.m23 = 0.5f;
            return scaleOffset * (projectionMatrix * viewMatrix);
        }
        
        /// <summary>
        /// 创建一个四边形网格
        /// </summary>
        public static Mesh CreateQuad(float width = 1f, float height = 1f)
        {
            Mesh mesh = new Mesh();

            float w = width * 0.5f;
            float h = height * 0.5f;

            Vector3[] verts = new Vector3[] { new Vector3(w, -h, 0), new Vector3(-w, -h, 0), new Vector3(w, h, 0), new Vector3(-w, h, 0) };

            int[] tris = new int[] { 0, 2, 1, 2, 3, 1 };

            Vector3[] normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, };

            Vector2[] uvs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1), };

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.name = "Quad";
            return mesh;
        }
        
        public static float Remap(float value, float iMin, float iMax, float oMin, float oMax)
        {
            value = Mathf.Clamp(value, iMin, iMax);
            float a = Mathf.InverseLerp(iMin, iMax, value);
            return Mathf.Lerp(oMin, oMax, a);
        }

        public static Vector3 Saturate(Vector3 v3)
        {
            return new Vector3(Mathf.Clamp01(v3.x), Mathf.Clamp01(v3.y), Mathf.Clamp01(v3.z));
        }
        
        /// <summary>
        /// 将范围[iMin，iMax]中的值映射到范围[0,1]
        /// </summary>
        public static float RemapTo01(float value, float iMin, float iMax)
        {
            // Ensure value is within the input range
            value = Mathf.Clamp(value, iMin, iMax);

            // Calculate the remapped value in the [0, 1] range
            return (value - iMin) / (iMax - iMin);
        }

        public static bool CheckForStringInFile(string filePath, string searchString)
        {
            try
            {
                // 判断文件是否存在
                if (File.Exists(filePath))
                {
                    // 读取文件的全部内容
                    string fileContent = File.ReadAllText(filePath);

                    // 检查文件内容是否包含指定的字符串
                    if (fileContent.Contains(searchString))
                    {
                        // Debug.Log("文件中包含_CameraDepthTextureAddCloudMask字符串。");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("文件: " + filePath + "中未检测到"+ searchString + "! 镜头光晕无法与体积云交互!");
                        return false;
                    }
                }
                else
                {
                    Debug.LogError("文件不存在: " + filePath);
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("读取文件时发生错误: " + e.Message);
                return false;
            }
        }
    }
}