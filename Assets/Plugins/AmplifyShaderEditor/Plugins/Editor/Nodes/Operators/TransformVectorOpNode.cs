


using System;
using UnityEngine;

namespace AmplifyShaderEditor
{
	public enum CoordinateSpaces
	{
		Tangent,
		Local,
		World,
		View,
		Clip,
		Screen
	}

	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"Transform Vector"
#else
"变换矢量"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Math Operators"
#else
"数学运算符"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"Transforma a vector into another"
#else
"将一个向量转换为另一个向量"
#endif
, null, KeyCode.None, false )]
	public sealed class TransformVectorOpNode : ParentNode
	{
		[SerializeField]
		private CoordinateSpaces m_source = CoordinateSpaces.Tangent;
		[SerializeField]
		private CoordinateSpaces m_destination = CoordinateSpaces.World;

		private const string InputTangentrStr = "float4 tangent: TANGENT";
		private const string ColorValueStr = ".tangent";

		private const string InputNormalStr = "float3 normal : NORMAL";
		private const string NormalValueStr = ".normal";

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT4, false, Constants.EmptyPortValue );
			AddOutputPort( WirePortDataType.FLOAT4, Constants.EmptyPortValue );
			m_useInternalPortData = true;
		}

		void AddTangentInfo( ref MasterNodeDataCollector dataCollector )
		{
			dataCollector.AddToInput( UniqueId, InputTangentrStr, true );
			dataCollector.AddToInput( UniqueId, InputTangentrStr, true );
			dataCollector.AddToInput( UniqueId, InputNormalStr, true );
			dataCollector.AddToFragmentLocalVariables( UniqueId, "float3 binormal = cross( normalize( v.normal ), normalize( v.tangent.xyz ) ) * v.tangent.w;" );
			dataCollector.AddToFragmentLocalVariables( UniqueId, "float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );" );

		}

		public override string GenerateShaderForOutput( int outputId,  ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{

			
			
			
			

			string value = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			
			dataCollector.AddToIncludes( UniqueId, Constants.UnityShaderVariables );



			switch ( m_source )
			{
				case CoordinateSpaces.Tangent:
				{
					AddTangentInfo( ref dataCollector );
					switch ( m_destination )
					{
						case CoordinateSpaces.Tangent:
						{
							return value;
						}
						
						
						
						
						
						
						
						
						
					}
				}
				break;
				case CoordinateSpaces.Local:
				{
					switch ( m_destination )
					{
						case CoordinateSpaces.Tangent:
						{
							AddTangentInfo( ref dataCollector );
							return "float4(mul( rotation , " + value + ".xyz ),1)";
						}
						case CoordinateSpaces.Local:
						{
							return value;
						}
						case CoordinateSpaces.World:
						{
							return "mul( _Object2World , " + value + " )";
						}
						case CoordinateSpaces.View:
						{
							return "mul( UNITY_MATRIX_MV , " + value + " )";
						}
					}
				}
				break;
				case CoordinateSpaces.World:
				{
					switch ( m_destination )
					{
						
						
						
						case CoordinateSpaces.Local:
						{
							return "mul( _World2Object , " + value + " )";
						}
						case CoordinateSpaces.World:
						{
							return value;
						}
						case CoordinateSpaces.View:
						{
							return "mul( UNITY_MATRIX_V , " + value + " )";
						}
					}
				}
				break;
				case CoordinateSpaces.View:
				{
					UIUtils.ShowMessage( UniqueId, "View as Source is not supported", MessageSeverity.Warning );
					return value;
				}
			}

			return UIUtils.UnknownError( this );
		}


		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_source = ( CoordinateSpaces ) Enum.Parse( typeof( CoordinateSpaces ), GetCurrentParam( ref nodeParams ) );
			m_destination = ( CoordinateSpaces ) Enum.Parse( typeof( CoordinateSpaces ), GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_source );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_destination );
		}
	}
}
