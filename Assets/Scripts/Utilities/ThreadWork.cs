using UnityEngine;
using System.Collections;

public class ThreadWork {

	System.Threading.Thread _thread;
	object _lock=new object();

	bool _isDone;

	public delegate void ThreadFunctionDelg(ThreadWork t);
	public ThreadFunctionDelg ThreadFunction; 

	public bool IsDone
	{
		get{
			bool v;
			lock(_lock)
			{
				v=_isDone;
			}
			return v;
		}
		set{
			lock(_lock)
			{
				_isDone=value;
			}
		}
	}

	public void Start()
	{
		_thread = new System.Threading.Thread (Run);
		_thread.Start ();
	}

	public void Stop()
	{
		IsDone = true;
		if(_thread!=null)
			_thread.Abort ();
	}


	void Run()
	{
		if (ThreadFunction != null)
						ThreadFunction (this);
		IsDone = true;
	}
}
