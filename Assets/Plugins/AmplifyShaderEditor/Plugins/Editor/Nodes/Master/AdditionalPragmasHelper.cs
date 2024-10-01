


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class AdditionalPragmasHelper
	{
		private const string AdditionalPragmasStr = 
#if !WB_LANGUAGE_CHINESE
" Additional Pragmas"
#else
"其他Pragmas"
#endif
;
		private const float ShaderKeywordButtonLayoutWidth = 15;
		private ParentNode m_currentOwner;

		[SerializeField]
		private List<string> m_additionalPragmas = new List<string>();
		public List<string> PragmaList { get { return m_additionalPragmas; } set { m_additionalPragmas = value; } }

		[SerializeField]
		private List<string> m_outsidePragmas = new List<string>();
		public List<string> OutsideList { get { return m_outsidePragmas; } set { m_outsidePragmas = value; } }

		public void Draw( ParentNode owner )
		{
			m_currentOwner = owner;
			bool value = owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedAdditionalPragmas;
			NodeUtils.DrawPropertyGroup( ref value, AdditionalPragmasStr, DrawMainBody, DrawButtons );
			owner.ContainerGraph.ParentWindow.InnerWindowVariables.ExpandedAdditionalPragmas = value;

		}

		void DrawButtons()
		{
			EditorGUILayout.Separator();

			
			if( GUILayout.Button( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
			{
				m_additionalPragmas.Add( string.Empty );
				EditorGUI.FocusTextInControl( null );
			}

			
			if( GUILayout.Button( string.Empty, UIUtils.MinusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
			{
				if( m_additionalPragmas.Count > 0 )
				{
					m_additionalPragmas.RemoveAt( m_additionalPragmas.Count - 1 );
					EditorGUI.FocusTextInControl( null );
				}
			}
		}

		void DrawMainBody()
		{
			EditorGUILayout.Separator();
			int itemCount = m_additionalPragmas.Count;
			int markedToDelete = -1;
			for( int i = 0; i < itemCount; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUI.BeginChangeCheck();
					m_additionalPragmas[ i ] = EditorGUILayout.TextField( m_additionalPragmas[ i ] );
					if( EditorGUI.EndChangeCheck() )
					{
						m_additionalPragmas[ i ] = UIUtils.RemoveShaderInvalidCharacters( m_additionalPragmas[ i ] );
					}

					
					if( m_currentOwner.GUILayoutButton( string.Empty, UIUtils.PlusStyle, GUILayout.Width( ShaderKeywordButtonLayoutWidth ) ) )
					{
						m_additionalPragmas.Insert( i + 1, string.Empty );
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
				if( m_additionalPragmas.Count > markedToDelete )
				{
					m_additionalPragmas.RemoveAt( markedToDelete );
					EditorGUI.FocusTextInControl( null );
				}
			}
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox( 
#if !WB_LANGUAGE_CHINESE
"Please add your pragmas without the #pragma keywords"
#else
"请添加不带#pragma关键字的pragmas"
#endif
, MessageType.Info );
		}

		public void ReadFromString( ref uint index, ref string[] nodeParams )
		{
			int count = Convert.ToInt32( nodeParams[ index++ ] );
			for( int i = 0; i < count; i++ )
			{
				m_additionalPragmas.Add( nodeParams[ index++ ] );
			}
		}

		public void WriteToString( ref string nodeInfo )
		{
			IOUtils.AddFieldValueToString( ref nodeInfo, m_additionalPragmas.Count );
			for( int i = 0; i < m_additionalPragmas.Count; i++ )
			{
				IOUtils.AddFieldValueToString( ref nodeInfo, m_additionalPragmas[ i ] );
			}
		}

		public void AddToDataCollector( ref MasterNodeDataCollector dataCollector )
		{
			for( int i = 0; i < m_additionalPragmas.Count; i++ )
			{
				if( !string.IsNullOrEmpty( m_additionalPragmas[ i ] ) )
					dataCollector.AddToPragmas( -1, m_additionalPragmas[ i ] );
			}

			for( int i = 0; i < m_outsidePragmas.Count; i++ )
			{
				if( !string.IsNullOrEmpty( m_outsidePragmas[ i ] ) )
					dataCollector.AddToPragmas( -1, m_outsidePragmas[ i ] );
			}
		}

		public void Destroy()
		{
			m_additionalPragmas.Clear();
			m_additionalPragmas = null;
			m_currentOwner = null;
		}
	}
}
