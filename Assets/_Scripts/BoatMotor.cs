using UnityEngine;
using System.Collections;

public class BoatMotor : MonoBehaviour
{
    [Header("Motor Properties")]
    public float m_motorPower;
    public float m_movingTurnSpeed, m_stationaryTurnSpeed;

    private Camera m_cam;

    private Rigidbody m_boatRB;

    private float m_forwardAmount, m_turnTarAng;

    // Use this for initialization
    void Start()
    {
        m_cam = Camera.main;

        m_boatRB = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }    

    public void Move(float h, float v)
    {
        Vector3 move = new Vector3(h, 0.0f, v).normalized;

        m_forwardAmount = move.magnitude;

        if (m_forwardAmount > 0.0f)
        {
            // Rotate input to match camera
            move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y - transform.eulerAngles.y, 0.0f) * move;

            // Look "forward"
            Quaternion lookRot = Quaternion.LookRotation(move);
            m_turnTarAng = lookRot.eulerAngles.y;           
        }
        else
        {
            //Stop rotating
            m_turnTarAng = transform.rotation.eulerAngles.y;
        }

        float turn = m_turnTarAng - transform.localRotation.eulerAngles.y;

        /*
        if (Mathf.Abs(turn) > 180.0f)
        {
            if (transform.rotation.eulerAngles.y < m_turnTarAng)
            {
                turn = m_turnTarAng - (transform.rotation.eulerAngles.y + 360.0f);
            }
            else
            {
                turn = (m_turnTarAng + 360.0f) - transform.rotation.eulerAngles.y;
            }
        }
        turn /= 180.0f;
        */

        //Debug.Log("turn == " + turn.ToString());

        Vector3 heading = Quaternion.Euler(0.0f, turn, 0.0f) * (transform.forward * m_forwardAmount);
        heading = transform.InverseTransformVector(heading);

        Vector3 force = new Vector3(heading.x, heading.y, -heading.z) * m_motorPower * Time.deltaTime;
        //Vector3 force = Quaternion.Euler(0.0f, 180.0f, 0.0f) * heading * m_motorPower * Time.deltaTime;
        force = transform.TransformVector(force);

        //Debug.DrawLine(transform.TransformPoint(transform.position), transform.TransformPoint(transform.position) + (transform.forward * m_motorPower * m_forwardAmount), Color.red, 0.1f, false);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * 2.0f * m_forwardAmount), Color.red, 0.1f, false);
        Debug.DrawLine(transform.position, transform.position + heading * 5.0f, Color.blue, 0.1f, false);
        Debug.DrawLine(transform.position, transform.position + force, Color.yellow, 0.1f, false);

        m_boatRB.AddForceAtPosition(-force, transform.position);    
    }
}
