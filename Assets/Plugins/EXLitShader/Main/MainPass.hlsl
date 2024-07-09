#ifndef MAIN_PASS
#define MAIN_PASS

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#define REQUIRE_SH//注意:由于当SH和光照贴图都不需要时,Litshader的写法会造成插值器的浪费,当确定不需要SH时将此定义注释
// #define SURFACE_GRADIENT

#if defined(_PARALLAXMAP)
    #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif
#if defined(_SHADER_PLANT)
    #if defined(_WINDQUALITY_FASTEST) || defined(_WINDQUALITY_FAST) || defined(_WINDQUALITY_BETTER) || defined(_WINDQUALITY_BEST) || defined(_WINDQUALITY_PALM)
        #define ENABLE_WIND
    #endif
#endif

#define GEOM_TYPE_BRANCH 0
#define GEOM_TYPE_FROND 1
#define GEOM_TYPE_LEAF 2
#define GEOM_TYPE_FACINGLEAF 3


struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float4 texcoord : TEXCOORD0;//UV0
    float4 texcoord1 : TEXCOORD1;//UV1
    float4 texcoord2 : TEXCOORD2;//UV2
    float4 texcoord3 : TEXCOORD3;//UV3
    float4 vertexColor : COLOR;
    uint vertexID : SV_VERTEXID;//顶点ID
    UNITY_VERTEX_INPUT_INSTANCE_ID//实例化相关宏,无脑添加

};
struct Varyings
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
    #elif defined(META_PASS)
        #ifdef EDITOR_VISUALIZATION
            float2 VizUV : TEXCOORD1;
            float4 LightCoord : TEXCOORD2;
        #endif
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
        half4 interpolator : TEXCOORD11;//注意优化
    #endif

    float4 positionCS : SV_POSITION;
    #if !defined(META_PASS)
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    #endif
};

#include "../FunctionLibrary/UnityTextureLibrary.hlsl"
#include "../FunctionLibrary/BaseFunctionLibrary.hlsl"
#include "../FunctionLibrary/UtilityFunctionLibrary.hlsl"
#include "../FunctionLibrary/LightingModelLibrary.hlsl"
#include "../Main/VertexAndFragmentFunction.hlsl"

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    #if !defined(META_PASS)
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    #else
        // output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.texcoord1.xy, input.texcoord2.xy);
        #ifdef EDITOR_VISUALIZATION
            UnityEditorVizData(input.positionOS.xyz, input.texcoord.xy, input.texcoord1.xy, input.texcoord2.xy, output.VizUV, output.LightCoord);
        #endif
    #endif

    //如若计算顶点动画,在此处对input.positionOS,input.normalOS,input.tangentOS计算修改即可
    //------------------------------------------
    VertexAnimator(input, input.positionOS.xyz, input.normalOS, input.tangentOS);
    //------------------------------------------

    //公告牌
    float3 treePos = float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
    #if defined(EFFECT_BILLBOARD)
        // crossfade faces
        bool topDown = (input.texcoord.z > 0.5);
        float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
        float3 cameraDir = normalize(mul((float3x3)UNITY_MATRIX_M, _WorldSpaceCameraPos - treePos));
        float viewDot = max(dot(viewDir, input.normalOS), dot(cameraDir, input.normalOS));
        viewDot *= viewDot;
        viewDot *= viewDot;
        viewDot += topDown ? 0.38 : 0.18; // different scales for horz and vert billboards to fix transition zone

        // if invisible, avoid overdraw
        if (viewDot < 0.3333)
        {
            input.positionOS.xyz = float3(0, 0, 0);
        }
        output.interpolator.a = clamp(viewDot, 0, 1);
        // adjust lighting on billboards to prevent seams between the different faces
        if (topDown)
        {
            input.normalOS += cameraDir;
        }
        else
        {
            half3 binormal = cross(input.normalOS, input.tangentOS.xyz) * input.tangentOS.w;
            float3 right = cross(cameraDir, binormal);
            input.normalOS = cross(binormal, right);
        }
        input.normalOS = normalize(input.normalOS);
    #endif

    //色调变体相关
    #ifdef EFFECT_HUE_VARIATION
        float hueVariationAmount = frac(treePos.x + treePos.y + treePos.z);
        output.interpolator.g = saturate(hueVariationAmount * _HueVariationColor.a);
    #endif

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half4 tangentWS = half4(normalInput.tangentWS.xyz, input.tangentOS.w * GetOddNegativeScale());
    #if !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS) && !defined(SCENESELECTION_PASS)//shadowcasterpass和depthOnlyPass不需要这些
        #if !defined(DEPTHNORMAL_PASS)
            output.positionWS = vertexInput.positionWS;
        #endif
        output.normalWS.xyz = normalInput.normalWS;
        output.tangentWS.xyz = tangentWS.xyz;
        output.bitangentWS.xyz = cross(output.normalWS.xyz, output.tangentWS.xyz) * tangentWS.w;
        
        output.normalWS.w = viewDirWS.x;
        output.tangentWS.w = viewDirWS.y;
        output.bitangentWS.w = viewDirWS.z;
    #endif
    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        output.viewDirTS = GetViewDirectionTangentSpace(tangentWS, normalInput.normalWS, viewDirWS);
    #endif

    output.texcoord.xy = TRANSFORM_TEX(input.texcoord.xy, _BaseMap);
    #ifdef REQUIRE_UV3
        output.texcoord.zw = input.texcoord3.xy;
    #endif
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2) || defined(_ADDITIONAL_LIGHTS_VERTEX)
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            #else
                output.fogFactorAndVertexLight = half4(0, vertexLight);
            #endif
        #else
            output.fogFactor = fogFactor;
        #endif
    #endif
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    #if defined(LIGHTMAP_ON) || defined(REQUIRE_SH)
        OUTPUT_LIGHTMAP_UV(input.texcoord1.xy, unity_LightmapST, output.staticLightmapUV);
        OUTPUT_SH(normalInput.normalWS, output.vertexSH);
    #endif
    #ifdef DYNAMICLIGHTMAP_ON
        output.dynamicLightmapUV = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif
    #ifdef REQUIRE_VERTEXCOLOR
        output.vertexColor = input.vertexColor;
    #endif


    // #if !defined(META_PASS)
    #if defined(SHADOWCASTER_PASS)
        output.positionCS = GetShadowPositionHClipSupportShadowBias(input.positionOS.xyz, input.normalOS);
    #elif defined(META_PASS)
        output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.texcoord1.xy, input.texcoord2.xy);
    #else
        output.positionCS = vertexInput.positionCS;
    #endif

    // #endif

    return output;
}

//SAMPLE_GI
void frag(Varyings input, bool vFace : SV_IsFrontFace

, out half4 outColor : SV_Target0
#if defined(_WRITE_RENDERING_LAYERS) && !defined(SCENESELECTION_PASS) && !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS)
    , out float4 outRenderingLayers : SV_Target1
#endif

)
{
    #if !defined(META_PASS)
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    #endif
    //为避免干扰,顶点动画和片元着色器代码请在VertexAndFragmentFunction.hlsl文件中的VertexAnimator/FragmentShading函数中书写
    outColor = FragmentShading(input, vFace);
    // outColor = half4(input.normalWS.xyz, 1.0);

    #if defined(_WRITE_RENDERING_LAYERS) && !defined(SCENESELECTION_PASS) && !defined(SHADOWCASTER_PASS) && !defined(DEPTHONLY_PASS) && !defined(META_PASS)
        uint renderingLayers = GetMeshRenderingLayer();
        outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif
}



#endif