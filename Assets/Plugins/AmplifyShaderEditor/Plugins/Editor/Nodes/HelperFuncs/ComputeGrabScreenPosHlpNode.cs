


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Compute Grab Screen Pos"
#else
"计算抓取屏幕位置"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Computes texture coordinate for doing a screenspace-mapped texture sample. Input is clip space position"
#else
"计算纹理坐标以进行屏幕空间映射纹理采样。输入是剪辑空间位置"
#endif
)]
	public sealed class ComputeGrabScreenPosHlpNode : HelperParentNode
	{
		private readonly string[] ComputeGrabScreenPosFunction =
		{
			"inline float4 ComputeGrabScreenPos( float4 pos )\n",
			"{\n",
			"#if UNITY_UV_STARTS_AT_TOP\n",
			"\tfloat scale = -1.0;\n",
			"#else\n",
			"\tfloat scale = 1.0;\n",
			"#endif\n",
			"\tfloat4 o = pos * 0.5f;\n",
			"\to.xy = float2( o.x, o.y*scale ) + o.w;\n",
			"#ifdef UNITY_SINGLE_PASS_STEREO\n",
			"\to.xy = TransformStereoScreenSpaceTex ( o.xy, pos.w );\n",
			"#endif\n",
			"\to.zw = pos.zw;\n",
			"\treturn o;\n",
			"}\n"
		};

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ComputeGrabScreenPos";
			m_inputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT4, false );
			m_outputPorts[ 0 ].Name = "XYZW";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "computeGrabScreenPos" + OutputId;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.IsTemplate && dataCollector.TemplateDataCollectorInstance.CurrentSRPType == TemplateSRPType.HDRP )
			{
				dataCollector.AddFunction( m_funcType, ComputeGrabScreenPosFunction, false );
			}
			return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
		}
	}
}
