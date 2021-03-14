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
using ItSeez3D.AvatarSdkSamples.Core;
using ItSeez3D.AvatarSdkSamples.SamplePipelineTraits;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
#if UNITY_2018_4
	public class MultiplayerSample : GettingStartedSample
    {
        public AvatarNetworkManager NetworkManager;
        protected override void DisplayHead(TexturedMesh headMesh, TexturedMesh haircutMesh)
        {
            NetworkManager.AvatarCode = currentAvatarCode;
            NetworkManager.gameObject.SetActive(true);
        }

        protected override void SetControlsInteractable(bool interactable)
        {
            if (string.IsNullOrEmpty(NetworkManager.AvatarCode))
            {
                base.SetControlsInteractable(interactable);
            }
        }

        public IEnumerator GetAvatar(string avatarCode, GameObject avatarObject)
        {
            var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(avatarCode, false);
            yield return avatarHeadRequest.Await();
            TexturedMesh headTexturedMesh = avatarHeadRequest.Result;
            if (avatarCode == currentAvatarCode &&
                avatarObject.GetComponent<RotateByMouse>() == null)
            {
                avatarObject.AddComponent<RotateByMouse>();
            }

            TexturedMesh haircutTexturedMesh = null;
            // get identities of all haircuts available for the generated avatar
            var haircutsIdRequest = avatarProvider.GetHaircutsIdAsync(avatarCode);
            yield return haircutsIdRequest.Await();

            if (haircutsIdRequest.Result != null && haircutsIdRequest.Result.Length > 0)
            {
                var generatedHaircuts = haircutsIdRequest.Result.ToList();

                // show default haircut if it exists
                var defaultHaircut = PipelineSampleTraitsFactory.Instance.GetTraitsFromAvatarCode(avatarCode).GetDefaultAvatarHaircut(avatarCode);
                int haircutIdx = generatedHaircuts.FindIndex(h => h.Contains(defaultHaircut));

                // select random haircut if default doesn't exist
                if (haircutIdx < 0)
                    haircutIdx = UnityEngine.Random.Range(0, generatedHaircuts.Count);
                string haircutId = generatedHaircuts[haircutIdx];

                // load TexturedMesh for the chosen haircut 
                var haircutRequest = avatarProvider.GetHaircutMeshAsync(avatarCode, haircutId);
                yield return (haircutRequest.Await());
                haircutTexturedMesh = haircutRequest.Result;

                if (haircutTexturedMesh != null)
                {
                    // create haircut object in the scene
                    var haircutObject = new GameObject("Haircut");
                    var haircutMeshRenderer = haircutObject.AddComponent<SkinnedMeshRenderer>();
                    haircutMeshRenderer.sharedMesh = haircutTexturedMesh.mesh;
                    var haircutMaterial = ShadersUtils.ConfigureHaircutMaterial(null, haircutId, false);
                    haircutMaterial.mainTexture = haircutTexturedMesh.texture;
                    haircutMeshRenderer.material = haircutMaterial;
                    haircutObject.transform.SetParent(avatarObject.transform);
                    haircutObject.transform.localPosition = new Vector3(0, 0, 0);
                    haircutObject.transform.localScale = new Vector3(4, 4, 4);
                }
            }

            var headObject = new GameObject("Head");
            var headMeshRenderer = headObject.AddComponent<SkinnedMeshRenderer>();
            headMeshRenderer.sharedMesh = headTexturedMesh.mesh;
            var headMaterial = new Material(ShadersUtils.GetHeadShader(false));
            headMaterial.mainTexture = headTexturedMesh.texture;
            headMeshRenderer.material = headMaterial;

            headObject.transform.SetParent(avatarObject.transform);
            headObject.transform.localPosition = new Vector3(0, 0, 0);
            headObject.transform.localScale = new Vector3(4, 4, 4);
        }
    }
#else
	public class MultiplayerSample : GettingStartedSample
	{
		public GameObject NetworkManager;

		protected override IEnumerator CheckAvailablePipelines()
		{
			SetControlsInteractable(false);
			progressText.text = "The sample works only in the UNITY 2018.4! See scene documentation to get details.";
			yield break;
		}
	}
#endif
}