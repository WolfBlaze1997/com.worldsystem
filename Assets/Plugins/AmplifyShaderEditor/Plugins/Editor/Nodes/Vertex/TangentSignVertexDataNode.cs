


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Vertex Tangent Sign"
#else
"顶点切线符号"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vertex Data"
#else
"顶点数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Vertex tangent sign in object space, return the W value of tangent vector that contains only the sign of the tangent"
#else
"对象空间中的顶点切线符号，返回仅包含切线符号的切线向量的W值"
#endif
)]
	public sealed class TangentSignVertexDataNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputVectorPorts( WirePortDataType.FLOAT, "Sign" );
			m_drawPreviewAsSphere = true;
			m_previewShaderGUID = "f5466d126f4bb1f49917eac88b1cb6af";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			return GeneratorUtils.GenerateVertexTangentSign( ref dataCollector, UniqueId, CurrentPrecisionType ); ;
		}
	}
}
