#ifndef SSR_SURF_FX
#define SSR_SURF_FX
// #include "SSR_Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

TEXTURE2D(_SSR_NoiseTex);
float4 _SSR_NoiseTex_TexelSize;

float4 _SSR_Settings;
#define SAMPLES _SSR_Settings.x //采样计数
#define MAX_RAY_LENGTH _SSR_Settings.y //光线长度
#define THICKNESS _SSR_Settings.z //厚度
#define JITTER _SSR_Settings.w //抖动


inline float GetLinearDepth(float2 uv)
{
    float DeviceDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, uv).r;
    float PositionVSz = LinearEyeDepth(DeviceDepth, _ZBufferParams);
    return PositionVSz;
}


float4 SSR_Pass(float2 uv, float3 normalVS, float3 rayStart)
{
    
    float3 viewDirVS = normalize(rayStart);
    float3 rayDir = reflect(viewDirVS, normalVS);

    // if ray is toward the camera, early exit (optional)
    // if (rayDir.z < 0) return 0;

    float rayLength = MAX_RAY_LENGTH;

    float3 rayEnd = rayStart + rayDir * rayLength;
    if (rayEnd.z < _ProjectionParams.y)
    {
        rayLength = (_ProjectionParams.y - rayStart.z) / rayDir.z;
        rayEnd = rayStart + rayDir * rayLength;
    }

    float4 sposStart = mul(unity_CameraProjection, float4(rayStart, 1.0));
    float4 sposEnd = mul(unity_CameraProjection, float4(rayEnd, 1.0));
    float k0 = rcp(sposStart.w);
    float q0 = rayStart.z * k0;
    float k1 = rcp(sposEnd.w);
    float q1 = rayEnd.z * k1;
    float4 p = float4(uv, q0, k0);
    
    
    // 长度（像素）
    float2 uv1 = (sposEnd.xy * rcp(rayEnd.z) + 1.0) * 0.5;
    float2 duv = uv1 - uv;
    float2 duvPixel = abs(duv * _ScreenSize.xy);
    float pixelDistance = max(duvPixel.x, duvPixel.y);
    int sampleCount = (int)clamp(pixelDistance, 1, SAMPLES);
    float4 pincr = float4(duv, q1 - q0, k1 - k0) * rcp(sampleCount);
    
    float jitter = SAMPLE_TEXTURE2D(_SSR_NoiseTex, sampler_PointRepeat, uv * _ScreenSize.xy * _SSR_NoiseTex_TexelSize.xy).r;
    pincr *= 1.0 + jitter * JITTER;
    p += pincr * (jitter * JITTER);

    float collision;

    UNITY_LOOP
    for (int k = 0; k < sampleCount; k++)
    {
        p += pincr;
        if (any(floor(p.xy) != 0)) return 0; // exit if out of screen space
        float pz = p.z / p.w;

        float depthDiff;
        float sceneDepth = GetLinearDepth(p.xy);
        depthDiff = pz - sceneDepth;
        UNITY_BRANCH
        if (depthDiff > 0 && depthDiff < THICKNESS)
        {
            p -= pincr;
            float hitAccuracy = 1.0 - abs(depthDiff) / THICKNESS;
            float zdist = (pz - rayStart.z) / (0.0001 + rayEnd.z - rayStart.z);
            float rayFade = 1.0 - saturate(zdist);
            collision = hitAccuracy * rayFade;
            break;
        }
    }


return lerp(half4(0, 0, 0, 0),
        half4(min(SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_LinearClamp, p.xy).rgb,3.6),1.0),
        step(0,collision));
    
}



#endif // SSR_SURF_FX
