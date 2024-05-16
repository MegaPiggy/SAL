using UnityEngine;

namespace SALT
{
    public class InputBehaviour : MonoBehaviour, InputDetector
    {
        public virtual void Awake()
        {
            //Singleton<GameContext>.Instance.UserInputService.InputBegan += InputBegan;
            //Singleton<GameContext>.Instance.UserInputService.InputChanged += InputChanged;
            //Singleton<GameContext>.Instance.UserInputService.InputEnded += InputEnded;
        }

        public virtual void OnDestroy()
        {
            //Singleton<GameContext>.Instance.UserInputService.InputBegan -= InputBegan;
            //Singleton<GameContext>.Instance.UserInputService.InputChanged -= InputChanged;
            //Singleton<GameContext>.Instance.UserInputService.InputEnded -= InputEnded;
        }

        public virtual void InputBegan(UserInputService.InputObject inputObject, bool wasProcessed)
        {
        }

        public virtual void InputChanged(UserInputService.InputObject inputObject, bool wasProcessed)
        {
        }

        public virtual void InputEnded(UserInputService.InputObject inputObject, bool wasProcessed)
        {
        }
    }
}
