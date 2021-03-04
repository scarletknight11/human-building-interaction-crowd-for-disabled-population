/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

#if UNITY_EDITOR

using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdk.Core.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace ItSeez3D.AvatarSdk.Cloud.Editor
{
	[InitializeOnLoad]
	public class UpdateCheckerUi
	{
		private static List<UpdateChecker> updateCheckers = new List<UpdateChecker>();
		static UpdateCheckerUi()
		{
			updateCheckers  = CoreTools.DetectFlavour().Select(f => 
			new UpdateChecker(
				f.GetTraits().UpdateCheckMemo, 
				f.GetTraits().UpdateCheckUrl, 
				f.GetTraits().Version,
				f.GetTraits().Name,
				()=> { ShowUpdateWindow(string.Format("{0}: Update recommended", f.GetTraits().Name)); }
			)).ToList();
			EditorApplication.update += InitializeOnce;
		}
		private static void InitializeOnce()
		{
			EditorApplication.update -= InitializeOnce;
			foreach(var checker in updateCheckers)
			{
				checker.CheckOnStartup();
			}
		}

		[MenuItem ("Window/itSeez3D Avatar SDK/Check for updates")]
		public static void CheckForUpdatesMenu ()
		{
			foreach (var checker in updateCheckers)
			{
				checker.CheckForUpdates(automaticCheck: false);
			}
		}

		private static void ShowUpdateWindow (string header = "Update recommended")
		{
			var msg = "There is a new version of Avatar SDK plugin! We recommend you to upgrade.";
			Utils.DisplayWarning (header, msg);
		}
	}
}
#endif