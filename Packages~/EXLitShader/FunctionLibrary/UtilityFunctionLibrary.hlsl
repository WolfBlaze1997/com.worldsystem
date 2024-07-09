#ifndef DEBUG_FUNCTION
#define DEBUG_FUNCTION
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
#include "../FunctionLibrary/UnityTextureLibrary.hlsl"

// Decodes HDR textures
// handles dLDR, RGBM formats
inline half3 DecodeHDR(half4 data, half4 decodeInstructions, int colorspaceIsGamma)
{
    // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
    half alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

    // If Linear mode is not supported we can skip exponent part
    if (colorspaceIsGamma)
        return (decodeInstructions.x * alpha) * data.rgb;

    return (decodeInstructions.x * pow(alpha, decodeInstructions.y)) * data.rgb;
}
// Decodes HDR textures
// handles dLDR, RGBM formats
inline half3 DecodeHDR(half4 data, half4 decodeInstructions)
{
    #if defined(UNITY_COLORSPACE_GAMMA)
        return DecodeHDR(data, decodeInstructions, 1);
    #else
        return DecodeHDR(data, decodeInstructions, 0);
    #endif
}
real Pow2(real x)
{
    return x * x;
}
real4 ChessboardGrid_Debug(real3 computePosWS, real deviceDepth, uint scale)
{
    // 以下部分创建棋盘效果。
    // 比例是平方反比。
    // 缩放、镜像和捕捉坐标。
    uint3 worldIntPos = uint3(abs(computePosWS.xyz * scale));
    // 将表面划分为正方形。计算颜色 ID 值。
    bool white = ((worldIntPos.x) & 1) ^(worldIntPos.y & 1) ^(worldIntPos.z & 1);
    // 根据 ID 值（黑色或白色）为正方形着色。
    half4 color = white ? half4(1, 1, 1, 1) : half4(0, 0, 0, 1);
    // 在远裁剪面附近将颜色设置为
    // 黑色。
    return color;
    #if UNITY_REVERSED_Z
        // 具有 REVERSED_Z 的平台（如 D3D）的情况。
        if (deviceDepth < 0.0001)
            return half4(0, 0, 0, 1);
    #else
        // 没有 REVERSED_Z 的平台（如 OpenGL）的情况。
        if (deviceDepth > 0.9999)
            return half4(0, 0, 0, 1);
    #endif
}

//------------------------------------------------------------------------------------
//SNoise
float3 mod2D289(float3 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float2 mod2D289(float2 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float3 permute(float3 x)
{
    return mod2D289(((x * 34.0) + 1.0) * x);
}
float SNoiseGenerate(float2 v, float scale)
{
    v *= scale;
    const float4 C = float4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);
    float2 i1;
    i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod2D289(i);
    float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;
    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;
    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}


//高性能 GradientNoise
//https://www.shadertoy.com/view/XdXGW8
float2 GradientNoiseDir(float2 x)
{
    const float2 k = float2(0.3183099, 0.3678794);
    x = x * k + k.yx;
    return -1.0 + 2.0 * frac(16.0 * k * frac(x.x * x.y * (x.x + x.y)));
}
float GradientNoiseGenerate(float2 UV, float Scale)
{
    float2 p = UV * Scale;
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(dot(GradientNoiseDir(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
    dot(GradientNoiseDir(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
    lerp(dot(GradientNoiseDir(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
    dot(GradientNoiseDir(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}


//SimpleNoise
inline float noise_randomValue(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}
inline float noise_interpolate(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}
inline float valueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);
    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = noise_randomValue(c0);
    float r1 = noise_randomValue(c1);
    float r2 = noise_randomValue(c2);
    float r3 = noise_randomValue(c3);
    float bottomOfGrid = noise_interpolate(r0, r1, f.x);
    float topOfGrid = noise_interpolate(r2, r3, f.x);
    float t = noise_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}
float SimpleNoise(float2 UV)
{
    float t = 0.0;
    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += valueNoise(UV / freq) * amp;
    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += valueNoise(UV / freq) * amp;
    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += valueNoise(UV / freq) * amp;
    return t;
}



//用于给灰度添加噪点
half NoiseAppend(half dst, half2 range = half2(-0.5, 0.5))
{
    half temp_output_1_0_g3 = dst;
    half sinIn7_g3 = sin(temp_output_1_0_g3);
    half sinInOffset6_g3 = sin((temp_output_1_0_g3 + 1.0));
    half lerpResult20_g3 = lerp(range.x, range.y, frac((sin(((sinIn7_g3 - sinInOffset6_g3) * 91.2228)) * 43758.55)));
    half temp_output_3_0 = (lerpResult20_g3 + sinIn7_g3);
    return temp_output_3_0;
}
//沃罗诺伊噪点,来自ASE
half2 voronoihash4(half2 p)
{
    p = half2(dot(p, half2(127.1, 311.7)), dot(p, half2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}
half Voronoi_half(half2 coord, half scale, half angle_time, inout half2 id, inout half2 mr)
{
    coord *= scale;
    half2 n = floor(coord);
    half2 f = frac(coord);
    half F1 = 8.0;
    half F2 = 8.0; half2 mg = 0;
    for (int j = -1; j <= 1; j++)
    {
        for (int i = -1; i <= 1; i++)
        {
            half2 g = half2(i, j);
            half2 o = voronoihash4(n + g);
            o = (sin(angle_time + o * 6.2831) * 0.5 + 0.5); half2 r = f - g - o;
            half d = 0.5 * dot(r, r);
            if (d < F1)
            {
                F2 = F1;
                F1 = d; mg = g; mr = r; id = o;
            }
            else if (d < F2)
            {
                F2 = d;
            }
        }
    }
    return F1;
}

uint2 ComputeFadeMaskSeed(float3 V, uint2 positionSS)
{
    uint2 fadeMaskSeed;

    // Is this a reasonable quality gate?
    #if defined(SHADER_QUALITY_HIGH)
        if (IsPerspectiveProjection())
        {
            // Start with the world-space direction V. It is independent from the orientation of the camera,
            // and only depends on the position of the camera and the position of the fragment.
            // Now, project and transform it into [-1, 1].
            float2 pv = PackNormalOctQuadEncode(V);
            // Rescale it to account for the resolution of the screen.
            pv *= _ScreenParams.xy;
            // The camera only sees a small portion of the sphere, limited by hFoV and vFoV.
            // Therefore, we must rescale again (before quantization), roughly, by 1/tan(FoV/2).
            pv *= UNITY_MATRIX_P._m00_m11;
            // Truncate and quantize.
            fadeMaskSeed = asuint((int2)pv);
        }
        else
    #endif
    {
        // Can't use the view direction, it is the same across the entire screen.
        fadeMaskSeed = positionSS;
    }

    return fadeMaskSeed;
}

//----------------------------------------------------------------------------------
//LOD公告牌
void BillboardLod(float texcoordZ, inout half3 normalOS, inout half4 tangentOS, inout float3 positionOS, out half viewDot)
{
    float3 treePos = float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
    #if defined(EFFECT_BILLBOARD)
        // crossfade faces
        bool topDown = (texcoordZ > 0.5);
        float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
        float3 cameraDir = normalize(mul((float3x3)UNITY_MATRIX_M, _WorldSpaceCameraPos - treePos));
        viewDot = max(dot(viewDir, normalOS), dot(cameraDir, normalOS));
        viewDot *= viewDot;
        viewDot *= viewDot;
        viewDot += topDown ? 0.38 : 0.18; // different scales for horz and vert billboards to fix transition zone

        // if invisible, avoid overdraw
        if (viewDot < 0.3333)
        {
            positionOS.xyz = float3(0, 0, 0);
        }
        viewDot = clamp(viewDot, 0, 1);
        // adjust lighting on billboards to prevent seams between the different faces
        if (topDown)
        {
            normalOS += cameraDir;
        }
        else
        {
            half3 binormal = cross(normalOS, tangentOS.xyz) * tangentOS.w;
            float3 right = cross(cameraDir, binormal);
            normalOS = cross(binormal, right);
        }
        normalOS = normalize(normalOS);
    #endif
}

//huodini的VAT(顶点动画贴图)流程
void HuodiniVAT_Soft(
    //input
    float2 VAT_UV,
    TEXTURE2D(positionTex), SAMPLER(sampler_positionTex),
    TEXTURE2D(rotateTex), SAMPLER(sampler_rotateTex), bool isPosTexHDR,
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

    float4 positionColor = SAMPLE_TEXTURE2D_LOD(positionTex, sampler_positionTex, frameUV, 0);
    if (isPosTexHDR)
    {
        positionOS += positionColor * strength;
    }
    else
    {
        positionOS += ((BoundMax - BoundMin) * positionColor.rgb + BoundMin) * strength;
    }

    half4 rotateColor = SAMPLE_TEXTURE2D_LOD(rotateTex, sampler_rotateTex, frameUV, 0);
    rotateColor = (rotateColor - 0.5) * 2;

    half3 b = rotateColor.a * half3(0.0, 1.0, 0.0) + cross(rotateColor.rgb, half3(0.0, 1.0, 0.0));
    normalOS = normalize(cross(rotateColor.rgb, b) * 2 + half3(0.0, 1.0, 0.0));

    half3 c = rotateColor.a * half3(-1.0, 0.0, 0.0) + cross(rotateColor.rgb, half3(-1.0, 0.0, 0.0));
    tangentOS = normalize(cross(rotateColor.rgb, c) * 2 + half3(-1.0, 0.0, 0.0));
}




#endif