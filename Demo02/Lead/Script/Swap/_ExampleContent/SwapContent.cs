using UnityEngine;


namespace MobileController
{
    [CreateAssetMenu(fileName = "ContentType", menuName = "MobileController/ContentType", order = 1)]
    public class SwapContent : ScriptableObject
    {
        public enum ContentType { type_0, type_1, type_2 }

        [Header("Properties")]
        public ContentType contentType;
        public Sprite contentImage;
    }
}