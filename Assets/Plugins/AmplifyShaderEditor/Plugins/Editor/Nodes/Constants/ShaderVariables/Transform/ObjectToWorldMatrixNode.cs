


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Object To World Matrix"
#else
"对象到世界矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Current model matrix"
#else
"当前模型矩阵"
#endif
)]
	public sealed class ObjectToWorldMatrixNode : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
            m_value = "unity_ObjectToWorld";
			m_HDValue = "GetObjectToWorldMatrix()";
			m_LWValue = "GetObjectToWorldMatrix()"; 
		}
    }
}
