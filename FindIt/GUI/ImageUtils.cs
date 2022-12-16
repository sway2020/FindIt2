// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt.GUI
{
    public class ImageUtils
    {
        private static Texture2D focusedFilterTexture;
        private static HashSet<PrefabInfo> fixedPrefabs = new HashSet<PrefabInfo>();

        public static void AddThumbnailVariantsInAtlas(PrefabInfo prefab)
        {
            Texture2D texture = prefab.m_Atlas[prefab.m_Thumbnail].texture;
            prefab.m_Atlas = ResourceLoader.CreateTextureAtlas("FindItThumbnails_" + prefab.m_Thumbnail, new string[] { }, null);

            ResourceLoader.AddTexturesInAtlas(prefab.m_Atlas, GenerateMissingThumbnailVariants(texture));

            // Debugging.Message("Generated thumbnails variants for: " + prefab.name);
        }

        public static void FixThumbnails(PrefabInfo prefab, UIButton button, Asset asset = null)
        {
            if (fixedPrefabs.Contains(prefab)) return;

            // Fixing thumbnails
            if (prefab.m_Atlas == null || prefab.m_Thumbnail.IsNullOrWhiteSpace() ||
                // used for more than one prefab
                prefab.m_Thumbnail == "Thumboldasphalt" ||
                prefab.m_Thumbnail == "Thumbbirdbathresidential" ||
                prefab.m_Thumbnail == "Thumbcrate" ||
                prefab.m_Thumbnail == "Thumbhedge" ||
                prefab.m_Thumbnail == "Thumbhedge2" ||
                // empty thumbnails
                prefab.m_Thumbnail == "thumb_Ferry Info Sign" ||
                prefab.m_Thumbnail == "thumb_Paddle Car 01" ||
                prefab.m_Thumbnail == "thumb_Paddle Car 02" ||
                prefab.m_Thumbnail == "thumb_Pier Rope Pole" ||
                prefab.m_Thumbnail == "thumb_RailwayPowerline Singular" ||
                prefab.m_Thumbnail == "thumb_Rubber Tire Row" ||
                prefab.m_Thumbnail == "thumb_Dam" ||
                prefab.m_Thumbnail == "thumb_Power Line" ||
                // terrible thumbnails
                prefab.m_Thumbnail == "thumb_Railway Crossing Long" ||
                prefab.m_Thumbnail == "thumb_Railway Crossing Medium" ||
                prefab.m_Thumbnail == "thumb_Railway Crossing Short" ||
                prefab.m_Thumbnail == "thumb_Railway Crossing Very Long"
                )
            {
                ThumbnailManager.MakeThumbnail(prefab, button);
                fixedPrefabs.Add(prefab);
                return;
            }

            if (prefab.m_Atlas != null && prefab.m_Atlas[prefab.m_Thumbnail] != null && (
                // Missing variations
                prefab.m_Atlas.name == "AssetThumbs" ||
                prefab.m_Atlas.name == "Monorailthumbs" ||
                prefab.m_Atlas.name == "Netpropthumbs" ||
                prefab.m_Atlas.name == "Animalthumbs" ||
                prefab.m_Atlas.name == "PublictransportProps" ||
                prefab.m_Thumbnail == "thumb_Path Rock 01" ||
                prefab.m_Thumbnail == "thumb_Path Rock 02" ||
                prefab.m_Thumbnail == "thumb_Path Rock 03" ||
                prefab.m_Thumbnail == "thumb_Path Rock 04" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 01" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 02" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 03" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 04" ||
                // Plazas & Promenades and Financial Districts DLC growables have missing variations
                ((( prefab.m_requiredExpansion & SteamHelper.ExpansionBitMask.PlazasAndPromenades) != 0 || 
                   (prefab.m_requiredExpansion & SteamHelper.ExpansionBitMask.FinancialDistricts) != 0) &&
                   !prefab.m_isCustomContent && (asset?.assetType == Asset.AssetType.Growable)) ||
                // Financial Districts DLC props have missing variations
                (((prefab.m_requiredExpansion & SteamHelper.ExpansionBitMask.FinancialDistricts) != 0) &&
                   !prefab.m_isCustomContent && (asset?.assetType == Asset.AssetType.Prop))
                ))
            {
                AddThumbnailVariantsInAtlas(prefab);

                if (button != null)
                {
                    button.atlas = prefab.m_Atlas;
                    button.normalFgSprite = prefab.m_Thumbnail;
                    button.hoveredFgSprite = prefab.m_Thumbnail + "Hovered";
                    button.pressedFgSprite = prefab.m_Thumbnail + "Pressed";
                    button.disabledFgSprite = prefab.m_Thumbnail + "Disabled";
                    button.focusedFgSprite = null;
                }
                fixedPrefabs.Add(prefab);
                return;
            }

            // Requested custom thumbnails or having null thumbnail atlas
            if (asset != null && (prefab.m_Atlas != null) && !(prefab is NetInfo))
            {
                if (asset.tagsCustom.Contains("bad_thumbnail") || prefab.m_Atlas[prefab.m_Thumbnail] == null)
                {
                    ThumbnailManager.MakeThumbnail(prefab, button);
                    fixedPrefabs.Add(prefab);
                    return;
                }
            }
        }

        public static void ScaleTexture(Texture2D tex, int width, int height)
        {
            tex.filterMode = FilterMode.Trilinear;
            var newPixels = new Color[width * height];
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    newPixels[y * width + x] = tex.GetPixelBilinear(((float)x) / width, ((float)y) / height);
                }
            }
            tex.Resize(width, height);
            tex.SetPixels(newPixels);
            tex.Apply();
        }

        public static void ScaleTexture2(Texture2D tex, int width, int height)
        {
            //var newPixels = new Color[width * height];

            float ratio = ((float)width) / tex.width;
            if (tex.height * ratio > height)
            {
                ratio = ((float)height) / tex.height;
            }

            if (ratio > 1f) ratio = 1f;

            int newW = Mathf.RoundToInt(tex.width * ratio);
            int newH = Mathf.RoundToInt(tex.height * ratio);

            ScaleTexture(tex, newW, newH);
        }

        public static void CropTexture(Texture2D tex, int x, int y, int width, int height)
        {
            var newPixels = tex.GetPixels(x, y, width, height);
            tex.Resize(width, height);
            tex.SetPixels(newPixels);
            tex.Apply();
        }

        // Colorize the focused icon blue using the LUT texture
        // Use a border of 8 (256/32) to ensure we don't pick up neighboring patches
        private static Color32 ColorizeFocused(Color32 c)
        {
            if (focusedFilterTexture == null)
            {
                focusedFilterTexture = ResourceLoader.LoadTextureFromAssembly("FindIt.Icons.SelectFilter.png");
            }

            int b = c.b * 31 / 255;
            float u = ((8f + (float)c.r) / 271) / 32 + ((float)b / 32);
            float v = 1f - ((8f + (float)c.g) / 271);
            Color32 result = focusedFilterTexture.GetPixelBilinear(u, v);
            result.a = c.a;
            return result;
        }

        public static bool FixFocusedTexture(PrefabInfo prefab)
        {
            if (prefab == null || prefab.m_Atlas == null || prefab.m_Thumbnail.IsNullOrWhiteSpace()) return false;

            UITextureAtlas.SpriteInfo sprite = prefab.m_Atlas[prefab.m_Thumbnail + "Focused"];
            if (sprite != null)
            {
                Color32[] pixels = sprite.texture.GetPixels32();

                int count = 0;

                foreach (Color32 pixel in pixels)
                {
                    if (pixel.a > 127 && (pixel.r + pixel.g + pixel.b) > 0)
                    {
                        Color.RGBToHSV(pixel, out float h, out float s, out float v);

                        if (h < 0.66f || h > 0.68f || s < 0.98f)
                        {
                            return false;
                        }

                        if (++count > 32)
                        {
                            break;
                        }
                    }
                }

                if (count > 0)
                {
                    ImageUtils.FixFocusedTexture(prefab.m_Atlas[prefab.m_Thumbnail].texture, sprite.texture);
                    Color32[] colors = sprite.texture.GetPixels32();

                    prefab.m_Atlas.texture.SetPixels32((int)(sprite.region.x * prefab.m_Atlas.texture.width), (int)(sprite.region.y * prefab.m_Atlas.texture.height), sprite.texture.width, sprite.texture.height, colors);
                    prefab.m_Atlas.texture.Apply();

                    return true;
                }
            }

            return false;
        }

        public static void RefreshAtlas(UITextureAtlas atlas)
        {
            Texture2D[] textures = new Texture2D[atlas.sprites.Count];

            int i = 0;
            foreach (UITextureAtlas.SpriteInfo sprite in atlas.sprites)
            {
                textures[i++] = sprite.texture;
            }
            atlas.AddTextures(textures);
        }

        public static void FixFocusedTexture(Texture2D baseTexture, Texture2D focusedTexture)
        {
            var newPixels = new Color32[baseTexture.width * baseTexture.height];
            var pixels = baseTexture.GetPixels32();

            ApplyFilter(pixels, newPixels, ColorizeFocused);
            focusedTexture.SetPixels32(newPixels);
            focusedTexture.Apply(false);
        }

        // Our own version of this as the one in AssetImporterThumbnails has hardcoded dimensions
        // and generates ugly dark blue focused thumbnails.
        public static Texture2D[] GenerateMissingThumbnailVariants(Texture2D baseTexture)
        {
            var newPixels = new Color32[baseTexture.width * baseTexture.height];
            var pixels = baseTexture.GetPixels32();

            ApplyFilter(pixels, newPixels, ColorizeFocused);
            Texture2D focusedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            focusedTexture.SetPixels32(newPixels);
            focusedTexture.Apply(false);
            focusedTexture.name = baseTexture.name + "Focused";

            ApplyFilter(pixels, newPixels, c => new Color32((byte)(128 + c.r / 2), (byte)(128 + c.g / 2), (byte)(128 + c.b / 2), c.a));
            Texture2D hoveredTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            hoveredTexture.SetPixels32(newPixels);
            hoveredTexture.Apply(false);
            hoveredTexture.name = baseTexture.name + "Hovered";

            ApplyFilter(pixels, newPixels, c => new Color32((byte)(192 + c.r / 4), (byte)(192 + c.g / 4), (byte)(192 + c.b / 4), c.a));
            Texture2D pressedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            pressedTexture.SetPixels32(newPixels);
            pressedTexture.Apply(false);
            pressedTexture.name = baseTexture.name + "Pressed";

            return new Texture2D[]
            {
                baseTexture,
                focusedTexture,
                hoveredTexture,
                pressedTexture
            };
        }

        delegate Color32 Filter(Color32 c);

        private static void ApplyFilter(Color32[] src, Color32[] dst, Filter filter)
        {
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = filter(src[i]);
            }
        }
    }
}
