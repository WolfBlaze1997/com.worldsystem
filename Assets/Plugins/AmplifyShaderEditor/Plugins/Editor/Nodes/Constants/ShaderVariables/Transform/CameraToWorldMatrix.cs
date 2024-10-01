


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Camera To World Matrix"
#else
"相机到世界矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Current camera to world matrix"
#else
"当前相机到世界矩阵"
#endif
)]
	public sealed class CameraToWorldMatrix : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "Out", WirePortDataType.FLOAT4x4 );
            m_value = "unity_CameraToWorld";
			m_drawPreview = false;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			GeneratorUtils.RegisterUnity2019MatrixDefines( ref dataCollector );
			return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
		}
	}
}
