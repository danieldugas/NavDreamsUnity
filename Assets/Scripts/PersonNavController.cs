using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonNavController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoNavStep()
    {
        this.transform.Translate(new Vector3(0.0f, 0.0f, 0.01f));
        this.transform.Rotate(new Vector3(0.0f, 0.2f, 0.0f));
        this.GetComponent<Animator>().SetFloat("Speed", 0.1f);
    }
    
}
