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
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// Resource manager for Cloud SDK
	/// </summary>
	public class CloudComputationParametersController : ComputationParametersController
	{
		/// <summary>
		/// Converts ComputationParameters to the json format for cloud computations
		/// </summary>
		public override string GetCalculationParametersJson(ComputationParameters computationParams)
		{
			JSONObject resourcesJson = new JSONObject();

			if (computationParams != null)
			{
				if (!IsListNullOrEmpty(computationParams.blendshapes.Values))
					resourcesJson[BLENDSHAPES_KEY] = computationParams.blendshapes.ToJson();

				if (!IsListNullOrEmpty(computationParams.haircuts.Values))
					resourcesJson[HAIRCUTS_KEY] = computationParams.haircuts.ToJson();

				if (!IsListNullOrEmpty(computationParams.additionalTextures.Values))
					resourcesJson[ADDITIONAL_TEXTURES] = computationParams.additionalTextures.ToJson();

				if (!computationParams.modelInfo.IsEmpty())
					resourcesJson[MODEL_INFO] = computationParams.modelInfo.ToJson();

				if (!computationParams.avatarModifications.IsEmpty())
					resourcesJson[AVATAR_MODIFICATIONS] = computationParams.avatarModifications.ToJson();

				if (!computationParams.shapeModifications.IsEmpty())
					resourcesJson[SHAPE_MODIFICATIONS] = computationParams.shapeModifications.ToJson();
			}

			return resourcesJson.ToString(4);
		}
	}
}
