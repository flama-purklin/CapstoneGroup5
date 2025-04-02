using UnityEngine;
using System;
using System.Collections.Generic;

namespace CoreControl.MysteryParsing
{
    /// <summary>
    /// Helper class to execute code on the main thread from background threads.
    /// This is needed because Unity's API can only be called from the main thread.
    /// </summary>
    public class MainThreadHelper : MonoBehaviour
    {
        private static MainThreadHelper _instance;
        public static MainThreadHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Create a new game object for our manager
                    var go = new GameObject("MainThreadHelper");
                    // Add the script to the game object
                    _instance = go.AddComponent<MainThreadHelper>();
                    // Make sure the object is never destroyed during scene changes
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private readonly object _lock = new object();
        
        private void Update()
        {
            lock (_lock)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
        
        public void QueueOnMainThread(Action action)
        {
            if (action == null)
            {
                Debug.LogError("Action to execute can't be null");
                return;
            }
            
            lock (_lock)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}