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
            move = Quaternion.Euler(0.0f, m_cam.transform.eulerAngles.y, 0.0f) * move;

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

        Debug.Log("turn == " + turn.ToString());        

        float turnSpeed = Mathf.Lerp(m_stationaryTurnSpeed, m_movingTurnSpeed, m_forwardAmount);
        //transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);

        m_boatRB.AddForceAtPosition(transform.forward * m_motorPower * m_forwardAmount, transform.TransformPoint(transform.position));    
    }
}
