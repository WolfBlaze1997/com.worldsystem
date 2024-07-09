#ifndef UNIVERSAL_LIT_META_PASS_INCLUDED
#define UNIVERSAL_LIT_META_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

#include "../FunctionLibrary/UnityTextureLibrary.hlsl"


//这里是为了避免使用了Varyings作为输入参数的函数报错
#define Varyings debugVaryings
#include "../FunctionLibrary/BaseFunctionLibrary.hlsl"
#undef Varyings

half4 UniversalFragmentMetaLit(Varyings input) : SV_Target
{

    //转化贴图
    UnityTexture2D baseMap = UnityBuildTexture2DStruct(_BaseMap);
    UnityTexture2D bumpMap = UnityBuildTexture2DStruct(_BumpMap);
    UnityTexture2D MRAMap = UnityBuildTexture2DStruct(_MRAMap);
    UnityTexture2D EmissionMap = UnityBuildTexture2DStruct(_EmissionMap);
    UnityTexture2D HeightMap = UnityBuildTexture2DStruct(_HeightMap);
    UnityTexture2D ClearCoatMap = UnityBuildTexture2DStruct(_ClearCoatMap);
    UnityTexture2D DetailAlbedoMap = UnityBuildTexture2DStruct(_DetailAlbedoMap);
    UnityTexture2D DetailNormalMap = UnityBuildTexture2DStruct(_DetailNormalMap);
    UnityTexture2D DetailAlbedoMap2 = UnityBuildTexture2DStruct(_DetailAlbedoMap2);
    UnityTexture2D DetailNormalMap2 = UnityBuildTexture2DStruct(_DetailNormalMap2);
    UnityTexture2D SubsurfaceMap = UnityBuildTexture2DStruct(_SubsurfaceMap);

    //UV,视差
    float2 baseUV = input.uv.xy;

    //采样贴图
    half4 baseTex = baseMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate);
    half3 MRATex = MRAMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate).xyz;
    half3 normalTS = bumpMap.SampleNormalSupportNoTileing(baseUV, _ScaleOrRotate, _BumpScale);
    half3 EmissionTex = EmissionMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate).rgb;
    half2 ClearCoatTex = ClearCoatMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate).rg;
    half HeightTex = HeightMap.SampleSupportNoTileing(baseUV, _ScaleOrRotate).r;
    //从贴图构建表面数据,SurfaceDataExtend包含了能从贴图中获得的所有数据,需要其他贴图扩充SurfaceDataExtend结构即可

    SurfaceDataExtend surfaceDataEx = GetSurfaceDataExtend(
        baseTex.rgb, //half3 albedo 反照率
        MRATex.g, //half perceptualRoughness
        MRATex.r, //half metallic
        normalTS, //half3 normalTS
        EmissionTex, //half3 emission
        MRATex.b, //half occlusion
        baseTex.a, //half alpha
        HeightTex, //half height
        ClearCoatTex.r, //half clearCoatMask
        ClearCoatTex.g, //half clearCoatPerceptualRoughness
        _Smoothness, //控制光滑度,perceptualSmoothness = saturate((1.0-perceptualRoughness)*_Smoothness);
    _BaseColor, //控制diffuse,diffuse = diffuse * _BaseColor;
    _SpecColor, //控制specular,specular = specular * _SpecColor;
    _Metallic, //控制金属度,metallic = metallic * _Metallic;
    _EmissionColor, //控制自发光颜色,emission = emissionTex * _EmissionColor;
    _ClearCoatMask, //控制清漆遮罩,ClearCoatMask = ClearCoatMask * _ClearCoatMask
    _ClearCoatSmoothness, //控制清漆光滑度,clearCoatSmoothness = saturate((1.0-clearCoatPerceptualRoughness)*_ClearCoatSmoothness);
    _OcclusionStrength//控制AO贴图强度,occlusion = saturate(lerp(1.0, occlusion, _OcclusionStrength));
    );
    // surfaceDataEx.SetupSubsurface(SubsurfaceTex.rgb * SubsurfaceTex.a * _SubsurfaceColor.rgb);
    // return half4(SubsurfaceTex.rgb, 1.0);
    // return baseTex.aaaa;

    //alphaTest
    #if defined(_ALPHATEST_ON)
        clip(surfaceDataEx.alpha - _Cutoff);
    #endif

    //获得BRDFData
    BRDFData brdfData = surfaceDataEx.GetBRDFData();

    // SurfaceData surfaceData;
    // InitializeStandardLitSurfaceData(input.uv, surfaceData);

    // BRDFData brdfData;
    // InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    MetaInput metaInput;
    metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
    metaInput.Emission = surfaceDataEx.emission;
    return UniversalFragmentMeta(input, metaInput);
}
#endif
