#ifndef WANG_INPUT
#define WANG_INPUT
//URP12中的GetMeshRenderingLightLayer函数已被改用GetMeshRenderingLayer,这个define可以帮助在URP12中保持兼容性
#define GetMeshRenderingLightLayer GetMeshRenderingLayer
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"


CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half3 _SpecColor;
    half3 _EmissionColor;
    half _Cutoff;
    half _Roughness;
    half _Metallic;
    half _BumpScale;
    half _Parallax;
    half _OcclusionStrength;
    half _HorizonOcclusion;
    half _ClearCoatMask;
    half _ClearCoatSmoothness;
    half4 _SubsurfaceColor;//
    half _SubsurfaceIndirect;//
    half4 _HueVariationColor;
    half _ScaleOrRotate;
    bool _CustomReflect;
    half4 _CustomReflectMap_HDR;

    float4 _DetailMap0_ST;
    half _DetailOcclusionStrength0;
    half _DetailNormalScale0;
    float4 _DetailMap1_ST;
    half _DetailOcclusionStrength1;
    half _DetailNormalScale1;
    float4 _DetailMap2_ST;
    half _DetailOcclusionStrength2;
    half _DetailNormalScale2;
    float4 _DetailMap3_ST;
    half _DetailOcclusionStrength3;
    half _DetailNormalScale3;
    
    int _ObjectId;
    int _PassValue;

    float WindEnabled_Enum;
    float _GlobalWindTime;
    float4 _ST_WindVector;
    float4 _ST_WindGlobal;
    float4 _ST_WindBranch;
    float4 _ST_WindBranchTwitch;
    float4 _ST_WindBranchWhip;
    float4 _ST_WindBranchAnchor;
    float4 _ST_WindBranchAdherences;
    float4 _ST_WindTurbulences;
    float4 _ST_WindLeaf1Ripple;
    float4 _ST_WindLeaf1Tumble;
    float4 _ST_WindLeaf1Twitch;
    float4 _ST_WindLeaf2Ripple;
    float4 _ST_WindLeaf2Tumble;
    float4 _ST_WindLeaf2Twitch;
    float4 _ST_WindFrondRipple;
    float4 _ST_WindAnimation;
    
    // float4 _ST_WindVectorHistory;
    // float4 _ST_WindGlobalHistory;
    // float4 _ST_WindBranchHistory;
    // float4 _ST_WindBranchTwitchHistory;
    // float4 _ST_WindBranchWhipHistory;
    // float4 _ST_WindBranchAnchorHistory;
    // float4 _ST_WindBranchAdherencesHistory;
    // float4 _ST_WindTurbulencesHistory;
    // float4 _ST_WindLeaf1RippleHistory;
    // float4 _ST_WindLeaf1TumbleHistory;
    // float4 _ST_WindLeaf1TwitchHistory;
    // float4 _ST_WindLeaf2RippleHistory;
    // float4 _ST_WindLeaf2TumbleHistory;
    // float4 _ST_WindLeaf2TwitchHistory;
    // float4 _ST_WindFrondRippleHistory;
    // float4 _ST_WindAnimationHistory;


    half _debugFloat01;
    float _debugFloat02;
    float _debugFloat03;

    half4 _UseExtraMapChannel_Alpha;
    half4 _UseMixMapChannel_Parallax;
    half4 _UseMixMapChannel_ClearCoatMask;
    half4 _UseMixMapChannel_ClearCoatPerceptualRoughness;
    half4 _UseMixMapChannel_Noise;

    half _MainDirectDiffuseStrength;
    half _MainDirectSpecularStrength;
    half _AddDirectDiffuseStrength;
    half _AddDirectSpecularStrength;
    half _IndirectDiffuseStrength;
    half _IndirectSpecularStrength;

    //houdini VAT
    bool _AutoPlay;
    float _DisplayFrame;
    float _PlaySpeed;
    float _AnimatorStrength;
    bool _IsPosTexHDR;

    float _HoudiniFPS;
    float _FrameCount;
    float _BoundMax_X;
    float _BoundMax_Y;
    float _BoundMax_Z;
    float _BoundMin_X;
    float _BoundMin_Y;
    float _BoundMin_Z;
CBUFFER_END
//UnityBuildTexture2DStruct()需要这些定义,如果着色器中未使用这些参数(一般只用于debug),编译器将自动排除,无需担心破坏 SRP Batching 程序
float4 _BaseMap_TexelSize;//(1/width,1/height,width,height)
float4 _BaseMap_MipInfo;
float4 _NRAMap_ST;
float4 _NRAMap_TexelSize;
float4 _NRAMap_MipInfo;
float4 _EmissionMixMap_ST;
float4 _EmissionMixMap_TexelSize;
float4 _EmissionMixMap_MipInfo;
float4 _ExtraMixMap_ST;
float4 _ExtraMixMap_TexelSize;
float4 _ExtraMixMap_MipInfo;

float4 _HeightMap_ST;
float4 _HeightMap_TexelSize;
float4 _HeightMap_MipInfo;
float4 _ClearCoatMap_ST;
float4 _ClearCoatMap_TexelSize;
float4 _ClearCoatMap_MipInfo;

float4 _DetailMap0_TexelSize;
float4 _DetailMap0_MipInfo;
float4 _DetailMap1_TexelSize;
float4 _DetailMap1_MipInfo;
float4 _DetailMap2_TexelSize;
float4 _DetailMap2_MipInfo;
float4 _DetailMap3_TexelSize;
float4 _DetailMap3_MipInfo;

float4 _NoiseMap_ST;
float4 _NoiseMap_TexelSize;
float4 _NoiseMap_MipInfo;

float4 _PositionVATMap_ST;
float4 _PositionVATMap_TexelSize;
float4 _PositionVATMap_MipInfo;

float4 _RotateVATMap_ST;
float4 _RotateVATMap_TexelSize;
float4 _RotateVATMap_MipInfo;

// float4 _SubsurfaceMap_ST;
// float4 _SubsurfaceMap_TexelSize;
// float4 _SubsurfaceMap_MipInfo;
// #ifdef UNITY_DOTS_INSTANCING_ENABLED
//     UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//     UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_ST)
//     UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_TexelSize)
//     UNITY_DOTS_INSTANCED_PROP(float4, _NormalMap_ST)
//     UNITY_DOTS_INSTANCED_PROP(float4, _NormalMap_TexelSize)
//     UNITY_DOTS_INSTANCED_PROP(float4, _RMTMap_ST)
//     UNITY_DOTS_INSTANCED_PROP(float4, _RMTMap_TexelSize)
//     UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
//     UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
//     UNITY_DOTS_INSTANCED_PROP(float, _BumpScale)
//     UNITY_DOTS_INSTANCED_PROP(float, _debugFloat01)
//     UNITY_DOTS_INSTANCED_PROP(float, _debugFloat02)
//     UNITY_DOTS_INSTANCED_PROP(float, _debugFloat03)
//     UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

//     #define _BaseMap_ST              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_BaseMap_ST)
//     #define _BaseMap_TexelSize              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_BaseMap_TexelSize)
//     #define _NormalMap_ST          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4, Metadata_NormalMap_ST)
//     #define _NormalMap_TexelSize                 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_NormalMap_TexelSize)
//     #define _RMTMap_ST             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RMTMap_ST)
//     #define _RMTMap_TexelSize               UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_RMTMap_TexelSize)
//     #define _BaseColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_BaseColor)
//     #define _EmissionColor               UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_EmissionColor)
//     #define _BumpScale      UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_BumpScale)
//     #define _debugFloat01 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_debugFloat01)
//     #define _debugFloat02 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_debugFloat02)
//     #define _debugFloat03 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float, Metadata_debugFloat03)
// #endif

TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
TEXTURE2D(_NRAMap);SAMPLER(sampler_NRAMap);
TEXTURE2D(_EmissionMixMap);SAMPLER(sampler_EmissionMixMap);
TEXTURE2D(_ExtraMixMap);SAMPLER(sampler_ExtraMixMap);
TEXTURECUBE(_CustomReflectMap);SAMPLER(sampler_CustomReflectMap);

TEXTURE2D(_DetailMap0);SAMPLER(sampler_DetailMap0);
TEXTURE2D(_DetailMap1);SAMPLER(sampler_DetailMap1);
TEXTURE2D(_DetailMap2);SAMPLER(sampler_DetailMap2);
TEXTURE2D(_DetailMap3);SAMPLER(sampler_DetailMap3);

TEXTURE2D(_HeightMap);SAMPLER(sampler_HeightMap);
TEXTURE2D(_ClearCoatMap);SAMPLER(sampler_ClearCoatMap);
TEXTURE2D(_NoiseMap);SAMPLER(sampler_NoiseMap);

TEXTURE2D(_PositionVATMap);SAMPLER(sampler_PositionVATMap);
TEXTURE2D(_RotateVATMap);SAMPLER(sampler_RotateVATMap);


float3 _LightDirection;
float3 _LightPosition;


// #include "../FunctionLibrary/UnityTextureLibrary.hlsl"
// #include "../FunctionLibrary/UtilityFunctionLibrary.hlsl"
// #include "../FunctionLibrary/BaseFunctionLibrary.hlsl"
// #include "../FunctionLibrary/LightingModelLibrary.hlsl"



///////////////////////////////////////////////////////////////////////////////
//                      Material Property Helpers                            //
///////////////////////////////////////////////////////////////////////////////
// half Alpha(half albedoAlpha, half4 color, half cutoff)
// {
//     // #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
//     half alpha = albedoAlpha * color.a;
//     // #else
//     //     half alpha = color.a;
//     // #endif

//     #if defined(_ALPHATEST_ON)
//         clip(alpha - cutoff);
//     #endif

//     return alpha;
// }

// half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
// {
//     return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
// }

// half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
// {
//     // #ifdef _NORMALMAP
//     half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
//     #if BUMP_SCALE_NOT_SUPPORTED
//         return UnpackNormal(n);
//     #else
//         return UnpackNormalScale(n, scale);
//     #endif
//     // #else
//     //     return half3(0.0h, 0.0h, 1.0h);
//     // #endif

// }

// half3 SampleEmission(float2 uv, half3 emissionColor, TEXTURE2D_PARAM(emissionMap, sampler_emissionMap))
// {
//     #ifndef _EMISSION
//         return 0;
//     #else
//         return SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb * emissionColor;
//     #endif
// }


// // #ifdef _SPECULAR_SETUP
// //     #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
// // #else
// #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_EmissionMixMap, sampler_EmissionMixMap, uv)
// // #endif  r:metallic a:smoothness

// half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
// {
//     half4 specGloss;

//     // #ifdef _METALLICSPECGLOSSMAP
//     specGloss = half4(SAMPLE_METALLICSPECULAR(uv));
//     // #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//     //     specGloss.a = albedoAlpha * _Roughness;
//     // #else
//         specGloss.a = (1.0 - specGloss.g) * _Roughness;
//     specGloss.g = 0.0;
//     specGloss.b = 0.0;

//     // #endif
//     // #else // _METALLICSPECGLOSSMAP
//     //     #if _SPECULAR_SETUP
//     //         specGloss.rgb = _SpecColor.rgb;
//     //     #else
//     //         specGloss.rgb = _Metallic.rrr;
//     //     #endif

//     //     #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//     //         specGloss.a = albedoAlpha * _Roughness;
//     //     #else
//     //         specGloss.a = _Roughness;
//     //     #endif
//     // #endif

//     return specGloss;
// }

// half SampleOcclusion(float2 uv)
// {
//     // #ifdef _OCCLUSIONMAP
//     // TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
//     // #if defined(SHADER_API_GLES)
//     return half4(SAMPLE_METALLICSPECULAR(uv)).b;
//     // #else
//     //     half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
//     //     return LerpWhiteTo(occ, _OcclusionStrength);
//     // #endif
//     // #else
//     //     return half(1.0);
//     // #endif

// }

// Returns clear coat parameters
// .x/.r == mask
// .y/.g == smoothness
// half2 SampleClearCoat(float2 uv)
// {
//     #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
//         half2 clearCoatMaskSmoothness = half2(_ClearCoatMask, _ClearCoatSmoothness);

//         #if defined(_CLEARCOATMAP)
//             clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
//         #endif

//         return clearCoatMaskSmoothness;
//     #else
//         return half2(0.0, 1.0);
//     #endif  // _CLEARCOAT

// }

// void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
// {
//     #if defined(_PARALLAXMAP)
//         uv += ParallaxMapping(TEXTURE2D_ARGS(_HeightMap, sampler_HeightMap), viewDirTS, _Parallax, uv);
//     #endif
// }

// // Used for scaling detail albedo. Main features:
// // - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// // - No effect is applied if detailAlbedo is 0.5.
// half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
// {
//     // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
//     // detailAlbedo *= _DetailAlbedoMapScale;
//     // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
//     // return detailAlbedo * 2.0f;

//     // A bit more optimized
//     return half(2.0) * detailAlbedo * scale - scale + half(1.0);
// }

// half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
// {
//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)
//         half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;
//         detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
//         return albedo * LerpWhiteTo(detailAlbedo, detailMask);
//     #else
//         return albedo;
//     #endif
// }
// half3 ApplyDetailAlbedo(half3 detailAlbedo, half3 albedo, half detailMask, half detailAlbedoMapScale)
// {
//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)
//         detailAlbedo = ScaleDetailAlbedo(detailAlbedo, detailAlbedoMapScale);
//         return albedo * LerpWhiteTo(detailAlbedo, detailMask);
//     #else
//         return albedo;
//     #endif
// }
// half3 ApplyDetailOcclusion(half detailOcclusion, half occlusion, half detailMask, half detailAlbedoMapScale)
// {
//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)
//         // detailAlbedo = ScaleDetailAlbedo(detailAlbedo, detailAlbedoMapScale);
//         return min(occlusion, LerpWhiteTo(detailOcclusion, detailMask));
//     #else
//         return occlusion;
//     #endif
// }

// half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
// {
//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)
//         #if BUMP_SCALE_NOT_SUPPORTED
//             half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
//         #else
//             half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
//         #endif

//         // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
//         // For visual consistancy we going to do in all cases
//         detailNormalTS = normalize(detailNormalTS);

//         return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
//     #else
//         return normalTS;
//     #endif
// }
// half3 ApplyDetailNormal_modif (half3 detailNormalTS, half3 normalTS, half detailMask)
// {
//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)

//         // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
//         // For visual consistancy we going to do in all cases
//         detailNormalTS = normalize(detailNormalTS);

//         return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
//     #else
//         return normalTS;
//     #endif
// }

// inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
// {
//     half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
//     outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

//     half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
//     outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

//     #if _SPECULAR_SETUP
//         outSurfaceData.metallic = half(1.0);
//         outSurfaceData.specular = specGloss.rgb;
//     #else
//         outSurfaceData.metallic = specGloss.r;
//         outSurfaceData.specular = half3(0.0, 0.0, 0.0);
//     #endif

//     outSurfaceData.smoothness = specGloss.a;
//     outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BaseMap), _BumpScale);
//     outSurfaceData.occlusion = SampleOcclusion(uv);
//     outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_BaseMap));

//     #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
//         half2 clearCoat = SampleClearCoat(uv);
//         outSurfaceData.clearCoatMask = clearCoat.r;
//         outSurfaceData.clearCoatSmoothness = clearCoat.g;
//     #else
//         outSurfaceData.clearCoatMask = half(0.0);
//         outSurfaceData.clearCoatSmoothness = half(0.0);
//     #endif

//     #if defined(_DETAIL)
//         half detailMask = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, uv).a;
//         float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
//         outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
//         outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
//     #endif
// }


#endif