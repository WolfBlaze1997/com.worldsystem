using System;
using UnityEditor;
using UnityEngine;

namespace WorldSystem.Editor
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class HierarchyIcon
    {
        private static readonly EditorApplication.HierarchyWindowItemCallback HiearchyItemCallback = DrawHierarchyIcon;
        
        private static Texture2D _hierarchyEventIcon;
        private static Texture2D HierarchyEventIcon
        {
            get
            {
                if (_hierarchyEventIcon == null)
                {
                    _hierarchyEventIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem//Textures/Icon/WorldManager-icon.png");
                }
                return _hierarchyEventIcon;
            }
        }
        
        static HierarchyIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI = 
                (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, HiearchyItemCallback);
        }
        
        /// <summary>
        /// 绘制图标
        /// </summary>
        private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null && gameObject.name == "WorldManager")
            {
                Rect rect = new Rect(selectionRect.x, selectionRect.y, 16f, 16f);
                GUI.DrawTexture(rect, HierarchyEventIcon);
            }
        }
    }
#endif
    
}