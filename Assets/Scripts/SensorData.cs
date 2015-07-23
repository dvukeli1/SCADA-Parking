/* 
Model klasa - TREBA REVIDIRATI ZBOG REDUDANCIJE KODA
*/

using UnityEngine;
using System.Collections;
using System;

using System.Threading;
using System.Collections.Generic;

public class SensorData : MonoBehaviour {
    public static SensorData Instance;
    private  List<byte> respond = new List<byte>();
    private List<byte> Grana1Data = new List<byte>();
    private List<byte> Grana2Data = new List<byte>();
    private  List<byte> Grana3Data = new List<byte>();
    private List<byte> Grana4Data = new List<byte>();
    private List<byte> Grana5Data = new List<byte>();
    public List<GameObject> m_Grana1;
    public List<GameObject> m_Grana2;
    public List<GameObject> m_Grana3;
    public List<GameObject> m_Grana4;
    public List<GameObject> m_Grana5;

    private UnitySerialPort serialPort;

	private byte stx = 0x02;
	private byte lenght = 0;
	private byte branchID = 0;
	private byte sensorID = 0;
	private byte x1 = 0;
	private byte x2 = 0;
	private byte x3 = 0;
	private byte x4 = 0;
	private byte crc = 0;
	private byte comm = 0;
	private byte etx = 0x03;
	private byte[] msg;
    private bool occupied;
    private byte[] test;

    public List<byte> Respond
    {
        get
        {
            return respond;
        }

        set
        {
            respond = value;
        }
    }

    public List<byte> Grana1Data1
    {
        get
        {
            return Grana1Data;
        }

        set
        {
            Grana1Data = value;
        }
    }

    public List<byte> Grana2Data1
    {
        get
        {
            return Grana2Data;
        }

        set
        {
            Grana2Data = value;
        }
    }

    public List<byte> Grana3Data1
    {
        get
        {
            return Grana3Data;
        }

        set
        {
            Grana3Data = value;
        }
    }

    public List<byte> Grana4Data1
    {
        get
        {
            return Grana4Data;
        }

        set
        {
            Grana4Data = value;
        }
    }

    public List<byte> Grana5Data1
    {
        get
        {
            return Grana5Data;
        }

        set
        {
            Grana5Data = value;
        }
    }

    void Awake()
    {
        // Define the script Instance
        Instance = this;
   
    }

        void Start()
    {
        Respond = new List<byte>();
        Grana1Data1 = new List<byte>();
        Grana2Data1 = new List<byte>();
        Grana3Data1 = new List<byte>();
        Grana4Data1 = new List<byte>();
        Grana5Data1 = new List<byte>();
        occupied = ToBoolean(0);
        foreach (GameObject go in m_Grana1)
        {
           go.transform.parent = this.gameObject.transform;
        }
        serialPort = GetComponent<UnitySerialPort>() as UnitySerialPort;
        StartCoroutine(Pool());
    }

    public void setState(byte branchID,byte sensorID,bool state){
		this.lenght = 0x05;
		if (state) {
			this.comm = 0xBB;
		} else {
			this.comm = 0xDD;
		}
		this.branchID = branchID;
		this.sensorID = sensorID;
		this.crc = (byte)(stx ^ lenght ^ comm ^ branchID ^ sensorID);
		msg= new byte[7];
		msg [0] = this.stx;
		msg [1] = this.lenght;
		msg [2] = this.comm;
		msg [3] = this.branchID;
		msg [4] = this.sensorID;
		msg [5] = this.crc;
		msg [6] = this.etx;
		//DebugMsg (msg);
		SendSerial (msg);

	}

	public void SetToRed(byte branchID,byte sensorID){
		this.lenght = 0x05;
		this.comm = 0xBB;
		this.branchID = branchID;
		this.sensorID = sensorID;
		this.crc =  (byte)(stx ^ lenght ^ comm ^ branchID ^ sensorID);
		//Debug.Log ("Red "+ msg);
		
	}

	public void setToGreen(byte branchID,byte sensorID){
		this.lenght = 0x05;
		this.comm = 0xDD;
		this.branchID = branchID;
		this.sensorID = sensorID;
		this.crc =  (byte)(stx ^ lenght ^ comm ^ branchID ^ sensorID);
	}

	public void checkStatus(){
		this.lenght = 0x03;
		this.comm = 0xAA;
		this.crc =  (byte)(stx ^ lenght ^ comm);
		msg= new byte[5];
		msg [0] = this.stx;
		msg [1] = this.lenght;
		msg [2] = this.comm;
		msg [3] = this.crc;
		msg [4] = this.etx;
		//DebugMsg (msg);
        SendSerial(msg);

	}

	private string DebugMsg(byte[] chunk){
		string log = "0x";
		log += BitConverter.ToString (chunk).Replace("-", " 0x");
			Debug.Log (log);
		return log;
	}

	public void SendSerial(byte[] msg){
		//SendSerialByte (msg);
		Thread.Sleep(1000);
        Debug.Log("DOBIO SAM OVO !!!! = " + DebugMsg( Grana2Data1.ToArray()));
        
	}
	
	
    private bool ToBoolean( int value)
    {
        if (value != 0) return true;
        else return false;
    }

	IEnumerator Pool()
	{
        yield return new WaitForSeconds(5);
       // while (true)
        //{
            checkStatus();
            yield return new WaitForSeconds(10);
        //}
       
	}
}
