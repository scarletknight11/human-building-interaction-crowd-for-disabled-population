/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, June 2020
*/

using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
	public class FullbodyAvatarModificationsSetter : ComputationParametersPanel, IAvatarModificationsSetter
	{
		public Toggle genderToggle;
		public Toggle heightToggle;
		public Toggle weightToggle;
		public Toggle smileRemovalToggle;
		public Toggle glassesRemovalToggle;
		public Toggle enhanceLightingToggle;
		public Toggle glareToggle;
		public Toggle eyelidShadowToggle;
		public Toggle eyeIrisColorToggle;
		public Toggle eyeScleraColorToggle;
		public Toggle hairColorToggle;
		public Toggle parametricEyesToggle;
		public Toggle teethColorToggle;
		public Toggle generatedHaircutFacesNumberToggle;
		public Toggle generatedHaircutTextureSizeToggle;
		public Toggle modelTextureSizeToggle;

		public Dropdown genderDropdown;
		public InputField heightInput;
		public InputField weightInput;

		public InputField irisColorInput;
		public InputField scleraColorInput;
		public InputField hairColorInput;
		public InputField teethColorInput;

		public InputField modelTextureWidthInput;
		public InputField modelTextureHeightInput;
		public InputField generatedHaircutTextureWidthInput;
		public InputField generatedHaircutTextureHeightInput;
		public InputField generatedHaircutFacesNumberInput;


		private AvatarModificationsGroup allParameters = null;
		private AvatarModificationsGroup defaultParameters = null;

		public void UpdateParameters(AvatarModificationsGroup allParameters, AvatarModificationsGroup defaultParameters)
		{
			this.allParameters = allParameters;
			this.defaultParameters = defaultParameters;

			SelectDefaultParameters();
		}

		public AvatarModificationsGroup GetParameters()
		{
			AvatarModificationsGroup avatarModificationsParams = new AvatarModificationsGroup();
			avatarModificationsParams.addGlare = CreatePropertyAndSetValue(allParameters.addGlare, glareToggle, true);
			avatarModificationsParams.addEyelidShadow = CreatePropertyAndSetValue(allParameters.addEyelidShadow, eyelidShadowToggle, true);
			avatarModificationsParams.parametricEyesTexture = CreatePropertyAndSetValue(allParameters.parametricEyesTexture, parametricEyesToggle, true);
			avatarModificationsParams.eyeIrisColor = CreatePropertyAndSetValue(allParameters.eyeIrisColor, eyeIrisColorToggle, StringToColor(irisColorInput.text));
			avatarModificationsParams.hairColor = CreatePropertyAndSetValue(allParameters.hairColor, hairColorToggle, StringToColor(hairColorInput.text));
			avatarModificationsParams.eyeScleraColor = CreatePropertyAndSetValue(allParameters.eyeScleraColor, eyeScleraColorToggle, StringToColor(scleraColorInput.text));
			avatarModificationsParams.teethColor = CreatePropertyAndSetValue(allParameters.teethColor, teethColorToggle, StringToColor(teethColorInput.text));
			
			avatarModificationsParams.generatedHaircutFacesCount = CreatePropertyAndSetValue(allParameters.generatedHaircutFacesCount, generatedHaircutFacesNumberToggle, GetInt(generatedHaircutFacesNumberInput.text));
			avatarModificationsParams.generatedHaircutTextureSize = CreatePropertyAndSetValue(allParameters.generatedHaircutTextureSize, generatedHaircutTextureSizeToggle, GetSize(generatedHaircutTextureWidthInput.text, generatedHaircutTextureHeightInput.text));
			avatarModificationsParams.textureSize = CreatePropertyAndSetValue(allParameters.textureSize, modelTextureSizeToggle, GetSize(modelTextureWidthInput.text, modelTextureHeightInput.text));

			avatarModificationsParams.removeSmile = CreatePropertyAndSetValue(allParameters.removeSmile, smileRemovalToggle, true);
			avatarModificationsParams.removeGlasses = CreatePropertyAndSetValue(allParameters.removeGlasses, glassesRemovalToggle, true);
			avatarModificationsParams.enhanceLighting = CreatePropertyAndSetValue(allParameters.enhanceLighting, enhanceLightingToggle, true);

			avatarModificationsParams.gender = CreatePropertyAndSetValue(allParameters.gender, genderToggle, GetGender());
			avatarModificationsParams.height = CreatePropertyAndSetValue(allParameters.height, heightToggle, GetFloat(heightInput.text));
			avatarModificationsParams.weight = CreatePropertyAndSetValue(allParameters.weight, weightToggle, GetFloat(weightInput.text));

			return avatarModificationsParams;
		}

		public override void SelectAllParameters()
		{
			SetToggleValue(parametricEyesToggle, allParameters.parametricEyesTexture.IsAvailable, allParameters.parametricEyesTexture.IsAvailable);
			SetToggleValue(glareToggle, allParameters.addGlare.IsAvailable, allParameters.addGlare.IsAvailable);
			SetToggleValue(eyelidShadowToggle, allParameters.addEyelidShadow.IsAvailable, allParameters.addEyelidShadow.IsAvailable);
			SetToggleValue(eyeIrisColorToggle, allParameters.eyeIrisColor.IsAvailable, allParameters.eyeIrisColor.IsAvailable);
			SetToggleValue(eyeScleraColorToggle, allParameters.eyeScleraColor.IsAvailable, allParameters.eyeScleraColor.IsAvailable);
			SetToggleValue(hairColorToggle, allParameters.hairColor.IsAvailable, allParameters.hairColor.IsAvailable);
			SetToggleValue(teethColorToggle, allParameters.teethColor.IsAvailable, allParameters.teethColor.IsAvailable);
			SetToggleValue(modelTextureSizeToggle, allParameters.textureSize.IsAvailable, allParameters.textureSize.IsAvailable);
			SetToggleValue(generatedHaircutFacesNumberToggle, allParameters.generatedHaircutFacesCount.IsAvailable, allParameters.generatedHaircutFacesCount.IsAvailable);
			SetToggleValue(generatedHaircutTextureSizeToggle, allParameters.generatedHaircutTextureSize.IsAvailable, allParameters.generatedHaircutTextureSize.IsAvailable);
			SetToggleValue(smileRemovalToggle, allParameters.removeSmile.IsAvailable, allParameters.removeSmile.IsAvailable);
			SetToggleValue(glassesRemovalToggle, allParameters.removeGlasses.IsAvailable, allParameters.removeGlasses.IsAvailable);
			SetToggleValue(enhanceLightingToggle, allParameters.enhanceLighting.IsAvailable, allParameters.enhanceLighting.IsAvailable);
			SetToggleValue(genderToggle, allParameters.gender.IsAvailable, allParameters.gender.IsAvailable);
			SetToggleValue(heightToggle, allParameters.height.IsAvailable, allParameters.height.IsAvailable);
			SetToggleValue(weightToggle, allParameters.weight.IsAvailable, allParameters.weight.IsAvailable);
		}

		public override void DeselectAllParameters()
		{
			SetToggleValue(parametricEyesToggle, allParameters.parametricEyesTexture.IsAvailable, false);
			SetToggleValue(glareToggle, allParameters.addGlare.IsAvailable, false);
			SetToggleValue(eyelidShadowToggle, allParameters.addEyelidShadow.IsAvailable, false);
			SetToggleValue(eyeIrisColorToggle, allParameters.eyeIrisColor.IsAvailable, false);
			SetToggleValue(eyeScleraColorToggle, allParameters.eyeScleraColor.IsAvailable, false);
			SetToggleValue(hairColorToggle, allParameters.hairColor.IsAvailable, false);
			SetToggleValue(teethColorToggle, allParameters.teethColor.IsAvailable, false);
			SetToggleValue(modelTextureSizeToggle, allParameters.textureSize.IsAvailable, false);
			SetToggleValue(generatedHaircutFacesNumberToggle, allParameters.generatedHaircutFacesCount.IsAvailable, false);
			SetToggleValue(generatedHaircutTextureSizeToggle, allParameters.generatedHaircutTextureSize.IsAvailable, false);
			SetToggleValue(smileRemovalToggle, allParameters.removeSmile.IsAvailable, false);
			SetToggleValue(glassesRemovalToggle, allParameters.removeGlasses.IsAvailable, false);
			SetToggleValue(enhanceLightingToggle, allParameters.enhanceLighting.IsAvailable, false);
			SetToggleValue(genderToggle, allParameters.gender.IsAvailable, false);
			SetToggleValue(heightToggle, allParameters.height.IsAvailable, false);
			SetToggleValue(weightToggle, allParameters.weight.IsAvailable, false);
		}

		public override void SelectDefaultParameters()
		{
			SetToggleValue(eyeIrisColorToggle, allParameters.eyeIrisColor.IsAvailable, defaultParameters.eyeIrisColor.IsAvailable);
			SetToggleValue(eyeScleraColorToggle, allParameters.eyeScleraColor.IsAvailable, defaultParameters.eyeScleraColor.IsAvailable);
			SetToggleValue(hairColorToggle, allParameters.hairColor.IsAvailable, defaultParameters.hairColor.IsAvailable);
			SetToggleValue(teethColorToggle, allParameters.teethColor.IsAvailable, defaultParameters.teethColor.IsAvailable);
			SetToggleValue(modelTextureSizeToggle, allParameters.textureSize.IsAvailable, defaultParameters.textureSize.IsAvailable);
			SetToggleValue(generatedHaircutFacesNumberToggle, allParameters.generatedHaircutFacesCount.IsAvailable, defaultParameters.generatedHaircutFacesCount.IsAvailable);
			SetToggleValue(generatedHaircutTextureSizeToggle, allParameters.generatedHaircutTextureSize.IsAvailable, defaultParameters.generatedHaircutTextureSize.IsAvailable);
			SetToggleValue(genderToggle, allParameters.gender.IsAvailable, defaultParameters.gender.IsAvailable);
			SetToggleValue(heightToggle, allParameters.height.IsAvailable, defaultParameters.height.IsAvailable);
			SetToggleValue(weightToggle, allParameters.weight.IsAvailable, defaultParameters.weight.IsAvailable);

			//bool properties
			SetToggleValue(glareToggle, allParameters.addGlare.IsAvailable, defaultParameters.addGlare.Value);
			SetToggleValue(eyelidShadowToggle, allParameters.addEyelidShadow.IsAvailable, defaultParameters.addEyelidShadow.Value);
			SetToggleValue(parametricEyesToggle, allParameters.parametricEyesTexture.IsAvailable, defaultParameters.parametricEyesTexture.Value);
			SetToggleValue(smileRemovalToggle, allParameters.removeSmile.IsAvailable, defaultParameters.removeSmile.IsAvailable);
			SetToggleValue(glassesRemovalToggle, allParameters.removeGlasses.IsAvailable, defaultParameters.removeGlasses.IsAvailable);
			SetToggleValue(enhanceLightingToggle, allParameters.enhanceLighting.IsAvailable, defaultParameters.enhanceLighting.IsAvailable);
		}

		private Color StringToColor(string str)
		{
			try
			{
				string[] parts = str.Split(',');
				int red = int.Parse(parts[0]);
				int green = int.Parse(parts[1]);
				int blue = int.Parse(parts[2]);
				return new Color(red / 255.0f, green / 255.0f, blue / 255.0f);
			}
			catch
			{
				Debug.LogErrorFormat("Unable to parse color value: {0}", str);
				return Color.white;
			}
		}

		private TextureSize GetSize(string wstr, string hstr)
		{
			TextureSize result = new TextureSize();
			result.width = GetInt(wstr);
			result.height = GetInt(hstr);
			return result;
		}

		private int GetInt(string str)
		{
			int result;
			if (int.TryParse(str, out result))
			{
				return result;
			}
			else
			{
				Debug.LogErrorFormat("Unable to parse string as int: {0}", str);
				return 0;
			}
		}

		private float GetFloat(string str)
		{
			float result;
			if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			else
			{
				str = str.Replace(',', '.');
				if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				{
					return result;
				}
				else
				{
					Debug.LogErrorFormat("Unable to parse string as float: {0}", str);
					return 0;
				}
			}
		}

		private AvatarGender GetGender()
		{
			return genderDropdown.value == 0 ? AvatarGender.Male : AvatarGender.Female;
		}
	}
}
