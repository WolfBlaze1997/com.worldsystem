


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Texture 3 Matrix"
#else
"纹理3矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Texture 3 Matrix"
#else
"纹理3矩阵"
#endif
, null, UnityEngine.KeyCode.None, true, true )]
	public sealed class Texture3MatrixNode : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
			m_value = "UNITY_MATRIX_TEXTURE3";
			m_drawPreview = false;
		}
	}
}
