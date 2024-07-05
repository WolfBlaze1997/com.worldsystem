
Shader "Hidden/WorldSystem/ScreenShadowsOptimize_V1_1_20240604"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        Cull Off
//        Blend Off
        ZWrite Off
        ZTest Always
        Blend DstColor Zero
//        BlendOp Multiply
        
        Pass
        {
            Name "ScreenShadows"
            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "CloudShadows_V1_1_20240604.hlsl"


            // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
            // D3D
            // UNITY_REVERSED_Z is defined on D3D
            // Depth Buffer goes [1, 0] on D3D
            // CS Depth goes [1, 0] on D3D

            // OpenGL
            // UNITY_REVERSED_Z is not defined on OpenGL
            // Depth Buffer goes [0, 1] on OpenGL
            // CS Depth goes [-1, 1] on OpenGL

            // For the reconstruction function (ComputeWorldSpacePosition) to work, the depth value must be in the normalized device coordinate (NDC) space. 
            // In D3D, Z is in range [0,1], in OpenGL, Z is in range [-1, 1].


            bool IsFarPlane(float rawDepth)
            {
            #if UNITY_REVERSED_Z
                if (rawDepth <= 0.0)
                    return true;
            #else
                if(rawDepth >= 1.0)
                    return true;
            #endif
                return false;
            }

            float GetDepthCS(float rawDepth)
            {
            #if UNITY_REVERSED_Z
                return rawDepth;
            #else
                return lerp(-1, 1, rawDepth);
            #endif
            }

            float3 frag(Varyings IN) : SV_Target
            {
                float depth = SampleSceneDepth(IN.texcoord);
                float depthCS = GetDepthCS(depth);
    
                float3 cloudShadows = float3(1,1,1);
    
                if (!IsFarPlane(depth))
                {
                    float3 positionWS = ComputeWorldSpacePosition(IN.texcoord, depthCS, UNITY_MATRIX_I_VP);
                    cloudShadows = GetCloudShadowAttenuation(positionWS);
                }
                return cloudShadows;
            }
            ENDHLSL
        }
    }
}