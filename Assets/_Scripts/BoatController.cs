using UnityEngine;
using System.Collections;

public class BoatController : MonoBehaviour
{    
    public BoatMotor m_boatMotor;

    private float m_h, m_v;

	// Use this for initialization
	void Start ()
    {
                
	}
	
	// Update is called once per frame
	void Update ()
    {
        GetInput();

        m_boatMotor.Move(m_h, m_v);
	}

    void GetInput ()
    {
        m_h = Input.GetAxis("Horizontal");
        m_v = Input.GetAxis("Vertical");        
    }
}
