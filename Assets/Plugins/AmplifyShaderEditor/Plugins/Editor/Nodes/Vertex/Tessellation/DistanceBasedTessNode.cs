




namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Distance-based Tessellation"
#else
"基于距离的细分"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Calculates tessellation based on distance from camera"
#else
"根据与相机的距离计算细分"
#endif
)]
	public sealed class DistanceBasedTessNode : TessellationParentNode
	{
		private const string FunctionBody = "UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, {0},{1},{2})";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Factor"
#else
"因素"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Min Dist"
#else
"最小距离"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Max Dist"
#else
"最大距离"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT4, Constants.EmptyPortValue );
		}

		protected override string BuildTessellationFunction( ref MasterNodeDataCollector dataCollector )
		{
			return string.Format(	FunctionBody,
									m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector ),
									m_inputPorts[ 2 ].GeneratePortInstructions( ref dataCollector ),
									m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector ) );
		}
	}
}
