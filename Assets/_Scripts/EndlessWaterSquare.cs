using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Creates an endless water system with squares
public class EndlessWaterSquare : MonoBehaviour
{
    //The object the water will follow
    public GameObject m_boatObj;

    //One water square
    public GameObject m_waterSqrObj;

    //Water square data
    private float m_squareWidth = 800f, m_innerSquareResolution = 5f, m_outerSquareResolution = 25f; //inner = 5, outer = 25

    //The list with all water mesh squares == the entire ocean we can see
    private List<WaterSquare> m_waterSquares = new List<WaterSquare>();

    //Stuff needed for the thread
    //The timer that keeps track of seconds since start to update the water because we cant use Time.time in a thread
    private float m_secondsSinceStart;

    //The position of the boat
    private Vector3 m_boatPos;

    //The position of the ocean has to be updated in the thread because it follows the boat
    //Is not the same as pos of boat because it moves with the same resolution as the smallest water square resolution
    private Vector3 m_oceanPos;

    //Has the thread finished updating the water so we can add the stuff from the thread to the main thread
    private bool m_hasThreadUpdatedWater;

    void Start()
    {
        //Create the sea
        CreateEndlessSea();

        //Init the time
        m_secondsSinceStart = Time.time;        
    }

    void Update()
    {
        //Update the time since start to get correct wave height which depends on time since start
        m_secondsSinceStart = Time.time;

        UpdateWater();
    }

    //Update the water 
    void UpdateWater()
    {
        //Update the position of the boat
        m_boatPos = m_boatObj.transform.position;

        //Move the water to the boat
        //MoveWaterToBoat();
        m_oceanPos = new Vector3(m_boatPos.x, m_oceanPos.y, m_boatPos.z);

        //Add the new position of the ocean to this transform
        transform.position = m_oceanPos;

        WaterController.m_current.m_oceanCenterPos = new Vector4(m_oceanPos.x, m_oceanPos.y, m_oceanPos.z, 0.0f);
        
        /*
        //Update the vertices
        for (int i = 0; i < m_waterSquares.Count; i++)
        {
            m_waterSquares[i].MoveSea(m_oceanPos, Time.time);
        }
        */
                
    }

    //Move the endless water to the boat's position in steps that's the same as the water's resolution
    void MoveWaterToBoat()
    {
        //Round to nearest resolution
        float x = m_innerSquareResolution * (int)Mathf.Round(m_boatPos.x / m_innerSquareResolution);
        float z = m_innerSquareResolution * (int)Mathf.Round(m_boatPos.z / m_innerSquareResolution);

        //Should we move the water?
        if (m_oceanPos.x != x || m_oceanPos.z != z)
        {
            Debug.Log("Moved sea");
            m_oceanPos = new Vector3(x, m_oceanPos.y, z);
        }
    }

    //Init the endless sea by creating all squares
    void CreateEndlessSea()
    {
        //The center piece
        AddWaterPlane(0f, 0f, 0.0f, m_squareWidth, m_innerSquareResolution);

        //The 8 squares around the center square
        for (int x = -1; x <= 1; x += 1)
        {
            for (int z = -1; z <= 1; z += 1)
            {
                //Ignore the center pos
                if (x == 0 && z == 0)
                {
                    continue;
                }

                //The y-Pos should be lower than the square with high resolution to avoid an ugly seam
                float yPos = -0.5f;
                AddWaterPlane(x * m_squareWidth, z * m_squareWidth, yPos, m_squareWidth, m_outerSquareResolution);
            }
        }
    }

    //Add one water plane
    void AddWaterPlane(float xCoord, float zCoord, float yPos, float squareWidth, float spacing)
    {
        //ocean center
        Vector3 centerPos = transform.position;

        //Patch coords relative to ocean
        centerPos.x += xCoord;
        centerPos.y = yPos;
        centerPos.z += zCoord;

        //GameObject waterPlane = Instantiate(m_waterSqrObj, centerPos, transform.rotation) as GameObject;
        GameObject waterPlane = Instantiate(m_waterSqrObj) as GameObject;

        waterPlane.SetActive(true);

        //Debug.Log("New plane Instantiated at " + waterPlane.transform.position.ToString() + " with rotation " + waterPlane.transform.rotation.ToString());

        waterPlane.transform.position = centerPos;
        
        //Debug.Log("New plane moved to " + waterPlane.transform.position.ToString() + " with rotation " + waterPlane.transform.rotation.ToString());
        
        //Parent it
        waterPlane.transform.parent = transform;

        //Give it moving water properties and set its width and resolution to generate the water mesh
        WaterSquare newWaterSquare = new WaterSquare(waterPlane, squareWidth, spacing);

        m_waterSquares.Add(newWaterSquare);
    }
}