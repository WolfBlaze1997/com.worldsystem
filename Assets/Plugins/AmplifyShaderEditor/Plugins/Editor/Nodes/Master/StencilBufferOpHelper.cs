using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AmplifyShaderEditor
{

	[Serializable]
	public class StencilBufferOpHelper
	{
		public readonly static string[] StencilComparisonValues =
		{
			"<Default>",
			"Greater" ,
			"GEqual" ,
			"Less" ,
			"LEqual" ,
			"Equal" ,
			"NotEqual" ,
			"Always" ,
			"Never"
		};

		public readonly static Dictionary<string,int> StencilComparisonValuesDict = new Dictionary<string, int>()
		{
			{"Greater" , 1},
			{"GEqual" ,	 2},
			{"Less" ,	 3},
			{"LEqual" ,	 4},
			{"Equal" ,	 5},
			{"NotEqual", 6},
			{"Always" ,	 7},
			{"Never"  ,  8},
		};

		public readonly static string[] StencilComparisonLabels =
		{
			"<Default>",
			"Greater" ,
			"Greater or Equal" ,
			"Less" ,
			"Less or Equal" ,
			"Equal" ,
			"Not Equal" ,
			"Always" ,
			"Never"
		};


		public readonly static string[] StencilOpsValues =
		{
			"<Default>",
			"Keep",
			"Zero",
			"Replace",
			"IncrSat",
			"DecrSat",
			"Invert",
			"IncrWrap",
			"DecrWrap"
		};

		public readonly static Dictionary<string,int> StencilOpsValuesDict = new Dictionary<string, int>()
		{
			{"Keep",	1},
			{"Zero",	2},
			{"Replace",	3},
			{"IncrSat",	4},
			{"DecrSat",	5},
			{"Invert",	6},
			{"IncrWrap",7},
			{"DecrWrap",8},
		};

		public readonly static string[] StencilOpsLabels =
		{
			"<Default>",
			"Keep",
			"Zero",
			"Replace",
			"IncrSat",
			"DecrSat",
			"Invert",
			"IncrWrap",
			"DecrWrap"
		};


		private const string FoldoutLabelStr = 
#if !WB_LANGUAGE_CHINESE
" Stencil Buffer"
#else
"模板缓冲器"
#endif
;
		private GUIContent ReferenceValueContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Reference"
#else
"参考资料"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"The value to be compared against (if Comparison is anything else than always) and/or the value to be written to the buffer (if either Pass, Fail or ZFail is set to replace)"
#else
"要与之进行比较的值（如果Comparison不是总是这样）和/或要写入缓冲区的值（是否将Pass、Fail或ZFail设置为替换）"
#endif
);
		private GUIContent ReadMaskContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Read Mask"
#else
"读取掩码"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"An 8 bit mask as an 0-255 integer, used when comparing the reference value with the contents of the buffer (referenceValue & readMask) comparisonFunction (stencilBufferValue & readMask)"
#else
"一个8位掩码，作为0-255的整数，用于将参考值与缓冲区的内容进行比较（referenceValue和readMask）比较函数（stencilBufferValue和readMask）"
#endif
);
		private GUIContent WriteMaskContent = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Write Mask"
#else
"写入掩码"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"An 8 bit mask as an 0-255 integer, used when writing to the buffer"
#else
"8位掩码，作为0-255整数，在写入缓冲区时使用"
#endif
);
		private const string ComparisonStr = 
#if !WB_LANGUAGE_CHINESE
"Comparison"
#else
"比较"
#endif
;
		private const string PassStr = 
#if !WB_LANGUAGE_CHINESE
"Pass"
#else
"通过"
#endif
;
		private const string FailStr = "Fail";
		private const string ZFailStr = "ZFail";

		private const string ComparisonFrontStr = 
#if !WB_LANGUAGE_CHINESE
"Comp. Front"
#else
"Comp。正面"
#endif
;
		private const string PassFrontStr = 
#if !WB_LANGUAGE_CHINESE
"Pass Front"
#else
"正面通行证"
#endif
;
		private const string FailFrontStr = 
#if !WB_LANGUAGE_CHINESE
"Fail Front"
#else
"失败前线"
#endif
;
		private const string ZFailFrontStr = 
#if !WB_LANGUAGE_CHINESE
"ZFail Front"
#else
"ZFail前部"
#endif
;

		private const string ComparisonBackStr = 
#if !WB_LANGUAGE_CHINESE
"Comp. Back"
#else
"Comp。< 返回"
#endif
;
		private const string PassBackStr = 
#if !WB_LANGUAGE_CHINESE
"Pass Back"
#else
"返回"
#endif
;
		private const string FailBackStr = 
#if !WB_LANGUAGE_CHINESE
"Fail Back"
#else
"失败返回"
#endif
;
		private const string ZFailBackStr = 
#if !WB_LANGUAGE_CHINESE
"ZFail Back"
#else
"ZFail返回"
#endif
;

		private const int ReferenceDefaultValue = 0;
		private const int ReadMaskDefaultValue = 255;
		private const int WriteMaskDefaultValue = 255;
		private const int ComparisonDefaultValue = 0;
		private const int PassStencilOpDefaultValue = 0;
		private const int FailStencilOpDefaultValue = 0;
		private const int ZFailStencilOpDefaultValue = 0;

		[SerializeField]
		private bool m_active;

		[SerializeField]
		private InlineProperty m_refValue = new InlineProperty( ReferenceDefaultValue );
		[SerializeField]
		private InlineProperty m_readMask = new InlineProperty( ReadMaskDefaultValue );
		[SerializeField]
		private InlineProperty m_writeMask = new InlineProperty( WriteMaskDefaultValue );

		
		[SerializeField]
		private InlineProperty m_comparisonFunctionIdx = new InlineProperty( ComparisonDefaultValue );
		[SerializeField]
		private InlineProperty m_comparisonFunctionBackIdx = new InlineProperty( ComparisonDefaultValue );

		
		[SerializeField]
		private InlineProperty m_passStencilOpIdx = new InlineProperty( PassStencilOpDefaultValue );
		[SerializeField]
		private InlineProperty m_passStencilOpBackIdx = new InlineProperty( PassStencilOpDefaultValue );

		
		[SerializeField]
		private InlineProperty m_failStencilOpIdx = new InlineProperty( FailStencilOpDefaultValue );
		[SerializeField]
		private InlineProperty m_failStencilOpBackIdx = new InlineProperty( FailStencilOpDefaultValue );

		
		[SerializeField]
		private InlineProperty m_zFailStencilOpIdx = new InlineProperty( ZFailStencilOpDefaultValue );
		[SerializeField]
		private InlineProperty m_zFailStencilOpBackIdx = new InlineProperty( ZFailStencilOpDefaultValue );

		public string CreateStencilOp( UndoParentNode owner )
		{
			string result = "\t\tStencil\n\t\t{\n";
			result += string.Format( "\t\t\tRef {0}\n", m_refValue.GetValueOrProperty() );
			if( m_readMask.Active || m_readMask.IntValue != ReadMaskDefaultValue )
			{
				result += string.Format( "\t\t\tReadMask {0}\n", m_readMask.GetValueOrProperty() );
			}

			if( m_writeMask.Active || m_writeMask.IntValue != WriteMaskDefaultValue )
			{
				result += string.Format( "\t\t\tWriteMask {0}\n", m_writeMask.GetValueOrProperty() );
			}

			if( ( owner as StandardSurfaceOutputNode ).CurrentCullMode == CullMode.Off )
			{
				if( m_comparisonFunctionIdx.IntValue != ComparisonDefaultValue || m_comparisonFunctionIdx.Active )
					result += string.Format( "\t\t\tCompFront {0}\n", m_comparisonFunctionIdx.GetValueOrProperty( StencilComparisonValues[ m_comparisonFunctionIdx.IntValue ] ) );
				if( m_passStencilOpIdx.IntValue != PassStencilOpDefaultValue || m_passStencilOpIdx.Active )
					result += string.Format( "\t\t\tPassFront {0}\n", m_passStencilOpIdx.GetValueOrProperty( StencilOpsValues[ m_passStencilOpIdx.IntValue ] ) );
				if( m_failStencilOpIdx.IntValue != FailStencilOpDefaultValue || m_failStencilOpIdx.Active )
					result += string.Format( "\t\t\tFailFront {0}\n", m_failStencilOpIdx.GetValueOrProperty( StencilOpsValues[ m_failStencilOpIdx.IntValue ] ) );
				if( m_zFailStencilOpIdx.IntValue != ZFailStencilOpDefaultValue || m_zFailStencilOpIdx.Active )
					result += string.Format( "\t\t\tZFailFront {0}\n", m_zFailStencilOpIdx.GetValueOrProperty( StencilOpsValues[ m_zFailStencilOpIdx.IntValue ] ) );

				if( m_comparisonFunctionBackIdx.IntValue != ComparisonDefaultValue || m_comparisonFunctionBackIdx.Active )
					result += string.Format( "\t\t\tCompBack {0}\n", m_comparisonFunctionBackIdx.GetValueOrProperty( StencilComparisonValues[ m_comparisonFunctionBackIdx.IntValue ] ) );
				if( m_passStencilOpBackIdx.IntValue != PassStencilOpDefaultValue || m_passStencilOpBackIdx.Active )
					result += string.Format( "\t\t\tPassBack {0}\n", m_passStencilOpBackIdx.GetValueOrProperty( StencilOpsValues[ m_passStencilOpBackIdx.IntValue ] ) );
				if( m_failStencilOpBackIdx.IntValue != FailStencilOpDefaultValue || m_failStencilOpBackIdx.Active )
					result += string.Format( "\t\t\tFailBack {0}\n", m_failStencilOpBackIdx.GetValueOrProperty( StencilOpsValues[ m_failStencilOpBackIdx.IntValue ] ) );
				if( m_zFailStencilOpBackIdx.IntValue != ZFailStencilOpDefaultValue || m_zFailStencilOpBackIdx.Active )
					result += string.Format( "\t\t\tZFailBack {0}\n", m_zFailStencilOpBackIdx.GetValueOrProperty( StencilOpsValues[ m_zFailStencilOpBackIdx.IntValue ] ) );
			}
			else
			{
				if( m_comparisonFunctionIdx.IntValue != ComparisonDefaultValue || m_comparisonFunctionIdx.Active )
					result += string.Format( "\t\t\tComp {0}\n", m_comparisonFunctionIdx.GetValueOrProperty( StencilComparisonValues[ m_comparisonFunctionIdx.IntValue ] ) );
				if( m_passStencilOpIdx.IntValue != PassStencilOpDefaultValue || m_passStencilOpIdx.Active )
					result += string.Format( "\t\t\tPass {0}\n", m_passStencilOpIdx.GetValueOrProperty( StencilOpsValues[ m_passStencilOpIdx.IntValue ] ) );
				if( m_failStencilOpIdx.IntValue != FailStencilOpDefaultValue || m_failStencilOpIdx.Active )
					result += string.Format( "\t\t\tFail {0}\n", m_failStencilOpIdx.GetValueOrProperty( StencilOpsValues[ m_failStencilOpIdx.IntValue ] ) );
				if( m_zFailStencilOpIdx.IntValue != ZFailStencilOpDefaultValue || m_zFailStencilOpIdx.Active )
					result += string.Format( "\t\t\tZFail {0}\n", m_zFailStencilOpIdx.GetValueOrProperty( StencilOpsValues[ m_zFailStencilOpIdx.IntValue ] ) );
			}


			result += "\t\t}\n";
			return result;
		}

		public void Draw( UndoParentNode owner )
		{
			bool foldoutValue = owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedStencilOptions;
			NodeUtils.DrawPropertyGroup( owner, ref foldoutValue, ref m_active, FoldoutLabelStr, () =>
			{
				float cache = EditorGUIUtility.labelWidth;
				float cache2 = EditorGUIUtility.fieldWidth;
				EditorGUIUtility.labelWidth = 110;
				EditorGUIUtility.fieldWidth = 30;
				m_refValue.IntSlider( ref owner, ReferenceValueContent, 0, 255 );
				m_readMask.IntSlider( ref owner, ReadMaskContent, 0, 255 );
				m_writeMask.IntSlider( ref owner, WriteMaskContent, 0, 255 );
				
				EditorGUIUtility.fieldWidth = cache2;
				if( ( owner as StandardSurfaceOutputNode ).CurrentCullMode == CullMode.Off )
				{
					m_comparisonFunctionIdx.EnumTypePopup( ref owner, ComparisonFrontStr, StencilComparisonLabels );
					m_passStencilOpIdx.EnumTypePopup( ref owner, PassFrontStr, StencilOpsLabels );
					m_failStencilOpIdx.EnumTypePopup( ref owner, FailFrontStr, StencilOpsLabels );
					m_zFailStencilOpIdx.EnumTypePopup( ref owner, ZFailFrontStr, StencilOpsLabels );
					EditorGUILayout.Separator();
					m_comparisonFunctionBackIdx.EnumTypePopup( ref owner, ComparisonBackStr, StencilComparisonLabels );
					m_passStencilOpBackIdx.EnumTypePopup( ref owner, PassBackStr, StencilOpsLabels );
					m_failStencilOpBackIdx.EnumTypePopup( ref owner, FailBackStr, StencilOpsLabels );
					m_zFailStencilOpBackIdx.EnumTypePopup( ref owner, ZFailBackStr, StencilOpsLabels );
				}
				else
				{
					m_comparisonFunctionIdx.EnumTypePopup( ref owner, ComparisonStr, StencilComparisonLabels );
					m_passStencilOpIdx.EnumTypePopup( ref owner, PassFrontStr, StencilOpsLabels );
					m_failStencilOpIdx.EnumTypePopup( ref owner, FailFrontStr, StencilOpsLabels );
					m_zFailStencilOpIdx.EnumTypePopup( ref owner, ZFailFrontStr, StencilOpsLabels );
				}
				EditorGUIUtility.labelWidth = cache;
			} );
			owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedStencilOptions = foldoutValue;
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			m_active = Convert.ToBoolean( nodeParams[ index++ ] );
			if( UIUtils.CurrentShaderVersion() > 14501 )
			{
				m_refValue.ReadFromString( ref index, ref nodeParams );
				m_readMask.ReadFromString( ref index, ref nodeParams );
				m_writeMask.ReadFromString( ref index, ref nodeParams );
				m_comparisonFunctionIdx.ReadFromString( ref index, ref nodeParams );
				m_passStencilOpIdx.ReadFromString( ref index, ref nodeParams );
				m_failStencilOpIdx.ReadFromString( ref index, ref nodeParams );
				m_zFailStencilOpIdx.ReadFromString( ref index, ref nodeParams );
			}
			else
			{
				m_refValue.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				m_readMask.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				m_writeMask.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				m_comparisonFunctionIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				m_passStencilOpIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				m_failStencilOpIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				m_zFailStencilOpIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
			}

			if( UIUtils.CurrentShaderVersion() > 13203 )
			{
				if( UIUtils.CurrentShaderVersion() > 14501 )
				{
					m_comparisonFunctionBackIdx.ReadFromString( ref index, ref nodeParams );
					m_passStencilOpBackIdx.ReadFromString( ref index, ref nodeParams );
					m_failStencilOpBackIdx.ReadFromString( ref index, ref nodeParams );
					m_zFailStencilOpBackIdx.ReadFromString( ref index, ref nodeParams );
				}
				else
				{
					m_comparisonFunctionBackIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
					m_passStencilOpBackIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
					m_failStencilOpBackIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
					m_zFailStencilOpBackIdx.IntValue = Convert.ToInt32( nodeParams[ index++ ] );
				}
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_active );
			m_refValue.WriteToString( ref nodeInfo );
			m_readMask.WriteToString( ref nodeInfo );
			m_writeMask.WriteToString( ref nodeInfo );
			m_comparisonFunctionIdx.WriteToString( ref nodeInfo );
			m_passStencilOpIdx.WriteToString( ref nodeInfo );
			m_failStencilOpIdx.WriteToString( ref nodeInfo );
			m_zFailStencilOpIdx.WriteToString( ref nodeInfo );
			m_comparisonFunctionBackIdx.WriteToString( ref nodeInfo );
			m_passStencilOpBackIdx.WriteToString( ref nodeInfo );
			m_failStencilOpBackIdx.WriteToString( ref nodeInfo );
			m_zFailStencilOpBackIdx.WriteToString( ref nodeInfo );
		}

		public bool Active
		{
			get { return m_active; }
			set { m_active = value; }
		}
	}
}
