using UnityEngine;


namespace MobileController
{
    public class CallExplosion : MonoBehaviour
    {
        [Header("References")]
        public GameObject particleExplosion;


        /**/


        private void OnCollisionEnter(Collision collision)
        {
            // instanciate particles
            GameObject.Instantiate(particleExplosion, this.transform.position, Quaternion.identity);
            GameObject.Destroy(this.gameObject);
        }
    }
}