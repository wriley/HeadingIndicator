using System;
using System.Net.Sockets;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading;
using System.Text;

public class rotateToNetwork : MonoBehaviour {


	private Thread receiveThread;
	private UdpClient receiveSocket;
	private int receivePort = 49003;
	private bool stopReceive = false;

	public float heading = 0.0f;

	// Use this for initialization
	void Start () {
		Application.runInBackground = true;

		init ();
	}

	void OnApplicationQuit()
	{
		Debug.Log("Aborting thread");
		receiveSocket.Client.Close();
		receiveThread.Abort();
	}

	// Update is called once per frame
	void Update () {
		Quaternion quat = Quaternion.identity;
		quat.eulerAngles = new Vector3(0,0,heading);
		transform.rotation = quat;
	}
	
	private void init()
	{
		Debug.Log("Sending select packet");
		SendSelectPacket();

		Debug.Log("Starting receive thread");
		receiveSocket = new UdpClient(receivePort);
		receiveThread = new Thread(new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}

	private void ReceiveData()
	{
		receiveSocket.BeginReceive(new AsyncCallback(ReceiveDataCallback), null);
	}

	private void ReceiveDataCallback(IAsyncResult res)
	{
		IPEndPoint ip = new IPEndPoint(IPAddress.Any, receivePort);
		byte[] data = receiveSocket.EndReceive(res, ref ip);

		for(int i = 5; i < data.Length; i += 36)
		{
			if(data[i] == 17)
			{
				// heading will be 3rd set of 4 bytes (0, 1, 2)
				heading = BitConverter.ToSingle(data, i + 4 + (4 * 2));
			}
		}

		if(!stopReceive)
		{
			ReceiveData();
		}
	}

	private void SendSelectPacket()
	{
		UdpClient sendSocket = new UdpClient("127.0.0.1", 49000);

		// DSEL0 for data 17 (pitch, roll, headings)
		byte[] selectPacket = new byte[] { 68, 83, 69, 76, 48, 17, 0, 0, 0 };
		try
		{
			sendSocket.Send(selectPacket, selectPacket.Length);
			sendSocket.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("Error sending: " + ex.Message.ToString());
		}
	}
}
