using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles.ReedsSheppPaths
{
    //All possible Reeds-Shepp paths
    //L, S, R are the steering directions (left, straight, or right)
    //f, b means if we are driving forward or backward
    //u means the two corresponding segments have the same length u
    //pi2 means we are driving a distance of pi/2 radians in this curve, which is 90 degrees
    public enum PathWords
    {
        Lf_Sf_Lf, //8.1: CSC, same turn
        Lb_Sb_Lb,
        Rf_Sf_Rf,
        Rb_Sb_Rb,
        
        Lf_Sf_Rf, //8.2: CSC, different turn
        Lb_Sb_Rb,
        Rf_Sf_Lf,
        Rb_Sb_Lb,
        
        Lf_Rb_Lf, //8.3: C|C|C
        Lb_Rf_Lb,
        Rf_Lb_Rf,
        Rb_Lf_Rb,
        
        Lf_Rb_Lb, //8.4: C|CC
        Lb_Rf_Lf,
        Rf_Lb_Rb,
        Rb_Lf_Rf,

        Lf_Rf_Lb, //8.4: CC|C
        Lb_Rb_Lf,
        Rf_Lf_Rb,
        Rb_Lb_Rf,

        Lf_Ruf_Lub_Rb, //8.7: CCu|CuC 
        Lb_Rub_Luf_Rf,
        Rf_Luf_Rub_Lb,
        Rb_Lub_Ruf_Lf,

        Lf_Rub_Lub_Rf, //8.8: C|CuCu|C
        Lb_Ruf_Luf_Rb,
        Rf_Lub_Rub_Lf,
        Rb_Luf_Ruf_Lb,

        Lf_Rbpi2_Sb_Lb, //8.9: C|C(pi/2)SC, same turn
        Lb_Rfpi2_Sf_Lf,
        Rf_Lbpi2_Sb_Rb,
        Rb_Lfpi2_Sf_Rf,

        Lf_Rbpi2_Sb_Rb, //8.10: C|C(pi/2)SC, different turn
        Lb_Rfpi2_Sf_Rf,
        Rf_Lbpi2_Sb_Lb,
        Rb_Lfpi2_Sf_Lf,

        Lf_Sf_Rfpi2_Lb, //8.9 (reversed): CSC(pi/2)|C, same turn
        Lb_Sb_Rbpi2_Lf,
        Rf_Sf_Lfpi2_Rb,
        Rb_Sb_Lbpi2_Rf,

        Lf_Sf_Lfpi2_Rb, //8.10 (reversed): CSC(pi/2)|C, different turn
        Lb_Sb_Lbpi2_Rf,
        Rf_Sf_Rfpi2_Lb,
        Rb_Sb_Rbpi2_Lf,

        Lf_Rbpi2_Sb_Lbpi2_Rf, //8.11: C|C(pi/2)SC(pi/2)|C
        Lb_Rfpi2_Sf_Lfpi2_Rb,
        Rf_Lbpi2_Sb_Rbpi2_Lf,
        Rb_Lfpi2_Sf_Rfpi2_Lb
    }
}
