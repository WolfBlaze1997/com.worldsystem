
Shader "Hidden/INabStudio/SSMS_URP"
{
    Properties
    {
        [Toggle]ANTI_FLICKER("anti_flicker_on", Float) = 0
        [Toggle]_HIGH_QUALITY("_high_quality_on", Float) = 0
    }
    
    SubShader
    {
        ZTest Always Cull Off ZWrite Off

        HLSLINCLUDE
        #include "SSMS_URP.hlsl"
        ENDHLSL

        // 0: Prefilter // 0
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_prefilter
            #pragma shader_feature _ ANTI_FLICKER_ON 
            #pragma shader_feature _ _HIGH_QUALITY_ON 
            ENDHLSL
        }

        // 2: First level downsampler // 1
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_downsample1
            #pragma shader_feature _ ANTI_FLICKER_ON 
            #pragma shader_feature _ _HIGH_QUALITY_ON 
            ENDHLSL
        }
        
        // 4: Second level downsampler // 2
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_downsample2
            #pragma shader_feature _ ANTI_FLICKER_ON 
            #pragma shader_feature _ _HIGH_QUALITY_ON 
            ENDHLSL
        }
        // 5: Upsampler // 3
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_upsample
            #pragma shader_feature _ ANTI_FLICKER_ON 
            #pragma shader_feature _ _HIGH_QUALITY_ON 
            ENDHLSL
        }
        
        // 7: Combiner // 4
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_upsample_final
            #pragma shader_feature _ ANTI_FLICKER_ON 
            #pragma shader_feature _ _HIGH_QUALITY_ON 
            ENDHLSL
        }
        

        
    }
    CustomEditor "UnityEditor.Rendering.Fullscreen.ShaderGraph.FullscreenShaderGUI"
    FallBack "Hidden/Shader Graph/FallbackError"

}
