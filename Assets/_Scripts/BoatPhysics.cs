using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BoatSpace
{
    public class BoatPhysics : MonoBehaviour
    {
        //Drags
        public GameObject m_underWaterObj;

        //Script that's doing everything needed with the boat mesh, such as finding out which part is above the water
        private ModifyBoatMesh m_modifyBoatMesh;

        //Mesh for debugging
        private Mesh m_underWaterMesh;

        //The boat's rigidbody
        private Rigidbody m_boatRB;

        //The density of the water the boat is traveling in
        private float m_rhoWater = 1027f;

        void Start()
        {
            //Get the boat's rigidbody
            m_boatRB = gameObject.GetComponent<Rigidbody>();

            //Init the script that will modify the boat mesh
            m_modifyBoatMesh = new ModifyBoatMesh(gameObject);

            //Meshes that are below and above the water
            m_underWaterMesh = m_underWaterObj.GetComponent<MeshFilter>().mesh;
        }

        void Update()
        {
            //Generate the under water mesh
            m_modifyBoatMesh.GenerateUnderwaterMesh();

            //Display the under water mesh
            m_modifyBoatMesh.DisplayMesh(m_underWaterMesh, "UnderWater Mesh", m_modifyBoatMesh.m_underWaterTriangleData);
        }

        void FixedUpdate()
        {
            //Add forces to the part of the boat that's below the water
            if (m_modifyBoatMesh.m_underWaterTriangleData.Count > 0)
            {
                AddUnderWaterForces();
            }
        }

        //Add all forces that act on the squares below the water
        void AddUnderWaterForces()
        {
            //Get all triangles
            List<TriangleData> underWaterTriangleData = m_modifyBoatMesh.m_underWaterTriangleData;

            for (int i = 0; i < underWaterTriangleData.Count; i++)
            {
                //This triangle
                TriangleData triangleData = underWaterTriangleData[i];

                //Calculate the buoyancy force
                Vector3 buoyancyForce = BuoyancyForce(m_rhoWater, triangleData);

                //Add the force to the boat
                m_boatRB.AddForceAtPosition(buoyancyForce, triangleData.center);


                //Debug

                //Normal
                Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);

                //Buoyancy
                Debug.DrawRay(triangleData.center, buoyancyForce.normalized * -3f, Color.blue);
            }
        }

        //The buoyancy force so the boat can float
        private Vector3 BuoyancyForce(float rho, TriangleData triangleData)
        {
            //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

            // F_buoyancy = rho * g * V
            // rho - density of the mediaum you are in
            // g - gravity
            // V - volume of fluid directly above the curved surface 

            // V = z * S * n 
            // z - distance to surface
            // S - surface area
            // n - normal to the surface
            Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;

            //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
            buoyancyForce.x = 0f;
            buoyancyForce.z = 0f;

            return buoyancyForce;
        }
    }
}