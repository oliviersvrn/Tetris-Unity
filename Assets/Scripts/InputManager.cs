using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float _initialDelay = 0.5f;
    public float _delay = 0.1f;

    public delegate void CallBack();
    private List<CustomInput> _inputs = new List<CustomInput>();

    private static InputManager instance = null;

    void Awake()
    {
		if (instance != null)
			Destroy(gameObject);
		else
			instance = this;
    }

    void Start()
    {
        _inputs.Add(new CustomInput(KeyCode.LeftArrow,  GameController.instance.Left));
        _inputs.Add(new CustomInput(KeyCode.RightArrow, GameController.instance.Right));
        _inputs.Add(new CustomInput(KeyCode.DownArrow,  GameController.instance.Down));
        _inputs.Add(new CustomInput(KeyCode.UpArrow,    GameController.instance.Drop));
        _inputs.Add(new CustomInput(KeyCode.Z,          GameController.instance.RotateLeft));
        _inputs.Add(new CustomInput(KeyCode.D,          GameController.instance.RotateRight));
        _inputs.Add(new CustomInput(KeyCode.Space,      GameController.instance.Hold));
    }

    void Update()
    {
        for (int i = 0; i < _inputs.Count; i++)
        {
            if (Input.GetKey(_inputs[i].keyCode))
            {
                if (_inputs[i].timer == 0)
                {
                    _inputs[i].callBack();
                }
                else if (_inputs[i].timer >= _initialDelay)
                {
                    if (_inputs[i].timer >= _initialDelay + _delay)
                    {
                        _inputs[i] = new CustomInput(_inputs[i].keyCode, _inputs[i].callBack, _initialDelay);
                        _inputs[i].callBack();
                    }
                }
                _inputs[i] = new CustomInput(_inputs[i].keyCode, _inputs[i].callBack, _inputs[i].timer + Time.deltaTime);
            }
            else
            {
                _inputs[i] = new CustomInput(_inputs[i].keyCode, _inputs[i].callBack, 0f);
            }
        }
    }

    struct CustomInput
    {
        public KeyCode keyCode;
        public CallBack callBack;
        public float timer;

        public CustomInput(KeyCode keyCode, CallBack callBack, float timer = 0f)
        {
            this.keyCode = keyCode;
            this.callBack = callBack;
            this.timer = timer;
        }
    };
}
