/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using ItSeez3D.AvatarSdk.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{

	
	/// <summary>
	/// This sample demonstrates how to use the level-of-details functionality.
	/// </summary>
	public class LODSample : GettingStartedSample
	{
		#region UI
		public Text detailsLevelText = null;
		public Text currentBlendshapeText = null;
		public GameObject[] avatarControls = null;
		public ItemsSelectingView blendshapesSelectingView = null;

		public ToggleGroup lodToggleGroup = null;
		public Toggle lodTogglePrefab = null;
		public GameObject lodTogglesPanel = null;

		public Button convertToObjButton = null;
		public Button convertToFbxButton = null;
		public Button createPrefabButton = null;
		public GameObject head2AdditionalParamsPanel = null;
		#endregion

		private int currentDetailsLevel = 0;
		private int currentBlendshape = 0;

		// Blendshapes names with their index in avatar mesh
		private Dictionary<int, string> availableBlendshapes = new Dictionary<int, string>();

		protected List<Toggle> toggles = new List<Toggle>();

		HashSet<PipelineType> setOfHead2Pipelines = new HashSet<PipelineType> { PipelineType.HEAD_2_0_BUST_MOBILE, PipelineType.HEAD_2_0_HEAD_MOBILE };

		#region GettingStartedSample overrided methods
		public override void OnPipelineTypeToggleChanged(PipelineType newType)
		{
			if(setOfHead2Pipelines.Contains(newType))
			{
				head2AdditionalParamsPanel.SetActive(true);
			} else if(head2AdditionalParamsPanel.activeSelf)
			{
				head2AdditionalParamsPanel.SetActive(false);
			}
			base.OnPipelineTypeToggleChanged(newType);
		}
		protected override void Start()
		{
			selectedPipelineType = PipelineType.HEAD_2_0_HEAD_MOBILE;
			Debug.Log("LOD functionality is currently supported for head/mobile subtype only");
			base.Start();
		}

		protected override IEnumerator GenerateAvatarFunc(byte[] photoBytes)
		{
			EnableAvatarControls(false);
			int numberOfToggles;
			switch(selectedPipelineType)
			{
				case PipelineType.FACE:
					numberOfToggles = 9;
					break;
				case PipelineType.HEAD_2_0_BUST_MOBILE:
					numberOfToggles = 0;
					break;
				default:
					numberOfToggles = 8;
					break;
			}
			InitLodToggles(numberOfToggles);
			yield return base.GenerateAvatarFunc(photoBytes);
			EnableAvatarControls(true);
		}


		protected override IEnumerator ConfigureComputationParameters(PipelineType pipelineType, ComputationParameters computationParameters)
		{
			// Generate avatar with all available blendshapes and without haircuts
			var parametersRequest = avatarProvider.GetParametersAsync(ComputationParametersSubset.ALL, pipelineType);
			yield return parametersRequest;
			if (parametersRequest.IsError)
				yield break;

			computationParameters.blendshapes = parametersRequest.Result.blendshapes;
			computationParameters.avatarModifications = parametersRequest.Result.avatarModifications;

			if(setOfHead2Pipelines.Contains(pipelineType))
			{
				var additionalParamsModel = head2AdditionalParamsPanel.GetComponent<Head2AdditionalLodParameters>().Model;
				string errorText = "";
				if(!additionalParamsModel.IsValid(out errorText))
				{
					Debug.LogError(errorText);
					yield break;
				}
				if (additionalParamsModel.NumberOfFaces.HasValue)
				{
					computationParameters.avatarModifications.generatedHaircutFacesCount.Value = additionalParamsModel.NumberOfFaces.Value;
				}
				if (additionalParamsModel.HaircutTextureSize != null)
				{
					computationParameters.avatarModifications.generatedHaircutTextureSize.Value = additionalParamsModel.HaircutTextureSize;
				}
				if (additionalParamsModel.ModelTextureSize != null)
				{
					computationParameters.avatarModifications.textureSize.Value = additionalParamsModel.ModelTextureSize;
				}
				computationParameters.haircuts.AddValue("base\\generated");
			}
		}

		protected AsyncRequest<TexturedMesh> GetHaircut(string currentAvatarCode, PipelineType pipeline)
		{
			AsyncRequest<TexturedMesh> result;
			if (setOfHead2Pipelines.Contains(pipeline))
			{
				currentHaircutId = "base/generated";
				result = avatarProvider.GetHaircutMeshAsync(currentAvatarCode, currentHaircutId);
			}
			else
			{
				currentHaircutId = string.Empty;
				result = new AsyncRequest<TexturedMesh>();
				result.Result = null;
				result.IsDone = true;
			}
			return result;
		}

		protected override IEnumerator GenerateAndDisplayHead(byte[] photoBytes, PipelineType pipeline)
		{
			ComputationParameters computationParameters = ComputationParameters.Empty;
			yield return ConfigureComputationParameters(pipeline, computationParameters);

			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, computationParameters);
			yield return Await(initializeRequest);
			currentAvatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(currentAvatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(currentAvatarCode);
			yield return Await(calculateRequest);

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(currentAvatarCode, true, currentDetailsLevel);
			yield return Await(avatarHeadRequest);

			var haircutRequest = GetHaircut(currentAvatarCode, pipeline);
			yield return haircutRequest;
			TexturedMesh headTexturedMesh = null;
			if (haircutRequest.IsError)
			{
				currentHaircutId = string.Empty;
			} else
			{
				headTexturedMesh = haircutRequest.Result;
			}
			DisplayHead(avatarHeadRequest.Result, headTexturedMesh);

			//Retrieve blendshape names from the mesh and add an empty blendshape with index -1
			Mesh mesh = avatarHeadRequest.Result.mesh;
			availableBlendshapes.Clear();
			availableBlendshapes.Add(-1, "None");
			for (int i = 0; i < mesh.blendShapeCount; i++)
				availableBlendshapes.Add(i, mesh.GetBlendShapeName(i));
			ChangeCurrentBlendshape(-1);
			blendshapesSelectingView.InitItems(availableBlendshapes.Values.ToList());

			detailsLevelText.text = string.Format("Triangles count:\n{0}", avatarHeadRequest.Result.mesh.triangles.Length / 3);

			IMeshConverter meshConverter = AvatarSdkMgr.IoCContainer.Create<IMeshConverter>();
			if (meshConverter.IsObjConvertEnabled)
				convertToObjButton.gameObject.SetActive(true);

			if (meshConverter.IsFBXExportEnabled)
				convertToFbxButton.gameObject.SetActive(true);

#if UNITY_EDITOR_WIN
			if (meshConverter.IsFBXExportEnabled)
				createPrefabButton.gameObject.SetActive(true);
#endif
		}

		protected override void SetControlsInteractable(bool interactable)
		{
			base.SetControlsInteractable(interactable);
			if (toggles != null)
			{
				foreach (Toggle t in toggles)
					t.interactable = interactable;
			}
		}

		#endregion

		#region UI handling
		public void PrevBlendshapeClick()
		{
			ChangeCurrentBlendshape(currentBlendshape - 1);
		}

		public void NextBlendshapeClick()
		{
			ChangeCurrentBlendshape(currentBlendshape + 1);
		}

		public void OnBlendshapeListButtonClick()
		{
			SetControlsInteractable(false);
			blendshapesSelectingView.Show(new List<string>() { availableBlendshapes[currentBlendshape] }, list =>
			{
				SetControlsInteractable(true);
				// Find KeyValuePair for selected blendshape name. Assume that returned list contains only one element.
				var pair = availableBlendshapes.FirstOrDefault(p => p.Value == list[0]);
				ChangeCurrentBlendshape(pair.Key);
			});
		}

		public void ConvertAvatarToObjFormat()
		{
			var headObject = GameObject.Find(HEAD_OBJECT_NAME);
			var headMeshRenderer = headObject.GetComponent<SkinnedMeshRenderer>();

			var outputObjDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(currentAvatarCode, AvatarSubdirectory.OBJ_EXPORT);
			var outputObjFile = IOUtils.CombinePaths(outputObjDir, "model.obj");
			CoreTools.SaveAvatarMesh(headMeshRenderer, currentAvatarCode, outputObjFile, MeshFileFormat.OBJ, true, false, haircutId: currentHaircutId, levelOfDetails: currentDetailsLevel, recolorTexture: false);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			System.Diagnostics.Process.Start(outputObjDir);
#endif
			progressText.text = string.Format("OBJ file was saved to avatar directory");
		}

		public void ExportAvatarAsFbx()
		{
			var headObject = GameObject.Find(HEAD_OBJECT_NAME);
			var headMeshRenderer = headObject.GetComponent<SkinnedMeshRenderer>();

			var exportDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(currentAvatarCode, AvatarSubdirectory.FBX_EXPORT);
			var outputFbxFile = IOUtils.CombinePaths(exportDir, "model.fbx");
			CoreTools.SaveAvatarMesh(headMeshRenderer, currentAvatarCode, outputFbxFile, MeshFileFormat.FBX, haircutId: currentHaircutId, levelOfDetails: currentDetailsLevel, recolorTexture: false);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			System.Diagnostics.Process.Start(exportDir);
#else
			progressText.text = string.Format("FBX file was saved to avatar directory");
#endif
		}

		public void CreateAvatarPrefab()
		{
#if UNITY_EDITOR
			AvatarPrefabBuilder.Instance.CreateAvatarPrefab(GameObject.Find(AVATAR_OBJECT_NAME), HEAD_OBJECT_NAME, HAIRCUT_OBJECT_NAME, currentAvatarCode,
				currentHaircutId, new List<Type>() { typeof(RotateByMouse) }, currentDetailsLevel);
#endif
		}

		private void EnableAvatarControls(bool isEnabled)
		{
			if (avatarControls == null)
				return;

			for (int i=0; i<avatarControls.Length; i++)
				avatarControls[i].SetActive(isEnabled);
		}
		#endregion

		#region LOD methods
		private IEnumerator ChangeMeshDetailsLevel(int newDetailsLevel)
		{
			int numberOfLevels = selectedPipelineType == PipelineType.FACE ? 9 : 8;
			if (newDetailsLevel < 0 || newDetailsLevel > numberOfLevels)
				yield break;

			currentDetailsLevel = newDetailsLevel;
			SetControlsInteractable(false);
			yield return ChangeMeshResolution(currentAvatarCode, currentDetailsLevel);
			SetControlsInteractable(true);
		}

		private IEnumerator ChangeMeshResolution(string avatarCode, int detailsLevel)
		{
			var headObject = GameObject.Find(HEAD_OBJECT_NAME);
			if (headObject == null)
				yield break;

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(avatarCode, true, detailsLevel);
			yield return Await(avatarHeadRequest);

			SkinnedMeshRenderer meshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
			meshRenderer.sharedMesh = avatarHeadRequest.Result.mesh;
			detailsLevelText.text = string.Format("Triangles count:\n{0}", meshRenderer.sharedMesh.triangles.Length / 3);
		}

		private void InitLodToggles(int countDetailsLevels)
		{
			currentDetailsLevel = 0;

			foreach (Toggle t in toggles)
				Destroy(t.gameObject);
			toggles.Clear();
			lodTogglesPanel.SetActive(countDetailsLevels != 0);

			for (int i=0; i<countDetailsLevels; i++)
			{
				Toggle toggle = Instantiate<Toggle>(lodTogglePrefab);
				toggle.isOn = i == 0;
				toggle.gameObject.transform.SetParent(lodTogglesPanel.transform);
				toggle.group = lodToggleGroup;
				ToggleId toggleId = toggle.gameObject.GetComponentInChildren<ToggleId>();
				toggleId.Text = string.Format("LOD{0}", i);
				toggleId.Item = i;
				toggle.onValueChanged.AddListener((isChecked) => 
				{
					if (isChecked)
						StartCoroutine(ChangeMeshDetailsLevel((int)toggleId.Item));
				});
				toggles.Add(toggle);
			}
		}
		#endregion

		#region blendshapes method

		private void ChangeCurrentBlendshape(int blendshapeIdx)
		{
			if (!availableBlendshapes.ContainsKey(blendshapeIdx))
				return;

			currentBlendshape = blendshapeIdx;

			var headObject = GameObject.Find(HEAD_OBJECT_NAME);
			var meshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
			foreach (int idx in availableBlendshapes.Keys)
			{
				if (idx >= 0)
					meshRenderer.SetBlendShapeWeight(idx, idx == currentBlendshape ? 100.0f : 0.0f);
			}

			currentBlendshapeText.text = availableBlendshapes[currentBlendshape];
		}

		#endregion
	}
}
