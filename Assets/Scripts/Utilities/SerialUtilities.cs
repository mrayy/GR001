using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

public class SerialUtilities  {
	static public string[] getPortNames ()
	{
		int p = (int)Environment.OSVersion.Platform;
		// Are we on Unix?
		if (p == 4 || p == 128 || p == 6) {
			string[] ttys = Directory.GetFiles ("/dev/", "tty.*");
			return ttys;
		}else{
			return SerialPort.GetPortNames();
		}
	}

}
