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
using System.Runtime.InteropServices;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	public enum MeshFileFormat
	{
		OBJ,
		FBX
	}

	public interface IMeshConverter
	{
		bool IsObjConvertEnabled { get; }

		bool IsFBXExportEnabled { get; }

		IntPtr CreateMeshObject(string plyModelFile, string templateModelFile, string textureFile);

		IntPtr CreateMeshObjectWithTexture(string plyModelFile, string templateModelFile, IntPtr textureImage, int textureWidth, int textureHeight);

		int ReleaseMeshObject(IntPtr mesh);

		int MergeMeshObjects(IntPtr dstMesh, IntPtr srcMesh);

		int ApplyBlendshapesToMeshObject(IntPtr mesh, string blendshapesDir, string blendshapesNamesWithWeights);

		int LoadBlendshapesForMeshObject(IntPtr mesh, string blendshapesDir);

		int SaveMeshToObj(IntPtr mesh, string objModelFile, string textureFile);

		int SaveMeshToFbx(IntPtr mesh, string fbxModelFile, string textureFile);
	}

	public class CoreMeshConverter : IMeshConverter
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
		[DllImport(DllHelperCore.dll)]
		private static extern IntPtr createMeshObject(string plyModelFile, string templateModelFile, string textureFile);

		[DllImport(DllHelperCore.dll)]
		private static extern IntPtr createMeshObjectWithTexture(string plyModelFile, string templateModelFile, IntPtr textureImage, int textureWidth, int textureHeight);

		[DllImport(DllHelperCore.dll)]
		private static extern int releaseMeshObject(IntPtr mesh);

		[DllImport(DllHelperCore.dll)]
		private static extern int saveMeshToObj(IntPtr mesh, string objModelFile, string textureFile);

		[DllImport(DllHelperCore.dll)]
		private static extern int mergeMeshObjects(IntPtr dstMesh, IntPtr srcMesh);

		[DllImport(DllHelperCore.dll)]
		private static extern int applyBlendshapesToMeshObject(IntPtr mesh, string blendshapesDir, string blendshapesNamesWithWeights);

		[DllImport(DllHelperCore.dll)]
		private static extern int loadBlendshapesForMeshObject(IntPtr mesh, string blendshapesDir);

		public virtual IntPtr CreateMeshObject(string plyModelFile, string templateModelFile, string textureFile)
		{
			return createMeshObject(plyModelFile, templateModelFile, textureFile);
		}

		public virtual IntPtr CreateMeshObjectWithTexture(string plyModelFile, string templateModelFile, IntPtr textureImage, int textureWidth, int textureHeight)
		{
			return createMeshObjectWithTexture(plyModelFile, templateModelFile, textureImage, textureWidth, textureHeight);
		}

		public virtual int ReleaseMeshObject(IntPtr mesh)
		{
			return releaseMeshObject(mesh);
		}

		public virtual int SaveMeshToObj(IntPtr mesh, string objModelFile, string textureFile)
		{
			return saveMeshToObj(mesh, objModelFile, textureFile);
		}

		public virtual int MergeMeshObjects(IntPtr dstMesh, IntPtr srcMesh)
		{
			return mergeMeshObjects(dstMesh, srcMesh);
		}

		public virtual int ApplyBlendshapesToMeshObject(IntPtr mesh, string blendshapesDir, string blendshapesNamesWithWeights)
		{
			return applyBlendshapesToMeshObject(mesh, blendshapesDir, blendshapesNamesWithWeights);
		}

		public virtual int LoadBlendshapesForMeshObject(IntPtr mesh, string blendshapesDir)
		{
			return loadBlendshapesForMeshObject(mesh, blendshapesDir);
		}

		public bool IsObjConvertEnabled { get { return true; } }
#else
		public virtual IntPtr CreateMeshObject(string plyModelFile, string templateModelFile, string textureFile)
		{
			Debug.LogError("Method not implemented!");
			return IntPtr.Zero;
		}

		public virtual IntPtr CreateMeshObjectWithTexture(string plyModelFile, string templateModelFile, IntPtr textureImage, int textureWidth, int textureHeight)
		{
			Debug.LogError("Method not implemented!");
			return IntPtr.Zero;
		}

		public virtual int ReleaseMeshObject(IntPtr mesh)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual int SaveMeshToObj(IntPtr mesh, string objModelFile, string textureFile)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual int MergeMeshObjects(IntPtr dstMesh, IntPtr srcMesh)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual int ApplyBlendshapesToMeshObject(IntPtr mesh, string blendshapesDir, string blendshapesNamesWithWeights)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual int LoadBlendshapesForMeshObject(IntPtr mesh, string blendshapesDir)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual bool IsObjConvertEnabled { get { return false; } }
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || (UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX)
		[DllImport(DllHelperCore.dll)]
		private static extern int saveMeshToFbx(IntPtr mesh, string fbxModelFile, string textureFile);

		public virtual int SaveMeshToFbx(IntPtr mesh, string fbxModelFile, string textureFile)
		{
			textureFile = textureFile.Replace('\\', '/');
			return saveMeshToFbx(mesh, fbxModelFile, textureFile);
		}

		public virtual bool IsFBXExportEnabled { get { return true; } }
#else
		public virtual int SaveMeshToFbx(IntPtr mesh, string fbxModelFile, string textureFile)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual bool IsFBXExportEnabled { get { return false; } }
#endif
	}
}
