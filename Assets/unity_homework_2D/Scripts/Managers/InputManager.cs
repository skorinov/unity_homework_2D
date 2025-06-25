using Constants;
using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dropKey = KeyCode.S;

        public System.Action<float> OnHorizontalInput;
        public System.Action OnJumpInput;
        public System.Action OnDropInput;
        
        public System.Action OnUINavigateUp;
        public System.Action OnUINavigateDown;
        public System.Action OnUINavigateLeft;
        public System.Action OnUINavigateRight;
        public System.Action OnUIConfirm;
        public System.Action OnUICancel;

        private bool _gameInputEnabled = false;
        private bool _uiInputEnabled = true;

        private void Update()
        {
            HandleEscapeInput();
            HandleGameInput();
            HandleUIInput();
        }

        private void HandleEscapeInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnUICancel?.Invoke();
            }
        }

        private void HandleGameInput()
        {
            if (!_gameInputEnabled) return;

            OnHorizontalInput?.Invoke(Input.GetAxis(GameConstants.HORIZONTAL_INPUT));

            if (Input.GetKeyDown(jumpKey)) OnJumpInput?.Invoke();
            if (Input.GetKeyDown(dropKey)) OnDropInput?.Invoke();
        }

        private void HandleUIInput()
        {
            if (!_uiInputEnabled) return;

            // Navigation with arrows and WASD
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) 
                OnUINavigateUp?.Invoke();
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) 
                OnUINavigateDown?.Invoke();
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) 
                OnUINavigateLeft?.Invoke();
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) 
                OnUINavigateRight?.Invoke();
                
            // Confirm with Enter or Space
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) 
                OnUIConfirm?.Invoke();
        }

        public void EnableGameInput()
        {
            _gameInputEnabled = true;
            _uiInputEnabled = false;
        }

        public void EnableUIInput()
        {
            _gameInputEnabled = false;
            _uiInputEnabled = true;
        }

        public void DisableAllInput()
        {
            _gameInputEnabled = false;
            _uiInputEnabled = false;
        }

        private void Start()
        {
            EnableUIInput();
        }
    }
}