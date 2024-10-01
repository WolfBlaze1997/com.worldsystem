using UnityEngine;


namespace MobileController
{
    public class TouchDisplay : MonoBehaviour
    {
        [Header("Behaviour")]
        public bool displayCursorOnMove = true;

        [Header("References")]
        public RectTransform touchRect;


        private bool isFollowing = false;
        
        private bool isMobile = false;
        private bool checkedDevice = false;


        /**/


        #region Setup

        private void CheckDevice()
        {
            isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
            checkedDevice = true;
        }

        #endregion


        #region Display

        public void ShowTouch()
        {
            // check if is running on mobile
            if(!checkedDevice) CheckDevice();

            if(!isMobile)
            {
                MoveTouch();

                touchRect.gameObject.SetActive(true);
                Cursor.visible = displayCursorOnMove;

                isFollowing = true;
            }
        }


        public void HideTouch()
        {
            // check if is running on mobile
            if (!checkedDevice) CheckDevice();

            if (!isMobile)
            {
                touchRect.gameObject.SetActive(false);
                Cursor.visible = true;

                isFollowing = false;
            }
        }

        #endregion


        #region Move

        private void Update()
        {
            if (isFollowing && Input.GetKey(KeyCode.Mouse0)) MoveTouch();
            else HideTouch();
        }


        private void MoveTouch()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -Camera.main.transform.position.z;
            touchRect.position = mousePosition;
        }

        #endregion
    }
}