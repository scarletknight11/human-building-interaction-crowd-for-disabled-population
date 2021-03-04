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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class CommonShapeModificationsSetter : ComputationParametersPanel, IShapeModificationsSetter
	{
		public Toggle cartoonishToggle;

		public Slider cartoonishSlider;

		private ShapeModificationsGroup allParameters;
		private ShapeModificationsGroup defaultParameters;

		public void UpdateParameters(ShapeModificationsGroup allParameters, ShapeModificationsGroup defaultParameters)
		{
			this.allParameters = allParameters;
			this.defaultParameters = defaultParameters;

			SelectDefaultParameters();
		}

		public ShapeModificationsGroup GetParameters()
		{
			ShapeModificationsGroup shapeModificationsParams = new ShapeModificationsGroup();
			shapeModificationsParams.cartoonishV03 = CreatePropertyAndSetValue(allParameters.cartoonishV03, cartoonishToggle, cartoonishSlider.value);
			return shapeModificationsParams;
		}

		public override void SelectAllParameters()
		{
			SetToggleValue(cartoonishToggle, allParameters.cartoonishV03.IsAvailable, allParameters.cartoonishV03.IsAvailable);
		}

		public override void DeselectAllParameters()
		{
			SetToggleValue(cartoonishToggle, allParameters.cartoonishV03.IsAvailable, false);
		}

		public override void SelectDefaultParameters()
		{
			SetToggleValue(cartoonishToggle, allParameters.cartoonishV03.IsAvailable, defaultParameters.cartoonishV03.IsAvailable);
		}
	}
}
