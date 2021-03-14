using GLTF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;

namespace ItSeez3D.AvatarSdk.Core.GLTF
{
	/// <summary>
	/// Class to load an avatar model in GLTF format and to add it to the unity scene.
	/// </summary>
	public class GLTFAvatarLoader
	{
		private class AvatarMeshObject
		{
			public GameObject gameObject;
			public SkinnedMeshRenderer renderer;

			public AvatarMeshObject(GameObject gameObject, SkinnedMeshRenderer renderer)
			{
				this.gameObject = gameObject;
				this.renderer = renderer;
			}
		}

		/// <summary>
		/// Shader for rendering avatar body mesh
		/// </summary>
		public Shader BodyShader { get; set; }

		private AvatarMeshObject body;

		private Dictionary<string, AvatarMeshObject> haircuts = new Dictionary<string, AvatarMeshObject>();
		private List<string> haircutsNames = new List<string>();
		private int currentHaircutIdx = -1;

		private bool playAnimationOnStart = false;

		private bool useLighting = false;

		public GLTFAvatarLoader()
		{
			BodyShader = ShadersUtils.GetHeadShader(useLighting);
		}

		/// <summary>
		/// Load the model on the scene
		/// </summary>
		/// <param name="avatarCode">Full body avatar code</param>
		/// <param name="avatarGameObject">Parent GameObject for the avatar</param>
		public IEnumerator LoadModelOnSceneAsync(string avatarCode, GameObject avatarGameObject)
		{
			string meshFilename = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.MESH_GLTF);
			yield return LoadAsync(meshFilename, avatarGameObject);
		}

		/// <summary>
		/// Load the model on the scene
		/// </summary>
		/// <param name="filePath">Path to the GLTF file</param>
		/// <param name="avatarGameObject">Parent GameObject for the avatar</param>
		public IEnumerator LoadAsync(string filePath, GameObject avatarGameObject)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				yield break;

			var importOptions = new ImportOptions();

			CoroutineGLTFSceneImporter sceneImporter = null;
			try
			{
				sceneImporter = new CoroutineGLTFSceneImporter(Path.GetFileName(filePath), importOptions);

				string directoryPath = URIHelper.GetDirectoryName(filePath);
				importOptions.DataLoader = new UnityGLTF.Loader.FileLoader(directoryPath);

				sceneImporter.SceneParent = avatarGameObject;
				//sceneImporter.CustomShaderName = BodyShader ? BodyShader.name : null;

				yield return sceneImporter.LoadSceneAsync();

				SkinnedMeshRenderer[] renderers = avatarGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer renderer in renderers)
				{
					if (renderer.gameObject.name.StartsWith("haircut"))
					{
						string haircutName = ExtractHaircutName(renderer.gameObject.name);
						AvatarMeshObject haircutObject = new AvatarMeshObject(renderer.gameObject, renderer);
						haircutObject.gameObject.SetActive(false);
						haircutObject.renderer.sharedMaterial.shader = ShadersUtils.GetHaircutShader(haircutName, useLighting);
						haircuts.Add(haircutName, haircutObject);
					}
					else if (renderer.gameObject.name == "mesh")
					{
						body = new AvatarMeshObject(renderer.gameObject, renderer);
						body.renderer.sharedMaterial.shader = BodyShader;
						RenameBlendshapes(body.renderer.sharedMesh);
					}
				}

				haircutsNames = haircuts.Keys.ToList();

				var animations = sceneImporter.SceneParent.GetComponents<Animation>();
				if (playAnimationOnStart && animations.Any())
				{
					animations.FirstOrDefault().Play();
				}
			}
			finally
			{
				if (importOptions.DataLoader != null)
				{
					sceneImporter?.Dispose();
					sceneImporter = null;
					importOptions.DataLoader = null;
				}
			}
		}

		/// <summary>
		/// Returns GameObject on the body mesh
		/// </summary>
		public GameObject GetBodyObject()
		{
			return body.gameObject;
		}

		/// <summary>
		/// Returns list of available haircuts
		/// </summary>
		public List<string> GetHaircuts()
		{
			return haircuts.Keys.ToList();
		}

		/// <summary>
		/// Returns name of the current displayed haircut 
		/// </summary>
		public string GetCurrentHaircutName()
		{
			return currentHaircutIdx >= 0 ? haircutsNames[currentHaircutIdx] : string.Empty;
		}

		/// <summary>
		/// Shows haircut by name
		/// </summary>
		public void ShowHaircut(string haircutName)
		{
			currentHaircutIdx = -1;
			for (int i=0; i<haircutsNames.Count; i++)
			{
				haircuts[haircutsNames[i]].gameObject.SetActive(false);
				if (haircutName == haircutsNames[i])
				{
					currentHaircutIdx = i;
					haircuts[haircutName].gameObject.SetActive(true);
				}
			}
		}

		/// <summary>
		/// Shows haircut by index
		/// </summary>
		public void ShowHaircut(int haircutIdx)
		{
			if (haircutIdx >= 0 && haircutIdx < haircutsNames.Count)
				ShowHaircut(haircutsNames[haircutIdx]);
			else
				ShowHaircut(string.Empty);
		}

		/// <summary>
		/// Shows next haircut
		/// </summary>
		public void ShowNextHaircut()
		{
			int haircutidx = currentHaircutIdx + 1;
			if (haircutidx >= haircutsNames.Count)
				haircutidx = -1;
			ShowHaircut(haircutidx);
		}

		/// <summary>
		/// Shows previous haircut
		/// </summary>
		public void ShowPrevHaircut()
		{
			int haircutIdx = currentHaircutIdx - 1;
			if (haircutIdx < -1)
				haircutIdx = haircutsNames.Count - 1;
			ShowHaircut(haircutIdx);
		}

		/// <summary>
		/// Returns a list of available blendshapes
		/// </summary>
		public List<string> GetBlendshapes()
		{
			List<string> blendshapes = new List<string>();
			for (int i = 0; i < body.renderer.sharedMesh.blendShapeCount; i++)
				blendshapes.Add(body.renderer.sharedMesh.GetBlendShapeName(i));
			return blendshapes;
		}

		/// <summary>
		/// Sets all blendshapes weights to zero
		/// </summary>
		public void ClearBlendshapesWeights()
		{
			for (int i = 0; i < body.renderer.sharedMesh.blendShapeCount; i++)
				body.renderer.SetBlendShapeWeight(i, 0.0f);
		}

		/// <summary>
		/// Sets the weight for the blendshapes with the provided index
		/// </summary>
		public void SetBlendshapeWeight(int blendshapeIdx, float weight)
		{
			body.renderer.SetBlendShapeWeight(blendshapeIdx, weight);
		}

		private string ExtractHaircutName(string haircutObjectName)
		{
			string prefixPattern = "haircut_";
			int pos = haircutObjectName.IndexOf(prefixPattern);
			return haircutObjectName.Substring(pos + prefixPattern.Length);
		}

		private void RenameBlendshapes(Mesh avatarMesh)
		{
			if (avatarMesh.blendShapeCount == 0)
				return;

			List<Vector3[]> blendshapesDeltaVertices = new List<Vector3[]>();
			for (int i=0; i<avatarMesh.blendShapeCount; i++)
			{
				Vector3[] deltaVertices = new Vector3[avatarMesh.vertexCount];
				avatarMesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, null, null);
				blendshapesDeltaVertices.Add(deltaVertices);
			}

			avatarMesh.ClearBlendShapes();

			int blendshapeIdx = 0;
			if (blendshapesDeltaVertices.Count == 51 || blendshapesDeltaVertices.Count == 66)
			{
				TextAsset mobileBlendshapesListAsset = Resources.Load<TextAsset>("mobile_51_list");
				var mobileList = mobileBlendshapesListAsset.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < 51; i++, blendshapeIdx++)
					avatarMesh.AddBlendShapeFrame(mobileList[i], 100, blendshapesDeltaVertices[blendshapeIdx], null, null);
			}
			if (blendshapesDeltaVertices.Count == 15 || blendshapesDeltaVertices.Count == 66)
			{
				TextAsset visemesBlendshapesListAsset = Resources.Load<TextAsset>("visemes_15_list");
				var visemesList = visemesBlendshapesListAsset.text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < 15; i++, blendshapeIdx++)
					avatarMesh.AddBlendShapeFrame(visemesList[i], 100, blendshapesDeltaVertices[blendshapeIdx], null, null);
			}
		}
	}
}
