using UnityEngine;
using System.Collections;
using System;
using PropertyEnabledInspector;

[Serializable]
public class VSidoJoint: MonoBehaviour {
	
	public bool UseLimits;
	[Serializable]
	public struct CLimits
	{
		public float Min;
		public float Max;
	}
	[SerializeField]
	public CLimits Limits;

	public enum EAxis
	{
		XAxis,
		YAxis,
		ZAxis
	}

	public EAxis RotationAxis;

	public bool FlipAxis=false;

	public float CurrentAngle;/*
	{
		get{return _currentAngle;}
	}*/
	public float TargetAngle;/*
	{
		set{ _targetAngle=value;}
		get{return _targetAngle;}
	}*/


	public int ID;
	/*
	{
		set{ _ID = value;}
		get{return _ID;}
	}
		
	float _currentAngle=0;
	float _targetAngle=0;
	int _ID;*/

	// Use this for initialization
	void Start () {
		if (UseLimits && Limits.Max < Limits.Min)
						Limits.Max = Limits.Min;
	}
	float fix(float v)
	{
		if(v>180)
			v=v-360;
		return v;
	}
	float ClampLimits(float v)
	{
		if (!UseLimits)return v;

		return Mathf.Min (Mathf.Max (v, Limits.Min), Limits.Max);
	}
	// Update is called once per frame
	void Update () {
	//	if(transform.hasChanged)
		{
			Vector3 euler=transform.localRotation.eulerAngles;
			switch(RotationAxis)
			{
			case EAxis.XAxis:
				euler.x=ClampLimits(fix(euler.x));
				transform.localRotation=Quaternion.Euler(euler);
				TargetAngle=euler.x;
				break;

			case EAxis.YAxis:
				euler.y=ClampLimits(fix(euler.y));
				transform.localRotation=Quaternion.Euler(euler);
				TargetAngle=euler.y;
				break;
				
			case EAxis.ZAxis:
				euler.z=ClampLimits(fix(euler.z));
				transform.localRotation=Quaternion.Euler(euler);
				TargetAngle=euler.z;
				break;
			}
			if(TargetAngle>180)
				TargetAngle=TargetAngle-360;
			if(FlipAxis)
				TargetAngle=-TargetAngle;
		}
	}

	public short GetAngleValue()
	{
		return (short)(TargetAngle*1000.0f/90.0f);
	}
}
