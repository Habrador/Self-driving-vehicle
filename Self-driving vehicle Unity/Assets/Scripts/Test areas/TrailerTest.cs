using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used when testing the skeleton car algorithms
public class TrailerTest : MonoBehaviour
{
    //Cant be independent of the drag vehicle because we need the rotation of the drag vehicle
    //previous update to update the trailer
    //The pivot point of the trailer is always where it's attached to the drag vehicle
    //Dimensions
    //private float width = 2f;
    //private float length = 7f;
    //The position of the rear wheels in relation to where the trailer is attached
    [System.NonSerialized]
    public float rearWheelZOffset = -6f;
    //The position of the rear of the trailer body in relation to the attachment point
    //Now we can calculate the front position of the body, which may extend further than 
    //the attachment point if we have a semi with a trailer
    //private float bodyRearZOffset = -7f;
    //The attachment point if we want to attach a trailer, in realtion to the pivot point
    //which is where the trailer is attached to whats infont of it
    [System.NonSerialized]
    public float trailerAttachmentZOffset = -7f;
}
