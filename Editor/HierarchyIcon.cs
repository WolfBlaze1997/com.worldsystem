using System;
using UnityEditor;
using UnityEngine;

namespace WorldSystem.Editor
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class HierarchyIcon
    {
        // 层级窗口项回调
        private static readonly EditorApplication.HierarchyWindowItemCallback hiearchyItemCallback = DrawHierarchyIcon;
        
        
        private static Texture2D hierarchyEventIcon;
        private static Texture2D HierarchyEventIcon
        {
            get
            {
                if (hierarchyEventIcon == null)
                {
                    hierarchyEventIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.worldsystem//Textures/Icon/WorldManager-icon.png");
                }
                return hierarchyEventIcon;
            }
        }
        
        /// <summary>
        /// 静态构造 Packages/com.worldsystem/
        /// </summary>
        static HierarchyIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI = 
                (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, hiearchyItemCallback);
        }

        
        // 绘制icon方法
        private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null && gameObject.name == "WorldManager")
            {
                // 设置icon的位置与尺寸（Hierarchy窗口的左上角是起点）
                Rect rect = new Rect(selectionRect.x, selectionRect.y, 16f, 16f);
                // Rect rect = new Rect(selectionRect.x + selectionRect.width - 16f, selectionRect.y, 16f, 16f);
                
                // 画icon
                GUI.DrawTexture(rect, HierarchyEventIcon);
            }
        }
    }
#endif
    
}