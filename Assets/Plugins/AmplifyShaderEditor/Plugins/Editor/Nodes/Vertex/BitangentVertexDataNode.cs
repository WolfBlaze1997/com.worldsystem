


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Vertex Bitangent"
#else
"顶点位变化"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vertex Data"
#else
"顶点数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Calculated bitangent vector in object space, can be used in both local vertex offset and fragment outputs. Already has tangent sign and object transform into account"
#else
"在对象空间中计算出的位移向量，可用于局部顶点偏移和片段输出。已经考虑了切线符号和对象变换"
#endif
)]
	public sealed class BitangentVertexDataNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputVectorPorts( WirePortDataType.FLOAT3, "XYZ" );
			m_drawPreviewAsSphere = true;
			m_previewShaderGUID = "76873532ab67d2947beaf07151383cbe";
		}

		public override void PropagateNodeData( NodeData nodeData, ref MasterNodeDataCollector dataCollector )
		{
			base.PropagateNodeData( nodeData, ref dataCollector );
			dataCollector.DirtyNormal = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if ( dataCollector.PortCategory == MasterNodePortCategory.Fragment || dataCollector.PortCategory == MasterNodePortCategory.Debug )
			{
				dataCollector.ForceNormal = true;
				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, CurrentPrecisionType );
				dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
			}

			string vertexBitangent = GeneratorUtils.GenerateVertexBitangent( ref dataCollector, UniqueId, CurrentPrecisionType );
			return GetOutputVectorItem( 0, outputId, vertexBitangent );
		}
	}
}
