using UnityEngine;


namespace MobileController
{
    public class CallShake : MonoBehaviour
    {
        [Header("References")]
        public FPSCamera FPScamera;


        /**/


        public void ShakeCamera()
        {
            // call camera shake
            FPScamera.ShakeCamera();
        }
    }
}