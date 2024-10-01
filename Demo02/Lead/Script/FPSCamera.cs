using System.Collections;
using UnityEngine;


namespace MobileController
{
    public class FPSCamera : MonoBehaviour
    {
        [Header("Camera Behaviour")]
        public float rotationSpeed = 5.0f;
        public float smoothness = 0.1f;
        public float shakeIntensity = 0.1f;
        public float shakeDuration = 0.5f;
        [Space(5)]
        public Vector2 mobileMoveMultiplier = new Vector2(3f, 1.3f);
        public Vector2 editorMoveMultiplier = Vector2.one;
        [Space(15)]

        [Header("References")]
        public TouchDrag touchDrag;


        private Vector2 rotationInput = Vector2.zero;
        private Vector3 currentRotation;
        private Vector3 targetRotation;

        private Vector2 moveMultiplier;


        /**/


        #region Setup

        private void Start()
        {
            // check device
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) moveMultiplier = mobileMoveMultiplier;
            else moveMultiplier = editorMoveMultiplier;

            // set rotation
            currentRotation = transform.localEulerAngles;
            targetRotation = currentRotation;
        }

        #endregion


        #region Move Camera

        private void Update()
        {
            rotationInput = touchDrag.GetTouchDragMovement();

            // calculate new rotation based on input
            targetRotation += new Vector3(rotationInput.y * moveMultiplier.y, -rotationInput.x * moveMultiplier.x, 0) * rotationSpeed;
            targetRotation.x = Mathf.Clamp(targetRotation.x, -90f, 90f);

            // interpolate to new rotation
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * smoothness);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }

        #endregion


        #region Shake

        public void ShakeCamera()
        {
            // start shake coroutine
            StartCoroutine(Shake());
        }


        private IEnumerator Shake()
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                float xOffset = Random.Range(-1f, 1f) * shakeIntensity;
                float yOffset = Random.Range(-1f, 1f) * shakeIntensity;

                transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = originalPosition;
        }

        #endregion
    }
}