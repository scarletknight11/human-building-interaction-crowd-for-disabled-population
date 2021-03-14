/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, May 2020
*/

using ItSeez3D.AvatarSdk.Cloud;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdk.Core.GLTF;
using ItSeez3D.AvatarSdkSamples.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
	public class FullbodyBasicSample : GettingStartedSample
	{
		public ParametersConfigurationPanel parametersPanel = null;

		public GameObject baseControlsParent;

		public GameObject generatedAvatarControlsParent;

		public GameObject haircutsControlsParent;

		public GameObject blendshapesControlsParent;

		public GameObject animationControlsParent;

		public Text haircutNameText;

		public Text blendshapeNameText;

		public HaircutsSelectingView haircutsSelectingView;

		public ItemsSelectingView blendshapesSelectingView;

		public ModelInfoDataPanel modelInfoPanel;

		public AnimationManager animationManager;


		private readonly string generatedHaircutName = "base/generated";

		private readonly string baldHaircutName = "bald";

		private bool isParametersPanelActive = false;

		private bool isAvatarDisplayed = false;

		private GLTFAvatarLoader avatarLoader = null;

		private List<string> blendshapes = null;

		private int currentBlendshapeIdx = -1;

		#region public methods
		public FullbodyBasicSample()
		{
			selectedPipelineType = PipelineType.FULLBODY;
		}

		public void OnParametersButtonClick()
		{
			isParametersPanelActive = !isParametersPanelActive;
			parametersPanel.SwitchActiveState(isParametersPanelActive);

			progressText.gameObject.SetActive(!isParametersPanelActive);
			generatedAvatarControlsParent.SetActive(!isParametersPanelActive && isAvatarDisplayed);
		}

		public void OnNextHaircutButtonClick()
		{
			avatarLoader.ShowNextHaircut();
			haircutNameText.text = avatarLoader.GetCurrentHaircutName();
		}

		public void OnPrevHaircutButtonClick()
		{
			avatarLoader.ShowPrevHaircut();
			haircutNameText.text = avatarLoader.GetCurrentHaircutName();
		}

		public void OnHaircutListButtonClick()
		{
			baseControlsParent.SetActive(false);
			string currentHaircutName = avatarLoader.GetCurrentHaircutName();
			if (string.IsNullOrEmpty(currentHaircutName))
				currentHaircutName = baldHaircutName;
			haircutsSelectingView.Show(new List<string>() { currentHaircutName }, list =>
			{
				baseControlsParent.SetActive(true);
				avatarLoader.ShowHaircut(list[0]);
				haircutNameText.text = avatarLoader.GetCurrentHaircutName();
			});
		}

		public void OnNextBlendshapeButtonClick()
		{
			currentBlendshapeIdx++;
			if (currentBlendshapeIdx >= blendshapes.Count)
				currentBlendshapeIdx = 0;
			SetBlendshape(currentBlendshapeIdx);
		}

		public void OnPrevBlendshapeButtonClick()
		{
			currentBlendshapeIdx--;
			if (currentBlendshapeIdx < 0)
				currentBlendshapeIdx = blendshapes.Count - 1;
			SetBlendshape(currentBlendshapeIdx);
		}

		public void OnBlendshapesListButtonClick()
		{
			baseControlsParent.SetActive(false);
			blendshapesSelectingView.Show(new List<string>() { blendshapes[currentBlendshapeIdx] }, list =>
			{
				baseControlsParent.SetActive(true);
				currentBlendshapeIdx = blendshapes.IndexOf(list[0]);
				SetBlendshape(currentBlendshapeIdx);
			});
		}

		public void OnDownloadFbxButtonClick()
		{
			StartCoroutine(DownloadFullbodyMeshAsync(MeshFormat.FBX));
		}

		public void OnDownloadPlyButtonClick()
		{
			StartCoroutine(DownloadFullbodyMeshAsync(MeshFormat.PLY));
		}
		#endregion public methods

		#region base overrided methods
		protected override IEnumerator CheckAvailablePipelines()
		{
			// Fullbody avatars are available on the Pro plan. Need to verify it.
			SetControlsInteractable(false);
			var pipelineAvailabilityRequest = avatarProvider.IsPipelineSupportedAsync(selectedPipelineType);
			yield return Await(pipelineAvailabilityRequest);
			if (pipelineAvailabilityRequest.IsError)
				yield break;

			if (pipelineAvailabilityRequest.Result == true)
			{
				yield return UpdateAvatarParameters();
				progressText.text = string.Empty;
				SetControlsInteractable(true);
			}
			else
			{
				string errorMsg = "You can't generate fullbody avatars.\nThis option is available on the PRO plan.";
				progressText.text = errorMsg;
				progressText.color = Color.red;
				Debug.LogError(errorMsg);
			}
		}

		protected override IEnumerator GenerateAndDisplayHead(byte[] photoBytes, PipelineType pipeline)
		{
			isAvatarDisplayed = false;
			generatedAvatarPipeline = pipeline;
			generatedAvatarControlsParent.SetActive(false);

			if (isParametersPanelActive)
				OnParametersButtonClick();

			ComputationParameters computationParameters = ComputationParameters.Empty;
			yield return ConfigureComputationParameters(pipeline, computationParameters);

			// generate avatar from the photo and get its code in the Result of request
			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, computationParameters);
			yield return Await(initializeRequest);
			currentAvatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(currentAvatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(currentAvatarCode);
			yield return Await(calculateRequest);

			CloudAvatarProvider cloudAvatarProvider = avatarProvider as CloudAvatarProvider;
			var downloadingRequest = cloudAvatarProvider.DownloadFullbodyAvatarDataAsync(currentAvatarCode, MeshFormat.GLTF);
			yield return Await(downloadingRequest);

			var modelInfoRequest = cloudAvatarProvider.DownloadModelInfoAsync(currentAvatarCode);
			yield return Await(modelInfoRequest);

			yield return DisplayHead();

			ConfigureBlendshapesControls(computationParameters.blendshapes.FullNames);

			ModelInfo modelInfo = CoreTools.GetAvatarModelInfo(currentAvatarCode);
			modelInfoPanel.UpdateData(modelInfo);

			progressText.text = string.Empty;
			generatedAvatarControlsParent.SetActive(true);
			isAvatarDisplayed = true;
		}

		protected override IEnumerator ConfigureComputationParameters(PipelineType pipelineType, ComputationParameters computationParameters)
		{
			parametersPanel.ConfigureComputationParameters(computationParameters);
			yield break;
		}

		protected override void SetControlsInteractable(bool interactable)
		{
			base.SetControlsInteractable(interactable);
			parametersPanel.SetControlsInteractable(interactable);
		}
		#endregion

		#region private methods
		private IEnumerator DisplayHead()
		{
			var avatarObject = new GameObject(AVATAR_OBJECT_NAME);
			avatarObject.AddComponent<MoveByMouse>();

			avatarLoader = new GLTFAvatarLoader();
			avatarLoader.BodyShader = ShadersUtils.GetHeadShader(false);
			yield return avatarLoader.LoadModelOnSceneAsync(currentAvatarCode, avatarObject);

			animationManager.CreateAnimator(avatarLoader.GetBodyObject());

			DisplayInitialHaircut();
		}

		private void DisplayInitialHaircut()
		{
			List<string> haircuts = avatarLoader.GetHaircuts();
			if (haircuts.Count > 0)
			{
				avatarLoader.ShowHaircut(generatedHaircutName);
				haircutNameText.text = avatarLoader.GetCurrentHaircutName();
				haircutsControlsParent.SetActive(true);

				haircuts.Insert(0, baldHaircutName);
				haircutsSelectingView.InitItems(currentAvatarCode, haircuts, avatarProvider);
			}
			else
				haircutsControlsParent.SetActive(false);
		}

		private void ConfigureBlendshapesControls(List<string> blendshapesSets)
		{
			bool isMobileBlendshapesSetExist = blendshapesSets.Exists(s => s.Contains("mobile_51"));
			blendshapes = avatarLoader.GetBlendshapes();
			blendshapes.Insert(0, "None");
			currentBlendshapeIdx = 0;
			blendshapeNameText.text = blendshapes[currentBlendshapeIdx];
			animationControlsParent.SetActive(blendshapes.Count > 1 && isMobileBlendshapesSetExist);
			blendshapesControlsParent.SetActive(blendshapes.Count > 1 && !isMobileBlendshapesSetExist);
			blendshapesSelectingView.InitItems(blendshapes);
		}

		private IEnumerator UpdateAvatarParameters()
		{
			SetControlsInteractable(false);

			// Get all available parameters
			var allParametersRequest = avatarProvider.GetParametersAsync(ComputationParametersSubset.ALL, selectedPipelineType);
			// Get default parameters
			var defaultParametersRequest = avatarProvider.GetParametersAsync(ComputationParametersSubset.DEFAULT, selectedPipelineType);
			yield return Await(allParametersRequest, defaultParametersRequest);

			if (allParametersRequest.IsError || defaultParametersRequest.IsError)
			{
				Debug.LogError("Unable to get parameters list");
			}
			else
			{
				parametersPanel.UpdateParameters(allParametersRequest.Result, defaultParametersRequest.Result);
				SetControlsInteractable(true);
			}
		}

		private void SetBlendshape(int idx)
		{
			avatarLoader.ClearBlendshapesWeights();
			if (idx != 0)
				avatarLoader.SetBlendshapeWeight(idx - 1, 100.0f);
			blendshapeNameText.text = blendshapes[idx];
		}

		private IEnumerator DownloadFullbodyMeshAsync(MeshFormat format)
		{
			SetControlsInteractable(false);
			CloudAvatarProvider cloudAvatarProvider = avatarProvider as CloudAvatarProvider;
			var downloadingRequest = cloudAvatarProvider.DownloadFullbodyAvatarDataAsync(currentAvatarCode, format);
			yield return Await(downloadingRequest);

			if (downloadingRequest.IsError)
			{
				progressText.text = string.Format("Unable to get model in {0} format: {1}", format.MeshFormatToStr(), downloadingRequest.ErrorMessage);
			}
			else
			{
				string avatarDirectory = AvatarSdkMgr.Storage().GetAvatarDirectory(currentAvatarCode);
				progressText.text = string.Format("Model in {0} format is stored in the: {1}", format.MeshFormatToStr(), avatarDirectory);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
				System.Diagnostics.Process.Start(avatarDirectory);
#endif
			}

			SetControlsInteractable(true);
		}
		#endregion private methods
	}
}
