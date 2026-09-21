using UnityEngine;

public class OSCPrinter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
              Debug.Log("start");  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void printOSC(float value)
    {
        Debug.Log("value"+value);
    }
}
