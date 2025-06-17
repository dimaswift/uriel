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

	    public static Dispatcher Instance
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
		

		public void Update() 
		{
			lock(ExecutionQueue) 
			{
				while (ExecutionQueue.Count > 0) 
				{
					ExecutionQueue.Dequeue().Invoke();
				}
			}
		}

		public void Enqueue(IEnumerator action) 
		{
			lock (ExecutionQueue) 
			{
				ExecutionQueue.Enqueue (() => 
				{
					StartCoroutine (action);
				});
			}
		}

		public void Enqueue(Action action)
		{
			Enqueue(ActionWrapper(action));
		}

		public Task EnqueueAsync(Action action)
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

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;
		}


		private IEnumerator ActionWrapper(Action a)
		{
			a();
			yield return null;
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