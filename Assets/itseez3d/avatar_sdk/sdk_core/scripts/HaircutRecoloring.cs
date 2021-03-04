/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Interface to select the haircut color
	/// </summary>
	public interface IColorPicker
	{
		Color Color { get; set; }

		void SetOnValueChangeCallback(Action<Color> onValueChange);
	}

	public class HaircutRecoloring : MonoBehaviour
	{
		public GameObject colorPickerGameObject;

		private IColorPicker colorPicker;

		private Material haircutMaterial = null;

		private Color defaultColor = Color.clear;

		private Color averageColor = Color.clear;

		public Color CurrentColor { get; private set; }

		public Vector4 CurrentTint { get; private set; }

		void Start()
		{
			if (colorPickerGameObject == null)
				Debug.LogWarning("Color picker is not set!");
			else
			{
				colorPicker = colorPickerGameObject.GetComponentInChildren<IColorPicker>();
				colorPicker.Color = LineUpColor;
				colorPicker.SetOnValueChangeCallback(OnColorChange);
			}
		}

		/// <summary>
		/// Updates material without reseting current colors
		/// </summary>
		public void UpdateHaircutMaterial(Material haircutMaterial)
		{
			this.haircutMaterial = haircutMaterial;
			UpdateMaterialParameters();
		}

		/// <summary>
		/// Resets colors and updates material
		/// </summary>
		public void ResetHaircutMaterial(Material haircutMaterial)
		{
			ResetHaircutMaterial(haircutMaterial, Color.clear);
		}

		/// <summary>
		/// Resets colors and updates material
		/// </summary>
		public void ResetHaircutMaterial(Material haircutMaterial, Color defaultColor)
		{
			ResetHaircutMaterial(haircutMaterial, haircutMaterial.mainTexture as Texture2D, defaultColor);
		}

		public void ResetHaircutMaterial(Material haircutMaterial, Texture2D texture, Color defaultColor)
		{
			this.defaultColor = defaultColor;
			this.haircutMaterial = haircutMaterial;
			if (texture != null)
				averageColor = CoreTools.CalculateAverageColor(texture);
			ResetTint();
		}

		public void ResetHaircutMaterial(Material haircutMaterial, Texture2D texture, Color defaultColor, Color currentColor)
		{
			this.defaultColor = defaultColor;
			this.haircutMaterial = haircutMaterial;
			if (texture != null)
				averageColor = CoreTools.CalculateAverageColor(texture);

			if (colorPicker != null)
				colorPicker.Color = currentColor;
			OnColorChange(currentColor);
		}

		public void ResetTint()
		{
			Color c = LineUpColor;
			if (colorPicker != null)
				colorPicker.Color = c;
			OnColorChange(c);
		}

		private Color LineUpColor { get { return defaultColor == Color.clear ? averageColor : defaultColor; } }

		private void OnColorChange (Color color)
		{
			if (haircutMaterial == null)
				return;

			CurrentColor = color;
			CurrentTint = CoreTools.CalculateTint (color, averageColor);
			UpdateMaterialParameters();
		}

		private void UpdateMaterialParameters()
		{
			if (haircutMaterial != null)
			{
				haircutMaterial.SetVector("_ColorTarget", CurrentColor);
				haircutMaterial.SetVector("_ColorTint", CurrentTint);
				haircutMaterial.SetFloat("_TintCoeff", 0.8f);
			}
		}
	}
}
