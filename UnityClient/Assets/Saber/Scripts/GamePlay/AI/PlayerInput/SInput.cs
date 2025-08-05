using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Saber.AI
{
    public class SInput
    {
        public IInputSystem Input_System;

        public InputAxis Horizontal = new("Horizontal", true, true);
        public InputAxis Vertical = new("Vertical", true, true);
        public InputAxis UpDown = new("UpDown", false, true);

        // public Dictionary<string, InputRow> DInputs = new Dictionary<string, InputRow>();        //Shame it cannot be Serialided :(
        /// <summary>Default Input Row Values </summary>
        public List<InputRow> Inputs = new();

        [Tooltip("Inputs won't work on Time.Scale = 0")]
        public bool IgnoreOnPause = true;

        public float m_Horizontal; //Horizontal Right & Left   Axis X
        public float m_Vertical; //Vertical   Forward & Back Axis Z
        public float m_UPDown; //Up Down value    

        public UnityEvent<Vector3> MovementEvent = new();

        protected Vector3 m_InputAxis;


        public SInput()
        {
            Input_System = DefaultInput.GetInputSystem(); //Get Which Input System is being used

            //Update to all the Inputs the Input System
            foreach (var i in Inputs)
                i.InputSystem = Input_System;

            Horizontal.InputSystem = Vertical.InputSystem = UpDown.InputSystem = Input_System;
            //  List_to_Dictionary();
        }

        public virtual void ResetInputs()
        {
            foreach (var input in Inputs)
            {
                if (input.ResetOnDisable && input.Active) input.OnInputChanged.Invoke(input.InputValue = false); //Sent false to all Input listeners 
            }
        }

        public void Update() => SetInput();

        /// <summary>Send all the Inputs to the Actor</summary>
        protected virtual void SetInput()
        {
            if (IgnoreOnPause && Time.timeScale == 0)
                return;

            m_Horizontal = Horizontal.GetAxis;
            m_Vertical = Vertical.GetAxis;
            m_UPDown = UpDown.GetAxis;

            m_InputAxis = new Vector3(m_Horizontal, m_UPDown, m_Vertical);

            MovementEvent.Invoke(m_InputAxis); //Invoke the Event for the Movement AXis

            //   Debug.Log($"activemap [{ActiveMap.name.Value}] [{ActiveMapIndex}]");

            foreach (var item in Inputs)
                _ = item.GetValue; //This will set the Current Input value to the inputs and Invoke the Values
        }


        /// <summary>Enable/Disable an Input Row</summary>
        public virtual void EnableInput(string name, bool value)
        {
            // Debug.Log($"EnableInput {name} {value}");

            string[] inputsName = name.Split(',');

            foreach (var inp in inputsName)
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    if (Inputs[i].name == inp) Inputs[i].Active = value;
                }
                //if (DInputs.TryGetValue(inp, out InputRow input)) input.Active = value;
            }
        }

        public virtual void SetInput(string name, bool value)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i].name == name)
                {
                    Inputs[i].InputValue = value;
                }
            }
        }


        /// <summary>  Resets the value and toggle of an Input to False </summary>
        public virtual void ResetInput(string name)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i].name == name)
                {
                    Inputs[i].InputValue = false;
                }
            }
        }

        /// <summary>Enable an Input Row</summary>
        public virtual void EnableInput(string name) => EnableInput(name, true);

        /// <summary> Disable an Input Row </summary>
        public virtual void DisableInput(string name) => EnableInput(name, false);

        /// <summary>Check if an Input Row  is active</summary>
        public virtual bool IsActive(string name)
        {
            var i = GetInput(name);
            if (i != null) return i.Active;
            return false;
        }

        /// <summary>Check if an Input Row  exist  and returns it</summary>
        public virtual InputRow FindInput(string name)
        {
            return Inputs.Find(item => item.name == name);
        }

        public IInputAction GetInput(string name)
        {
            return Inputs.Find(item => item.name == name);
        }


        public void ConnectInput(string name, UnityAction<bool> action)
        {
            if (string.IsNullOrEmpty(name))
                return;
            foreach (var item in Inputs)
                item.InputChanged.AddListener(action);
        }

        public void DisconnectInput(string name, UnityAction<bool> action)
        {
            if (string.IsNullOrEmpty(name))
                return;
            foreach (var item in Inputs)
                item.InputChanged.RemoveListener(action);
        }


        //public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }

    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// <summary>Input Class to change directly between Keys and Unity Inputs </summary>
    [System.Serializable]
    public class InputRow : IInputAction
    {
        public string name = "InputName";
        public bool active = true;
        public bool ignoreOnPause;
        public InputType type = InputType.Input;

        public string input = "Value";

        //[SearcheableEnum] 
        public KeyCode key = KeyCode.A;
        public bool debug;

        /// <summary>Type of Button of the Row System</summary>
        public InputButton GetPressed = InputButton.Press;

        /// <summary>Current Input Value</summary>

        // public bool InputValue = false;
        public bool InputValue
        {
            get => m_Input;
            set
            {
                if (m_Input != value)
                {
                    m_Input = value;
                    if (debug) Debug.Log($"<color=cyan><B>[Input {name} : {m_Input}]</B></color>");
                }
            }
        }

        private bool m_Input;

        //  public bool ToggleValue = false;
        [Tooltip("When the Input is disabled the input value will set to false and it will send that value to all possible connections")]
        public bool ResetOnDisable = true;


        public UnityEvent OnInputDown = new();
        public UnityEvent OnInputUp = new();
        public UnityEvent OnLongPress = new();
        public UnityEvent OnLongPressReleased = new();
        public UnityEvent OnDoubleTap = new();
        public UnityEvent<bool> OnInputChanged = new();
        public UnityEvent<bool> OnInputToggle => OnInputChanged;
        public UnityEvent OnInputEnable = new();
        public UnityEvent OnInputDisable = new();

        protected IInputSystem inputSystem = new DefaultInput();

        #region LONG PRESS and Double Tap

        public float DoubleTapTime = 0.3f; //Double Tap Time

        [Tooltip("Time the Input Should be Pressed")]
        public float LongPressTime = 0.5f;

        [Tooltip("Smooth decrese the acumulated pressed time")]
        public bool SmoothDecrease;

        //public FloatReference LongPressTime = new FloatReference(0.5f);
        private bool FirstInputPress;
        private bool InputCompleted;
        private float InputStartTime;
        public UnityEvent OnInputPressed = new();
        public UnityEvent<float> OnInputFloat = new();

        #endregion

        /// <summary>Return True or False to the Selected type of Input of choice</summary>
        public virtual bool GetValue
        {
            get
            {
                if (!active)
                    return false;
                if (ignoreOnPause)
                    return false;
                if (inputSystem == null)
                    return false;

                var oldValue = InputValue;

                switch (GetPressed)
                {
                    case InputButton.Press:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                                OnInputDown.Invoke();
                            else
                                OnInputUp.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }

                        if (InputValue)
                            OnInputPressed.Invoke();

                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Down:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                                OnInputDown.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }

                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Up:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonUp(input) : Input.GetKeyUp(key);

                        if (oldValue != InputValue)
                        {
                            if (!InputValue)
                                OnInputUp.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }

                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.LongPress:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue)
                            OnInputChanged.Invoke(InputValue); //Just to make sure the Input is Pressed

                        //Debug.Log($"FirstInputPress = {FirstInputPress} | InputCompleted {InputCompleted}");

                        if (InputValue)
                        {
                            if (!FirstInputPress && !InputCompleted)
                            {
                                FirstInputPress = true;
                                InputStartTime = 0;
                                OnInputFloat.Invoke(0);
                                OnInputDown.Invoke();
                            }
                            else
                            {
                                if (!InputCompleted)
                                {
                                    if (InputStartTime >= LongPressTime)
                                    {
                                        OnInputFloat.Invoke(1);
                                        OnLongPress.Invoke(); //Complete the long press
                                        FirstInputPress = false;
                                        InputCompleted = true;
                                        //  return (InputValue = true);
                                    }
                                    else
                                    {
                                        InputStartTime += Time.deltaTime;
                                        OnInputFloat.Invoke(Mathf.Clamp01(InputStartTime / LongPressTime));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (InputCompleted)
                                OnLongPressReleased.Invoke(); //Invoke when the Input Long press is completed and released


                            //If the Input was released before the LongPress was completed  
                            if (FirstInputPress)
                            {
                                if (SmoothDecrease)
                                {
                                    InputStartTime -= Time.deltaTime;

                                    if (InputStartTime > 0)
                                    {
                                        OnInputFloat.Invoke(Mathf.Clamp01(InputStartTime / LongPressTime));
                                    }
                                    else
                                    {
                                        ResetLongPress();
                                    }
                                }
                                else
                                {
                                    ResetLongPress();
                                }
                            }
                            else
                            {
                                InputCompleted = false;
                            }
                        }

                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.DoubleTap:
                    {
                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue)
                        {
                            OnInputChanged.Invoke(InputValue); //Just to make sure the Input is Pressed

                            if (InputValue)
                            {
                                if (InputStartTime != 0 && STools.ElapsedTime(InputStartTime, DoubleTapTime))
                                {
                                    FirstInputPress = false; //This is in case it was just one Click/Tap this will reset it
                                }

                                if (!FirstInputPress)
                                {
                                    OnInputDown.Invoke();
                                    InputStartTime = Time.time;
                                    FirstInputPress = true;
                                }
                                else
                                {
                                    if ((Time.time - InputStartTime) <= DoubleTapTime)
                                    {
                                        FirstInputPress = false;
                                        InputStartTime = 0;
                                        OnDoubleTap.Invoke(); //Sucesfull Double tap
                                    }
                                    else
                                    {
                                        FirstInputPress = false;
                                    }
                                }
                            }
                        }

                        break;
                    }
                    case InputButton.Toggle:
                    {
                        var toggle = (type == InputType.Input) ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (toggle)
                        {
                            InputValue ^= true;
                            OnInputToggle.Invoke(InputValue);

                            if (InputValue)
                                OnInputDown.Invoke();
                            else
                                OnInputUp.Invoke();
                        }

                        break;
                    }
                    case InputButton.Axis:
                    {
                        var axisValue = InputSystem.GetAxis(input);
                        InputValue = Mathf.Abs(axisValue) > 0;

                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                            {
                                OnInputDown.Invoke();
                            }
                            else
                            {
                                OnInputUp.Invoke();
                                OnInputFloat.Invoke(0);
                            }

                            OnInputChanged.Invoke(InputValue);
                        }

                        if (InputValue)
                        {
                            OnInputPressed.Invoke();
                            OnInputFloat.Invoke(axisValue);
                        }

                        break;
                    }
                    default: break;
                }

                return InputValue;

                void ResetLongPress()
                {
                    InputStartTime = 0;
                    OnInputUp.Invoke(); //Set it as interrupted
                    FirstInputPress = false; //This will reset the Long Press
                    InputCompleted = false;
                }
            }
        }

        public IInputSystem InputSystem
        {
            get => inputSystem;
            set => inputSystem = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public bool Active
        {
            get => active;
            set
            {
                //Debug.Log($"EnableInput {name} - {value}"); ;
                active = value;
                if (value)
                    OnInputEnable.Invoke();
                else
                    OnInputDisable.Invoke();
            }
        }

        public InputButton Button => GetPressed;

        public UnityEvent InputDown => this.OnInputDown;

        public UnityEvent InputUp => this.OnInputUp;

        public UnityEvent<bool> InputChanged => this.OnInputChanged;


        #region Constructors

        public InputRow(KeyCode k)
        {
            active = true;
            type = InputType.Key;
            key = k;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string input, KeyCode key)
        {
            active = true;
            type = InputType.Key;
            this.key = key;
            this.input = input;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string unityInput, KeyCode k, InputButton pressed)
        {
            active = true;
            type = InputType.Key;
            key = k;
            input = unityInput;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string name, string unityInput, KeyCode k, InputButton pressed, InputType itype)
        {
            this.name = name;
            active = true;
            type = itype;
            key = k;
            input = unityInput;
            GetPressed = pressed;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(bool active, string name, string unityInput, KeyCode k, InputButton pressed, InputType itype)
        {
            this.name = name;
            this.active = active;
            type = itype;
            key = k;
            input = unityInput;
            GetPressed = pressed;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow()
        {
            active = true;
            name = "InputName";
            type = InputType.Input;
            input = "Value";
            key = KeyCode.A;
            GetPressed = InputButton.Press;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        #endregion
    }

    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    [System.Serializable]
    public class InputAxis
    {
        public bool active = true;
        public string name = "NewAxis";
        public bool raw = true;
        public string input = "Value";
        IInputSystem inputSystem = new DefaultInput();
        float currentAxisValue;

        /// <summary>Returns the Axis Value</summary>
        public float GetAxis
        {
            get
            {
                if (inputSystem == null || !active)
                    return 0f;
                currentAxisValue = raw ? inputSystem.GetAxisRaw(input) : inputSystem.GetAxis(input);
                return currentAxisValue;
            }
        }

        /// <summary> Set/Get which Input System this Axis is using by Default is set to use the Unity Input System </summary>
        public IInputSystem InputSystem
        {
            get => inputSystem;
            set => inputSystem = value;
        }

        public InputAxis()
        {
            active = true;
            raw = true;
            input = "Value";
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string value)
        {
            active = true;
            raw = false;
            input = value;
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string InputValue, bool active, bool isRaw)
        {
            this.active = active;
            this.raw = isRaw;
            input = InputValue;
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string name, string InputValue, bool active, bool raw)
        {
            this.active = active;
            this.raw = raw;
            input = InputValue;
            this.name = name;
            inputSystem = new DefaultInput();
        }
    }
    
    public enum InputType
    {
        Input,
        Key
    }

    public enum InputButton
    {
        Press = 0,
        Down = 1,
        Up = 2,
        LongPress = 3,
        DoubleTap = 4,
        Toggle = 5,
        Axis = 6
    }
}