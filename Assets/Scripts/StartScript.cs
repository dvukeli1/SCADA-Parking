using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Threading;

public class StartScript : MonoBehaviour {

    public GameObject camera;
    public GameObject m_Etaza0;
    public GameObject m_Etaza1;
    public GameObject m_Etaza2;
    public GameObject m_Etaza3;
    public GameObject m_Sve_etaze;
    private Camera camera1;
    private Camera camera2;


    // Use this for initialization
    void Start () {
        camera.SetActive(true);
	}


    public float fadeTime = 0.5f;
    public float waveScale = .07f;              // Higher numbers make the effect more exaggerated. Can be negative, .5/-.5 is the max
    public float waveFrequency = 25.0f;
    bool inProgress = false;
    bool swap = false;


    //Fade metoda promjena etaza
    IEnumerator DoFade()
    {
       
        if (inProgress == true)
        {
            yield return false;
        }

      

        inProgress = true;

        swap = !swap;

        yield return StartCoroutine(ScreenWipe.use.DreamWipe(swap ? camera1 : camera2, swap ? camera2 : camera1, fadeTime, waveScale, waveFrequency));

        inProgress = false;

    
    }


    //Keycode za projenu etaza
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            Application.LoadLevel("Main");
        }

        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            //m_Sve_etaze.SetActive(true);
            m_Etaza0.SetActive(true);
            m_Etaza1.SetActive(false);
            m_Etaza2.SetActive(false);
            m_Etaza3.SetActive(false);
            m_Sve_etaze.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            m_Etaza0.SetActive(false);
            m_Etaza1.SetActive(true);
            m_Etaza2.SetActive(false);
            m_Etaza3.SetActive(false);
            m_Sve_etaze.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            m_Etaza0.SetActive(false);
            m_Etaza1.SetActive(false);
            m_Etaza2.SetActive(true);
            m_Etaza3.SetActive(false);
            m_Sve_etaze.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F4) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            m_Etaza0.SetActive(false);
            m_Etaza1.SetActive(false);
            m_Etaza2.SetActive(false);
            m_Etaza3.SetActive(true);
            m_Sve_etaze.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            m_Etaza0.SetActive(false);
            m_Etaza1.SetActive(false);
            m_Etaza2.SetActive(false);
            m_Etaza3.SetActive(false);
            m_Sve_etaze.SetActive(true);
        }
    }
}
