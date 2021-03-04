/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, June 2019
*/

using ItSeez3D.AvatarSdk.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class ModelInfoParametersPanel : ComputationParametersPanel
	{
		public Toggle hairColorToggle;
		public Toggle skinColorToggle;
		public Toggle genderToggle;
		public Toggle ageToggle;
		public Toggle landmarksToggle;
		public Toggle eyeScleraColorToggle;
		public Toggle eyeIrisColorToggle;
		public Toggle lipsColorToggle;
		public Toggle predictHaircutToggle;
		public Toggle raceToggle;

		private ModelInfoGroup allParameters = null;
		private ModelInfoGroup defaultParameters = null;

		public void UpdateParameters(ModelInfoGroup allParameters, ModelInfoGroup defaultParameters)
		{
			this.allParameters = allParameters;
			this.defaultParameters = defaultParameters;

			SelectDefaultParameters();
		}

		public ModelInfoGroup GetParameters()
		{
			ModelInfoGroup modelInfoParams = new ModelInfoGroup();
			modelInfoParams.hairColor = CreatePropertyAndSetValue(allParameters.hairColor, hairColorToggle, true);
			modelInfoParams.skinColor = CreatePropertyAndSetValue(allParameters.skinColor, skinColorToggle, true);
			modelInfoParams.gender = CreatePropertyAndSetValue(allParameters.gender, genderToggle, true);
			modelInfoParams.age = CreatePropertyAndSetValue(allParameters.age, ageToggle, true);
			modelInfoParams.facialLandmarks68 = CreatePropertyAndSetValue(allParameters.facialLandmarks68, landmarksToggle, true);
			modelInfoParams.eyeScleraColor = CreatePropertyAndSetValue(allParameters.eyeScleraColor, eyeScleraColorToggle, true);
			modelInfoParams.eyeIrisColor = CreatePropertyAndSetValue(allParameters.eyeIrisColor, eyeIrisColorToggle, true);
			modelInfoParams.lipsColor = CreatePropertyAndSetValue(allParameters.lipsColor, lipsColorToggle, true);
			modelInfoParams.predictHaircut = CreatePropertyAndSetValue(allParameters.predictHaircut, predictHaircutToggle, true);
			modelInfoParams.race = CreatePropertyAndSetValue(allParameters.race, raceToggle, true);
			return modelInfoParams;
		}

		public override void SelectAllParameters()
		{
			SetToggleValue(hairColorToggle, allParameters.hairColor.IsAvailable, allParameters.hairColor.IsAvailable);
			SetToggleValue(skinColorToggle, allParameters.skinColor.IsAvailable, allParameters.skinColor.IsAvailable);
			SetToggleValue(genderToggle, allParameters.gender.IsAvailable, allParameters.gender.IsAvailable);
			SetToggleValue(ageToggle, allParameters.age.IsAvailable, allParameters.age.IsAvailable);
			SetToggleValue(landmarksToggle, allParameters.facialLandmarks68.IsAvailable, allParameters.facialLandmarks68.IsAvailable);
			SetToggleValue(eyeScleraColorToggle, allParameters.eyeScleraColor.IsAvailable, allParameters.eyeScleraColor.IsAvailable);
			SetToggleValue(eyeIrisColorToggle, allParameters.eyeIrisColor.IsAvailable, allParameters.eyeIrisColor.IsAvailable);
			SetToggleValue(lipsColorToggle, allParameters.lipsColor.IsAvailable, allParameters.lipsColor.IsAvailable);
			SetToggleValue(predictHaircutToggle, allParameters.predictHaircut.IsAvailable, allParameters.predictHaircut.IsAvailable);
			SetToggleValue(raceToggle, allParameters.race.IsAvailable, allParameters.race.IsAvailable);
		}

		public override void DeselectAllParameters()
		{
			SetToggleValue(hairColorToggle, allParameters.hairColor.IsAvailable, false);
			SetToggleValue(skinColorToggle, allParameters.skinColor.IsAvailable, false);
			SetToggleValue(genderToggle, allParameters.gender.IsAvailable, false);
			SetToggleValue(ageToggle, allParameters.age.IsAvailable, false);
			SetToggleValue(landmarksToggle, allParameters.facialLandmarks68.IsAvailable, false);
			SetToggleValue(eyeScleraColorToggle, allParameters.eyeScleraColor.IsAvailable, false);
			SetToggleValue(eyeIrisColorToggle, allParameters.eyeIrisColor.IsAvailable, false);
			SetToggleValue(lipsColorToggle, allParameters.lipsColor.IsAvailable, false);
			SetToggleValue(predictHaircutToggle, allParameters.predictHaircut.IsAvailable, false);
			SetToggleValue(raceToggle, allParameters.race.IsAvailable, false);
		}

		public override void SelectDefaultParameters()
		{
			SetToggleValue(hairColorToggle, allParameters.hairColor.IsAvailable, defaultParameters.hairColor.IsAvailable);
			SetToggleValue(skinColorToggle, allParameters.skinColor.IsAvailable, defaultParameters.skinColor.IsAvailable);
			SetToggleValue(genderToggle, allParameters.gender.IsAvailable, defaultParameters.gender.IsAvailable);
			SetToggleValue(ageToggle, allParameters.age.IsAvailable, defaultParameters.age.IsAvailable);
			SetToggleValue(landmarksToggle, allParameters.facialLandmarks68.IsAvailable, defaultParameters.facialLandmarks68.IsAvailable);
			SetToggleValue(eyeScleraColorToggle, allParameters.eyeScleraColor.IsAvailable, defaultParameters.eyeScleraColor.IsAvailable);
			SetToggleValue(eyeIrisColorToggle, allParameters.eyeIrisColor.IsAvailable, defaultParameters.eyeIrisColor.IsAvailable);
			SetToggleValue(lipsColorToggle, allParameters.lipsColor.IsAvailable, defaultParameters.lipsColor.IsAvailable);
			SetToggleValue(predictHaircutToggle, allParameters.predictHaircut.IsAvailable, defaultParameters.predictHaircut.IsAvailable);
			SetToggleValue(raceToggle, allParameters.race.IsAvailable, defaultParameters.race.IsAvailable);
		}
	}
}
