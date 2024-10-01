


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Parallax Offset"
#else
"视差偏移"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"UV Coordinates"
#else
"UV坐标"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Calculates UV offset for parallax normal mapping"
#else
"计算视差法线贴图的UV偏移"
#endif
)]
	public sealed class ParallaxOffsetHlpNode : HelperParentNode
	{
		public readonly string[] ParallaxOffsetFunc = 
		{
			"inline float2 ParallaxOffset( half h, half height, half3 viewDir )\n",
			"{\n",
			"\th = h * height - height/2.0;\n",
			"\tfloat3 v = normalize( viewDir );\n",
			"\tv.z += 0.42;\n",
			"\treturn h* (v.xy / v.z);\n",
			"}\n"
		};

		void OnSRPActionEvent( int outputId, ref MasterNodeDataCollector dataCollector )
		{
			dataCollector.AddFunction( ParallaxOffsetFunc[ 0 ], ParallaxOffsetFunc, false );
		}

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ParallaxOffset";
			m_inputPorts[ 0 ].ChangeProperties( "H", WirePortDataType.FLOAT, false );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Height"
#else
"身高"
#endif
);
			AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"ViewDir (tan)"
#else
"ViewDir（棕褐色）"
#endif
);
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT2, false );
			m_outputPorts[ 0 ].Name = "Out";
			OnHDAction = OnSRPActionEvent;
			OnLightweightAction = OnSRPActionEvent;
			m_previewShaderGUID = "6085f804c6fbf354eac039c11feaa7cc";
		}

		protected override void OnUniqueIDAssigned()
		{
			base.OnUniqueIDAssigned();
			m_localVarName = "paralaxOffset" + OutputId;
		}
	}
}
