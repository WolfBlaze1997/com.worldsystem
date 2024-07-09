#ifndef LIGHTING_FUNCTION_LIBRARY
#define LIGHTING_FUNCTION_LIBRARY
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"


//---------------------------------------------------------------------------------
struct EnvLighting
{
    half3 staticLightingMap;//静态光照贴图
    half3 dynamicLightingMap;//动态光照贴图
    half3 lightingProbes;//SH
    half3 volumeProbe;
    half3 vertexSH;
    half3 bakedGI;//光照贴图 or SH;
    // LightingProbesGlobalVar lightingProbesGlobalVar;
    // LightingMapGlobalVar lightingMapGlobalVar;

};
void GetEnvLighting_float(float2 staticLightmapUV, float2 dynamicLightmapUV, half3 vertexSH, half3 normalWS, half3 viewDir, float2 positionSS, float3 positionWS, Light mainlight, out EnvLighting envLighting)
{
    envLighting = (EnvLighting)0;

    float3 absolutePositionWS = GetAbsolutePositionWS(positionWS);
    envLighting.dynamicLightingMap = SampleLightmap(0, dynamicLightmapUV, normalWS);
    envLighting.staticLightingMap = SampleLightmap(staticLightmapUV, 0, normalWS);
    #if (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
        envLighting.volumeProbe = SampleProbeVolumePixel(vertexSH, absolutePositionWS, normalWS, viewDir, positionSS);
    #endif
    envLighting.lightingProbes = SampleSHPixel(vertexSH, normalWS);
    envLighting.vertexSH = vertexSH;

    #if defined(LIGHTMAP_ON)
        envLighting.bakedGI = envLighting.staticLightingMap;
    #elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
        envLighting.bakedGI = envLighting.volumeProbe;
    #else
        envLighting.bakedGI = envLighting.lightingProbes;
    #endif

    #if defined(LIGHTMAP_ON) && defined(_MIXED_LIGHTING_SUBTRACTIVE)
        envLighting.bakedGI = SubtractDirectMainLightFromLightmap(mainlight, normalWS, envLighting.bakedGI);
    #endif

    #if defined(DYNAMICLIGHTMAP_ON)
        envLighting.bakedGI += envLighting.dynamicLightingMap;
    #endif
}
void EnvLighting_float(EnvLighting envLighting, out half3 StaticLightingMap, out half3 DynamicLightingMap, out half3 LightingProbes, out half3 VolumeProbe,
out half3 VertexSH, out half3 BakedGI)
{
    StaticLightingMap = envLighting.staticLightingMap;//静态光照贴图
    DynamicLightingMap = envLighting.dynamicLightingMap;//动态光照贴图
    LightingProbes = envLighting.lightingProbes;//SH
    VolumeProbe = envLighting.volumeProbe;
    VertexSH = envLighting.vertexSH;
    BakedGI = envLighting.bakedGI;//光照贴图 or SH;

}
//---------------------------------------------------------------------------------
real pow2_WB(real a)
{
    return a * a;
}
inline half Pow5(half x)
{
    return x * x * x * x * x;
}
half3 SeaSurfaceLighting(float3 viewDirWS, float3 normalWS, float3 tangentWS, float3 bitangentWS, Light mainlight,
float2 _slopeVariance, float _sunlightReflectionIntensity, float2 _sunlightReflectanceRange)
{
    half2 fSlopeVariance01 = saturate(_slopeVariance);

    half VdotN = dot(viewDirWS, normalWS);
    half VdotN01 = clamp(VdotN, 0.0001, 1.0);
    half VdotT = dot(viewDirWS, tangentWS);
    half VdotB = dot(viewDirWS, bitangentWS);

    half coeff = rsqrt(max(
        sqrt(max(1.0 / (VdotN01 * VdotN01) - 1.0, 0.00001)) *
        lerp(fSlopeVariance01.y, fSlopeVariance01.x, 1.0 / (pow2_WB(VdotT / VdotB) + 1.0)) * 2
        , 0.0001));

    coeff = max(coeff, 0.01);
    coeff = exp(-coeff * coeff) / (max(coeff, 0.01) * 3.5449) + 1.0;


    half3 halfDir = normalize(viewDirWS + mainlight.direction);
    half NdotL = dot(mainlight.direction, normalWS);
    half NdotL01 = clamp(NdotL, 0.0001, 1.0);
    half NdotH01 = clamp(dot(halfDir, normalWS), 0.0001, 1.0);
    half TdotL = dot(mainlight.direction, tangentWS);
    half BdotL = dot(mainlight.direction, bitangentWS);
    half fSlopeVariance01_xyMix_L = lerp(fSlopeVariance01.y, fSlopeVariance01.x, 1.0 / (pow2_WB(TdotL / BdotL) + 1.0));
    half BdotH = dot(halfDir, bitangentWS);
    half TdotH = dot(halfDir, tangentWS);
    half VdotH = dot(viewDirWS, halfDir);
    half BdotHdVdotH = BdotH / VdotH;
    half TdotHdVdotH = TdotH / VdotH;
    half coeff1 = rsqrt(max(
        sqrt(1.0 / NdotL01 * NdotL01 - 1.0) *
        lerp(fSlopeVariance01.y, fSlopeVariance01.x, 1.0 / (pow2_WB(TdotL / BdotL) + 1.0)) * 2,
        0.0001));


    coeff1 = exp(-coeff1 * coeff1) / max(coeff1, 0.01) * 3.5449;
    coeff1 = (coeff + coeff1) * VdotN01 * Pow4(NdotH01) * 4.0;

    half coeff2 = exp( - (BdotHdVdotH * BdotHdVdotH / fSlopeVariance01.x + TdotHdVdotH * TdotHdVdotH / fSlopeVariance01.y));
    coeff2 = coeff2 * (smoothstep(0.6, 0.9, NdotL * 0.5 + 0.5));
    coeff2 = Pow5(1.0 - VdotH) * coeff2 * _sunlightReflectionIntensity ;

    half waterLightingCoeff = clamp(coeff2 / coeff1, _sunlightReflectanceRange.x, _sunlightReflectanceRange.y);
    half3 waterLighting = waterLightingCoeff * mainlight.color * mainlight.shadowAttenuation;
    return waterLighting;
}

#endif