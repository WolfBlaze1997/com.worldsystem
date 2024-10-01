


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Transpose Model View Matrix"
#else
"转置模型视图矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Transpose of model * view matrix"
#else
"模型*视图矩阵的转置"
#endif
)]
	public sealed class TransposeMVMatrix : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
			m_value = "UNITY_MATRIX_T_MV";
			m_drawPreview = false;
		}
	}
}
