


namespace AmplifyShaderEditor
{
	[System.Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"World To Object"
#else
"世界反对"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Object Transform"
#else
"对象变换"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Transforms input to Object Space"
#else
"将输入转换为对象空间"
#endif
)]
	public sealed class WorldToObjectTransfNode : ParentTransfNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_matrixName = "unity_WorldToObject";
			m_matrixHDName = "GetWorldToObjectMatrix()";
			m_matrixLWName = "GetWorldToObjectMatrix()";
			m_previewShaderGUID = "79a5efd1e3309f54d8ba3e7fdf5e459b";
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
				return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );

			string value = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string matrixName = string.Empty;
			if( dataCollector.IsTemplate  )
			{
				if( dataCollector.TemplateDataCollectorInstance.CurrentSRPType == TemplateSRPType.HDRP && !string.IsNullOrEmpty( m_matrixHDName ) )
				{
					string varName = "localWorldVar" + OutputId;
					dataCollector.AddLocalVariable( UniqueId, PrecisionType.Float, WirePortDataType.FLOAT4, varName, value );
					dataCollector.AddLocalVariable( UniqueId, string.Format( "({0}).xyz", varName ), string.Format( "GetCameraRelativePositionWS(({0}).xyz);", varName ) );
					value = varName;
					matrixName = m_matrixHDName;
				}
				else if( dataCollector.TemplateDataCollectorInstance.CurrentSRPType == TemplateSRPType.URP && !string.IsNullOrEmpty( m_matrixLWName ) )
				{
					matrixName = m_matrixLWName;
				}
				else
				{
					matrixName = m_matrixName;
				}
			}
			else
			{
				matrixName = m_matrixName;
			}

			RegisterLocalVariable( 0, string.Format( "mul({0},{1})", matrixName, value ), ref dataCollector, "transform" + OutputId );
			return GetOutputVectorItem( 0, outputId, m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory ) );
		}
	}
}
