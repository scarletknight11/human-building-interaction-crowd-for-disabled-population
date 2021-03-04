﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Little helpers that have no other place to go.
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Currently native plugins (compiled from C and C++) do not support non-ASCII file paths.
		/// This is to be fixed in the future.
		/// </summary>
		public static bool HasNonAscii (string s)
		{
			return s.Any (c => c > 127);
		}

		/// <summary>
		/// In editor display the actual window, otherwise just log a warning.
		/// </summary>
		public static void DisplayWarning (string title, string msg)
		{
			Debug.LogFormat ("{0}: {1}", title, msg);
			#if UNITY_EDITOR
			EditorUtility.DisplayDialog (title, msg, "Ok");
			#endif
		}

		/// <summary>
		/// Return true if we're currently in editor and not playing the game.
		/// </summary>
		public static bool IsDesignTime ()
		{
			#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
				return true;
			#endif

			return false;
		}

		public static GameObject FindSubobjectByName (GameObject obj, string name, bool includeInactive = true)
		{
			foreach (var trans in obj.GetComponentsInChildren<Transform>(includeInactive))
				if (trans.name == name)
					return trans.gameObject;

			return null;
		}

		public static bool TryParseGuid(string guidStr, out Guid guid)
		{
			try
			{
				guid = new Guid(guidStr);
				return true;
			}
			catch
			{
				guid = Guid.Empty;
				return false;
			}
		}
	}
}

