/*
Klasa koja upravlja znakovima
*/

using UnityEngine;
using System.Collections;

public class SignScript : MonoBehaviour {

    UnitySerialPort communications;
    public GameObject comPort;
    public GameObject arrow;
    public GameObject xSign;
    private bool occupied = false;

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

    // Use this for initialization

    void Start () {
          
        }


    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit != null && hit.collider != null)
        {
            OnMouseDown();
        }

        if (!Occupied)
        {
            arrow.SetActive(true);
            xSign.SetActive(false);
        }
        else
        {
            arrow.SetActive(false);
            xSign.SetActive(true);
        }


    }

    void OnMouseDown()
    {

        communications = comPort.GetComponent<UnitySerialPort>() as UnitySerialPort;
        Debug.Log("Mouse Down");
        if (!Occupied)
        {
            Occupied = !Occupied;
            arrow.SetActive(true);
            xSign.SetActive(false);
            if (communications.isActiveAndEnabled)
            {
                communications.setState(0x01, 0x23, Occupied);
            }
        }
        else
        {
            Occupied = !Occupied;
            arrow.SetActive(false);
            xSign.SetActive(true);
            if (communications.isActiveAndEnabled)
            {
                communications.setState(0x1, 0x23, Occupied);
            }
        }
    }
}
