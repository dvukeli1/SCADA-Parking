using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SignAutomationScript : MonoBehaviour {

    public Text info;
    public GameObject arrow;
    public GameObject xSign;
    private bool manual = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit != null && hit.collider != null)
        {
            OnMouseDown();
        }
        SignScript script = this.gameObject.GetComponent<SignScript>() as SignScript;
        if (info.text.Equals("0") || manual )
        {
            script.Occupied = true;
        }
        else
        {
            script.Occupied = false;
        }

        if(info.text.Equals("0") && manual)
        {
            manual = !manual;
        }
	}

    void OnMouseDown()
    {
        manual = !manual;
    }
}
