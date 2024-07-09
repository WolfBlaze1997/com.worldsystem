#ifndef UNIVERSAL_SPEEDTREE8_INPUT_INCLUDED
#define UNIVERSAL_SPEEDTREE8_INPUT_INCLUDED

#ifdef EFFECT_BUMP
    #define _NORMALMAP
#endif

// #define _ALPHATEST_ON

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
// #if defined(SUPPORT_SRP_BATCHING)
#if !defined(DISABLE_SRP_BATCHING)
    CBUFFER_START(UnityPerMaterial)
        float _WindEnabled;//
        float _GlobalWindTime;//
        half4 _BaseColor;//
        int _ObjectId;//
        int _PassValue;//
        half _Smoothness;//_Smoothness
        half _Metallic;//
        half4 _HueVariationColor;//
        half _BillboardShadowFade;//
        half4 _SubsurfaceColor;//
        half _SubsurfaceIndirect;//

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
    CBUFFER_END
#endif

#if defined(DISABLE_SRP_BATCHING)
    #if defined(ENABLE_WIND) && !defined(_WINDQUALITY_NONE)
        #define SPEEDTREE_Y_UP
        #include "SpeedTreeWind.cginc"
        float _WindEnabled;
        UNITY_INSTANCING_BUFFER_START(STWind)
        UNITY_DEFINE_INSTANCED_PROP(float, _GlobalWindTime)
        UNITY_INSTANCING_BUFFER_END(STWind)
    #endif

    half4 _BaseColor;
    // int _TwoSided;

    #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
    #endif

    float4 _MainTex_TexelSize;
    float4 _MainTex_MipInfo;

    half _Smoothness;
    half _Metallic;


    #ifdef EFFECT_HUE_VARIATION
        half4 _HueVariationColor;
    #endif

    #ifdef EFFECT_BILLBOARD
        half _BillboardShadowFade;
    #endif

    #ifdef EFFECT_SUBSURFACE
        half4 _SubsurfaceColor;
        half _SubsurfaceIndirect;
    #endif
#endif
#include "../FunctionLibrary/SpeedTreeWind.hlsl"

// TEXTURE2D(_BaseMap);
// SAMPLER(sampler_BaseMap);
sampler2D _MRAMap;
sampler2D _SubsurfaceTex;


// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

#define GEOM_TYPE_BRANCH 0
#define GEOM_TYPE_FROND 1
#define GEOM_TYPE_LEAF 2
#define GEOM_TYPE_FACINGLEAF 3

#define _Surface 0.0 // Speed Trees are always opaque

#endif
