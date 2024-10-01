


using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Remainder"
#else
"剩余"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Remainder between two int variables"
#else
"两个int变量之间的余数"
#endif
,tags: 
#if !WB_LANGUAGE_CHINESE
"modulo fmod"
#else
"模fmod"
#endif
)]
	public sealed class SimpleRemainderNode : DynamicTypeNode
	{
		private const string VertexFragRemainder = "( {0} % {1} )";
		
		private const string RemainderCalculationInt = "( {0} - {1} * ({0}/{1}))";
		private const string RemainderCalculationFloat = "( {0} - {1} * floor({0}/{1}))";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_useInternalPortData = true;
			m_textLabelWidth = 35;
			ChangeInputType( WirePortDataType.INT, false );
			ChangeOutputType( WirePortDataType.INT, false );
			m_useInternalPortData = true;
			m_previewShaderGUID = "8fdfc429d6b191c4985c9531364c1a95";
		}

		public override string BuildResults( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			base.BuildResults( outputId, ref dataCollector, ignoreLocalvar );
			string opMode = VertexFragRemainder;
			string result = string.Empty;
			switch( m_outputPorts[ 0 ].DataType )
			{
				case WirePortDataType.FLOAT:
				case WirePortDataType.FLOAT2:
				case WirePortDataType.FLOAT3:
				case WirePortDataType.FLOAT4:
				case WirePortDataType.INT:
				case WirePortDataType.COLOR:
				case WirePortDataType.OBJECT:
				{
					result = string.Format( opMode, m_inputA, m_inputB );
				}
				break;
				case WirePortDataType.FLOAT3x3:
				case WirePortDataType.FLOAT4x4:
				{
					result = UIUtils.InvalidParameter( this );
				}
				break;
			}

			return CreateOutputLocalVariable( 0, result, ref dataCollector );
		}
	}
}
