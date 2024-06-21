#ifndef ALTOS_MATH_INCLUDED
#define ALTOS_MATH_INCLUDED

static float _INF = 1e20;
static float _INV_INF = 1.0/1e20;

static float4 _LinearFalloff = float4(0.53, 0.27, 0.13, 0.07);
static float4 _SqrtFalloff = float4(0.39, 0.28, 0.20, 0.14);
static float4 _Pow2Falloff = float4(0.75, 0.19, 0.05, 0.01);
static float4 _ExpFalloff = float4(0.40, 0.24, 0.19, 0.17);


float CalculateHorizon(float3 position, float3 direction, float planetRadius)
{
	float h = max(position.y, 0);
	float r = planetRadius;
	float a = r + h;
	float b = r / a;
	float c = acos(b);
	float angle = direction.y * 1.571;
	float d = angle - c;
	
	return smoothstep(-radians(_INV_INF), radians(_INV_INF), d);
}


float os_InverseLerp(float a, float b, float v)
{
	return (v - a) / (b - a);
}

float os_RemapUnclamped(float iMin, float iMax, float oMin, float oMax, float v)
{
    float t = os_InverseLerp(iMin, iMax, v);
	return lerp(oMin, oMax, t);
}

float os_Remap(float iMin, float iMax, float oMin, float oMax, float v)
{
	v = clamp(v, iMin, iMax);
    return os_RemapUnclamped(iMin, iMax, oMin, oMax, v);
}

float os_Map01(float iMin, float iMax, float v)
{
    return saturate(os_Remap(iMin, iMax, 0.0, 1.0, v));
}

float EaseIn(float a)
{
	return a * a;
}

float EaseOut(float a)
{
	return 1 - EaseIn(1 - a);
}

float EaseInOut(float a)
{
	return lerp(EaseIn(a), EaseOut(a), a);
}


float rand3dTo1d(float3 vec, float3 dotDir = float3(12.9898, 78.233, 154.681))
{
	float random = dot(sin(vec.xyz), dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float2 rand3dto2d(float3 vec, float3 seed = 4141)
{
	return float2(
		rand3dTo1d(vec + seed),
		rand3dTo1d(vec + seed, float3(67.416, 44.529, 46.749))
	);
}

float rand2dTo1d(float2 vec, float2 dotDir = float2(12.9898, 78.233))
{
	float random = dot(sin(vec.xy), dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float2 rand2dTo2d(float2 vec, float seed = 4605)
{
	return float2(
		rand2dTo1d(vec + seed.xx),
		rand2dTo1d(vec + seed.xx, float2(39.346, 11.135))
	);
}


float2 GetDir(float x, float y, float seed = 4605)
{
	return rand2dTo2d(float2(x, y), seed) * 2.0 - 1.0;
}

float GetPerlinNoise(float2 position, float seed = 4605)
{
	float2 lowerLeft = GetDir(floor(position.x), floor(position.y), seed);
	float2 lowerRight = GetDir(ceil(position.x), floor(position.y), seed);
	float2 upperLeft = GetDir(floor(position.x), ceil(position.y), seed);
	float2 upperRight = GetDir(ceil(position.x), ceil(position.y), seed);
	
	float2 f = frac(position);
	
	lowerLeft = dot(lowerLeft, f);
	lowerRight = dot(lowerRight, f - float2(1.0, 0.0));
	upperLeft = dot(upperLeft, f - float2(0.0, 1.0));
	upperRight = dot(upperRight, f - float2(1.0, 1.0));
	
	float2 t = float2(EaseInOut(f.x), EaseInOut(f.y));
	float lowerMix = lerp(lowerLeft.x, lowerRight.x, t.x);
	float upperMix = lerp(upperLeft.x, upperRight.x, t.x);
	return lerp(lowerMix, upperMix, t.y);
}

float GetLayeredPerlinNoise(int octaves, float2 position, float gain, float lacunarity, float seed = 4605)
{
	float value = 0.0;
	float amp = 1.0;
	float frequency = 1.0;
	float c = 0.0;
	
	for (int i = 1; i <= octaves; i++)
	{
		value += GetPerlinNoise(position * frequency, seed) * amp;
		c += amp;
		amp *= gain;
		frequency *= lacunarity;
	}
	value /= c;
	return saturate(value + 0.5);
}


void GradientPerlinNoise_float(float2 position, out float value)
{
	value = GetPerlinNoise(position);
}

#endif
