using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using FindIt.GUI;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace FindIt
{
    // This patch adds some of Find It's own UI stuff (like the custom tag and steam sprites) to the game's default panels
    [HarmonyPatch(typeof(GeneratedScrollPanel))]
    [HarmonyPatch("CreateButton")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(string), typeof(int), typeof(UITextureAtlas), typeof(UIComponent), typeof(bool), typeof(UITextureAtlas), typeof(string) })]
    internal static class CreateButtonPatch
    {
        private static void Postfix(UIButton __result, GeneratedScrollPanel __instance, string name, string tooltip, string baseIconName, int index, UITextureAtlas atlas, UIComponent tooltipBox, bool enabled, UITextureAtlas badgeAtlas, string badgeSpriteName)
        {
            if (Settings.hideExtraUIonVP) return;

            __result.eventVisibilityChanged += new PropertyChangedEventHandler<bool>(Init);

            GeneratedScrollPanel panel = __instance;
            SimulationManager.instance.AddAction(() =>
            {
                if (__result.objectUserData is PrefabInfo prefab)
                {
                    string key = Asset.GetName(prefab);

                    if (AssetTagList.instance.assets.ContainsKey(key))
                    {
                        if (AssetTagList.instance.assets[key].onButtonClicked == null)
                        {
                            MethodInfo onButtonClicked = panel.GetType().GetMethod("OnButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance);
                            AssetTagList.instance.assets[key].onButtonClicked = Delegate.CreateDelegate(typeof(Asset.OnButtonClicked), panel, onButtonClicked, false) as Asset.OnButtonClicked;
                        }
                    }
                }
            });
        }

        private static UIComponent tsContainer;

        private static void Init(UIComponent component, bool b)
        {
            component.eventVisibilityChanged -= new PropertyChangedEventHandler<bool>(Init);

            try
            {
                if (component.objectUserData is PrefabInfo prefab)
                {
                    string name = Asset.GetName(prefab);

                    if (AssetTagList.instance.assets.ContainsKey(name))
                    {
                        ImageUtils.FixThumbnails(prefab, component as UIButton);

                        Asset asset = AssetTagList.instance.assets[name];

                        component.eventVisibilityChanged += (c, p) =>
                        {
                            if (Settings.unlockAll)
                            {
                                c.isEnabled = true;
                            }
                            else
                            {
                                c.isEnabled = ToolsModifierControl.IsUnlocked(prefab.GetUnlockMilestone());
                            }
                        };

                        // Fixing focused texture
                        component.eventClicked += new MouseEventHandler(FixFocusedTexture);

                        // Find TSContainer
                        if (tsContainer == null)
                        {
                            tsContainer = component.parent.parent.parent.parent.parent;
                        }
                        float uiScale = 1.0f;
                        if (tsContainer != null) uiScale = tsContainer.transform.localScale.x;

                        // Adding custom tag icon
                        UISprite tagSprite = component.AddUIComponent<UISprite>();
                        tagSprite.size = new Vector2(20 * uiScale, 16 * uiScale);
                        tagSprite.atlas = FindIt.atlas;
                        tagSprite.spriteName = "Tag";
                        tagSprite.opacity = 0.5f;
                        tagSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
                        tagSprite.relativePosition = new Vector3(component.width - tagSprite.width - (5 * uiScale), 5 * uiScale);
                        tagSprite.isVisible = false;

                        if (CustomTagsLibrary.assetTags.ContainsKey(name))
                        {
                            tagSprite.tooltip = CustomTagsLibrary.assetTags[name];
                        }
                        else
                        {
                            tagSprite.tooltip = null;
                        }

                        tagSprite.eventMouseEnter += (c, p) =>
                        {
                            tagSprite.opacity = 1f;
                        };

                        tagSprite.eventMouseLeave += (c, p) =>
                        {
                            tagSprite.opacity = 0.5f;
                        };

                        tagSprite.eventClick += (c, p) =>
                        {
                            p.Use();

                            UITagsWindow.ShowAt(asset, tagSprite);
                        };

                        component.eventMouseEnter += (c, p) =>
                        {
                            tagSprite.isVisible = true;
                        };

                        component.eventMouseLeave += (c, p) =>
                        {
                            if (asset.tagsCustom.Count == 0)
                            {
                                tagSprite.isVisible = false;
                            }
                        };

                        // adding DLC/steam icon
                        UISprite m_dlcSprite = component.AddUIComponent<UISprite>();
                        m_dlcSprite.size = new Vector2(16 * uiScale, 16 * uiScale);
                        m_dlcSprite.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
                        m_dlcSprite.opacity = 0.8f;
                        m_dlcSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
                        m_dlcSprite.relativePosition = new Vector3(component.width - m_dlcSprite.width - (3 * uiScale), component.height - m_dlcSprite.height - (3 * uiScale));
                        m_dlcSprite.isVisible = false;
                        m_dlcSprite.eventMouseLeave += (c, p) =>
                        {
                            m_dlcSprite.tooltipBox.Hide();
                        };

                        m_dlcSprite.tooltip = null;
                        m_dlcSprite.isVisible = false;

                        if (asset != null)
                        {
                            // next 2 assets, show blue steam icon
                            if (FindIt.isNext2Enabled && AssetTagList.instance.next2Assets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                                m_dlcSprite.tooltip = "Network Extension 2 Mod";
                            }
                            // etst assets, show blue steam icon
                            else if (FindIt.isETSTEnabled && AssetTagList.instance.etstAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                                m_dlcSprite.tooltip = "Extra Train Station Tracks Mod";
                            }
                            // owtt assets, show blue steam icon
                            else if (FindIt.isOWTTEnabled && AssetTagList.instance.owttAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                                m_dlcSprite.tooltip = "One-Way Train Tracks Mod";
                            }
                            // tvp patch assets, show blue steam icon
                            else if ((FindIt.isTVPPatchEnabled || FindIt.isTVP2Enabled) && asset.assetType == Asset.AssetType.Prop && AssetTagList.instance.tvppAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";

                                // if based on vanilla assets, show blue steam icon
                                if (!asset.prefab.m_isCustomContent)
                                {
                                    m_dlcSprite.tooltip = "Tree & Vehicle Props Mod";
                                    if (asset.prefab.m_requiredExpansion != SteamHelper.ExpansionBitMask.None || asset.prefab.m_requiredModderPack != SteamHelper.ModderPackBitMask.None)
                                    {
                                        m_dlcSprite.tooltip += $"\n{UIScrollPanelItem.GetDLCSpriteToolTip(asset.prefab.m_requiredExpansion, asset.prefab.m_requiredModderPack)}";
                                    }
                                }
                                else
                                {
                                    if (!asset.author.IsNullOrWhiteSpace() && (asset.steamID != 0))
                                    {
                                        m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\nBy " + asset.author + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else if (asset.steamID != 0)
                                    {
                                        m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else
                                    {
                                        m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\n" + Translations.Translate("FIF_UIS_CNWS");
                                    }
                                }
                            }
                            // ntcp assets, show blue steam icon
                            else if (FindIt.isNTCPEnabled && asset.assetType == Asset.AssetType.Prop && AssetTagList.instance.ntcpAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";

                                // if based on vanilla assets, show blue steam icon
                                if (!asset.prefab.m_isCustomContent)
                                {
                                    m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod";
                                    if (asset.prefab.m_requiredExpansion != SteamHelper.ExpansionBitMask.None || asset.prefab.m_requiredModderPack != SteamHelper.ModderPackBitMask.None)
                                    {
                                        m_dlcSprite.tooltip += $"\n{UIScrollPanelItem.GetDLCSpriteToolTip(asset.prefab.m_requiredExpansion, asset.prefab.m_requiredModderPack)}";
                                    }
                                }
                                else
                                {
                                    if (!asset.author.IsNullOrWhiteSpace() && (asset.steamID != 0))
                                    {
                                        m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod\nBy " + asset.author + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else if (asset.steamID != 0)
                                    {
                                        m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod" + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else
                                    {
                                        m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod\n" + Translations.Translate("FIF_UIS_CNWS");
                                    }
                                }
                            }
                            // vanilla assets, show corresponding dlc icons
                            else if (!asset.prefab.m_isCustomContent)
                            {
                                SetDLCSprite(m_dlcSprite, asset.prefab.m_requiredExpansion, asset.prefab.m_requiredModderPack);
                            }
                            // custom assets, show steam icon(has workshop info) or yellow cogwheel icon(no workshop info)
                            else
                            {
                                if (!asset.author.IsNullOrWhiteSpace() && (asset.steamID != 0))
                                {
                                    m_dlcSprite.opacity = 0.2f;
                                    m_dlcSprite.isVisible = true;
                                    m_dlcSprite.spriteName = "UIFilterWorkshopItems";
                                    m_dlcSprite.tooltip = "By " + asset.author + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                }
                                else
                                {
                                    m_dlcSprite.opacity = 0.45f;
                                    m_dlcSprite.isVisible = true;
                                    m_dlcSprite.spriteName = "UIFilterProcessingBuildings";
                                    m_dlcSprite.tooltip = Translations.Translate("FIF_UIS_CNWS");
                                }
                            }
                        }

                        if (PlatformService.IsOverlayEnabled() && m_dlcSprite.spriteName == "UIFilterWorkshopItems")
                        {
                            m_dlcSprite.eventMouseUp += (c, p) =>
                            {
                                if (!p.used && p.buttons == UIMouseButton.Right)
                                {
                                    PublishedFileId publishedFileId = new PublishedFileId(asset.steamID);

                                    if (publishedFileId != PublishedFileId.invalid)
                                    {
                                        if (!Settings.useDefaultBrowser)
                                        {
                                            PlatformService.ActivateGameOverlayToWorkshopItem(publishedFileId);
                                        }
                                        else
                                        {
                                            // UnityEngine.Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedFileId);
                                            System.Diagnostics.Process.Start("https://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedFileId);
                                        }
                                        p.Use();
                                    }
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }

        private static void FixFocusedTexture(UIComponent component, UIMouseEventParameter p)
        {
            component.eventClicked -= new MouseEventHandler(FixFocusedTexture);

            try
            {
                if (component.objectUserData is PrefabInfo prefab)
                {
                    if (ImageUtils.FixFocusedTexture(prefab))
                    {
                        // Debugging.Message("Fixed focused texture: " + prefab.name);
                    }
                    UIScrollPanelItem.fixedFocusedTexture.Add(prefab);
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }

        private static void SetDLCSprite(UISprite sprite, SteamHelper.ExpansionBitMask expansion, SteamHelper.ModderPackBitMask modderPack)
        {
            if (expansion == SteamHelper.ExpansionBitMask.None && modderPack == SteamHelper.ModderPackBitMask.None) return;
            sprite.isVisible = true;
            sprite.tooltip = UIScrollPanelItem.GetDLCSpriteToolTip(expansion, modderPack);
            sprite.spriteName = UIScrollPanelItem.GetDLCSpriteName(expansion, modderPack);
        }
    }
}
