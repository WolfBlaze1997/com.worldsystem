


using UnityEngine;
using System;
using UnityEditor;
	
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Weighted Blend"
#else
"加权混合"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Mix all channels through weighted average sum"
#else
"通过加权平均和混合所有通道"
#endif
, null, KeyCode.None, true )]
	public sealed class WeightedBlendNode : WeightedAvgNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_inputData = new string[ 6 ];
			m_previewShaderGUID = "6076cbeaa41ebb14c85ff81b58df7d88";
		}
		
		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			GetInputData( ref dataCollector, ignoreLocalvar );

			string result = string.Empty;
			string avgSum = string.Empty;

			string localVarName = "weightedBlendVar" + OutputId;
			dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, m_inputPorts[ 0 ].DataType, localVarName, m_inputData[ 0 ] );
			
			if ( m_activeCount  < 2 )
			{
				return CreateOutputLocalVariable( 0, m_inputData[ 1 ], ref dataCollector );
			}
			else
			{
				for ( int i = 0; i < m_activeCount; i++ )
				{
					result += localVarName + Constants.VectorSuffixes[ i ] + "*" + m_inputData[ i + 1 ];
					avgSum += localVarName + Constants.VectorSuffixes[ i ];
					if ( i != ( m_activeCount - 1 ) )
					{
						result += " + ";
						avgSum += " + ";
					}
				}
			}

			result = UIUtils.AddBrackets( result ) + "/" + UIUtils.AddBrackets( avgSum );
			result = UIUtils.AddBrackets( result );
			RegisterLocalVariable( 0, result, ref dataCollector, "weightedAvg" + OutputId );
			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
