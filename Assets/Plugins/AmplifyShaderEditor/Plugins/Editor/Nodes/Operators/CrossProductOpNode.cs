


using System;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Cross"
#else
"十字架"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Vector Operators"
#else
"矢量运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Cross product of two three-component vectors ( A x B )"
#else
"两个三分量向量的叉积（A x B）"
#endif
, null, KeyCode.X )]
	public sealed class CrossProductOpNode : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"Lhs"
#else
"Lhs"
#endif
);
			AddInputPort( WirePortDataType.FLOAT3, false, 
#if !WB_LANGUAGE_CHINESE
"Rhs"
#else
"Rhs"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT3, "Out" );
			m_useInternalPortData = true;
			m_previewShaderGUID = "65a9be5cc7037654db8e148d669f03ee";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
			string lhsStr = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string rhsStr = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			string result = "cross( " + lhsStr + " , " + rhsStr + " )";
			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}

	}
}
