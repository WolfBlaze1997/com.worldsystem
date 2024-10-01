


using System;
using UnityEngine;

namespace AmplifyShaderEditor
{
	public enum UnpackInputMode
	{
		Tangent,
		Object
	}

	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Unpack Scale Normal"
#else
"开箱秤正常"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Textures"
#else
"纹理"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Applies UnpackNormal/UnpackScaleNormal function"
#else
"应用UnpackNormal/UnpackScaleNormal功能"
#endif
)]
	[Serializable]
	public class UnpackScaleNormalNode : ParentNode
	{

		[SerializeField]
		private UnpackInputMode m_inputMode = UnpackInputMode.Tangent;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT4, false, 
#if !WB_LANGUAGE_CHINESE
"Value"
#else
"价值观"
#endif
);
			AddInputPort( WirePortDataType.FLOAT, false, 
#if !WB_LANGUAGE_CHINESE
"Scale"
#else
"规模"
#endif
);
			m_inputPorts[ 1 ].FloatInternalData = 1;
			AddOutputVectorPorts( WirePortDataType.FLOAT3, "XYZ" );
			m_useInternalPortData = true;
			m_autoWrapProperties = true;
			m_previewShaderGUID = "8b0ae05e25d280c45af81ded56f8012e";
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_inputMode = (UnpackInputMode)EditorGUILayoutEnumPopup( 
#if !WB_LANGUAGE_CHINESE
"Type"
#else
"类型"
#endif
, m_inputMode );
		}
		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string src = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			

			bool isScaledNormal = false;
			if ( m_inputPorts[ 1 ].IsConnected )
			{
				isScaledNormal = true;
			}
			else
			{
				if ( m_inputPorts[ 1 ].FloatInternalData != 1 )
				{
					isScaledNormal = true;
				}
			}

			string normalMapUnpackMode = string.Empty;
			string scaleValue = isScaledNormal?m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector ):"1.0";
			normalMapUnpackMode = GeneratorUtils.GenerateUnpackNormalStr( ref dataCollector, CurrentPrecisionType, UniqueId, OutputId, src, isScaledNormal, scaleValue , m_inputMode );
			if( isScaledNormal && !( dataCollector.IsTemplate && dataCollector.IsSRP ) )
			{
				dataCollector.AddToIncludes( UniqueId, Constants.UnityStandardUtilsLibFuncs );
			}
			
			int outputUsage = 0;
			for ( int i = 0; i < m_outputPorts.Count; i++ )
			{
				if ( m_outputPorts[ i ].IsConnected )
					outputUsage += 1;
			}


			if ( outputUsage > 1 && !dataCollector.IsSRP )
			{
				string varName = "localUnpackNormal" + OutputId;
				dataCollector.AddLocalVariable( UniqueId, "float3 " + varName + " = " + normalMapUnpackMode + ";" );
				return GetOutputVectorItem( 0, outputId, varName );
			}
			else
			{
				return GetOutputVectorItem( 0, outputId, normalMapUnpackMode );
			}
		}

		public override void WriteToString( ref string nodeInfo , ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo , ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo , m_inputMode );
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 18912 )
			{
				m_inputMode = (UnpackInputMode)Enum.Parse( typeof( UnpackInputMode ) , GetCurrentParam( ref nodeParams ) );
			}
		}
	}
}
