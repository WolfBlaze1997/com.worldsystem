using UnityEngine;


namespace MobileController
{
    public class FPSController : MonoBehaviour
    {
        [Header("Controller Behaviour")]
        [Min(0)] public float moveSpeed = 8.0f;
        [Min(0)] public float smoothMovement = 0.025f;
        public float normalHeight;
        public float crouchHeight;
        public float throwForce;
        [Space(15)]

        [Header("References")]
        public Analog analog;
        public Swap swap;
        public GameObject type_1;
        public GameObject type_2;
        public GameObject type_3;
        public Animator gunAnim;


        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 moveDirection;

        private bool isCrouch = false;


        /**/
        


        #region Setup 

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            cameraTransform = Camera.main.transform;
        }

        #endregion


        #region Controller

        private void Update()
        {
            // handle movement
            Vector2 input = analog.GetStickPosition();
            Vector3 inputDir = cameraTransform.forward * input.y + cameraTransform.right * input.x;

            // apply gravity
            if (characterController.isGrounded) moveDirection = inputDir * moveSpeed;
            moveDirection.y -= 9.81f * Time.deltaTime;
            characterController.Move(moveDirection * Time.deltaTime);
        }


        public void GunSwap()
        {
            SwapContent _content = swap.GetCurrentItem();

            if ((int)_content.contentType == 0)
            {
                type_1.SetActive(true);
                type_2.SetActive(false);
                type_3.SetActive(false);
            }
            else if ((int)_content.contentType == 1)
            {
                type_1.SetActive(false);
                type_2.SetActive(true);
                type_3.SetActive(false);
            }
            else if ((int)_content.contentType == 2)
            {
                type_1.SetActive(false);
                type_2.SetActive(false);
                type_3.SetActive(true);
            }

            gunAnim.Play("Idle");
            gunAnim.Play("Swap");
        }


        public void ToggleHeight()
        {
            isCrouch = !isCrouch;

            if (!isCrouch) characterController.height = normalHeight;
            else characterController.height = crouchHeight;
        }


        public void ThrowObject(GameObject _obj)
        {
            // instantiate the throwable object
            GameObject throwable = Instantiate(_obj, Camera.main.transform.position + this.transform.forward, Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));

            // add force
            Rigidbody rb = throwable.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 throwDirection = Camera.main.transform.forward;
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }
        }

        #endregion
    }
}