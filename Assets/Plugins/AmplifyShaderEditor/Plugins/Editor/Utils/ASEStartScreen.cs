


using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace AmplifyShaderEditor
{
	public class ASEStartScreen : EditorWindow
	{
		[MenuItem( "Window/Amplify Shader Editor/Start Screen", false, 1999 )]
		public static void Init()
		{
			ASEStartScreen window = (ASEStartScreen)GetWindow( typeof( ASEStartScreen ), true, "Amplify Shader Editor Start Screen" );
			window.minSize = new Vector2( 650, 500 );
			window.maxSize = new Vector2( 650, 500 );
			window.Show();
		}

		private readonly static string ChangeLogGUID = "580cccd3e608b7f4cac35ea46d62d429";
		private readonly static string ResourcesGUID = "c0a0a980c9ba86345bc15411db88d34f";
		private readonly static string BuiltInGUID = "e00e6f90ab8233e46a41c5e33917c642";
		private readonly static string UniversalGUID = "a9d68dd8913f05d4d9ce75e7b40c6044";
		private readonly static string HighDefinitionGUID = "d1c0b77896049554fa4b635531caf741";

		private readonly static string IconGUID = "2c6536772776dd84f872779990273bfc";

		public readonly static string ChangelogURL = "https://amplify.pt/Banner/ASEchangelog.json";

		private readonly static string ManualURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Manual";
		private readonly static string BasicURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Tutorials#Official_-_Basics";
		private readonly static string BeginnerURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Tutorials#Official_-_Beginner_Series";
		private readonly static string NodesURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Nodes";
		private readonly static string SRPURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Scriptable_Rendering_Pipeline";
		private readonly static string FunctionsURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Manual#Shader_Functions";
		private readonly static string TemplatesURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/Templates";
		private readonly static string APIURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Shader_Editor/API";

		private readonly static string DiscordURL = "https://discordapp.com/invite/EdrVAP5";
		private readonly static string ForumURL = "https://forum.unity.com/threads/best-tool-asset-store-award-amplify-shader-editor-node-based-shader-creation-tool.430959/";

		private readonly static string SiteURL = "http://amplify.pt/download/";
		private readonly static string StoreURL = "https://assetstore.unity.com/packages/tools/visual-scripting/amplify-shader-editor-68570";

		private readonly static GUIContent SamplesTitle = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Shader Samples"
#else
"着色器示例"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Import samples according to you project rendering pipeline"
#else
"根据您的项目渲染管道导入示例"
#endif
);
		private readonly static GUIContent ResourcesTitle = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Learning Resources"
#else
"学习资源"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Check the online wiki for various topics about how to use ASE with node examples and explanations"
#else
"查看在线维基，了解如何使用ASE以及节点示例和解释的各种主题"
#endif
);
		private readonly static GUIContent CommunityTitle = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Community"
#else
"社区"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Need help? Reach us through our discord server or the official support Unity forum"
#else
"需要帮助？通过我们的不和谐服务器或官方支持Unity论坛联系我们"
#endif
);
		private readonly static GUIContent UpdateTitle = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Latest Update"
#else
"最新更新"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Check the lastest additions, improvements and bug fixes done to ASE"
#else
"查看ASE的最新添加、改进和错误修复"
#endif
);
		private readonly static GUIContent ASETitle = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
"Amplify Shader Editor"
#else
"放大着色器编辑器"
#endif
, 
#if !WB_LANGUAGE_CHINESE
"Are you using the latest version? Now you know"
#else
"您使用的是最新版本吗？现在你知道了"
#endif
);

		private const string OnlineVersionWarning = "Please enable \"Allow downloads over HTTP*\" in Player Settings to access latest version information via Start Screen.";

		Vector2 m_scrollPosition = Vector2.zero;
		Preferences.ShowOption m_startup = Preferences.ShowOption.Never;

		[NonSerialized]
		Texture packageIcon = null;
		[NonSerialized]
		Texture textIcon = null;
		[NonSerialized]
		Texture webIcon = null;

		GUIContent HDRPbutton = null;
		GUIContent URPbutton = null;
		GUIContent BuiltInbutton = null;

		GUIContent Manualbutton = null;
		GUIContent Basicbutton = null;
		GUIContent Beginnerbutton = null;
		GUIContent Nodesbutton = null;
		GUIContent SRPusebutton = null;
		GUIContent Functionsbutton = null;
		GUIContent Templatesbutton = null;
		GUIContent APIbutton = null;

		GUIContent DiscordButton = null;
		GUIContent ForumButton = null;

		GUIContent ASEIcon = null;
		RenderTexture rt;

		[NonSerialized]
		GUIStyle m_buttonStyle = null;
		[NonSerialized]
		GUIStyle m_buttonLeftStyle = null;
		[NonSerialized]
		GUIStyle m_buttonRightStyle = null;
		[NonSerialized]
		GUIStyle m_minibuttonStyle = null;
		[NonSerialized]
		GUIStyle m_labelStyle = null;
		[NonSerialized]
		GUIStyle m_linkStyle = null;

		private ChangeLogInfo m_changeLog;
		private bool m_infoDownloaded = false;
		private string m_newVersion = string.Empty;

		private static Dictionary<int, ASESRPPackageDesc> m_srpSamplePackages = new Dictionary<int, ASESRPPackageDesc>()
		{
			{ ( int )ASESRPBaseline.ASE_SRP_10, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_10, "2edbf4a9b9544774bbef617e92429664", "9da5530d5ebfab24c8ecad68795e720f" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_11, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_11, "2edbf4a9b9544774bbef617e92429664", "9da5530d5ebfab24c8ecad68795e720f" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_12, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_12, "13ab599a7bda4e54fba3e92a13c9580a", "aa102d640b98b5d4781710a3a3dd6983" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_13, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_13, "13ab599a7bda4e54fba3e92a13c9580a", "aa102d640b98b5d4781710a3a3dd6983" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_14, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_14, "f6f268949ccf3f34fa4d18e92501ed82", "7a0bb33169d95ec499136d59cb25918b" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_15, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_15, "69bc3229216b1504ea3e28b5820bbb0d", "641c955d37d2fac4f87e00ac5c9d9bd8" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_16, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_16, "4f665a06c5a2aa5499fa1c79ac058999", "2690f45490c175045bbdc63395bf6278" ) },
			{ ( int )ASESRPBaseline.ASE_SRP_17, new ASESRPPackageDesc( ASESRPBaseline.ASE_SRP_17, "47fc5ccecd261894994c1e9e827cf553", "f42c2bc4dab4723429b0d30b635c3035" ) },
		};

		private void OnEnable()
		{
			rt = new RenderTexture( 16, 16, 0 );
			rt.Create();

			m_startup = (Preferences.ShowOption)EditorPrefs.GetInt( Preferences.PrefStartUp, 0 );

			if( textIcon == null )
			{
				Texture icon = EditorGUIUtility.IconContent( "TextAsset Icon" ).image;
				var cache = RenderTexture.active;
				RenderTexture.active = rt;
				Graphics.Blit( icon, rt );
				RenderTexture.active = cache;
				textIcon = rt;

				Manualbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Manual"
#else
"手册"
#endif
, textIcon );
				Basicbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Basic use tutorials"
#else
"基本使用教程"
#endif
, textIcon );
				Beginnerbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Beginner Series"
#else
"初学者系列"
#endif
, textIcon );
				Nodesbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Node List"
#else
"节点列表"
#endif
, textIcon );
				SRPusebutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" SRP HDRP/URP use"
#else
"SRP HDRP/URP使用"
#endif
, textIcon );
				Functionsbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Shader Functions"
#else
"着色器功能"
#endif
, textIcon );
				Templatesbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Shader Templates"
#else
"着色器模板"
#endif
, textIcon );
				APIbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Node API"
#else
"节点API"
#endif
, textIcon );
			}

			if( packageIcon == null )
			{
				packageIcon = EditorGUIUtility.IconContent( "BuildSettings.Editor.Small" ).image;
				HDRPbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" HDRP Samples"
#else
"HDRP样品"
#endif
, packageIcon );
				URPbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" URP Samples"
#else
"URP样品"
#endif
, packageIcon );
				BuiltInbutton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Built-In Samples"
#else
"内置样品"
#endif
, packageIcon );
			}

			if( webIcon == null )
			{
				webIcon = EditorGUIUtility.IconContent( "BuildSettings.Web.Small" ).image;
				DiscordButton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Discord"
#else
"不和谐"
#endif
, webIcon );
				ForumButton = new GUIContent( 
#if !WB_LANGUAGE_CHINESE
" Unity Forum"
#else
"团结论坛"
#endif
, webIcon );
			}

			if( m_changeLog == null )
			{
				var changelog = AssetDatabase.LoadAssetAtPath<TextAsset>( AssetDatabase.GUIDToAssetPath( ChangeLogGUID ) );
				string lastUpdate = string.Empty;
				if(changelog != null )
				{
					int oldestReleaseIndex = changelog.text.LastIndexOf( string.Format( "v{0}.{1}.{2}", VersionInfo.Major, VersionInfo.Minor, VersionInfo.Release ) );

					lastUpdate = changelog.text.Substring( 0, changelog.text.IndexOf( "\nv", oldestReleaseIndex + 25 ) );
					lastUpdate = lastUpdate.Replace( "* ", "\u2022 " );
				}
				m_changeLog = new ChangeLogInfo( VersionInfo.FullNumber, lastUpdate );
			}

			if( ASEIcon == null )
			{
				ASEIcon = new GUIContent( AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( IconGUID ) ) );
			}
		}

		private void OnDisable()
		{
			if( rt != null )
			{
				rt.Release();
				DestroyImmediate( rt );
			}
		}

		public void OnGUI()
		{
			if( !m_infoDownloaded )
			{
				m_infoDownloaded = true;

				StartBackgroundTask( StartRequest( ChangelogURL, () =>
				{
					var temp = ChangeLogInfo.CreateFromJSON( www.downloadHandler.text );
					if( temp != null && temp.Version >= m_changeLog.Version )
					{
						m_changeLog = temp;
					}

					int version = m_changeLog.Version;
					int major = version / 10000;
					int minor = version / 1000 - major * 10;
					int release = version / 100 - ( version / 1000 ) * 10;
					int revision = version - ( version / 100 ) * 100;

					m_newVersion = major + "." + minor + "." + release + ( revision > 0 ? "." + revision : "" );

					Repaint();
				} ) );
			}

			if( m_buttonStyle == null )
			{
				m_buttonStyle = new GUIStyle( GUI.skin.button );
				m_buttonStyle.alignment = TextAnchor.MiddleLeft;
			}

			if( m_buttonLeftStyle == null )
			{
				m_buttonLeftStyle = new GUIStyle( "ButtonLeft" );
				m_buttonLeftStyle.alignment = TextAnchor.MiddleLeft;
				m_buttonLeftStyle.margin = m_buttonStyle.margin;
				m_buttonLeftStyle.margin.right = 0;
			}

			if( m_buttonRightStyle == null )
			{
				m_buttonRightStyle = new GUIStyle( "ButtonRight" );
				m_buttonRightStyle.alignment = TextAnchor.MiddleLeft;
				m_buttonRightStyle.margin = m_buttonStyle.margin;
				m_buttonRightStyle.margin.left = 0;
			}

			if( m_minibuttonStyle == null )
			{
				m_minibuttonStyle = new GUIStyle( "MiniButton" );
				m_minibuttonStyle.alignment = TextAnchor.MiddleLeft;
				m_minibuttonStyle.margin = m_buttonStyle.margin;
				m_minibuttonStyle.margin.left = 20;
				m_minibuttonStyle.normal.textColor = m_buttonStyle.normal.textColor;
				m_minibuttonStyle.hover.textColor = m_buttonStyle.hover.textColor;
			}

			if( m_labelStyle == null )
			{
				m_labelStyle = new GUIStyle( "BoldLabel" );
				m_labelStyle.margin = new RectOffset( 4, 4, 4, 4 );
				m_labelStyle.padding = new RectOffset( 2, 2, 2, 2 );
				m_labelStyle.fontSize = 13;
			}

			if( m_linkStyle == null )
			{
				var inv = AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( "1004d06b4b28f5943abdf2313a22790a" ) ); 
				m_linkStyle = new GUIStyle();
				m_linkStyle.normal.textColor = new Color( 0.2980392f, 0.4901961f, 1f );
				m_linkStyle.hover.textColor = Color.white;
				m_linkStyle.active.textColor = Color.grey;
				m_linkStyle.margin.top = 3;
				m_linkStyle.margin.bottom = 2;
				m_linkStyle.hover.background = inv;
				m_linkStyle.active.background = inv;
			}

			EditorGUILayout.BeginHorizontal( GUIStyle.none, GUILayout.ExpandWidth( true ) );
			{
				
				EditorGUILayout.BeginVertical( GUILayout.Width( 175 ) );
				{
					GUILayout.Label( SamplesTitle, m_labelStyle );
					EditorGUILayout.BeginHorizontal();
					if( GUILayout.Button( HDRPbutton, m_buttonLeftStyle ) )
						ImportSample( HDRPbutton.text, TemplateSRPType.HDRP );

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					if( GUILayout.Button( URPbutton, m_buttonLeftStyle ) )
						ImportSample( URPbutton.text, TemplateSRPType.URP );

					EditorGUILayout.EndHorizontal();
					if( GUILayout.Button( BuiltInbutton, m_buttonStyle ) )
						ImportSample( BuiltInbutton.text, TemplateSRPType.BiRP );

					GUILayout.Space( 10 );

					GUILayout.Label( ResourcesTitle, m_labelStyle );
					if( GUILayout.Button( Manualbutton, m_buttonStyle ) )
						Application.OpenURL( ManualURL );

					if( GUILayout.Button( Basicbutton, m_buttonStyle ) )
						Application.OpenURL( BasicURL );

					if( GUILayout.Button( Beginnerbutton, m_buttonStyle ) )
						Application.OpenURL( BeginnerURL );

					if( GUILayout.Button( Nodesbutton, m_buttonStyle ) )
						Application.OpenURL( NodesURL );

					if( GUILayout.Button( SRPusebutton, m_buttonStyle ) )
						Application.OpenURL( SRPURL );

					if( GUILayout.Button( Functionsbutton, m_buttonStyle ) )
						Application.OpenURL( FunctionsURL );

					if( GUILayout.Button( Templatesbutton, m_buttonStyle ) )
						Application.OpenURL( TemplatesURL );

					if( GUILayout.Button( APIbutton, m_buttonStyle ) )
						Application.OpenURL( APIURL );
				}
				EditorGUILayout.EndVertical();

				
				EditorGUILayout.BeginVertical( GUILayout.Width( 650 - 175 - 9 ), GUILayout.ExpandHeight( true ) );
				{
					GUILayout.Label( CommunityTitle, m_labelStyle );
					EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
					{
						if( GUILayout.Button( DiscordButton, GUILayout.ExpandWidth( true ) ) )
						{
							Application.OpenURL( DiscordURL );
						}
						if( GUILayout.Button( ForumButton, GUILayout.ExpandWidth( true ) ) )
						{
							Application.OpenURL( ForumURL );
						}
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Label( UpdateTitle, m_labelStyle );
					m_scrollPosition = GUILayout.BeginScrollView( m_scrollPosition, "ProgressBarBack", GUILayout.ExpandHeight( true ), GUILayout.ExpandWidth( true ) );
					GUILayout.Label( m_changeLog.LastUpdate, "WordWrappedMiniLabel", GUILayout.ExpandHeight( true ) );
					GUILayout.EndScrollView();

					EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
					{
						EditorGUILayout.BeginVertical();
						GUILayout.Label( ASETitle, m_labelStyle );

						GUILayout.Label( "Installed Version: " + VersionInfo.StaticToString() );

						if( m_changeLog.Version > VersionInfo.FullNumber )
						{
							var cache = GUI.color;
							GUI.color = Color.red;
							GUILayout.Label( "New version available: " + m_newVersion, "BoldLabel" );
							GUI.color = cache;
						}
						else
						{
							var cache = GUI.color;
							GUI.color = Color.green;
							GUILayout.Label( "You are using the latest version", "BoldLabel" );
							GUI.color = cache;
						}

						EditorGUILayout.BeginHorizontal();
						GUILayout.Label( "Download links:" );
						if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Amplify"
#else
"放大"
#endif
, m_linkStyle ) )
							Application.OpenURL( SiteURL );
						GUILayout.Label( "-" );
						if( GUILayout.Button( 
#if !WB_LANGUAGE_CHINESE
"Asset Store"
#else
"资产存储"
#endif
, m_linkStyle ) )
							Application.OpenURL( StoreURL );
						EditorGUILayout.EndHorizontal();
						GUILayout.Space( 7 );
						EditorGUILayout.EndVertical();

						GUILayout.FlexibleSpace();
						EditorGUILayout.BeginVertical();
						GUILayout.Space( 7 );
						GUILayout.Label( ASEIcon );
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal( "ProjectBrowserBottomBarBg", GUILayout.ExpandWidth( true ), GUILayout.Height(22) );
			{
				GUILayout.FlexibleSpace();
				EditorGUI.BeginChangeCheck();
				var cache = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 100;
				m_startup = (Preferences.ShowOption)EditorGUILayout.EnumPopup( "Show At Startup", m_startup, GUILayout.Width( 220 ) );
				EditorGUIUtility.labelWidth = cache;
				if( EditorGUI.EndChangeCheck() )
				{
					EditorPrefs.SetInt( Preferences.PrefStartUp, (int)m_startup );
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		void ImportSample( string pipeline, TemplateSRPType srpType )
		{
			if( EditorUtility.DisplayDialog( "Import Sample", "This will import the samples for" + pipeline.Replace( " Samples", "" ) + ", please make sure the pipeline is properly installed and/or selected before importing the samples.\n\nContinue?", "Yes", "No" ) )
			{
				AssetDatabase.ImportPackage( AssetDatabase.GUIDToAssetPath( ResourcesGUID ), false );

				switch ( srpType )
				{
					case TemplateSRPType.BiRP:
					{
						AssetDatabase.ImportPackage( AssetDatabase.GUIDToAssetPath( BuiltInGUID ), false );
						break;
					}
					case TemplateSRPType.URP:
					{
						if ( m_srpSamplePackages.TryGetValue( ( int )ASEPackageManagerHelper.CurrentURPBaseline, out ASESRPPackageDesc desc ) )
						{
							string path = AssetDatabase.GUIDToAssetPath( desc.guidURP );
							if ( !string.IsNullOrEmpty( path ) )
							{
								AssetDatabase.ImportPackage( AssetDatabase.GUIDToAssetPath( UniversalGUID ), false );
								AssetDatabase.ImportPackage( path, false );
							}
						}
						break;
					}
					case TemplateSRPType.HDRP:
					{
						if ( m_srpSamplePackages.TryGetValue( ( int )ASEPackageManagerHelper.CurrentHDRPBaseline, out ASESRPPackageDesc desc ) )
						{
							string path = AssetDatabase.GUIDToAssetPath( desc.guidHDRP );
							if ( !string.IsNullOrEmpty( path ) )
							{
								AssetDatabase.ImportPackage( AssetDatabase.GUIDToAssetPath( HighDefinitionGUID ), false );
								AssetDatabase.ImportPackage( path, false );
							}
						}
						break;
					}
					default:
					{
						
						break;
					}

				}
			}
		}

		UnityWebRequest www;

		IEnumerator StartRequest( string url, Action success = null )
		{
			using( www = UnityWebRequest.Get( url ) )
			{
				yield return www.SendWebRequest();

				while( www.isDone == false )
					yield return null;

				if( success != null )
					success();
			}
		}

		public static void StartBackgroundTask( IEnumerator update, Action end = null )
		{
			EditorApplication.CallbackFunction closureCallback = null;

			closureCallback = () =>
			{
				try
				{
					if( update.MoveNext() == false )
					{
						if( end != null )
							end();
						EditorApplication.update -= closureCallback;
					}
				}
				catch( Exception ex )
				{
					if( end != null )
						end();
					Debug.LogException( ex );
					EditorApplication.update -= closureCallback;
				}
			};

			EditorApplication.update += closureCallback;
		}
	}

	[Serializable]
	internal class ChangeLogInfo
	{
		public int Version;
		public string LastUpdate;

		public static ChangeLogInfo CreateFromJSON( string jsonString )
		{
			return JsonUtility.FromJson<ChangeLogInfo>( jsonString );
		}

		public ChangeLogInfo( int version, string lastUpdate )
		{
			Version = version;
			LastUpdate = lastUpdate;
		}
	}
}
