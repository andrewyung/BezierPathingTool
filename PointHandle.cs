using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(BezierPoints))]
class PointHandle : Editor
{
    private bool hideTools = false;

    void OnEnable()
    {
#if (UNITY_EDITOR)
        SceneView.onSceneGUIDelegate -= OnScene;
        SceneView.onSceneGUIDelegate += OnScene;

        Selection.selectionChanged -= OnSelection;
        Selection.selectionChanged += OnSelection;
#endif
    }
    
    private void OnSelection()
    {
        if (target != null && !Selection.Contains((target as BezierPoints).gameObject.GetInstanceID()))
        {
            hideTools = true;
        }
        else
        {
            hideTools = false;
        }
    }

    private void OnScene(SceneView sceneview)
    {
        BezierPoints bezierPoints = target as BezierPoints;

        if (target != null && bezierPoints.gameObject.activeSelf)
        {
            if (bezierPoints == null || bezierPoints.CurvesCount == 0)
            return;

            if (!hideTools)
            {
                for (int i = 0; i < bezierPoints.CurvesCount; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    Vector3 endPosition1 = bezierPoints.getEndpoint1(i);
                    Vector3 endPosition2 = bezierPoints.getEndpoint2(i);

                    Vector3 controlPosition1 = bezierPoints.getControlPoint1(i);
                    Vector3 controlPosition2 = bezierPoints.getControlPoint2(i);

                    Quaternion startRotation = bezierPoints.getStartRotation(i);
                    Quaternion endRotation = bezierPoints.getEndRotation(i);

                    Handles.color = Color.green;
                    Handles.DrawLine(endPosition1, endPosition2);
                    Handles.color = Color.red;
                    Handles.DrawLine(endPosition1, controlPosition1);
                    Handles.DrawLine(endPosition2, controlPosition2);
                    Handles.DrawLine(controlPosition1, controlPosition2);

                    endPosition1 = Handles.PositionHandle(endPosition1, Quaternion.identity);
                    endPosition2 = Handles.PositionHandle(endPosition2, Quaternion.identity);
                    controlPosition1 = Handles.PositionHandle(controlPosition1, Quaternion.identity);
                    controlPosition2 = Handles.PositionHandle(controlPosition2, Quaternion.identity);
                    startRotation = Handles.RotationHandle(startRotation, endPosition1);
                    endRotation = Handles.RotationHandle(endRotation, endPosition2);

                    //Draw handle
                    Handles.DrawBezier(endPosition1, endPosition2, controlPosition1, controlPosition2, bezierPoints.lineColor, null, 5);


                    GUIStyle style = new GUIStyle();
                    style.fontSize = (Mathf.Clamp(Mathf.RoundToInt((endPosition1 - endPosition2).magnitude * (10 - Mathf.RoundToInt(Vector3.Distance(Camera.current.transform.position, (endPosition1 + endPosition2) / 2)))), 
                                                    25, Mathf.RoundToInt((endPosition1 - endPosition2).magnitude)));
                    //Handles.Label((endPosition1 + endPosition2 + controlPosition1 + controlPosition2) / 4, i.ToString(), style);
                    Handles.BeginGUI();
                    Vector3 pos = (endPosition1 + endPosition2 + controlPosition1 + controlPosition2) / 4;
                    Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
                    GUI.Label(new Rect(pos2D.x, pos2D.y, 100, 100), i.ToString(), style);
                    Handles.EndGUI();

                    //Handles.DrawBezier(endPosition1, endPosition2, controlPosition1, controlPosition2, Color.white, null, 2f);


                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(bezierPoints, "Beizer point changed");
                    
                        bezierPoints.setEndPoint1(i, endPosition1);
                        bezierPoints.setEndPoint2(i, endPosition2);
                        bezierPoints.setControlPoint1(i, controlPosition1);
                        bezierPoints.setControlPoint2(i, controlPosition2);
                        bezierPoints.setStartRotation(i, startRotation);
                        bezierPoints.setEndRotation(i, endRotation);
                    }
                }
            }
        
            Handles.color = bezierPoints.lineColor;
        }
    }

    private void clearFoldedPrefs(int maxIndex)
    {
        for (int i = 0; i < maxIndex; i++)
        {
            if (EditorPrefs.HasKey("folded" + i))
            {
                EditorPrefs.DeleteKey("folded" + i);
            }
        }
    }

    private void removeFoldedPrefAt(int atIndex, int maxIndex)
    {
        if (EditorPrefs.HasKey("folded" + atIndex))
        {
            for (int i = atIndex; i < maxIndex - 1; i++)
            {
                if (EditorPrefs.HasKey("folded" + (i + 1)))
                {
                    EditorPrefs.SetBool("folded" + i, EditorPrefs.GetBool("folded" + (i + 1)));
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void setFoldedPref(int atIndex, bool value)
    {
        EditorPrefs.SetBool("folded" + atIndex, value);
    }

    public override void OnInspectorGUI()
    {
        BezierPoints bezierPoints = (BezierPoints)target;

        bezierPoints.lineColor = EditorGUILayout.ColorField("Line Color", bezierPoints.lineColor);

        if (GUILayout.Button("Clear all curves"))
        {
            clearFoldedPrefs(bezierPoints.CurvesCount);

            Undo.RecordObject(bezierPoints, "Cleared beizer curves");
            bezierPoints.clearAll();
        }

        GUILayout.BeginHorizontal();

        bezierPoints.autoLerp = EditorGUILayout.Toggle("Auto lerp", bezierPoints.autoLerp);
        bezierPoints.lerpBackwards = EditorGUILayout.Toggle("Lerp backwards", bezierPoints.lerpBackwards);

        GUILayout.EndHorizontal();

        bezierPoints.looping = EditorGUILayout.Toggle("Looping", bezierPoints.looping);

        EditorGUILayout.Space();

        //add curve button
        if (GUILayout.Button("Add Beizer curve"))
        {
            setFoldedPref(bezierPoints.CurvesCount, false);

            Undo.RecordObject(bezierPoints, "Added beizer curve");
            bezierPoints.addBeizeCurve();
        }
        
        //if points exists
        if (bezierPoints.CurvesCount > 0)
        {
            //slider for lerp value
            bezierPoints.lerpValue = EditorGUILayout.Slider("Lerp Value", bezierPoints.lerpValue, 0, 1);
            
            //show all points for each curve
            for (int i = 0; i < bezierPoints.CurvesCount; i++)
            {
                //Debug.Log(EditorPrefs.HasKey("folded" + (i / 2)) + " : " + i);
                setFoldedPref(i, EditorGUILayout.Foldout(EditorPrefs.GetBool("folded" + (i)), "Beizer curve " + i, true));
                if (EditorPrefs.HasKey("folded" + (i)) && EditorPrefs.GetBool("folded" + (i)))
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();

                    Vector3 endPoint1 = EditorGUILayout.Vector3Field("End Point 1", bezierPoints.getEndpoint1(i));
                    Vector3 endPoint2 = EditorGUILayout.Vector3Field("End Point 2", bezierPoints.getEndpoint2(i));

                    Vector3 controlPoint1 = EditorGUILayout.Vector3Field("Control Point 1", bezierPoints.getControlPoint1(i));
                    Vector3 controlPoint2 = EditorGUILayout.Vector3Field("Control Point 2", bezierPoints.getControlPoint2(i));

                    GUILayout.BeginHorizontal();

                    AnimationCurve animCurve = EditorGUILayout.CurveField("Speed curve", bezierPoints.getAnimationCurve(i));
                    float speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier", bezierPoints.getCurveMultiplier(i));

                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    Quaternion startRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Start Rotation", bezierPoints.getStartRotation(i).eulerAngles));
                    Quaternion endRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("End Rotation", bezierPoints.getEndRotation(i).eulerAngles));

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(bezierPoints, "Changed point position");

                        bezierPoints.setEndPoint1(i, endPoint1);
                        bezierPoints.setEndPoint2(i, endPoint2);

                        bezierPoints.setControlPoint1(i, controlPoint1);
                        bezierPoints.setControlPoint2(i, controlPoint2);

                        bezierPoints.setStartRotation(i, startRotation);
                        bezierPoints.setEndRotation(i, endRotation);

                        bezierPoints.setAnimationCurve(i, animCurve);
                        bezierPoints.setCurveMultiplier(i, Mathf.Clamp(speedMultiplier, 0.1f, int.MaxValue));
                    }

                    EditorGUI.indentLevel--;

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Clear"))
                    {
                        removeFoldedPrefAt(i, bezierPoints.CurvesCount);

                        Undo.RecordObject(bezierPoints, "Removed curve");
                        bezierPoints.removeCurve(i);
                    }

                    if (bezierPoints.CurvesCount > i + 1)
                    {
                        if (GUILayout.Button("Snap to next"))
                        {
                            Undo.RecordObject(bezierPoints, "Snap curve to next");
                            bezierPoints.setEndPoint2(i, bezierPoints.getEndpoint1(i + 1));
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        GUILayout.Button("Snap to next");
                        GUI.enabled = true;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            //clear all curves
            if (GUILayout.Button("Clear all curves"))
            {
                clearFoldedPrefs(bezierPoints.CurvesCount);

                Undo.RecordObject(bezierPoints, "Cleared beizer curves");
                bezierPoints.clearAll();
            }
        }
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }
}