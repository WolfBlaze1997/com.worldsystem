#ifndef BASE_COORD_AND_VECTOR_FUNCTION
#define BASE_COORD_AND_VECTOR_FUNCTION

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

//--------------------------------------------------------
//定义坐标信息的抽象,包括坐标在各种空间的位置:
//OS对象空间,WS世界空间,VS摄像机空间,LS主灯光空间,CS裁剪空间,NDC规格化设备空间(未除w),SS屏幕空间,DS设备空间,SC阴影空间

struct Position
{
    float3 OS;float3 WS;float3 VS;float4 CS;float4 NDC;
    float3 SS;float4 DS;float3 LS;float4 SC;
};
//定义矢量信息的抽象
//OS对象空间,WS世界空间,VS摄像机空间,LS主灯光空间,CS裁剪空间,TS切线空间
struct Direction
{
    half3 OS;half3 WS;half3 VS;half3 uCS;half3 LS;half3 TS;
};
void Position_float(Position position,
out float3 OS, out float3 WS, out float3 VS, out float4 CS, out float4 NDC,
out float3 SS, out float4 DS, out float3 LS, out float4 SC)
{
    OS = position.OS; WS = position.WS; VS = position.VS; CS = position.CS;NDC = position.NDC;SS = position.SS; DS = position.DS; LS = position.LS; SC = position.SC;
}
void Direction_float(Direction direction,
out float3 OS, out float3 WS, out float3 VS, out float3 uCS, out float3 LS, out float3 TS)
{
    OS = direction.OS; WS = direction.WS; VS = direction.VS; uCS = direction.uCS; LS = direction.LS; TS = direction.TS;
}

//---------------------------------------------------------------------------------------------------------------------------
//位置变换
void GetPositionTransformSpaceFromObject_float(float3 positionOS, out Position position)
{
    position = (Position)0;
    position.OS = positionOS;
    position.WS = TransformObjectToWorld(position.OS);
    position.VS = TransformWorldToView(position.WS);
    position.CS = TransformWorldToHClip(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除以w才是正确的NDC坐标但是unity的帮助函数中并未除以w,除w即为positionSS
    position.SS = position.NDC.xyz / position.NDC.w;//
    position.DS = float4(position.SS.xy * _ScreenParams.xy, position.SS.z, 1.0);
    position.LS = mul(_MainLightWorldToLight, float4(position.WS, 1.0)).xyz;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
void GetPositionTransformSpaceFromWorld_float(float3 positionWS, out Position position)
{
    position = (Position)0;
    position.WS = positionWS;
    position.OS = TransformWorldToObject(position.WS);
    position.VS = TransformWorldToView(position.WS);
    position.CS = TransformWorldToHClip(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除w即为positionSS
    position.SS = position.NDC.xyz / position.NDC.w;//
    position.DS = float4(position.SS.xy * _ScreenParams.xy, position.SS.z, 1.0);
    position.LS = mul(_MainLightWorldToLight, float4(position.WS, 1.0)).xyz;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
void GetPositionTransformSpaceFromView_float(float3 positionVS, out Position position)
{
    position = (Position)0;
    position.VS = positionVS;
    position.WS = mul(UNITY_MATRIX_I_V, float4(position.VS, 1.0)).xyz;
    position.OS = TransformWorldToObject(position.WS);
    position.CS = TransformWorldToHClip(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除w即为positionSS
    position.SS = position.NDC.xyz / position.NDC.w;//
    position.DS = float4(position.SS.xy * _ScreenParams.xy, position.SS.z, 1.0);
    position.LS = mul(_MainLightWorldToLight, float4(position.WS, 1.0)).xyz;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
void GetPositionTransformSpaceFromHClip_float(float4 positionCS, out Position position)
{
    position = (Position)0;
    position.CS = positionCS;
    position.WS = ComputeWorldSpacePosition(position.CS, UNITY_MATRIX_I_VP);
    position.OS = TransformWorldToObject(position.WS);
    position.VS = TransformWorldToView(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除w即为positionSS
    position.SS = position.NDC.xyz / position.NDC.w;//
    position.DS = float4(position.SS.xy * _ScreenParams.xy, position.SS.z, 1.0);
    position.LS = mul(_MainLightWorldToLight, float4(position.WS, 1.0)).xyz;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
void GetPositionTransformSpaceFromScreen_float(float3 positionSS, out Position position)
{
    position = (Position)0;
    position.SS = positionSS;
    position.WS = ComputeWorldSpacePosition(position.SS.xy, position.SS.z, UNITY_MATRIX_I_VP);//position.SS.z=deviceDepth
    position.OS = TransformWorldToObject(position.WS);
    position.VS = TransformWorldToView(position.WS);
    position.CS = TransformWorldToHClip(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除w即为positionSS
    position.DS = float4(position.SS.xy * _ScreenParams.xy, position.SS.z, 1.0);
    position.LS = mul(_MainLightWorldToLight, float4(position.WS, 1.0)).xyz;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
void GetPositionTransformSpaceFromDevice_float(float4 positionDS, out Position position)
{
    position = (Position)0;
    position.DS = positionDS;
    position.SS = float3(position.DS.xy / _ScreenParams.xy, position.DS.z);//
    position.WS = ComputeWorldSpacePosition(position.SS.xy, position.SS.z, UNITY_MATRIX_I_VP);
    position.OS = TransformWorldToObject(position.WS);
    position.VS = TransformWorldToView(position.WS);
    position.CS = TransformWorldToHClip(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除w即为positionSS
    position.LS = mul(_MainLightWorldToLight, float4(position.WS, 1.0)).xyz;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
void GetPositionTransformSpaceFromMainLight_float(float3 positionLS, out Position position)
{
    position = (Position)0;
    position.LS = positionLS;//_MainLightWorldToLight在主灯光cookie有贴图时才有效
    position.WS = mul(Inverse(_MainLightWorldToLight), float4(position.LS, 1.0)).xyz;
    position.OS = TransformWorldToObject(position.WS);
    position.VS = TransformWorldToView(position.WS);
    position.CS = TransformWorldToHClip(position.WS);
    float4 ndc = position.CS * 0.5f;
    position.NDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    position.NDC.zw = position.CS.zw;//此NDC未除w,除w即为positionSS
    position.SS = position.NDC.xyz / position.NDC.w;//
    position.DS = float4(position.SS.xy * _ScreenParams.xy, position.SS.z, 1.0);
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        position.SC = ComputeScreenPos(position.CS);
    #else
        position.SC = TransformWorldToShadowCoord(position.WS);
    #endif
}
half3 TransformWorldToTangent_Dir(half3 directionWS, half3x3 TBN, bool doNormalize)
{
    float3 row0 = TBN[0];
    float3 row1 = TBN[1];
    float3 row2 = TBN[2];
    float3 col0 = cross(row1, row2);
    float3 col1 = cross(row2, row0);
    float3 col2 = cross(row0, row1);
    float determinant = dot(row0, col0);
    float sgn = determinant < 0.0 ? (-1.0) : 1.0;
    real3x3 matTBN_I_T = real3x3(col0, col1, col2);
    half3 directionTS = sgn * mul(matTBN_I_T, directionWS);
    if (doNormalize)
        directionTS = normalize(directionTS);
    return directionTS;
}


//---------------------------------------------------------------------------------------------------------------------------
//方向变换
void GetDirectionTransformSpaceFromWorld_float(half3 dirWS, bool doNormalize, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.WS = dirWS;
    if (doNormalize)
        direction.WS = normalize(direction.WS);
    direction.OS = TransformWorldToObjectDir(direction.WS, doNormalize);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.LS = mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz;
    if (doNormalize)
        direction.LS = normalize(direction.LS);
    direction.TS = TransformWorldToTangent_Dir(direction.WS, TBN, doNormalize);
}
void GetDirectionTransformSpaceFromObject_float(half3 dirOS, bool doNormalize, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.OS = dirOS;
    if (doNormalize)
        direction.OS = normalize(direction.OS);
    direction.WS = TransformObjectToWorldDir(direction.OS, doNormalize);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.LS = mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz;
    if (doNormalize)
        direction.LS = normalize(direction.LS);
    direction.TS = TransformWorldToTangent_Dir(direction.WS, TBN, doNormalize);
}
void GetDirectionTransformSpaceFromView_float(half3 dirVS, bool doNormalize, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.VS = dirVS;
    if (doNormalize)
        direction.VS = normalize(direction.VS);
    direction.WS = mul(UNITY_MATRIX_I_V, half4(direction.VS, 0.0)).xyz;
    direction.OS = TransformWorldToObjectDir(direction.WS, doNormalize);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.LS = mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz;
    if (doNormalize)
        direction.LS = normalize(direction.LS);
    direction.TS = TransformWorldToTangent_Dir(direction.WS, TBN, doNormalize);
}
void GetDirectionTransformSpaceFromMainLight_float(half3 dirLS, bool doNormalize, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.LS = dirLS;
    if (doNormalize)
        direction.LS = normalize(direction.LS);
    direction.WS = mul(Inverse(_MainLightWorldToLight), float4(direction.LS, 0.0)).xyz;
    if (doNormalize)
        direction.WS = normalize(direction.WS);
    direction.OS = TransformWorldToObjectDir(direction.WS, doNormalize);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.TS = TransformWorldToTangent_Dir(direction.WS, TBN, doNormalize);
}
void GetDirectionTransformSpaceFromTangent_float(half3 dirTS, bool doNormalize, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.TS = dirTS;
    if (doNormalize)
        direction.TS = normalize(direction.TS);
    direction.WS = TransformTangentToWorld(dirTS, TBN);
    if (doNormalize)
        direction.WS = normalize(direction.WS);
    direction.OS = TransformWorldToObjectDir(direction.WS, doNormalize);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.LS = mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz;
    if (doNormalize)
        direction.LS = normalize(direction.LS);
}


//---------------------------------------------------------------------------------------------------------------------------
//法线变换
void GetDirectionTransformSpaceFromWorldNormal_float(half3 normalWS, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.WS = SafeNormalize(normalWS);
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.VS = TransformWorldToViewNormal(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);//注意,这里由于视锥体在裁剪空间中的变形,向量再在裁剪空间中是非规格化的,规格化将导致信息的丢失;
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
}
void GetDirectionTransformSpaceFromObjectNormal_float(half3 normalOS, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.OS = SafeNormalize(normalOS);
    direction.WS = TransformObjectToWorldNormal(direction.OS);
    direction.VS = TransformWorldToViewNormal(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);//注意,这里由于视锥体在裁剪空间中的变形,向量再在裁剪空间中是非规格化的,规格化将导致信息的丢失;
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
}
void GetDirectionTransformSpaceFromViewNormal_float(half3 normalVS, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.VS = SafeNormalize(normalVS);
    direction.WS = mul(UNITY_MATRIX_I_V, half4(direction.VS, 0.0)).xyz;
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);//注意,这里由于视锥体在裁剪空间中的变形,向量再在裁剪空间中是非规格化的,规格化将导致信息的丢失;
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
}
void GetDirectionTransformSpaceFromMainLightNormal_float(half3 normalLS, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.LS = SafeNormalize(normalLS);
    direction.WS = SafeNormalize(mul(Inverse(_MainLightWorldToLight), half4(direction.LS, 0.0)).xyz);
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.VS = TransformWorldToViewNormal(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
}
void GetDirectionTransformSpaceFromTangentNormal_float(half3 normalTS, out Direction direction, half3x3 TBN = k_identity3x3)
{
    direction = (Direction)0;
    direction.TS = SafeNormalize(normalTS);
    direction.WS = NormalizeNormalPerPixel(TransformTangentToWorld(direction.TS, TBN));
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.VS = TransformWorldToViewNormal(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
}


//---------------------------------------------------------------------------------------------------------------------------
//解码
half4 DecodeNormalRG(float2 normalMapRG, half scale = half(1.0))
{
    #if defined(UNITY_ASTC_NORMALMAP_ENCODING)
        half4 normaltex = half4(1, normalMapRG.y, 0, normalMapRG.x);
    #elif defined(UNITY_NO_DXT5nm)
        //通用rbg,未使用DXT5nm格式时使用
        half4 normaltex = half4(normalMapRG.x, normalMapRG.y, 1.0, 1.0);
    #else
        //DXT5nm (1, y, 0, x) or BC5 (x, y, 0, 1)
        half4 normaltex = half4(normalMapRG.x, normalMapRG.y, 0.0, 1.0);
    #endif
    return normaltex;
    // return UnpackNormalScale_real(normaltex, scale);

}

//---------------------------------------------------------------------------------------------------------------------------
float sum(float3 v)
{
    return v.x + v.y + v.z;
}
float4 hash4(float2 p)
{
    return frac(sin(float4(1.0 + dot(p, float2(37.0, 17.0)),
    2.0 + dot(p, float2(11.0, 47.0)),
    3.0 + dot(p, float2(41.0, 29.0)),
    4.0 + dot(p, float2(23.0, 31.0)))) * 103.0);
}


//---------------------------------------------------------------------------------------------------------------------------
//Unity standard PBR
void LightingPhysicallyBased(half3 normalWS, half3 viewDirWS, Light light, BRDFData brdfData, BRDFData brdfDataClearCoat,
half clearCoatMask, inout half3 outDirectDiffuse, inout half3 outDirectSpecular)
{
    half NdotL = saturate(dot(normalWS, light.direction));
    half3 radiance = NdotL * light.color * light.distanceAttenuation * light.shadowAttenuation;

    half diffuseTerm = dot(normalWS, light.direction) * 0.5 + 0.5;
    half specularTerm = DirectBRDFSpecular(brdfData, normalWS, light.direction, viewDirWS);
    half3 directDiffuse = brdfData.diffuse * diffuseTerm;
    half3 directSpecular = brdfData.specular * specularTerm;

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        half CoatSpecular = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, light.direction, viewDirWS);
        half NoV = saturate(dot(normalWS, viewDirWS));
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);
        half mask = 1.0 - clearCoatMask * coatFresnel;
        directDiffuse = directDiffuse * mask;
        directSpecular = directSpecular * mask + CoatSpecular * clearCoatMask;
    #endif

    directDiffuse *= radiance;
    directSpecular *= radiance;
    
    outDirectDiffuse += directDiffuse;
    outDirectSpecular += directSpecular;
}
void LightingPhysicallyBased_AdditionaLighting(float3 positionWS, float2 positionSS, half3 normalWS, half3 viewDirWS, half4 shadowMask, BRDFData brdfData, BRDFData brdfDataClearCoat, half clearCoatMask, out half3 outAddDirectDiffuse, out half3 outAddDirectSpecular)
{
    outAddDirectDiffuse = 0;
    outAddDirectSpecular = 0;
    //附加灯光光照
    #if defined(_ADDITIONAL_LIGHTS)

        half3 addDirectdiffuse = 0;
        half3 addDirectSpecular = 0;
        //这是为了兼容LIGHT_LOOP_BEGIN宏
        InputData inputData = (InputData)0;
        inputData.normalizedScreenSpaceUV = positionSS;
        inputData.positionWS = positionWS;

        uint lightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(lightCount)
        Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
        #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, GetMeshRenderingLayer()))
        #endif
        {
            LightingPhysicallyBased(normalWS, viewDirWS, light, brdfData, brdfDataClearCoat,
            clearCoatMask, addDirectdiffuse, addDirectSpecular);
        }
        LIGHT_LOOP_END

        outAddDirectDiffuse += addDirectdiffuse;
        outAddDirectSpecular += addDirectSpecular;
    #endif
}
//---------------------------------------------------------------------------------------------------------------------------

//huodini的VAT(顶点动画贴图)流程
void HuodiniVAT_Soft(
    //input
    float2 VAT_UV,
    sampler2D positionTex,
    sampler2D rotateTex, bool isPosTexHDR,
    float inputTime, bool isAutoPlay, float displayFrame, float playSpeed, float strength,
    //houdini VAT data
    float houdiniFPS, float frameCount, float BoundMax_X, float BoundMax_Y, float BoundMax_Z, float BoundMin_X, float BoundMin_Y, float BoundMin_Z,
    //output
    inout float3 positionOS, out float3 normalOS, out float3 tangentOS
)
{
    float3 BoundMax = float3(BoundMax_X, BoundMax_Y, BoundMax_Z);
    float3 BoundMin = float3(BoundMin_X, BoundMin_Y, BoundMin_Z);

    float ActivePixelsRatioY = 1.0 - (-BoundMax_X * 10 - floor(-BoundMax_X * 10));
    float ActivePixelsRatioX = 1.0 - (ceil(BoundMin_Z * 10) - BoundMin_Z * 10);

    float a = isAutoPlay ? floor(frac(houdiniFPS / (frameCount - 0.01) * inputTime * playSpeed) * frameCount + displayFrame) + 1.0 : floor(displayFrame);

    float frameUVy = 1.0 - fmod(a - 1.0, frameCount) / frameCount * ActivePixelsRatioY - (1.0 - VAT_UV.y) * ActivePixelsRatioY;
    float frameUVx = VAT_UV.x * ActivePixelsRatioX;
    float2 frameUV = float2(frameUVx, frameUVy);

    float4 positionColor = tex2Dlod(positionTex,float4(frameUV,0,0));
    //  SAMPLE_TEXTURE2D_LOD(positionTex, sampler_positionTex, frameUV, 0);
    if (isPosTexHDR)
    {
        positionOS += positionColor.xyz * strength;
    }
    else
    {
        positionOS += ((BoundMax - BoundMin) * positionColor.rgb + BoundMin) * strength;
    }

    half4 rotateColor = tex2Dlod(rotateTex,float4(frameUV,0,0));
    // SAMPLE_TEXTURE2D_LOD(rotateTex, sampler_rotateTex, frameUV, 0);
    rotateColor = (rotateColor - 0.5) * 2;

    half3 b = rotateColor.a * half3(0.0, 1.0, 0.0) + cross(rotateColor.rgb, half3(0.0, 1.0, 0.0));
    normalOS = normalize(cross(rotateColor.rgb, b) * 2 + half3(0.0, 1.0, 0.0));

    half3 c = rotateColor.a * half3(-1.0, 0.0, 0.0) + cross(rotateColor.rgb, half3(-1.0, 0.0, 0.0));
    tangentOS = normalize(cross(rotateColor.rgb, c) * 2 + half3(-1.0, 0.0, 0.0));
}




#endif