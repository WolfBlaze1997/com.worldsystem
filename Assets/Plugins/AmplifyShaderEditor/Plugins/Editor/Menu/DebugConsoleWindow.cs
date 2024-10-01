




using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
namespace AmplifyShaderEditor
{
	public sealed class DebugConsoleWindow : EditorWindow
	{
		private const float WindowSizeX = 250;
		private const float WindowSizeY = 250;
		private const float WindowPosX = 5;
		private const float WindowPosY = 5;
		private Rect m_availableArea;

		private bool m_wikiAreaFoldout = true;
		private bool m_miscAreaFoldout = true;
		private Vector2 m_currentScrollPos;

		private int m_minURLNode = 0;
		private int m_maxURLNode = -1;
		private string m_root = string.Empty;
#if ASE_CONSOLE_WINDOW
		public readonly static bool DeveloperMode = true;
		public static bool UseShaderPanelsInfo = true;
		[MenuItem( "Window/Amplify Shader Editor/Open Debug Console" )]
		static void OpenMainShaderGraph()
		{
			OpenWindow();
		}
		[MenuItem( "Window/Amplify Shader Editor/Create Template Menu Items" )]
		public static void CreateTemplateMenuItems()
		{
			UIUtils.CurrentWindow.TemplatesManagerInstance.CreateTemplateMenuItems();
		}

#else
		public readonly static bool DeveloperMode = false;
		public static bool UseShaderPanelsInfo = false;
#endif

		public static DebugConsoleWindow OpenWindow()
		{
			if ( DeveloperMode )
			{
				DebugConsoleWindow currentWindow = ( DebugConsoleWindow ) DebugConsoleWindow.GetWindow( typeof( DebugConsoleWindow ), false, "ASE Debug Console" );
				currentWindow.titleContent.tooltip = "Debug Options for ASE. Intented only for ASE development team";
				currentWindow.minSize = new Vector2( WindowSizeX, WindowSizeY );
				currentWindow.maxSize = new Vector2( WindowSizeX, 2 * WindowSizeY ); ;
				currentWindow.wantsMouseMove = true;
				return currentWindow;
			}
			return null;
		}
		private void OnEnable()
		{
			m_root = Application.dataPath + "/../NodesInfo/";
			if( !Directory.Exists( m_root ) )
				Directory.CreateDirectory( m_root );
		}
		void OnGUI()
		{
			m_availableArea = new Rect( WindowPosX, WindowPosY, position.width - 2 * WindowPosX, position.height - 2 * WindowPosY );
			GUILayout.BeginArea( m_availableArea );
			{
				m_currentScrollPos = EditorGUILayout.BeginScrollView( m_currentScrollPos, GUILayout.Width( 0 ), GUILayout.Height( 0 ) );
				{
					EditorGUILayout.BeginVertical();
					{
						AmplifyShaderEditorWindow window = UIUtils.CurrentWindow;
						if ( window != null )
						{
							EditorGUILayout.Separator();

							NodeUtils.DrawPropertyGroup( ref m_wikiAreaFoldout, 
#if !WB_LANGUAGE_CHINESE
"Wiki Helper"
#else
"Wiki助手"
#endif
, ShowWikiHelperFunctions );

							EditorGUILayout.Separator();

							NodeUtils.DrawPropertyGroup( ref m_miscAreaFoldout, 
#if !WB_LANGUAGE_CHINESE
"Misc"
#else
"其他"
#endif
, ShowMiscFuntions );

							EditorGUILayout.Separator();
						}
						else
						{
							EditorGUILayout.LabelField( 
#if !WB_LANGUAGE_CHINESE
"Please open an ASE window to access debug options"
#else
"请打开ASE窗口以访问调试选项"
#endif
);
						}
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndScrollView();
			}
			GUILayout.EndArea();
		}

		void ShowWikiHelperFunctions()
		{
			AmplifyShaderEditorWindow window = UIUtils.CurrentWindow;
			EditorGUILayout.Separator();

			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Nodes Screen Shots"
#else
"节点屏幕截图"
#endif
) )
			{
				window.CurrentNodeExporterUtils.ActivateAutoScreenShot( m_root+"Shots/" ,0,-1 );
			}

			GUILayout.BeginHorizontal();
			if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Nodes URLs"
#else
"节点URL"
#endif
) )
			{
				window.CurrentNodeExporterUtils.ActivateNodesURL( m_minURLNode, m_maxURLNode );
			}
			m_minURLNode = EditorGUILayout.IntField( m_minURLNode );
			m_maxURLNode = EditorGUILayout.IntField( m_maxURLNode );
			GUILayout.EndHorizontal();
			EditorGUILayout.Separator();

			if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Nodes CSV Export"
#else
"节点CSV导出"
#endif
) )
			{
				window.CurrentNodeExporterUtils.GenerateNodesCSV( m_root );
			}
			EditorGUILayout.Separator();

			if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Nodes Undo Test"
#else
"节点撤消测试"
#endif
) )
			{
				window.CurrentNodeExporterUtils.ActivateAutoUndo();
			}

			EditorGUILayout.Separator();

			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Nodes Info"
#else
"节点信息"
#endif
) )
			{
				window.CurrentPaletteWindow.DumpAvailableNodes( false,  m_root );
				window.CurrentPaletteWindow.DumpAvailableNodes( true, m_root );
			}

			EditorGUILayout.Separator();

			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Shortcuts Info"
#else
"快捷方式信息"
#endif
) )
			{
				window.ShortcutManagerInstance.DumpShortcutsToDisk( Application.dataPath + "/../NodesInfo/" );
			}
		}

		void ShowMiscFuntions()
		{
			AmplifyShaderEditorWindow window = UIUtils.CurrentWindow;
			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Force Example Shader Compilation"
#else
"强制着色器示例编译"
#endif
) )
			{
				UIUtils.ForceExampleShaderCompilation();
			}
			EditorGUILayout.Separator();

			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Refresh Available Nodes"
#else
"刷新可用节点"
#endif
) )
			{
				window.RefreshAvaibleNodes();
			}

			EditorGUILayout.Separator();

			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Dump Uniform Names"
#else
"转储制服名称"
#endif
) )
			{
				
				window.DuplicatePrevBufferInstance.DumpUniformNames();
			}

			EditorGUILayout.Separator();

			if ( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Force Palette Update"
#else
"强制调色板更新"
#endif
) )
			{
				Debug.Log( UIUtils.CurrentWindow.IsShaderFunctionWindow );
				window.CurrentPaletteWindow.ForceUpdate = true;
			}

			EditorGUILayout.Separator();

			if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Detect Infinite Loops"
#else
"检测无限循环"
#endif
) )
			{
				if( window.IsShaderFunctionWindow )
				{
					Debug.Log( "Starting infinite loop detection over shader functions" );
					List<FunctionOutput> nodes = window.OutsideGraph.FunctionOutputNodes.NodesList;
					for( int i = 0; i < nodes.Count; i++ )
					{
						UIUtils.DetectNodeLoopsFrom( nodes[ i ], new Dictionary<int, int>() );
					}
				}
				else
				{
					if( window.OutsideGraph.MultiPassMasterNodes.Count > 0 )
					{
						Debug.Log( "Starting infinite loop detection over shader from template" );
						List<TemplateMultiPassMasterNode> nodes = window.OutsideGraph.MultiPassMasterNodes.NodesList;
						for( int i = 0; i < nodes.Count; i++ )
						{
							UIUtils.DetectNodeLoopsFrom( nodes[ i ], new Dictionary<int, int>() );
						}
					}
					else
					{
						Debug.Log( "Starting infinite loop detection over standard shader" );
						UIUtils.DetectNodeLoopsFrom( window.OutsideGraph.CurrentMasterNode, new Dictionary<int, int>() );
					}
				}
				Debug.Log( "End infinite loop detection" );
			}
		}
	}
}



