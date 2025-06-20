using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Uriel.Utils
{
    public class Dispatcher : MonoBehaviour
    {
	    public static void Init()
	    {
		    instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
	    }
	    
	    private static Dispatcher instance = null;

	    private static Dispatcher Instance
	    {
		    get
		    {
			    if (!instance)
			    {
				    instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
			    }
			    return instance;
		    }
	    }
	    
		private static readonly Queue<Action> ExecutionQueue = new ();
		private static readonly List<(Func<bool> condition, Action action)> ConditionalQueue = new ();

		public void Update() 
		{
			lock (ConditionalQueue) 
			{
				for (int i = ConditionalQueue.Count - 1; i >= 0; i--)
				{
					var cond = ConditionalQueue[i];
					if (cond.condition())
					{
						cond.action();
						ConditionalQueue.RemoveAt(i);
					}
				}
			}
			lock (ExecutionQueue) 
			{
				while (ExecutionQueue.Count > 0) 
				{
					ExecutionQueue.Dequeue().Invoke();
				}
			}
		}
		
		public static void WaitFor(Func<bool> condition, Action action)
		{
			lock (ConditionalQueue)
			{
				ConditionalQueue.Add((condition, action));
			}
		}
		
		public static void Enqueue(Action action)
		{
			lock (ExecutionQueue)
			{
				ExecutionQueue.Enqueue(action);
			}
		}

		public static Task EnqueueAsync(Action action)
		{
			var tcs = new TaskCompletionSource<bool>();
			void WrappedAction() 
			{
				try 
				{
					action();
					tcs.TrySetResult(true);
				} catch (Exception ex) 
				{
					tcs.TrySetException(ex);
				}
			}
			Enqueue(WrappedAction);
			return tcs.Task;
		}
		
		private void Awake() 
		{
			if (instance == null) 
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
		}

		void OnDestroy() 
		{
			instance = null;
		}
    }
}