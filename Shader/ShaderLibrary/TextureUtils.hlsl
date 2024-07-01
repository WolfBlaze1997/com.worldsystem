#ifndef ALTOS_TEX_UTILS_INCLUDED
#define ALTOS_TEX_UTILS_INCLUDED

#pragma warning( push )
#pragma warning (disable : 3568 ) 
#pragma target 4.5
#pragma warning ( pop )

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Math.hlsl"

float _CLOUD_RENDER_SCALE;

// Texture2D _Halton_23_Sequence;
float4 _Halton_23_Sequence_TexelSize;
SamplerState altos_linear_clamp_sampler;
SamplerState altos_linear_repeat_sampler;
SamplerState altos_point_repeat_sampler;
SamplerState altos_point_clamp_sampler;
uint _FrameId;
// Texture2D _DitheredDepthTexture;
float4 _RenderTextureDimensions; // x = 1/width, y = 1/height, z = width, w = height
float4 _RenderScale; // x = scale, y = rcp(scale)


float4x4 unity_CameraInvProjection_fov;

void GetWSRayDirectionFromUV(float2 uv, out float3 rayDirection, out float viewLength)
{
	// float3 viewVector = mul(unity_CameraInvProjection, float4(uv * 2 - 1, 0.0, -1)).xyz;
	// float3 viewVector = mul(unity_CameraInvProjection_fov, float4(uv * 2 - 1, 0.0, -1)).xyz;
	float3 viewVector = mul(UNITY_MATRIX_I_P, float4(uv * 2 - 1, 0.0, -1)).xyz;
	viewVector.y = -viewVector.y;

	viewVector = mul(unity_CameraToWorld, float4(viewVector, 0.0)).xyz;
	viewLength = length(viewVector);
	rayDirection = viewVector / viewLength;
}

bool IsUVInRange01(float2 UV)
{
	if (UV.x <= 0.0 || UV.x >= 1.0 || UV.y <= 0.0 || UV.y >= 1.0)
	{
		return false;
	}
	return true;
}

float2 GetTexCoordSize(float renderTextureScale)
{
	return rcp(_ScreenParams.xy * renderTextureScale);
}

int GetPixelIndex(float2 uv, int2 size)
{
	return uv.y * size.y * size.x + uv.x * size.x;
}

int GetPixelIndexConstrained(float2 uv, int2 size, int2 domain)
{
	int indY = (uv.y * size.y) % domain.y;
	int indX = (uv.x * size.x) % domain.x;
	int ind = indY * domain.x + indX;
	return ind;
}

// float2 GetHaltonFromTexture(int index)
// {
// 	return _Halton_23_Sequence.Load(int3(index % _Halton_23_Sequence_TexelSize.z, 0, 0)).rg;
// }

float Halton(int base, int index)
{
	float result = 0.0;
	float f = 1.0;
	while (index > 0)
	{
		f = f / float(base);
		result += f * float(index % base);
		index = index / base;
	}
	return result;
}

float2 Halton23(int index)
{
	return float2(Halton(2, index), Halton(3, index));
}


float3 ScreenToViewVector(float2 UV)
{
    float3 viewDirectionTemp = mul(unity_CameraInvProjection, float4(UV * 2 - 1, 0.0, -1)).xyz;
    return mul(unity_CameraToWorld, float4(viewDirectionTemp, 0)).xyz;
}

void ScreenToViewVector_float(float2 UV, out float3 viewVector)
{
#ifdef SHADERGRAPH_PREVIEW
	viewVector = float3(0.0, 0.0, 0.0);
#endif
	
    viewVector = ScreenToViewVector(UV);
}

float4 ObjectToClipPos(float3 pos)
{
	return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos, 1)));
}



int _USE_DITHERED_DEPTH;

float DitherDepth(float2 UV)
{
    // When downscaling, we want to use the SOURCE texel size
    float2 texCoord = GetTexCoordSize(1.0);
    
    // When downscaling, we want to use the texels down and to the left, since this is how Unity downscales itself.
    float3 offset = float3(-texCoord.x, -texCoord.y, 0.0);
    
    float depth00 = SampleSceneDepth(UV);
    float depth10 = SampleSceneDepth(UV + offset.xz);
    float depth01 = SampleSceneDepth(UV + offset.zy);
    float depth11 = SampleSceneDepth(UV + offset.xy);
    
    float x = floor(UV.x * _ScreenParams.x * 0.5) + floor(UV.y * _ScreenParams.y * 0.5);
    int checkerboard = frac(x * 0.5) * 2.0;
    
    //return min(depth00, min(depth10, min(depth01, depth11)));
    //return max(depth00, max(depth10, max(depth01, depth11)));
    
    //
    if(checkerboard == 0)
        return min(depth00, min(depth10, min(depth01, depth11)));
    
    return max(depth00, max(depth10, max(depth01, depth11)));
}


// Borrowed from the kind soul who runs VertexFragment, Steven Sell. 
// https://www.vertexfragment.com/ramblings/unity-postprocessing-sobel-outline/
// You have my eternal thanks!
// Also, I appreciate the clever use of swizzling in the offset.
float SobelDepth(float ldc, float ldl, float ldr, float ldu, float ldd)
{
    return abs(ldl - ldc) + abs(ldr - ldc) + abs(ldu - ldc) + abs(ldd - ldc);
}

float SobelSampleDepth(float2 UV, float3 offset)
{
    float pixelCenter = Linear01Depth(SampleSceneDepth(UV), _ZBufferParams);
    float pixelLeft   = Linear01Depth(SampleSceneDepth(UV - offset.xz), _ZBufferParams);
    float pixelRight  = Linear01Depth(SampleSceneDepth(UV + offset.xz), _ZBufferParams);
    float pixelDown   = Linear01Depth(SampleSceneDepth(UV - offset.zy), _ZBufferParams);
    float pixelUp     = Linear01Depth(SampleSceneDepth(UV + offset.zy), _ZBufferParams);

    return SobelDepth(pixelCenter, pixelLeft, pixelRight, pixelUp, pixelDown);
}

float FindEdges(float2 UV)
{
    float _OutlineThickness = 4.0;
    float3 offset = float3(_ScreenParams.zw - 1.0, 0.0) * _OutlineThickness;
    float sobelDepth = SobelSampleDepth(UV, offset);

    return sobelDepth;
}

#endif
