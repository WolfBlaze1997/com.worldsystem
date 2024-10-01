


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World To Object Matrix"
#else
"世界对象矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Inverse of current world matrix"
#else
"当前世界矩阵的逆"
#endif
)]
	public sealed class WorldToObjectMatrix : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
            m_value = "unity_WorldToObject";
			m_HDValue = "GetWorldToObjectMatrix()";
			m_LWValue = "GetWorldToObjectMatrix()";
			m_drawPreview = false;
		}
    }
}
