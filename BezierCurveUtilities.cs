using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurveUtilities {

    [Serializable]
	public struct CubicBezier2D
    {
        public Vector2 position1;//start point
        public Vector2 position2;//control point 1
        public Vector2 position3;//control point 2
        public Vector2 position4;//end point

        /// <summary>
        /// Initializes the positions of this struct
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        public CubicBezier2D(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            position1 = p1;
            position2 = p2;
            position3 = p3;
            position4 = p4;
        }
    }

    /// <summary>
    /// Determines the 4 points (start point, 2 control points and end point) of the beizer curve in the animation curve. 
    /// This only works as expected when there are only 2 keyframes in the animation curve.
    /// </summary>
    /// <param name="animCurve">Unity's animation curve</param>
    /// <returns></returns>
    public static CubicBezier2D getAnimationCurvePositions(AnimationCurve animCurve)
    {
        float aspectRatio = Screen.height / Screen.width;

        float tangLengthX = Mathf.Abs(animCurve.keys[0].time - animCurve.keys[1].time) * 0.333333f;
        float tangLengthY = tangLengthX;
        Vector2 p0 = new Vector2(animCurve.keys[0].time, animCurve.keys[0].value);
        Vector2 p1 = new Vector2(animCurve.keys[1].time, animCurve.keys[1].value);
        Vector2 c0 = p0;
        Vector2 c1 = p1;
        c0.x += tangLengthX;
        c0.y += tangLengthY * aspectRatio * animCurve.keys[0].outTangent;
        c1.x -= tangLengthX;
        c1.y -= tangLengthY * aspectRatio * animCurve.keys[1].inTangent;

        return new CubicBezier2D(p0, c0, c1, p1);
    }

    /// <summary>
    /// Returns the area between leftBound and rightBound based on the points of the cubic bezier
    /// </summary>
    /// <param name="leftBound">Left keyframe bound</param>
    /// <param name="rightBound">Right keyframe bound</param>
    /// <param name="p1y">value (y axis) of the left bound</param>
    /// <param name="p2y">value (y axis) of the first control point</param>
    /// <param name="p3y">value (y axis) of the second control point</param>
    /// <param name="p4y">value (y axis) of the right bound</param>
    /// <returns></returns>
    public static float calculateCubicBezierArea(float leftBound, float rightBound, float p1y, float p2y, float p3y, float p4y)
    {
        //integral of beizer curve
        float set00 = 1f - rightBound;
        float set01 = 0.25f * rightBound * rightBound * rightBound * rightBound;
        float set02 = rightBound * rightBound * rightBound;

        float set10 = 1f - leftBound;
        float set11 = 0.25f * leftBound * leftBound * leftBound * leftBound;
        float set12 = leftBound * leftBound * leftBound;

        return (-p1y * 0.25f * set00 * set00 * set00 * set00) +
                        (3f * p2y * (set01 - ((2f / 3f) * set02) + (0.5f * rightBound * rightBound))) +
                        (3f * p3y * (((1f / 3f) * set02) - set01)) +
                        (p4y * set01) - (
                    (-p1y * 0.25f * set10 * set10 * set10 * set10) +
                        (3f * p2y * (set11 - ((2f / 3f) * set12) + (0.5f * leftBound * leftBound))) +
                        (3f * p3y * (((1f / 3f) * set12) - set11)) +
                        (p4y * set11));
    }
    public static float calculateCubicBezierArea(float leftBound, float rightBound, CubicBezier2D bezier)
    {
        return calculateCubicBezierArea(leftBound, rightBound, bezier.position1.y, bezier.position2.y, bezier.position3.y, bezier.position4.y);
    }
}
