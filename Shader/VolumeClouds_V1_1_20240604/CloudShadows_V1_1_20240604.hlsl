#ifndef ALTOS_CLOUD_SHADOWS_INCLUDED
#define ALTOS_CLOUD_SHADOWS_INCLUDED

#include "../ShaderLibrary/Math.hlsl"
#define VISUALIZE_CASCADES 0

Texture2D _CloudShadowmap;

float4 _ShadowmapResolution;
float4x4 _CloudShadow_WorldToShadowMatrix[4];
float3 _ShadowCasterCameraPosition;
float4 _CloudShadowOrthoParams;
float _CloudShadowDistance;
float _ShadowRadius;
float _ShadowMapCascades[4];
float3 _ShadowCameraCenter;


SamplerState linear_clamp_sampler;
SamplerState point_clamp_sampler;

float GetOpticalDepth(float3 data, float depthEye)
{
	return min(data.b, data.g * max(0, depthEye - data.r));
}

float GetTransmittance(float v)
{
	return exp(-v);
}

float GetLSFalloff(float2 positionLS)
{
    return 0;
	float r = length(float2(0.5, 0.5) - positionLS.xy) * 2.0;
	return os_Remap(0.99, 1.0, 0.0, 1.0, saturate(r));
}

float GetWSFalloff(float3 positionWS)
{
    float d = length(positionWS - float3(0,0,0).xyz);
    float r = os_Remap(_CloudShadowDistance - _CloudShadowDistance * 0.01, _CloudShadowDistance, 0, 1, d);
    return r;
}

// This method gets the optical depth from the the cloud shadow texture.
// Then, we return the transmittance from the filtered optical depth.
float3 GetCloudShadowAttenuation(float3 positionWS)
{
    int cascadeIndex = 0;
    float3 positionLS = mul(_CloudShadow_WorldToShadowMatrix[cascadeIndex], float4(positionWS, 1.0)).xyz;
    
    #if VISUALIZE_CASCADES
    const float3 _CascadeVisualization[4] = {float3(1, 0, 0), float3(0, 1, 0), float3(0, 0, 1), float3(0, 1, 1)};
    return _CascadeVisualization[cascadeIndex];
    #endif
    
	float3 shadowData = float3(0, 0, 0);
	
    #define SAMPLE_COUNT 16.0
	#define INV_SAMPLE_COUNT 1.0 / SAMPLE_COUNT
	
	for(float y = -1.5; y <= 1.5; y += 1.0)
	{
		for(float x = -1.5; x <= 1.5; x += 1.0)
		{
            float2 uv = positionLS.xy + _ShadowmapResolution.zw * float2(x, y) * 1.0;
			shadowData += _CloudShadowmap.SampleLevel(linear_clamp_sampler, uv, 0).rgb;
		}
	}
    shadowData *= INV_SAMPLE_COUNT;
    
	
	float depthEyeLS = positionLS.z * _CloudShadowOrthoParams.z;
    float opticalDepth = GetOpticalDepth(shadowData, depthEyeLS);
    float transmittance = GetTransmittance(opticalDepth);
    transmittance = lerp(transmittance, 1.0, GetWSFalloff(positionWS));
    return transmittance;
}

void GetCloudShadowAttenuation_float(float3 positionWS, out float attenuation)
{
	attenuation = 1.0;
	#ifndef SHADERGRAPH_PREVIEW
		attenuation = GetCloudShadowAttenuation(positionWS).x;
	#endif
}

#endif
