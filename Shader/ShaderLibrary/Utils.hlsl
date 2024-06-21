#ifndef ALTOS_UTILS_INCLUDED
#define ALTOS_UTILS_INCLUDED


float4x4 GetMatrixScale(float3 s)
{
	float4x4 m =
	{
		s.x, 0, 0, 0,
		0, s.y, 0, 0,
		0, 0, s.z, 0,
		0, 0, 0, 1
	};
	return m;
}

float4x4 GetMatrixTranslate(float3 t)
{
	float4x4 m =
	{
		1, 0, 0, t.x,
		0, 1, 0, t.y,
		0, 0, 1, t.z,
		0, 0, 0, 1,
	};
	
	return m;
}

float3x3 GetEulerMatrix(float3 angles)
{
	float3 s, c;
	sincos(angles, s, c);

	return float3x3(c.y * c.z + s.x * s.y * s.z, c.z * s.x * s.y - c.y * s.z, c.x * s.y,
        c.x * s.z, c.x * c.z, -s.x,
        -c.z * s.y + c.y * s.x * s.z, c.y * c.z * s.x + s.y * s.z, c.x * c.y);
}

float4x4 GetMatrixRotationZ(float3 r)
{
	float4x4 m =
	{
		cos(r.z), sin(r.z), 0, 0,
		-sin(r.z), cos(r.z), 0, 0,
		0, 0, 1, 0,
		0, 0, 0, 1
	};

	return m;
}

float4x4 GetMatrixRotationX(float3 r)
{
		float4x4 m =
		{
			1, 0, 0, 0,
			0, cos(r.x), -sin(r.x), 0,
			0, sin(r.x), cos(r.x), 0,
			0, 0, 0, 1
		};
		return m;
}

float4x4 GetMatrixRotationY(float3 r)
{
	float4x4 m =
	{
		cos(r.y), 0, sin(r.y), 0,
		0, 1, 0, 0,
		-sin(r.y), 0, cos(r.y), 0,
		0, 0, 0, 1
	};
	
	return m;
}

float4x4 GetMatrixTRS(float3 t, float3 r, float3 s)
{
	float4x4 trs = mul(GetMatrixTranslate(t), mul(GetMatrixRotationY(r), mul(GetMatrixRotationX(r), mul(GetMatrixRotationZ(r), GetMatrixScale(s)))));
	return trs;
}

float3 RotateAroundAxis(float3 In, float3 Axis, float Rotation)
{
	Rotation = radians(Rotation);
	float s = sin(Rotation);
	float c = cos(Rotation);
	float one_minus_c = 1.0 - c;

	Axis = normalize(Axis);
	float3x3 rot_mat =
	{
		one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
	};

	return mul(rot_mat, In);
}

float3 LookAtDirection(float3 In, float3 Direction)
{
	float3 fAxis = float3(0, 0, 1);
	float3 axis = cross(fAxis, normalize(Direction));
	float rotation = acos(dot(fAxis, normalize(Direction)));
	return RotateAroundAxis(In, axis, rotation);
}

#endif
