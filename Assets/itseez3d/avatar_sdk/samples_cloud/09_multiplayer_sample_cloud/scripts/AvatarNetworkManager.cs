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
using UnityEngine;
using UnityEngine.Networking;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
#pragma warning disable 0618
	public class NetworkMessage : MessageBase
    {
        public string avatarCode;
    }

    public class AvatarNetworkManager : NetworkManager
    {
        public string AvatarCode { get; set; }
        private int currentStartPosition = -1;
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            NetworkMessage message = extraMessageReader.ReadMessage<NetworkMessage>();
            string avatarCode = message.avatarCode;
            Transform startPos;
            if (++currentStartPosition < startPositions.Count)
            {
                startPos = startPositions[currentStartPosition];
            }
            else
            {
                return;
            }

            var avatarObject = Instantiate(base.playerPrefab);
            avatarObject.GetComponent<AvatarNetworkBehaviour>().AvatarCode = avatarCode;
            avatarObject.GetComponent<AvatarNetworkBehaviour>().Rotation = startPos.rotation;
            avatarObject.GetComponent<AvatarNetworkBehaviour>().Position = startPos.position;

            NetworkServer.AddPlayerForConnection(conn, avatarObject, playerControllerId);
        }
        public override void OnClientConnect(NetworkConnection conn)
        {
            NetworkMessage msg = new NetworkMessage();
            msg.avatarCode = AvatarCode;
            ClientScene.AddPlayer(conn, 0, msg);
        }
    }
#pragma warning restore 0618
}
#endif