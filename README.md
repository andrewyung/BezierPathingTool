# BezierPathingTool
A simple and easy to use bezier pathing tool for the Unity game engine. Interpolates position and rotation. Each bezier segment can be controled with a speed and a speed curve.

### Instructions
- Drag BezierPoints script onto gameobject you want to have pathing for
- Setup path using the inspector and/or handles in editor

#### Movement can be controlled through the Unity inspector or code with:
- autoLerp - Automatically move the gameobject along the path (stops at the end if lerpBackwards is not set)
- lerpBackwards - Moves gameobject in reverse along path if autoLerp is set
- looping - Repeats the movement of the gameobject repeatly by toggling lerpBackwards when the start or end of the path has been reached
- lerpValue - Can be modified so the object would at at a certain point along the path

#### Examples
<img src="/SampleGifs/bezier1.gif?raw=true">
<img src="/SampleGifs/bezier2.gif?raw=true">
<img src="/SampleGifs/bezier3.gif?raw=true">


#### Improvements?
Option to normalize the speed of movement of the whole path by taking into account of each bezier curve length used to create the path.
