





using UnityEngine;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World Tangent"
#else
"世界切线"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Surface Data"
#else
"地表数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Per pixel world tangent vector"
#else
"每像素世界切线向量"
#endif
, null, KeyCode.None, true, false, null, null, 
#if !WB_LANGUAGE_CHINESE
"kebrus"
#else
"克卜鲁斯"
#endif
)]
	public sealed class VertexTangentNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputVectorPorts( WirePortDataType.FLOAT3, "XYZ" );
			m_drawPreviewAsSphere = true;
			m_previewShaderGUID = "61f0b80493c9b404d8c7bf56d59c3f81";
		}

		public override void PropagateNodeData( NodeData nodeData, ref MasterNodeDataCollector dataCollector )
		{
			base.PropagateNodeData( nodeData , ref dataCollector );
			dataCollector.DirtyNormal = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( dataCollector.IsTemplate )
			{
				return GetOutputVectorItem( 0, outputId, dataCollector.TemplateDataCollectorInstance.GetWorldTangent( CurrentPrecisionType ) );
			}

			if( dataCollector.PortCategory == MasterNodePortCategory.Fragment || dataCollector.PortCategory == MasterNodePortCategory.Debug )
			{
				dataCollector.ForceNormal = true;

				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, CurrentPrecisionType );
				dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
			}

			string worldTangent = GeneratorUtils.GenerateWorldTangent( ref dataCollector, UniqueId );

			return GetOutputVectorItem( 0, outputId, worldTangent );
		}
	}
}
