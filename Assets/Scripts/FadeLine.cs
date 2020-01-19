using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FadeLine : MonoBehaviour
{
    private bool fading = false;
    public float fadeTime = 1f;
    private float timer = 0f;
    private float oldTimeScale;

    public delegate void CallBack();
    private static List<System.Tuple<int, CallBack>> callBacks = new List<System.Tuple<int, CallBack>>();

    void Start()
    {
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        Color color = line.material.color;
        color.a = 0f;
        line.material.color = color;
    }

    void Update()
    {
        if (fading)
        {
            timer += Time.unscaledDeltaTime;

            LineRenderer line = gameObject.GetComponent<LineRenderer>();

            Color color = line.material.color;

            if (timer <= fadeTime)
            {
                color.a = Mathf.Sin(timer / fadeTime * Mathf.PI);
            }
            else
            {
                color.a = 0;
                fading = false;
                timer = 0;
                Time.timeScale = oldTimeScale;
                if (callBacks.Count > 0)
                {
                    callBacks[0].Item2();
                    callBacks.RemoveAt(0);
                }
            }

            line.material.color = color;
        }
    }

    public void StartFading(bool freezeTime, CallBack cb = null, int priority = 0)
    {
        if (!fading)
        {
            if (freezeTime)
            {
                oldTimeScale = Time.timeScale;
                Time.timeScale = 0f;

                int i = 0;

                for (i = 0; i < callBacks.Count; i++)
                {
                    if (priority >= callBacks[i].Item1)
                    {
                        callBacks.Insert(i, System.Tuple.Create(priority, cb));
                        break;
                    }
                }
                if (i == callBacks.Count)
                    callBacks.Add(System.Tuple.Create(priority, cb));
            }

            fading = true;
        }
    }
}
