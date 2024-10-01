


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World Space Camera Pos"
#else
"世界空间相机位置"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Camera And Screen"
#else
"摄像头和屏幕"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"World Space Camera position"
#else
"世界空间相机位置"
#endif
)]
	public sealed class WorldSpaceCameraPos : ConstantShaderVariable
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputProperties( 0, "XYZ", WirePortDataType.FLOAT3 );
			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );

			m_value = "_WorldSpaceCameraPos";
			m_previewShaderGUID = "6b0c78411043dd24dac1152c84bb63ba";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			return GetOutputVectorItem( 0, outputId, base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar ) );
		}
	}
}
