﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace ItSeez3D.AvatarSdk.Core
{
	public static class CoreTools
	{
#region Version and Platform

		/// <summary>
		/// Current version of an SDK. Used for update checks in the editor.
		/// </summary>
		public static Version CloudSdkVersion { get { return Flavour.CLOUD.GetTraits().Version; } }
		public static Version OfflineSdkVersion { get { return Flavour.OFFLINE.GetTraits().Version; } }
		public static List<Flavour> DetectFlavour()
		{
			List<Flavour> flavours = new List<Flavour>();
			if (Directory.Exists("Assets/itseez3d/avatar_sdk/sdk_offline"))
			{
				flavours.Add(Flavour.OFFLINE);
			}
			if (Directory.Exists("Assets/itseez3d/avatar_sdk/sdk_cloud"))
			{
				flavours.Add(Flavour.CLOUD);
			}
			return flavours;
		}

		public static string GetCurrentPlatform()
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			return "Windows";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			return "MacOS";
#elif UNITY_WEBGL
			return "WebGL";
#elif UNITY_ANDROID
			return "Android";
#elif UNITY_IOS
			return "IOS";
#else
			return "Unknown";
#endif
		}
#endregion

#region Save avatar files

		/// <summary>
		/// Some of the files involved in avatar generation (e.g. textures) may be large. This function helps to
		/// work around this by saving file in a separate thread, thus not blocking the main thread.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="path">Full absolute path.</param>
		public static AsyncRequest<string> SaveFileAsync (byte[] bytes, string path)
		{
			var request = new AsyncRequestThreaded<string> (() => {
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes (path, bytes);
				return path;
			});
			request.State = AvatarSdkMgr.Str (Strings.SavingFiles);
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Helper method that automatically generates full path to file from file type and avatar id, and then calls
		/// SaveFileAsync.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="code">Avatar code.</param>
		/// <param name="file">Avatar file type.</param>
		/// <param name="levelOfDetails">Level of details</param>
		public static AsyncRequest<string> SaveAvatarFileAsync (byte[] bytes, string code, AvatarFile file, int levelOfDetails = 0)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetAvatarFilename (code, file, levelOfDetails);
				return SaveFileAsync (bytes, filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<string> ("");
				request.SetError (string.Format ("Could not save {0}, reason: {1}", file, ex.Message));
				return request;
			}
		}

		public static ModelInfo GetAvatarModelInfo(string avatarCode)
		{
			try
			{
				string modelInfoFilePath = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.MODEL_JSON);
				if (File.Exists(modelInfoFilePath))
				{
					ModelInfo modelInfo = JsonUtility.FromJson<ModelInfo>(File.ReadAllText(modelInfoFilePath));
					return modelInfo;
				}
				return null;
			}
			catch(Exception exc)
			{
				Debug.LogErrorFormat("Exception during reading model info file: {0}", exc);
				return null;
			}
		}

		public static Color GetAvatarPredictedHairColor(string avatarCode)
		{
			ModelInfo modelInfo = GetAvatarModelInfo(avatarCode);
			if (modelInfo != null)
			{
				if (modelInfo.hair_color != null)
					return modelInfo.hair_color.ToUnityColor();
			}
			else
				Debug.LogError("Unable to get predicted color - model info is null");

			return Color.clear;
		}

		public static string GetAvatarPredictedHaircut(string avatarCode)
		{
			ModelInfo modelInfo = GetAvatarModelInfo(avatarCode);
			if (modelInfo != null)
			{
				if (modelInfo.haircut_name != null)
					return modelInfo.haircut_name;
			}
			else
				Debug.LogError("Unable to get predicted haircut - model info is null");
			return string.Empty;
		}

		/// <summary>
		/// Same as SaveAvatarFileAsync, but for haircut points, because they are unique for each avatar and should be stored in avatar folder.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="code">Avatar unique code.</param>
		/// <param name="haircutId">Unique ID of a haircut.</param>
		public static AsyncRequest<string> SaveAvatarHaircutPointCloudZipFileAsync (
			byte[] bytes,
			string code,
			string haircutId
		)
		{
			try {
				var filename = HaircutsPersistentStorage.Instance.GetHaircutMetadata(haircutId, code).PathToPointCloudZip;
				return SaveFileAsync (bytes, filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<string> ("Saving file");
				request.SetError (string.Format ("Could not save point cloud zip, reason: {0}", ex.Message));
				return request;
			}
		}

		public static AsyncRequest<string> SaveHaircutFileAsync(
			byte[] bytes,
			string fileName
		)
		{
			try
			{
				return SaveFileAsync(bytes, fileName);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				var request = new AsyncRequest<string>("Saving file");
				request.SetError(string.Format("Could not save {0}, reason: {1}", fileName, ex.Message));
				return request;
			}
		}

		public static void SavePipelineType(PipelineType pipelineType, string avatarCode)
		{
			try
			{
				string pipelineInfoFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.PIPELINE_INFO);
				var traits = (PipelineTypeTraits)pipelineType;
				File.WriteAllText(pipelineInfoFile, string.Format("{0}|{1}", traits.PipelineTypeName, traits.PipelineSubtypeName));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
#endregion

#region Load avatar files

		public static PipelineType LoadPipelineType(string avatarCode)
		{
			var avatarDirectory = AvatarSdkMgr.Storage().GetAvatarDirectory(avatarCode);
			string filePath = Path.Combine(avatarDirectory, AvatarSdkMgr.Storage().AvatarFilenames[AvatarFile.PIPELINE_INFO]);

			if (File.Exists(filePath))
			{
				try
				{
					string fileContent = File.ReadAllText(filePath);
					string[] contentParts = fileContent.Split('|');
					return PipelineTraitsFactory.Instance.GetTraitsFromPipelineName(contentParts[0], contentParts[1]).Type;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}

			// Also the pipeline type can be retrieved from the model.json
			ModelInfo modelInfo = GetAvatarModelInfo(avatarCode);
			if (modelInfo != null)
			{
				if (!string.IsNullOrEmpty(modelInfo.pipeline) || string.IsNullOrEmpty(modelInfo.pipeline_subtype))
				{
					PipelineTypeTraits pipelineTraits = PipelineTraitsFactory.Instance.GetTraitsFromPipelineName(modelInfo.pipeline, modelInfo.pipeline_subtype);
					if (pipelineTraits != null)
						return pipelineTraits.Type;
				}
			}

			return PipelineTraitsFactory.GetDefaultPipelineType();
		}

		/// <summary>
		/// Read text file asynchronously
		/// </summary>
		public static AsyncRequest<string> ReadFileAsync(string path)
		{
			var request = new AsyncRequestThreaded<string>(() => File.ReadAllText(path));
			AvatarSdkMgr.SpawnCoroutine(request.Await());
			return request;
		}

		/// <summary>
		/// See LoadMeshDataFromDiskAsync.
		/// </summary>
		private static IEnumerator LoadMeshDataFromDisk (string avatarId, int levelOfDetails, AsyncRequest<MeshData> request)
		{
			var meshBytesRequest = FileLoader.LoadAvatarFileAsync (avatarId, AvatarFile.MESH_PLY, levelOfDetails);
			yield return request.AwaitSubrequest (meshBytesRequest, finalProgress: 0.5f);
			if (request.IsError)
				yield break;

			var parsePlyTimer = new MeasureTime ("Parse ply");
			var parsePlyRequest = PlyToMeshDataAsync (meshBytesRequest.Result);
			yield return request.AwaitSubrequest (parsePlyRequest, finalProgress: 1);
			if (request.IsError)
				yield break;
			parsePlyTimer.Stop ();

			request.Result = parsePlyRequest.Result;
			request.IsDone = true;
		}

		/// <summary>
		/// Loads the mesh data and converts from .ply format into Unity format.
		/// </summary>
		public static AsyncRequest<MeshData> LoadMeshDataFromDiskAsync (string avatarId, int levelOfDetails)
		{
			var request = new AsyncRequest <MeshData> (AvatarSdkMgr.Str (Strings.LoadingFiles));
			AvatarSdkMgr.SpawnCoroutine (LoadMeshDataFromDisk (avatarId, levelOfDetails, request));
			return request;
		}

		/// <summary>
		/// LoadAvatarHeadFromDiskAsync implementation.
		/// </summary>
		private static IEnumerator LoadAvatarHeadFromDisk (
			string avatarId,
			bool withBlendshapes,
			int detailsLevel,
			string additionalTextureName,
			AsyncRequest<TexturedMesh> request
		)
		{
			// loading two files simultaneously
			var meshDataRequest = LoadMeshDataFromDiskAsync(avatarId, detailsLevel);
			string textureFilename = AvatarSdkMgr.Storage().GetAvatarTextureFilename(avatarId, additionalTextureName);
			var textureBytesRequest = FileLoader.LoadFileAsync(textureFilename);

			yield return request.AwaitSubrequests (0.6f, meshDataRequest, textureBytesRequest);
			if (request.IsError)
				yield break;

			MeshData meshData = meshDataRequest.Result;

			var parseTextureTimer = new MeasureTime ("Parse texture data");
			// at this point we have all data we need to generate a textured mesh
			var texturedMesh = new TexturedMesh {
				mesh = CreateMeshFromMeshData (meshData, "HeadMesh"),
				texture = new Texture2D (0, 0)
			};

			// This actually blocks the main thread for a few frames, which is bad for VR.
			// To optimize: load jpg/png texture in C++ code in a separate thread and only SetPixels here in Unity. Should be faster.
			texturedMesh.texture.LoadImage (textureBytesRequest.Result);
			parseTextureTimer.Stop ();

			if (withBlendshapes)
			{
				// adding blendshapes...
				using (new MeasureTime ("Add blendshapes")) {
					var addBlendshapesRequest = AddBlendshapesAsync (avatarId, texturedMesh.mesh, meshData.indexMap, detailsLevel);
					yield return request.AwaitSubrequest (addBlendshapesRequest, 1.0f);
					if (addBlendshapesRequest.IsError)
						Debug.LogError ("Could not add blendshapes!");
				}
			}

			request.Result = texturedMesh;
			request.IsDone = true;
		}

		private static IEnumerator LoadingAvatarTextureFromDisk(string avatarCode, string textureName, AsyncRequest<Texture2D> request)
		{
			string textureFilename = AvatarSdkMgr.Storage().GetAvatarTextureFilename(avatarCode, textureName);
			var textureBytesRequest = FileLoader.LoadFileAsync(textureFilename);

			yield return request.AwaitSubrequest(textureBytesRequest, 0.9f);
			if (request.IsError)
				yield break;

			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(textureBytesRequest.Result);

			request.Result = texture2D;
			request.IsDone = true;
		}

		/// <summary>
		/// Loads the avatar head files from disk into TexturedMesh object (parses .ply file too).
		/// </summary>
		/// <param name="avatarCode">Avatar code</param>
		/// <param name="withBlendshapes">If True, blendshapes will be loaded and added to mesh.</param>
		/// <param name="detailsLevel">Indicates polygons count in mesh. 0 - highest resolution, 3 - lowest resolution.</param>
		/// <param name="additionalTextureName">Should be set if the additional texture is required. Otherwise set to null</param>
		public static AsyncRequest<TexturedMesh> LoadAvatarHeadFromDiskAsync (string avatarCode, bool withBlendshapes, int detailsLevel, string additionalTextureName)
		{
			var request = new AsyncRequest <TexturedMesh> (AvatarSdkMgr.Str (Strings.LoadingAvatar));
			AvatarSdkMgr.SpawnCoroutine(LoadAvatarHeadFromDisk(avatarCode, withBlendshapes, detailsLevel, additionalTextureName, request));
			return request;
		}

		/// <summary>
		/// Loads avatar texture from disk
		/// </summary>
		/// <param name="avatarCode">Avatar code</param>
		/// <param name="textureName">Texture name or null for default texture</param>
		/// <returns></returns>
		public static AsyncRequest<Texture2D> LoadAvatarTextureFromDiskAsync(string avatarCode, string textureName)
		{
			var request = new AsyncRequest<Texture2D>(AvatarSdkMgr.Str(Strings.LoadingTexture));
			AvatarSdkMgr.SpawnCoroutine(LoadingAvatarTextureFromDisk(avatarCode, textureName, request));
			return request;
		}

		/// <summary>
		/// LoadHaircutFromDiskAsync implementation.
		/// </summary>
		private static IEnumerator LoadHaircutFromDiskFunc (
			string avatarCode, string haircutId, AsyncRequest<TexturedMesh> request
		)
		{
			var loadingTime = Time.realtimeSinceStartup;
			if(string.IsNullOrEmpty(HaircutsPersistentStorage.Instance.GetHaircutMetadata(haircutId, avatarCode).PathToPointCloud))
			{
				yield return LoadHaircutWithoutPointCloudFromDiskFunc(avatarCode, haircutId, request);
				yield break;
			}

			var haircutsMetadata = HaircutsPersistentStorage.Instance.GetHaircutMetadata(haircutId, avatarCode);
			// start three async request in parallel
			var haircutTexture = FileLoader.LoadFileAsync(haircutsMetadata.Texture);
			var haircutMesh = FileLoader.LoadFileAsync(haircutsMetadata.MeshPly);
			var haircutPoints = FileLoader.LoadAvatarHaircutPointcloudFileAsync(avatarCode, haircutId);

			// wait until mesh and points load
			yield return request.AwaitSubrequests (0.4f, haircutMesh, haircutPoints);
			if (request.IsError)
				yield break;

			// we can start another two subrequests, now parsing the ply files
			var parseHaircutPly = PlyToMeshDataAsync (haircutMesh.Result);
			var parseHaircutPoints = PlyToPointsAsync (haircutPoints.Result);

			// await everything else we need for the haircut
			yield return request.AwaitSubrequests (0.95f, parseHaircutPly, parseHaircutPoints, haircutTexture);
			if (request.IsError)
				yield break;

			// now we have all data we need to generate a textured mesh
			var haircutMeshData = ReplacePointCoords (parseHaircutPly.Result, parseHaircutPoints.Result);

			var texturedMesh = new TexturedMesh ();
			texturedMesh.mesh = CreateMeshFromMeshData (haircutMeshData, "HaircutMesh");
			texturedMesh.texture = new Texture2D (0, 0);
			texturedMesh.texture.LoadImage (haircutTexture.Result);

			request.Result = texturedMesh;
			request.IsDone = true;

			Debug.LogFormat ("Took {0} seconds to load a haircut", Time.realtimeSinceStartup - loadingTime);
		}

		private static IEnumerator LoadHaircutWithoutPointCloudFromDiskFunc(
			string avatarCode, string haircutId, AsyncRequest<TexturedMesh> request
		)
		{
			var loadingTime = Time.realtimeSinceStartup;

			var haircutMetadata = HaircutsPersistentStorage.Instance.GetHaircutMetadata(haircutId, avatarCode);
			var haircutTexture = FileLoader.LoadFileAsync(haircutMetadata.Texture);
			var haircutMesh = FileLoader.LoadFileAsync(haircutMetadata.MeshPly);
			yield return request.AwaitSubrequests(0.4f, haircutMesh);
			if (request.IsError)
				yield break;

			// we can start another two subrequests, now parsing the ply files
			var parseHaircutPly = PlyToMeshDataAsync(haircutMesh.Result);
			yield return request.AwaitSubrequests(0.95f, parseHaircutPly, haircutTexture);
			if (request.IsError)
				yield break;

			var texturedMesh = new TexturedMesh();
			texturedMesh.mesh = CreateMeshFromMeshData(parseHaircutPly.Result, "HaircutMesh");
			texturedMesh.texture = new Texture2D(0, 0);
			texturedMesh.texture.LoadImage(haircutTexture.Result);

			request.Result = texturedMesh;
			request.IsDone = true;

			Debug.LogFormat("Took {0} seconds to load a haircut", Time.realtimeSinceStartup - loadingTime);
		}

		/// <summary>
		/// Loads the avatar haircut files from disk into TexturedMesh object (parses .ply files too).
		/// </summary>
		/// <returns>Async request which gives complete haircut TexturedMesh object eventually.</returns>
		public static AsyncRequest<TexturedMesh> LoadHaircutFromDiskAsync (string avatarCode, string haircutId)
		{
			var request = new AsyncRequest <TexturedMesh> (AvatarSdkMgr.Str (Strings.LoadingHaircut));
			AvatarSdkMgr.SpawnCoroutine (LoadHaircutFromDiskFunc (avatarCode, haircutId, request));
			return request;
		}

#endregion

#region Delete avatar files

		/// <summary>
		/// Delete entire avatar directory.
		/// </summary>
		public static void DeleteAvatarFiles (string avatarCode)
		{
			var path = AvatarSdkMgr.Storage ().GetAvatarDirectory (avatarCode);
			Directory.Delete (path, true);
		}

		/// <summary>
		/// Delete particular avatar file by type (e.g. zip mesh file after unzip).
		/// </summary>
		public static void DeleteAvatarFile (string avatarCode, AvatarFile file)
		{
			var path = AvatarSdkMgr.Storage ().GetAvatarFilename (avatarCode, file);
			File.Delete (path);
		}

#endregion

#region Zip utils

		/// <summary>
		/// Unzips the file asynchronously.
		/// </summary>
		/// <param name="path">Absolute path to zip file.</param>
		/// <param name="location">Unzip location. If null, then files will be unzipped in the location of .zip file.</param>
		public static AsyncRequest<string> UnzipFileAsync (string path, string location = null)
		{
			if (string.IsNullOrEmpty (location))
				location = Path.GetDirectoryName (path);

			AsyncRequest<string> request = null;
			Func<string> unzipFunc = () => {
				ZipUtils.Unzip (path, location);
				File.Delete(path);
				return location;
			};

			// unzip asynchronously in a separate thread
			request = new AsyncRequestThreaded<string> (() => unzipFunc (), AvatarSdkMgr.Str (Strings.UnzippingFile));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		public static AsyncRequest<byte[]> ZipDirectoryAsync(string directoryPath)
		{
			AsyncRequestThreaded<byte[]> request = new AsyncRequestThreaded<byte[]>(() => { return ZipUtils.CreateZipArchive(directoryPath); }, AvatarSdkMgr.Str(Strings.CreatingZipArchive));
			AvatarSdkMgr.SpawnCoroutine(request.Await());
			return request;
		}

#endregion

#region Ply/mesh utils

		/// <summary>
		/// Parsing .ply data asynchronously into Unity mesh data (vertices, triangles, etc.)
		/// </summary>
		/// <param name="plyBytes">Binary content of .ply file.</param>
		public static AsyncRequest<MeshData> PlyToMeshDataAsync (byte[] plyBytes)
		{
			var request = new AsyncRequestThreaded<MeshData> (() => {
				var meshData = new MeshData ();
				PlyReader.ReadMeshDataFromPly (
					plyBytes,
					out meshData.vertices,
					out meshData.triangles,
					out meshData.uv,
					out meshData.indexMap
				);
				return meshData;
			}, AvatarSdkMgr.Str (Strings.ParsingMeshData));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Parsing .ply-encoded 3D points (e.g. "haircut point cloud").
		/// </summary>
		public static AsyncRequest<Vector3[]> PlyToPointsAsync (byte[] plyBytes)
		{
			var request = new AsyncRequestThreaded <Vector3[]> (() => {
				Vector3[] points;
				PlyReader.ReadPointCloudFromPly (plyBytes, out points);
				return points;
			}, AvatarSdkMgr.Str (Strings.ParsingPoints));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Create Unity Mesh object from MeshData. Must be called from main thread!
		/// </summary>
		/// <returns>Unity Mesh object.</returns>
		/// <param name="meshData">Data (presumably parsed from ply).</param>
		/// <param name="meshName">Name of mesh object.</param>
		public static Mesh CreateMeshFromMeshData (MeshData meshData, string meshName)
		{
			Mesh mesh = new Mesh ();
			mesh.name = meshName;
			mesh.vertices = meshData.vertices;
			mesh.triangles = meshData.triangles;
			mesh.uv = meshData.uv;
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			ImproveNormals (mesh, meshData.indexMap);
			return mesh;
		}

		/// <summary>
		/// Replace 3D point coordinates of a mesh with "coords", keeping mesh topology the same.
		/// Useful for reusing haircut meshes.
		/// </summary>
		/// <returns>Mesh data with replaced coordinates.</returns>
		/// <param name="meshData">Original mesh data.</param>
		/// <param name="coords">New 3D coordinates.</param>
		public static MeshData ReplacePointCoords (MeshData meshData, Vector3[] coords)
		{
			var vertices = new Vector3[meshData.vertices.Length];
			for (int i = 0; i < vertices.Length; ++i)
				vertices [i] = coords [meshData.indexMap [i]];
			meshData.vertices = vertices;
			return meshData;
		}

		/// <summary>
		/// Initially duplicated vertices have different normals.
		/// We have to solve it by setting average normal to avoid seams on a mesh.
		/// </summary>
		private static void ImproveNormals (Mesh mesh, int[] indexMap)
		{
			var vertices = mesh.vertices;
			var originalNormals = mesh.normals;

			Vector3[] normals = new Vector3[originalNormals.Length];
			bool[] normalSetFlag = new bool[originalNormals.Length]; 
			for (int i = 0; i < vertices.Length; i++) {
				if (indexMap [i] != i) {
					var n1 = originalNormals [i];
					var n2 = originalNormals [indexMap [i]];
					var n = (n1 + n2).normalized;
					normals [i] = n;
					normals [indexMap [i]] = n;
					normalSetFlag [i] = true;
					normalSetFlag [indexMap [i]] = true;
				} else if (!normalSetFlag [i]) {
					normals [i] = originalNormals [i];
				}
			}
			mesh.normals = normals;
		}

#endregion

#region Blendshapes

		/// <summary>
		/// Read blendshapes from the avatar directory and add them to 3D head mesh.
		/// </summary>
		private static IEnumerator AddBlendshapes (string avatarId, Mesh mesh, int[] indexMap, int levelOfDetails, AsyncRequest<Mesh> request)
		{
			var blendshapesDirs = AvatarSdkMgr.Storage ().GetAvatarBlendshapesDirs(avatarId, levelOfDetails);

			var loadBlendshapesRequest = new AsyncRequestThreaded<Dictionary<string, Vector3[]>> ((r) => {
				var timer = new MeasureTime ("Read all blendshapes");
				var blendshapes = new Dictionary<string, Vector3[]> ();
				List<string> blendshapeFiles = new List<string>();
				foreach (string dir in blendshapesDirs)
				{
					if (Directory.Exists(dir))
						blendshapeFiles.AddRange(Directory.GetFiles(dir));
				}
				var blendshapeReader = new BlendshapeReader (indexMap);

				for (int i = 0; i < blendshapeFiles.Count; ++i) {
					var blendshapePath = blendshapeFiles [i];
					var filename = Path.GetFileName (blendshapePath);

					// crude parsing of filenames
					if (!filename.EndsWith (".bin"))
						continue;
					var tokens = filename.Split (new []{ ".bin" }, StringSplitOptions.None);
					if (tokens.Length != 2)
						continue;

					var blendshapeName = tokens [0];
					blendshapes [blendshapeName] = blendshapeReader.ReadVerticesDeltas (blendshapePath);
					r.Progress = (float)i / blendshapeFiles.Count;
				}

				timer.Stop ();
				return blendshapes;
			}, AvatarSdkMgr.Str (Strings.ParsingBlendshapes));

			yield return request.AwaitSubrequest (loadBlendshapesRequest, finalProgress: 0.9f);
			if (request.IsError)
				yield break;

			var addBlendshapesTimer = DateTime.Now;
			float targetFps = 30.0f;

			int numBlendshapes = 0, loadedSinceLastPause = 0;
			var blendshapesDict = loadBlendshapesRequest.Result;
			foreach (var blendshape in blendshapesDict) {
#if UNITY_2018_3
				// Unity 2018.3 crashes when zero blendshape is used. So we don't add "base_head" as a workaround
				if (blendshape.Key != "base_head")
#endif
				{
					mesh.AddBlendShapeFrame(blendshape.Key, 100.0f, blendshape.Value, null, null);
					++numBlendshapes;
					++loadedSinceLastPause;

					if ((DateTime.Now - addBlendshapesTimer).TotalMilliseconds > 1000.0f / targetFps && loadedSinceLastPause >= 5)
					{
						// Debug.LogFormat ("Pause after {0} blendshapes to avoid blocking the main thread", numBlendshapes);
						yield return null;
						addBlendshapesTimer = DateTime.Now;
						loadedSinceLastPause = 0;
					}
				}
			}
			//It is fix for Unity 2018.3 where blendshapes are broken
			// https://issuetracker.unity3d.com/issues/blendshapes-change-their-shape-when-gpu-skinning-is-enabled
			mesh.vertices = mesh.vertices;

			request.Result = mesh;
			request.IsDone = true;
		}

		/// <summary>
		/// Read blendshapes from the avatar directory and add them to 3D head mesh.
		/// </summary>
		public static AsyncRequest<Mesh> AddBlendshapesAsync (string avatarId, Mesh mesh, int[] indexMap, int levelOfDetails)
		{
			var request = new AsyncRequest<Mesh> (AvatarSdkMgr.Str (Strings.LoadingAnimations));
			AvatarSdkMgr.SpawnCoroutine (AddBlendshapes (avatarId, mesh, indexMap, levelOfDetails, request));
			return request;
		}

		/// <summary>
		/// Detect blendshapes and their weights in the SkinnedMeshRenderer of the avatar
		/// </summary>
		public static Dictionary<string, float> GetBlendshapesWithWeights(SkinnedMeshRenderer meshRenderer, string avatarId)
		{
			var blendshapesNames = AvatarSdkMgr.Storage().GetFullBlendshapesNames(avatarId);

			Dictionary<string, float> blendshapesWithWeights = new Dictionary<string, float>();
			for(int i=0; i<meshRenderer.sharedMesh.blendShapeCount; i++)
			{
				string name = meshRenderer.sharedMesh.GetBlendShapeName(i);
				float weight = meshRenderer.GetBlendShapeWeight(i);

				string blendshapeName = blendshapesNames.FirstOrDefault(s => 
				{
					int idx = s.LastIndexOf(Path.DirectorySeparatorChar);
					if (idx > 0)
						s = s.Substring(idx + 1);
					return s == name;
				});
				if (!string.IsNullOrEmpty(blendshapeName))
					blendshapesWithWeights.Add(blendshapeName, weight);
				else
					Debug.LogErrorFormat("Blendshape {0} not found!", name);
			}
			return blendshapesWithWeights;
		}

		/// <summary>
		/// Merge blendshapes and weights into multiline string.
		/// For example: blendshape1=0.5f\nblendshape2=0.7f....
		/// </summary>
		public static string BlendshapesWithWeightsToString(Dictionary<string, float> blendshapes)
		{
			string lines = string.Empty;
			foreach(var p in blendshapes)
			{
				if (p.Value > 0.0f)
					lines += string.Format("{0}={1}\n", p.Key, p.Value / 100.0f);
			}
			return lines;
		}

#endregion Blendshapes

#region Recoloring

		/// <summary>
		/// Average color across the haircut texture. We ignore the pixels with full transparency.
		/// </summary>
		/// <returns>The average color.</returns>
		/// <param name="texture">Unity texture.</param>
		public static Color CalculateAverageColor (Texture2D texture)
		{
			var w = texture.width;
			var h = texture.height;

			var pixels = texture.GetPixels ();

			var avgChannels = new double[3];
			Array.Clear (avgChannels, 0, avgChannels.Length);

			int numNonTransparentPixels = 0;
			float minAlphaThreshold = 0.1f;
			for (int i = 0; i < h; ++i)
				for (int j = 0; j < w; ++j) {
					var idx = i * w + j;
					if (pixels [idx].a < minAlphaThreshold)
						continue;

					++numNonTransparentPixels;
					avgChannels [0] += pixels [idx].r;
					avgChannels [1] += pixels [idx].g;
					avgChannels [2] += pixels [idx].b;
				}

			for (int ch = 0; ch < 3; ++ch) {
				avgChannels [ch] /= (double)numNonTransparentPixels;
				avgChannels [ch] = Math.Max (avgChannels [ch], 0.15);
			}

			return new Color ((float)avgChannels [0], (float)avgChannels [1], (float)avgChannels [2]);
		}

		/// <summary>
		/// Calculate what tint to apply given selected color and an average color of the texture.
		/// </summary>
		/// <returns>The tint color.</returns>
		/// <param name="targetColor">Target color.</param>
		/// <param name="avgColor">Average color.</param>
		public static Vector4 CalculateTint (Color targetColor, Color avgColor)
		{
			var tint = Vector4.zero;
			tint [0] = targetColor.r - avgColor.r;
			tint [1] = targetColor.g - avgColor.g;
			tint [2] = targetColor.b - avgColor.b;
			tint [3] = 0;  // alpha does not matter
			return tint;
		}

#endregion

#region Utils

		/// <summary>
		/// Checks whether the current platform is supported.
		/// </summary>
		/// <returns>True if platform is supported</returns>
		/// <param name="errorMessage">Outpur error message in case platform is not suppported.</param>
		public static bool IsPlatformSupported(SdkType sdkType, out string errorMessage)
		{
			bool isSupported = false;
			var runtimePlatform = Application.platform;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
			isSupported = true;
#endif
#if UNITY_WEBGL
			if (sdkType == SdkType.Cloud)
				isSupported = true;
#endif

			if (!isSupported)
			{
				var msg = "Avatar generation is not supported for the current platform.\n";
				msg += "Your platform is: {0}\nList of supported platforms:\n{1}\n";
				msg += "\nPlease switch to one of the supported platforms in File -> Build Settings -> Switch platform\n";
				msg += "or use avatar generation in the Cloud (try samples from 'samples_cloud' folder).\n";
				msg += "We are planning to support offline avatar generation on more platforms in future versions,\n";
				msg += "please stay tuned and you won't miss the update!";

				var supportedPlatforms = new RuntimePlatform[] {
					RuntimePlatform.WindowsEditor,
					RuntimePlatform.WindowsPlayer,
					RuntimePlatform.Android,
					RuntimePlatform.IPhonePlayer,
					RuntimePlatform.OSXEditor,
					RuntimePlatform.OSXPlayer,
					RuntimePlatform.LinuxEditor,
					RuntimePlatform.LinuxPlayer
				};

				var listOfSupported = string.Join("\n", supportedPlatforms.Select(p => p.ToString()).ToArray());
				msg = string.Format(msg, runtimePlatform.ToString(), listOfSupported);
				errorMessage = msg;
				return false;
			}

			var bitness = IntPtr.Size * 8;
			var platformIsWindows = runtimePlatform == RuntimePlatform.WindowsEditor || runtimePlatform == RuntimePlatform.WindowsPlayer;
			if (platformIsWindows && bitness != 64)
			{
				var msg = "Avatar SDK plugin for Windows currently works only in 64-bit version.\n";
				msg += "Please try to switch to x86_64 architecture in File -> Build Settings";
				errorMessage = msg;
				return false;
			}

			// exception not thrown, everything is fine!
			Debug.LogFormat("Platform is supported!");
			errorMessage = string.Empty;
			return true;
		}
#endregion

#region haircuts naming
		/// <summary>
		/// Since SDK version 1.5.0 all haircuts ids have new format.
		/// To provide backward compatibility for avatars created by previous version of SDK, we need to distinguish them.
		/// This method allows to check if the haircut id is from the previos version or not.
		/// </summary>
		public static bool IsHaircutIdInOldFormat(string haircutId)
		{
			return haircutId.LastIndexOfAny(new char[] { '\\', '/' }) == -1;
		}

		/// <summary>
		/// Remove the prefix from the haircuts. Remain only the part after the last slash
		/// </summary>
		public static string GetShortHaircutId(string haircutId)
		{
			string[] parts = haircutId.Split(new char[] { '\\', '/' });
			return parts[parts.Length - 1];
		}
#endregion

#region Export functionality

		/// <summary>
		/// Returns the texture file name that will be saved for the given mesh file
		/// </summary>
		public static string GetOutputTextureFilename(string outputMeshFilePath)
		{
			return Path.Combine(Path.GetDirectoryName(outputMeshFilePath), Path.GetFileNameWithoutExtension(outputMeshFilePath) + ".png");
		}

		private static unsafe IntPtr CreateHaircutMeshObject(string avatarId, string haircutId, bool recolorTexture = false, Color color = new Color(), Vector4 tint = new Vector4())
		{
			var haircuitMetadata = HaircutsPersistentStorage.Instance.GetHaircutMetadata(haircutId, avatarId);
			var pointCloudPlyFile = haircuitMetadata.PathToPointCloud;
			var haircutPlyFile = haircuitMetadata.MeshPly;
			var srcHaircutTextureFile = haircuitMetadata.Texture;

			if(!File.Exists(pointCloudPlyFile))
			{
				pointCloudPlyFile = haircutPlyFile;
			}

			IMeshConverter meshConverter = CreateMeshConverter();
			IntPtr haircutMesh = IntPtr.Zero;

			if (recolorTexture)
			{
				Texture2D haircutRecoloredTexture = ImageUtils.RecolorTexture(srcHaircutTextureFile, color, tint);
				Color32[] pixels = haircutRecoloredTexture.GetPixels32();
				fixed (Color32* ptr = &pixels[0])
				{
					IntPtr imagePtr = (IntPtr)ptr;
					haircutMesh = meshConverter.CreateMeshObjectWithTexture(pointCloudPlyFile, haircutPlyFile, imagePtr, haircutRecoloredTexture.width, haircutRecoloredTexture.height);
				}
			}
			else
			{
				haircutMesh = meshConverter.CreateMeshObject(pointCloudPlyFile, haircutPlyFile, srcHaircutTextureFile);
			}

			if (haircutMesh == IntPtr.Zero)
				Debug.LogFormat("Unable to create haircut mesh object: {0}, {1}, {2}", pointCloudPlyFile, haircutPlyFile, srcHaircutTextureFile);

			return haircutMesh;
		}

		/// <summary>
		/// Saves avatar head mesh file to the OBJ or FBX format
		/// </summary>
		public static unsafe void SaveAvatarMesh(SkinnedMeshRenderer headMeshRenderer, string avatarId, string outputMeshFile, MeshFileFormat format = MeshFileFormat.OBJ,
			bool applyBlendshapeWeights = false, bool saveBlendshapes = true,
			string haircutId = null, Color color = new Color(), Vector4 tint = new Vector4(), int levelOfDetails = 0, bool recolorTexture = true)
		{
			var storage = AvatarSdkMgr.Storage();
			var plyFile = storage.GetAvatarFilename(avatarId, AvatarFile.MESH_PLY, levelOfDetails);
			var srcTextureFile = storage.GetAvatarFilename(avatarId, AvatarFile.TEXTURE);
			var dstTextureFile = GetOutputTextureFilename(outputMeshFile);
			var blendshapesDir = storage.GetAvatarBlendshapesRootDir(avatarId, levelOfDetails);

			IMeshConverter meshConverter = CreateMeshConverter();
			IntPtr headMesh = meshConverter.CreateMeshObject(plyFile, "", srcTextureFile);
			if (headMesh == IntPtr.Zero)
			{
				Debug.LogFormat("Unable to create mesh object: {0}, {1}", plyFile, srcTextureFile);
				return;
			}

			if (headMeshRenderer != null && applyBlendshapeWeights)
			{
				string blendshapesWeightsLines = BlendshapesWithWeightsToString(GetBlendshapesWithWeights(headMeshRenderer, avatarId));
				meshConverter.ApplyBlendshapesToMeshObject(headMesh, blendshapesDir, blendshapesWeightsLines);
			}

			if (saveBlendshapes)
				meshConverter.LoadBlendshapesForMeshObject(headMesh, blendshapesDir);

			if (!string.IsNullOrEmpty(haircutId))
			{
				IntPtr haircutMesh = CreateHaircutMeshObject(avatarId, haircutId, recolorTexture, color, tint);
				if (haircutMesh != IntPtr.Zero)
				{
					int mergeResult = meshConverter.MergeMeshObjects(headMesh, haircutMesh);
					if (mergeResult != 0)
						Debug.LogError("Unable to merge head mesh and texture mesh.");
					meshConverter.ReleaseMeshObject(haircutMesh);
				}
			}

			int res = 0;
			if (format == MeshFileFormat.FBX)
				res = meshConverter.SaveMeshToFbx(headMesh, outputMeshFile, dstTextureFile);
			else
				res = meshConverter.SaveMeshToObj(headMesh, outputMeshFile, dstTextureFile);
			if (res != 0)
				Debug.LogErrorFormat("Error during saving mesh to {0}. result code: {1}. output file: {2}. Texture file: {3}", format, res, outputMeshFile, dstTextureFile);

			meshConverter.ReleaseMeshObject(headMesh);
		}

		private const int IMAGE_SIZE_LIMIT = 960;
		/// <summary>
		/// Check if image needs to be downscaled and execute scaling if need
		/// </summary>
		/// <param name="srcImageBytes">Image to check</param>
		/// <returns>Image downscaled (if need, source image otherwise)</returns>
		public static IEnumerator DownscaleImageIfNeedFunc(byte[] srcImageBytes, AsyncRequest<byte[]> request)
		{
			var downscaleRequest = ImageUtils.DownscaleImageAsync(srcImageBytes, IMAGE_SIZE_LIMIT);
			yield return request.AwaitSubrequest(downscaleRequest, 0.9f);
			if(downscaleRequest.Result != null)
			{
				request.Result = downscaleRequest.Result.ToTexture2D().EncodeToJPG();
			}
			else
			{
				request.Result = srcImageBytes;
			}
			request.IsDone = true;
		}

		public static AsyncRequest<byte[]> DownscaleImageIfNeedAsync(byte [] srcImageBytes)
		{
			var request = new AsyncRequest<byte[]>("Rescaling image");
			AvatarSdkMgr.SpawnCoroutine(DownscaleImageIfNeedFunc(srcImageBytes, request));
			return request;
		}

		/// <summary>
		/// Converts current haircut mesh from ply to fbx.
		/// </summary>
		public unsafe static void HaircutPlyToObj(string avatarId, string haircutId, string objFile, 
			bool recolorTexture = false, Color color = new Color(), Vector4 tint = new Vector4())
		{
			var dstTextureFile = GetOutputTextureFilename(objFile);
			IMeshConverter meshConverter = CreateMeshConverter();
			IntPtr haircutMesh = CreateHaircutMeshObject(avatarId, haircutId, recolorTexture, color, tint);

			if (haircutMesh == IntPtr.Zero)
			{
				Debug.LogError("Haircut mesh object is NULL");
				return;
			}

			int res = meshConverter.SaveMeshToObj(haircutMesh, objFile, dstTextureFile);
			if (res != 0)
				Debug.LogErrorFormat("Error during saving haircut mesh to fbx. result code: {0}. output file: {1}. Texture file: {2}", res, objFile, dstTextureFile);

			meshConverter.ReleaseMeshObject(haircutMesh);
		}

		/// <summary>
		/// Converts current haircut mesh from ply to fbx format without saving texture.
		/// </summary>
		public unsafe static void HaircutPlyToFbx(string avatarId, string haircutId, string fbxFile, 
			bool recolorTexture = false, Color color = new Color(), Vector4 tint = new Vector4())
		{
			var dstTextureFile = GetOutputTextureFilename(fbxFile);
			IMeshConverter meshConverter = CreateMeshConverter();
			IntPtr haircutMesh = CreateHaircutMeshObject(avatarId, haircutId, recolorTexture, color, tint);

			if (haircutMesh == IntPtr.Zero)
			{
				Debug.LogError("Haircut mesh object is NULL");
				return;
			}

			int res = meshConverter.SaveMeshToFbx(haircutMesh, fbxFile, dstTextureFile);
			if (res != 0)
				Debug.LogErrorFormat("Error during saving haircut mesh to fbx. result code: {0}. output file: {1}. Texture file: {2}", res, fbxFile, dstTextureFile);

			meshConverter.ReleaseMeshObject(haircutMesh);
		}

		private static IMeshConverter CreateMeshConverter()
		{
			IMeshConverter meshConverter = AvatarSdkMgr.IoCContainer.Create<IMeshConverter>();
			if (meshConverter == null)
				Debug.LogError("Unable to create mesh converter");
			return meshConverter;
		}

#endregion
	}
}