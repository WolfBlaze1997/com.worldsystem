





using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World Bitangent"
#else
"世界比特币"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Surface Data"
#else
"地表数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Per pixel world bitangent vector"
#else
"每像素世界位变化矢量"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"kebrus"
#else
"克卜鲁斯"
#endif
)]
	public sealed class VertexBinormalNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputVectorPorts( WirePortDataType.FLOAT3, "XYZ" );
			m_drawPreviewAsSphere = true;
			m_previewShaderGUID = "76873532ab67d2947beaf07151383cbe";
		}

		public override void PropagateNodeData( NodeData nodeData, ref MasterNodeDataCollector dataCollector )
		{
			base.PropagateNodeData( nodeData, ref dataCollector );
			dataCollector.DirtyNormal = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( dataCollector.IsTemplate )
				return GetOutputVectorItem( 0, outputId, dataCollector.TemplateDataCollectorInstance.GetWorldBinormal( CurrentPrecisionType ) );

			if( dataCollector.PortCategory == MasterNodePortCategory.Fragment || dataCollector.PortCategory == MasterNodePortCategory.Debug )
			{
				dataCollector.ForceNormal = true;

				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, CurrentPrecisionType );
				dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
			}

			string worldBitangent = GeneratorUtils.GenerateWorldBitangent( ref dataCollector, UniqueId );

			return GetOutputVectorItem( 0, outputId, worldBitangent );
		}
	}
}
