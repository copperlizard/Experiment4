using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class OrbitCam : MonoBehaviour
{
    public GameObject m_target; //m_target == cam follow target

    //public GameManager m_GameManager;

    public RaycastHit m_hit; //player target

    public Vector3 m_zoomTarShift = new Vector3(0.2f, 0.0f, 0.0f);

    public float m_minDist = 0.0f, m_maxDist = 100.0f, m_startDist = 5.0f, m_zoomDist = 1.8f, 
        m_minTilt = -50.0f, m_maxTilt = 45.0f, m_hidePlayerDist = 0.5f, m_rotSpeed = 2.5f, 
        m_damp = 0.0001f, m_fudge = 0.05f;

    public bool m_HideCursor = true, m_rightClickZoom = true;
    
    [HideInInspector]
    public Camera m_thisCam;

    [HideInInspector]
    public LayerMask m_thisLayerMask;

    [HideInInspector]
    public float m_dist, m_h, m_v, m_d;

    [HideInInspector]
    public bool m_hitCurrent = false;

    private RaycastHit m_interAt;

    private Quaternion m_rot;

    private Vector3 m_curVel = Vector3.zero;

    private bool m_playerHidden = false, m_zoom = false;

	// Use this for initialization
	public virtual void Start ()
    {
        m_thisCam = GetComponent<Camera>();
        m_thisLayerMask = ~LayerMask.GetMask("Player", "PlayerBubble", "Ignore Raycast", "Enemy");

        m_dist = m_startDist;
        transform.position = m_target.transform.position + new Vector3(0.0f, 0.0f, -m_dist);

        if(m_HideCursor)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
        
        /*
        m_GameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        if (m_GameManager == null)
        {
            Debug.Log("no game manager!");
        } 
        */            
	}

    // Update is called once per frame
    public virtual void Update ()
    {
        GetInput();  
	}

    // LateUpdate is called once per frame after Update
    void LateUpdate ()
    {   
        float tilt;
        if (m_v * m_rotSpeed < 180.0f)
        {
            tilt = m_v * m_rotSpeed;
        }
        else
        {
            tilt = (m_v * m_rotSpeed) - 360.0f;
        }
        
        if (tilt >= m_maxTilt)
        {
            tilt = m_maxTilt;
            m_v = m_maxTilt / m_rotSpeed;
        }
        else if (tilt < m_minTilt)
        {
            tilt = m_minTilt;
            m_v = (m_minTilt + 360.0f) / m_rotSpeed;
        }

        if (tilt < 0.0f)
        {
            tilt += 360.0f;
        }

        m_rot = Quaternion.Euler(tilt, m_h * m_rotSpeed, 0.0f);
        
        // Find new cam position
        m_dist -= m_d;        
        m_dist = Mathf.Clamp(m_dist, m_minDist, m_maxDist);

        Vector3 tarPos;
        if (m_rightClickZoom)
        {
            if (!m_zoom)
            {
                tarPos = (m_rot * new Vector3(0.0f, 0.0f, -m_dist)) + m_target.transform.position;
            }
            else
            {
                tarPos = (m_rot * new Vector3(0.0f, 0.0f, -m_zoomDist)) + m_target.transform.position + m_target.transform.TransformVector(m_zoomTarShift);
            }
        }
        else
        {
            tarPos = (m_rot * new Vector3(0.0f, 0.0f, -m_dist)) + m_target.transform.position;
        }        

        //Check for sight line intersection
        tarPos = IntersectCheck(tarPos);

        //Hide player and weapons if camera too close
        float distFromPlayer = (m_target.transform.position - tarPos).magnitude;
        
        if (distFromPlayer <= m_hidePlayerDist && !m_playerHidden)
        {
            //Debug.Log("Hiding player!");

            m_playerHidden = true;
            m_thisCam.cullingMask = m_thisLayerMask;
        }
        else if (distFromPlayer > m_hidePlayerDist && m_playerHidden)
        {
            //Debug.Log("Un Hiding player!");
            
            m_playerHidden = false;
            m_thisCam.cullingMask = ~0;
        }

        //Move camera
        transform.position = Vector3.SmoothDamp(transform.position, tarPos, ref m_curVel, m_damp);
        transform.rotation = m_rot;

        //Find "hit"
        if(!Physics.Raycast(transform.position, transform.forward, out m_hit, m_maxDist, m_thisLayerMask))
        {
            m_hit.point = transform.position + transform.forward * m_maxDist;
            m_hit.normal = Vector3.up;
            m_hitCurrent = false;
        }
        else
        {
            m_hitCurrent = true;
        }        
    }

    public void GetInput ()
    {
        /*
        if (!m_GameManager.m_paused)
        {
            bool aiming = Input.GetMouseButton(1);
            m_h += Input.GetAxis("Mouse X") * ((aiming) ? 0.5f : 1.0f);
            m_v -= Input.GetAxis("Mouse Y") * ((aiming) ? 0.5f : 1.0f);
            //m_d = Input.GetAxis("Mouse ScrollWheel");

            m_zoom = Input.GetButton("Fire2");
        }    
        */

        bool aiming = Input.GetMouseButton(1);
        m_h += Input.GetAxis("Mouse X") * ((aiming) ? 0.5f : 1.0f);
        m_v -= Input.GetAxis("Mouse Y") * ((aiming) ? 0.5f : 1.0f);
        m_d = Input.GetAxis("Mouse ScrollWheel");

        m_zoom = Input.GetButton("Fire2");
    }

    Vector3 IntersectCheck (Vector3 target)
    {
        //If intersection (cast ray from camera to player)
        if (Physics.Raycast(m_target.transform.position, target - m_target.transform.position, out m_interAt, (!m_zoom) ? m_dist : m_zoomDist, m_thisLayerMask))
        {   
#if UNITY_EDITOR
            Debug.DrawLine(m_target.transform.position, m_interAt.point, Color.yellow, 0.01f, true);
#endif      

            target = m_interAt.point + (m_interAt.normal * m_fudge);
        }

        return target;
    }

    public void SetCamDist (float dist)
    {
        m_dist = Mathf.Clamp(dist, m_minDist, m_maxDist);
    }

    public float GetCamDist ()
    {
        return m_dist;
    }

#if UNITY_EDITOR
    void OnDrawGizmos ()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_hit.point, 0.05f);            
        }
    }
#endif

    float ClampAngle (float ang, float min, float max)
    {
        if (ang < -360.0f)
        {
            ang += 360.0f;
        }
        else if (ang > 360.0f)
        {
            ang -= 360.0f;
        }
        return Mathf.Clamp(ang, min, max);
    }    
}
