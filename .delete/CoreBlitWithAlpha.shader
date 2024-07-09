Shader "Hidden/Universal/CoreBlitWithAlpha"
{
    HLSLINCLUDE

        #pragma target 2.0
        #pragma editor_sync_compilation
        #pragma multi_compile _ DISABLE_TEXTURE2D_X_ARRAY
        #pragma multi_compile _ BLIT_SINGLE_SLICE
        // Core.hlsl for XR dependencies
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" }

        // 0: Alpha
        Pass
        {
            ZWrite Off ZTest Always
//            Blend SrcAlpha OneMinusSrcAlpha
            Blend One OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragNearest
            ENDHLSL
        }
    }

    Fallback Off
}
