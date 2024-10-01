#ifndef OS_LSPP_PASS_INCLUDED
#define OS_LSPP_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float GetRandomfloat(float2 Seed)
{
	return saturate(frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453));
}

SamplerState linear_clamp_sampler;
int _LightingScatter_FalloffDirective;
float _LightingScatter_OccOverDistanceAmount;


#if defined(USING_STEREO_MATRICES)
	#define FLIP -1
#else
	#define FLIP 1
#endif


float3 EstimateLightScattering( TEXTURE2D_X(OccluderTex), float2 UV, float FogDensity, bool SoftenScreenEdges, bool AnimateNoise, float MaxRayDistanceScreen, int NumberOfSamples, float3 SunColorTint, float FalloffIntensity)
{
	float3 Color = float3(0, 0, 0);
	
	// Early Exit
	if (FogDensity <= 1e-5)
		return Color;
	
	// Get light
	float3 mainLightDirection, mainLightColor;

	Light mainLight = GetMainLight();
	mainLightDirection = mainLight.direction;
	mainLightColor = mainLight.color * SunColorTint;

	UV = UnityStereoTransformScreenSpaceTex(UV);
	
	// Adjust simulated fog density by vector alignment
	float falloff = 1.0;
	float3 viewVector;
	[branch]
	if (_LightingScatter_FalloffDirective == 0 && unity_OrthoParams.w == 0.0)
	{
		viewVector = mul(unity_CameraInvProjection, float4(UV * 2 - 1, 0.0, -1 * FLIP)).xyz;
		viewVector = mul(unity_CameraToWorld, float4(viewVector, 0.0)).xyz;
		falloff = saturate(dot(mainLightDirection, normalize(viewVector)));
	}
	else
	{
		viewVector = unity_CameraToWorld._m02_m12_m22 * FLIP;
		falloff = saturate(dot(mainLightDirection, normalize(viewVector)));
	}
	

	falloff = pow(falloff, FalloffIntensity);
	if(unity_OrthoParams.w == 1.0){
		falloff = 1.0;
	}
	FogDensity *= falloff;
	
	
	// Early Exit
	if(FogDensity <= 0.001)
		return Color;
	
	// Find light position in screen space

	float3 lightDirWS = mul(unity_WorldToCamera, float4(mainLightDirection, 0.0)).xyz;
	float4 lightPosCS = mul(unity_CameraProjection, float4(lightDirWS, 1.0));
	float2 lightPosUV= -lightPosCS.xy / lightPosCS.w;
	lightPosUV *= FLIP;
	
	
	// Find light direction
	float2 UVRemap = (UV * 2.0) - 1.0;

	float2 directionToLight = lightPosUV - UVRemap;
	if(unity_OrthoParams.w == 1.0){
		float4 lightDirCS = mul(unity_CameraProjection, float4(lightDirWS, 0.0));
		lightPosUV = normalize(lightDirCS.xy);
		directionToLight = lightPosUV;
	}

	
	// Get Noise
	float seedOffset = AnimateNoise ? _Time.x : 0;
	float r = GetRandomfloat(UV + seedOffset);

	// Determine Step Length
	float invSampleCount = 1.0 / float(NumberOfSamples);
	float stepLength = MaxRayDistanceScreen * invSampleCount;
	float2 offset = r * directionToLight * stepLength;
	
	
	// Setup for loop
	float alpha = 1.0;
	float invMaxDistance = 1.0 / MaxRayDistanceScreen;
	float3 illuminationFactor = FogDensity * mainLightColor * stepLength * invMaxDistance;
	
	for (int i = 0; i < NumberOfSamples; i++)
	{
		float2 samplePosition = UV + (directionToLight * stepLength * i) + offset;
		
		float decayFactor = exp(-FogDensity * stepLength * i * invMaxDistance);
		
		// Assume no occlusion if outside bounds
		// float occlusion = _OcclusionAssumption;
		float occlusion = 0;
		if (all(samplePosition > 0) && all(samplePosition < 1))
		{
			occlusion = SAMPLE_TEXTURE2D_X_LOD(OccluderTex, linear_clamp_sampler, samplePosition, 0).r;
		}
		
		// Blend into corners
		if (SoftenScreenEdges)
		{
			float2 adj = smoothstep(0.9, 1.0, abs((samplePosition * 2.0) - 1.0));
			occlusion = lerp(occlusion, 1.0, max(adj.x, adj.y));
		}
		
		float occlusionFalloff = float(i) * invSampleCount;
		occlusion = lerp(occlusion, 1.0, occlusionFalloff * _LightingScatter_OccOverDistanceAmount); // Assume that this sample is less likely to occlude this pixel as the sample distance increases
		
		// Evaluate Color Contribution and Transmittance 
		Color += illuminationFactor * alpha * occlusion;
		alpha *= decayFactor;
	}

	
	return Color;
}
#endif