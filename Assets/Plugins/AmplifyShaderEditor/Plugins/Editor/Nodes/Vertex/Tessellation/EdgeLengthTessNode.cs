


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Edge Length Tessellation"
#else
"边缘长度镶嵌"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Tessellation level computed based on triangle edge length on the screen"
#else
"基于屏幕上三角形边长计算的细分级别"
#endif
)]
	public sealed class EdgeLengthTessNode : TessellationParentNode
	{
		private const string FunctionBody = "UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, {0})";
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Edge Length"
#else
"边缘长度"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT4, Constants.EmptyPortValue );
		}

		protected override string BuildTessellationFunction( ref MasterNodeDataCollector dataCollector )
		{
			return string.Format( FunctionBody, m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector ) );
		}
	}
}
