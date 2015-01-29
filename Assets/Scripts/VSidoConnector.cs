using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;

public class VSidoConnector: MonoBehaviour  {

	SerialPort _serialPort;
	VSidoProtocol _client=new VSidoProtocol();

	public VSidoJoint[] Joints;

	ThreadWork _thread=new ThreadWork();
	bool can_poll_bytes_to_read=true;

	int[] _LastSetAngles;

	void _UpdateHandler(ThreadWork t)
	{
		_LastSetAngles = new int[Joints.Length];
		int[] newAngles=new int[Joints.Length];
		for (int i=0; i<_LastSetAngles.Length; ++i)
		{
			_LastSetAngles [i] = 9999;
			newAngles[i]=9999;
		}

		while(!t.IsDone)
		{
			for (int i=0; i<Joints.Length; ++i)
			{
				short angle=Joints[i].GetAngleValue();
				if(angle==_LastSetAngles [i])
					newAngles[i]=9999;
				else 
					newAngles[i]=angle;
				_LastSetAngles [i] = angle;
			}
			byte[] buffer=new byte[Joints.Length*8];
			int offset=0;
			for (int i=0; i<Joints.Length; ++i)
			{
				if(newAngles[i]!=9999)
				{
					byte[] opcode = _client.gen_cmd_set_object( (byte)(Joints[i].ID),(short)(newAngles[i]));
					for(int j=0;j<opcode.Length;++j)
						buffer[offset+j]=opcode[j];
					offset+=opcode.Length;
				}
			}
			if(offset>0)
			{
				try{

					_serialPort.Write(buffer, 0, offset);
				}catch(Exception e)
				{
				}
			}
			System.Threading.Thread.Sleep(30);
		}
	}

	public VSidoConnector()
	{
		_serialPort = new SerialPort ();
		_serialPort.ReadBufferSize = 4096;
		_serialPort.WriteBufferSize = 2048;
		_serialPort.DataReceived += _SerialDataReceived;
		_serialPort.DtrEnable = true;
		_serialPort.RtsEnable = true;
		_serialPort.ReadTimeout = 1;
		_serialPort.WriteTimeout = 1000;
		
		// HAX: cant use compile time flags here, so cache result in a variable
		if (UnityEngine.Application.platform.ToString().StartsWith("Windows"))
			can_poll_bytes_to_read = false;

		string[] ports = SerialUtilities.getPortNames ();
		for(int i=0;i<ports.Length;i++)
			Debug.Log ("Port["+i.ToString()+"]: "+ ports[i]);

		_thread.ThreadFunction += _UpdateHandler;
	}

	void OnDestroy()
	{
		_thread.Stop();
	}
	
	private string byte_to_hex_show(byte[] data)
	{
		string str = "";
		string hex = "";
		
		for (int i = 0; i < data.Length; i++)
		{
			hex = data[i].ToString("X");
			if (hex.Length % 2 == 1)
			{
//				hex = "0" + hex;　
			}
			if (data[i] == (byte)0xff)
			{
				str += "\n";
			}
			str += hex + " ";
			hex = "";
		}
		return str;
	}

	void _OnDataReceived()
	{
		int len=_serialPort.BytesToRead;
		byte[] data = new byte[len];
		_serialPort.Read (data, 0, len);
		Debug.Log ("Data Received: " + byte_to_hex_show(data));
	}
	void _SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		_OnDataReceived ();
	}

	public bool Connect(int port)
	{
		string[] ports = SerialUtilities.getPortNames ();
		if (port >= ports.Length)
			return false;
		Disconnect ();
		_serialPort.PortName = ports [port];
		_serialPort.BaudRate = 115200;

		try
		{
			_serialPort.Open ();
		}catch(Exception e)
		{
			Debug.Log("Failed to open serialPort:"+ e.Message);
		}

		if(_serialPort.IsOpen)
		{
			Debug.Log ("Connected to Robot at port: "+ports[port]);
			_thread.Start ();
		}
		return _serialPort.IsOpen;
	}

	public void Disconnect()
	{
		_serialPort.Close ();
	}
	public bool IsConnected()
	{
			return _serialPort.IsOpen;
	}
	public bool Walk(int speed)
	{
		if (!IsConnected ())
						return false;
		byte[] opcode= _client.gen_cmd_set_walk (speed, 0, 15);
		_serialPort.Write (opcode, 0, opcode.Length);
		return true;
	}
	public bool GetIK(byte ik)
	{
		if (!IsConnected ())
			return false;
		byte[] opcode= _client.gen_cmd_get_IK (ik);
		_serialPort.Write (opcode, 0, opcode.Length);
		return true;
	}

	public bool SetJointAngle(int id,float v)
	{
		if (!IsConnected ())
			return false;
		v = v * 3000 - 1500;
		byte[] opcode = _client.gen_cmd_set_object( (byte)(id),(short)(v));
		_serialPort.Write(opcode, 0, opcode.Length);
		return true;
	}

	public void Update()
	{
		if(!IsConnected())
			return;
		if(can_poll_bytes_to_read && _serialPort.BytesToRead==0)
			return;

		return;
		int inputData;

		while(true)
		{
			try 
			{						
				inputData = _serialPort.ReadByte();
			} catch (Exception e) // swallow read exceptions
			{
				//Log(e.GetType().Name + ": "+e.Message);	
				if (e.GetType() == typeof(TimeoutException))
					return;
				else 
					throw;							
				
			}
		}
		//_OnDataReceived ();
	}
}
