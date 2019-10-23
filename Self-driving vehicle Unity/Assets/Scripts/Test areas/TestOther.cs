using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;

public class TestOther : MonoBehaviour 
{
    public void Start()
    {
        //float start = 0;
    
        for (int i = 0; i < 360; i++)
        {
            Debug.Log(HelpStuff.RoundValue(i, 20f));

            //start
        }
    }


}
