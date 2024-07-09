#ifndef LIGHTING_MODEL_LIBRARY
#define LIGHTING_MODEL_LIBRARY
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "../FunctionLibrary/UtilityFunctionLibrary.hlsl"
#include "../Main/Input.hlsl"
#include "../FunctionLibrary/BaseFunctionLibrary.hlsl"

float4 GetShadowPositionHClipSupportShadowBias(float3 positionOS, half3 normalOS)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    float3 normalWS = TransformObjectToWorldNormal(normalOS);
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
        float3 lightDirectionWS = _LightDirection;
    #endif
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    #if UNITY_REVERSED_Z
        positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
        positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif
    return positionCS;
}

struct PBR
{
    half3 diffuseTerm;
    half3 specularTerm;
    // half3 GetDiffuseTerm(half3 brdfDiffuse, half satNdotL)
    // {
    //     return brdfDiffuse * NdotL;
    // }
    half3 GetSpecularTerm(half3 brdfSpecular, half satNdotL, half F, half V, half D)
    {
        return brdfSpecular * satNdotL * F * V * D / 4.0;
    }
    half3 GetSpecularTerm(half3 brdfSpecular, half satNdotL, half satNdotH, half satNdotV, half F, half G, half D)
    {
        half V = G / (satNdotH * satNdotV);
        return GetSpecularTerm(brdfSpecular, satNdotL, F, V, D);
    }
};

half UnityOptimizationsBRDF(Available available, BRDFData brdfData)
{
    float NoH = saturate(dot(float3(available.model.normal.WS), available.halfDir.WS));
    float LoH = half(saturate(dot(available.lights.mainlightDirection.WS, available.halfDir.WS)));
    // BRDFspec = (D * V * F) / 4.0
    // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
    // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
    half D = brdfData.roughness2 / Pow2(NoH * NoH * brdfData.roughness2MinusOne + 1.00001f);
    half V_F_Div4 = 1.0 / (max(LoH * LoH, 0.1) * brdfData.normalizationTerm);// (V*F/4.0)
    half specularTerm = D * V_F_Div4;
    #if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH)
        specularTerm = specularTerm - HALF_MIN;
        specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
    #endif
    return specularTerm;
}

half3 LightingPhysicallyBased(Available available, Light light, BRDFData brdfData, BRDFData brdfDataClearCoat, half clearCoatMask, bool specularHighlightsOff)
{
    half NdotL = saturate(dot(available.model.normal.WS, light.direction));
    half3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation) * NdotL;

    half3 brdf = brdfData.diffuse;
    #ifndef _SPECULARHIGHLIGHTS_OFF
        [branch] if (!specularHighlightsOff)
        {
            brdf += brdfData.specular * DirectBRDFSpecular(brdfData, available.model.normal.WS, light.direction, available.camera.viewDir.WS);
            #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
                // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
                // We rely on the compiler to merge these and compute them only once.
                half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, available.model.normal.WS, light.direction, available.camera.viewDir.WS);

                // Mix clear coat and base layer using khronos glTF recommended formula
                // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
                // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
                half NoV = saturate(dot(available.model.normal.WS, available.camera.viewDir.WS));
                // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
                // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
                half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);
                brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
            #endif // _CLEARCOAT

        }
    #endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 LightingPhysicallyBased(Available available, Light light, BRDFData brdfData, BRDFData brdfDataClearCoat,
half clearCoatMask, bool specularHighlightsOff, inout half3 outDirectDiffuse, inout half3 outDirectSpecular)
{
    half NdotL = saturate(dot(available.model.normal.WS, light.direction));
    half3 radiance = light.color * light.distanceAttenuation * NdotL;

    half3 directDiffuse = brdfData.diffuse;
    half3 directSpecular = 0;
    #ifndef _SPECULARHIGHLIGHTS_OFF
        [branch] if (!specularHighlightsOff)
        {
            directSpecular = brdfData.specular * DirectBRDFSpecular(brdfData, available.model.normal.WS, light.direction, available.camera.viewDir.WS);
            #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
                half CoatSpecular = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, available.model.normal.WS, light.direction, available.camera.viewDir.WS);
                half NoV = saturate(dot(available.model.normal.WS, available.camera.viewDir.WS));
                half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);
                half mask = 1.0 - clearCoatMask * coatFresnel;
                directDiffuse = directDiffuse * mask;
                directSpecular = directSpecular * mask + CoatSpecular * clearCoatMask;
            #endif
        }
    #endif
    // directDiffuse *= radiance;
    directDiffuse *= (radiance * light.shadowAttenuation);
    directSpecular *= (radiance * light.shadowAttenuation);
    
    outDirectDiffuse += directDiffuse;
    outDirectSpecular += directSpecular;
    return directDiffuse + directSpecular;
}


//支持forward+
half3 GlobalIllumination(Available available, BRDFData brdfData, BRDFData brdfDataClearCoat, half clearCoatMask, out half3 indirectDiffuse, out half3 indirectSpecular)
{
    half3 reflectVector = available.reflectDir.WS;
    half NdotV = saturate(dot(available.model.normal.WS, available.camera.viewDir.WS));
    half fresnelTerm = Pow4(1.0 - NdotV);
    indirectDiffuse = available.envLighting.bakedGI * brdfData.diffuse;
    indirectSpecular = available.envReflect.bakedReflect * EnvironmentBRDFSpecular(brdfData, fresnelTerm);

    //开启清漆时的间接光光照
    #if defined(_CLEARCOAT)
        #if USE_FORWARD_PLUS
            half3 coatIndirectSpecular = GlossyEnvironmentReflection(available.reflectDir.WS, available.model.position.WS, brdfDataClearCoat.perceptualRoughness, 1.0h, available.model.position.SS.xy);
            //发现forward+的反射探针混合在某些情况下会出现BUG, 暂时使用forward的算法;
            // half3 coatIndirectSpecular = GlossyEnvironmentReflection(available.reflectDir.WS, available.model.position.WS, brdfDataClearCoat.perceptualRoughness, 1.0h);
        #else
            half3 coatIndirectSpecular = GlossyEnvironmentReflection(available.reflectDir.WS, available.model.position.WS, brdfDataClearCoat.perceptualRoughness, 1.0h);
        #endif
        half3 coatSpecular = coatIndirectSpecular * EnvironmentBRDFSpecular(brdfDataClearCoat, fresnelTerm) * clearCoatMask;
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
        half mask = 1.0 - coatFresnel * clearCoatMask;
        indirectDiffuse = indirectDiffuse * mask;
        indirectSpecular = indirectSpecular * mask + coatSpecular;
    #endif
    half3 giColor = indirectDiffuse + indirectSpecular;
    if (IsOnlyAOLightingFeatureEnabled())
    {
        giColor = half3(1, 1, 1); // "Base white" for AO debug lighting mode
        indirectDiffuse = 0.5;
        indirectSpecular = 0.5;
    }
    return giColor;
}


struct LightingTerm
{
    half3 indirectDiffuse;
    half3 indirectSpecular;
    half3 mainDirectDiffuse;
    half3 mainDirectSpecular;
    half3 addDirectDiffuse;
    half3 addDirectSpecular;
    half3 emission;
    half3 vertexLighting;
};
LightingTerm GetLightingTerm(Available available, BRDFData brdfData, BRDFData brdfDataClearCoat, half clearCoatMask, half3 emission)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
        bool specularHighlightsOff = true;
    #else
        bool specularHighlightsOff = false;
    #endif

    LightingTerm lightingTerm = (LightingTerm)0;
    lightingTerm.emission = emission;
    //间接光光照
    GlobalIllumination(available, brdfData, brdfDataClearCoat, clearCoatMask, lightingTerm.indirectDiffuse, lightingTerm.indirectSpecular);

    //主灯光光照
    if (IsMatchingLightLayer(available.lights.mainlight.layerMask, available.model.renderLayer))
    {
        LightingPhysicallyBased(available, available.lights.mainlight, brdfData, brdfDataClearCoat,
        clearCoatMask, specularHighlightsOff, lightingTerm.mainDirectDiffuse, lightingTerm.mainDirectSpecular);
    }

    //附加灯光光照
    #if defined(_ADDITIONAL_LIGHTS)
        half3 forwardPlusAddDirectdiffuse = 0;
        half3 forwardPlusAddDirectSpecular = 0;
        //支持forward+
        #if USE_FORWARD_PLUS
            for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
            {
                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
                Light light = available.lights.GetAdditionalLight_Ref(lightIndex, available.model.position.WS);

                #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, available.model.renderLayer))
                #endif
                {
                    // lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                    // inputData.normalWS, inputData.viewDirectionWS,
                    // surfaceData.clearCoatMask, specularHighlightsOff);
                    LightingPhysicallyBased(available, light, brdfData, brdfDataClearCoat,
                    clearCoatMask, specularHighlightsOff, forwardPlusAddDirectdiffuse, forwardPlusAddDirectSpecular);
                }
            }
        #endif

        half3 clusteredAddDirectdiffuse = 0;
        half3 clusteredAddDirectSpecular = 0;
        #if USE_CLUSTERED_LIGHTING && !USE_FORWARD_PLUS
            for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
            {
                Light light = available.lights.GetAdditionalLight_Ref(lightIndex, available.model.position.WS);
                if (IsMatchingLightLayer(light.layerMask, available.model.renderLayer))
                {
                    LightingPhysicallyBased(available, light, brdfData, brdfDataClearCoat,
                    clearCoatMask, specularHighlightsOff, clusteredAddDirectdiffuse, clusteredAddDirectSpecular);
                }
            }
        #endif

        //重新定义LIGHT_LOOP_BEGIN/LIGHT_LOOP_END,
        #undef LIGHT_LOOP_BEGIN
        #undef LIGHT_LOOP_END
        #if USE_FORWARD_PLUS
            #define LIGHT_LOOP_BEGIN(lightCount) { \
            uint lightIndex; \
            ClusterIterator _urp_internal_clusterIterator = ClusterInit(available.model.position.SS.xy, available.model.position.WS, 0); \
            [loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) { \
            lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT; \
            FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
            #define LIGHT_LOOP_END } }
        #else
            #define LIGHT_LOOP_BEGIN(lightCount) \
            for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex) {
                #define LIGHT_LOOP_END }
        #endif

        half3 addDirectdiffuse = 0;
        half3 addDirectSpecular = 0;
        LIGHT_LOOP_BEGIN(available.lights.addlightCount)
        // for (uint lightIndex = 0; lightIndex < available.lights.addlightCount; lightIndex++)
        // {
        Light light = available.lights.GetAdditionalLight_Ref(lightIndex, available.model.position.WS);
        #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, available.model.renderLayer))
        #endif
        {
            LightingPhysicallyBased(available, light, brdfData, brdfDataClearCoat,
            clearCoatMask, specularHighlightsOff, addDirectdiffuse, addDirectSpecular);
        }
        // }
        
        LIGHT_LOOP_END

        lightingTerm.addDirectDiffuse = forwardPlusAddDirectdiffuse + clusteredAddDirectdiffuse + addDirectdiffuse;
        lightingTerm.addDirectSpecular = forwardPlusAddDirectSpecular + clusteredAddDirectSpecular + addDirectSpecular;
    #endif

    //顶点光照
    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
        lightingTerm.vertexLighting += available.vertexLighting * brdfData.diffuse;
    #endif

    return lightingTerm;
}

void OcclusionToLightingTerm(EnvOcclusion envOcclusion, inout LightingTerm lightingTerm, half3 albedo)
{
    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_OCCLUSION)
        #ifdef _AO_MULTI_BOUNCE
            // //多重反弹AO,根据albedo模拟ao反弹的颜色,效果较弱,酌情使用
            lightingTerm.mainDirectDiffuse *= envOcclusion.GTAOMultiBounce(envOcclusion.mixDirectDiffuseAO, albedo);
            lightingTerm.mainDirectSpecular *= envOcclusion.mixDirectSpecularAO;
            lightingTerm.addDirectDiffuse *= envOcclusion.GTAOMultiBounce(envOcclusion.mixDirectDiffuseAO, albedo);
            lightingTerm.addDirectSpecular *= envOcclusion.mixDirectSpecularAO;
            lightingTerm.indirectDiffuse *= envOcclusion.GTAOMultiBounce(envOcclusion.mixIndirectDiffuserAO, albedo);
            lightingTerm.indirectSpecular *= envOcclusion.mixIndirectSpecularAO;
        #else
            lightingTerm.mainDirectDiffuse *= envOcclusion.mixDirectDiffuseAO;
            lightingTerm.mainDirectSpecular *= envOcclusion.mixDirectSpecularAO;
            lightingTerm.addDirectDiffuse *= envOcclusion.mixDirectDiffuseAO;
            lightingTerm.addDirectSpecular *= envOcclusion.mixDirectSpecularAO;
            lightingTerm.indirectDiffuse *= envOcclusion.mixIndirectDiffuserAO;
            lightingTerm.indirectSpecular *= envOcclusion.mixIndirectSpecularAO;
        #endif
    #endif
}


LightingData GetLightingData(LightingTerm lightingTerm)
{
    LightingData lightingData = (LightingData)0;
    lightingData.mainLightColor = lightingTerm.mainDirectDiffuse + lightingTerm.mainDirectSpecular;
    lightingData.additionalLightsColor = lightingTerm.addDirectDiffuse + lightingTerm.addDirectSpecular;
    lightingData.giColor = lightingTerm.indirectDiffuse + lightingTerm.indirectSpecular;
    lightingData.emissionColor = lightingTerm.emission;
    lightingData.vertexLightingColor = lightingTerm.vertexLighting;
    return lightingData;
}


struct LightingModel
{
    half3 BlinnPhong;
    half3 PBR;
};


#endif