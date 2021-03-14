﻿using UnityEngine;
using System.Collections;
using System;

/*
 * 
 * 
 * 1. GetControlPoint / SetControlPoint of a curve
 * 2. GetPoint <-- polynomial implementation
 * 3. GetDirection <-- result of GetVelocity (first derivative of curve)
 * 4. Define the ControlPoints modes for connectors and extremes
 * 5. GetControlPointMode / SetControlPointMode
 * 
 */


/* Control points which connect curves can be in 3 modes */
public enum ControlPointMode : int {
	MIRRORED = 0,
	ALIGNED = 1,
	FREE = 2	
}

public class BezierSpline : MonoBehaviour {



	/* We will keep one reference of each control point which connect curves */
	[SerializeField]
	private ControlPointMode[] modes;

    /* Make them saveable */
    [SerializeField]
    private Vector3[] controlPoints;

    [SerializeField]
    private bool loop;

    public bool Loop {
        get {
            return loop;
        }
        set {
            loop = value;
            if(loop) {
                /* We need to: match the mode and position of both ends of the spline */
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, controlPoints[0]);
            }
        }
    }

    public int curves;

    public Vector3 GetControlPoint(int p) {
        return controlPoints[p];
    }

    public void SetControlPoint(int p, Vector3 point) {
        /*  When a point is moved, if it is the middle one
        *   adjust the two around it if the mode is not free
        */
        if (GetControlPointMode(p) != ControlPointMode.FREE && p % 3 == 0) {
            Vector3 deltaAdjust = point - controlPoints[p]; // get the difference between the new and current location
            if(loop) {
                /* For loops
                *   On extreme cases, set both ends to be equal
                *   For all other cases, adjust the surrounding points accoridngly by the delta
                *   of the newPosition - originalPosition
                */
                if(p == 0) {
                    controlPoints[p + 1] += deltaAdjust;
                    controlPoints[controlPoints.Length - 2] += deltaAdjust;
                    controlPoints[controlPoints.Length - 1] = point;
                } else if (p == controlPoints.Length - 1) {
                    controlPoints[0] = point;
                    controlPoints[1] += deltaAdjust;
                    controlPoints[p - 1] += deltaAdjust;
                } else {
                    controlPoints[p - 1] += deltaAdjust;
                    controlPoints[p + 1] += deltaAdjust;
                }
            } else {
                if (p > 0) {
                    controlPoints[p - 1] += deltaAdjust;
                }
                if(p + 1 < controlPoints.Length - 1) {
                    controlPoints[p + 1] += deltaAdjust;
                }
            }
        }
        controlPoints[p] = point;
		EnforceConstraintMode(p);
    }

    public int GetControlPointsCount {
        get {
            return Curves * 3 + 1;
        }
    }

    public int Curves {
        get {
            return curves;
        }
    }

    public void Reset() {
        controlPoints = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 1f, 0f),
            new Vector3(3f, 2f, 0f),
            new Vector3(4f, 3f, 0f)
        };
        curves = 1;

		// the first point and the last of the first curve
		modes = new ControlPointMode[] {
			ControlPointMode.FREE,
			ControlPointMode.FREE
		};
    }

    public Vector3 GetPoint(float t) {
        return transform.TransformPoint(CubicBezierCurve.GetPoint(controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3], t));
    }

    public Vector3 GetPoint(float t, int curveSection) {
        int offset = curveSection * 3;
        return transform.TransformPoint(CubicBezierCurve.GetPoint(
            controlPoints[0 + offset], 
            controlPoints[1 + offset], 
            controlPoints[2 + offset], 
            controlPoints[3 + offset], t));
    }

    public Vector3 GetDirection(float t, int curveSection) {
        return GetCurveVelocity(t, curveSection).normalized;
    }

    public Vector3 GetCurveVelocity(float t, int curveSection) {
        int offset = curveSection * 3;
        return transform.TransformPoint(CubicBezierCurve.GetCurveRateOfChange(
            controlPoints[0 + offset], 
            controlPoints[1 + offset], 
            controlPoints[2 + offset], 
            controlPoints[3 + offset], t)) - transform.position;
    }

    /*  
        The Spline will receive 3 more points. Although the curve has 4, the first point
        of the new curve will be the last point of the previous one
    */
    public void AddCurve() {
        // make the first point of the new curve, the last of the original
        Vector3 point = controlPoints[controlPoints.Length - 1];
        
        // resize the current controlPoints array
        Array.Resize(ref controlPoints, controlPoints.Length + 3);
        
        // set up the new point in the resized array
        point.x += 1f;
        controlPoints[controlPoints.Length - 3] = point;
        point.x += 1f;
        controlPoints[controlPoints.Length - 2] = point;
        point.x += 1f;
        controlPoints[controlPoints.Length - 1] = point;

        curves += 1;

		/* When adding a new curve, make the new control point connector the same as the one before by default */
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];

        /* Enforce the mode on the new curve's connector control point */
        EnforceConstraintMode(controlPoints.Length - 4);

        if (loop) {
            // match position
            controlPoints[controlPoints.Length - 1] = controlPoints[0];
            // match mode
            modes[modes.Length - 1] = modes[0];
            // enforce
            EnforceConstraintMode(0);
        }
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float oneMinus = 1f - t;
        return
                (float)Math.Pow(oneMinus, 3) * p0 +
                3f * (float)Math.Pow(oneMinus, 2) * t * p1 +
                3f * oneMinus * (float)Math.Pow(t, 2) * p2 +
                (float)Math.Pow(t, 3) * p3;
    }

    public static Vector3 GetCurveRateOfChange(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float oneMinus = 1f - t;
        return
            3f * oneMinus * oneMinus * (p1 - p0) +
            6f * oneMinus * t * (p2 - p1) +
            3f * t * t * (p3 - p2);

    }

	/* Get and Set the contorl point for the right curve */

	public ControlPointMode GetControlPointMode(int i) {
		return modes[(i + 1) / 3];
	}

	public void SetControlPointMode(int p, ControlPointMode mode) {
        int modeIndex = (p + 1) / 3;
        modes[modeIndex] = mode;

        /* If we are in loop mode, ensure both ends share the same constraint if either changed */
        if (loop) {
            if(modeIndex == 0) {
                modes[modes.Length - 1] = mode;
            } else if (modeIndex == modes.Length - 1) {
                modes[0] = mode;
            }
        }  
		EnforceConstraintMode(p);
	}

	/* 
	 * 	Function which will enforce the constraint between points 
	 *	To be called when a a point is Set or a mode is Set
	*/

	public void EnforceConstraintMode(int index) {

		// get current mode
		int curModeIndex = (index + 1) / 3;
		ControlPointMode mode = modes[curModeIndex];

		/* We can only enforce the constraint on points which belong to the union of 2 curves.
		 * We disregard the extreme cases IFF NOT LOOPING (first and last control point and the case on which the mode
		 * is FREE, since there is nothing to adjust
		 */
		if(mode == ControlPointMode.FREE || !loop && (curModeIndex == 0 || curModeIndex == modes.Length - 1)) {
			return;
		}

		/* 
		 *	Find the middle point 
		 *	1. if the middle is selected:
		 *		a. adjust the one after it
		 *	2. else, adjust the opposite of the currently selected
         *
         *  3. LOOPING:
         *      If we are looping, we have to ROLL over indexes.
		*/
		int midIndex = curModeIndex * 3, fixIndex, adjustIndex;
		if(midIndex <= index) {
			fixIndex = midIndex - 1;
            if (fixIndex < 0) { // loop case
                fixIndex = controlPoints.Length - 2;
            }
            adjustIndex = midIndex + 1; // this will be the adjusted one
            if (adjustIndex >= controlPoints.Length) { // loop case
                adjustIndex = 1;
            }
        } else {
			fixIndex = midIndex + 1;
            if(fixIndex >= controlPoints.Length) { // loop case
                fixIndex = 1;
            }
            adjustIndex = midIndex - 1;
            if(adjustIndex < 0) { // loop case
                adjustIndex = controlPoints.Length - 2;
            }
        }

        
        Vector3 middlePoint = controlPoints[midIndex];
        
        /*  Mirroring */
        Vector3 enforcedTangent = middlePoint - controlPoints[fixIndex];
        
        /* Align - respect distance */
        if (mode == ControlPointMode.ALIGNED) {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middlePoint, controlPoints[adjustIndex]);
        }
        controlPoints[adjustIndex] = middlePoint + enforcedTangent; 
	}
}
