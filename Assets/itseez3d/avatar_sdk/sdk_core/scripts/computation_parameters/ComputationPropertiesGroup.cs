﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, May 2019
*/

using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Base class that stores group of the computation properties
	/// </summary>
	public class ComputationPropertiesGroup
	{
		public virtual void FromJson(JSONNode node)
		{
			foreach (JSONNode group in node.Keys)
			{
				JSONNode availableProperties = node[group.Value];
				foreach (JSONNode property in availableProperties)
				{
					string propertyName = property.Value;
					Properties.ForEach(p =>
					{
						if (p.Name == propertyName)
						{
							p.GroupName = group.Value;
							p.IsAvailable = true;
						}
					});
				}
			}
		}

		public virtual List<ComputationProperty> Properties
		{
			get { return new List<ComputationProperty>(); }
		}


		public virtual JSONNode ToJson(bool useGroupName = true)
		{
			JSONNode node = new JSONObject();
			foreach(ComputationProperty property in Properties)
			{
				if (property != null && property.HasValue)
					property.AddToJsonNode(node, useGroupName);
			}
			return node;
		}

		public virtual bool IsEmpty()
		{
			foreach(ComputationProperty property in Properties)
			{
				if (property.HasValue)
					return false;
			}
			return true;
		}

		public void SetPropertiesToUnavailableState()
		{
			foreach (ComputationProperty property in Properties)
				property.IsAvailable = false;
		}
	}

	/// <summary>
	/// Avatar Modification group
	/// </summary>
	public class AvatarModificationsGroup : ComputationPropertiesGroup
	{
		public ComputationProperty<bool> curvedBottom = new ComputationProperty<bool>("plus", "curved_bottom");
		public ComputationProperty<bool> addGlare = new ComputationProperty<bool>("plus", "add_glare");
		public ComputationProperty<bool> addEyelidShadow = new ComputationProperty<bool>("plus", "add_eyelid_shadow");
		public ComputationProperty<Color> eyeScleraColor = new ComputationProperty<Color>("plus", "eye_sclera_color");
		public ComputationProperty<Color> eyeIrisColor = new ComputationProperty<Color>("plus", "eye_iris_color");
		public ComputationProperty<Color> hairColor = new ComputationProperty<Color>("plus", "hair_color");
		public ComputationProperty<bool> parametricEyesTexture = new ComputationProperty<bool>("plus", "parametric_eyes_texture");
		public ComputationProperty<bool> allowModifyNeck = new ComputationProperty<bool>("indie", "allow_modify_neck");
		public ComputationProperty<bool> allowModifyVertices = new ComputationProperty<bool>("plus", "allow_modify_vertices");
		public ComputationProperty<Color> lipsColor = new ComputationProperty<Color>("plus", "lips_color");
		public ComputationProperty<Color> teethColor = new ComputationProperty<Color>("plus", "teeth_color");
		public ComputationProperty<float> caricatureAmount = new ComputationProperty<float>("plus", "caricature_amount");
		public ComputationProperty<bool> slightlyCartoonishTexture = new ComputationProperty<bool>("plus", "slightly_cartoonish_texture");

		public ComputationProperty<TextureSize> textureSize = new ComputationProperty<TextureSize>("plus", "texture_size");
		public ComputationProperty<TextureSize> generatedHaircutTextureSize = new ComputationProperty<TextureSize>("plus", "generated_haircut_texture_size");
		public ComputationProperty<int> generatedHaircutFacesCount = new ComputationProperty<int>("plus", "generated_haircut_faces_count");

		public ComputationProperty<bool> removeSmile = new ComputationProperty<bool>("plus", "remove_smile");
		public ComputationProperty<bool> enhanceLighting = new ComputationProperty<bool>("plus", "enhance_lighting");
		public ComputationProperty<bool> removeGlasses = new ComputationProperty<bool>("plus", "remove_glasses");

		public ComputationProperty<AvatarGender> gender = new ComputationProperty<AvatarGender>("plus", "gender");
		public ComputationProperty<float> height = new ComputationProperty<float>("plus", "height");
		public ComputationProperty<float> weight = new ComputationProperty<float>("plus", "weight");

		private void FromDefaultParams(JSONNode availableProperties, string groupName)
		{
			foreach (string key in availableProperties.Keys)
			{
				var matchedProperty = Properties.FirstOrDefault(p => p.Name.Equals(key));
				if (matchedProperty != null)
				{
					matchedProperty.GroupName = groupName;
					matchedProperty.IsAvailable = true;
					if (matchedProperty is ComputationProperty<bool> && availableProperties[key].IsBoolean)
					{
						(matchedProperty as ComputationProperty<bool>).Value = availableProperties[key].AsBool;
					}

				}
			}
		}

		private void FromAllParams(JSONArray array, string groupName)
		{
			foreach (var element in array)
			{
				string propertyName = element.Value;
				Properties.ForEach(p =>
				{
					if (p.Name == propertyName)
					{
						p.GroupName = groupName;
						p.IsAvailable = true;
					}
				});
			}
		}

		public override void FromJson(JSONNode node)
		{
			foreach (JSONNode group in node.Keys)
			{
				if (node[group.Value].IsArray) //'All' parameters stored in array whereas 'Default' is an object that contains key-values
				{
					var arrayOfProperties = node[group.Value].AsArray;
					FromAllParams(arrayOfProperties, group.Value);
				}
				else
				{
					FromDefaultParams(node[group.Value], group.Value);
				}
			}
		}

		public AvatarModificationsGroup() { }

		public AvatarModificationsGroup(JSONNode node) : this()
		{
			FromJson(node);
		}

		public override List<ComputationProperty> Properties
		{
			get
			{
				List<ComputationProperty> list = new List<ComputationProperty>()
				{
					curvedBottom,
					addGlare,
					addEyelidShadow,
					eyeIrisColor,
					eyeScleraColor,
					parametricEyesTexture,
					hairColor,
					allowModifyNeck,
					allowModifyVertices,
					lipsColor,
					teethColor,
					caricatureAmount,
					slightlyCartoonishTexture,
					textureSize,
					generatedHaircutTextureSize,
					generatedHaircutFacesCount,
					removeSmile,
					enhanceLighting,
					removeGlasses,
					gender,
					weight,
					height

				};
				return list.Where(p => p != null).ToList();
			}
		}
	}

	/// <summary>
	/// Model Info group
	/// </summary>
	public class ModelInfoGroup : ComputationPropertiesGroup
	{
		public ComputationProperty<bool> hairColor = new ComputationProperty<bool>("plus", "hair_color");
		public ComputationProperty<bool> skinColor = new ComputationProperty<bool>("plus", "skin_color");
		public ComputationProperty<bool> gender = new ComputationProperty<bool>("plus", "gender");
		public ComputationProperty<bool> age = new ComputationProperty<bool>("plus", "age");
		public ComputationProperty<bool> facialLandmarks68 = new ComputationProperty<bool>("plus", "facial_landmarks_68");
		public ComputationProperty<bool> eyeScleraColor = new ComputationProperty<bool>("plus", "eye_sclera_color");
		public ComputationProperty<bool> eyeIrisColor = new ComputationProperty<bool>("plus", "eye_iris_color");
		public ComputationProperty<bool> predictHaircut = new ComputationProperty<bool>("plus", "predict_haircut");
		public ComputationProperty<bool> lipsColor = new ComputationProperty<bool>("plus", "lips_color");
		public ComputationProperty<bool> race = new ComputationProperty<bool>("plus", "race");

		public ModelInfoGroup() { }

		public ModelInfoGroup(JSONNode node) : this()
		{
			FromJson(node);
		}

		public override List<ComputationProperty> Properties
		{
			get
			{
				List<ComputationProperty> list = new List<ComputationProperty>()
				{
					hairColor,
					skinColor,
					gender,
					age,
					facialLandmarks68,
					eyeScleraColor,
					eyeIrisColor,
					predictHaircut,
					lipsColor,
					race
				};
				return list.Where(p => p != null).ToList();
			}
		}

		public override JSONNode ToJson(bool useGroupName = true)
		{
			JSONNode node = null;
			if (useGroupName)
				node = new JSONObject();
			else
				node = new JSONArray();

			foreach (ComputationProperty property in Properties)
			{
				if (property != null && property.HasValue)
					property.AddToJsonNodeAsListItem(node, useGroupName);
			}
			return node;
		}

		public void SetAll(bool value)
		{
			foreach (ComputationProperty<bool> property in Properties)
			{
				property.Value = value;
			}
		}
	}

	public class ShapeModificationsGroup : ComputationPropertiesGroup
	{
		public ComputationProperty<float> cartoonishV03 = new ComputationProperty<float>("indie", "cartoonish_v0.3");

		public ShapeModificationsGroup() { }

		public ShapeModificationsGroup(JSONNode node) : this()
		{
			FromJson(node);
		}

		public override List<ComputationProperty> Properties
		{
			get
			{
				List<ComputationProperty> list = new List<ComputationProperty>();
				if (cartoonishV03 != null)
					list.Add(cartoonishV03);
				return list;
			}
		}
	}
}
