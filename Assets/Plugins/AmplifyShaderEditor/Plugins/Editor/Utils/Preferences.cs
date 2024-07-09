// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmplifyShaderEditor
{
	public class Preferences
	{
		public enum ShowOption
		{
			Always = 0,
			OnNewVersion = 1,
			Never = 2
		}

		private static readonly GUIContent StartUp = new GUIContent( "Show start screen on Unity launch", "You can set if you want to see the start screen everytime Unity launchs, only just when there's a new version available or never." );
		public static readonly string PrefStartUp = "ASELastSession";
		public static ShowOption GlobalStartUp { get; private set; } = 0;

		private static readonly GUIContent AlwaysSnapToGrid = new GUIContent( "Always Snap to Grid", "Always snap to grid when dragging nodes around, instead of using control." );
		public static readonly string PrefAlwaysSnapToGrid = "ASEAlwaysSnapToGrid";
		public static bool GlobalAlwaysSnapToGrid { get; private set; } = false;

		private static readonly GUIContent EnableUndo = new GUIContent( "Enable Undo (unstable)", "Enables undo for actions within the shader graph canvas. Currently unstable, use with caution." );
		public static readonly string PrefEnableUndo = "ASEEnableUndo";
		public static bool GlobalEnableUndo { get; private set; } = false;

		private static readonly GUIContent AutoSRP = new GUIContent( "Auto import SRP shader templates", "By default Amplify Shader Editor checks for your SRP version and automatically imports the correct corresponding shader templates.\nTurn this OFF if you prefer to import them manually." );
		public static readonly string PrefAutoSRP = "ASEAutoSRP";
		public static bool GlobalAutoSRP { get; private set; } = true;

		private static readonly GUIContent DefineSymbol = new GUIContent( "Add Amplify Shader Editor define symbol", "Turning it OFF will disable the automatic insertion of the define symbol and remove it from the list while turning it ON will do the opposite.\nThis is used for compatibility with other plugins, if you are not sure if you need this leave it ON." );
		public static readonly string PrefDefineSymbol = "ASEDefineSymbol";
		public static bool GlobalDefineSymbol { get; private set; } = true;

		private static readonly GUIContent ClearLog = new GUIContent( "Clear Log on Update", "Clears the previously generated log each time the Update button is pressed" );
		public static readonly string PrefClearLog = "ASEClearLog";
		public static bool GlobalClearLog { get; private set; } = true;

		private static readonly GUIContent LogShaderCompile = new GUIContent( "Log Shader Compile", "Log message to console when a shader compilation is finished" );
		public static readonly string PrefLogShaderCompile = "ASELogShaderCompile";
		public static bool GlobalLogShaderCompile { get; private set; } = false;

		private static readonly GUIContent LogBatchCompile = new GUIContent( "Log Batch Compile", "Log message to console when a batch compilation is finished" );
		public static readonly string PrefLogBatchCompile = "ASELogBatchCompile";
		public static bool GlobalLogBatchCompile { get; private set; } = false;

		private static readonly GUIContent UpdateOnSceneSave = new GUIContent( "Update on Scene save (Ctrl+S)", "ASE is aware of Ctrl+S and will use it to save shader" );
		public static readonly string PrefUpdateOnSceneSave = "ASEUpdateOnSceneSave";
		public static bool GlobalUpdateOnSceneSave { get; private set; } = true;

		private static readonly GUIContent DisablePreviews = new GUIContent( "Disable Node Previews", "Disable preview on nodes from being updated to boost up performance on large graphs" );
		public static readonly string PrefDisablePreviews = "ASEActivatePreviews";
		public static bool GlobalDisablePreviews { get; private set; } = false;

		private static readonly GUIContent ForceTemplateMinShaderModel = new GUIContent( "Force Template Min. Shader Model", "If active, when loading a shader its shader model will be replaced by the one specified in template if what is loaded is below the one set over the template." );
		public static readonly string PrefForceTemplateMinShaderModel = "ASEForceTemplateMinShaderModel";
		public static bool GlobalForceTemplateMinShaderModel { get; private set; } = true;

		private static readonly GUIContent ForceTemplateInlineProperties = new GUIContent( "Force Template Inline Properties", "If active, defaults all inline properties to template values." );
		public static readonly string PrefForceTemplateInlineProperties = "ASEForceTemplateInlineProperties";
		public static bool GlobalForceTemplateInlineProperties { get; private set; } = false;


		[SettingsProvider]
		public static SettingsProvider ImpostorsSettings()
		{
			var provider = new SettingsProvider( "Preferences/Amplify Shader Editor", SettingsScope.User )
			{
				guiHandler = ( string searchContext ) =>
				{
					PreferencesGUI();
				},

				keywords = new HashSet<string>( new[] { "start", "screen", "import", "shader", "templates", "macros", "macros", "define", "symbol" } ),

			};
			return provider;
		}

		private static void ResetSettings()
		{
			IOUtils.SetAmplifyDefineSymbolOnBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
			UIUtils.ActivatePreviews( true );

			EditorPrefs.DeleteKey( PrefStartUp );
			EditorPrefs.DeleteKey( PrefAlwaysSnapToGrid );
			EditorPrefs.DeleteKey( PrefEnableUndo );
			EditorPrefs.DeleteKey( PrefAutoSRP );
			EditorPrefs.DeleteKey( PrefDefineSymbol );
			EditorPrefs.DeleteKey( PrefClearLog );
			EditorPrefs.DeleteKey( PrefLogShaderCompile );
			EditorPrefs.DeleteKey( PrefLogBatchCompile );
			EditorPrefs.DeleteKey( PrefUpdateOnSceneSave );
			EditorPrefs.DeleteKey( PrefDisablePreviews );
			EditorPrefs.DeleteKey( PrefForceTemplateMinShaderModel );
			EditorPrefs.DeleteKey( PrefForceTemplateInlineProperties );

			LoadSettings();
		}

		private static void LoadSettings()
		{
			GlobalStartUp = ( ShowOption )EditorPrefs.GetInt( PrefStartUp, 0 );
			GlobalAlwaysSnapToGrid = EditorPrefs.GetBool( PrefAlwaysSnapToGrid, false );
			GlobalEnableUndo = EditorPrefs.GetBool( PrefEnableUndo, false );
			GlobalAutoSRP = EditorPrefs.GetBool( PrefAutoSRP, true );
			GlobalDefineSymbol = EditorPrefs.GetBool( PrefDefineSymbol, true );
			GlobalClearLog = EditorPrefs.GetBool( PrefClearLog, true );
			GlobalLogShaderCompile = EditorPrefs.GetBool( PrefLogShaderCompile, false );
			GlobalLogBatchCompile = EditorPrefs.GetBool( PrefLogBatchCompile, false );
			GlobalUpdateOnSceneSave = EditorPrefs.GetBool( PrefUpdateOnSceneSave, true );
			GlobalDisablePreviews = EditorPrefs.GetBool( PrefDisablePreviews, false );
			GlobalForceTemplateMinShaderModel = EditorPrefs.GetBool( PrefForceTemplateMinShaderModel, true );
			GlobalForceTemplateInlineProperties = EditorPrefs.GetBool( PrefForceTemplateInlineProperties, false );
		}

		private static void SaveSettings()
		{
			bool prevDefineSymbol = EditorPrefs.GetBool( PrefDefineSymbol, true );
			bool prevDisablePreviews = EditorPrefs.GetBool( PrefDisablePreviews, false );

			if ( GlobalDefineSymbol != prevDefineSymbol )
			{
				if ( GlobalDefineSymbol )
				{
					IOUtils.SetAmplifyDefineSymbolOnBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
				}
				else
				{
					IOUtils.RemoveAmplifyDefineSymbolOnBuildTargetGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
				}
			}

			if ( GlobalDisablePreviews != prevDisablePreviews )
			{
				UIUtils.ActivatePreviews( !GlobalDisablePreviews );
			}

			EditorPrefs.SetInt( PrefStartUp, ( int )GlobalStartUp );
			EditorPrefs.SetBool( PrefAlwaysSnapToGrid, GlobalAlwaysSnapToGrid );
			EditorPrefs.SetBool( PrefEnableUndo, GlobalEnableUndo );
			EditorPrefs.SetBool( PrefAutoSRP, GlobalAutoSRP );
			EditorPrefs.SetBool( PrefDefineSymbol, GlobalDefineSymbol );
			EditorPrefs.SetBool( PrefClearLog, GlobalClearLog );
			EditorPrefs.SetBool( PrefLogShaderCompile, GlobalLogShaderCompile );
			EditorPrefs.SetBool( PrefLogBatchCompile, GlobalLogBatchCompile );
			EditorPrefs.SetBool( PrefUpdateOnSceneSave, GlobalUpdateOnSceneSave );
			EditorPrefs.SetBool( PrefDisablePreviews, GlobalDisablePreviews );
			EditorPrefs.SetBool( PrefForceTemplateMinShaderModel, GlobalForceTemplateMinShaderModel );
			EditorPrefs.SetBool( PrefForceTemplateInlineProperties, GlobalForceTemplateInlineProperties );
		}

		[InitializeOnLoadMethod]
		public static void Initialize()
		{
			LoadSettings();
		}

		public static void PreferencesGUI()
		{
			var cache = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 250;

			EditorGUI.BeginChangeCheck();
			{
				GlobalStartUp = ( ShowOption )EditorGUILayout.EnumPopup( StartUp, GlobalStartUp );
				GlobalAlwaysSnapToGrid = EditorGUILayout.Toggle( AlwaysSnapToGrid, GlobalAlwaysSnapToGrid );
				GlobalEnableUndo = EditorGUILayout.Toggle( EnableUndo, GlobalEnableUndo );
				GlobalAutoSRP = EditorGUILayout.Toggle( AutoSRP, GlobalAutoSRP );
				GlobalDefineSymbol = EditorGUILayout.Toggle( DefineSymbol, GlobalDefineSymbol );
				GlobalClearLog = EditorGUILayout.Toggle( ClearLog, GlobalClearLog );
				GlobalLogShaderCompile = EditorGUILayout.Toggle( LogShaderCompile, GlobalLogShaderCompile );
				GlobalLogBatchCompile = EditorGUILayout.Toggle( LogBatchCompile, GlobalLogBatchCompile );
				GlobalUpdateOnSceneSave = EditorGUILayout.Toggle( UpdateOnSceneSave, GlobalUpdateOnSceneSave );
				GlobalDisablePreviews = EditorGUILayout.Toggle( DisablePreviews, GlobalDisablePreviews );
				GlobalForceTemplateMinShaderModel = EditorGUILayout.Toggle( ForceTemplateMinShaderModel, GlobalForceTemplateMinShaderModel );
				GlobalForceTemplateInlineProperties = EditorGUILayout.Toggle( ForceTemplateInlineProperties, GlobalForceTemplateInlineProperties );
			}
			if ( EditorGUI.EndChangeCheck() )
			{
				SaveSettings();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if( GUILayout.Button( "Reset and Forget All" ) )
			{
				ResetSettings();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = cache;
		}
	}
}
