using System;
using System.Net.Sockets;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Threading;
using System.Text;

public class NetworkListener : MonoBehaviour
{


    private Thread receiveThread;
    private UdpClient receiveSocket;
    private int receivePort = 49003;
    private bool stopReceive = false;

    public float pitch = 0.0f;
    public float roll = 0.0f;
    public float heading = 0.0f;

    [SerializeField]
    private GameObject HeadingIndicator;
    [SerializeField]
    private GameObject AttitudeIndicator;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;

        init();
    }

    void OnApplicationQuit()
    {
        Debug.Log("Aborting thread");
        receiveSocket.Client.Close();
        receiveThread.Abort();
    }

    // Update is called once per frame
    void Update()
    {
        // pitch
        AttitudeIndicator.transform.localPosition = new Vector3(0, Mathf.Clamp(pitch, -70f, 70f) / 100.0f, 0);

        // roll
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = new Vector3(0, 0, Mathf.Clamp(roll, -110f, 110f));
        AttitudeIndicator.transform.rotation = quat;
                       
        // heading
        quat.eulerAngles = new Vector3(0, 0, heading);
        HeadingIndicator.transform.rotation = quat;


    }

    private void init()
    {
        Debug.Log("Sending select packets");
        
        // pitch, roll, headings
        SendSelectPacket(17);

        // speeds
        SendSelectPacket(3);

        //AoA, side-slip, paths
        SendSelectPacket(18);

        // angular velocities
        SendSelectPacket(16);

        // lat, lon, altitude
        SendSelectPacket(20);


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

        for (int i = 5; i < data.Length; i += 36)
        {
            if (data[i] == 17)
            {
                pitch = BitConverter.ToSingle(data, i + 4);
                roll = BitConverter.ToSingle(data, i + 8);
                // heading will be 3rd set of 4 bytes (0, 1, 2)
                heading = BitConverter.ToSingle(data, i + 12);

                //Debug.Log("pitch: " + pitch + " roll: " + roll + " heading: " + heading);
            }
        }

        if (!stopReceive)
        {
            ReceiveData();
        }
    }

    private void SendSelectPacket(byte select)
    {
        UdpClient sendSocket = new UdpClient("127.0.0.1", 49000);

        byte[] selectPacket = new byte[] { 68, 83, 69, 76, 48, select, 0, 0, 0 };
        try
        {
            sendSocket.Send(selectPacket, selectPacket.Length);
            sendSocket.Close();
            Debug.Log("Sent select packet for " + select);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending: " + ex.Message.ToString());
        }
    }
}
