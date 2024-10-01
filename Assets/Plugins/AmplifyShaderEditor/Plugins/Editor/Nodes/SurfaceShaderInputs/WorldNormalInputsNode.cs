


using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"[Deprecated] World Normal"
#else
"[弃用]世界正常"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Surface Data"
#else
"地表数据"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Vertex Normal World"
#else
"顶点法线世界"
#endif
, null, KeyCode.None, true, true, 
#if !WB_LANGUAGE_CHINESE
"World Normal"
#else
"世界正常"
#endif
, typeof( WorldNormalVector ) )]
	public sealed class WorldNormalInputsNode : SurfaceShaderINParentNode
	{
		private const string PerPixelLabelStr = "Per Pixel";

		[SerializeField]
		private bool m_perPixel = true;

		[SerializeField]
		private string m_precisionString;

		[SerializeField]
		private bool m_addInstruction = false;

		public override void Reset()
		{
			base.Reset();
			m_addInstruction = true;
		}

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentInput = SurfaceInputs.WORLD_NORMAL;
			InitialSetup();
			
		}

		
		
		
		
		

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_perPixel = EditorGUILayoutToggleLeft( PerPixelLabelStr, m_perPixel );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				if ( m_addInstruction )
				{
					string precision = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, WirePortDataType.FLOAT3 );
					dataCollector.AddVertexInstruction( precision + " worldNormal = UnityObjectToWorldNormal(" + Constants.VertexShaderInputStr + ".normal)", UniqueId );
					m_addInstruction = false;
				}

				return GetOutputVectorItem( 0, outputId, "worldNormal" );
			}
			else
			{
				dataCollector.AddToInput( UniqueId, SurfaceInputs.WORLD_NORMAL, CurrentPrecisionType );
				dataCollector.AddToInput( UniqueId, SurfaceInputs.INTERNALDATA, addSemiColon: false );
				if ( dataCollector.PortCategory != MasterNodePortCategory.Debug && m_perPixel && dataCollector.DirtyNormal )
				{
					
					m_precisionString = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, WirePortDataType.FLOAT3 );
					string result = string.Format( Constants.WorldNormalLocalDecStr, m_precisionString );
					int count = 0;
					for ( int i = 0; i < m_outputPorts.Count; i++ )
					{
						if ( m_outputPorts[ i ].IsConnected )
						{
							if ( m_outputPorts[ i ].ConnectionCount > 2 )
							{
								count = 2;
								break;
							}
							count += 1;
							if ( count > 1 )
								break;
						}
					}
					if ( count > 1 )
					{
						string localVarName = "WorldNormal" + OutputId;
						dataCollector.AddToLocalVariables( UniqueId, CurrentPrecisionType, m_outputPorts[ 0 ].DataType, localVarName, result );
						return GetOutputVectorItem( 0, outputId, localVarName );
					}
					else
					{
						return GetOutputVectorItem( 0, outputId, result );
					}
				}
				else
				{
					return base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalVar );
				}
			}
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if ( UIUtils.CurrentShaderVersion() > 2504 )
				m_perPixel = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_perPixel );
		}
	}
}
