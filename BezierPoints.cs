using System;
using System.Collections.Generic;
using UnityEngine;

public class BezierPoints : MonoBehaviour
{
    [Serializable]
    public class CurvePoints
    {
        public Vector3[] endPoints;
        public Vector3[] controlPoints;

        public Quaternion startRotation;
        public Quaternion endRotation;

        public AnimationCurve speedCurve;
        public BezierCurveUtilities.CubicBezier2D speedCurvePoints;

        public float totalCurveArea = 1;

        public float speedCurveAverage = 1;

        public float speedMultiplier = 1;

        public CurvePoints()
        {
            endPoints = new Vector3[]{ Vector3.zero, Vector3.zero};
            controlPoints = new Vector3[] { Vector3.zero, Vector3.zero };

            startRotation = Quaternion.identity;
            endRotation = Quaternion.identity;
        }
        public CurvePoints(Vector3[] endPoints, Vector3[] controlPoints)
        {
            this.endPoints = endPoints;
            this.controlPoints = controlPoints;

            startRotation = Quaternion.identity;
            endRotation = Quaternion.identity;
        }
    }
    
    [SerializeField]
    private List<CurvePoints> curves = new List<CurvePoints>();

    public Color lineColor = Color.white;

    public int CurvesCount
    {
        get { return curves.Count; }
    }

    public float lerpValue;
    public bool autoLerp = false;
    public bool looping = true;
    public bool lerpBackwards = false;
    
    //Total area under animation curves
    private float totalArea = 0;

    void Update()
    {
        //logic for controlling the movement state
        if (curves != null && autoLerp)
        {
            if (lerpValue <= 1 && lerpValue >= 0)
            {
                if (!lerpBackwards)
                {
                    lerpValue += Time.deltaTime * 0.2f;
                }
                else
                {
                    lerpValue -= Time.deltaTime * 0.2f;
                }
            }
            lerpValue = Mathf.Clamp(lerpValue, 0, 1);
            transform.position = getPosition(lerpValue);
            transform.rotation = getRotation(lerpValue);

            if (looping)
            {
                if (lerpValue >= 1 || lerpValue <= 0)
                {
                    lerpValue = Mathf.Clamp01(lerpValue);
                    lerpBackwards = !lerpBackwards;
                }
            }
        }
    }

    /// <summary>
    /// Get rotation evaluated at testLerpValue.
    /// </summary>
    /// <param name="testLerpValue"></param>
    /// <returns></returns>
    public Quaternion getRotation(float testLerpValue)
    {
        Quaternion rotation = Quaternion.identity;

        //if length isnt 0 and endpoints isnt null
        if (curves.Count > 0)
        {
            testLerpValue = Mathf.Clamp(testLerpValue, 0, 1);

            //determine total allocated lerp time
            float totalTimeBlockSize = 0;
            for (int i = 0; i < CurvesCount; i++)
            {
                totalTimeBlockSize += 1 / curves[i].speedCurveAverage;
            }

            float timeBlockStartValue = 0;
            int offset;
            float lerp = 0;
            for (offset = 0; offset < CurvesCount; offset++)
            {
                timeBlockStartValue += (1 / curves[offset].speedCurveAverage);
                if ((testLerpValue * totalTimeBlockSize) <= timeBlockStartValue)
                {
                    lerp = ((testLerpValue * totalTimeBlockSize) - (timeBlockStartValue - (1 / curves[offset].speedCurveAverage))) / (1 / curves[offset].speedCurveAverage);
                    break;
                }
            }
            //int offset = (int)Mathf.Floor(lerpValuePosition / (1f / totalDistance));
            //(int) Mathf.Floor(lerpValuePosition / (1f / (curves.Count)));

            if (offset < curves.Count)
            {
                rotation = Quaternion.Lerp(curves[offset].startRotation, curves[offset].endRotation, lerp);
            }
        }
        return rotation;
    }

    /// <summary>
    /// Get position evaluated at testLerpValue.
    /// </summary>
    /// <param name="testLerpValue"></param>
    /// <returns></returns>
    public Vector3 getPosition(float testLerpValue)
    {
        Vector3 position = Vector3.zero;

        //if length isnt 0 and endpoints isnt null
        if (curves.Count > 0)
        {
            testLerpValue = Mathf.Clamp(testLerpValue, 0, 0.999f);

            //determine total allocated lerp time
            float totalTimeBlockSize = 0;
            for (int i = 0; i < CurvesCount; i++)
            {
                totalTimeBlockSize += 1 / curves[i].speedCurveAverage;
            }

            float timeBlockStartValue = 0;
            int offset;
            float lerpValue = 0;
            for (offset = 0; offset < CurvesCount; offset++)
            {
                timeBlockStartValue += (1f / curves[offset].speedCurveAverage);
                //if is correct animation curve to determine area for
                if ((testLerpValue * totalTimeBlockSize) <= timeBlockStartValue)
                {
                    float lerp = ((testLerpValue * totalTimeBlockSize) - (timeBlockStartValue - (1f / curves[offset].speedCurveAverage))) / (1f / curves[offset].speedCurveAverage);
                    
                    lerpValue = BezierCurveUtilities.calculateCubicBezierArea(0, lerp, curves[offset].speedCurvePoints) / curves[offset].totalCurveArea;//determine percentage of area under curve on the left of lerp value in animation curve
                    break;
                }
            }

            if (offset < curves.Count)
            {
                float subtractedLerpVal = 1 - lerpValue;

                //cubic polynomial. (1-lerpVal)^3 * P0 + 3(1-lerpVal)^2 * lerpVal * P1 + 3(1-lerpVal) * lerpVal^2 * P2 + lerpVal^3 * P3
                position = (subtractedLerpVal * subtractedLerpVal * subtractedLerpVal * curves[offset].endPoints[0]) +
                                    (3 * subtractedLerpVal * subtractedLerpVal * lerpValue * curves[offset].controlPoints[0]) +
                                    (3 * subtractedLerpVal * lerpValue * lerpValue * curves[offset].controlPoints[1]) +
                                    (lerpValue * lerpValue * lerpValue * curves[offset].endPoints[1]);
            }
        }
        return position;
    }


    //******************** functions to modify the path ***********************

    public void addBeizeCurve(Vector3 endPoint1, Vector3 endPoint2, Vector3 controlPoint1, Vector3 controlPoint2)
    {
        curves.Add(new CurvePoints(new Vector3[] { endPoint1, endPoint2 }, new Vector3[] { controlPoint1, controlPoint2 }));

        totalArea += 1;

        setAnimationCurve(curves.Count - 1, new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) }));
    }
    public void addBeizeCurve()
    {
        Vector3 endPoint1;
        Vector3 endPoint2;
        Vector3 controlPoint1;
        Vector3 controlPoint2;

        if (curves.Count > 0)
        {
            endPoint1 = curves[curves.Count - 1].endPoints[1];
            endPoint2 = endPoint1 + new Vector3(1, 0, 0);
            controlPoint1 = endPoint1 + new Vector3(0, 0, 1);
            controlPoint2 = endPoint1 + new Vector3(1, 0, 1);
        }
        else
        {
            endPoint1 = transform.position + new Vector3(0, 0, 0);
            endPoint2 = transform.position + new Vector3(1, 0, 0);
            controlPoint1 = transform.position + new Vector3(0, 0, 1);
            controlPoint2 = transform.position + new Vector3(1, 0, 1);
        }

        addBeizeCurve(endPoint1, endPoint2, controlPoint1, controlPoint2);
    }

    public void clearAll()
    {
        curves.Clear();
        totalArea = 0;
    }

    public bool setAnimationCurve(int ofCurve, AnimationCurve curve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            totalArea -= curves[ofCurve].speedCurveAverage;

            curves[ofCurve].speedCurve = curve;

            BezierCurveUtilities.CubicBezier2D bezier= BezierCurveUtilities.getAnimationCurvePositions(curves[ofCurve].speedCurve);
            curves[ofCurve].speedCurvePoints = bezier;

            float area = BezierCurveUtilities.calculateCubicBezierArea(0, 1, bezier.position1.y, bezier.position2.y, bezier.position3.y, bezier.position4.y);
            float testArea = BezierCurveUtilities.calculateCubicBezierArea(0, 0.5f, bezier.position1.y, bezier.position2.y, bezier.position3.y, bezier.position4.y);
            
            curves[ofCurve].totalCurveArea = area;
            
            totalArea += (area);

            return true;
        }
        return false;
    }

    public bool setCurveMultiplier(int ofCurve, float multiplier)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            totalArea -= curves[ofCurve].speedCurveAverage;

            float baseAverage = curves[ofCurve].speedCurveAverage / curves[ofCurve].speedMultiplier;

            curves[ofCurve].speedMultiplier = multiplier;

            curves[ofCurve].speedCurveAverage = (multiplier * baseAverage);
            totalArea += curves[ofCurve].speedCurveAverage;
            
            return true;
        }
        return false;
    }

    public AnimationCurve getAnimationCurve(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].speedCurve;
        }
        return null;
    }
    public float getCurveMultiplier(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].speedMultiplier;
        }
        return -1;
    }

    public bool setStartRotation(int ofCurve, Quaternion rotation)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            curves[ofCurve].startRotation = rotation;
            return true;
        }
        return false;
    }
    public bool setEndRotation(int ofCurve, Quaternion rotation)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            curves[ofCurve].endRotation = rotation;
            return true;
        }
        return false;
    }
    public Quaternion getStartRotation(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].startRotation;
        }
        return Quaternion.identity;
    }
    public Quaternion getEndRotation(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].endRotation;
        }
        return Quaternion.identity;
    }

    public bool setEndPoint1(int ofCurve, Vector3 position)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            curves[ofCurve].endPoints[0] = position;
            return true;
        }
        return false;
    }
    public bool setEndPoint2(int ofCurve, Vector3 position)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            curves[ofCurve].endPoints[1] = position;
            return true;
        }
        return false;
    }
    public bool setControlPoint1(int ofCurve, Vector3 position)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            curves[ofCurve].controlPoints[0] = position;
            return true;
        }
        return false;
    }
    public bool setControlPoint2(int ofCurve, Vector3 position)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            curves[ofCurve].controlPoints[1] = position;
            return true;
        }
        return false;
    }

    public Vector3 getEndpoint1(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].endPoints[0];
        }
        return Vector3.zero;
    }
    public Vector3 getEndpoint2(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].endPoints[1];
        }
        return Vector3.zero;
    }
    public Vector3 getControlPoint1(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].controlPoints[0];
        }
        return Vector3.zero;
    }
    public Vector3 getControlPoint2(int ofCurve)
    {
        if (isWithinArrayCheck(ofCurve))
        {
            return curves[ofCurve].controlPoints[1];
        }
        return Vector3.zero;
    }

    private bool isWithinArrayCheck(int ofCurve)
    {
        if (CurvesCount > ofCurve && ofCurve >= 0)
        {
            return true;
        }
        return false;
    }

    public bool removeCurve(int atIndex)
    {
        if (isWithinArrayCheck(atIndex))
        {
            totalArea -= curves[atIndex].speedCurveAverage;
            curves.RemoveAt(atIndex);
            return true;
        }
        return false;
    }
}
