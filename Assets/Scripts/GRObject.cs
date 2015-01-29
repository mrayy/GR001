using UnityEngine;
using System.Collections;

public class GRObject : MonoBehaviour {
	
	public int SerialPort;
	public VSidoConnector _robot;
	// Use this for initialization
	void Start () {
		if (!_robot.Connect (SerialPort)) {
				Debug.Log ("Failed to connect to robot.");
		}else
		{
		}
		_robot.GetIK (1);
		_robot.Walk(0);

	}
	void OnDestroy()
	{
		_robot.Disconnect ();
		Debug.Log ("Robot is closed.");
	}

	float angle=0;

	// Update is called once per frame
	void Update () {
		angle += Time.deltaTime*0.3f;
		//_robot.SetJointAngle (3, Mathf.Abs(Mathf.Sin(  angle+Mathf.PI/2))*0.5f);
		_robot.Update ();
	}
} 
