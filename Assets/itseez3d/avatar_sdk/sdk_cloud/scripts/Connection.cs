﻿/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdk.Core.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;


namespace ItSeez3D.AvatarSdk.Cloud
{
	public class DownloadedFileInfo
	{
		public byte[] bytes;
		public string fileName;
	}

	public class Connection : ConnectionBase
	{
		private CloudComputationParametersController parametersController = new CloudComputationParametersController();

		#region Connection data

		// access data
		private string tokenType = null, accessToken = null;

		/// <summary>
		/// Unique identifier of a player. Player only have access to those avatar files he or she created.
		/// PlayerUID is used to sign the majority of requests, without the appropriate PlayerUID the access will be
		/// forbidden.
		/// </summary>
		private string playerUID = null;

		#endregion

		#region Helpers

		/// <summary>
		/// Urlencoded param string.
		/// </summary>
		public virtual string UrlWithParams(string url, Dictionary<string, string> param)
		{
			if (param == null || param.Count == 0)
				return url;

			var paramTokens = new List<string>();
			foreach (var item in param)
			{
#if UNITY_2018_3_OR_NEWER
				var token = string.Format("{0}={1}", UnityWebRequest.EscapeURL(item.Key), UnityWebRequest.EscapeURL(item.Value));
#else
				var token = string.Format ("{0}={1}", WWW.EscapeURL (item.Key), WWW.EscapeURL (item.Value));
#endif
				paramTokens.Add(token);
			}

			return string.Format("{0}?{1}", url, string.Join("&", paramTokens.ToArray()));
		}

		/// <summary>
		/// Simple overload for a single-parameter use case.
		/// </summary>
		public virtual string UrlWithParams(string url, string param, string value)
		{
			return UrlWithParams(url, new Dictionary<string, string> { { param, value } });
		}

		/// <returns>Dictionary with required auth HTTP headers.</returns>
		public virtual Dictionary<string, string> GetAuthHeaders()
		{
			var headers = new Dictionary<string, string>() {
				{ "Authorization", string.Format ("{0} {1}", tokenType, accessToken) },
#if !UNITY_WEBGL
				{ "X-Unity-Plugin-Platform", CoreTools.GetCurrentPlatform() },
				{ "X-Unity-Plugin-Version", CoreTools.CloudSdkVersion.ToString () },
#endif
			};
			if (!string.IsNullOrEmpty(playerUID))
				headers.Add("X-PlayerUID", playerUID);
			return headers;
		}

		/// <summary>
		/// Adds auth header to UnityWebRequest.
		/// </summary>
		protected void SetAuthHeaders(UnityWebRequest request)
		{
			var headers = GetAuthHeaders();
			foreach (var h in headers)
				request.SetRequestHeader(h.Key, h.Value);
		}

		/// <summary>
		/// Helper factory method.
		/// </summary>
		/// <returns>Constructed UnityWebRequest object that does HTTP GET request for the given url.</returns>
		protected UnityWebRequest HttpGet(string url)
		{
			if (string.IsNullOrEmpty(url))
				Debug.LogError("Provided empty url!");
			var r = UnityWebRequest.Get(url);
			SetAuthHeaders(r);
			return r;
		}
#endregion

#region Generic request processing

		private static void PrintWebRequestInfo(UnityWebRequest webRequest, double requestDurationInSec)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}: {1}", webRequest.method, webRequest.url).AppendLine();
			if (webRequest.uploadHandler != null)
			{
				sb.AppendFormat("Content type: {0}", webRequest.uploadHandler.contentType).AppendLine();
				sb.AppendFormat("Uploaded body: {0}", Encoding.UTF8.GetString(webRequest.uploadHandler.data)).AppendLine();
			}
			sb.AppendFormat("Uploaded bytes: {0}", webRequest.uploadedBytes).AppendLine();
			sb.AppendFormat("Response code: {0}", webRequest.responseCode).AppendLine();
			if (webRequest.downloadHandler != null && webRequest.downloadHandler.text.Length < 5000)
			{
				sb.AppendFormat("Response body: {0}", webRequest.downloadHandler.text).AppendLine();
			}
			sb.AppendFormat("Downloaded bytes: {0}", webRequest.downloadedBytes).AppendLine();
			sb.AppendFormat("Request duration: {0}", requestDurationInSec);
			Debug.LogFormat("{0}", sb.ToString());
		}

		/// <summary>
		/// Helper routine that waits until the request finishes and updates progress for the request object.
		/// </summary>
		private static IEnumerator AwaitAndTrackProgress<T>(UnityWebRequest webRequest, AsyncWebRequest<T> request)
		{
			DateTime sendRequestTime = DateTime.Now;
			webRequest.SendWebRequest();
			do
			{
				yield return null;

				switch (request.ProgressTracking)
				{
					case TrackProgress.DOWNLOAD:
						request.Progress = webRequest.downloadProgress;
						break;
					case TrackProgress.UPLOAD:
						request.Progress = webRequest.uploadProgress;
						break;
				}

				request.BytesDownloaded = webRequest.downloadedBytes;
				request.BytesUploaded = webRequest.uploadedBytes;
			} while (!webRequest.isDone);

			PrintWebRequestInfo(webRequest, (DateTime.Now - sendRequestTime).TotalSeconds);
		}

		/// <summary>
		/// Basic validations of UnityWebRequest response.
		/// </summary>
		/// <returns><c>true</c>, if response is good, <c>false</c> otherwise.</returns>
		private static bool IsGoodResponse(UnityWebRequest webRequest, out StatusCode status, out string error)
		{
			error = string.Empty;

			try
			{
				status = new StatusCode(webRequest.responseCode);

				if (webRequest.isNetworkError)
				{  // apparently the API has changed in 2017
					error = webRequest.error;
					return false;
				}

				if (status.IsBad)
				{
					error = string.Format("Bad response code. Msg: {0}", webRequest.downloadHandler.text);
					return false;
				}

				if (!webRequest.downloadHandler.isDone)
				{
					error = "Could not download response";
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				status = new StatusCode();
				error = string.Format("Exception while checking response: {0}", ex.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Send the request, wait till it finishes and process the result.
		/// </summary>
		/// <returns>The web request func.</returns>
		/// <param name="webRequestFactory">Function that returns the new copy of UnityWebRequest
		/// (needed for retry).</param>
		/// <param name="request">"Parent" async request object to report progress to.</param>
		/// <param name="parseDataFunc">Function that takes care of parsing data (usually JSON parsing).</param>
		private IEnumerator AwaitWebRequestFunc<T>(
			Func<UnityWebRequest> webRequestFactory,
			AsyncWebRequest<T> request,
			Func<UnityWebRequest, T> parseDataFunc
		)
		{
			UnityWebRequest webRequest = null;

			StatusCode status = new StatusCode();
			string error = string.Empty;

			int numAttempts = 2, lastAttempt = numAttempts - 1;
			bool goodResponse = false;
			for (int attempt = 0; attempt < numAttempts; ++attempt)
			{
				webRequest = webRequestFactory();
				yield return AwaitAndTrackProgress(webRequest, request);

				if (goodResponse = IsGoodResponse(webRequest, out status, out error))
					break;

				if(status.Value == (long)StatusCode.Code.TOO_MANY_REQUESTS_THROTTLING)
				{
					var responseHeaders = webRequest.GetResponseHeaders();
					const string retryKey = "Retry-After";
					if (responseHeaders.ContainsKey(retryKey))
					{
						var retryAfterStr = responseHeaders[retryKey];
						int retryAfter;
						int.TryParse(retryAfterStr, out retryAfter);
						request.RetryPeriod = retryAfter;
					}
				}

				// all API requests have Authorization header, except for authorization requests
				bool isAuthRequest = webRequest.GetRequestHeader("Authorization") == null;
				Debug.LogWarningFormat("Server error: {0}, request: {1}", error, webRequest.url);
				if (status.Value != (long)StatusCode.Code.UNAUTHORIZED || isAuthRequest)
				{
					// cannot recover, request has failed
					break;
				}

				if (attempt == lastAttempt)
				{
					Debug.LogError("No more retries left");
					break;
				}

				Debug.LogWarning("Auth issue, let's try one more time after refreshing access token");
				yield return AuthorizeAsync();
			}

			if (!goodResponse)
			{
				Debug.LogErrorFormat("Could not send the request, status: {0}, error: {1}", status, error);
				request.Status = status;
				request.SetError(error);
				yield break;
			}

			T data = default(T);
			try
			{
				data = parseDataFunc(webRequest);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			if (data == null)
			{
				request.SetError("Could not parse request data");
				yield break;
			}
			else
			{
				request.Result = data;
			}

			request.IsDone = true;
		}

		/// <summary>
		/// There is only one way to check if the pipeline is supported.
		/// Need to request available parameters for this pipeline. In case the good response (200 OK) the pipeline is considered as available.
		/// We don't want to log errors from this this request, so we implemented this method to track request instead of AwaitWebRequestFunc
		/// </summary>
		/// <param name="url"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		private IEnumerator CheckIfPipelineSupportedFunc(string url, AsyncRequest<bool> request)
		{
			DateTime sendRequestTime = DateTime.Now;
			UnityWebRequest webRequest = HttpGet(url);
			yield return webRequest.SendWebRequest();

			while (!webRequest.isDone)
				yield return null;

			PrintWebRequestInfo(webRequest, (DateTime.Now - sendRequestTime).TotalSeconds);

			if (!webRequest.isDone)
				Debug.LogErrorFormat("Request isn't completed");

			if (webRequest.isNetworkError)
			{
				Debug.LogErrorFormat("Could not send the request, status: {0}, error: {1}", webRequest.responseCode, webRequest.error);
				request.SetError(webRequest.downloadHandler.text);
				yield break;
			}

			if (webRequest.isHttpError)
			{
				if (webRequest.responseCode == 400)
				{
					request.Result = false;
					request.IsDone = true;
				}
				else
				{
					Debug.LogErrorFormat("Got error in response, status: {0}, error: {1}", webRequest.responseCode, webRequest.downloadHandler.text);
					request.SetError(webRequest.downloadHandler.text);
					yield break;
				}
			}
			else
			{
				request.Result = true;
				request.IsDone = true;

			}
		}

		/// <summary>
		/// Variation of AwaitWebRequestFunc when we don't actually need the result.
		/// </summary>
		public virtual IEnumerator AwaitWebRequest(Func<UnityWebRequest> webRequestFactory, AsyncWebRequest request)
		{
			yield return AwaitWebRequestFunc(webRequestFactory, request, (r) => new object());
		}

		/// <summary>
		/// Call AwaitWebRequestFunc, interpret response as JSON.
		/// </summary>
		public virtual IEnumerator AwaitJsonWebRequest<DataType>(
			Func<UnityWebRequest> webRequestFactory,
			AsyncWebRequest<DataType> request)
		{
			yield return AwaitWebRequestFunc(webRequestFactory, request, (r) =>
			{
				return JsonUtility.FromJson<DataType>(r.downloadHandler.text);
			});
		}

		/// <summary>
		/// Call AwaitWebRequestFunc for paginated requests.
		/// </summary>
		public virtual IEnumerator AwaitJsonPageWebRequest<T>(
			Func<UnityWebRequest> webRequestFactory,
			AsyncWebRequest<Page<T>> request
		)
		{
			yield return AwaitWebRequestFunc(webRequestFactory, request, (r) =>
			{
				// Unity JsonUtility does not support Json array parsing, so we have to hack around it
				// by wrapping it into object with a single array field.
				var wrappedArrayJson = string.Format("{{ \"content\": {0} }}", r.downloadHandler.text);
				var page = JsonUtility.FromJson<Page<T>>(wrappedArrayJson);
				var paginationHeader = r.GetResponseHeader("Link");

				// parse "Link" header to get links to adjacent pages
				if (!string.IsNullOrEmpty(paginationHeader))
				{
					var regex = new Regex(@".*\<(?<link>.+)\>.+rel=""(?<kind>.*)""");
					var tokens = paginationHeader.Split(',');
					foreach (var token in tokens)
					{
						var match = regex.Match(token);
						if (!match.Success)
							continue;

						string link = match.Groups["link"].Value, kind = match.Groups["kind"].Value;
						if (string.IsNullOrEmpty(link) || string.IsNullOrEmpty(kind))
							continue;

						if (kind == "first")
							page.firstPageUrl = link;
						else if (kind == "next")
							page.nextPageUrl = link;
						else if (kind == "prev")
							page.prevPageUrl = link;
						else if (kind == "last")
							page.lastPageUrl = link;
					}
				}
				return page;
			});
		}

		/// <summary>
		/// Call AwaitWebRequestFunc for binary data.
		/// </summary>
		public virtual IEnumerator AwaitDataAsync(Func<UnityWebRequest> webRequestFactory, AsyncWebRequest<byte[]> request)
		{
			yield return AwaitWebRequestFunc(webRequestFactory, request, (r) => r.downloadHandler.data);
		}

		/// <summary>
		/// Call AwaitWebRequestFunc for string data.
		/// </summary>
		public virtual IEnumerator AwaitStringDataAsync(Func<UnityWebRequest> webRequestFactory, AsyncWebRequest<string> request)
		{
			yield return AwaitWebRequestFunc(webRequestFactory, request, (r) => r.downloadHandler.text);
		}

		/// <summary>
		/// Call AwaitWebRequestFunc for downloading file data and its name.
		/// </summary>
		public virtual IEnumerator AwaitFileDataAsync(Func<UnityWebRequest> webRequestFactory, AsyncWebRequest<DownloadedFileInfo> request)
		{
			yield return AwaitWebRequestFunc(webRequestFactory, request, (r) =>
			{
				DownloadedFileInfo fileInfo = new DownloadedFileInfo();
				fileInfo.bytes = r.downloadHandler.data;
				var headers = r.GetResponseHeaders();
				string contentDispositionHeader = "Content-Disposition";
				string filenameTag = "filename";
				if (headers.ContainsKey(contentDispositionHeader))
				{
					string contentDispositionValue = headers[contentDispositionHeader];
					int filenamePos = contentDispositionValue.IndexOf(filenameTag);
					if (filenamePos >= 0)
					{
						fileInfo.fileName = contentDispositionValue.Substring(filenamePos + filenameTag.Length + 1);
					}
					else
						Debug.LogErrorFormat("Filename isn't found in the Content-Disposition header: {0}", contentDispositionValue);
				}
				return fileInfo;
			});
		}

		/// <summary>
		/// Send HTTP GET request, deserialize response as DataType.
		/// </summary>
		/// <returns>Async request object that will contain an instance of DataType on success.</returns>
		public virtual AsyncWebRequest<DataType> AvatarJsonRequest<DataType>(string url)
		{
			var request = new AsyncWebRequest<DataType>();
			AvatarSdkMgr.SpawnCoroutine(AwaitJsonWebRequest(() => HttpGet(url), request));
			return request;
		}

		/// <summary>
		/// Send HTTP GET request, deserialize response as Page<T>.
		/// </summary>
		/// <returns>Async request object that will contain an instance of Page<T> on success.</returns>
		public virtual AsyncWebRequest<Page<T>> AvatarJsonPageRequest<T>(string url)
		{
			var request = new AsyncWebRequest<Page<T>>();
			AvatarSdkMgr.SpawnCoroutine(AwaitJsonPageWebRequest(() => HttpGet(url), request));
			return request;
		}

		/// <summary>
		/// Same as the other version, but provide base url + page number instead of absolute url.
		/// </summary>
		/// <returns>Async request object that will contain an instance of Page<T> on success.</returns>
		public virtual AsyncWebRequest<Page<T>> AvatarJsonPageRequest<T>(string baseUrl, int pageNumber)
		{
			return AvatarJsonPageRequest<T>(string.Format("{0}?page={1}", baseUrl, pageNumber));
		}

		/// <summary>
		/// Loop until the desired number of pages is downloaded.
		/// </summary>
		public virtual IEnumerator AwaitMultiplePages<T>(string url, AsyncRequest<T[]> request, int maxItems = int.MaxValue)
		{
			List<T> items = new List<T>();
			do
			{
				var pageRequest = AvatarJsonPageRequest<T>(url);
				yield return pageRequest;
				if (pageRequest.IsError)
				{
					request.SetError(string.Format("Page request failed. Error: {0}", pageRequest.ErrorMessage));
					yield break;
				}

				// Debug.LogFormat ("Successfully loaded page {0}", url);
				var page = pageRequest.Result;

				for (int i = 0; i < page.content.Length && items.Count < maxItems; ++i)
					items.Add(page.content[i]);

				url = page.nextPageUrl;
			} while (items.Count < maxItems && !string.IsNullOrEmpty(url));

			request.Result = items.ToArray();
			request.IsDone = true;
		}

		/// <summary>
		/// Download pages until the desired number of items is requested.
		/// </summary>
		public virtual AsyncWebRequest<DataType[]> AvatarJsonArrayRequest<DataType>(string url, int maxItems = int.MaxValue)
		{
			var request = new AsyncWebRequest<DataType[]>();
			AvatarSdkMgr.SpawnCoroutine(AwaitMultiplePages(url, request, maxItems));
			return request;
		}

		/// <summary>
		/// Download file.
		/// </summary>
		public virtual AsyncWebRequest<byte[]> AvatarDataRequestAsync(string url)
		{
			var request = new AsyncWebRequest<byte[]>();
			AvatarSdkMgr.SpawnCoroutine(AwaitDataAsync(() => HttpGet(url), request));
			return request;
		}

		/// <summary>
		/// Download file and get its name.
		/// </summary>
		public virtual AsyncWebRequest<DownloadedFileInfo> FileDataRequestAsync(string url)
		{
			var request = new AsyncWebRequest<DownloadedFileInfo>();
			AvatarSdkMgr.SpawnCoroutine(AwaitFileDataAsync(() => HttpGet(url), request));
			return request;
		}

		/// <summary>
		/// Request to get model info json
		/// </summary>
		public virtual AsyncWebRequest<string> GetModelInfoAsync(string avatarCode)
		{
			string url = GetUrl("avatars", avatarCode, "model_info");
			var request = new AsyncWebRequest<string>(Strings.RequestingModelInfo);
			AvatarSdkMgr.SpawnCoroutine(AwaitStringDataAsync(() => HttpGet(url), request));
			return request;
		}

#endregion

#region Auth functions

		/// <summary>
		/// <c>true</c> if this session is authorized; otherwise, <c>false</c>.
		/// </summary>
		public virtual bool IsAuthorized { get { return !string.IsNullOrEmpty(accessToken); } }

		public virtual string TokenType { get { return tokenType; } }

		public virtual string AccessToken { get { return accessToken; } }

		/// <summary>
		/// Player unique ID in a current session.
		/// </summary>
		public virtual string PlayerUID
		{
			get { return playerUID; }
			set { playerUID = value; }
		}

		/// <summary>
		/// AuthorizeAsync implementation.
		/// </summary>
		private IEnumerator Authorize(AsyncRequest request)
		{
			var accessCredentials = AuthUtils.LoadCredentials();
			if (accessCredentials == null || string.IsNullOrEmpty(accessCredentials.clientSecret))
			{
				request.SetError("Could not find API keys! Please provide valid credentials via Window->ItSeez3D Avatar SDK");
				yield break;
			}

			var authRequest = AuthorizeClientCredentialsGrantTypeAsync(accessCredentials);
			yield return request.AwaitSubrequest(authRequest, 0.5f);
			if (request.IsError)
				yield break;

			tokenType = authRequest.Result.token_type;
			accessToken = authRequest.Result.access_token;
			Debug.LogFormat("Successful authentication!");

			// guarantees we re-register a Player if clientId changes
			var playerIdentifier = string.Format("player_uid_{0}", accessCredentials.clientId.Substring(0, accessCredentials.clientId.Length / 3));

			if (string.IsNullOrEmpty(playerUID))
				playerUID = AvatarSdkMgr.Storage().LoadPlayerUID(playerIdentifier);

			if (string.IsNullOrEmpty(playerUID))
			{
				Debug.Log("Registering new player UID");
				var playerRequest = RegisterPlayerAsync();
				yield return request.AwaitSubrequest(playerRequest, 1);
				if (request.IsError)
					yield break;

				playerUID = playerRequest.Result.code;
				AvatarSdkMgr.Storage().StorePlayerUID(playerIdentifier, playerUID);
			}

			request.IsDone = true;
		}

		/// <summary>
		/// Authorize this session using the credentials loaded from encrypted binary resource.
		/// </summary>
		public virtual AsyncRequest AuthorizeAsync()
		{
			var request = new AsyncRequest(AvatarSdkMgr.Str(Strings.Authentication));
			AvatarSdkMgr.SpawnCoroutine(Authorize(request));
			return request;
		}

		/// <summary>
		/// Provide your own credentials to authorize the session.
		/// This method is useful if you don't want to store SecretID on the client. You can obtain access token
		/// on your own server instead, send this to your app and use for API calls.
		/// </summary>
		public virtual void AuthorizeWithCredentials(string tokenType, string accessToken, string playerUID)
		{
			this.tokenType = tokenType;
			this.accessToken = accessToken;
			this.playerUID = playerUID;
		}

		/// <summary>
		/// Obtain token using client credentials.
		/// </summary>
		private AsyncWebRequest<AccessData> AuthorizeClientCredentialsGrantTypeAsync(AccessCredentials credentials)
		{
			var request = new AsyncWebRequest<AccessData>(AvatarSdkMgr.Str(Strings.RequestingApiToken));
			Func<UnityWebRequest> webRequestFactory = () => GenerateAuthRequest(credentials);
			AvatarSdkMgr.SpawnCoroutine(AwaitJsonWebRequest(webRequestFactory, request));
			return request;
		}

		/// <summary>
		/// Obtain token using itSeez3D username and password. Not for production use!
		/// </summary>
		private AsyncWebRequest<AccessData> AuthorizePasswordGrantTypeAsync(
			string clientId,
			string clientSecret,
			string username,
			string password
		)
		{
			Debug.LogWarning("Don't use this auth method in production, use other grant types!");
			var request = new AsyncWebRequest<AccessData>(AvatarSdkMgr.Str(Strings.RequestingApiToken));

			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(clientId))
			{
				request.SetError("itSeez3D credentials not provided");
				Debug.LogError(request.ErrorMessage);
				return request;
			}

			var form = new Dictionary<string, string>() {
				{ "grant_type", "password" },
				{ "username", username },
				{ "password", password },
				{ "client_id", clientId },
				{ "client_secret", clientSecret },
			};
			Func<UnityWebRequest> webRequestFactory = () => HttpPost(GetUrl("o", "token"), form);
			AvatarSdkMgr.SpawnCoroutine(AwaitJsonWebRequest(webRequestFactory, request));
			return request;
		}

		/// <summary>
		/// Register unique player UID that is used later to sign the requests.
		/// </summary>
		/// <param name="comment">Arbitrary data associated with player UID.</param>
		public virtual AsyncWebRequest<Player> RegisterPlayerAsync(string comment = "")
		{
			var r = new AsyncWebRequest<Player>(AvatarSdkMgr.Str(Strings.RegisteringPlayerID));
			var form = new Dictionary<string, string>() {
				{ "comment", comment },
			};
			Func<UnityWebRequest> webRequestFactory = () =>
			{
				var webRequest = HttpPost(GetUrl("players"), form);
				SetAuthHeaders(webRequest);
				return webRequest;
			};
			AvatarSdkMgr.SpawnCoroutine(AwaitJsonWebRequest(webRequestFactory, r));
			return r;
		}

#endregion

#region Creating/awaiting/downloading an avatar

		/// <summary>
		/// Upload photo and create avatar instance on the server. Calculations will start right away after the photo is uploaded.
		/// </summary>
		public virtual AsyncWebRequest<AvatarData> CreateAvatarWithPhotoAsync(
			string name, string description, byte[] photoBytes, bool forcePowerOfTwoTexture = false,
			PipelineType pipeline = PipelineType.FACE, ComputationParameters computationParameters = null
		)
		{
			var request = new AsyncWebRequest<AvatarData>(AvatarSdkMgr.Str(Strings.UploadingPhoto), TrackProgress.UPLOAD);

			var traits = pipeline.Traits();
			var textFields = new Dictionary<string, string>
			{
				{"name", name },
				{"pipeline", traits.PipelineTypeName},
				{"pipeline_subtype", traits.PipelineSubtypeName}
			};

			if (!string.IsNullOrEmpty(description))
				textFields.Add("description", description);

			if (computationParameters != null)
			{
				string parametersJson = parametersController.GetCalculationParametersJson(computationParameters);
				Debug.LogFormat("Computation parameters json: {0}", parametersJson);
				textFields.Add("parameters", parametersJson);
			}

			Func<UnityWebRequest> webRequestFactory = () =>
			{
				List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
				formData.Add(new MultipartFormFileSection("photo", photoBytes, "photo.jpg", "application/octet-stream"));
				foreach (var item in textFields)
					formData.Add(new MultipartFormDataSection(item.Key, item.Value));

				var webRequest = UnityWebRequest.Post(GetUrl("avatars"), formData);
#if UNITY_2017 || UNITY_2018
				webRequest.chunkedTransfer = false;
#endif
				SetAuthHeaders(webRequest);
				return webRequest;
			};

			Debug.LogFormat("Uploading photo...");
			AvatarSdkMgr.SpawnCoroutine(AwaitJsonWebRequest(webRequestFactory, request));
			return request;
		}

		/// <summary>
		/// Get avatar information by code.
		/// </summary>
		public virtual AsyncWebRequest<AvatarData> GetAvatarAsync(string avatarCode)
		{
			var r = AvatarJsonRequest<AvatarData>(GetUrl("avatars", avatarCode));
			r.State = AvatarSdkMgr.Str(Strings.GettingAvatarInfo);
			return r;
		}

		/// <summary>
		/// Get list of all haircuts for avatar.   
		/// </summary>
		public virtual AsyncWebRequest<AvatarHaircutData[]> GetHaircutsAsync(AvatarData avatar)
		{
			var r = AvatarJsonArrayRequest<AvatarHaircutData>(avatar.haircuts);
			r.State = AvatarSdkMgr.Str(Strings.RequestingHaircutInfo);
			return r;
		}

		/// <summary>
		/// Get list of all textures for avatar.
		/// </summary>
		public virtual AsyncWebRequest<TextureData[]> GetTexturesAsync(AvatarData avatar)
		{
			var r = AvatarJsonArrayRequest<TextureData>(GetUrl("avatars", avatar.code, "textures"));
			r.State = AvatarSdkMgr.Str(Strings.RequestingTextureInfo);
			return r;
		}

		/// <summary>
		/// Download thumbnail with the specified resolution.
		/// </summary>
		/// <param name="avatar">Avatar data</param>
		/// <returns></returns>
		public virtual AsyncWebRequest<byte[]> DownloadAvatarThumbnailBytesAsync(AvatarData avatar, int maxW, int maxH)
		{
			var param = new Dictionary<string, string> {
				{ "max_w", maxW.ToString () },
				{ "max_h", maxH.ToString () },
			};
			var url = UrlWithParams(avatar.thumbnail, param);

			var r = AvatarDataRequestAsync(url);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingThumbnail);
			return r;
		}

		/// <summary>
		/// Download mesh zip file into memory.
		/// </summary>
		/// <param name="levelOfDetails">Level of mesh details. 0 - highest resolution, 7 - lowest resolution</param>
		/// <returns></returns>
		public virtual AsyncWebRequest<byte[]> DownloadMeshZipAsync(AvatarData avatar, int levelOfDetails = 0, MeshFormat format = MeshFormat.PLY)
		{
			var paramsDictionary = new Dictionary<string, string>()
			{
				{ "lod", levelOfDetails.ToString() },
				{ "fmt", format.MeshFormatToStr() }
			};
			var url = UrlWithParams(avatar.mesh, paramsDictionary);
			var r = AvatarDataRequestAsync(url);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHeadMesh);
			return r;
		}

		/// <summary>
		/// Downloading coordinates of the vertices of the head model. This can be used to save download time, because faces and UV are always the same.
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadPointCloudZipAsync(AvatarData avatar)
		{
			var r = AvatarDataRequestAsync(GetUrl("avatars", avatar.code, "pointcloud"));
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHeadMesh);
			return r;
		}

		/// <summary>
		/// Download main texture into memory. Can be used right away to create Unity texture.
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadTextureBytesAsync(AvatarData avatar)
		{
			var r = AvatarDataRequestAsync(avatar.texture);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHeadTexture);
			return r;
		}

		public virtual AsyncWebRequest<DownloadedFileInfo> DownloadAdditionalTextureBytesAsync(AvatarData avatar, string textureName)
		{
			var r = FileDataRequestAsync(GetUrl("avatars", avatar.code, "textures", textureName, "file"));
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHeadTexture);
			return r;
		}

		/// <summary>
		/// Downloads haircut zip file into memory.
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadHaircutMeshZipAsync(AvatarHaircutData haircut)
		{
			var r = AvatarDataRequestAsync(haircut.mesh);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHaircutMesh);
			return r;
		}

		/// <summary>
		/// Download haircut texture into memory. Can be used right away to create Unity texture.
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadHaircutTextureBytesAsync(AvatarHaircutData haircut)
		{
			var r = AvatarDataRequestAsync(haircut.texture);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHaircutTexture);
			return r;
		}

		/// <summary>
		/// Download haircut preview into memory. 
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadHaircutPreviewBytesAsync(AvatarHaircutData haircut)
		{
			var r = AvatarDataRequestAsync(haircut.preview);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHaircutPreview);
			return r;
		}

		/// <summary>
		/// Downloads the haircut point cloud zip into memory.
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadHaircutPointCloudZipAsync(AvatarHaircutData haircut)
		{
			var r = AvatarDataRequestAsync(haircut.pointcloud);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingHaircutPointCloud);
			return r;
		}

		/// <summary>
		/// Downloads zip archive with point clouds for all haircuts. It is recommended to use this request
		/// for less overall download time (instead of downloading all individual haircuts separately).
		/// </summary>
		public virtual AsyncWebRequest<byte[]> DownloadAllHaircutPointCloudsZipAsync(AvatarData avatar)
		{
			string url = string.Format("{0}pointclouds/", avatar.haircuts);
			var r = AvatarDataRequestAsync(url);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingAllHaircutPointClouds);
			return r;
		}

		/// <summary>
		/// Downloads zip archive with all the blendshapes.
		/// </summary>
		/// <param name="format">Format of blendshapes inside the zip file. Use BIN if you don't know which one to choose.</param>
		/// <param name="levelOfDetails">Level of mesh details. 0 - highest resolution, 7 - lowest resolution</param>
		public virtual AsyncWebRequest<byte[]> DownloadBlendshapesZipAsync(AvatarData avatar, MeshFormat format = MeshFormat.BIN, int levelOfDetails = 0)
		{
			string url = string.Format("{0}?fmt={1}&lod={2}", avatar.blendshapes, format.MeshFormatToStr(), levelOfDetails);
			var r = AvatarDataRequestAsync(url);
			r.State = AvatarSdkMgr.Str(Strings.DownloadingBlendshapes);
			return r;
		}

		public virtual AsyncWebRequest<byte[]> DownloadUmaBonesDataAsync(AvatarData avatar)
		{
			var r = AvatarDataRequestAsync(GetUrl("avatars", avatar.code, "bones"));
			r.State = AvatarSdkMgr.Str(Strings.DownloadingUmaBones);
			return r;
		}

#endregion

#region Actions with avatars on the server (list/update/delete/...)

		/// <summary>
		/// Get a particular page in the list of avatars.
		/// </summary>
		public virtual AsyncWebRequest<Page<AvatarData>> GetAvatarsPageAsync(int pageNumber)
		{
			var r = AvatarJsonPageRequest<AvatarData>(GetUrl("avatars"), pageNumber);
			r.State = AvatarSdkMgr.Str(Strings.GettingAvatarList);
			return r;
		}

		/// <summary>
		/// Get "maxItems" latest avatars (will loop through the pages).
		/// </summary>
		public virtual AsyncRequest<AvatarData[]> GetAvatarsAsync(int maxItems = int.MaxValue, Dictionary<string, string> filters = null)
		{
			var url = GetUrl("avatars");
			url = UrlWithParams(url, filters);
			var r = AvatarJsonArrayRequest<AvatarData>(url, maxItems);
			r.State = AvatarSdkMgr.Str(Strings.GettingAvatarList);
			return r;
		}

		/// <summary>
		/// Edit avatar name/description on the server.
		/// </summary>
		public virtual AsyncWebRequest EditAvatarAsync(AvatarData avatar, string name = null, string description = null)
		{
			var request = new AsyncWebRequest(AvatarSdkMgr.Str(Strings.EditingAvatar));

			byte[] requestBodyData = null;
			using (var requestBody = new MultipartBody())
			{
				requestBody.WriteTextField("name", name);
				requestBody.WriteTextField("description", description);
				requestBody.WriteFooter();
				requestBodyData = requestBody.GetRequestBodyData();

				Func<UnityWebRequest> webRequestFactory = () =>
				{
					var webRequest = UnityWebRequest.Post(avatar.url, " ");
#if UNITY_2017 || UNITY_2018
					webRequest.chunkedTransfer = false;
#endif
					webRequest.method = "PATCH";
					webRequest.uploadHandler = new UploadHandlerRaw(requestBodyData);
					webRequest.SetRequestHeader(
						"Content-Type", string.Format("multipart/form-data; boundary=\"{0}\"", requestBody.Boundary)
					);
					SetAuthHeaders(webRequest);
					return webRequest;
				};

				Debug.LogFormat("Uploading photo...");
				AvatarSdkMgr.SpawnCoroutine(AwaitJsonWebRequest(webRequestFactory, request));
				return request;
			}
		}

		/// <summary>
		/// Delete avatar record on the server (does not delete local files).
		/// </summary>
		public virtual AsyncWebRequest DeleteAvatarAsync(AvatarData avatar)
		{
			var request = new AsyncWebRequest(AvatarSdkMgr.Str(Strings.DeletingAvatarOnServer));

			Func<UnityWebRequest> webRequestFactory = () =>
			{
				var webRequest = UnityWebRequest.Delete(avatar.url);
				SetAuthHeaders(webRequest);
				webRequest.downloadHandler = new DownloadHandlerBuffer();
				return webRequest;
			};
			AvatarSdkMgr.SpawnCoroutine(AwaitWebRequest(webRequestFactory, request));
			return request;
		}

#endregion

#region Parameters

		/// <summary>
		/// Request to get available parameters for the pipeline
		/// </summary>
		public virtual AsyncWebRequest<string> GetParametersAsync(PipelineType pipelineType, ComputationParametersSubset parametersSubset)
		{
			string subsetStr = "available";
			if (parametersSubset == ComputationParametersSubset.DEFAULT)
				subsetStr = "default";

			var tratis = pipelineType.Traits();
			var url = GetUrl("parameters", subsetStr, tratis.PipelineTypeName);
			url = UrlWithParams(url, "pipeline_subtype", tratis.PipelineSubtypeName);
			var request = new AsyncWebRequest<string>(Strings.GettingParametersList);
			AvatarSdkMgr.SpawnCoroutine(AwaitStringDataAsync(() => HttpGet(url), request));
			return request;
		}

		public virtual AsyncRequest<bool> CheckIfPipelineSupportedAsync(PipelineType pipelineType)
		{
			var tratis = pipelineType.Traits();
			string url = GetUrl("parameters", "available", tratis.PipelineTypeName);
			url = UrlWithParams(url, "pipeline_subtype", tratis.PipelineSubtypeName);
			AsyncRequest<bool> request = new AsyncRequest<bool>(Strings.PerformingRequest);
			AvatarSdkMgr.SpawnCoroutine(CheckIfPipelineSupportedFunc(url, request));
			return request;
		}
#endregion

#region Higher-level API, composite requests

		/// <summary>
		/// AwaitAvatarCalculationsAsync implementation.
		/// </summary>
		private IEnumerator AwaitAvatarCalculationsLoop(AvatarData avatar, AsyncRequest<AvatarData> request)
		{
			while (!Strings.FinalStates.Contains(avatar.status))
			{
				yield return new WaitForSecondsRealtime(4);
				var avatarStatusRequest = GetAvatarAsync(avatar.code);
				yield return avatarStatusRequest;

				if (avatarStatusRequest.Status.Value == (long)StatusCode.Code.NOT_FOUND)
				{
					Debug.LogWarning("404 error most likely means that avatar was deleted from the server");
					request.SetError(string.Format("Avatar status response: {0}", avatarStatusRequest.ErrorMessage));
					yield break;
				}

				if (avatarStatusRequest.Status.Value == (long)StatusCode.Code.TOO_MANY_REQUESTS_THROTTLING)
				{
					Debug.LogWarning("Too many requests!");
					yield return new WaitForSecondsRealtime(avatarStatusRequest.RetryPeriod.Value);
				}

				if (avatarStatusRequest.IsError)
				{
					Debug.LogWarningFormat("Status polling error: {0}", avatarStatusRequest.ErrorMessage);
					// Most likely this is a temporary issue. Keep polling.
					continue;
				}

				avatar = avatarStatusRequest.Result;
				Debug.LogFormat("Status: {0}, progress: {1}%", avatar.status, avatar.progress);
				request.State = AvatarSdkMgr.Str(avatar.status);

				if (avatar.status == Strings.Computing)
					request.Progress = (float)avatar.progress / 100;
			}

			if (Strings.GoodFinalStates.Contains(avatar.status))
			{
				request.Result = avatar;
				request.IsDone = true;
			}
			else
			{
				request.SetError(string.Format("Avatar calculations failed, status: {0}", avatar.status));
			}
		}

		/// <summary>
		/// Wait until avatar is calculated. Report progress through the async request object.
		/// This function will return error (request.IsError == true) only if calculations failed on server or
		/// avatar has been deleted from the server. In all other cases it will continue to poll status.
		/// </summary>
		public virtual AsyncRequest<AvatarData> AwaitAvatarCalculationsAsync(AvatarData avatar)
		{
			var request = new AsyncRequest<AvatarData>(AvatarSdkMgr.Str(Strings.StartingCalculations));
			AvatarSdkMgr.SpawnCoroutine(AwaitAvatarCalculationsLoop(avatar, request));
			return request;
		}

#endregion
	}
}
