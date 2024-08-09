#ifndef SSMS_HLSL
#define SSMS_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 // The Blit.hlsl file provides the vertex shader (Vert),
// input structure (Attributes) and output strucutre (Varyings)
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"


TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_BaseTex);
SAMPLER(sampler_BaseTex);

float2 _MainTex_TexelSize;
float2 _BaseTex_TexelSize;

float _PrefilterOffs;
float _Threshold;
float3 _Curve;
float _SampleScale;
float _Intensity;

// SMSS
TEXTURE2D(_FadeTex);
SAMPLER(sampler_FadeTex);

TEXTURE2D(_FogFactor_RT);
SAMPLER(sampler_FogFactor_RT);

float _Radius;
float _BlurWeight;

#pragma shader_feature ANTI_FLICKER_ON 
#pragma shader_feature _HIGH_QUALITY_ON 

// Brightness function
float Brightness(float3 c)
{
    return max(max(c.r, c.g), c.b);
}

// 3-tap median filter
float3 Median(float3 a, float3 b, float3 c)
{
    return a + b + c - min(min(a, b), c) - max(max(a, b), c);
}

float CustomLuminocity(float3 a)
{
    float sum = a.r + a.b + a.z;
    sum /=3;
    sum = sum * (1 / sum);
	return sum;
}

float4 ToFloat4(float3 rgb)
{
    return float4(rgb, 0);
}

float3 ToFloat3(float4 rgba)
{
    return rgba.rgb;
}

#define HALF_MAX        65504.0 // (2 - 2^-10) * 2^15

// Clamp HDR value within a safe range
half3 SafeHDR(half3 c)
{
    return min(c, HALF_MAX);
}

half4 SafeHDR(half4 c)
{
    return min(c, HALF_MAX);
}

// Downsample with a 4x4 box filter
float3 DownsampleFilter(float2 uv)
{
    float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);

    float3 s;
    s  = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xy));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zy));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xw));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zw));

    return s * (1.0 / 4);
}

// Downsample with a 4x4 box filter + anti-flicker filter
float3 DownsampleAntiFlickerFilter(float2 uv)
{
    float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);

    float3 s1 = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xy));
    float3 s2 = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zy));
    float3 s3 = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xw));
    float3 s4 = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zw));

    // Karis's luma weighted average (using brightness instead of luma)
    float s1w = 1 / (Brightness(s1) + 1);
    float s2w = 1 / (Brightness(s2) + 1);
    float s3w = 1 / (Brightness(s3) + 1);
    float s4w = 1 / (Brightness(s4) + 1);
    float one_div_wsum = 1 / (s1w + s2w + s3w + s4w);

    return (s1 * s1w + s2 * s2w + s3 * s3w + s4 * s4w) * one_div_wsum;
}

float3 UpsampleFilter(float2 uv)
{
#ifdef _HIGH_QUALITY_ON
    // 9-tap bilinear upsampler (tent filter)
    float4 d = _MainTex_TexelSize.xyxy * float4(1, 1, -1, 0) * _SampleScale;

    float3 s;
    s  = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - d.xy));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - d.wy)) * 2;
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - d.zy));
    
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zw)) * 2;
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv       )) * 4;
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xw)) * 2;
    
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zy));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.wy)) * 2;
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xy));

    return s * (1.0 / 16);
#else
    // 4-tap bilinear upsampler
    float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1) * (_SampleScale * 0.5);

    float3 s;
    s  = ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xy));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zy));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xw));
    s += ToFloat3(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zw));

    return s * (1.0 / 4);
#endif
}

//
// fragment shader
//

// SMSS
float AdjustDepth(float4 d){

    //float Out = CustomLuminocity(d.rgb);
    //Out *= d.a;

    float Out = d.r;

	Out = SAMPLE_TEXTURE2D(_FadeTex,sampler_FadeTex, float2(Out, 0.5)).r;
	return saturate(Out);
}

//struct Attributes
//{
//	float4 positionOS : POSITION;
//	float2 uv : TEXCOORD0;
//	UNITY_VERTEX_INPUT_INSTANCE_ID
//};
//
//struct Varyings
//{
//	float4 positionCS : SV_POSITION;
//	//float3 positionVS : TEXCOORD1;
//	float2 uv : TEXCOORD0;
//	UNITY_VERTEX_INPUT_INSTANCE_ID
//    UNITY_VERTEX_OUTPUT_STEREO
//};
//
//Varyings VertDefault(Attributes input)
//{
//	Varyings output = (Varyings)0;
//
//	UNITY_SETUP_INSTANCE_ID(input);
//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//
//	VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
//
//	//output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
//	output.positionCS = positionInputs.positionCS;
//	//output.positionVS = positionInputs.positionVS;
//	output.uv = input.uv;
//
//	return output;
//}

float4 frag_prefilter(Varyings i) : SV_Target
{
	float2 uv = i.texcoord    + _MainTex_TexelSize.xy * _PrefilterOffs;

    #ifdef ANTI_FLICKER_ON
    float3 d = _MainTex_TexelSize.xyx * float3(1, 1, 0);
    float4 s0 = SafeHDR(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv));
    float3 s1 = SafeHDR(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - d.xz).rgb);
    float3 s2 = SafeHDR(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.xz).rgb);
    float3 s3 = SafeHDR(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - d.zy).rgb);
    float3 s4 = SafeHDR(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + d.zy).rgb);
    float3 m = Median(Median(s0.rgb, s1, s2), s3, s4);
    
    #else
    float4 s0 = SafeHDR(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv));
    float3 m = s0.rgb;
    #endif

    // Pixel brightness
    float br = Brightness(m);

    // Under-threshold part: quadratic curve
    float rq = clamp(br - _Curve.x, 0, _Curve.y);
    rq = _Curve.z * rq * rq;

    // Combine and apply the brightness response curve.
    m *= max(rq, br - _Threshold) / max(br, 1e-5); 

    // SMSS 
    float depth = AdjustDepth(SAMPLE_TEXTURE2D(_FogFactor_RT,sampler_FogFactor_RT, i.texcoord   )); 

    return ToFloat4(m * depth);
}


float4 frag_downsample1(Varyings i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

#ifdef ANTI_FLICKER_ON
    return ToFloat4(DownsampleAntiFlickerFilter(i.texcoord   ));
#else
    return ToFloat4(DownsampleFilter(i.texcoord   ));
#endif
}

float4 frag_downsample2(Varyings i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    return ToFloat4(DownsampleFilter(i.texcoord   ));
}

float4 frag_upsample(Varyings i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float3 base = ToFloat3(SAMPLE_TEXTURE2D(_BaseTex,sampler_BaseTex, i.texcoord   ));
    float3 blur = UpsampleFilter(i.texcoord   );

    return ToFloat4(base + blur * (1 + _BlurWeight)) / (1 + (_BlurWeight * 0.735));
}

float4 frag_upsample_final(Varyings i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	float4 base = SAMPLE_TEXTURE2D(_BaseTex,sampler_BaseTex, i.texcoord   );
    float3 blur = UpsampleFilter(i.texcoord   );

    // SMSS
    float depth = AdjustDepth(SAMPLE_TEXTURE2D(_FogFactor_RT,sampler_FogFactor_RT, i.texcoord   )); 

    return lerp(base, float4(blur,1) * (1 / _Radius), clamp(depth ,0,_Intensity));
}


#endif 