using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")] 
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dropKey = KeyCode.S;

        // Input events
        public System.Action<float> OnHorizontalInput;
        public System.Action OnJumpInput;
        public System.Action OnDropInput;

        private void Update()
        {
            OnHorizontalInput?.Invoke(Input.GetAxis("Horizontal"));

            if (Input.GetKeyDown(jumpKey)) OnJumpInput?.Invoke();
            if (Input.GetKeyDown(dropKey)) OnDropInput?.Invoke();
        }
    }
}