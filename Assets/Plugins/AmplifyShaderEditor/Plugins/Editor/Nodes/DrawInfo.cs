


using UnityEngine;
namespace AmplifyShaderEditor
{
	public class DrawInfo
	{
		public Rect TransformedCameraArea;
		public Rect CameraArea;
		public Vector2 MousePosition;
		public Vector2 CameraOffset;
		public float InvertedZoom;
		public bool LeftMouseButtonPressed;
		public EventType CurrentEventType;
		public Vector2 TransformedMousePos;
		public bool ZoomChanged;
	}
}
