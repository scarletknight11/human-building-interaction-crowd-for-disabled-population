﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdk.Core
{
	public enum PathOriginOptions
	{
		FullPath,
		RelativeToAssetsFolder
	}

	public enum SampleScene
	{
		CLOUD_00_ALL_SAMPLES,
		CLOUD_01_GETTING_STARTED,
		CLOUD_02_GALLERY,
		CLOUD_03_FULLBODY,
		CLOUD_04_LOD,
		CLOUD_05_PARAMETERS,
		CLOUD_06_CARTOONISH_AVATAR,
		CLOUD_07_WEBGL,
		CLOUD_08_FULLBODY_LEGACY,
		CLOUD_09_MULTIPLAYER,
		OFFLINE_00_ALL_SAMPLES,
		OFFLINE_01_GETTING_STARTED,
		OFFLINE_02_GALLERY,
		OFFLINE_03_FULLBODY,
		OFFLINE_04_LOD,
		OFFLINE_05_PARAMETERS,
		AVATAR_VIEWER,
	}


	/// <summary>
	/// This class contains methods to get path to the plugin folders
	/// All plugin files must be in the "itseez3d/avatar_sdk" directory. But the "itseez3d/avatar_sdk" can be located in any place of your project.
	/// </summary>
	public static class PluginStructure
	{
		public static readonly string MISC_AUTH_RESOURCES_DIR = "itseez3d_misc/auth/resources";
		public static readonly string MISC_OFFLINE_RESOURCES_DIR = "itseez3d_misc/sdk_offline/resources/bin";
		public static readonly string OFFLINE_RESOURCES_DIR = "itseez3d/avatar_sdk/sdk_offline/resources/bin";
		public static readonly string PREFABS_DIR = "itseez3d_prefabs";
 
		public static readonly string VIEWER_SCENE_PATH = "itseez3d/avatar_sdk/samples_core/scenes/avatar_viewer.unity";
 

		private static readonly string itseez3dDir = "itseez3d";
		private static readonly string avatarSdkDir = "avatar_sdk";
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
		private static readonly string assetsDir = "Assets";
#endif

		private static Dictionary<SampleScene, string> scenesPaths = new Dictionary<SampleScene, string>()
		{
			{ SampleScene.CLOUD_00_ALL_SAMPLES, "itseez3d/avatar_sdk/samples_cloud/00_all_samples_cloud/scenes/00_all_samples_cloud.unity" },
			{ SampleScene.CLOUD_01_GETTING_STARTED, "itseez3d/avatar_sdk/samples_cloud/01_getting_started_sample_cloud/scenes/01_getting_started_sample_cloud.unity" },
			{ SampleScene.CLOUD_02_GALLERY, "itseez3d/avatar_sdk/samples_cloud/02_gallery_sample_cloud/scenes/02_gallery_sample_cloud.unity" },
			{ SampleScene.CLOUD_03_FULLBODY, "itseez3d/avatar_sdk/samples_cloud/03_fullbody_sample_cloud/scenes/03_fullbody_sample_cloud.unity" },
			{ SampleScene.CLOUD_04_LOD, "itseez3d/avatar_sdk/samples_cloud/04_lod_sample_cloud/scenes/04_lod_sample_cloud.unity" },
			{ SampleScene.CLOUD_05_PARAMETERS, "itseez3d/avatar_sdk/samples_cloud/05_parameters_sample_cloud/scenes/05_parameters_sample_cloud.unity" },
			{ SampleScene.CLOUD_06_CARTOONISH_AVATAR, "itseez3d/avatar_sdk/samples_cloud/06_cartoonish_avatar_sample_cloud/scenes/06_cartoonish_avatar_sample_cloud.unity" },
			{ SampleScene.CLOUD_07_WEBGL, "itseez3d/avatar_sdk/samples_cloud/07_webgl_sample/scenes/07_webgl_sample.unity" },
			{ SampleScene.CLOUD_08_FULLBODY_LEGACY, "itseez3d/avatar_sdk/samples_cloud/08_fullbody_legacy_sample_cloud/scenes/08_fullbody_legacy_sample_cloud.unity" },
			{ SampleScene.CLOUD_09_MULTIPLAYER, "itseez3d/avatar_sdk/samples_cloud/09_multiplayer_sample_cloud/scenes/09_multiplayer_sample_cloud.unity" },
			{ SampleScene.OFFLINE_00_ALL_SAMPLES, "itseez3d/avatar_sdk/samples_offline/00_all_samples_offline/scenes/00_all_samples_offline.unity" },
			{ SampleScene.OFFLINE_01_GETTING_STARTED, "itseez3d/avatar_sdk/samples_offline/01_getting_started_sample_offline/scenes/01_getting_started_sample_offline.unity" },
			{ SampleScene.OFFLINE_02_GALLERY, "itseez3d/avatar_sdk/samples_offline/02_gallery_sample_offline/scenes/02_gallery_sample_offline.unity" },
			{ SampleScene.OFFLINE_03_FULLBODY, "itseez3d/avatar_sdk/samples_offline/03_fullbody_sample_offline/scenes/03_fullbody_sample_offline.unity" },
			{ SampleScene.OFFLINE_04_LOD, "itseez3d/avatar_sdk/samples_offline/04_lod_sample_offline/scenes/04_lod_sample_offline.unity" },
			{ SampleScene.OFFLINE_05_PARAMETERS, "itseez3d/avatar_sdk/samples_offline/05_parameters_sample_offline/scenes/05_parameters_sample_offline.unity" },
			{ SampleScene.AVATAR_VIEWER, "itseez3d/avatar_sdk/samples_core/scenes/avatar_viewer.unity" },
		};

		public static string GetPluginDirectoryPath(string dir, PathOriginOptions originOption)
		{
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
			string pluginLocationPath = FindPluginLocation(Application.dataPath);
			if (string.IsNullOrEmpty(pluginLocationPath))
			{
				Debug.LogError("Avatar SDK plugin location can't be found!");
				return null;
			}

			if (originOption == PathOriginOptions.RelativeToAssetsFolder)
				pluginLocationPath = pluginLocationPath.Substring(pluginLocationPath.IndexOf(assetsDir));

			return Path.Combine(pluginLocationPath, dir);
#else
			return dir;
#endif
		}

		/// <summary>
		/// Creates the directory inside "Assets" folder if necessary.
		/// </summary>
		public static void CreatePluginDirectory(string dir)
		{
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
			if (!dir.Contains(assetsDir))
			{
				Debug.LogErrorFormat("Invalid directory: {0}", dir);
			}

			dir = dir.Substring(dir.IndexOf(assetsDir));
			string[] folders = dir.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			List<string> existingPath = new List<string> { "Assets" };
			for (int i = 1; i < folders.Length; ++i)
			{
				var prevPathStr = string.Join("/", existingPath.ToArray());
				existingPath.Add(folders[i]);
				var existingPathStr = string.Join("/", existingPath.ToArray());
				if (!AssetDatabase.IsValidFolder(existingPathStr))
					AssetDatabase.CreateFolder(prevPathStr, folders[i]);
				AssetDatabase.SaveAssets();
			}
			AssetDatabase.Refresh();
#endif
		}

 
		public static string GetScenePath(SampleScene scene, bool withExtension = false)
		{
			string scenePath = GetPluginDirectoryPath(scenesPaths[scene], PathOriginOptions.RelativeToAssetsFolder).Replace('\\', '/');
			if (!withExtension)
				scenePath = scenePath.Remove(scenePath.IndexOf(".unity"));
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
			scenePath = scenePath.Substring("Assets/".Length);
#endif
			return scenePath;
		}

		/// <summary>
		/// Find location of the avatar plugin in the project structure
		/// </summary>
		private static string FindPluginLocation(string rootDir)
		{
			foreach (string dirPath in Directory.GetDirectories(rootDir))
			{
				string dir = Path.GetFileName(dirPath);
				if (dir == itseez3dDir)
				{
					foreach (string itseez3dSubdirPath in Directory.GetDirectories(dirPath))
					{
						if (itseez3dSubdirPath.EndsWith(avatarSdkDir))
							return rootDir;
					}
				}
				else
				{
					string location = FindPluginLocation(dirPath);
					if (!string.IsNullOrEmpty(location))
						return location;
				}
			}
			return string.Empty;
		}
	}
}
