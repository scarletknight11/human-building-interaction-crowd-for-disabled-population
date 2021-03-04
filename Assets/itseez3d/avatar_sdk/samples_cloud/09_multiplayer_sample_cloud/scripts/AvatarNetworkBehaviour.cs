/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

#if UNITY_2018_4
using ItSeez3D.AvatarSdk.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
#pragma warning disable 0618
    public class AvatarNetworkBehaviour : NetworkBehaviour
    {
        public MultiplayerSample Handler;

        [SyncVar]
        public string AvatarCode;
        [SyncVar]
        public Quaternion Rotation;
        [SyncVar]
        public Vector3 Position;

        public override void OnStartClient()
        {
            Handler = FindObjectOfType<MultiplayerSample>();
            gameObject.transform.position = Position;
            gameObject.transform.rotation = Rotation;
            AvatarSdkMgr.SpawnCoroutine(Handler.GetAvatar(AvatarCode, gameObject));
        }
    }
#pragma warning restore 0618
}
#endif