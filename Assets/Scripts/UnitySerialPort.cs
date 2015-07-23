/*
Klasa komunikacije sa COM portovima, sve postavke se nalaze u XML-u - 
RIJESITI VANJSKO PRISTUPANJE XML-u zbog postavki/ naci alternativnpo ADMIN rijesenje 
*/

using UnityEngine;
using System.Collections;

using System.IO;
using System.IO.Ports;
using System;

using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Xml;


public class UnitySerialPort : MonoBehaviour
{
    // Init a static reference if script is to be accessed by others when used in a 
    // none static nature eg. its dropped onto a gameObject. The use of "Instance"
    // allows access to public vars as such as those available to the unity editor.
    public static UnitySerialPort Instance;
    private List<Dictionary<string, string>> etaze = new List<Dictionary<string, string>>();
    private Dictionary<string, string> obj;
  
    #region Properties

    // The serial port
    private SerialPort SerialPort = new SerialPort();
   

    // The script update can run as either a seperate thread
    // or as a standard coroutine. This can be selected via 
    // the unity editor.

    public enum LoopUpdateMethod
    { Threading, Coroutine }

    // This is the public property made visible in the editor.
    public LoopUpdateMethod UpdateMethod =
        LoopUpdateMethod.Threading;

    // Thread used to recieve and send serial data
    private Thread serialThread;

    // List of all baudrates available to the arduino platform
    private ArrayList baudRates =
        new ArrayList() { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };

    // List of all com ports available on the system
    private ArrayList comPorts =
        new ArrayList();

    // If set to true then open the port when the start
    // event is called.
    public bool OpenPortOnStart = true;

    // Holder for status report information
    private string portStatus = "";

    public string PortStatus
    {
        get { return portStatus; }
        set { portStatus = value; }
    }

    public ArrayList ComPorts
    {
        get { return comPorts; }
    }
    // Current com port and set of default
    public string ComPort;

    // Current baud rate and set of default
    public int BaudRate = 9600;

    public int ReadTimeout = 10;

    public int WriteTimeout = 10;

    // Property used to run/keep alive the serial thread loop
    private bool isRunning = false;
    public bool IsRunning
    {
        get { return isRunning; }
        set { isRunning = value; }
    }

    // Set the gui to show ready
    private string rawData = "Ready";
    public string RawData
    {
        get { return rawData; }
        set { rawData = value; }
    }

    // Storage for parsed incoming data
    private string[] chunkData;
    public string[] ChunkData
    {
        get { return chunkData; }
        set { chunkData = value; }
    }

    // Refs populated by the editor inspector for default gui
    // functionality if script is to be used in a non-static
    // context.
    public GameObject ComStatusText;

    public GameObject RawDataText;

    private List<byte> Respond = new List<byte>();
    private List<byte> Grana1Data = new List<byte>();
    private List<byte> Grana2Data = new List<byte>();
    private List<byte> Grana3Data = new List<byte>();
    private List<byte> Grana4Data = new List<byte>();
    private List<byte> Grana5Data = new List<byte>();

    public List<GameObject> m_Grana1;
    public List<GameObject> m_Grana2;
    public List<GameObject> m_Grana3;
    public List<GameObject> m_Grana4;
    public List<GameObject> m_Grana5;

    private TextAsset PortAsset;

    private UnitySerialPort serialPort;

    private byte STX = 0x02;
    private byte sendLenght = 0;
    private byte branchID = 0;
    private byte sensorID = 0;
    private byte x1 = 0;
    private byte x2 = 0;
    private byte x3 = 0;
    private byte x4 = 0;
    private byte crc = 0;
    private byte sendComm = 0;
    private byte ETX = 0x03;

    private byte[] msg;
    private bool occupied;
    private byte[] test;
    private int dataPosition = 0;
    private List<byte> data = new List<byte>();
    private byte rData = 0;
    private byte[] debugData = new byte[50];
    private bool isSTX = false;
    private bool isETX = false;
   
    
    private byte lenght = 0x00;
    private byte comm = 0x00;
    private byte CRC = 0x00;
    private int msgPoss = 0;
    private SensorData ToData;
    private string status = "pool";
    private XmlDocument xmlDoc;
    private string path;
    #endregion Properties

    #region Unity Frame Events

    /// <summary>
    /// The awake call is used to populate refs to the gui elements used in this 
    /// example. These can be removed or replaced if needed with bespoke elements.
    /// This will not affect the functionality of the system. If we are using awake
    /// then the script is being run non staticaly ie. its initiated and run by 
    /// being dropped onto a gameObject, thus enabling the game loop events to be 
    /// called e.g. start, update etc.
    /// </summary>
    void Awake()
    {
        path = Application.dataPath + "/StreamingAssets/ComPorts.xml";
        xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
                                                    // Define the script Instance
        Instance = this;
        xmlDoc.Load(path);
        XMLReader();

        for (int i = 0; i < etaze.Count; i++)
        {
            string etazaName = "";
            string baudrate = "";
            string active = "";
            etaze[i].TryGetValue("name", out etazaName);
            Debug.Log(" OVO IMAM " + etazaName);
            Debug.Log(" OVO JE TAG " + this.gameObject.tag);
            if (this.gameObject.tag == etazaName)
            {
                etaze[i].TryGetValue("comPort", out ComPort);
                Debug.Log(" COMPORT " + ComPort);
                etaze[i].TryGetValue("baudrate", out baudrate);
                etaze[i].TryGetValue("active", out active);
                if (active.Equals("true"))
                {
                    OpenPortOnStart = true;
                }
                else
                {
                    OpenPortOnStart = false;
                }
            }
        }
        // ComPort = this.gameObject.tag;

        // If we have used the editor inspector to populate any included gui
        // elements then lets initiate them and set some default values.

        // Details if the port is open or closed
        if (ComStatusText != null)
        { ComStatusText.GetComponent<GUIText>().text = "ComStatus: Closed"; }
    }

    void GameObjectSerialPort_DataRecievedEvent(string[] Data, string RawData)
    {
        Debug.Log("Data Recieved: " + RawData);
    }

    /// <summary>
    /// The start call is used to populate a list of available com ports on the
    /// system. The correct port can then be selected via the respective guitext
    /// or a call to UpdateComPort();
    /// </summary>
    void Start()
    {

        Debug.Log("PortName " + ComPort);
        ToData = gameObject.GetComponent<SensorData>() as SensorData;
        // Population of comport list via system.io.ports
        PopulateComPorts();

        // If set to true then open the port. You must 
        // ensure that the port is valid etc. for this! 
        this.gameObject.name = ComPort;
        if (OpenPortOnStart) {
            OpenSerialPort();
            StartCoroutine(Pool());
        }
        else
        {
            this.gameObject.SetActive(false);
        }
        
    }


    void XMLReader()
    {
       // PortAsset = new TextAsset();
        //PortAsset = Resources.Load(Application.dataPath + "/StreamingAssets/ComPorts.xml") as TextAsset;
       
        Debug.Log(Application.dataPath + "/StreamingAssets/ComPorts.xml");
        //xmlDoc.LoadXml(PortAsset.text); // load the file.
      
        XmlNodeList etazaList = xmlDoc.GetElementsByTagName("etaza"); // array of etaza.

        foreach (XmlNode etazaInfo in etazaList)
        {
            XmlNodeList etazaContent = etazaInfo.ChildNodes;
            obj = new Dictionary<string, string>(); // Create a object(Dictionary) to colect the both nodes inside the level node and then put into levels[] array.

            foreach (XmlNode portSettings in etazaContent) // levels itens nodes.
            {
                if (portSettings.Name == "name")
                {
                    obj.Add("name", portSettings.InnerText); // put this in the dictionary.
                }
                if (portSettings.Name == "comPort")
                {
                    obj.Add("comPort", portSettings.InnerText); // put this in the dictionary.
                }

                if (portSettings.Name == "baudrate")
                {
                    obj.Add("baudrate", portSettings.InnerText); // put this in the dictionary.
                }
                if (portSettings.Name == "active")
                {
                    obj.Add("active", portSettings.InnerText); // put this in the dictionary.
                }
            }
            etaze.Add(obj); // add whole obj dictionary in the levels[].
           
          
        }
    }
    /// <summary>
    /// Clean up the thread and close the port on application close event.
    /// </summary>
    void OnApplicationQuit()
    {
        // Call to cloase the serial port
        CloseSerialPort();

        Thread.Sleep(500);

        if (UpdateMethod == LoopUpdateMethod.Threading)
        {
            // Call to end and cleanup thread
            StopSerialThread();
        }

        Thread.Sleep(500);
    }

    #endregion Unity Frame Events

    #region Object Serial Port

    /// <summary>
    /// Opens the defined serial port and starts the serial thread used
    /// to catch and deal with serial events.
    /// </summary>
    public void OpenSerialPort()
    {
        try
        {
            // Initialise the serial port
            SerialPort = new SerialPort(ComPort, BaudRate);

            SerialPort.ReadTimeout = ReadTimeout;

            SerialPort.WriteTimeout = WriteTimeout;

            // Open the serial port
            SerialPort.Open();

            // Update the gui if applicable
            if (Instance != null && Instance.ComStatusText != null)
            { Instance.ComStatusText.GetComponent<GUIText>().text = "ComStatus: Open"; }

            if (UpdateMethod == LoopUpdateMethod.Threading)
            {
                // If the thread does not exist then start it
                if (serialThread == null)
                { StartSerialThread(); }
            }

            if (UpdateMethod == LoopUpdateMethod.Coroutine)
            {
                if (isRunning == false)
                {
                    StartSerialCoroutine();
                }
                else
                {
                    isRunning = false;

                    // Give it chance to timeout
                    Thread.Sleep(100);

                    try
                    {
                        // Kill it just in case
                        StopCoroutine("SerialCoroutineLoop");
                    }
                    catch (Exception ex)
                    {
                        print("Error N: " + ex.Message.ToString());
                    }

                    // Restart it once more
                    StartSerialCoroutine();
                }
            }

            print("SerialPort successfully opened!");

        }
        catch (Exception ex)
        {
            // Failed to open com port or start serial thread
            Debug.Log("Error 1: " + ex.Message.ToString());
        }
    }

    /// <summary>
    /// Cloases the serial port so that changes can be made or communication
    /// ended.
    /// </summary>
    public void CloseSerialPort()
    {
        try
        {
            // Close the serial port
            SerialPort.Close();

            // Update the gui if applicable
            if (Instance.ComStatusText != null)
            { Instance.ComStatusText.GetComponent<GUIText>().text = "ComStatus: Closed"; }
        }
        catch (Exception ex)
        {
            if (SerialPort == null || SerialPort.IsOpen == false)
            {
                // Failed to close the serial port. Uncomment if
                // you wish but this is triggered as the port is
                // already closed and or null.

                // Debug.Log("Error 2A: " + "Port already closed!");
            }
            else
            {
                // Failed to close the serial port
                Debug.Log("Error 2B: " + ex.Message.ToString());
            }
        }

        Debug.Log("Serial port closed!");
    }

    #endregion Object Serial Port

    #region Serial Coroutine

    /// <summary>
    /// Function used to start coroutine for reading serial 
    /// data.
    /// </summary>
    public void StartSerialCoroutine()
    {
        isRunning = true;

        StartCoroutine("SerialCoroutineLoop");
    }

    /// <summary>
    /// A Coroutine used to recieve serial data thus not 
    /// affecting generic unity playback etc.
    /// </summary>
    public IEnumerator SerialCoroutineLoop()
    {
        while (isRunning)
        {
            GenericSerialLoop();
            yield return null;
        }

        print("Ending Coroutine!");
    }

    /// <summary>
    /// Function used to stop the coroutine and kill
    /// off any instance
    /// </summary>
    public void StopSerialCoroutine()
    {
        isRunning = false;

        Thread.Sleep(100);

        try
        {
            StopCoroutine("SerialCoroutineLoop");
        }
        catch (Exception ex)
        {
            print("Error 2A: " + ex.Message.ToString());
        }

        // Reset the serial port to null
        if (SerialPort != null)
        { SerialPort = null; }

        // Update the port status... just in case :)
        portStatus = "Ended Serial Loop Coroutine!";

        print("Ended Serial Loop Coroutine!");
    }

    #endregion Serial Coroutine

    #region Serial Thread

    /// <summary>
    /// Function used to start seperate thread for reading serial 
    /// data.
    /// </summary>
    public void StartSerialThread()
    {
        try
        {
            // define the thread and assign function for thread loop
            serialThread = new Thread(new ThreadStart(SerialThreadLoop));
            // Boolean used to determine the thread is running
            isRunning = true;
            // Start the thread
            serialThread.Start();

            Debug.Log("Serial thread started!");
        }
        catch (Exception ex)
        {
            // Failed to start thread
            Debug.Log("Error 3: " + ex.Message.ToString());
        }
    }

    /// <summary>
    /// The serial thread loop. A Seperate thread used to recieve
    /// serial data thus not affecting generic unity playback etc.
    /// </summary>
    private void SerialThreadLoop()
    {
        while (isRunning)
        { GenericSerialLoop(); }

        Debug.Log("Ending Thread!");
    }

    /// <summary>
    /// Function used to stop the serial thread and kill
    /// off any instance
    /// </summary>
    public void StopSerialThread()
    {
        // Set isRunning to false to let the while loop
        // complete and drop out on next pass
        isRunning = false;

        // Pause a little to let this happen
        Thread.Sleep(100);

        // If the thread still exists kill it
        // A bit of a hack using Abort :p
        if (serialThread != null)
        {
            serialThread.Abort();
            // serialThread.Join();
            Thread.Sleep(100);
            serialThread = null;
        }

        // Reset the serial port to null
        if (SerialPort != null)
        { SerialPort = null; }

        // Update the port status... just in case :)
        portStatus = "Ended Serial Loop Thread";
        dataPosition = 0;
        data.Clear();
        Debug.Log("Ended Serial Loop Thread!");
    }

    #endregion Serial Thread 

    #region Static Functions


    /// <summary>
    /// Function used to send string data over serial without
    /// a line return included.
    /// </summary>
    /// <param name="data"></param>
    public void SendSerialByte(byte[] data)
    {
        Debug.Log("Sent data: " + BitConverter.ToString(data));
        if (SerialPort != null)

            try
            {
                { SerialPort.Write(data, 0, data.Length); }
            }
            catch (Exception timeout)
            {
                { SerialPort.Write(data, 0, data.Length); }
                Debug.Log("Ovdje sam pucao JEBEM SE U DUPE !!!!! ");
            }


    }

    #endregion Static Functions



    public void SerialFlush() {
        SerialPort.DiscardInBuffer();
    }

    private void GenericSerialLoop()
    {

        rData = 0;

        try
        {
            // Check that the port is open. If not skip and do nothing
            if (SerialPort.IsOpen)
            {

               Debug.Log("Vrtim!!!!");
                rData = (byte)SerialPort.ReadByte();
             //   Debug.Log("Citam!!!!" + (int)rData);
                if (rData.ToString() != "") {

                    parseData(rData);
                }



            }
        }
        catch (TimeoutException timeout)
        {
           // Debug.Log("Timeout error: " + timeout.Message.ToString());
           // Application.LoadLevel("Main");
        }
        catch (Exception ex)
        {
            // This could be thrown if we close the port whilst the thread 
            // is reading data. So check if this is the case!
            if (SerialPort.IsOpen)
            {
                // Something has gone wrong!
                Debug.Log("Error 4: " + ex.Message.ToString());
            }
            else
            {
                Debug.Log("Error 5: Port Closed Exception!");
            }
        }
    }


    private void parseData(byte chunk) {

      
        if (chunk == STX && !isSTX)
        {
            isSTX = true;
            CRC = chunk;
            msgPoss = 0;
          
        }
        if (isSTX)
        {
          //  Debug.Log("broj pozicije bita = " + msgPoss);
            data.Add(chunk);
            //debugData[dataPosition] = chunk;
            //dataPosition++;
           // DebugMsg(debugData);
            
         
          
            if (msgPoss == 1)
            {
               
                lenght = data[1];
                Debug.Log("duzina = " + lenght);

            }
            if (msgPoss == lenght && lenght > 0)
            {
               
                for (int i= 1; i<lenght-1;i++)
                {
                    CRC = (byte)(CRC ^ data[i]);
                }
                if (CRC == data[msgPoss])
                {
                  //  Debug.Log("Ocitani CRC = " + data[msgPoss] + " Izracunati CRC = " + CRC);
                }
            }
            if (msgPoss == (lenght + 1) && ETX == data[lenght + 1])
            {
                
                switch (data[2]) {
                    case 0xBB:
                        isSTX = false;
                     //   Debug.Log("RESPOND  = " + DebugMsg(data.ToArray()));
                        Respond = data;
                        ResetData();
                        break;
                    case 0xDD:
                        isSTX = false;
                      //  Debug.Log("RESPOND  = " + DebugMsg(data.ToArray()));
                        Respond = data;
                        ResetData();
                        break;
                    case 0xAA:
                       
                        switch (data[3])
                        {
                            case 1:
                                isSTX = false;
                             //   Debug.Log("Grana1  = " + DebugMsg(data.ToArray()));
                                Grana1Data = data;
                                // Debug.Log("Dobio g1 = " + DebugMsg(Grana1Data.ToArray()));
                                StartCoroutine(SetData(Grana1Data, m_Grana1));
                                ResetData();
                                
                                break;
                            case 2:
                                isSTX = false;
                             //   Debug.Log("Grana2  = " + DebugMsg(data.ToArray()));
                                Grana2Data = data;
                                StartCoroutine(SetData(Grana2Data, m_Grana2));
                                ResetData();
                                break;
                            case 3:
                                isSTX = false;
                              //  Debug.Log("Grana3  = " + DebugMsg(data.ToArray()));
                                Grana3Data = data;
                                StartCoroutine(SetData(Grana3Data, m_Grana3));
                                ResetData();
                                break;
                            case 4:
                                isSTX = false;
                              //  Debug.Log("Grana4  = " + DebugMsg(data.ToArray()));
                                Grana4Data= data;
                                StartCoroutine(SetData(Grana4Data, m_Grana4));
                                ResetData();
                                break;
                            case 5:
                                isSTX = false;
                               // Debug.Log("Grana5  = " + DebugMsg(data.ToArray()));
                                Grana5Data= data;
                                StartCoroutine(SetData(Grana5Data, m_Grana5));
                               ResetData();
                                break;
                        }
                        ResetData();
                        break;
                }
                ResetData();
            }
            msgPoss++;
            if (msgPoss > 9) msgPoss = 0;
        }
        if(data.Count > 9)
        {
            data.Clear();
            isSTX = false;
            isETX = false;
        }
    }

    private void ResetData()
    {
        // DebugMsg(data.ToArray());
        data.Clear();
        msgPoss = -1;
        // debugData.Initialize();
        isETX = true;
        isETX = false;
    }

    private string DebugMsg(byte[] chunk)
    {
        string log = "0x";
        log += BitConverter.ToString(chunk).Replace("-", " 0x");
       // Debug.Log(log);
        return log;
    }

    public void PopulateComPorts()
    {
        // Loop through all available ports and add them to the list
        foreach (string cPort in System.IO.Ports.SerialPort.GetPortNames())
        {
            comPorts.Add(cPort); // Debug.Log(cPort.ToString());
        }

        // Update the port status just in case :)
        portStatus = "ComPort list population complete";
    }

    /// <summary>
    /// Function used to update the current selected com port
    /// </summary>
    public string UpdateComPort()
    {
        // If open close the existing port
        if (SerialPort != null && SerialPort.IsOpen)
        { CloseSerialPort(); }

        // Find the current id of the existing port within the
        // list of available ports
        int currentComPort = comPorts.IndexOf(ComPort);

        // check against the list of ports and get the next one.
        // If we have reached the end of the list then reset to zero.
        if (currentComPort + 1 <= comPorts.Count - 1)
        {
            // Inc the port by 1 to get the next port
            ComPort = (string)comPorts[currentComPort + 1];
        }
        else
        {
            // We have reached the end of the list reset to the
            // first available port.
            ComPort = (string)comPorts[0];
        }

        // Update the port status just in case :)
        portStatus = "ComPort set to: " + ComPort.ToString();

        // Return the new ComPort just in case
        return ComPort;
    }

    /// <summary>
    /// Function used to update the current baudrate
    /// </summary>
    public int UpdateBaudRate()
    {
        // If open close the existing port
        if (SerialPort != null && SerialPort.IsOpen)
        { CloseSerialPort(); }

        // Find the current id of the existing rate within the
        // list of defined baudrates
        int currentBaudRate = baudRates.IndexOf(BaudRate);

        // check against the list of rates and get the next one.
        // If we have reached the end of the list then reset to zero.
        if (currentBaudRate + 1 <= baudRates.Count - 1)
        {
            // Inc the rate by 1 to get the next rate
            BaudRate = (int)baudRates[currentBaudRate + 1];
        }
        else
        {
            // We have reached the end of the list reset to the
            // first available rate.
            BaudRate = (int)baudRates[0];
        }

        // Update the port status just in case :)
        portStatus = "BaudRate set to: " + BaudRate.ToString();

        // Return the new BaudRate just in case
        return BaudRate;
    }
    public void setState(byte branchID, byte sensorID, bool state)
    {
        this.lenght = 0x05;
        if (state)
        {
            this.comm = 0xBB;
        }
        else
        {
            this.comm = 0xDD;
        }
        this.branchID = branchID;
        this.sensorID = sensorID;
        this.crc = (byte)(STX ^ lenght ^ comm ^ branchID ^ sensorID);
        msg = new byte[7];
        msg[0] = this.STX;
        msg[1] = this.lenght;
        msg[2] = this.comm;
        msg[3] = this.branchID;
        msg[4] = this.sensorID;
        msg[5] = this.crc;
        msg[6] = this.ETX;
        status = "change";
       // SendSerial(msg,status);

    }

    public void checkStatus()
    {
        this.lenght = 0x03;
        this.comm = 0xAA;
        this.crc = (byte)(STX ^ lenght ^ comm);
        msg = new byte[5];
        msg[0] = this.STX;
        msg[1] = this.lenght;
        msg[2] = this.comm;
        msg[3] = this.crc;
        msg[4] = this.ETX;
        status = "pool";
        SendSerialByte(msg);

    }

    public void SendSerial(byte[] msg, string status)
    {
            SendSerialByte(msg);
    }

    IEnumerator StateChange()
    {
        yield return new WaitForSeconds(1);
       // setState();

    }

    private void setUP (List<byte> granaData, List<GameObject> granaLista)
    {
        //StartCoroutine(SetData(granaData, granaLista));
        if (granaData.Count > 0 && granaLista.Count > 0)
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 3; i++)
            {
                data[i] = granaData[7-i];
            }

            SenzorScript update;
            BitArray singleData = new BitArray(data);
            int pos = 0;

            foreach (GameObject g in granaLista)
            {
                update = g.GetComponent<SenzorScript>() as SenzorScript;
                update.Occupied = singleData[pos];
                print("Pozicija " + pos + " Vrijednost " + update.Occupied.ToString());
                pos++;
            }
        }
    }

    IEnumerator SetData(List<byte> granaData, List<GameObject> granaLista)
    {
        if (granaData.Count > 0 && granaLista.Count > 0)
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 3; i++)
            {
                data[i] = granaData[7-i];
            }

            //SenzorScript update;
            BitArray singleData = new BitArray(data);
            int pos = 0;

            foreach (GameObject g in granaLista)
            {
                g.GetComponent<SenzorScript>().Occupied = singleData[pos];
                //update.Occupied = singleData[pos];
                pos++;
            }
        }
        yield return new WaitForSeconds(0.2f);
    } 
    IEnumerator Pool()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (status.Equals("pool")) checkStatus();
            if (status.Equals("change"))
            {
                SendSerialByte(msg);
                status = "pool";
            }
            yield return new WaitForSeconds(1);
           
        }

    }
}
