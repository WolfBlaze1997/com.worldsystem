using UnityEngine;
using UnityEngine.UI;


namespace MobileController
{
    [RequireComponent(typeof(Analog))]
    public class Heatmap : MonoBehaviour
    {
        [Header("Behaviour")]
        [Range(0f, 1f)]
        public float transparencyThreshold = 0.35f;
        [Range(0f, 1f)]
        public float maxAlpha = 0.9f;
        public Gradient colorGradient;
        [Space(15)]

        [Header("References")]
        public RectTransform stickTarget;
        public RectTransform heatmapParent;
        public Image heatmapImage;


        private Analog _analogStick;


        /**/


        #region Setup 

        private void Awake()
        {
            _analogStick = this.GetComponent<Analog>();
        }

        #endregion


        #region Heatmap

        private void Update()
        {
            // set heatmapParent rotation
            var dir = stickTarget.position - heatmapParent.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            heatmapParent.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // set heatmapParent transparency
            float heatStep = 0f;
            float heatMagnitude = _analogStick.GetStickPosition().magnitude;

            // assign step
            if (_analogStick.GetStickPosition().magnitude > transparencyThreshold) heatStep = (heatMagnitude - transparencyThreshold) / (1f - transparencyThreshold) * maxAlpha;

            // set color
            Color heatmapColor = colorGradient.Evaluate(heatStep);
            heatmapColor.a = heatStep;
            heatmapImage.color = heatmapColor;
        }

        #endregion

    }
}