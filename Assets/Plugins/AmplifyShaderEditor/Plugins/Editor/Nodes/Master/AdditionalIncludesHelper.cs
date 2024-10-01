using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class AdditionalIncludesHelper
	{
		private const string AdditionalIncludesStr = 
#if !WB_LANGUAGE_CHINESE
" Additional Includes"
#else
"其他包括"
#endif
;
		private const float ShaderKeywordButtonLayoutWidth = 15;
		private ParentNode m_currentOwner;

		[SerializeField]
		private List<string> m_additionalIncludes = new List<string>();
		public List<string> IncludeList { get { return m_additionalIncludes; } set { m_additionalIncludes = value; } }

		[SerializeField]
		private List<string> m_outsideIncludes = new List<string>();
		public List<string> OutsideList { get { return m_outsideIncludes; } set { m_outsideIncludes = value; } }

		public void Draw( ParentNode owner )
		{
			m_currentOwner = owner;
			bool value = owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedAdditionalIncludes;
			NodeUtils.DrawPropertyGroup( ref value, AdditionalIncludesStr, DrawMainBody, DrawButtons );
			owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedAdditionalIncludes = value;

		}

		void DrawButtons()
		{
			EditorGUILayout.Separator();

			
			if( GUILayout.Button( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
			{
				m_additionalIncludes.Add( string.Empty );
				EditorGUI.FocusTextInControl( null );
			}

			
			if( GUILayout.Button( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
			{
				if( m_additionalIncludes.Count > 0 )
				{
					m_additionalIncludes.RemoveAt( m_additionalIncludes.Count - 1 );
					EditorGUI.FocusTextInControl( null );
				}
			}
		}

		void DrawMainBody()
		{
			EditorGUILayout.Separator();
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			int itemCount = m_additionalIncludes.Count;
			int markedToDelete = -1;
			for( int i = 0; i < itemCount; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUI.BeginChangeCheck();
					m_additionalIncludes[ i ] = EditorGUILayout.TextField( m_additionalIncludes[ i ] );
					if( EditorGUI.EndChangeCheck() )
					{
						m_additionalIncludes[ i ] = UIUtils.RemoveShaderInvalidCharacters( m_additionalIncludes[ i ] );
					}

					
					if( m_currentOwner.GUILayoutButton( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
					{
						m_additionalIncludes.Insert( i + 1, string.Empty );
						EditorGUI.FocusTextInControl( null );
					}

					
					if( m_currentOwner.GUILayoutButton( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
					{
						markedToDelete = i;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if( markedToDelete > -1 )
			{
				if( m_additionalIncludes.Count > markedToDelete )
				{
					m_additionalIncludes.RemoveAt( markedToDelete );
					EditorGUI.FocusTextInControl( null );
				}
			}
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Please add your includes without the #include \"\" keywords"
#else
"请添加不带#include关键字的包含项"
#endif
, MessageType.Info );
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			int count = Convert.ToInt32( nodeParams[ index++ ] );
			for( int i = 0; i < count; i++ )
			{
				m_additionalIncludes.Add( nodeParams[ index++ ] );
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_additionalIncludes.Count );
			for( int i = 0; i < m_additionalIncludes.Count; i++ )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_additionalIncludes[ i ] );
			}
		}

		public void AddToDataCollector( ref MasterNodeDataCollector dataCollector )
		{
			for( int i = 0; i < m_additionalIncludes.Count; i++ )
			{
				if( !string.IsNullOrEmpty( m_additionalIncludes[ i ] ) )
					dataCollector.AddToIncludes( -1, m_additionalIncludes[ i ] );
			}

			for( int i = 0; i < m_outsideIncludes.Count; i++ )
			{
				if( !string.IsNullOrEmpty( m_outsideIncludes[ i ] ) )
					dataCollector.AddToIncludes( -1, m_outsideIncludes[ i ] );
			}
		}

		public void Destroy()
		{
			m_additionalIncludes.Clear();
			m_additionalIncludes = null;
			m_currentOwner = null;
		}
	}
}
