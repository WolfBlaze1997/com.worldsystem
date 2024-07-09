#ifndef BASE_COORD_AND_VECTOR_FUNCTION
#define BASE_COORD_AND_VECTOR_FUNCTION
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "../FunctionLibrary/UtilityFunctionLibrary.hlsl"
#include "../Main/Input.hlsl"
#include "../FunctionLibrary/UnityTextureLibrary.hlsl"

//其他着色器引用此文件时,可能由于Varyings的不一致,导致Varyings作为输入的函数报错
//此时只需要 #define Varyings debugVaryings,之后#undef Varyings即可
struct debugVaryings
{
    float4 texcoord : TEXCOORD0;//xy:uv0,zw:uv3

    #if !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS) && !defined(SCENESELECTION_PASS)//shadowcasterpass和depthOnlyPass不需要这些,使用宏控制以避免插值器的浪费
        #if !defined(DEPTHNORMAL_PASS)//depthNormalPass不需要,使用宏控制以避免插值器的浪费
            float3 positionWS : TEXCOORD1;
        #endif
        //viewDirWS,存在normalWS,tangentWS,bitangentWS的w通道中
        half4 normalWS : TEXCOORD2;
        half4 tangentWS : TEXCOORD3;
        half4 bitangentWS : TEXCOORD4;
    #endif

    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2) || defined(_ADDITIONAL_LIGHTS_VERTEX)
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
        #else
            half fogFactor : TEXCOORD5;
        #endif
    #endif

    #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
        float4 shadowCoord : TEXCOORD6;
    #endif
    #if defined(REQUIRE_SH) || defined(LIGHTMAP_ON)
        DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
    #endif
    #ifdef DYNAMICLIGHTMAP_ON
        float2 dynamicLightmapUV : TEXCOORD8; // Dynamic lightmap UVs
    #endif
    #ifdef REQUIRE_VERTEXCOLOR
        float4 vertexColor : TEXCOORD9;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirTS : TEXCOORD10;
    #endif

    #if defined(EFFECT_BILLBOARD) || defined(EFFECT_HUE_VARIATION)
        half4 interpolator : TEXCOORD11;//注意优化,建议完成后将其拆分放入positionWS,normalWS的w通道
    #endif

    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


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
    bool isNormalized;//检查是否规格化

};
Position GetPositionTransformSpaceFromObject(float3 positionOS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
Position GetPositionTransformSpaceFromWorld(float3 positionWS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
Position GetPositionTransformSpaceFromView(float3 positionVS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
Position GetPositionTransformSpaceFromHClip(float4 positionCS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
Position GetPositionTransformSpaceFromScreen(float3 positionSS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
Position GetPositionTransformSpaceFromDevice(float4 positionDS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
Position GetPositionTransformSpaceFromMainLight(float3 positionLS)
{
    Position position = (Position)0;
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
    position.SC = TransformWorldToShadowCoord(position.WS);
    return position;
}
//判断向量再各个空间中是否成功规格化
void GetIsNormalized(bool doNormalize, inout Direction direction)
{
    direction.isNormalized = IsNormalized(direction.OS) &&
    IsNormalized(direction.WS) &&
    IsNormalized(direction.VS) &&
    IsNormalized(direction.LS) &&
    IsNormalized(direction.TS);
    if (doNormalize = false)
        direction.isNormalized = !(!IsNormalized(direction.OS) &&
    !IsNormalized(direction.WS) &&
    !IsNormalized(direction.VS) &&
    !IsNormalized(direction.LS) &&
    !IsNormalized(direction.TS));
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
Direction GetDirectionTransformSpaceFromWorld(half3 dirWS, bool doNormalize, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
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
    GetIsNormalized(doNormalize, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromObject(half3 dirOS, bool doNormalize, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
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
    GetIsNormalized(doNormalize, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromView(half3 dirVS, bool doNormalize, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
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
    GetIsNormalized(doNormalize, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromMainLight(half3 dirLS, bool doNormalize, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
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
    GetIsNormalized(doNormalize, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromTangent(half3 dirTS, bool doNormalize, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
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
    GetIsNormalized(doNormalize, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromWorldNormal(half3 normalWS, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
    direction.WS = SafeNormalize(normalWS);
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);//注意,这里由于视锥体在裁剪空间中的变形,向量再在裁剪空间中是非规格化的,规格化将导致信息的丢失;
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
    GetIsNormalized(true, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromObjectNormal(half3 normalOS, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
    direction.OS = SafeNormalize(normalOS);
    direction.WS = TransformObjectToWorldNormal(direction.OS);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);//注意,这里由于视锥体在裁剪空间中的变形,向量再在裁剪空间中是非规格化的,规格化将导致信息的丢失;
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
    GetIsNormalized(true, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromViewNormal(half3 normalVS, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
    direction.VS = SafeNormalize(normalVS);
    direction.WS = mul(UNITY_MATRIX_I_V, half4(direction.VS, 0.0)).xyz;
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);//注意,这里由于视锥体在裁剪空间中的变形,向量再在裁剪空间中是非规格化的,规格化将导致信息的丢失;
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
    GetIsNormalized(true, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromMainLightNormal(half3 normalLS, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
    direction.LS = SafeNormalize(normalLS);
    direction.WS = SafeNormalize(mul(Inverse(_MainLightWorldToLight), half4(direction.LS, 0.0)).xyz);
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.TS = TransformWorldToTangent(direction.WS, TBN);
    GetIsNormalized(true, direction);
    return direction;
}
Direction GetDirectionTransformSpaceFromTangentNormal(half3 normalTS, half3x3 TBN = k_identity3x3)
{
    Direction direction = (Direction)0;
    direction.TS = SafeNormalize(normalTS);
    direction.WS = NormalizeNormalPerPixel(TransformTangentToWorld(direction.TS, TBN));
    direction.OS = TransformWorldToObjectNormal(direction.WS);
    direction.VS = TransformWorldToViewDir(direction.WS);
    direction.uCS = TransformWorldToHClipDir(direction.WS);
    direction.LS = SafeNormalize(mul(_MainLightWorldToLight, half4(direction.WS, 0.0)).xyz);
    GetIsNormalized(true, direction);
    return direction;
}

//--------------------------------------------------------
//抽象化摄像机
struct Depth
{
    float deviceDepth;//deviceDepth,SV_POSITION语义传递到片元着色器的的Z值,URP中depth即默认为设备深度,depth = positionCS.z/positionCS.w = positionNDC.z / positionNDC.w
    float linearEyeDepth;//视空间的Z值,range[near,far]
    float linear0FarDepth;//映射到[0,far]的范围,常用于fogfactor的计算
    float linear01Depth;//0在相机位置，1在远平面位置
    float linear01DepthFromNear;//0在近平面，1在远平面

};
// struct CameraGlobalVar
// {
//     //以下是与摄像机or深度相关的unity提供的全局参数
//     //LOD淡入淡出
//     // float4 unity_LODFade;
//     //视锥体平面
//     float4 _FrustumPlanes[6];
//     float4 unity_CameraWorldClipPlanes[6];
//     float4 _ScaledScreenParams;
//     float4 _ScreenParams;
//     float4 _ScreenSize;
//     float4 _ProjectionParams;
//     float4 _ZBufferParams;
//     float4 unity_OrthoParams;
//     float3 _WorldSpaceCameraPos;
// };
struct Camera
{
    Position position;
    Direction forwardDir;
    Direction viewDir;
    Direction uViewDir;
    //摄像机分辨率(屏幕分辨率)
    uint2 pixelSize;
    //near,far平面值
    half nearValue;half farValue;half nearValueCS;half farValueCS;
    //屏幕纹理
    half depthTex;/*设备深度,depth*/half3 normalsTex;half3 opaqueTex;
    //深度
    Depth depth;
    float lODFade;
    // CameraGlobalVar globalVar;//unity提供的一些与摄像机有关的全局变量
    bool isPerspective;//摄像机是否是透视投影

};
Depth GetDepth(float4 vertexShaderInputPositionCS)
{
    Depth depth = (Depth)0;
    depth.deviceDepth = vertexShaderInputPositionCS.z;
    depth.linearEyeDepth = LinearEyeDepth(depth.deviceDepth, _ZBufferParams);
    depth.linear0FarDepth = max(depth.linearEyeDepth - _ProjectionParams.y, 0);
    depth.linear01Depth = Linear01Depth(depth.deviceDepth, _ZBufferParams);
    depth.linear01DepthFromNear = Linear01DepthFromNear(depth.deviceDepth, _ZBufferParams);
    return depth;
}
Camera GetMainCamera(half3 viewDirWS, float4 vertexShaderInputPositionCS, float3 positionWS, float2 positionSS, float3x3 TBN)
{
    //仅能获得当前摄像机
    Camera camera = (Camera)0;
    camera.position = GetPositionTransformSpaceFromWorld(GetCurrentViewPosition());
    camera.forwardDir = GetDirectionTransformSpaceFromWorld(GetViewForwardDir(), true, TBN);
    camera.viewDir = GetDirectionTransformSpaceFromWorld(viewDirWS, true, TBN);
    camera.uViewDir = GetDirectionTransformSpaceFromWorld(GetWorldSpaceViewDir(positionWS), false, TBN);
    camera.pixelSize = asuint(_ScreenParams.xy);
    camera.nearValue = _ProjectionParams.y;
    camera.farValue = _ProjectionParams.z;
    camera.nearValueCS = UNITY_NEAR_CLIP_VALUE;
    camera.farValueCS = UNITY_RAW_FAR_CLIP_VALUE;
    camera.depthTex = SampleSceneDepth(positionSS);
    camera.normalsTex = SampleSceneNormals(positionSS);
    camera.opaqueTex = SampleSceneColor(positionSS);
    camera.depth = GetDepth(vertexShaderInputPositionCS);
    camera.lODFade = unity_LODFade.x;
    camera.isPerspective = IsPerspectiveProjection();
    // camera.globalVar._FrustumPlanes = _FrustumPlanes;
    // camera.globalVar._ProjectionParams = _ProjectionParams;
    // camera.globalVar._ScaledScreenParams = _ScaledScreenParams;
    // camera.globalVar._ScreenParams = _ScreenParams;
    // camera.globalVar._WorldSpaceCameraPos = _WorldSpaceCameraPos;
    // camera.globalVar._ZBufferParams = _ZBufferParams;
    // camera.globalVar.unity_CameraWorldClipPlanes = unity_CameraWorldClipPlanes;
    // camera.globalVar.unity_LODFade = unity_LODFade;
    // camera.globalVar.unity_OrthoParams = unity_OrthoParams;
    return camera;
}

//--------------------------------------------------------
//抽象化模型与光栅化
struct Model
{
    Direction vertexNormal;
    Direction normal;
    Direction tangent;
    Direction bitangent;
    half3x3 TBN;
    Position position;
    Position pivot;
    Direction xAxisDir;
    Direction yAxisDir;
    Direction zAxisDir;
    float2 uv;
    float2 staticLightmapUV;
    float2 dynamicLightmapUV;
    float2 uv3;
    //这个主要用于三平面映射,三平面映射法线贴图时,需要定义SURFACE_GRADIENT以获得正确结果
    //三平面映射使用宏 SAMPLE_UVMAPPING_TEXTURE2D, 法线帖图映射定义SURFACE_GRADIENT,并使用SAMPLE_UVMAPPING_NORMALMAP,采样完成后取消定义
    UVMapping uvMapping;
    half4 vertexColor;
    bool isFront;//判断是否为正面
    uint renderLayer;
    void UpdateModel(float2 updateUV, half3 updateNormalTS)
    {
        uv = updateUV;
        normal = GetDirectionTransformSpaceFromTangentNormal(updateNormalTS, TBN);
        //三平面映射UV
        uvMapping.triplanarWeights = ComputeTriplanarWeights(normal.WS);
        uvMapping.normalWS = normal.WS;
        uvMapping.uv = uv;
    }
    void UpdateModelUV(float2 updateUV)
    {
        uv = updateUV;
        //三平面映射UV
        uvMapping.triplanarWeights = ComputeTriplanarWeights(normal.WS);
        uvMapping.uv = uv;
    }
    void UpdateModelNormal(half3 updateNormalTS)
    {
        normal = GetDirectionTransformSpaceFromTangentNormal(updateNormalTS, TBN);
        //三平面映射UV
        uvMapping.triplanarWeights = ComputeTriplanarWeights(normal.WS);
        uvMapping.normalWS = normal.WS;
    }
};
// #include "../MainPass/MainPass.hlsl"

Model GetModel(Varyings vertexShaderInput, bool vFace, half3 normalTS)
{
    Model model = (Model)1;

    #if !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS) && !defined(SCENESELECTION_PASS)
        //法线
        model.vertexNormal = GetDirectionTransformSpaceFromWorldNormal(vertexShaderInput.normalWS.xyz);
        model.vertexNormal.TS = half3(0.0, 0.0, 1.0);
        model.tangent = GetDirectionTransformSpaceFromWorldNormal(vertexShaderInput.tangentWS.xyz);
        model.tangent.TS = half3(1.0, 0.0, 0.0);
        model.bitangent = GetDirectionTransformSpaceFromWorldNormal(vertexShaderInput.bitangentWS.xyz);
        model.bitangent.TS = half3(0.0, 1.0, 0.0);
        model.TBN = half3x3(model.tangent.WS.xyz, model.bitangent.WS.xyz, model.vertexNormal.WS.xyz);
        model.normal = GetDirectionTransformSpaceFromTangentNormal(normalTS, model.TBN);
        //位置
        #if !defined(DEPTHNORMAL_PASS)
            model.position = GetPositionTransformSpaceFromWorld(vertexShaderInput.positionWS);
        #endif
    #endif

    model.position.DS = vertexShaderInput.positionCS;
    model.position.SS = float3(GetNormalizedScreenSpaceUV(vertexShaderInput.positionCS), vertexShaderInput.positionCS.z);//注意positionCS经过SV_position语义输入是设备坐标positionDS
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        model.position.SC = vertexShaderInput.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        model.position.SC = TransformWorldToShadowCoord(model.position.WS);
    #else
        model.position.SC = float4(0, 0, 0, 0);
    #endif
    //轴点or轴方向
    model.pivot = GetPositionTransformSpaceFromObject(float3(0.0, 0.0, 0.0));
    model.xAxisDir = GetDirectionTransformSpaceFromObject(half3(1.0, 0.0, 0.0), true, model.TBN);
    model.yAxisDir = GetDirectionTransformSpaceFromObject(half3(0.0, 1.0, 0.0), true, model.TBN);
    model.zAxisDir = GetDirectionTransformSpaceFromObject(half3(0.0, 0.0, 1.0), true, model.TBN);

    //纹理坐标
    model.uv = vertexShaderInput.texcoord.xy;
    #ifdef REQUIRE_UV3
        model.uv3 = vertexShaderInput.texcoord.zw;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
        model.dynamicLightmapUV = vertexShaderInput.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
        model.staticLightmapUV = vertexShaderInput.staticLightmapUV;
    #endif
    //顶点颜色
    #ifdef REQUIRE_VERTEXCOLOR
        model.vertexColor = vertexShaderInput.vertexColor;
    #endif
    model.isFront = vFace;
    //三平面映射UV
    model.uvMapping.mappingType = UV_MAPPING_TRIPLANAR;//这里映射模式修改为三平面映射
    model.uvMapping.triplanarWeights = ComputeTriplanarWeights(model.normal.WS);
    GetTriplanarCoordinate(model.position.WS, model.uvMapping.uvXZ, model.uvMapping.uvXY, model.uvMapping.uvZY);
    model.uvMapping.uv = model.uv;
    model.uvMapping.normalWS = model.normal.WS;
    #ifdef SURFACE_GRADIENT
        model.uvMapping.tangentWS = model.tangent.WS;
        model.uvMapping.bitangentWS = model.bitangent.WS;
    #endif

    model.renderLayer = GetMeshRenderingLayer();

    // model.renderLayer = GetMeshRenderingLightLayer();

    return model;
}
Model GetModel(Varyings vertexShaderInput, bool vFace)
{
    Model model = GetModel(vertexShaderInput, vFace, half3(0.0, 0.0, 1.0));
    model.normal = model.vertexNormal;
    //三平面映射UV
    model.uvMapping.mappingType = UV_MAPPING_TRIPLANAR;//这里映射模式修改为三平面映射
    model.uvMapping.triplanarWeights = ComputeTriplanarWeights(model.normal.WS);
    GetTriplanarCoordinate(model.position.WS, model.uvMapping.uvXZ, model.uvMapping.uvXY, model.uvMapping.uvZY);
    model.uvMapping.uv = model.uv;
    model.uvMapping.normalWS = model.normal.WS;
    #ifdef SURFACE_GRADIENT//
        model.uvMapping.tangentWS = model.tangent.WS;
        model.uvMapping.bitangentWS = model.bitangent.WS;
    #endif
    return model;
}
//--------------------------------------------------------
//灯光扩展
// struct LightGlobalVar
// {
//     float4 _MainLightPosition;//主灯光位置
//     half4 _MainLightColor;//主灯光颜色
//     half4 _MainLightOcclusionProbes;//主灯光遮蔽探针, (1.0, 0.0, 0.0, 0.0)用于选择阴影贴图位于哪个通道(默认是R通道)
//     uint _MainLightLayerMask;//主灯光层
//     half4 _AdditionalLightsCount;//附加灯光计数
//     #if USE_CLUSTERED_LIGHTING
//         // Directional lights would be in all clusters, so they don't go into the cluster structure.
//         // Instead, they are stored first in the light buffer.
//         uint _AdditionalLightsDirectionalCount;
//         // The number of Z-bins to skip based on near plane distance.
//         uint _AdditionalLightsZBinOffset;
//         // Scale from view-space Z to Z-bin.
//         float _AdditionalLightsZBinScale;
//         // Scale from screen-space UV [0, 1] to tile coordinates [0, tile resolution].
//         float2 _AdditionalLightsTileScale;
//         uint _AdditionalLightsTileCountX;
//     #endif
//     #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
//         StructuredBuffer<LightData> _AdditionalLightsBuffer;//附加灯光LightData结构缓冲区
//         StructuredBuffer<int> _AdditionalLightsIndices;//附加灯光索引结构缓冲区
//     #else
//         float4 _AdditionalLightsPosition[MAX_VISIBLE_LIGHTS];//附加灯光位置
//         half4 _AdditionalLightsColor[MAX_VISIBLE_LIGHTS];//附加灯光颜色
//         half4 _AdditionalLightsAttenuation[MAX_VISIBLE_LIGHTS];//附加灯光衰减
//         half4 _AdditionalLightsSpotDir[MAX_VISIBLE_LIGHTS];//附加灯光聚光灯方向
//         half4 _AdditionalLightsOcclusionProbes[MAX_VISIBLE_LIGHTS];//附加灯光遮蔽探针,(0.0, 0.0, 0.0, 0.0)似乎没有有效值
//         float _AdditionalLightsLayerMasks[MAX_VISIBLE_LIGHTS];//附加灯光层
//     #endif
//     #if USE_CLUSTERED_LIGHTING
//         float4 _AdditionalLightsZBins[MAX_ZBIN_VEC4S];
//         float4 _AdditionalLightsTiles[MAX_TILE_VEC4S];
//     #endif
//     uint maxVisibleLights;//最大可见灯光数
//     half4 unity_LightData;//x每对象灯光索引偏移, y与附加灯光计数有关, zw未知
//     half4 unity_LightIndices[2];//储存了8个光源的索引

// };
// struct ShadowGlobalVar
// {
//     SCREENSPACE_TEXTURE(_ScreenSpaceShadowmapTexture);	//声明屏幕空间阴影贴图
//     SAMPLER(sampler_ScreenSpaceShadowmapTexture);	//声明屏幕空间阴影贴图采样器
//     TEXTURE2D_SHADOW(_MainLightShadowmapTexture);	//声明主灯光阴影贴图
//     SAMPLER_CMP(sampler_MainLightShadowmapTexture);	//声明主灯光阴影贴图采样器
//     TEXTURE2D_SHADOW(_AdditionalLightsShadowmapTexture);	//声明附加灯光阴影贴图
//     SAMPLER_CMP(sampler_AdditionalLightsShadowmapTexture);	//声明附加灯光阴影贴图采样器
//     real4 unity_ShadowColor;	//阴影颜色(未使用,Unity中也找不到设置)
//     half4 _SubtractiveShadowColor;//阴影颜色
//     TEXTURE2D(unity_ShadowMask);	//声明ShadowMask贴图
//     SAMPLER(samplerunity_ShadowMask);	//声明ShadowMask贴图采样器
//     TEXTURE2D_ARRAY(unity_ShadowMasks);	//声明ShadowMask贴图数组
//     SAMPLER(samplerunity_ShadowMasks);	//声明ShadowMask贴图数组采样器
// };
struct Lights
{
    half4 shadowMask;
    Light mainlight;
    half3 mainlightColor;
    Direction mainlightDirection;
    float mainlightDistanceAttenuation;
    half mainlightShadowAttenuation;
    uint mainlightLayerMask;
    uint addlightCount;
    // LightGlobalVar lightGlobalVar;
    // ShadowGlobalVar shadowGlobalVar;
    Light GetAdditionalLight_Ref(uint i, float3 positionWS)
    {
        return GetAdditionalLight(i, positionWS, shadowMask);
        // #if USE_FORWARD_PLUS
        //     int lightIndex = i;
        // #else
        //     int lightIndex = GetPerObjectLightIndex(i);
        // #endif
        // Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);

        // #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
        //     half4 occlusionProbeChannels = _AdditionalLightsBuffer[lightIndex].occlusionProbeChannels;
        // #else
        //     half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[lightIndex];
        // #endif
        // light.shadowAttenuation = AdditionalLightShadow(lightIndex, positionWS, light.direction, shadowMask, occlusionProbeChannels);
        // #if defined(_LIGHT_COOKIES)
        //     real3 cookieColor = SampleAdditionalLightCookie(lightIndex, positionWS);
        //     light.color *= cookieColor;
        // #endif

        // return light;
    }
};
// LightGlobalVar GetLightGlobalVar()
// {
//     LightGlobalVar lightGlobalVar;
//     lightGlobalVar._MainLightPosition = _MainLightPosition;
//     lightGlobalVar._MainLightColor = _MainLightColor;
//     lightGlobalVar._MainLightOcclusionProbes = _MainLightOcclusionProbes;
//     lightGlobalVar._MainLightLayerMask = _MainLightLayerMask;
//     lightGlobalVar._AdditionalLightsCount = _AdditionalLightsCount;
//     #if USE_CLUSTERED_LIGHTING
//         lightGlobalVar._AdditionalLightsDirectionalCount = _AdditionalLightsDirectionalCount;
//         lightGlobalVar._AdditionalLightsZBinOffset = _AdditionalLightsZBinOffset;
//         lightGlobalVar._AdditionalLightsZBinScale = _AdditionalLightsZBinScale;
//         lightGlobalVar._AdditionalLightsTileScale = _AdditionalLightsTileScale;
//         lightGlobalVar._AdditionalLightsTileCountX = _AdditionalLightsTileCountX;
//     #endif
//     #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
//         lightGlobalVar._AdditionalLightsBuffer = _AdditionalLightsBuffer;
//         lightGlobalVar._AdditionalLightsIndices = _AdditionalLightsIndices;
//     #else
//         lightGlobalVar._AdditionalLightsPosition = _AdditionalLightsPosition;
//         lightGlobalVar._AdditionalLightsColor = _AdditionalLightsColor;
//         lightGlobalVar._AdditionalLightsAttenuation = _AdditionalLightsAttenuation;
//         lightGlobalVar._AdditionalLightsSpotDir = _AdditionalLightsSpotDir;
//         lightGlobalVar._AdditionalLightsOcclusionProbes = _AdditionalLightsOcclusionProbes;
//         lightGlobalVar._AdditionalLightsLayerMasks = _AdditionalLightsLayerMasks;
//     #endif
//     #if USE_CLUSTERED_LIGHTING
//         lightGlobalVar._AdditionalLightsZBins = _AdditionalLightsZBins;
//         lightGlobalVar._AdditionalLightsTiles = _AdditionalLightsTiles;
//     #endif
//     lightGlobalVar.maxVisibleLights = MAX_VISIBLE_LIGHTS;
//     lightGlobalVar.unity_LightData = unity_LightData;
//     lightGlobalVar.unity_LightIndices = unity_LightIndices;
//     return lightGlobalVar;
// }
// ShadowGlobalVar GetShadowGlobalVar()
// {
//     ShadowGlobalVar shadowGlobalVar;
//     shadowGlobalVar._ScreenSpaceShadowmapTexture = _ScreenSpaceShadowmapTexture;
//     shadowGlobalVar.sampler_ScreenSpaceShadowmapTexture = sampler_ScreenSpaceShadowmapTexture;
//     shadowGlobalVar._MainLightShadowmapTexture = _MainLightShadowmapTexture;
//     shadowGlobalVar.sampler_MainLightShadowmapTexture = sampler_MainLightShadowmapTexture;
//     shadowGlobalVar._AdditionalLightsShadowmapTexture = _AdditionalLightsShadowmapTexture;
//     shadowGlobalVar.sampler_AdditionalLightsShadowmapTexture = sampler_AdditionalLightsShadowmapTexture;
//     shadowGlobalVar.unity_ShadowColor = unity_ShadowColor;
//     shadowGlobalVar._SubtractiveShadowColor = _SubtractiveShadowColor;
//     shadowGlobalVar.unity_ShadowMask = unity_ShadowMask;
//     shadowGlobalVar.samplerunity_ShadowMask = samplerunity_ShadowMask;
//     shadowGlobalVar.unity_ShadowMasks = unity_ShadowMasks;
//     shadowGlobalVar.samplerunity_ShadowMasks = samplerunity_ShadowMasks;
//     return shadowGlobalVar;
// }
Lights GetLights(half3x3 TBN)
{
    Lights lights;
    lights.shadowMask = 0;
    lights.mainlight = GetMainLight();
    lights.mainlightColor = lights.mainlight.color;
    lights.mainlightDirection = GetDirectionTransformSpaceFromWorld(lights.mainlight.direction, true, TBN);
    lights.mainlightDirection.LS = half3(0.0, 0.0, 1.0);
    lights.mainlightDistanceAttenuation = lights.mainlight.distanceAttenuation;
    lights.mainlightShadowAttenuation = lights.mainlight.shadowAttenuation;
    lights.mainlightLayerMask = lights.mainlight.layerMask;
    lights.addlightCount = GetAdditionalLightsCount();
    // lights.lightGlobalVar = GetLightGlobalVar();
    // lights.shadowGlobalVar = GetShadowGlobalVar();
    return lights;
}
Lights GetLights(float4 shadowCoord, half3x3 TBN)
{
    Lights lights;
    lights.shadowMask = 0;
    lights.mainlight = GetMainLight(shadowCoord);
    lights.mainlightColor = lights.mainlight.color;
    lights.mainlightDirection = GetDirectionTransformSpaceFromWorld(lights.mainlight.direction, true, TBN);
    lights.mainlightDirection.LS = half3(0.0, 0.0, 1.0);
    lights.mainlightDistanceAttenuation = lights.mainlight.distanceAttenuation;
    lights.mainlightShadowAttenuation = lights.mainlight.shadowAttenuation;
    lights.mainlightLayerMask = lights.mainlight.layerMask;
    lights.addlightCount = GetAdditionalLightsCount();
    // lights.lightGlobalVar = GetLightGlobalVar();
    // lights.shadowGlobalVar = GetShadowGlobalVar();
    return lights;
}
Lights GetLights(float4 shadowCoord, float3 positionWS, float4 shadowMask, half3x3 TBN)
{
    Lights lights;
    lights.shadowMask = shadowMask;
    lights.mainlight = GetMainLight(shadowCoord, positionWS, lights.shadowMask);
    lights.mainlightColor = lights.mainlight.color;
    lights.mainlightDirection = GetDirectionTransformSpaceFromWorld(lights.mainlight.direction, true, TBN);
    lights.mainlightDirection.LS = half3(0.0, 0.0, 1.0);
    lights.mainlightShadowAttenuation = lights.mainlight.shadowAttenuation;
    lights.mainlightLayerMask = lights.mainlight.layerMask;
    lights.addlightCount = GetAdditionalLightsCount();
    // lights.lightGlobalVar = GetLightGlobalVar();
    // lights.shadowGlobalVar = GetShadowGlobalVar();
    return lights;
}
// Lights GetLights(InputData inputData, float4 shadowMask, AmbientOcclusionFactor aoFactor, half3x3 TBN)
// {
//     Lights lights;
//     lights.shadowMask = shadowMask;
//     lights.mainlight = GetMainLight(inputData, shadowMask, aoFactor);
//     lights.mainlightColor = lights.mainlight.color;
//     lights.mainlightDirection = GetDirectionTransformSpaceFromWorld(lights.mainlight.direction, true, TBN);
//     lights.mainlightDirection.LS = half3(0.0, 0.0, 1.0);
//     lights.mainlightDistanceAttenuation = lights.mainlight.distanceAttenuation;
//     lights.mainlightShadowAttenuation = lights.mainlight.shadowAttenuation;
//     lights.mainlightLayerMask = lights.mainlight.layerMask;
//     lights.addlightCount = GetAdditionalLightsCount();
//     // lights.lightGlobalVar = GetLightGlobalVar();
//     // lights.shadowGlobalVar = GetShadowGlobalVar();
//     return lights;
// }

//--------------------------------------------------------
//光照探针和光照贴图(bakedGI)的抽象,间接光漫反射
// struct LightingProbesGlobalVar
// {
//     half4 unity_AmbientSky;//渐变环境光情况下的天空环境光颜色
//     half4 unity_AmbientEquator;//渐变环境光情况下的赤道环境光颜色
//     half4 unity_AmbientGround;//渐变环境光情况下的地面环境光颜色
//     half4 unity_ProbesOcclusion;//探针遮蔽, 如果不使用光照贴图而是使用SH, 那么ShadowMask = unity_ProbesOcclusion;
//     half4 unity_SHAr;
//     half4 unity_SHAg;
//     half4 unity_SHAb;
//     half4 unity_SHBr;
//     half4 unity_SHBg;
//     half4 unity_SHBb;
//     half4 unity_SHC;

// };
// struct LightingMapGlobalVar
// {
//     // float4 unity_LightmapST;//静态光照贴图的ST属性
//     float4 unity_DynamicLightmapST;//动态光照贴图的ST属性
//     TEXTURE2D(unity_Lightmap);//声明光照贴图
//     SAMPLER(samplerunity_Lightmap);//声明光照贴图采样器
//     TEXTURE2D_ARRAY(unity_Lightmaps);//声明光照贴图数组
//     SAMPLER(samplerunity_Lightmaps);//声明光照贴图数组采样器
//     TEXTURE2D(unity_DynamicLightmap);//声明动态光照贴图
//     SAMPLER(samplerunity_DynamicLightmap);//声明动态光照贴图采样器
//     TEXTURE2D(unity_LightmapInd);
//     TEXTURE2D_ARRAY(unity_LightmapsInd);
//     TEXTURE2D(unity_DynamicDirectionality);
// };
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
// LightingProbesGlobalVar GetLightingProbesGlobalVar()
// {
//     LightingProbesGlobalVar lightingProbesGlobalVar;
//     lightingProbesGlobalVar.unity_AmbientSky = unity_AmbientSky;
//     lightingProbesGlobalVar.unity_AmbientEquator = unity_AmbientEquator;
//     lightingProbesGlobalVar.unity_AmbientGround = unity_AmbientGround;
//     lightingProbesGlobalVar.unity_ProbesOcclusion = unity_ProbesOcclusion;
//     return lightingProbesGlobalVar;
// }
// LightingMapGlobalVar GetLightingMapGlobalVar()
// {
//     LightingMapGlobalVar lightingMapGlobalVar;
//     // lightingMapGlobalVar.unity_LightmapST = unity_LightmapST;
//     lightingMapGlobalVar.unity_DynamicLightmapST = unity_DynamicLightmapST;
//     lightingMapGlobalVar.unity_Lightmap = unity_Lightmap;
//     lightingMapGlobalVar.samplerunity_Lightmap = samplerunity_Lightmap;
//     lightingMapGlobalVar.unity_Lightmaps = unity_Lightmaps;
//     lightingMapGlobalVar.samplerunity_Lightmaps = samplerunity_Lightmaps;
//     lightingMapGlobalVar.unity_DynamicLightmap = unity_DynamicLightmap;
//     lightingMapGlobalVar.samplerunity_DynamicLightmap = samplerunity_DynamicLightmap;
//     lightingMapGlobalVar.unity_LightmapInd = unity_LightmapInd;
//     lightingMapGlobalVar.unity_LightmapsInd = unity_LightmapsInd;
//     lightingMapGlobalVar.unity_DynamicDirectionality = unity_DynamicDirectionality;
//     return lightingMapGlobalVar;
// }

EnvLighting GetEnvLighting(float2 staticLightmapUV, float2 dynamicLightmapUV, half3 vertexSH, half3 normalWS, half3 viewDir, float2 positionSS, float3 absolutePositionWS, Light mainlight)
{
    EnvLighting envLighting = (EnvLighting)0;
    #if defined(DYNAMICLIGHTMAP_ON)
        envLighting.dynamicLightingMap = SampleLightmap(0, dynamicLightmapUV, normalWS);
    #endif
    #if defined(LIGHTMAP_ON)
        envLighting.staticLightingMap = SampleLightmap(staticLightmapUV, 0, normalWS);
    #endif
    #if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
        envLighting.volumeProbe = SampleProbeVolumePixel(vertexSH, absolutePositionWS, normalWS, viewDir, positionSS);
    #endif
    #if defined(REQUIRE_SH)
        envLighting.lightingProbes = SampleSHPixel(vertexSH, normalWS);
        envLighting.vertexSH = vertexSH;
    #endif

    #if defined(LIGHTMAP_ON) && defined(DYNAMICLIGHTMAP_ON)
        envLighting.bakedGI = envLighting.dynamicLightingMap + envLighting.staticLightingMap;
    #elif defined(DYNAMICLIGHTMAP_ON)
        envLighting.bakedGI = envLighting.dynamicLightingMap;
    #elif defined(LIGHTMAP_ON)
        envLighting.bakedGI = envLighting.staticLightingMap;
    #elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
        envLighting.bakedGI = envLighting.volumeProbe;
    #elif defined(REQUIRE_SH)
        envLighting.bakedGI = envLighting.lightingProbes;
    #endif

    #if defined(LIGHTMAP_ON) && defined(_MIXED_LIGHTING_SUBTRACTIVE)
        envLighting.bakedGI = SubtractDirectMainLightFromLightmap(mainlight, normalWS, envLighting.bakedGI);
    #endif
    // envLighting.lightingProbesGlobalVar = GetLightingProbesGlobalVar();
    // envLighting.lightingMapGlobalVar = GetLightingMapGlobalVar();
    return envLighting;
}
//--------------------------------------------------------
//反射探针抽象化,间接光镜面反射
// struct ReflectProbesGlobalVar
// {
//     half4 _GlossyEnvironmentColor;//_GlossyEnvironmentColor是一个全局颜色变量, 用于Unity URP中的环境照明.它是根据管道脚本中的skybox灯光系数 or 渐变环境照明 or 单一颜色环境照明 计算的.
//     TEXTURECUBE(_GlossyEnvironmentCubeMap);//光泽环境立方体贴图, 仅包含天空盒信息的立方体贴图
//     SAMPLER(sampler_GlossyEnvironmentCubeMap);//_GlossyEnvironmentCubeMap对应的采样器
//     half4 _GlossyEnvironmentCubeMap_HDR;//_GlossyEnvironmentCubeMap对应的HDR解码参数
//     TEXTURECUBE(unity_SpecCube0);//距离最近的反射探针捕捉的立方体贴图
//     SAMPLER(samplerunity_SpecCube0);
//     real4 unity_SpecCube0_HDR;
//     float4 unity_SpecCube0_BoxMax;//距离最近的反射探针捕捉的立方体贴图的盒投影参数
//     float4 unity_SpecCube0_BoxMin;//距离最近的反射探针捕捉的立方体贴图的盒投影参数
//     float4 unity_SpecCube0_ProbePosition;//距离最近的反射探针的位置
//     TEXTURECUBE(unity_SpecCube1);//距离第二近的反射探针捕捉的立方体贴图
//     SAMPLER(samplerunity_SpecCube1);
//     real4 unity_SpecCube1_HDR;
//     float4 unity_SpecCube1_BoxMax;//距离第二近的反射探针捕捉的立方体贴图的盒投影参数
//     float4 unity_SpecCube1_BoxMin;//距离第二近的反射探针捕捉的立方体贴图的盒投影参数
//     float4 unity_SpecCube1_ProbePosition;//距离第二近的反射探针的位置
// };
struct EnvReflect
{
    half mip;
    half3 reflectProbes;//最近反射探针捕捉
    half3 reflectSkybox;//仅包含天空盒的反射信息
    half3 bakedReflect;//反射探针捕捉 or 天空盒的反射信息 or 反射探针混合,一般情况下使用这个即可
    // ReflectProbesGlobalVar reflectProbesGlobalVar;

};
// ReflectProbesGlobalVar GetReflectProbesGlobalVar()
// {
//     ReflectProbesGlobalVar reflectProbesGlobalVar;
//     reflectProbesGlobalVar._GlossyEnvironmentColor = _GlossyEnvironmentColor;
//     reflectProbesGlobalVar._GlossyEnvironmentCubeMap = _GlossyEnvironmentCubeMap;
//     reflectProbesGlobalVar.sampler_GlossyEnvironmentCubeMap = sampler_GlossyEnvironmentCubeMap;
//     reflectProbesGlobalVar._GlossyEnvironmentCubeMap_HDR = _GlossyEnvironmentCubeMap_HDR;
//     reflectProbesGlobalVar.unity_SpecCube0 = unity_SpecCube0;
//     reflectProbesGlobalVar.samplerunity_SpecCube0 = samplerunity_SpecCube0;
//     reflectProbesGlobalVar.unity_SpecCube0_HDR = unity_SpecCube0_HDR;
//     reflectProbesGlobalVar.unity_SpecCube0_BoxMax = unity_SpecCube0_BoxMax;
//     reflectProbesGlobalVar.unity_SpecCube0_BoxMin = unity_SpecCube0_BoxMin;
//     reflectProbesGlobalVar.unity_SpecCube0_ProbePosition = unity_SpecCube0_ProbePosition;
//     reflectProbesGlobalVar.unity_SpecCube1 = unity_SpecCube1;
//     reflectProbesGlobalVar.samplerunity_SpecCube1 = samplerunity_SpecCube1;
//     reflectProbesGlobalVar.unity_SpecCube1_HDR = unity_SpecCube1_HDR;
//     reflectProbesGlobalVar.unity_SpecCube1_BoxMax = unity_SpecCube1_BoxMax;
//     reflectProbesGlobalVar.unity_SpecCube1_BoxMin = unity_SpecCube1_BoxMin;
//     reflectProbesGlobalVar.unity_SpecCube1_ProbePosition = unity_SpecCube1_ProbePosition;
//     return reflectProbesGlobalVar;
// }

//forward渲染通道
half3 GetReflectSkybox(half3 reflectDirWS, half mip)
{
    half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_GlossyEnvironmentCubeMap, sampler_GlossyEnvironmentCubeMap, reflectDirWS, mip));
    #if defined(UNITY_USE_NATIVE_HDR) || defined(UNITY_DOTS_INSTANCING_ENABLED)
        return encodedIrradiance.rbg;
    #else
        return DecodeHDREnvironment(encodedIrradiance, _GlossyEnvironmentCubeMap_HDR);
    #endif
}
half3 GetReflectProbes(half3 reflectDirWS, float3 positionWS, half mip)
{
    #ifdef _REFLECTION_PROBE_BOX_PROJECTION
        reflectDirWS = BoxProjectedCubemapDirection(reflectDirWS, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    #endif // _REFLECTION_PROBE_BOX_PROJECTION
    half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDirWS, mip));
    #if defined(UNITY_USE_NATIVE_HDR)
        return encodedIrradiance.rgb;
    #else
        return DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    #endif // UNITY_USE_NATIVE_HDR

}
half3 CalculateIrradianceFromReflectionProbes_Modif (half3 reflectSkybox, half3 reflectDirWS, float3 positionWS, half mip)
{
    half probe0Volume = CalculateProbeVolumeSqrMagnitude(unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    half probe1Volume = CalculateProbeVolumeSqrMagnitude(unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
    half volumeDiff = probe0Volume - probe1Volume;
    float importanceSign = unity_SpecCube1_BoxMin.w;
    bool probe0Dominant = importanceSign > 0.0f || (importanceSign == 0.0f && volumeDiff < - 0.0001h);
    bool probe1Dominant = importanceSign < 0.0f || (importanceSign == 0.0f && volumeDiff > 0.0001h);
    float desiredWeightProbe0 = CalculateProbeWeight(positionWS, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float desiredWeightProbe1 = CalculateProbeWeight(positionWS, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
    float weightProbe0 = probe1Dominant ? min(desiredWeightProbe0, 1.0f - desiredWeightProbe1) : desiredWeightProbe0;
    float weightProbe1 = probe0Dominant ? min(desiredWeightProbe1, 1.0f - desiredWeightProbe0) : desiredWeightProbe1;
    float totalWeight = weightProbe0 + weightProbe1;
    weightProbe0 /= max(totalWeight, 1.0f);
    weightProbe1 /= max(totalWeight, 1.0f);
    half3 irradiance = half3(0.0h, 0.0h, 0.0h);
    half3 originalReflectVector = reflectDirWS;
    if (weightProbe0 > 0.01f)
    {
        #ifdef _REFLECTION_PROBE_BOX_PROJECTION
            reflectDirWS = BoxProjectedCubemapDirection(originalReflectVector, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
        #endif
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDirWS, mip));
        #if defined(UNITY_USE_NATIVE_HDR)
            irradiance += weightProbe0 * encodedIrradiance.rbg;
        #else
            irradiance += weightProbe0 * DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
        #endif
    }
    if (weightProbe1 > 0.01f)
    {
        #ifdef _REFLECTION_PROBE_BOX_PROJECTION
            reflectDirWS = BoxProjectedCubemapDirection(originalReflectVector, positionWS, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
        #endif
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube1, samplerunity_SpecCube1, reflectDirWS, mip));
        #if defined(UNITY_USE_NATIVE_HDR) || defined(UNITY_DOTS_INSTANCING_ENABLED)
            irradiance += weightProbe1 * encodedIrradiance.rbg;
        #else
            irradiance += weightProbe1 * DecodeHDREnvironment(encodedIrradiance, unity_SpecCube1_HDR);
        #endif
    }
    if (totalWeight < 0.99f)
    {
        irradiance += (1.0f - totalWeight) * reflectSkybox;
    }
    return irradiance;
}
half3 GlossyEnvironmentReflection_Modif (half3 reflectProbes, half3 reflectSkybox, half3 reflectDirWS, float3 positionWS, half mip, half occlusion)
{
    #if !defined(_ENVIRONMENTREFLECTIONS_OFF)
        half3 irradiance = 1;

        #ifdef _REFLECTION_PROBE_BLENDING
            irradiance = CalculateIrradianceFromReflectionProbes_Modif (reflectSkybox, reflectDirWS, positionWS, mip);
        #else
            irradiance = reflectProbes;
        #endif // _REFLECTION_PROBE_BLENDING
        return irradiance * occlusion;
    #else
        return _GlossyEnvironmentColor.rgb * occlusion;
    #endif // _ENVIRONMENTREFLECTIONS_OFF

}

EnvReflect GetEnvReflect(half3 reflectDirWS, float3 positionWS, half perceptualRoughness, float2 normalizedScreenSpaceUV,
bool customReflect, TEXTURECUBE(_customReflectMap), SAMPLER(sampler_customReflectMap), half4 _customReflectMap_HDR)
{
    EnvReflect envReflect;
    envReflect.mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    envReflect.reflectProbes = GetReflectProbes(reflectDirWS, positionWS, envReflect.mip);
    envReflect.reflectSkybox = GetReflectSkybox(reflectDirWS, envReflect.mip);
    envReflect.bakedReflect = GlossyEnvironmentReflection_Modif (envReflect.reflectProbes, envReflect.reflectSkybox, reflectDirWS, positionWS, envReflect.mip, 1.0);
    //支持forward+
    #if defined(USE_FORWARD_PLUS)
        //forward+似乎无法单独获得 reflectProbes reflectSkybox,只能获得已经混合完毕的bakedReflect
        envReflect.reflectProbes = 0.0;
        envReflect.reflectSkybox = 0.0;
        envReflect.bakedReflect = GlossyEnvironmentReflection(reflectDirWS, positionWS, perceptualRoughness, 1.0h, normalizedScreenSpaceUV);
    #endif
    
    if (customReflect)
    {
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_customReflectMap, sampler_customReflectMap, reflectDirWS, envReflect.mip));
        #if defined(UNITY_USE_NATIVE_HDR) || defined(UNITY_DOTS_INSTANCING_ENABLED)
            envReflect.bakedReflect = encodedIrradiance.rbg;
        #else
            envReflect.bakedReflect = DecodeHDREnvironment(encodedIrradiance, _customReflectMap_HDR);
        #endif
    }
    // envReflect.reflectProbesGlobalVar = GetReflectProbesGlobalVar();
    return envReflect;
}


//--------------------------------------------------------
//unity内置雾的抽象
// struct FogGlobalVar
// {
//     float4 unity_FogParams;//雾计算参数 : (density / sqrt(ln(2)), density / ln(2), -1 / (end - start), end / (end - start))
//     real4  unity_FogColor;//雾颜色

// };
struct EnvFog
{
    real4 fogColor;
    half fogFactor;
    half3 MixFog(half3 fragColor)
    {
        return MixFogColor(fragColor, unity_FogColor.rgb, fogFactor);
    }
    // FogGlobalVar fogGlobalVar;

};
EnvFog GetEnvfog(Varyings vertexShaderInput, float3 positionWS)
{
    EnvFog envFog = (EnvFog)0;
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            half vertexFogFactor = vertexShaderInput.fogFactorAndVertexLight.x;
            envFog.fogFactor = InitializeInputDataFog(float4(positionWS, 1.0), vertexFogFactor);
        #else
            half vertexFogFactor = vertexShaderInput.fogFactor;
            envFog.fogFactor = InitializeInputDataFog(float4(positionWS, 1.0), vertexFogFactor);
        #endif
    #endif
    envFog.fogColor = unity_FogColor;
    // envFog.fogGlobalVar.unity_FogColor = envFog.fogColor;
    // envFog.fogGlobalVar.unity_FogParams = unity_FogParams;
    return envFog;
}
//--------------------------------------------------------
//环境光遮蔽
struct EnvOcclusion
{
    half bakedAO;
    half horizonAO;//用于防止漏光
    half SpecularAO;//用于遮蔽镜面反射
    AmbientOcclusionFactor SSAO;
    AmbientOcclusionFactor mixAO;//★混合了 bakedAO + horizonAO + SSAO
    half mixDirectDiffuseAO;
    half mixDirectSpecularAO;
    half mixIndirectDiffuserAO;
    half mixIndirectSpecularAO;

    real3 GTAOMultiBounce(real3 albedo)
    {
        real3 a = 2.0404 * albedo - 0.3324;
        real3 b = -4.7951 * albedo + 0.6417;
        real3 c = 2.7552 * albedo + 0.6903;
        real x = mixAO.indirectAmbientOcclusion;
        return max(x, ((x * a + b) * x + c) * x);
    }
    real3 GTAOMultiBounce(real ao, real3 albedo)
    {
        real3 a = 2.0404 * albedo - 0.3324;
        real3 b = -4.7951 * albedo + 0.6417;
        real3 c = 2.7552 * albedo + 0.6903;
        real x = ao;
        return max(x, ((x * a + b) * x + c) * x);
    }
    real GetSpecularOcclusionFromAmbientOcclusion(real NdotV, real ao, real perceptualRoughness)
    {
        half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        return saturate(PositivePow(NdotV + ao, exp2(-16.0 * roughness - 1.0)) - 1.0 + ao);
    }
    // Ref: Horizon Occlusion for Normal Mapped Reflections: http://marmosetco.tumblr.com/post/81245981087
    real GetHorizonOcclusion(real3 reflectDirWS, real3 vertexNormal, real horizonFade)
    {
        real specularOcclusion = saturate(1.0 + horizonFade * dot(reflectDirWS, vertexNormal));
        return specularOcclusion * specularOcclusion;
    }
};
EnvOcclusion GetEnvOcclusion(real2 positionSSxy, real3 reflectDirWS, real3 vertexNormal, real horizonFade, real aoMap, real NdotV, real perceptualRoughness)
{
    EnvOcclusion ao = (EnvOcclusion)1;
    #if defined(_OCCLUSION)
        ao.bakedAO = aoMap;
        ao.horizonAO = ao.GetHorizonOcclusion(reflectDirWS, vertexNormal, horizonFade);
        ao.SSAO = GetScreenSpaceAmbientOcclusion(positionSSxy);
        ao.mixDirectDiffuseAO = min(ao.SSAO.directAmbientOcclusion, saturate(ao.bakedAO + 0.3));
        ao.mixDirectSpecularAO = min(ao.mixDirectDiffuseAO, ao.horizonAO);
        ao.mixIndirectDiffuserAO = min(ao.SSAO.indirectAmbientOcclusion, ao.bakedAO);
        // ao.mixIndirectSpecularAO = ao.GetSpecularOcclusionFromAmbientOcclusion(NdotV, min(ao.mixIndirectDiffuserAO, ao.horizonAO), perceptualRoughness);
        // GetSpecularOcclusionFromAmbientOcclusion()使用了pow()和exp2()两个超越函数,消耗较大,暂不使用
        ao.mixIndirectSpecularAO = min(ao.mixIndirectDiffuserAO, ao.horizonAO);

        ao.mixAO.directAmbientOcclusion = min(ao.SSAO.directAmbientOcclusion, saturate(ao.bakedAO + 0.3));
        ao.mixAO.indirectAmbientOcclusion = min(min(ao.SSAO.indirectAmbientOcclusion, ao.bakedAO), ao.horizonAO);
    #endif
    return ao;
}
//--------------------------------------------------------
//扩展表面数据
struct SurfaceDataExtend
{
    //常用
    half3 albedo;
    half perceptualSmoothness;
    half perceptualRoughness;
    half roughness;
    half metallic;
    half reflectivity;
    half3 diffuse;
    half3 specular;
    half3 normalTS;
    half3 emission;
    half occlusion;
    half alpha;
    half height;
    half clearCoatMask;
    half clearCoatPerceptualSmoothness;
    half clearCoatPerceptualRoughness;
    //不常用
    half curvature;//曲率
    half convexity;//凸度
    half concavity;//凹度(空腔)
    half thickness;//厚度
    half3 subsurface;//次表面颜色
    // half3 position;//位置
    // half3 bentNormalsTS;//环境法线(弯曲法线)
    SurfaceData surfaceData;

    void UpdateSurfaceData()
    {
        surfaceData.albedo = albedo;
        surfaceData.specular = specular;
        surfaceData.metallic = metallic;
        surfaceData.smoothness = perceptualSmoothness;
        surfaceData.normalTS = normalTS;
        surfaceData.emission = emission;
        surfaceData.occlusion = occlusion;
        surfaceData.alpha = alpha;
        surfaceData.clearCoatMask = clearCoatMask;
        surfaceData.clearCoatSmoothness = clearCoatPerceptualSmoothness;
    }
    void UpdateSurfaceDataExtendFromSurfaceData()
    {
        albedo = surfaceData.albedo;
        specular = surfaceData.specular;
        metallic = surfaceData.metallic;
        perceptualSmoothness = surfaceData.smoothness;
        normalTS = surfaceData.normalTS;
        emission = surfaceData.emission;
        occlusion = surfaceData.occlusion;
        alpha = surfaceData.alpha;
        clearCoatMask = surfaceData.clearCoatMask;
        clearCoatPerceptualSmoothness = surfaceData.clearCoatSmoothness;
    }
    void UpdateAllData()
    {
        half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
        reflectivity = half(1.0) - oneMinusReflectivity;
        diffuse = albedo * oneMinusReflectivity * _BaseColor.rgb;
        specular = lerp(kDielectricSpec.rgb, albedo, metallic) * _SpecColor;

        surfaceData.albedo = albedo;
        surfaceData.specular = specular;
        surfaceData.metallic = metallic;
        surfaceData.smoothness = perceptualSmoothness;
        surfaceData.normalTS = normalTS;
        surfaceData.emission = emission;
        surfaceData.occlusion = occlusion;
        surfaceData.alpha = alpha;
        surfaceData.clearCoatMask = clearCoatMask;
        surfaceData.clearCoatSmoothness = clearCoatPerceptualSmoothness;
    }
    BRDFData GetBRDFData()
    {
        BRDFData brdfData = (BRDFData)0;
        brdfData.albedo = albedo;
        brdfData.diffuse = diffuse;
        brdfData.specular = specular;
        brdfData.reflectivity = reflectivity;
        brdfData.perceptualRoughness = perceptualRoughness;
        brdfData.roughness = roughness;
        brdfData.roughness2 = max(brdfData.roughness * brdfData.roughness, HALF_MIN);
        brdfData.grazingTerm = saturate(perceptualSmoothness + brdfData.reflectivity);
        brdfData.normalizationTerm = brdfData.roughness * half(4.0) + half(2.0);
        brdfData.roughness2MinusOne = brdfData.roughness2 - half(1.0);
        return brdfData;
    }
    BRDFData GetBRDFDataClearCoat(inout BRDFData brdfData)
    {
        BRDFData brdfDataClearCoat = (BRDFData)0;
        #if defined(_CLEARCOAT)
            brdfDataClearCoat.albedo = half(1.0);
            brdfDataClearCoat.diffuse = kDielectricSpec.aaa; // 1 - kDielectricSpec
            brdfDataClearCoat.specular = kDielectricSpec.rgb;
            brdfDataClearCoat.reflectivity = kDielectricSpec.r;
            brdfDataClearCoat.perceptualRoughness = clearCoatPerceptualRoughness;
            brdfDataClearCoat.roughness = max(PerceptualRoughnessToRoughness(brdfDataClearCoat.perceptualRoughness), HALF_MIN_SQRT);
            brdfDataClearCoat.roughness2 = max(brdfDataClearCoat.roughness * brdfDataClearCoat.roughness, HALF_MIN);
            brdfDataClearCoat.normalizationTerm = brdfDataClearCoat.roughness * half(4.0) + half(2.0);
            brdfDataClearCoat.roughness2MinusOne = brdfDataClearCoat.roughness2 - half(1.0);
            brdfDataClearCoat.grazingTerm = saturate(clearCoatPerceptualSmoothness + kDielectricSpec.x);
            // 效果相对较小，只在非移动平台生效
            #if !defined(SHADER_API_MOBILE)
                half ieta = lerp(1.0h, CLEAR_COAT_IETA, clearCoatMask);
                half coatRoughnessScale = Sq(ieta);
                half sigma = RoughnessToVariance(PerceptualRoughnessToRoughness(brdfData.perceptualRoughness));
                brdfData.perceptualRoughness = RoughnessToPerceptualRoughness(VarianceToRoughness(sigma * coatRoughnessScale));
                brdfData.roughness = max(PerceptualRoughnessToRoughness(brdfData.perceptualRoughness), HALF_MIN_SQRT);
                brdfData.roughness2 = max(brdfData.roughness * brdfData.roughness, HALF_MIN);
                brdfData.normalizationTerm = brdfData.roughness * 4.0h + 2.0h;
                brdfData.roughness2MinusOne = brdfData.roughness2 - 1.0h;
            #endif
            brdfData.specular = lerp(brdfData.specular, ConvertF0ForClearCoat15(brdfData.specular), clearCoatMask);
        #endif
        return brdfDataClearCoat;
    }
    void SetupSubsurfaceColor(half3 Subsurface)
    {
        subsurface = Subsurface;
    }
    void SetupCurvature(half Curvature)
    {
        curvature = Curvature;
    }
};
SurfaceDataExtend GetSurfaceDataExtend(
    half3 albedo,
    half perceptualRoughness,
    half metallic,
    half3 normalTS,
    half3 emission,
    half occlusion,
    half alpha,
    half height,
    half clearCoatMask,
    half clearCoatPerceptualRoughness,
    half _Roughness,
    half4 _BaseColor,
    half3 _SpecColor,
    half _Metallic,
    half3 _EmissionColor,
    half _ClearCoatMask,
    half _ClearCoatSmoothness,
    half _OcclusionStrength
)
{
    SurfaceDataExtend surfaceDataExtend = (SurfaceDataExtend)0;
    surfaceDataExtend.albedo = albedo;
    surfaceDataExtend.normalTS = normalTS;
    surfaceDataExtend.perceptualRoughness = saturate(perceptualRoughness * _Roughness);
    surfaceDataExtend.perceptualSmoothness = 1.0 - surfaceDataExtend.perceptualRoughness;
    surfaceDataExtend.roughness = max(PerceptualRoughnessToRoughness(surfaceDataExtend.perceptualRoughness), HALF_MIN_SQRT);
    surfaceDataExtend.metallic = saturate(metallic * _Metallic);
    half oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceDataExtend.metallic);
    surfaceDataExtend.reflectivity = half(1.0) - oneMinusReflectivity;
    surfaceDataExtend.diffuse = surfaceDataExtend.albedo * oneMinusReflectivity * _BaseColor.rgb;
    surfaceDataExtend.specular = lerp(kDielectricSpec.rgb, surfaceDataExtend.albedo, surfaceDataExtend.metallic) * _SpecColor;
    surfaceDataExtend.emission = emission * _EmissionColor;

    #if defined(_OCCLUSION)
        surfaceDataExtend.occlusion = saturate(lerp(1.0, occlusion, _OcclusionStrength));
    #else
        surfaceDataExtend.occlusion = 1.0;
    #endif
    surfaceDataExtend.alpha = alpha * _BaseColor.a;
    surfaceDataExtend.height = height;

    surfaceDataExtend.clearCoatMask = 0;
    surfaceDataExtend.clearCoatPerceptualSmoothness = 1;
    surfaceDataExtend.clearCoatPerceptualRoughness = 0;
    #if defined(_CLEARCOAT)
        surfaceDataExtend.clearCoatMask = clearCoatMask * _ClearCoatMask;
        surfaceDataExtend.clearCoatPerceptualSmoothness = saturate((1.0 - clearCoatPerceptualRoughness) * _ClearCoatSmoothness);
        surfaceDataExtend.clearCoatPerceptualRoughness = 1.0 - surfaceDataExtend.clearCoatPerceptualSmoothness;
    #endif
    
    surfaceDataExtend.surfaceData.albedo = surfaceDataExtend.albedo;
    surfaceDataExtend.surfaceData.specular = surfaceDataExtend.specular;
    surfaceDataExtend.surfaceData.metallic = surfaceDataExtend.metallic;
    surfaceDataExtend.surfaceData.smoothness = surfaceDataExtend.perceptualSmoothness;
    surfaceDataExtend.surfaceData.normalTS = surfaceDataExtend.normalTS;
    surfaceDataExtend.surfaceData.emission = surfaceDataExtend.emission;
    surfaceDataExtend.surfaceData.occlusion = surfaceDataExtend.occlusion;
    surfaceDataExtend.surfaceData.alpha = surfaceDataExtend.alpha;
    surfaceDataExtend.surfaceData.clearCoatMask = surfaceDataExtend.clearCoatMask;
    surfaceDataExtend.surfaceData.clearCoatSmoothness = surfaceDataExtend.clearCoatPerceptualSmoothness;

    // #ifdef _ALPHAPREMULTIPLY_ON
    // surfaceDataExtend.diffuse *= surfaceDataExtend.alpha;
    // surfaceDataExtend.alpha = surfaceDataExtend.alpha * oneMinusReflectivity + surfaceDataExtend.reflectivity; // NOTE: alpha modified and propagated up.
    // surfaceDataExtend.surfaceData.alpha = surfaceDataExtend.alpha;
    // #endif
    return surfaceDataExtend;
}
//--------------------------------------------------------
//贴花
SurfaceDataExtend ApplyDecalToSurfaceDataExtend(SurfaceDataExtend surfaceDataEx, half3 normalWS, float4 positionDS)
{
    #ifdef _DBUFFER
        half3 specularColor = 0;
        ApplyDecal(positionDS, surfaceDataEx.albedo, specularColor, normalWS,
        surfaceDataEx.metallic, surfaceDataEx.occlusion, surfaceDataEx.perceptualSmoothness);
        surfaceDataEx.UpdateAllData();//需要更新
        // return surfaceDataEx;
    #endif
    return surfaceDataEx;
}
//--------------------------------------------------------
//细节
void ApplyDetailToSurfaceDataExtend(inout SurfaceDataExtend surfaceDataEx, float2 baseUV, UnityTexture2D DetailMap0, half scaleOrRotate, half noise, half _DetailOcclusionStrength0, half _DetailNormalScale0)
{
    half DetailMask = DetailMap0.SampleSupportNoTileing(baseUV, scaleOrRotate, noise).a;
    float2 detailUV = DetailMap0.GetTransformedUV(baseUV);
    half4 DetailTex = DetailMap0.SampleSupportNoTileing(detailUV, scaleOrRotate, noise);//_DetailNormalMapScale
    half DetailOcclusion = LerpWhiteTo(DetailTex.b, _DetailOcclusionStrength0);
    half3 DetailNormalTS = DetailMap0.DecodeNormalRG(DetailTex.rg, _DetailNormalScale0);
    
    surfaceDataEx.occlusion = min(surfaceDataEx.occlusion, LerpWhiteTo(DetailOcclusion, DetailMask));
    surfaceDataEx.normalTS = lerp(surfaceDataEx.normalTS, BlendNormalRNM(surfaceDataEx.normalTS, DetailNormalTS), DetailMask);
}

// void ApplyDetailToSurfaceDataExtend(inout SurfaceDataExtend surfaceDataEx, float2 baseUV,
// UnityTexture2D DetailMap0, half _DetailOcclusionStrength0, half _DetailNormalScale0,
// UnityTexture2D DetailMap1, half _DetailOcclusionStrength1, half _DetailNormalScale1)
// {
//     #if defined(_DETAIL) || defined(_DETAIL_2MULTI)
//         ApplyDetailToSurfaceDataExtend(surfaceDataEx, baseUV, DetailMap0, _DetailOcclusionStrength0, _DetailNormalScale0);
//         #if defined(_DETAIL_2MULTI)
//             ApplyDetailToSurfaceDataExtend(surfaceDataEx, baseUV, DetailMap1, _DetailOcclusionStrength1, _DetailNormalScale1);
//         #endif
//     #endif
// }

//--------------------------------------------------------
//可获得的数据
struct Available
{
    //模型
    Model model;
    //摄像机
    Camera camera;
    //灯光
    Lights lights;
    //AO
    EnvOcclusion envOcclusion;
    //光照探针or光照贴图(BakedGI),间接光漫反射
    EnvLighting envLighting;
    //反射探针,间接光镜面反射
    EnvReflect envReflect;
    //Unity默认的简单雾效
    EnvFog envFog;
    //顶点光照
    half3 vertexLighting;
    Direction reflectDir;
    Direction halfDir;
    InputData inputData;
    void UpdateAvailableFromInputData()
    {
        envLighting.bakedGI = inputData.bakedGI;
        envFog.fogFactor = inputData.fogCoord;
        model.position.SS.xy = inputData.normalizedScreenSpaceUV;
        model.normal.WS = inputData.normalWS;
        model.position.CS = inputData.positionCS;
        model.position.WS = inputData.positionWS;
        model.position.SC = inputData.shadowCoord;
        lights.shadowMask = inputData.shadowMask;
        model.TBN = inputData.tangentToWorld;
        vertexLighting = inputData.vertexLighting;
        camera.viewDir.WS = inputData.viewDirectionWS;
        #if defined(DEBUG_DISPLAY)
            #if defined(DYNAMICLIGHTMAP_ON)
                model.dynamicLightmapUV = inputData.dynamicLightmapUV;
            #endif
            #if defined(LIGHTMAP_ON)
                model.staticLightmapUV = inputData.staticLightmapUV;
            #else
                envLighting.vertexSH = inputData.vertexSH;
            #endif
            model.uv = inputData.uv;
        #endif
    }
    void UpdateInputData()
    {
        inputData.bakedGI = envLighting.bakedGI;
        inputData.fogCoord = envFog.fogFactor;
        inputData.normalizedScreenSpaceUV = model.position.SS.xy;
        inputData.normalWS = model.normal.WS;
        inputData.positionCS = model.position.CS;
        inputData.positionWS = model.position.WS;
        inputData.shadowCoord = model.position.SC;
        inputData.shadowMask = lights.shadowMask;
        inputData.tangentToWorld = model.TBN;
        inputData.vertexLighting = vertexLighting;
        inputData.viewDirectionWS = camera.viewDir.WS;
        #if defined(DEBUG_DISPLAY)
            #if defined(DYNAMICLIGHTMAP_ON)
                inputData.dynamicLightmapUV = model.dynamicLightmapUV;
            #endif
            #if defined(LIGHTMAP_ON)
                inputData.staticLightmapUV = model.staticLightmapUV;
            #else
                inputData.vertexSH = envLighting.vertexSH;
            #endif
            inputData.uv = model.uv;
        #endif
    }
};

Available GetAvailable(Varyings vertexShaderInput, Model model, half perceptualRoughness,
bool customReflect, TEXTURECUBE(_customReflectMap), SAMPLER(sampler_customReflectMap), half4 _customReflectMap_HDR, half aoMap = 1.0, half horizonFade = 1.0)
{
    Available available;
    //获取模型
    available.model = model;
    //获取摄像机
    #if !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS) && !defined(SCENESELECTION_PASS)
        half3 viewDirWS = half3(vertexShaderInput.normalWS.w, vertexShaderInput.tangentWS.w, vertexShaderInput.bitangentWS.w);
    #else
        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(available.model.position.WS);
    #endif
    available.camera = GetMainCamera(viewDirWS, vertexShaderInput.positionCS, available.model.position.WS, available.model.position.SS.xy, available.model.TBN);
    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        available.camera.viewDir.TS = vertexShaderInput.viewDirTS;
    #endif
    //获取灯光
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        half4 shadowMask = SAMPLE_SHADOWMASK(available.model.staticLightmapUV);
    #elif !defined(LIGHTMAP_ON)
        half4 shadowMask = unity_ProbesOcclusion;
    #else
        half4 shadowMask = half4(1, 1, 1, 1);
    #endif
    available.lights = GetLights(available.model.position.SC, available.model.position.WS, shadowMask, available.model.TBN);
    //计算反射向量和半角向量
    available.reflectDir = GetDirectionTransformSpaceFromWorld(reflect(-available.camera.viewDir.WS, available.model.normal.WS), true, available.model.TBN);
    available.halfDir = GetDirectionTransformSpaceFromWorld(available.lights.mainlightDirection.WS + available.camera.viewDir.WS, true, available.model.TBN);

    //获得间接光漫反射(环境光照)
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        available.envLighting = GetEnvLighting(available.model.staticLightmapUV, available.model.dynamicLightmapUV, 0, available.model.normal.WS,
        available.camera.viewDir.WS, available.model.position.SS.xy, GetAbsolutePositionWS(available.model.position.WS), available.lights.mainlight);
    #else
        available.envLighting = (EnvLighting)0;
        #if defined(REQUIRE_SH)
            available.envLighting = GetEnvLighting(0, 0, vertexShaderInput.vertexSH, available.model.normal.WS,
            available.camera.viewDir.WS, available.model.position.SS.xy, GetAbsolutePositionWS(available.model.position.WS), available.lights.mainlight);
        #endif
    #endif

    //获得环境反射
    available.envReflect = GetEnvReflect(available.reflectDir.WS, available.model.position.WS, perceptualRoughness, available.model.position.SS.xy,
    customReflect, _customReflectMap, sampler_customReflectMap, _customReflectMap_HDR);
    //获得雾
    available.envFog = GetEnvfog(vertexShaderInput, available.model.position.WS);
    //获得AO
    available.envOcclusion = GetEnvOcclusion(available.model.position.SS.xy, available.reflectDir.WS, available.model.vertexNormal.WS, horizonFade, aoMap,
    saturate(dot(available.model.normal.WS, available.camera.viewDir.WS)), perceptualRoughness);
    //获得顶点光照
    available.vertexLighting = 0;
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        available.vertexLighting = vertexShaderInput.fogFactorAndVertexLight.yzw;
    #endif
    //获得inputdata
    InputData inputData = (InputData)0;
    inputData.bakedGI = available.envLighting.bakedGI;
    inputData.fogCoord = available.envFog.fogFactor;
    inputData.normalizedScreenSpaceUV = available.model.position.SS.xy;
    inputData.normalWS = available.model.normal.WS;
    inputData.positionCS = available.model.position.CS;
    inputData.positionWS = available.model.position.WS;
    inputData.shadowCoord = available.model.position.SC;
    inputData.shadowMask = available.lights.shadowMask;
    inputData.tangentToWorld = available.model.TBN;
    inputData.vertexLighting = available.vertexLighting;
    inputData.viewDirectionWS = available.camera.viewDir.WS;
    #if defined(DEBUG_DISPLAY)
        #if defined(DYNAMICLIGHTMAP_ON)
            inputData.dynamicLightmapUV = available.model.dynamicLightmapUV;
        #endif
        #if defined(LIGHTMAP_ON)
            inputData.staticLightmapUV = available.model.staticLightmapUV;
        #else
            inputData.vertexSH = available.envLighting.vertexSH;
        #endif
        inputData.uv = available.model.uv;
    #endif
    available.inputData = inputData;

    return available;
}
Available GetAvailable(Varyings vertexShaderInput, float vFac, half3 normalTS, half perceptualRoughness,
bool customReflect, TEXTURECUBE(_customReflectMap), SAMPLER(sampler_customReflectMap), half4 _customReflectMap_HDR, half aoMap = 1.0, half horizonFade = 1.0)
{
    //获取模型
    Model model = GetModel(vertexShaderInput, vFac, normalTS);
    Available available = GetAvailable(vertexShaderInput, model, perceptualRoughness,
    customReflect, _customReflectMap, sampler_customReflectMap, _customReflectMap_HDR, aoMap, horizonFade);
    return available;
}
//------------------------------------------------------
//支持渲染调试器
#if defined(DEBUG_DISPLAY)
    bool SupportDebugDisplay(inout InputData inputData, inout SurfaceDataExtend surfaceDataEx, inout  BRDFData brdfData, inout half4 debugColor)
    {
        debugColor = 0;
        bool CanDebug = CanDebugOverrideOutputColor(inputData, surfaceDataEx.surfaceData, brdfData, debugColor);
        surfaceDataEx.UpdateSurfaceDataExtendFromSurfaceData();
        return CanDebug;
    }
#endif

//------------------------------------------------------

#endif