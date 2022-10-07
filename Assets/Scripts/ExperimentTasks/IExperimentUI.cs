using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IExperimentUI
{
    void CalcHoverKey(Vector2[] fingertipAnchoredPositions);
    void Click(int index);
}
