using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    int coins = 0;
    int lives = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void coinAdd()
    {
        coins++;
        if(coins>99)
        {
            coins = 0;
            lives += 1;
        }
    }
}
