using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World To Tangent Matrix"
#else
"世界切线矩阵"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Matrix Transform"
#else
"矩阵变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"World to tangent transform matrix"
#else
"世界到切线变换矩阵"
#endif
)]
	public sealed class WorldToTangentMatrix : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputPort( WirePortDataType.FLOAT3x3, "Out" );
			
			m_drawPreview = false;
		}

		
		
		
		
		

		public override void PropagateNodeData( NodeData nodeData, ref MasterNodeDataCollector dataCollector )
		{
			base.PropagateNodeData( nodeData, ref dataCollector );
			dataCollector.DirtyNormal = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if( dataCollector.IsTemplate )
				return dataCollector.TemplateDataCollectorInstance.GetWorldToTangentMatrix( CurrentPrecisionType );

			if( dataCollector.IsFragmentCategory )
			{
				dataCollector.ForceNormal = true;

				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, CurrentPrecisionType );
				dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
			}

			GeneratorUtils.GenerateWorldToTangentMatrix( ref dataCollector, UniqueId, CurrentPrecisionType );

			return GeneratorUtils.WorldToTangentStr;
		}
	}
}
