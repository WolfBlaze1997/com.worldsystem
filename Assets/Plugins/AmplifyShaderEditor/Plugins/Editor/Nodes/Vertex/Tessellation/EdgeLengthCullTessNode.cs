



namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Edge Length Tessellation With Cull"
#else
"带Cull的边长镶嵌"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Tessellation level computed based on triangle edge length on the screen with patch frustum culling"
#else
"基于屏幕上三角形边长的拼接水平计算，并进行平截头体裁剪"
#endif
)]
	public sealed class EdgeLengthCullTessNode : TessellationParentNode
	{
		private const string FunctionBody = "UnityEdgeLengthBasedTessCull( v0.vertex, v1.vertex, v2.vertex, {0},{1})";
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
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Max Disp."
#else
"最大配置。"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT4, Constants.EmptyPortValue );
		}
		
		protected override string BuildTessellationFunction( ref MasterNodeDataCollector dataCollector )
		{
			return string.Format(	FunctionBody,
									m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector ),
									m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector ) );
		}
	}
}
