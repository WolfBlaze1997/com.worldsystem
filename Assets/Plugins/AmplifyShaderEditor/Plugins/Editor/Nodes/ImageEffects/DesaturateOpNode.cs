





namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Desaturate"
#else
"去饱和"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Image Effects"
#else
"图像效果"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Generic desaturation operation"
#else
"通用去饱和操作"
#endif
)]
	public sealed class DesaturateOpNode : ParentNode
	{
		private const string GenericDesaturateOp0 = "dot( {0}, float3( 0.299, 0.587, 0.114 ))";
		private const string GenericDesaturateOp1 = "lerp( {0}, {1}.xxx, {2} )";
		

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3, false, "RGB" );
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Fraction"
#else
"分数"
#endif
);
			AddOutputPort( WirePortDataType.FLOAT3, Constants.EmptyPortValue );
			m_useInternalPortData = true;
			m_previewShaderGUID = "faabe9efdf44b9648a523f1742abdfd3";
		}

		void UpdatePorts( int portId )
		{
			if ( portId == 0 )
			{
				m_inputPorts[ 0 ].MatchPortToConnection();
				m_outputPorts[ 0 ].ChangeType( m_inputPorts[ 0 ].DataType, false );
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{

			if ( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );

			string initalColorValue = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string fraction = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			string initialColorVarName = "desaturateInitialColor" + OutputId;
			dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT3, initialColorVarName, initalColorValue );

			string dotVarName = "desaturateDot" + OutputId;
			string dotVarValue = string.Format( GenericDesaturateOp0, initialColorVarName );

			dataCollector.AddLocalVariable( UniqueId, CurrentPrecisionType, WirePortDataType.FLOAT, dotVarName, dotVarValue );
			RegisterLocalVariable( 0, string.Format( GenericDesaturateOp1, initialColorVarName, dotVarName,fraction ), ref dataCollector, "desaturateVar" + OutputId );

			return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
		}
	}
}
