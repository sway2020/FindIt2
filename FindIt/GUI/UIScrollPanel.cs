// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIScrollPanel : UIFastList<UIScrollPanelItem.ItemData, UIScrollPanelItem, UIButton>
    {
        public FastList<UIScrollPanelItem.ItemData> savedItems;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (height > itemHeight && scrollbar == null)
            {
                DestroyScrollbars(parent);

                // Scrollbar
                UIScrollbar scroll = parent.AddUIComponent<UIScrollbar>();
                scroll.width = 20f;
                scroll.height = parent.parent.height;
                scroll.orientation = UIOrientation.Vertical;
                scroll.pivot = UIPivotPoint.BottomLeft;
                scroll.thumbPadding = new RectOffset(0, 0, 5, 5);
                scroll.AlignTo(scroll.parent, UIAlignAnchor.TopRight);
                scroll.minValue = 0;
                scroll.value = 0;
                scroll.incrementAmount = 50;

                UISlicedSprite tracSprite = scroll.AddUIComponent<UISlicedSprite>();
                tracSprite.relativePosition = Vector2.zero;
                tracSprite.autoSize = true;
                tracSprite.size = tracSprite.parent.size;
                tracSprite.fillDirection = UIFillDirection.Vertical;
                tracSprite.spriteName = ""; // "ScrollbarTrack";

                scroll.trackObject = tracSprite;

                UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
                thumbSprite.relativePosition = Vector2.zero;
                thumbSprite.fillDirection = UIFillDirection.Vertical;
                thumbSprite.autoSize = true;
                thumbSprite.width = thumbSprite.parent.width - 8;
                thumbSprite.spriteName = "ScrollbarThumb";

                scroll.thumbObject = thumbSprite;

                scrollbar = scroll;
            }
            else if (height <= itemHeight && scrollbar != null)
            {
                DestroyScrollbars(parent);
            }

            // adjust scrollbars in dropdown menus(needed for Yet Another Toolbar & Resize It)
            if (UIFilterExtraPanel.instance != null)
            {
                SamsamTS.UIUtils.DestroyDropDownScrollBar(UIFilterExtraPanel.instance.optionDropDownMenu);
                SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.optionDropDownMenu);
                SamsamTS.UIUtils.DestroyDropDownScrollBar(UIFilterExtraPanel.instance.assetCreatorDropDownMenu);
                SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.assetCreatorDropDownMenu);
                SamsamTS.UIUtils.DestroyDropDownScrollBar(UIFilterExtraPanel.instance.dlcDropDownMenu);
                SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.dlcDropDownMenu);
            }
            if (UIFilterTagPanel.instance != null)
            {
                SamsamTS.UIUtils.DestroyDropDownScrollBar(UIFilterTagPanel.instance.tagDropDownMenu);
                SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterTagPanel.instance.tagDropDownMenu);
            }
        }

        public static UIScrollPanel Create(UIScrollablePanel oldPanel)
        {
            UIScrollPanel scrollPanel = oldPanel.parent.AddUIComponent<UIScrollPanel>();
            scrollPanel.name = oldPanel.name;
            scrollPanel.autoLayout = false;
            scrollPanel.autoReset = false;
            scrollPanel.autoSize = false;
            scrollPanel.template = "PlaceableItemTemplate";
            scrollPanel.itemWidth = 109f;
            scrollPanel.itemHeight = 100f;
            scrollPanel.canSelect = true;
            scrollPanel.size = new Vector2(763, 100);
            scrollPanel.relativePosition = new Vector3(48, 5);
            scrollPanel.atlas = oldPanel.atlas;

            scrollPanel.parent.parent.eventSizeChanged += (c, p) =>
            {
                if (scrollPanel.isVisible)
                {
                    scrollPanel.size = new Vector2((int)((p.x - 40f) / scrollPanel.itemWidth) * scrollPanel.itemWidth, (int)(p.y / scrollPanel.itemHeight) * scrollPanel.itemHeight);
                    scrollPanel.relativePosition = new Vector3(scrollPanel.relativePosition.x, Mathf.Floor((p.y - scrollPanel.height) / 2));

                    if (scrollPanel.rightArrow != null)
                    {
                        scrollPanel.rightArrow.relativePosition = new Vector3(scrollPanel.relativePosition.x + scrollPanel.width, 0);
                    }
                }
            };

            int zOrder = oldPanel.zOrder;

            DestroyImmediate(oldPanel.gameObject);
            DestroyScrollbars(scrollPanel.parent);

            scrollPanel.zOrder = zOrder;

            // Left / Right buttons
            UIButton button = scrollPanel.parent.AddUIComponent<UIButton>();
            button.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            button.name = "ArrowLeft";
            button.size = new Vector2(32, 109);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.normalFgSprite = "ArrowLeft";
            button.focusedFgSprite = "ArrowLeftFocused";
            button.hoveredFgSprite = "ArrowLeftHovered";
            button.pressedFgSprite = "ArrowLeftPressed";
            button.disabledFgSprite = "ArrowLeftDisabled";
            button.isEnabled = false;
            button.relativePosition = new Vector3(16, 0);
            scrollPanel.leftArrow = button;

            button = scrollPanel.parent.AddUIComponent<UIButton>();
            button.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            button.name = "ArrowRight";
            button.size = new Vector2(32, 109);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.normalFgSprite = "ArrowRight";
            button.focusedFgSprite = "ArrowRightFocused";
            button.hoveredFgSprite = "ArrowRightHovered";
            button.pressedFgSprite = "ArrowRightPressed";
            button.disabledFgSprite = "ArrowRightDisabled";
            button.isEnabled = false;
            button.relativePosition = new Vector3(811, 0);
            scrollPanel.rightArrow = button;

            return scrollPanel;
        }

        private static void DestroyScrollbars(UIComponent parent)
        {
            UIScrollbar[] scrollbars = parent.GetComponentsInChildren<UIScrollbar>();
            foreach (UIScrollbar scrollbar in scrollbars)
            {
                DestroyImmediate(scrollbar.gameObject);
            }
        }
    }

    public class UIScrollPanelItem : IUIFastListItem<UIScrollPanelItem.ItemData, UIButton>
    {
        private ItemData currentData;
        private UISprite m_tagSprite;
        private UISprite m_dlcSprite;

        private UICheckBox m_batchCheckBox;
        private UILabel m_instanceCountLabel;

        private static UIComponent m_tooltipBox;

        public static HashSet<PrefabInfo> fixedFocusedTexture = new HashSet<PrefabInfo>();
        public static bool SimulatingClick;

        public UIButton component
        {
            get;
            set;
        }

        public class ItemData
        {
            public string name;
            public string tooltip;
            public UIComponent tooltipBox;
            public GeneratedScrollPanel panel;
            public Asset asset;
        }

        public void Init()
        {
            component.text = string.Empty;
            component.tooltipAnchor = UITooltipAnchor.Anchored;
            component.tabStrip = true;
            component.horizontalAlignment = UIHorizontalAlignment.Center;
            component.verticalAlignment = UIVerticalAlignment.Middle;
            component.pivot = UIPivotPoint.TopCenter;
            component.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            component.group = component.parent;

            component.eventTooltipShow += (c, p) =>
            {
                if (m_tooltipBox != null && m_tooltipBox.isVisible && m_tooltipBox != p.tooltip)
                {
                    m_tooltipBox.Hide();
                }
                m_tooltipBox = p.tooltip;
            };

            component.eventMouseLeave += (c, p) =>
            {
                if (m_tooltipBox != null && m_tooltipBox.isVisible)
                {
                    m_tooltipBox.Hide();
                }
            };

            UIComponent uIComponent = (component.childCount <= 0) ? null : component.components[0];
            if (uIComponent != null)
            {
                uIComponent.isVisible = false;
            }

            m_tagSprite = component.AddUIComponent<UISprite>();
            m_tagSprite.size = new Vector2(20, 16);
            m_tagSprite.atlas = FindIt.atlas;
            m_tagSprite.spriteName = "Tag";
            m_tagSprite.opacity = 0.5f;
            m_tagSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
            m_tagSprite.relativePosition = new Vector3(component.width - m_tagSprite.width - 5, 5);
            m_tagSprite.isVisible = false;

            m_tagSprite.eventMouseEnter += (c, p) =>
            {
                m_tagSprite.opacity = 1f;
            };

            m_tagSprite.eventMouseLeave += (c, p) =>
            {
                m_tagSprite.opacity = 0.5f;
            };

            m_tagSprite.eventClick += (c, p) =>
            {
                p.Use();

                UITagsWindow.ShowAt(currentData.asset, m_tagSprite);
            };

            // batch action check box
            m_batchCheckBox = SamsamTS.UIUtils.CreateCheckBox(component);
            m_batchCheckBox.isChecked = false;
            m_batchCheckBox.isVisible = false;
            m_batchCheckBox.width = 20;
            m_batchCheckBox.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);
            m_batchCheckBox.relativePosition = new Vector3(5, component.height - m_batchCheckBox.height - 5);
            m_batchCheckBox.eventClicked += (c, i) =>
            {
                if (currentData != null && currentData.asset != null && m_batchCheckBox)
                {
                    if (m_batchCheckBox.isChecked)
                    {
                        UIFilterTagPanel.instance.batchAssetSet.Add(currentData.asset);
                        // Debugging.Message("Batch - Add to batch set: " + currentData.asset.name);
                    }
                    else
                    {
                        UIFilterTagPanel.instance.batchAssetSet.Remove(currentData.asset);
                        // Debugging.Message("Batch - Remove from batch set: " + currentData.asset.name);
                    }
                }
            };

            m_instanceCountLabel = component.AddUIComponent<UILabel>();
            m_instanceCountLabel.textScale = 0.8f;
            //m_instanceCountLabel.padding = new RectOffset(0, 0, 8, 0);
            m_instanceCountLabel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            m_instanceCountLabel.backgroundSprite = "GenericTabDisabled";
            m_instanceCountLabel.relativePosition = new Vector3(5, 5);

            component.eventMouseEnter += (c, p) =>
            {
                if (currentData != null && currentData.asset != null &&
                    AssetTagList.instance.assets.ContainsValue(currentData.asset))
                {
                    m_tagSprite.isVisible = true;
                }
            };

            component.eventMouseLeave += (c, p) =>
            {
                if (m_tagSprite.isVisible && currentData != null && currentData.asset != null && currentData.asset.tagsCustom.Count == 0)
                {
                    m_tagSprite.isVisible = false;
                }
            };

            component.eventDoubleClick += (c, p) =>
            {
                if (currentData != null && currentData.asset != null)
                {
                    UIDoubleClickMenu.ShowAt(component, currentData.asset);
                }
            };

            m_dlcSprite = component.AddUIComponent<UISprite>();
            m_dlcSprite.size = new Vector2(18, 18);
            m_dlcSprite.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            m_dlcSprite.opacity = 0.55f;
            m_dlcSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
            m_dlcSprite.relativePosition = new Vector3(component.width - m_dlcSprite.width - 3, component.height - m_dlcSprite.height - 3);
            m_dlcSprite.isVisible = false;
            m_dlcSprite.eventMouseLeave += (c, p) =>
            {
                m_dlcSprite.tooltipBox.Hide();
            };
            if (PlatformService.IsOverlayEnabled())
            {
                m_dlcSprite.eventMouseUp += OnTooltipClicked;
            }
        }

        public void SimulateClickSafe()
        {
            try
            {
                SimulatingClick = true;
                this.component.SimulateClick();
            }
            finally
            {
                SimulatingClick = false;
            }
        }

        public void Display(ItemData data, int index)
        {
            try
            {
                if (data == null)
                {
                    Debugging.Message("Data null");
                }

                if (component == null || data?.name == null) return;

                currentData = data;

                component.Unfocus();
                component.name = data.name;
                component.gameObject.GetComponent<TutorialUITag>().tutorialTag = data.name;

                PrefabInfo prefab = data.asset.prefab;
                if (prefab == null)
                {
                    Debugging.Message("Couldn't display item. Prefab is null");
                    return;
                }

                ImageUtils.FixThumbnails(prefab, null, data.asset);

                component.atlas = prefab.m_Atlas;
                component.verticalAlignment = UIVerticalAlignment.Middle;

                component.normalFgSprite = prefab.m_Thumbnail;
                component.hoveredFgSprite = prefab.m_Thumbnail + "Hovered";
                component.pressedFgSprite = prefab.m_Thumbnail + "Pressed";
                component.disabledFgSprite = prefab.m_Thumbnail + "Disabled";
                component.focusedFgSprite = null;

                bool rico = false;

                if (FindIt.isRicoEnabled)
                {
                    string name = Asset.GetName(prefab);
                    if (AssetTagList.instance.assets.ContainsKey(name))
                    {
                        rico = AssetTagList.instance.assets[name].assetType == Asset.AssetType.Rico;
                    }
                }

                component.isEnabled = rico || Settings.unlockAll || ToolsModifierControl.IsUnlocked(prefab.GetUnlockMilestone());
                component.tooltip = data.tooltip;
                component.tooltipBox = data.tooltipBox;
                component.objectUserData = data.asset.prefab;
                component.forceZOrder = index;

                if (component.containsMouse)
                {
                    component.RefreshTooltip();

                    if (m_tooltipBox != null && m_tooltipBox.isVisible && m_tooltipBox != data.tooltipBox)
                    {
                        m_tooltipBox.Hide();
                        data.tooltipBox.Show(true);
                        data.tooltipBox.opacity = 1f;
                        data.tooltipBox.relativePosition = m_tooltipBox.relativePosition + new Vector3(0, m_tooltipBox.height - data.tooltipBox.height);
                    }

                    m_tooltipBox = data.tooltipBox;

                    RefreshTooltipAltas(component);
                }

                if (m_tagSprite != null)
                {
                    m_tagSprite.atlas = FindIt.atlas;

                    m_tagSprite.isVisible = currentData.asset != null && AssetTagList.instance.assets.ContainsValue(currentData.asset) && (component.containsMouse || currentData.asset.tagsCustom.Count > 0);
                }


                // batch action check box
                if (m_batchCheckBox != null && data.asset != null && UIFilterTagPanel.instance?.batchAssetSet != null)
                {
                    if (UIFilterTagPanel.instance.batchAssetSet.Contains(data.asset))
                    {
                        m_batchCheckBox.isChecked = true;
                    }
                    else
                    {
                        m_batchCheckBox.isChecked = false;
                    }

                    m_batchCheckBox.isVisible = UISearchBox.instance.tagPanel.isBatchActionsEnabled;
                }
                else if (m_batchCheckBox != null)
                {
                    m_batchCheckBox.isVisible = false;
                }

                if (m_instanceCountLabel != null && data.asset?.prefab != null)
                {
                    if (Settings.showInstancesCounter)
                    {
                        m_instanceCountLabel.isVisible = true;

                        uint count = 0;
                        if (AssetTagList.instance.prefabInstanceCountDictionary.ContainsKey(data.asset.prefab))
                        {
                            count = AssetTagList.instance.prefabInstanceCountDictionary[data.asset.prefab];
                        }

                        if (data.asset.prefab is NetInfo)
                        {
                            m_instanceCountLabel.text = (count == 0) ? Translations.Translate("FIF_UIS_UN") : Translations.Translate("FIF_UIS_IN");
                        }
                        else
                        {
                            if (data.asset.prefab is TreeInfo)
                            {
                                m_instanceCountLabel.text = (count == 0) ? Translations.Translate("FIF_UIS_UN") : count.ToString();
                            }
                            else
                            {
                                if (Settings.includePOinstances && FindIt.isPOEnabled)
                                {
                                    uint poCount = 0;
                                    poCount = ProceduralObjectsTool.GetPrefabPOInstanceCount(data.asset.prefab);
                                    m_instanceCountLabel.text = "";
                                    if (count == 0 && poCount == 0)
                                    {
                                        m_instanceCountLabel.text = Translations.Translate("FIF_UIS_UN");
                                    }
                                    if (count != 0)
                                    {
                                        m_instanceCountLabel.text += (count.ToString());
                                        if (poCount != 0) m_instanceCountLabel.text += (" + ");
                                    }
                                    if (poCount != 0)
                                    {
                                        m_instanceCountLabel.text += (poCount.ToString() + " PO");
                                    }
                                }
                                else
                                {
                                    m_instanceCountLabel.text = (count == 0) ? Translations.Translate("FIF_UIS_UN") : count.ToString();
                                }
                            }

                        }
                    }
                    else m_instanceCountLabel.isVisible = false;
                }

                if (m_dlcSprite != null)
                {
                    m_dlcSprite.tooltip = null;
                    m_dlcSprite.isVisible = false;
                    m_dlcSprite.opacity = 0.8f;

                    if (data.asset != null)
                    {
                        // next 2 assets, show blue steam icon
                        if (FindIt.isNext2Enabled && AssetTagList.instance.next2Assets.Contains(data.asset))
                        {
                            m_dlcSprite.isVisible = true;
                            m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                            m_dlcSprite.tooltip = "Network Extension 2 Mod";
                        }
                        // etst assets, show blue steam icon
                        else if (FindIt.isETSTEnabled && AssetTagList.instance.etstAssets.Contains(data.asset))
                        {
                            m_dlcSprite.isVisible = true;
                            m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                            m_dlcSprite.tooltip = "Extra Train Station Tracks Mod";
                        }
                        // owtt assets, show blue steam icon
                        else if (FindIt.isOWTTEnabled && AssetTagList.instance.owttAssets.Contains(data.asset))
                        {
                            m_dlcSprite.isVisible = true;
                            m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                            m_dlcSprite.tooltip = "One-Way Train Tracks Mod";
                        }
                        // tvp patch assets, show blue steam icon
                        else if ((FindIt.isTVPPatchEnabled || FindIt.isTVP2Enabled) && data.asset.assetType == Asset.AssetType.Prop && AssetTagList.instance.tvppAssets.Contains(data.asset))
                        {
                            m_dlcSprite.isVisible = true;
                            m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";

                            // if based on vanilla assets, show blue steam icon
                            if (!data.asset.prefab.m_isCustomContent)
                            {
                                m_dlcSprite.tooltip = "Tree & Vehicle Props Mod";
                                if (data.asset.prefab.m_dlcRequired != SteamHelper.DLC_BitMask.None)
                                {
                                    m_dlcSprite.tooltip += $"\n{UIScrollPanelItem.GetDLCSpriteToolTip(data.asset.prefab.m_dlcRequired)}";
                                }
                            }
                            else
                            {
                                if (!data.asset.author.IsNullOrWhiteSpace() && (data.asset.steamID != 0))
                                {
                                    m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\nBy " + data.asset.author + "\n" + "ID: " + data.asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                }
                                else if (data.asset.steamID != 0)
                                {
                                    m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\n" + "ID: " + data.asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                }
                                else
                                {
                                    m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\n" + Translations.Translate("FIF_UIS_CNWS");
                                }
                            }
                        }
                        // ntcp assets, show blue steam icon
                        else if (FindIt.isNTCPEnabled && data.asset.assetType == Asset.AssetType.Prop && AssetTagList.instance.ntcpAssets.Contains(data.asset))
                        {
                            m_dlcSprite.isVisible = true;
                            m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";

                            // if based on vanilla assets, show blue steam icon
                            if (!data.asset.prefab.m_isCustomContent)
                            {
                                m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod";
                                if (data.asset.prefab.m_dlcRequired != SteamHelper.DLC_BitMask.None)
                                {
                                    m_dlcSprite.tooltip += $"\n{UIScrollPanelItem.GetDLCSpriteToolTip(data.asset.prefab.m_dlcRequired)}";
                                }
                            }
                            else
                            {
                                if (!data.asset.author.IsNullOrWhiteSpace() && (data.asset.steamID != 0))
                                {
                                    m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod\nBy " + data.asset.author + "\n" + "ID: " + data.asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                }
                                else if (data.asset.steamID != 0)
                                {
                                    m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod" + "\n" + "ID: " + data.asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                }
                                else
                                {
                                    m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod\n" + Translations.Translate("FIF_UIS_CNWS");
                                }
                            }
                        }
                        // vanilla assets, show corresponding dlc icons
                        else if (!data.asset.prefab.m_isCustomContent)
                        {
                            SetDLCSprite(m_dlcSprite, data.asset.prefab.m_dlcRequired);
                        }
                        // custom assets, show steam icon(has workshop info) or yellow cogwheel icon(no workshop info)
                        else
                        {
                            if (!data.asset.author.IsNullOrWhiteSpace() && (data.asset.steamID != 0))
                            {
                                m_dlcSprite.opacity = 0.45f;
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItems";
                                m_dlcSprite.tooltip = "By " + data.asset.author + "\n" + "ID: " + data.asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                            }
                            else
                            {
                                m_dlcSprite.opacity = 0.55f;
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterProcessingBuildings";
                                m_dlcSprite.tooltip = Translations.Translate("FIF_UIS_CNWS");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (data != null)
                {
                    Debugging.Message("Display failed : " + data.name);
                }
                else
                {
                    Debugging.Message("Display failed");
                }
                Debugging.LogException(e);
            }
        }

        public void Select(int index)
        {
            try
            {
                if (currentData != null && currentData.asset != null && currentData.asset.prefab != null && !fixedFocusedTexture.Contains(currentData.asset.prefab))
                {
                    if (ImageUtils.FixFocusedTexture(currentData.asset.prefab))
                    {
                        // Debugging.Message("Fixed focused texture: " + currentData.asset.prefab.name);
                    }
                    fixedFocusedTexture.Add(currentData.asset.prefab);
                }

                component.normalFgSprite = currentData.asset.prefab.m_Thumbnail + "Focused";
                component.hoveredFgSprite = currentData.asset.prefab.m_Thumbnail + "Focused";
            }
            catch (Exception e)
            {
                if (currentData != null)
                {
                    Debugging.Message("Select failed : " + currentData.name);
                }
                else
                {
                    Debugging.Message("Select failed");
                }
                Debugging.LogException(e);
            }
        }

        public void Deselect(int index)
        {
            try
            {
                component.normalFgSprite = currentData.asset.prefab.m_Thumbnail;
                component.hoveredFgSprite = currentData.asset.prefab.m_Thumbnail + "Hovered";
            }
            catch (Exception e)
            {
                if (currentData != null)
                {
                    Debugging.Message("Deselect failed : " + currentData.name);
                }
                else
                {
                    Debugging.Message("Deselect failed");
                }
                Debugging.LogException(e);
            }
        }

        private void OnTooltipClicked(UIComponent c, UIMouseEventParameter p)
        {
            if (!p.used && p.buttons == UIMouseButton.Right && currentData != null && currentData.asset != null)
            {

                if (currentData.asset.steamID == 0 || !currentData.asset.prefab.m_isCustomContent) return;

                PublishedFileId publishedFileId = new PublishedFileId(currentData.asset.steamID);

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
        }

        public static void RefreshTooltipAltas(UIComponent item)
        {
            PrefabInfo prefab = item.objectUserData as PrefabInfo;
            if (prefab != null)
            {
                UISprite uISprite = item.tooltipBox.Find<UISprite>("Sprite");
                if (uISprite != null)
                {
                    if (prefab.m_InfoTooltipAtlas != null)
                    {
                        uISprite.atlas = prefab.m_InfoTooltipAtlas;
                    }
                    if (!string.IsNullOrEmpty(prefab.m_InfoTooltipThumbnail) && uISprite.atlas[prefab.m_InfoTooltipThumbnail] != null)
                    {
                        uISprite.spriteName = prefab.m_InfoTooltipThumbnail;
                    }
                    else
                    {
                        uISprite.spriteName = "ThumbnailBuildingDefault";
                    }
                }
            }
        }
        private void SetDLCSprite(UISprite sprite, SteamHelper.DLC_BitMask dlc)
        {
            if (dlc == SteamHelper.DLC_BitMask.None) return;
            sprite.isVisible = true;
            sprite.tooltip = GetDLCSpriteToolTip(dlc);
            sprite.spriteName = GetDLCSpriteName(dlc);
        }

        public static string GetDLCSpriteToolTip(SteamHelper.DLC_BitMask dlc)
        {
            switch (dlc)
            {
                case SteamHelper.DLC_BitMask.DeluxeDLC: return "Deluxe Upgrade Pack";
                case SteamHelper.DLC_BitMask.AfterDarkDLC: return "After Dark DLC";
                case SteamHelper.DLC_BitMask.SnowFallDLC: return "Snow Fall DLC";
                case SteamHelper.DLC_BitMask.NaturalDisastersDLC: return "Natural Disasters DLC";
                case SteamHelper.DLC_BitMask.InMotionDLC: return "Mass Transit DLC";
                case SteamHelper.DLC_BitMask.GreenCitiesDLC: return "Green Cities DLC";
                case SteamHelper.DLC_BitMask.ParksDLC: return "Parklife DLC";
                case SteamHelper.DLC_BitMask.PlazasAndPromenadesDLC: return "Plazas & Promenades DLC";
                case SteamHelper.DLC_BitMask.IndustryDLC: return "Industries DLC";
                case SteamHelper.DLC_BitMask.CampusDLC: return "Campus DLC";
                case SteamHelper.DLC_BitMask.UrbanDLC: return "Sunset Harbor DLC";
                case SteamHelper.DLC_BitMask.AirportDLC: return "Airports DLC";
                case SteamHelper.DLC_BitMask.Football: return "Match Day DLC";
                case SteamHelper.DLC_BitMask.Football2345: return "Stadiums: European Club Pack DLC";
                case SteamHelper.DLC_BitMask.OrientalBuildings: return "Pearls from the East DLC";
                case SteamHelper.DLC_BitMask.MusicFestival: return "Concerts DLC";
                case SteamHelper.DLC_BitMask.ModderPack1: return "Art Deco Content Creator Pack by Shroomblaze";
                case SteamHelper.DLC_BitMask.ModderPack2: return "High-Tech Buildings Content Creator Pack by GCVos";
                case SteamHelper.DLC_BitMask.ModderPack3: return "European Suburbias Content Creator Pack by Avanya";
                case SteamHelper.DLC_BitMask.ModderPack4: return "University City Content Creator Pack by KingLeno";
                case SteamHelper.DLC_BitMask.ModderPack5: return "Modern City Center Content Creator Pack by AmiPolizeiFunk";
                case SteamHelper.DLC_BitMask.ModderPack6: return "Modern Japan Content Creator Pack by Ryuichi Kaminogi";
                case SteamHelper.DLC_BitMask.ModderPack7: return "Train Stations Content Creator Pack by BadPeanut";
                case SteamHelper.DLC_BitMask.ModderPack8: return "Bridges & Piers Content Creator Pack by Armesto";
                case SteamHelper.DLC_BitMask.ModderPack10: return "Vehicles of the World Content Creator Pack by bsquiklehausen";
                case SteamHelper.DLC_BitMask.ModderPack11: return "Mid-Century Modern Content Creator Pack by REV0";
                case SteamHelper.DLC_BitMask.ModderPack12: return "Seaside Resorts Content Creator Pack by Gèze";
                case SteamHelper.DLC_BitMask.None: return "";
                default: return "Unknown DLC";
            }
        }

        public static string GetDLCSpriteName(SteamHelper.DLC_BitMask dlc)
        {
            switch (dlc)
            {
                case SteamHelper.DLC_BitMask.DeluxeDLC: return "DeluxeIcon";
                case SteamHelper.DLC_BitMask.AfterDarkDLC: return "ADIcon";
                case SteamHelper.DLC_BitMask.SnowFallDLC: return "WWIcon";
                case SteamHelper.DLC_BitMask.NaturalDisastersDLC: return "NaturalDisastersIcon";
                case SteamHelper.DLC_BitMask.InMotionDLC: return "MassTransitIcon";
                case SteamHelper.DLC_BitMask.GreenCitiesDLC: return "GreenCitiesIcon";
                case SteamHelper.DLC_BitMask.ParksDLC: return "ParkLifeIcon";
                case SteamHelper.DLC_BitMask.PlazasAndPromenadesDLC: return "PlazasPromenadesIcon";
                case SteamHelper.DLC_BitMask.IndustryDLC: return "IndustriesIcon";
                case SteamHelper.DLC_BitMask.CampusDLC: return "CampusIcon";
                case SteamHelper.DLC_BitMask.UrbanDLC: return "DonutIcon";
                case SteamHelper.DLC_BitMask.AirportDLC: return "AirportIcon";
                case SteamHelper.DLC_BitMask.Football: return "MDIcon";
                case SteamHelper.DLC_BitMask.Football2345: return "StadiumsDLCIcon";
                case SteamHelper.DLC_BitMask.OrientalBuildings: return "ChineseBuildingsTagIcon";
                case SteamHelper.DLC_BitMask.MusicFestival: return "ConcertsIcon";
                case SteamHelper.DLC_BitMask.ModderPack1: return "ArtDecoIcon";
                case SteamHelper.DLC_BitMask.ModderPack2: return "HighTechIcon";
                case SteamHelper.DLC_BitMask.ModderPack3: return "Modderpack3Icon";
                case SteamHelper.DLC_BitMask.ModderPack4: return "Modderpack4Icon";
                case SteamHelper.DLC_BitMask.ModderPack5: return "Modderpack5Icon";
                case SteamHelper.DLC_BitMask.ModderPack6: return "Modderpack6Icon";
                case SteamHelper.DLC_BitMask.ModderPack7: return "Modderpack7Icon";
                case SteamHelper.DLC_BitMask.ModderPack8: return "Modderpack8Icon";
                case SteamHelper.DLC_BitMask.ModderPack11: return "MidCenturyModernIcon";
                case SteamHelper.DLC_BitMask.ModderPack12: return "SeasideResortsIcon";
                case SteamHelper.DLC_BitMask.None: return "";
                default: return "ToolbarIconHelp";
            }
        }
    }
}
