


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Scale Matrix"
#else
"比例矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Scale Matrix"
#else
"比例矩阵"
#endif
,null, UnityEngine.KeyCode.None, true, true, 
#if !WB_LANGUAGE_CHINESE
"Object Scale"
#else
"对象比例"
#endif
)]
	public sealed class UnityScaleMatrix : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
			m_value = "unity_Scale";
			m_drawPreview = false;
		}
	}
}
