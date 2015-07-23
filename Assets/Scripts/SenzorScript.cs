using UnityEngine;
using System.Collections;

public class SenzorScript : MonoBehaviour {

   UnitySerialPort communications;
    public GameObject comPort;
   // public byte grana;
    //public byte adresa;
	private Color red = new Vector4(170F, 0f, 0F, 0.8f);
    private Color green = new Vector4(0F, 120f, 0F, 0.5f);
    private bool isManualyRed = false;
    private bool occupied = false;
    public enum e_Grana
    { m_1, m_2, m_3, m_4, m_5}
    public e_Grana m_Grana = e_Grana.m_1;
    public enum e_Adresa
    { m_1, m_2, m_3, m_4, m_5, m_6, m_7, m_8, m_9, m_10,
        m_11, m_12, m_13, m_14, m_15, m_16, m_17, m_18, m_19, m_20,
        m_21, m_22, m_23, m_24, m_25, m_26, m_27, m_28, m_29, m_30,
    }
    public e_Adresa m_Adresa = e_Adresa.m_1;

    public bool Occupied
    {
        get
        {
            return occupied;
        }

        set
        {
            occupied = value;
        }
    }

    void Start () {
		/*if (!Occupied) {
			Occupied = !Occupied;
			GetComponent<SpriteRenderer> ().color = green;
           
		}*/
        StartCoroutine(CheckStatus());

    }
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
		if (hit != null && hit.collider != null) {
			OnMouseDown();
		}
        communications = comPort.GetComponent<UnitySerialPort>() as UnitySerialPort;
        if (!communications.isActiveAndEnabled)
        {
            if (!Occupied)
            {
                GetComponent<SpriteRenderer>().color = green;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = red;
            }
        }
    }

	void OnMouseDown() {
        communications = comPort.GetComponent<UnitySerialPort>() as UnitySerialPort;
        if (communications.isActiveAndEnabled)
        {
            if (!Occupied)
            {
                //Occupied = !Occupied;
                setRed();
                isManualyRed = true;


            }
        }
        else
        {

            Occupied = !Occupied;
        }
	}

    IEnumerator CheckStatus()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (!Occupied && !isManualyRed)
            {
                GetComponent<SpriteRenderer>().color = green;
            }
            else if (Occupied && isManualyRed)
            {
                GetComponent<SpriteRenderer>().color = red;
                if (isManualyRed)
                {
                    setAuto();

                }
            }
            else if (isManualyRed)
            {
                GetComponent<SpriteRenderer>().color = Color.white;
                yield return new WaitForSeconds(0.5f);
                GetComponent<SpriteRenderer>().color = green;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = red;
            }
            yield return new WaitForSeconds(1);
        }
       
       
    }

    void setRed()
    {
        communications = comPort.GetComponent<UnitySerialPort>() as UnitySerialPort;
        if (communications.isActiveAndEnabled)
        {
            communications.setState((byte)(m_Grana + 1), (byte)(m_Adresa + 1), true);
        }
    }

    void setAuto()
    {
        isManualyRed = !isManualyRed;
        communications = comPort.GetComponent<UnitySerialPort>() as UnitySerialPort;
        if (communications.isActiveAndEnabled)
        {
            communications.setState((byte)(m_Grana + 1), (byte)(m_Adresa + 1), false);
        }
    }

}
