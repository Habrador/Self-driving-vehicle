using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathfindingForVehicles;



namespace SelfDrivingVehicle
{
    public class PIDController
    {
        //What the PID controller needs to save each frame
        private float error_old = 0f;
        private float error_sum = 0f;



        //Calculate the steer angle alpha by using a PDI controller
        public float GetNewValue(float error, PID_Parameters pid_parameters)
        {
            //The value we want to change with the PID controller to minimize rhe error, such as the steering angle
            float alpha = 0f;


            //P
            alpha = -pid_parameters.P * error;


            //I
            //The sum is the average of the last 1000 values
            error_sum = HelpStuff.AddValueToAverage(error_sum, Time.deltaTime * error, 1000f);

            alpha -= pid_parameters.I * error_sum;


            //D
            float d_dt_CTE = (error - error_old) / Time.deltaTime;

            alpha -= pid_parameters.D * d_dt_CTE;


            //Save for next loop
            error_old = error;


            return alpha;
        }
    }


    [System.Serializable]
    public struct PID_Parameters
    {
        public float P; //70
        public float I; //0.01
        public float D; //50

        public PID_Parameters(float P, float I, float D)
        {
            this.P = P;
            this.I = I;
            this.D = D;
        }
    }
}
