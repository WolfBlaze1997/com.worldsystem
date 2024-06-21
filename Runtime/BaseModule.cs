using UnityEngine;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class BaseModule : MonoBehaviour
    {
#if UNITY_EDITOR
        protected virtual void Awake()
        {
            HideFlagToggle();
        }

        protected void HideFlagToggle()
        {
            hideFlags = (WorldManager.Instance?.hideFlagToggle ?? false) ? HideFlags.None : HideFlags.HideInInspector;

            if (transform.parent != null && transform.parent.gameObject.GetComponent<WorldManager>() != null)
                gameObject.hideFlags = (WorldManager.Instance?.hideFlagToggle ?? false)
                    ? HideFlags.None
                    : HideFlags.HideInHierarchy;
            
        }

        protected virtual void DrawGizmos()
        {
        }
        
        protected virtual void DrawGizmosSelected()
        {
        }
#endif
    }
}