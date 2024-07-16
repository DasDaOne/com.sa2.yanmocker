using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace TmpSpriteYanMocker
{
	public static class TmpSpriteYanMocker
	{
		/// <summary>
		/// Sprite of CurrencyImage downloaded from Yandex
		/// </summary>
		public static Sprite MockedSprite;
		private static Texture2D MockedTexture;

		public static IEnumerator Initialize(TMP_SpriteAsset tmpSpriteAsset)
		{
			yield return GetSprite(tex =>
			{
				MockedTexture = tex;
			}, error =>
			{
				Debug.Log($"Downloading texture failed with error: {error}");
			});

			MockedTexture.wrapMode = TextureWrapMode.Clamp;
			
			MockedSprite = TextureToSprite(MockedTexture);

			tmpSpriteAsset.material.mainTexture = MockedTexture;

			float aspectRatio = MockedTexture.width / (float)MockedTexture.height;

			Vector2 textureScale = new (aspectRatio > 1 ? 1 : 1 / aspectRatio, aspectRatio < 1 ? 1 : aspectRatio);
			
			tmpSpriteAsset.material.mainTextureScale = textureScale;
			tmpSpriteAsset.material.mainTextureOffset = new Vector2(1 - textureScale.x, 1 - textureScale.y);
		}

		private static IEnumerator GetSprite(Action<Texture2D> onSuccessCallback, Action<string> onErrorCallback)
		{
			if (!Kimicu.YandexGames.Billing.Initialized)
			{
				Debug.LogWarning($"You need to initialize Billing before {nameof(TmpSpriteYanMocker)}");
				yield break;
			}
			
			if (Kimicu.YandexGames.Billing.CatalogProducts == null || Kimicu.YandexGames.Billing.CatalogProducts.Length == 0)
			{
				Debug.LogWarning("CatalogProducts is null or has length < 0 \n" +
				                 "Check presence of products in YandexConsole");
				yield break;
			}

			yield return DownloadTexture(Kimicu.YandexGames.Billing.CatalogProducts[0].priceCurrencyPicture, onSuccessCallback, onErrorCallback);
		}

		private static IEnumerator DownloadTexture(string url, Action<Texture2D> onSuccessCallback, Action<string> onErrorCallback)
		{
			UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
			
			yield return request.SendWebRequest();

			if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
				onErrorCallback.Invoke(request.error);
			else
				onSuccessCallback?.Invoke(DownloadHandlerTexture.GetContent(request));
		}
		
		private static Sprite TextureToSprite(Texture2D texture)
		{
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}
	}   
}
