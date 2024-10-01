


using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Object Space Light Dir"
#else
"对象空间灯光方向"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Lighting"
#else
"照明"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Computes object space light direction (not normalized)"
#else
"计算对象空间光方向（未归一化）"
#endif
)]
	public sealed class ObjSpaceLightDirHlpNode : HelperParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_funcType = "ObjSpaceLightDir";
			m_inputPorts[ 0 ].Visible = false;
			m_outputPorts[ 0 ].ChangeType( WirePortDataType.FLOAT3, false );
			m_outputPorts[ 0 ].Name = "XYZ";

			AddOutputPort( WirePortDataType.FLOAT, "X" );
			AddOutputPort( WirePortDataType.FLOAT, "Y" );
			AddOutputPort( WirePortDataType.FLOAT, "Z" );

			m_useInternalPortData = false;
			m_previewShaderGUID = "c7852de24cec4a744b5358921e23feee";
			m_drawPreviewAsSphere = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( dataCollector.IsTemplate )
			{
				
				return GetOutputVectorItem( 0, outputId, dataCollector.TemplateDataCollectorInstance.GetObjectSpaceLightDir( CurrentPrecisionType ) );
			}

			dataCollector.AddToIncludes( UniqueId, Constants.UnityCgLibFuncs );
			dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_POS );

			string vertexPos = GeneratorUtils.GenerateVertexPosition( ref dataCollector, UniqueId, WirePortDataType.FLOAT4 );
			return GetOutputVectorItem( 0, outputId, GeneratorUtils.GenerateObjectLightDirection( ref dataCollector, UniqueId, CurrentPrecisionType, vertexPos ) );
		}
	}
}
