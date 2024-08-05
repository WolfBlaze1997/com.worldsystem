#ifndef SSR_SOLVE
#define SSR_SOLVE
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "SSR_Common.hlsl"


TEXTURE2D_X (_MainTex);
float4 _MainTex_TexelSize;
TEXTURE2D_X (_RayCastRT);

float4 _MaterialData;
#define FRESNEL _MaterialData.y
#define FUZZYNESS _MaterialData.z

float4 _SSRSettings2;
#define ENERGY_CONSERVATION _SSRSettings2.y
#define REFLECTIONS_MULTIPLIER _SSRSettings2.z

float4 _SSRSettings4;
#define REFLECTIONS_MIN_INTENSITY _SSRSettings4.y
#define REFLECTIONS_MAX_INTENSITY _SSRSettings4.z

float3 _SSRSettings5;
#define SKYBOX_INTENSITY _SSRSettings5.z

float4 _SSRBlurStrength;
#define VIGNETTE_SIZE _SSRBlurStrength.z
#define VIGNETTE_POWER _SSRBlurStrength.w

samplerCUBE _SkyboxCubemap;
float3 _CameraViewDir;
#define CAMERA_VIEW_DIR _CameraViewDir

struct AttributesFS
{
    float4 positionHCS : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsSSR
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


VaryingsSSR VertSSR(AttributesFS input)
{
    VaryingsSSR output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionCS = float4(input.positionHCS.xyz, 1.0);

    #if UNITY_UV_STARTS_AT_TOP
		output.positionCS.y *= -1;
    #endif

    output.uv = input.uv;
    return output;
}

half4 FragResolve(VaryingsSSR i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    i.uv = SSRStereoTransformScreenSpaceTex(i.uv);

    half4 reflData = SAMPLE_TEXTURE2D_X(_RayCastRT, sampler_PointClamp, i.uv);
    
    half4 reflection = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, reflData.xy);


    reflection.rgb = min(reflection.rgb, 8.0); // stop NAN pixels
    half vd = dot2((reflData.xy - 0.5) * 2.0);
    half vignette = saturate(VIGNETTE_SIZE - vd * vd);
    vignette = pow(vignette, VIGNETTE_POWER);


    half reflectionIntensity = clamp(reflData.a * REFLECTIONS_MULTIPLIER, REFLECTIONS_MIN_INTENSITY,
                                     REFLECTIONS_MAX_INTENSITY);
    
    reflectionIntensity *= vignette;
    // reflection.rgb *= reflectionIntensity;

    reflection.rgb = min(reflection.rgb, 1.2); // clamp max brightness

    
    // 保持阿尔法通道中的模糊因子
    reflection.a = reflData.z;

    return half4(reflection.xyz,reflData.w * reflectionIntensity);
    
}


#endif // SSR_SOLVE
