﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, June 2020
*/

using ItSeez3D.AvatarSdk.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class MoveByMouse : RotateByMouse
	{
		float lastDoubleTouchMangnitude;
		Vector2 lastDoubleTouchCenter = Vector2.zero;

		void Update()
		{
			if (EventSystem.current.IsPointerOverGameObject() || IsPointerOverUIObject())
				return;

#if !UNITY_WEBGL
			if (Input.touchSupported)
			{
				if (Input.touches.Length == 1)
				{
					Touch t = Input.touches[0];
					if (t.phase == TouchPhase.Moved)
					{
						Vector2 delta = t.position - lastPosition;
						transform.Rotate(Vector3.up, -0.5f * delta.x);
					}
					lastPosition = t.position;
				}
				else if (Input.touches.Length == 2)
				{
					Touch t1 = Input.touches[0];
					Touch t2 = Input.touches[1];
					Vector2 doubleTouchCenter = t1.position + 0.5f * (t2.position - t1.position);
					float magnitude = (t1.position - t2.position).magnitude;
					if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
					{
						float magnitudeDelta = magnitude - lastDoubleTouchMangnitude;
						transform.Translate(0, 0, magnitudeDelta * 0.01f, Space.World);

						if (lastDoubleTouchCenter != Vector2.zero)
						{
							Vector2 centerDelta = doubleTouchCenter - lastDoubleTouchCenter;
							transform.Translate(0, centerDelta.y * 0.01f, 0);
						}
						
					}
					lastDoubleTouchMangnitude = magnitude;
					lastDoubleTouchCenter = doubleTouchCenter;
				}
			}
			else
#endif
			{
				if (Input.GetMouseButton(0))
				{
					var dx = Input.GetAxis("Mouse X");
					transform.Rotate(Vector3.up, -dx * 5);
				}

				if (Input.GetMouseButton(1))
				{
					var dy = Input.GetAxis("Mouse Y");
					transform.Translate(0, dy * 0.2f, 0);
				}

				Vector2 scrollDelta = Input.mouseScrollDelta;
				if (scrollDelta != Vector2.zero)
				{
					transform.Translate(0, 0, 0.1f * scrollDelta.y, Space.World);
				}
			}
		}
	}
}
