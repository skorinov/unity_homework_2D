using UnityEngine;

namespace Utilities
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        
        [Header("Singleton Settings")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            OnSingletonDestroy();
        }

        protected virtual void OnSingletonAwake() { }
        protected virtual void OnSingletonDestroy() { }
    }
}