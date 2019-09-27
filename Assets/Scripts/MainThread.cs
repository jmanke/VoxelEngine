using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThread : MonoBehaviour
{
    private static MainThread instance;
    private static ConcurrentQueue<System.Action> actions = new ConcurrentQueue<System.Action>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static void ExecuteOnMainThread(System.Action action)
    {
        actions.Enqueue(action);
    }

    public void Update()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}
