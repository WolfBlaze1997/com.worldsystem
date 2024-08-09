
Shader "Hidden/INabStudio/SSMS_URP"
{
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
            ENDHLSL
        }

        // 2: First level downsampler // 1
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_downsample1
            ENDHLSL
        }
        
        // 4: Second level downsampler // 2
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_downsample2
            ENDHLSL
        }
        // 5: Upsampler // 3
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_upsample
            ENDHLSL
        }
        
        // 7: Combiner // 4
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_upsample_final
            ENDHLSL
        }
        

        
    }
}
