/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, May 2019
*/

using System.Collections;
using System.Linq;
using ItSeez3D.AvatarSdk.Core;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using ItSeez3D.AvatarSdkSamples.Core;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
	public class CartoonishAvatarSample : GettingStartedSample
	{
		public Text cartoonishValueText;

		public Text currentHaircutNameText;

		public Text currentTextureNameText;

		public GameObject haircutControls;

		public HaircutsSelectingView haircutsSelectingView;

		public ItemsSelectingView texturesSelectingView;

		private float cartoonishValue = 0.5f;

		private int currentHaircutIndex = 0;
		private const string BALD_HAIRCUT_NAME = "bald";

		private List<string> availableTextures = new List<string>();
		private int currentTextureIndex = 0;
		private const string STANDARD_TEXTURE_NAME = "Standard";

		public CartoonishAvatarSample()
		{
			//To generate cartoonish avatars STYLED_FACE pipeline is used
			selectedPipelineType = PipelineType.STYLED_FACE;
		}

		#region base overrided methods
		protected override IEnumerator CheckAvailablePipelines()
		{
			// Cartoonish avatars are available starting from the INDIE plan. Need to verify it.
			SetControlsInteractable(false);
			var cartoonishPipelineAvailabilityRequest = avatarProvider.IsPipelineSupportedAsync(selectedPipelineType);
			yield return Await(cartoonishPipelineAvailabilityRequest);
			if (cartoonishPipelineAvailabilityRequest.IsError)
				yield break;

			if (cartoonishPipelineAvailabilityRequest.Result == true)
			{
				progressText.text = "Cartoonish avatars are available.";
				SetControlsInteractable(true);
			}
			else
			{
				string errorMsg = "You can't generate cartoonish avatars.\nThis option is available starting from the INDIE plan.";
				progressText.text = errorMsg;
				progressText.color = Color.red;
				Debug.LogError(errorMsg);
			}
		}

		protected override IEnumerator ConfigureComputationParameters(PipelineType pipelineType, ComputationParameters computationParameters)
		{
			var parametersRequest = avatarProvider.GetParametersAsync(ComputationParametersSubset.ALL, pipelineType);
			yield return Await(parametersRequest);
			if (parametersRequest.IsError)
				yield break;

			computationParameters.haircuts = parametersRequest.Result.haircuts;
			computationParameters.additionalTextures = parametersRequest.Result.additionalTextures;
			computationParameters.avatarModifications = parametersRequest.Result.avatarModifications;
			computationParameters.shapeModifications = parametersRequest.Result.shapeModifications;

			//cartoonishV03 paramater specifies the cartoonish level
			computationParameters.shapeModifications.cartoonishV03.Value = cartoonishValue;

			// Allow to make thin neck
			computationParameters.avatarModifications.allowModifyNeck.Value = true;
		}

		protected override IEnumerator GenerateAndDisplayHead(byte[] photoBytes, PipelineType pipeline)
		{
			haircutControls.SetActive(false);
			ComputationParameters computationParameters = ComputationParameters.Empty;
			yield return ConfigureComputationParameters(pipeline, computationParameters);

			// generate avatar from the photo and get its code in the Result of request
			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, computationParameters);
			yield return Await(initializeRequest);
			currentAvatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(currentAvatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(currentAvatarCode);
			yield return Await(calculateRequest);

			// get identities of all haircuts available for the generated avatar
			var haircutsIdRequest = avatarProvider.GetHaircutsIdAsync(currentAvatarCode);
			yield return Await(haircutsIdRequest);

			availableHaircuts = haircutsIdRequest.Result.ToList();
			availableHaircuts.Insert(0, BALD_HAIRCUT_NAME);
			currentHaircutId = BALD_HAIRCUT_NAME;
			currentHaircutIndex = availableHaircuts.IndexOf(currentHaircutId);
			currentHaircutNameText.text = BALD_HAIRCUT_NAME;
			haircutsSelectingView.InitItems(currentAvatarCode, availableHaircuts, avatarProvider);

			UpdateAvailableTextures(computationParameters.additionalTextures);
			currentTextureIndex = availableTextures.Count - 1;
			currentTextureNameText.text = availableTextures[currentTextureIndex];
			texturesSelectingView.InitItems(availableTextures);

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(currentAvatarCode, false, 0, MeshFormat.PLY, availableTextures[currentTextureIndex]);
			yield return Await(avatarHeadRequest);
			TexturedMesh headTexturedMesh = avatarHeadRequest.Result;

			DisplayHead(headTexturedMesh, null);
			haircutControls.SetActive(true);
		}
		#endregion

		#region UI handling
		public void OnCartoonishSliderChanged(float val)
		{
			cartoonishValue = val;
		}

		public void OnNextHaircutClick()
		{
			currentHaircutIndex = currentHaircutIndex == availableHaircuts.Count - 1 ? 0 : currentHaircutIndex + 1;
			StartCoroutine(ChangeHaircut());
		}

		public void OnPrevHaircutClick()
		{
			currentHaircutIndex = currentHaircutIndex == 0 ? availableHaircuts.Count - 1 : currentHaircutIndex - 1;
			StartCoroutine(ChangeHaircut());
		}

		public void OnNextTextureClick()
		{
			currentTextureIndex = currentTextureIndex == availableTextures.Count - 1 ? 0 : currentTextureIndex + 1;
			StartCoroutine(ChangeTexture());
		}

		public void OnPrevTextureClick()
		{
			currentTextureIndex = currentTextureIndex == 0 ? availableTextures.Count - 1 : currentTextureIndex - 1;
			StartCoroutine(ChangeTexture());
		}

		public void OnHaircutListButtonClick()
		{
			SetControlsInteractable(false);
			haircutsSelectingView.Show(new List<string>() { availableHaircuts[currentHaircutIndex] }, list =>
			{
				// Find index of the selected haircut.
				currentHaircutIndex = availableHaircuts.IndexOf(list[0]);
				StartCoroutine(ChangeHaircut());
			});
		}

		public void OnTexturesListButtonClick()
		{
			SetControlsInteractable(false);
			texturesSelectingView.Show(new List<string>() { availableTextures[currentTextureIndex] }, list => 
			{
				currentTextureIndex = availableTextures.IndexOf(list[0]);
				StartCoroutine(ChangeTexture());
			});
		}
		#endregion UI handling

		#region private methods
		private IEnumerator ChangeHaircut()
		{
			SetControlsInteractable(false);

			currentHaircutId = availableHaircuts[currentHaircutIndex];

			if (currentHaircutId == BALD_HAIRCUT_NAME)
			{
				currentHaircutNameText.text = currentHaircutId;
				UpdateHaircut(null);
			}
			else
			{
				ComputationListValue haircutProperty = new ComputationListValue(currentHaircutId);
				currentHaircutNameText.text = haircutProperty.Name;
				var haircutRequest = avatarProvider.GetHaircutMeshAsync(currentAvatarCode, currentHaircutId);
				yield return Await(haircutRequest);
				UpdateHaircut(haircutRequest.Result);
			}

			SetControlsInteractable(true);
		}

		private IEnumerator ChangeTexture()
		{
			SetControlsInteractable(false);
			string currentTexture = availableTextures[currentTextureIndex];
			currentTextureNameText.text = currentTexture;

			var textureRequest = avatarProvider.GetTextureAsync(currentAvatarCode, currentTexture == STANDARD_TEXTURE_NAME ? null : currentTexture);
			yield return Await(textureRequest);


			var head = GameObject.Find(HEAD_OBJECT_NAME);
			SkinnedMeshRenderer meshRenderer = head.GetComponent<SkinnedMeshRenderer>();
			meshRenderer.material.mainTexture = textureRequest.Result;

			SetControlsInteractable(true);
		}

		private void UpdateAvailableTextures(ComputationList texturesList)
		{
			availableTextures.Clear();
			availableTextures.Add(STANDARD_TEXTURE_NAME);
			texturesList.Values.ForEach(t => 
			{
				if (t.Name.Contains("cartoonish"))
					availableTextures.Add(t.Name);
			});
		}
		#endregion
	}
}
