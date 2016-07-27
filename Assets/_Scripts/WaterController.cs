using UnityEngine;
using System.Collections;

//Controlls the water
public class WaterController : MonoBehaviour
{
    public static WaterController m_current;

    public bool m_isMoving;

    [HideInInspector]
    public Vector4 m_oceanCenterPos;

    //Wave height and speed
    public float m_scale = 0.1f;
    public float m_speed = 1.0f;
    //The width between the waves
    public float m_waveDistance = 1f;
    //Noise parameters
    public float m_noiseStrength = 1f;
    public float m_noiseWalk = 1f;

    private Mesh m_mesh;

    void Start()
    {
        m_current = this;

        if (!m_isMoving)
        {
            m_scale = 0.0f;
            m_speed = 0.0f;
        }

        m_mesh = GetComponent<MeshFilter>().mesh;
    }

    void Update()
    {
        Vector4 bounds = new Vector4(m_mesh.bounds.size.x, m_mesh.bounds.size.y, m_mesh.bounds.size.z, 0.0f);

        Shader.SetGlobalVector("_WaterCenterPos", m_oceanCenterPos);
        Shader.SetGlobalVector("_WaterBounds", bounds);
               
        Shader.SetGlobalFloat("_WaterScale", m_scale);
        Shader.SetGlobalFloat("_WaterSpeed", m_speed);
        Shader.SetGlobalFloat("_WaterDistance", m_waveDistance);
        Shader.SetGlobalFloat("_WaterTime", Time.time);
        Shader.SetGlobalFloat("_WaterNoiseStrength", m_noiseStrength);
        Shader.SetGlobalFloat("_WaterNoiseWalk", m_noiseWalk);                
    }

    //Get the y coordinate from whatever wavetype we are using
    public float GetWaveYPos(Vector3 position, float timeSinceStart)
    {
        if (m_isMoving)
        {
            return WaveTypes.SinXWave(position - new Vector3(m_oceanCenterPos.x, m_oceanCenterPos.y, m_oceanCenterPos.z), m_speed, m_scale, m_waveDistance, m_noiseStrength, m_noiseWalk, timeSinceStart);
        }
        else
        {
            return 0f;
        }
    }

    //Find the distance from a vertice to water
    //Make sure the position is in global coordinates
    //Positive if above water
    //Negative if below water
    public float DistanceToWater(Vector3 position, float timeSinceStart)
    {
        float waterHeight = GetWaveYPos(position, timeSinceStart);

        float distanceToWater = position.y - waterHeight;

        return distanceToWater;
    }
}