using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CountScript : MonoBehaviour {

   
    private int occupied = 0;
    private int total = 0;
    private int free = 0;
    public Text m_Counter_text;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        occupied = 0;
        total = 0;
        free = 0;
        SenzorScript[] sensorScript = GetComponentsInChildren<SenzorScript>(); 
       // GameObject[] senzor = this.gameObject.
        foreach (SenzorScript ss in sensorScript)
        {
          
                if (ss.Occupied) occupied++;
                total++;
                free = total - occupied;
            
           
        }
        m_Counter_text.text = free.ToString();
	
	}
}
