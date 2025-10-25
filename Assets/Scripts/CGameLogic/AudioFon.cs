using UnityEngine;

namespace CGL
{
    public class AudioFon : MonoBehaviour
    {
        [HideInInspector] public AudioSource m_Source;
        public static AudioFon af = null;

        private void Awake()
        {
            if (af == null) af = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            m_Source = GetComponent<AudioSource>();
        }
    }
}