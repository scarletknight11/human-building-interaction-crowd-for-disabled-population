﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItSeez3D.AvatarSdkSamples.Core.Editor
{
	[InitializeOnLoad]
	public class ScenesManager
	{
 
		private static List<SampleScene> cloudScenes = new List<SampleScene>()
		{
			SampleScene.CLOUD_01_GETTING_STARTED,
			SampleScene.CLOUD_02_GALLERY,
			SampleScene.CLOUD_03_FULLBODY,
			SampleScene.CLOUD_04_LOD,
			SampleScene.CLOUD_05_PARAMETERS,
			SampleScene.CLOUD_06_CARTOONISH_AVATAR,
			SampleScene.CLOUD_07_WEBGL,
			SampleScene.CLOUD_08_FULLBODY_LEGACY,
			SampleScene.CLOUD_09_MULTIPLAYER,
			SampleScene.AVATAR_VIEWER
		};

		private static List<SampleScene> offlineScenes = new List<SampleScene>()
		{
			SampleScene.OFFLINE_01_GETTING_STARTED,
			SampleScene.OFFLINE_02_GALLERY,
			SampleScene.OFFLINE_03_FULLBODY,
			SampleScene.OFFLINE_04_LOD,
			SampleScene.OFFLINE_05_PARAMETERS,
			SampleScene.AVATAR_VIEWER
		};


		private static Dictionary<SampleScene, List<SampleScene>> scenesDependencies = new Dictionary<SampleScene, List<SampleScene>>()
		{
			{ SampleScene.CLOUD_00_ALL_SAMPLES, cloudScenes },
			{ SampleScene.OFFLINE_00_ALL_SAMPLES, offlineScenes },
			{ SampleScene.CLOUD_02_GALLERY, new List<SampleScene>{ SampleScene.AVATAR_VIEWER } },
			{ SampleScene.OFFLINE_02_GALLERY, new List<SampleScene>{ SampleScene.AVATAR_VIEWER } },
			{ SampleScene.CLOUD_05_PARAMETERS, new List<SampleScene>{ SampleScene.AVATAR_VIEWER } },
			{ SampleScene.OFFLINE_05_PARAMETERS, new List<SampleScene>{ SampleScene.AVATAR_VIEWER } },
			{ SampleScene.CLOUD_07_WEBGL, new List<SampleScene>{ SampleScene.AVATAR_VIEWER } },
		};


		static ScenesManager()
		{
			EditorSceneManager.sceneOpened += (s, m) => {
				EnableOpenedScenesInBuildSettings();
			};

			EditorSceneManager.sceneClosed += s => {
				EnableOpenedScenesInBuildSettings();
			};
		}

		private static void EnableOpenedScenesInBuildSettings()
		{
			List<string> openedScenes = new List<string>();
			for (int i = 0; i < EditorSceneManager.sceneCount; i++)
			{
				var s = EditorSceneManager.GetSceneAt(i);
				if (s.isLoaded)
					openedScenes.Add(s.path);
			}
			EnableScenesInBuildSettings(openedScenes);
		}

		private static void EnableScenesInBuildSettings(List<string> openedScenes)
		{
			List<EditorBuildSettingsScene> scenesInBuildSettings = EditorBuildSettings.scenes.ToList();

			// if we opened one of the samples that has dependencies, let's add these dependencies scenes to the build settings.
			foreach (string openedScene in openedScenes)
			{

				foreach (var sdkScene in scenesDependencies)
				{
					string path = PluginStructure.GetScenePath(sdkScene.Key);
					if (openedScene.Contains(path))
					{
						foreach (SampleScene dependentScene in sdkScene.Value)
						{
							string dependentScenePath = "Assets/" + PluginStructure.GetScenePath(dependentScene, true);
							if (!ContainsScene(scenesInBuildSettings, openedScene))
								scenesInBuildSettings.Add(new EditorBuildSettingsScene(openedScene, true));
							if (!ContainsScene(scenesInBuildSettings, dependentScenePath))
								scenesInBuildSettings.Add(new EditorBuildSettingsScene(dependentScenePath, true));
						}
					}
				}
			}
			EditorBuildSettings.scenes = scenesInBuildSettings.ToArray();
		}

		private static bool ContainsScene(List<EditorBuildSettingsScene> scenesList, string scenePath)
		{
			EditorBuildSettingsScene existedScene = scenesList.FirstOrDefault(s =>
			{
				if (!string.IsNullOrEmpty(s.path))
					return string.Compare(Path.GetFullPath(s.path), Path.GetFullPath(scenePath)) == 0;
				else
					return false;
			});
			return existedScene != null;
		}
	}
}

#endif
