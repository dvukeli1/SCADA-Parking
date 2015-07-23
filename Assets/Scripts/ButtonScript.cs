/*
"Mrtva" skripta  - buttoni su izbaceni
*/

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour {
    public GameObject m_Etaza;
    public GameObject m_Sve_etaze;
    private bool isEtaza = false;
    public int Etaza;
    // Use this for initialization
    void Start () {
   
}
	

    public void OnMouseDown()
    {
        isEtaza = !isEtaza;
        if (isEtaza)
        {
            m_Etaza.SetActive(true);
            m_Sve_etaze.SetActive(false);
        }
        else
        {
            m_Etaza.SetActive(false);
            m_Sve_etaze.SetActive(true);
        }
    }
}
