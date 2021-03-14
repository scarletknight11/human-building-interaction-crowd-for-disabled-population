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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace ItSeez3D.AvatarSdk.Core
{
	public class AvatarPrefabBuilder
	{
		#region signletion staff
		protected static AvatarPrefabBuilder instance = null;

		protected AvatarPrefabBuilder() { }

		public static AvatarPrefabBuilder Instance
		{
			get
			{
				if (instance == null)
					instance = new AvatarPrefabBuilder();
				return instance;
			}
		}
		#endregion

		public void CreateAvatarPrefab (GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId, 
			string haircutId, List<Type> removedObjectsTypes, int levelOfDetails = 0)
		{
			string prefabDir = Path.Combine(PluginStructure.GetPluginDirectoryPath(PluginStructure.PREFABS_DIR, PathOriginOptions.RelativeToAssetsFolder), avatarId);
			PluginStructure.CreatePluginDirectory(prefabDir);
			GameObject instantiatedAvatarObject = GameObject.Instantiate(avatarObject);
			SaveMeshAndMaterialForAvatarObject(prefabDir, instantiatedAvatarObject, headObjectName, haircutObjectName, avatarId, haircutId, levelOfDetails);

			CopyBlendshapesWeights(avatarObject, instantiatedAvatarObject, headObjectName);

			string prefabPath = prefabDir + "/avatar.prefab";
			if (removedObjectsTypes != null)
			{
				foreach (Type t in removedObjectsTypes)
				{
					var component = instantiatedAvatarObject.GetComponent(t);
					if (component != null)
						GameObject.DestroyImmediate(component);
				}
			}
#if UNITY_2018_3_OR_NEWER
			PrefabUtility.SaveAsPrefabAsset(instantiatedAvatarObject, prefabPath);
#else
			PrefabUtility.CreatePrefab(prefabPath, instantiatedAvatarObject);
#endif
			GameObject.DestroyImmediate(instantiatedAvatarObject);
			EditorUtility.DisplayDialog ("Prefab created successfully!", string.Format ("You can find your prefab in '{0}' folder", prefabDir), "Ok");
		}

		public void CreateAvatarUnifiedMeshPrefab(GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId,
			string haircutId, Color haircutColor, Vector4 haircutTint, List<Type> removedObjectsTypes)
		{
			string prefabDir = Path.Combine(PluginStructure.GetPluginDirectoryPath(PluginStructure.PREFABS_DIR, PathOriginOptions.RelativeToAssetsFolder), avatarId);
			PluginStructure.CreatePluginDirectory(prefabDir);
			GameObject unifiedAvatarObject = GameObject.Instantiate(avatarObject);
			GameObject haircutObject = GetChildByName(unifiedAvatarObject, haircutObjectName);
			if (haircutObject != null)
				GameObject.DestroyImmediate(haircutObject);
			SaveUnifiedMeshAndMaterialForAvatarObject(prefabDir, unifiedAvatarObject, headObjectName, haircutObjectName, avatarId, haircutId, haircutColor, haircutTint);

			CopyBlendshapesWeights(avatarObject, unifiedAvatarObject, headObjectName);

			string prefabPath = prefabDir + "/avatar.prefab";
			if (removedObjectsTypes != null)
			{
				foreach (Type t in removedObjectsTypes)
				{
					var component = unifiedAvatarObject.GetComponent(t);
					if (component != null)
						GameObject.DestroyImmediate(component);
				}
			}
#if UNITY_2018_3_OR_NEWER
			PrefabUtility.SaveAsPrefabAsset(unifiedAvatarObject, prefabPath);
#else
			PrefabUtility.CreatePrefab(prefabPath, unifiedAvatarObject);
#endif
			GameObject.DestroyImmediate(unifiedAvatarObject);
			EditorUtility.DisplayDialog("Prefab created successfully!", string.Format("You can find your prefab in '{0}' folder", prefabDir), "Ok");
		}

		protected void SaveMeshAndMaterialForAvatarObject(string prefabDir, GameObject avatarObject, string headObjectName, string haircutObjectName, 
			string avatarId, string haircutId, int levelOfDetails = 0)
		{
			GameObject headObject = GetChildByName(avatarObject, headObjectName);
			GameObject hairObject = GetChildByName(avatarObject, haircutObjectName);

			if (headObject != null)
			{
				string meshFilePath = Path.Combine(prefabDir, "head.fbx");
				string textureFilePath = CoreTools.GetOutputTextureFilename(meshFilePath);
				SkinnedMeshRenderer headMeshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
				headMeshRenderer.sharedMesh = SaveHeadMeshAsFbxAsset(avatarId, meshFilePath, levelOfDetails);
				headMeshRenderer.sharedMaterial.mainTexture = LoadTextureAsset(textureFilePath);
				headMeshRenderer.sharedMaterial = InstantiateAndSaveMaterial(headMeshRenderer.sharedMaterial, Path.Combine(prefabDir, "head_material.mat"));

				for (int i = 0; i < headMeshRenderer.sharedMesh.blendShapeCount; i++)
					headMeshRenderer.SetBlendShapeWeight(i, 0.0f);
			}

			if (hairObject != null && !string.IsNullOrEmpty(haircutId))
			{
				string haircutMeshFile = Path.Combine(prefabDir, "haircut.fbx");
				string haircutTextureFile = CoreTools.GetOutputTextureFilename(haircutMeshFile);
				MeshRenderer hairMeshRenderer = hairObject.GetComponentInChildren<MeshRenderer>();
				if (hairMeshRenderer != null)
				{
					hairObject.GetComponentInChildren<MeshFilter>().mesh = SaveHaircutMeshAsFbxAsset(avatarId, haircutId, haircutMeshFile);
					hairMeshRenderer.sharedMaterial.mainTexture = LoadTextureAsset(haircutTextureFile);
					hairMeshRenderer.sharedMaterial = InstantiateAndSaveMaterial(hairMeshRenderer.sharedMaterial, Path.Combine(prefabDir, "haircut_material.mat"));
				}
				else
				{
					SkinnedMeshRenderer hairSkinnedMeshRenderer = hairObject.GetComponentInChildren<SkinnedMeshRenderer>();
					if (hairSkinnedMeshRenderer != null)
					{
						hairSkinnedMeshRenderer.sharedMesh = SaveHaircutMeshAsFbxAsset(avatarId, haircutId, haircutMeshFile);
						hairSkinnedMeshRenderer.sharedMaterial.mainTexture = LoadTextureAsset(haircutTextureFile);
						hairSkinnedMeshRenderer.sharedMaterial = InstantiateAndSaveMaterial(hairSkinnedMeshRenderer.sharedMaterial, Path.Combine(prefabDir, "haircut_material.mat"));
					}
				}
			}

			AssetDatabase.SaveAssets();
		}

		protected void SaveUnifiedMeshAndMaterialForAvatarObject(string prefabDir, GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId, 
			string haircutId, Color haircutColor, Vector4 haircutTint)
		{
			if (string.IsNullOrEmpty(haircutId))
			{
				SaveMeshAndMaterialForAvatarObject(prefabDir, avatarObject, headObjectName, haircutObjectName, avatarId, string.Empty);
			}
			else
			{
				GameObject headObject = GetChildByName(avatarObject, headObjectName);

				string meshFilePath = Path.Combine(prefabDir, "head_with_haircut.fbx");
				string textureFilePath = CoreTools.GetOutputTextureFilename(meshFilePath);
				SkinnedMeshRenderer headMeshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
				headMeshRenderer.sharedMesh = SaveAvatarUnifiedMeshAsFbxAsset(headMeshRenderer, avatarId, haircutId, meshFilePath, haircutColor, haircutTint);
				headMeshRenderer.sharedMaterial = new Material(ShadersUtils.GetHeadShader(false));
				headMeshRenderer.sharedMaterial.mainTexture = LoadTextureAsset(textureFilePath);
				headMeshRenderer.sharedMaterial = InstantiateAndSaveMaterial(headMeshRenderer.sharedMaterial, Path.Combine(prefabDir, "avatar_material.mat"));
			}

			AssetDatabase.SaveAssets();
		}

		protected Material InstantiateAndSaveMaterial(Material material, string assetPath)
		{
			Material instanceMat = GameObject.Instantiate(material);
			AssetDatabase.CreateAsset(instanceMat, assetPath);
			return instanceMat;
		}

		protected Texture2D LoadTextureAsset(string texturePath)
		{
			return (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
		}

		protected Texture2D SaveTextureAsset(Texture texture, string texturePath)
		{
			Texture2D texture2D = texture as Texture2D;
			if (texture2D.format == TextureFormat.DXT1 || texture2D.format == TextureFormat.DXT5)
			{
				//It is compressed texture, so it is already saved somewhere
				return texture2D;
			}

			ImageUtils.SaveTextureToFile(texture2D, texturePath);
			AssetDatabase.Refresh();

			TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			textureImporter.isReadable = true;
			textureImporter.SaveAndReimport();

			return LoadTextureAsset(texturePath);
		}

		protected Mesh SaveAvatarUnifiedMeshAsFbxAsset(SkinnedMeshRenderer headMeshRenderer, string avatarId, string haircutId, string fbxPath, 
			Color haircutColor, Vector4 haircutTint)
		{
			CoreTools.SaveAvatarMesh(headMeshRenderer, avatarId, fbxPath, MeshFileFormat.FBX, false, true, haircutId, haircutColor, haircutTint);
			AssetDatabase.Refresh();
			Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(fbxPath, typeof(Mesh));
			return mesh;
		}

		protected Mesh SaveHeadMeshAsFbxAsset(string avatarId, string fbxPath, int levelOfDetails)
		{
			CoreTools.SaveAvatarMesh(null, avatarId, fbxPath, MeshFileFormat.FBX, false, true, levelOfDetails: levelOfDetails);
			AssetDatabase.Refresh();

			ModelImporter modelImporter = ModelImporter.GetAtPath(fbxPath) as ModelImporter;
			modelImporter.isReadable = true;
			modelImporter.SaveAndReimport();

			Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(fbxPath, typeof(Mesh));
			return mesh;
		}

		protected Mesh SaveHaircutMeshAsFbxAsset(string avatarId, string haircutId, string fbxPath)
		{
			CoreTools.HaircutPlyToFbx(avatarId, haircutId, fbxPath);
			AssetDatabase.Refresh();

			ModelImporter modelImporter = ModelImporter.GetAtPath(fbxPath) as ModelImporter;
			modelImporter.isReadable = true;
			modelImporter.SaveAndReimport();

			Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(fbxPath, typeof(Mesh));
			return mesh;
		}

		protected GameObject GetChildByName (GameObject obj, string name)
		{
			var children = obj.GetComponentsInChildren<Transform> ();
			foreach (var child in children) {
				if (child.name.ToLower () == name.ToLower ())
					return child.gameObject;
			}

			return null;
		}

		protected void CopyBlendshapesWeights(GameObject srcAvatarObject, GameObject dstAvatarObject, string headObjectName)
		{
			GameObject srcAvatarHead = GetChildByName(srcAvatarObject, headObjectName);
			GameObject dstAvatarHead = GetChildByName(dstAvatarObject, headObjectName);

			SkinnedMeshRenderer srcMeshRenderer = srcAvatarHead.GetComponentInChildren<SkinnedMeshRenderer>();
			SkinnedMeshRenderer dstMeshRenderer = dstAvatarHead.GetComponentInChildren<SkinnedMeshRenderer>();

			for (int i = 0; i < dstMeshRenderer.sharedMesh.blendShapeCount; i++)
				dstMeshRenderer.SetBlendShapeWeight(i, 0.0f);

			for (int i=0; i<srcMeshRenderer.sharedMesh.blendShapeCount; i++)
			{
				string blendshapeName = srcMeshRenderer.sharedMesh.GetBlendShapeName(i);
				int idx = dstMeshRenderer.sharedMesh.GetBlendShapeIndex(blendshapeName);
				if (idx >= 0)
				{
					dstMeshRenderer.SetBlendShapeWeight(idx, srcMeshRenderer.GetBlendShapeWeight(i));
				}
			}
		}
	}
}
#endif
