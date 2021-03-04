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
using System.IO;
using UnityEngine;
using UnityEditor;
using ItSeez3D.AvatarSdk.Core;

namespace ItSeez3D.AvatarSdk.Core
{
	public class FullbodyAvatarPrefabBuilder : AvatarPrefabBuilder
	{
		#region signletion staff
		private static FullbodyAvatarPrefabBuilder fullbodyInstance = null;

		protected FullbodyAvatarPrefabBuilder() { }

		public static FullbodyAvatarPrefabBuilder FullbodyInstance
		{
			get
			{
				if (fullbodyInstance == null)
					fullbodyInstance = new FullbodyAvatarPrefabBuilder();
				return fullbodyInstance;
			}
		}
		#endregion

		public void CreateFullbodyPrefab(GameObject bodyObject, BodyAttachment bodyAttachment, GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId, string haircutId)
		{
			string prefabDir = Path.Combine(PluginStructure.GetPluginDirectoryPath(PluginStructure.PREFABS_DIR, PathOriginOptions.RelativeToAssetsFolder), avatarId);
			PluginStructure.CreatePluginDirectory(prefabDir);

			//Create a copy of the avatarObject
			GameObject copiedAvatarObject = GameObject.Instantiate(avatarObject);
			copiedAvatarObject.transform.SetParent(bodyObject.transform);
			copiedAvatarObject.name = avatarObject.name;

			//Further actions will change the mesh of the avatarObject. It won't contain bones and weights.
			//So we have preserve original avatarObject and recover it as is was before the creating of prefab.
			avatarObject.transform.SetParent(null);
			SaveMeshAndMaterialForAvatarObject(prefabDir, copiedAvatarObject, headObjectName, haircutObjectName, avatarId, haircutId);

			Matrix4x4[] currentBindPoses = bodyAttachment.GetCurrentBindPosesForHeadAndNeck();
			Matrix4x4[] originalBindPoses = { bodyAttachment.headBindPose, bodyAttachment.neckBindPose };
			bodyAttachment.headBindPose = currentBindPoses[0];
			bodyAttachment.neckBindPose = currentBindPoses[1];

			// Remove RotateByMouse script
			GameObject.DestroyImmediate(bodyObject.GetComponentInChildren<RotateByMouse>());
			// Save prefab
#if UNITY_2018_3_OR_NEWER
			PrefabUtility.SaveAsPrefabAsset(bodyObject, prefabDir + "/fullbody.prefab");
#else
			PrefabUtility.CreatePrefab(prefabDir + "/fullbody.prefab", bodyObject);
#endif
			//Revert back RotateBuMouse script
			bodyObject.AddComponent<RotateByMouse>();

			//Remove the copy of the avatarObject and recover the original
			GameObject.DestroyImmediate(copiedAvatarObject);
			avatarObject.transform.SetParent(bodyObject.transform);

			bodyAttachment.headBindPose = originalBindPoses[0];
			bodyAttachment.neckBindPose = originalBindPoses[1];

			EditorUtility.DisplayDialog("Prefab created successfully!", string.Format("You can find your prefab in '{0}' folder", prefabDir), "Ok");
		}
	}
}
#endif
