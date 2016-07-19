using UnityEngine;
using System.Collections;

public class InstantiateTest : MonoBehaviour
{
    public GameObject m_testObj;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(Delay(2.0f));
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    IEnumerator Delay (float time)
    {
        yield return new WaitForSeconds(time);
        Instantiate(m_testObj);
    }
}
