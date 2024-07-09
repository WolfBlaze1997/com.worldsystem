#ifndef VERTEX_AND_FRAGMENT_FUNCTION
#define VERTEX_AND_FRAGMENT_FUNCTION

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "../FunctionLibrary/UnityTextureLibrary.hlsl"
#include "../FunctionLibrary/BaseFunctionLibrary.hlsl"
#include "../FunctionLibrary/UtilityFunctionLibrary.hlsl"
#include "../Main/Input.hlsl"
#include "../FunctionLibrary/LightingModelLibrary.hlsl"
#include "../FunctionLibrary/SpeedTree8Wind.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"


void VertexAnimator(Attributes input, inout float3 positionOS, inout half3 normalOS, inout half4 tangentOS)
{

    #if defined(_HOUDINI_VAT_SOFT)
        HuodiniVAT_Soft(input.texcoord3.xy, _PositionVATMap, sampler_PositionVATMap, _RotateVATMap, sampler_RotateVATMap, _IsPosTexHDR, _TimeParameters.x, _AutoPlay, _DisplayFrame, _PlaySpeed, _AnimatorStrength,
        _HoudiniFPS, _FrameCount, _BoundMax_X, _BoundMax_Y, _BoundMax_Z, _BoundMin_X, _BoundMin_Y, _BoundMin_Z, positionOS, normalOS, tangentOS.xyz);
    #endif

    //在此处计算顶点动画
    // 植物风动画
    #if defined(_SHADER_PLANT)
        int iWindQuality = WindEnabled_Enum;

        bool bBillboard = false;

        #if defined(LOD_FADE_CROSSFADE)
            bool bCrossfade = true;
        #else
            bool bCrossfade = false;
        #endif

        bool bHistory = false;
        SpeedTreeWind_float(positionOS, normalOS, input.texcoord, input.texcoord1, input.texcoord2, input.texcoord3, iWindQuality, bBillboard, bCrossfade, bHistory, positionOS);
    #endif
}

half4 FragmentShading(Varyings input, bool vFace)
{
    //转化贴图
    UnityTexture2D BaseMap = UnityBuildTexture2DStruct(_BaseMap);
    UnityTexture2D NRAMap = UnityBuildTexture2DStruct(_NRAMap);//rgb:albedo, a:occlusion
    UnityTexture2D EmissionMixMap = UnityBuildTexture2DStruct(_EmissionMixMap);
    UnityTexture2D ExtraMixMap = UnityBuildTexture2DStruct(_ExtraMixMap);
    UnityTexture2D HeightMap = UnityBuildTexture2DStruct(_HeightMap);
    UnityTexture2D DetailMap0 = UnityBuildTexture2DStruct(_DetailMap0);
    UnityTexture2D DetailMap1 = UnityBuildTexture2DStruct(_DetailMap1);
    UnityTexture2D DetailMap2 = UnityBuildTexture2DStruct(_DetailMap2);
    UnityTexture2D DetailMap3 = UnityBuildTexture2DStruct(_DetailMap3);
    UnityTexture2D ClearCoatMap = UnityBuildTexture2DStruct(_ClearCoatMap);
    UnityTexture2D NoiseMap = UnityBuildTexture2DStruct(_NoiseMap);


    //noise将在消除纹理平铺时使用
    half noiseTex = GradientNoiseGenerate(input.texcoord.xy, _ScaleOrRotate);
    #if defined(_SAMPLE_NOISETILING_USE_NOISEMAP)
        noiseTex = NoiseMap.Sample(input.texcoord.xy * _ScaleOrRotate).r * 2 - 1;
    #endif
    //UV,视差
    float2 baseUV = input.texcoord.xy;
    #if defined(_PARALLAXMAP)
        #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            half parallax = HeightMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex).r;
            #if defined(_USE_EXTRAMIXMAP_PARALLAX)
                parallax = dot(ExtraMixMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex), _UseMixMapChannel_Parallax);
            #endif
            baseUV += ParallaxOffset1Step(parallax, _Parallax, input.viewDirTS);
        #endif
    #endif

    //采样贴图,根据不同的着色器模型从贴图中解码信息
    //通用PBR: (albedo,alpha) (normal,roughness,AO) (emission,metallic)
    //植物 (albedo,alpha) (normal,roughness,AO) (emission,SubSurfaceWeight) metallic = 0;
    //通用采样解码
    half4 baseTex = BaseMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex);
    half3 albedo = baseTex.rgb;
    half alpha = baseTex.a;
    half4 NRATex = NRAMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex);
    half3 normalTS = NRAMap.DecodeNormalRG(NRATex.rg, _BumpScale);
    half perceptualRoughness = NRATex.b;
    half occlusion = NRATex.a;
    //混合贴图采样解码
    half4 EmissionMixTex = EmissionMixMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex);
    half4 ExtraMixTex = ExtraMixMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex);
    half3 emission = 0.0;
    half metallic = 1.0;
    half subSurfaceWeight = 0.0;
    //根据不同着色器模型解码
    #if defined(_SHADER_PBR)
        emission = EmissionMixTex.rgb;
        metallic = EmissionMixTex.a;
    #elif defined(_SHADER_PLANT)
        emission = EmissionMixTex.rgb;
        subSurfaceWeight = EmissionMixTex.a;
        metallic = 0.0;
    #endif

    //高度
    half height = HeightMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex).r;
    #if defined(_USE_EXTRAMIXMAP_PARALLAX)
        height = dot(ExtraMixTex, _UseMixMapChannel_Parallax);
    #endif
    //清漆
    half4 ClearCoatTex = ClearCoatMap.SampleSupportNoTileing(baseUV, input.positionCS.xy, _ScaleOrRotate, noiseTex);
    half clearCoatMask = ClearCoatTex.r;
    half clearCoatPerceptualRoughness = ClearCoatTex.g;

    //从贴图构建表面数据,SurfaceDataExtend包含了能从贴图中获得的所有数据,需要其他贴图扩充SurfaceDataExtend结构即可
    SurfaceDataExtend surfaceDataEx = GetSurfaceDataExtend(
        albedo, //half3 albedo 反照率
        perceptualRoughness, //half perceptualRoughness
        metallic, //half metallic
        normalTS, //half3 normalTS
        emission, //half3 emission
        occlusion, //half occlusion
        alpha, //half alpha
        height, //half height
        clearCoatMask, //half clearCoatMask
        clearCoatPerceptualRoughness, //half clearCoatPerceptualRoughness
        _Roughness, //控制光滑度,perceptualSmoothness = saturate((1.0-perceptualRoughness)*_Roughness); _Roughness
        _BaseColor, //控制diffuse,diffuse = diffuse * _BaseColor;
        _SpecColor, //控制specular,specular = specular * _SpecColor;
        _Metallic, //控制金属度,metallic = metallic * _Metallic;
        _EmissionColor, //控制自发光颜色,emission = emissionTex * _EmissionColor;
        _ClearCoatMask, //控制清漆遮罩,ClearCoatMask = ClearCoatMask * _ClearCoatMask
        _ClearCoatSmoothness, //控制清漆光滑度,clearCoatSmoothness = saturate((1.0-clearCoatPerceptualRoughness)*_ClearCoatSmoothness);
    _OcclusionStrength//控制AO贴图强度,occlusion = saturate(lerp(1.0, occlusion, _OcclusionStrength));
    );
    surfaceDataEx.SetupSubsurfaceColor(subSurfaceWeight * _SubsurfaceColor.rgb * saturate(albedo + 1.0 - _SubsurfaceColor.a));

    //应用细节到surfaceDataEx
    #if defined(_DETAIL) || defined(_DETAIL_2MULTI) || defined(_DETAIL_4MULTI)
        ApplyDetailToSurfaceDataExtend(surfaceDataEx, baseUV, DetailMap0, _ScaleOrRotate, noiseTex, _DetailOcclusionStrength0, _DetailNormalScale0);
        #if defined(_DETAIL_2MULTI) || defined(_DETAIL_4MULTI)
            ApplyDetailToSurfaceDataExtend(surfaceDataEx, baseUV, DetailMap1, _ScaleOrRotate, noiseTex, _DetailOcclusionStrength1, _DetailNormalScale1);
            #if  defined(_DETAIL_4MULTI)
                ApplyDetailToSurfaceDataExtend(surfaceDataEx, baseUV, DetailMap2, _ScaleOrRotate, noiseTex, _DetailOcclusionStrength2, _DetailNormalScale2);
                ApplyDetailToSurfaceDataExtend(surfaceDataEx, baseUV, DetailMap3, _ScaleOrRotate, noiseTex, _DetailOcclusionStrength3, _DetailNormalScale3);
            #endif
        #endif
    #endif

    //背面翻转法线
    #ifdef EFFECT_BACKSIDE_NORMALS
        surfaceDataEx.normalTS.z = IS_FRONT_VFACE(vFace, surfaceDataEx.normalTS.z, -surfaceDataEx.normalTS.z);
    #endif

    // adjust billboard normals to improve GI and matching
    #ifdef EFFECT_BILLBOARD
        surfaceDataEx.normalTS.z *= 0.5;
        surfaceDataEx.normalTS = normalize(surfaceDataEx.normalTS);
    #endif
    
    //获得模型,包含了能从模型中获得的所有数据
    Model model = GetModel(input, vFace, surfaceDataEx.normalTS);
    model.UpdateModelUV(baseUV);


    //应用贴花decal到surfaceDataEx和model
    #ifdef _DBUFFER
        surfaceDataEx = ApplyDecalToSurfaceDataExtend(surfaceDataEx, model.normal.WS, model.position.DS);//同时更新了模型中的法线
    #endif

    // 色调变体
    #ifdef EFFECT_HUE_VARIATION
        half3 shiftedColor = lerp(surfaceDataEx.albedo, _HueVariationColor.rgb, input.interpolator.g);
        // preserve vibrance
        half maxBase = max(surfaceDataEx.albedo.r, max(surfaceDataEx.albedo.g, surfaceDataEx.albedo.b));
        half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
        maxBase /= newMaxBase;
        maxBase = maxBase * 0.5f + 0.5f;
        shiftedColor.rgb *= maxBase;
        surfaceDataEx.albedo = saturate(shiftedColor);
        surfaceDataEx.UpdateAllData();
    #endif

    //数据采集完毕------------------------------------------------------------------------------------------------------
    //★获得Available(可获得的),Available内包含了,默认情况下shader能够从unity中获得的所有数据
    Available available = GetAvailable(input, model, surfaceDataEx.perceptualRoughness,
    _CustomReflect, _CustomReflectMap, sampler_CustomReflectMap, _CustomReflectMap_HDR, surfaceDataEx.occlusion, _HorizonOcclusion);
    SETUP_DEBUG_TEXTURE_DATA(available.inputData, baseUV, _BaseMap);//安装_BaseMap的纹理debug数据

    //alphaTest
    #if defined(_ALPHATEST_ON)
        #if defined(EFFECT_BILLBOARD)
            // clip(surfaceDataEx.alpha * input.interpolator.a * dot(available.model.normal.WS,available.lights.mainlightDirection.WS) - 0.3333);
            clip(surfaceDataEx.alpha * input.interpolator.a - 0.3333);
        #else
            clip(surfaceDataEx.alpha - _Cutoff);
        #endif
        //我们只在开启alphaTest时,启用Lod淡入淡出,不要给普通的不透明物体使用LODDithering,以免造成性能损失
        //支持Lod交叉淡入淡出,需要统一LODGroup的淡入淡出设置,以免出现不同的着色器变体,破坏SRP批处理
        #if !defined(SHADER_QUALITY_LOW) && !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS) && !defined(SCENESELECTION_PASS)
            #if defined(LOD_FADE_CROSSFADE)
                // LODDitheringTransition(ComputeFadeMaskSeed(available.camera.viewDir.WS, available.model.position.SS.xy), unity_LODFade.x);
                //urp15优化了CrossFade,现在使用采样贴图,而不是随机数算法
                LODFadeCrossFade(available.model.position.DS);
            #endif
        #endif
    #endif
    // return (input.normalWS.xyz,1.0);
    //获得BRDFData
    BRDFData brdfData = surfaceDataEx.GetBRDFData();
    //支持调试渲染
    #if defined(DEBUG_DISPLAY)
        half4 debugColor;
        if (SupportDebugDisplay(available.inputData, surfaceDataEx, brdfData, debugColor))
        {
            return debugColor;
        }
    #endif
    //获得清漆BRDFData
    BRDFData brdfDataClearCoat = surfaceDataEx.GetBRDFDataClearCoat(brdfData);
    //计算光照
    LightingTerm lightingTerm = GetLightingTerm(available, brdfData, brdfDataClearCoat, surfaceDataEx.clearCoatMask, surfaceDataEx.emission);
    //光照项控制
    // return half4(available.envLighting.bakedGI, 1.0);
    lightingTerm.mainDirectDiffuse *= _MainDirectDiffuseStrength;
    lightingTerm.mainDirectSpecular *= _MainDirectSpecularStrength;
    lightingTerm.addDirectDiffuse *= _AddDirectDiffuseStrength;
    lightingTerm.addDirectSpecular *= _AddDirectSpecularStrength;
    lightingTerm.indirectDiffuse *= _IndirectDiffuseStrength;
    lightingTerm.indirectSpecular *= _IndirectSpecularStrength;

    // return half4(lightingTerm.addDirectSpecular,1.0);
    // 植物的简化次表面,包括散射和透射 (hijack emissive)
    #if defined(_SHADER_PLANT)
        half fSubsurfaceRough = 0.7 - surfaceDataEx.perceptualSmoothness * 0.5;
        half fSubsurface = D_GGX(clamp(-dot(available.lights.mainlight.direction, available.camera.viewDir.WS), 0, 1), fSubsurfaceRough);
        float3 directSubsurface = surfaceDataEx.subsurface * available.lights.mainlight.color * fSubsurface * available.lights.mainlight.shadowAttenuation * available.lights.mainlight.distanceAttenuation * available.envOcclusion.mixDirectDiffuseAO;
        float3 indirectSubsurface = surfaceDataEx.subsurface * available.envLighting.bakedGI * _SubsurfaceIndirect * available.envOcclusion.mixIndirectDiffuserAO;
        lightingTerm.emission = lightingTerm.emission + directSubsurface + indirectSubsurface;//次表面存入emission
    #endif

    // 添加AO到LightingTerm
    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_OCCLUSION)
        OcclusionToLightingTerm(available.envOcclusion, lightingTerm, surfaceDataEx.albedo);
    #endif

    LightingData lightingData = GetLightingData(lightingTerm);

    half4 finalColor = CalculateFinalColor(lightingData, surfaceDataEx.alpha);
    // return half4(available.lights.mainlightShadowAttenuation.xxx,1.0);
    //混合雾
    finalColor.rgb = available.envFog.MixFog(finalColor.rgb);
    //注意,仅有透明物体输出alpha
    #ifdef _SURFACE_TYPE_TRANSPARENT
        finalColor.a = surfaceDataEx.alpha;
    #else
        finalColor.a = 1.0;
    #endif

    // return half4(dot(available.model.normal.WS, available.lights.mainlightDirection.WS).xxxx);
    // return half4(surfaceDataEx.emission, 1.0);
    // return half4(lightingTerm.indirectDiffuse * 2, 1.0);
    #if defined(SHADOWCASTER_PASS) || defined(DEPTHONLY_PASS)
        return 0;
    #elif defined(SCENESELECTION_PASS)
        // We use depth prepass for scene selection in the editor, this code allow to output the outline correctly
        return half4(_ObjectId, _PassValue, 1.0, 1.0);
    #elif defined(DEPTHNORMAL_PASS)
        return half4(available.model.normal.WS, 0.0);
    #elif defined(META_PASS)
        UnityMetaInput metaInput = (UnityMetaInput)0;
        metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
        metaInput.Emission = surfaceDataEx.emission;
        #ifdef EDITOR_VISUALIZATION
            metaInput.VizUV = input.VizUV;
            metaInput.LightCoord = input.LightCoord;
        #endif
        return UnityMetaFragment(metaInput);
    #else
        return finalColor;
    #endif
}


#endif