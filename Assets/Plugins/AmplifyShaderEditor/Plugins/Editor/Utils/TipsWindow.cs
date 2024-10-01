


using System.IO;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	public class TipsWindow : MenuParent
	{
		private static bool m_showWindow = false;
		private bool m_dontShowAtStart = false;

		private static List<string> AllTips = new List<string>() {
			"You can press W to toggle between a flat and color coded Wires and ports.",
			"You can press CTRL+W to toggle between multiline or singleline Wire connections.",
			"You can press P to globally open all node Previews.",
			"You can press F to Focus your selection, single tap centers the selection while double tap it to also zooms on in.",
			"You can press CTRL+F to open a search bar and Find a node by it's title",
			"You can press SPACE to open a context menu to add a new node and press TAB or SHIFT+TAB tocycle between the found nodes",
			"You can remove a node without breaking the graph connections by pressing ALT and then dragging the node out",
			"You can switch two input connections holding CTRL while dragging one input connection into the other",
		};

		int m_currentTip = 0;

		public TipsWindow( AmplifyShaderEditorWindow parentWindow ) : base( parentWindow, 0, 0, 0, 64, "Tips", MenuAnchor.TOP_LEFT, MenuAutoSize.NONE )
		{
			
		}

		public override void Draw( Rect parentPosition, Vector2 mousePosition, int mouseButtonId, bool hasKeyboadFocus )
		{
			base.Draw( parentPosition, mousePosition, mouseButtonId, hasKeyboadFocus );

			DrawWindow( mousePosition );
		}

		public void DrawWindow( Vector2 mousePosition )
		{
			if( !m_showWindow )
				return;

			Rect windowRect = new Rect( 0, 0, Screen.width, Screen.height );
			Vector2 center = windowRect.center;
			windowRect.size = new Vector2( 300, 200 );
			windowRect.center = center;
			Color temp = GUI.color;
			GUI.color = Color.white;
			GUI.Label( windowRect, string.Empty, GUI.skin.FindStyle( 
#if !WB_LANGUAGE_CHINESE
"flow node 0"
#else
"流节点0"
#endif
) );

			if( Event.current.type == EventType.MouseDown && !windowRect.Contains( mousePosition ) )
				m_showWindow = false;

			Rect titleRect = windowRect;
			titleRect.height = 35;
			GUI.Label( titleRect, 
#if !WB_LANGUAGE_CHINESE
"Quick Tip!"
#else
"快速提示！"
#endif
, GUI.skin.FindStyle( 
#if !WB_LANGUAGE_CHINESE
"TL Selection H2"
#else
"TL选择H2"
#endif
) );
			Rect button = titleRect;
			button.size = new Vector2( 14, 14 );
			button.y += 2;
			button.x = titleRect.xMax - 16;
			if( GUI.Button( button, string.Empty, GUI.skin.FindStyle( "WinBtnClose" ) ) )
				CloseWindow();

			button.y += 100;
			if( GUI.Button( button, ">" ) )
			{
				m_currentTip++;
				if( m_currentTip >= AllTips.Count )
					m_currentTip = 0;
			}
			
			Rect textRect = windowRect;
			textRect.yMin = titleRect.yMax;
			GUI.Label( textRect, AllTips[ m_currentTip ], GUI.skin.FindStyle( 
#if !WB_LANGUAGE_CHINESE
"WordWrappedLabel"
#else
"WordWrappedLabel"
#endif
) );

			Rect footerRect = windowRect;
			footerRect.yMin = footerRect.yMax - 18;
			footerRect.x += 3;
			GUI.Label( footerRect, (m_currentTip + 1) + 
#if !WB_LANGUAGE_CHINESE
" of "
#else
"属于"
#endif
+ AllTips.Count + 
#if !WB_LANGUAGE_CHINESE
" tips"
#else
"提示"
#endif
);
			footerRect.x += 170;
			EditorGUI.BeginChangeCheck();
			m_dontShowAtStart = GUI.Toggle( footerRect, m_dontShowAtStart, "Don't show at start" );
			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool( "DontShowTipAtStart", m_dontShowAtStart );
			}
			GUI.color = temp;

			if( Event.current.type == EventType.MouseDown && windowRect.Contains( mousePosition ) )
			{
				Event.current.Use();
				ParentWindow.MouseInteracted = true;
			}
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		public static void ShowWindow( bool toggle = true )
		{
			if( toggle )
				m_showWindow = !m_showWindow;
			else
				m_showWindow = true;

			
			
		}

		
		
		
		
		
		
		
		

		
		
		
		
		
		
		
		
		
		
		
		
		
		
		

		public static void CloseWindow()
		{
			m_showWindow = false;
		}
	}
}
