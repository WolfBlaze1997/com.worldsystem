#ifndef RGI_TACUM
#define RGI_TACUM

	// Copyright 2022 Kronnect - All Rights Reserved.
    TEXTURE2D_X(_PrevResolve);
    #define TEMPORAL_CHROMA_THRESHOLD 0.25
    half _TemporalResponseSpeed;
    #define TEMPORAL_RESPONSE_SPEED _TemporalResponseSpeed

    TEXTURE2D_X(_MainTex);
    float4 _MainTex_ST;
    float4 _MainTex_TexelSize;

	TEXTURE2D_X(_MotionVectorTexture);

	struct AttributesFS {
		float4 positionHCS : POSITION;
		float2 uv          : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

 	struct VaryingsSSR {
    	float4 positionCS : SV_POSITION;
    	float2 uv  : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
	};




	VaryingsSSR VertSSR(AttributesFS input) {
	    VaryingsSSR output;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_TRANSFER_INSTANCE_ID(input, output);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = float4(input.positionHCS.xyz, 1.0);

		#if UNITY_UV_STARTS_AT_TOP
		    output.positionCS.y *= -1;
		#endif

        output.uv = input.uv;
    	return output;
	}

    half2 GetVelocity(float2 uv) {
		half2 mv = SAMPLE_TEXTURE2D_X_LOD(_MotionVectorTexture, sampler_PointClamp, uv, 0).xy;
        return mv;
    }

	half4 FragAcum (VaryingsSSR i) : SV_Target { 

        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

        half4 newData = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, uv);

        half2 velocity = GetVelocity(uv);
        float2 prevUV = uv - velocity;

        if (any(floor(prevUV))!=0) {
            return newData;
        }

        half4 prevData = SAMPLE_TEXTURE2D_X(_PrevResolve, sampler_PointClamp, prevUV);

        half4 newDataN = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, uv + float2(0, 1) * _MainTex_TexelSize.xy);
        half4 newDataS = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, uv + float2(0, -1) * _MainTex_TexelSize.xy);
        half4 newDataW = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, uv + float2(-1, 0) * _MainTex_TexelSize.xy);
        half4 newDataE = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, uv + float2(1, 0) * _MainTex_TexelSize.xy);

        half4 newDataMin = min( newData, min( min(newDataN, newDataS), min(newDataW, newDataE) ));
        half4 newDataMax = max( newData, max( max(newDataN, newDataS), max(newDataW, newDataE) ));

        half4 newDataMinExt = newDataMin * (1 - TEMPORAL_CHROMA_THRESHOLD);
        half4 newDataMaxExt = newDataMax * (1 + TEMPORAL_CHROMA_THRESHOLD);
        
        // reduce noise by clamping history to present by certain threshold
        prevData = clamp(prevData, min(newDataMinExt, newDataMaxExt), max(newDataMinExt, newDataMaxExt));

        half delta = unity_DeltaTime.z * TEMPORAL_RESPONSE_SPEED;

        half4 res = lerp(prevData, newData, saturate(delta));
        return res;

	}



#endif // RGI