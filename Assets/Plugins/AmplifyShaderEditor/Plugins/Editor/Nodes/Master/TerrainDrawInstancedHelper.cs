


using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class TerrainDrawInstancedHelper
	{
		private readonly string[] InstancedPragmas =
		{
			"multi_compile_instancing",
			"instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd"
		};

		private readonly string[] InstancedGlobalsSRP =
		{
			"#ifdef UNITY_INSTANCING_ENABLED//ASE Terrain Instancing",
			"\tTEXTURE2D(_TerrainHeightmapTexture);//ASE Terrain Instancing",
			"\tTEXTURE2D( _TerrainNormalmapTexture);//ASE Terrain Instancing",
			"\tSAMPLER(sampler_TerrainNormalmapTexture);//ASE Terrain Instancing",
			"#endif//ASE Terrain Instancing",
			"UNITY_INSTANCING_BUFFER_START( Terrain )//ASE Terrain Instancing",
			"\tUNITY_DEFINE_INSTANCED_PROP( float4, _TerrainPatchInstanceData )//ASE Terrain Instancing",
			"UNITY_INSTANCING_BUFFER_END( Terrain)//ASE Terrain Instancing",
			"CBUFFER_START( UnityTerrain)//ASE Terrain Instancing",
			"\t#ifdef UNITY_INSTANCING_ENABLED//ASE Terrain Instancing",
			"\t\tfloat4 _TerrainHeightmapRecipSize;//ASE Terrain Instancing",
			"\t\tfloat4 _TerrainHeightmapScale;//ASE Terrain Instancing",
			"\t#endif//ASE Terrain Instancing",
			"CBUFFER_END//ASE Terrain Instancing"
		};

		private readonly string[] InstancedGlobalsDefault =
		{
			"#ifdef UNITY_INSTANCING_ENABLED//ASE Terrain Instancing",
			"\tsampler2D _TerrainHeightmapTexture;//ASE Terrain Instancing",
			"\tsampler2D _TerrainNormalmapTexture;//ASE Terrain Instancing",
			"#endif//ASE Terrain Instancing",
			"UNITY_INSTANCING_BUFFER_START( Terrain )//ASE Terrain Instancing",
			"\tUNITY_DEFINE_INSTANCED_PROP( float4, _TerrainPatchInstanceData )//ASE Terrain Instancing",
			"UNITY_INSTANCING_BUFFER_END( Terrain)//ASE Terrain Instancing",
			"CBUFFER_START( UnityTerrain)//ASE Terrain Instancing",
			"\t#ifdef UNITY_INSTANCING_ENABLED//ASE Terrain Instancing",
			"\t\tfloat4 _TerrainHeightmapRecipSize;//ASE Terrain Instancing",
			"\t\tfloat4 _TerrainHeightmapScale;//ASE Terrain Instancing",
			"\t#endif//ASE Terrain Instancing",
			"CBUFFER_END//ASE Terrain Instancing"
		};


		private readonly string ApplyMeshModificationInstruction = "{0} = ApplyMeshModification({0});";

		private readonly string[] ApplyMeshModificationFunctionSRP =
		{
			"{0} ApplyMeshModification( {0} {1} )\n",
			"{\n",
			"#ifdef UNITY_INSTANCING_ENABLED\n",
			"\tfloat2 patchVertex = {0}.xy;\n",
			"\tfloat4 instanceData = UNITY_ACCESS_INSTANCED_PROP( Terrain, _TerrainPatchInstanceData );\n",
			"\tfloat2 sampleCoords = ( patchVertex.xy + instanceData.xy ) * instanceData.z;\n",
			"\tfloat height = UnpackHeightmap( _TerrainHeightmapTexture.Load( int3( sampleCoords, 0 ) ) );\n",
			"\t{0}.xz = sampleCoords* _TerrainHeightmapScale.xz;\n",
			"\t{0}.y = height* _TerrainHeightmapScale.y;\n",
			"\t#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL\n",
			"\t\t{0} = float3(0, 1, 0);\n",
			"\t#else\n",
			"\t\t{0} = _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb* 2 - 1;\n",
			"\t#endif\n",
			"",
			"",
			"",
			"\t{0}.xy = sampleCoords* _TerrainHeightmapRecipSize.zw;\n",
			"",
			"#endif\n",
			"\treturn {0};\n",
			"}\n"
		};
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		



		private readonly string[] ApplyMeshModificationFunctionDefaultTemplate =
		{
			"{0} ApplyMeshModification( {0} {1} )",
			"{\n",
			"#ifdef UNITY_INSTANCING_ENABLED\n",
			"\tfloat2 patchVertex = {0}.xy;\n",
			"\tfloat4 instanceData = UNITY_ACCESS_INSTANCED_PROP( Terrain, _TerrainPatchInstanceData );\n",
			
            "\tfloat4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;\n" +
			"\tfloat4 uvoffset = instanceData.xyxy * uvscale;\n" +
			"\tuvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;\n" +
			"\tfloat2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);\n",

			"\t{0} = float4( (patchVertex.xy * uvscale.zw + uvoffset.zw), 0, 0 );\n",
			"\tfloat height = UnpackHeightmap( tex2Dlod( _TerrainHeightmapTexture, {0} ) );\n",
			"\t{0}.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;\n",
			"\t{0}.y = height * _TerrainHeightmapScale.y;\n",
			"\t{0} = tex2Dlod( _TerrainNormalmapTexture, {1} ).rgb * 2 - 1;\n",
			"#endif\n",
			"return {0};\n",
			"}\n"
		};

		private readonly string ApplyMeshModificationInstructionStandard = "ApplyMeshModification({0});";
		private readonly string[] ApplyMeshModificationFunctionStandard =
		{
			"void ApplyMeshModification( inout {0} v )",
			"#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)",
			"\tfloat2 patchVertex = v.vertex.xy;",
			"\tfloat4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);",
			"\t",
			"\tfloat4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;",
			"\tfloat4 uvoffset = instanceData.xyxy * uvscale;",
			"\tuvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;",
			"\tfloat2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);",
			"\t",
			"\tfloat hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));",
			"\tv.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;",
			"\tv.vertex.y = hm * _TerrainHeightmapScale.y;",
			"\tv.vertex.w = 1.0f;",
			"\t",
			"\tv.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);",
			"\tv.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;",
			"\t",
			"\t#ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL",
			"\t\tv.normal = float3(0, 1, 0);",
			"\t\t//data.tc.zw = sampleCoords;",
			"\t#else",
			"\t\tfloat3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;",
			"\t\tv.normal = 2.0f * nor - 1.0f;",
			"\t#endif",
			"#endif",
		};
		private readonly string[] AdditionalUsePasses =
		{
			"Hidden/Nature/Terrain/Utilities/PICKING",
			"Hidden/Nature/Terrain/Utilities/SELECTION"
		};
		private readonly string DrawInstancedLabel = 
#if !WB_LANGUAGE_CHINESE
"Instanced Terrain"
#else
"实例化地形"
#endif
;

		[SerializeField]
		private bool m_enable = false;

		public void Draw( UndoParentNode owner )
		{
			m_enable = owner.EditorGUILayoutToggle( DrawInstancedLabel, m_enable );
		}

		public void UpdateDataCollectorForTemplates( ref MasterNodeDataCollector dataCollector, ref List<string> vertexInstructions )
		{
			if( m_enable )
			{
				for( int i = 0; i < AdditionalUsePasses.Length; i++ )
				{
					dataCollector.AddUsePass( AdditionalUsePasses[ i ], false );
				}

				for( int i = 0; i < InstancedPragmas.Length; i++ )
				{
					dataCollector.AddToPragmas( -1, InstancedPragmas[ i ] );
				}

				if( dataCollector.IsSRP )
				{

					TemplateFunctionData functionData = dataCollector.TemplateDataCollectorInstance.CurrentTemplateData.VertexFunctionData;
					string uvCoord = dataCollector.TemplateDataCollectorInstance.GetUV( 0, MasterNodePortCategory.Vertex );
					string vertexNormal = dataCollector.TemplateDataCollectorInstance.GetVertexNormal( PrecisionType.Float, false, MasterNodePortCategory.Vertex );
					
					string vertexPos = dataCollector.TemplateDataCollectorInstance.GetVertexPosition( WirePortDataType.OBJECT, PrecisionType.Float, false, MasterNodePortCategory.Vertex );

					string functionHeader = string.Format( ApplyMeshModificationFunctionSRP[ 0 ], functionData.InVarType, functionData.InVarName );

					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					
					string functionBody = functionHeader +
					ApplyMeshModificationFunctionSRP[ 1 ] +
					ApplyMeshModificationFunctionSRP[ 2 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 3 ], vertexPos ) +
					ApplyMeshModificationFunctionSRP[ 4 ] +
					ApplyMeshModificationFunctionSRP[ 5 ] +
					ApplyMeshModificationFunctionSRP[ 6 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 7 ], vertexPos ) +
					string.Format( ApplyMeshModificationFunctionSRP[ 8 ], vertexPos ) +
					ApplyMeshModificationFunctionSRP[ 9 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 10 ], vertexNormal ) +
					ApplyMeshModificationFunctionSRP[ 11 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 12 ], vertexNormal ) +
					ApplyMeshModificationFunctionSRP[ 13 ] +
					ApplyMeshModificationFunctionSRP[ 14 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 15 ], uvCoord ) +
					ApplyMeshModificationFunctionSRP[ 16 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 17 ], uvCoord ) +
					ApplyMeshModificationFunctionSRP[ 18 ] +
					ApplyMeshModificationFunctionSRP[ 19 ] +
					string.Format( ApplyMeshModificationFunctionSRP[ 20 ], functionData.InVarName ) +
					ApplyMeshModificationFunctionSRP[ 21 ];
					dataCollector.AddFunction( functionHeader, functionBody );
					
					for( int i = 0; i < InstancedGlobalsSRP.Length; i++ )
					{
						dataCollector.AddToUniforms( -1, InstancedGlobalsSRP[ i ] );
					}


					string vertexVarName = dataCollector.TemplateDataCollectorInstance.CurrentTemplateData.VertexFunctionData.InVarName;
					vertexInstructions.Insert( 0, string.Format( ApplyMeshModificationInstruction, vertexVarName ) );
				}
				else
				{
					TemplateFunctionData functionData = dataCollector.TemplateDataCollectorInstance.CurrentTemplateData.VertexFunctionData;

					string uvCoord = dataCollector.TemplateDataCollectorInstance.GetUV( 0, MasterNodePortCategory.Vertex );
					string vertexNormal = dataCollector.TemplateDataCollectorInstance.GetVertexNormal( PrecisionType.Float, false, MasterNodePortCategory.Vertex );
					string vertexPos = dataCollector.TemplateDataCollectorInstance.GetVertexPosition( WirePortDataType.OBJECT, PrecisionType.Float, false, MasterNodePortCategory.Vertex );

					string functionHeader = string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 0 ], functionData.InVarType, functionData.InVarName );
					string functionBody = functionHeader +
										ApplyMeshModificationFunctionDefaultTemplate[ 1 ] +
										ApplyMeshModificationFunctionDefaultTemplate[ 2 ] +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 3 ], vertexPos ) +
										ApplyMeshModificationFunctionDefaultTemplate[ 4 ] +
										ApplyMeshModificationFunctionDefaultTemplate[ 5 ] +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 6 ], uvCoord ) +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 7 ], uvCoord ) +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 8 ], vertexPos ) +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 9 ], vertexPos ) +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 10 ], vertexNormal, uvCoord ) +
										ApplyMeshModificationFunctionDefaultTemplate[ 11 ] +
										string.Format( ApplyMeshModificationFunctionDefaultTemplate[ 12 ], functionData.InVarName ) +
										ApplyMeshModificationFunctionDefaultTemplate[ 13 ];


					dataCollector.AddFunction( functionHeader, functionBody );
					for( int i = 0; i < InstancedGlobalsDefault.Length; i++ )
					{
						dataCollector.AddToUniforms( -1, InstancedGlobalsDefault[ i ] );
					}


					string vertexVarName = dataCollector.TemplateDataCollectorInstance.CurrentTemplateData.VertexFunctionData.InVarName;
					vertexInstructions.Insert( 0, string.Format( ApplyMeshModificationInstruction, vertexVarName ) );

				}
			}
		}

		public void UpdateDataCollectorForStandard( ref MasterNodeDataCollector dataCollector )
		{
			if( m_enable )
			{
				for( int i = 0; i < AdditionalUsePasses.Length; i++ )
				{
					dataCollector.AddUsePass( AdditionalUsePasses[ i ], false );
				}

				for( int i = 0; i < InstancedPragmas.Length; i++ )
				{
					dataCollector.AddToPragmas( -1, InstancedPragmas[ i ] );
				}
				string functionBody = string.Empty;

				string functionHeader = string.Format( ApplyMeshModificationFunctionStandard[ 0 ], dataCollector.SurfaceVertexStructure );
				IOUtils.AddFunctionHeader( ref functionBody, functionHeader );
				for( int i = 1; i < ApplyMeshModificationFunctionStandard.Length; i++ )
				{
					IOUtils.AddFunctionLine( ref functionBody, ApplyMeshModificationFunctionStandard[ i ] );
				}
				IOUtils.CloseFunctionBody( ref functionBody );

				
				
				
				

				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				

				dataCollector.AddFunction( functionHeader, functionBody );
				for( int i = 0; i < InstancedGlobalsDefault.Length; i++ )
				{
					dataCollector.AddToUniforms( -1, InstancedGlobalsDefault[ i ] );
				}
				
				dataCollector.AddVertexInstruction( string.Format( ApplyMeshModificationInstructionStandard, "v" ) );
			}
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			m_enable = Convert.ToBoolean( nodeParams[ index++ ] );
		}

		public void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_enable );
		}

		public bool Enabled { get { return m_enable; } }
	}
}
