using UnityEngine;
using UnityEngine.Events;

namespace Saber.AI
{
    /// <summary>Basic Entries needed to use Malbers Input Component</summary>
    public interface IInputSource
    {
        void Enable(bool val);

        /// <summary>Allows the Input to move the Character (Use Axis)</summary>
        //bool MoveCharacter { get; set; }
         
        /// <summary>Returns the Input Action by its name</summary>
        IInputAction GetInput(string input);
        void EnableInput(string input);
        void DisableInput(string input);

        /// <summary>Changes the value of an Input using its name</summary>
        void SetInput(string input, bool value);

        /// <summary>Connects an input to a Bool Method</summary>
        void ConnectInput(string name, UnityAction<bool> action);

        /// <summary>Disconnect an input to a Bool Method</summary>
        void DisconnectInput(string name, UnityAction<bool> action);

        /// <summary> Reset the Input Value and Toggle value to false </summary> 
        void ResetInput(string name);
    }

    public interface IInputAction
    {
        /// <summary> Set/Get Input Action Active value</summary>
        bool Active { get; set; }

        /// <summary>Input Action Value True (Down/Pressed) False (Up/Released)</summary>
        bool GetValue { get; }

        string Name { get; }
 
        UnityEvent<bool> InputChanged { get; }
    }
   
    

    /// <summary> Common Entries for all Inputs on the Store </summary>
    public interface IInputSystem
    {
        float GetAxis(string Axis);
        float GetAxisRaw(string Axis);
        bool GetButtonDown(string button);
        bool GetButtonUp(string button);
        bool GetButton(string button);
    }


    /// <summary> Default Unity Input</summary>
    public class DefaultInput : IInputSystem
    {
        public float GetAxis(string Axis) => Input.GetAxis(Axis);

        public float GetAxisRaw(string Axis) => Input.GetAxisRaw(Axis);

        public bool GetButton(string button) => Input.GetButton(button);

        public bool GetButtonDown(string button) => Input.GetButtonDown(button);

        public bool GetButtonUp(string button) => Input.GetButtonUp(button);

        /// <summary> This Gets the Current Input System that is being used... Unity's, CrossPlatform or Rewired</summary>
        public static IInputSystem GetInputSystem()
        {
            return new DefaultInput();             //Set it as default the Unit Input System
        }
    }
}