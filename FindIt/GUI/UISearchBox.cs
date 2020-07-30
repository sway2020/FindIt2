﻿// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.GUI
{
    // Don't change the accessibility of some data members to private or it may break Picker mod's current reflection code
    // I've added a simple interface and sent Picker mod a pull request, but it hasn't beed accepted yet
    public class UISearchBox : UIPanel
    {
        public static UISearchBox instance;
        
        public UIPanel inputPanel;
        public UITextField input;
        public UIScrollPanel scrollPanel;
        public UIButton searchButton;
        public UIPanel filterPanel;
        public UIDropDown typeFilter;
        public UIPanel buildingFilters;
        public UIDropDown levelFilter;
        public UIDropDown sizeFilterX;
        public UIDropDown sizeFilterY;
        public UIFilterGrowable filterGrowable;
        public UIFilterPloppable filterPloppable;
        public UIFilterProp filterProp;
        public UIFilterTree filterTree;

        public UICheckBox workshopFilter;
        public UICheckBox vanillaFilter;
        public UIButton sortButton;

        public UIPanel toolIconPanel;
        
        public UISprite tagToolIcon;
        public UIFilterTag tagPanel;

        public UISprite extraFiltersIcon;
        public UIFilterExtra extraFiltersPanel;

        // true = sort by relevance
        // false = sort by most recently downloaded
        public bool sortButtonTextState = true;

        public enum DropDownOptions
        {
            All = 0,
            Network,
            Ploppable,
            Growable,
            Rico,
            GrwbRico,
            Prop,
            Decal,
            Tree
        }

        // building filter sizes
        public string[] filterItemsGrowable = { Translations.Translate("FIF_SE_IA"), "1", "2", "3", "4"};
        public string[] filterItemsRICO = { Translations.Translate("FIF_SE_IA"), "1", "2", "3", "4", "5-8", "9-12", "13+"};

        public ItemClass.Level buildingLevel
        {
            get { return (ItemClass.Level)(levelFilter.selectedIndex - 1); }
        }

        public Vector2 buildingSizeFilterIndex
        {
            get
            {
                if (sizeFilterX.selectedIndex == 0 && sizeFilterY.selectedIndex == 0) return Vector2.zero;
                return new Vector2(sizeFilterX.selectedIndex, sizeFilterY.selectedIndex);
            }
        }

        public override void Start()
        {
            instance = this;

            // panel for search input box, type filter and building filters
            inputPanel = AddUIComponent<UIPanel>();
            inputPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            inputPanel.backgroundSprite = "GenericTab";
            inputPanel.size = new Vector2(300, 40);
            inputPanel.relativePosition = new Vector2(0, -inputPanel.height - 40);

            // search input box
            input = SamsamTS.UIUtils.CreateTextField(inputPanel);
            input.size = new Vector2(inputPanel.width - 45, 30);
            input.padding.top = 7;
            input.relativePosition = new Vector3(5, 5);

            string search = null;
            input.eventTextChanged += (c, p) =>
            {
                search = p;
                Search();
            };

            input.eventTextCancelled += (c, p) =>
            {
                input.text = search;
            };

            // search button
            searchButton = inputPanel.AddUIComponent<UIButton>();
            searchButton.size = new Vector2(43, 49);
            searchButton.atlas = FindIt.instance.mainButton.atlas;
            searchButton.playAudioEvents = true;
            searchButton.normalFgSprite = "FindIt";
            searchButton.hoveredFgSprite = "FindItFocused";
            searchButton.pressedFgSprite = "FindItPressed";
            searchButton.relativePosition = new Vector3(inputPanel.width - 41, -3);

            searchButton.eventClick += (c, p) =>
            {
                input.Focus();
                input.SelectAll();
            };

            // panel for type filters and building filters
            filterPanel = AddUIComponent<UIPanel>();
            filterPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            filterPanel.backgroundSprite = "GenericTab";
            filterPanel.color = new Color32(196, 200, 206, 255);
            filterPanel.size = new Vector2(155, 35);
            filterPanel.SendToBack();
            filterPanel.relativePosition = new Vector3(inputPanel.width, -filterPanel.height - 40);

            // workshop filter checkbox (custom assets saved in local asset folder are also included)
            workshopFilter = SamsamTS.UIUtils.CreateCheckBox(filterPanel);
            workshopFilter.isChecked = true;
            workshopFilter.width = 80;
            workshopFilter.label.text = Translations.Translate("FIF_SE_WF");
            workshopFilter.label.textScale = 0.8f;
            workshopFilter.relativePosition = new Vector3(10, 10);
            workshopFilter.eventCheckChanged += (c, i) => Search();

            // vanilla filter checkbox
            vanillaFilter = SamsamTS.UIUtils.CreateCheckBox(filterPanel);
            vanillaFilter.isChecked = true;
            vanillaFilter.width = 80;
            vanillaFilter.label.text = Translations.Translate("FIF_SE_VF");
            vanillaFilter.label.textScale = 0.8f;
            vanillaFilter.relativePosition = new Vector3(workshopFilter.relativePosition.x + workshopFilter.width, 10);
            vanillaFilter.eventCheckChanged += (c, i) => Search();

            // asset type filter
            typeFilter = SamsamTS.UIUtils.CreateDropDown(filterPanel);
            typeFilter.size = new Vector2(100, 25);
            typeFilter.tooltip = Translations.Translate("FIF_POP_SCR");
            typeFilter.relativePosition = new Vector3(vanillaFilter.relativePosition.x + vanillaFilter.width, 5);

            if (FindIt.isRicoEnabled)
            {
                string[] items = {
                    Translations.Translate("FIF_SE_IA"), // All
                    Translations.Translate("FIF_SE_IN"), // Network
                    Translations.Translate("FIF_SE_IP"), // Ploppable
                    Translations.Translate("FIF_SE_IG"), // Growable
                    Translations.Translate("FIF_SE_IR"), // RICO
                    Translations.Translate("FIF_SE_IGR"), // Growable/RICO
                    Translations.Translate("FIF_SE_IPR"), // Prop
                    Translations.Translate("FIF_SE_ID"), // Decal
                    Translations.Translate("FIF_SE_IT") // Tree
                };
                typeFilter.items = items;
            }
            else
            {
                string[] items = {
                    Translations.Translate("FIF_SE_IA"), // All
                    Translations.Translate("FIF_SE_IN"), // Network
                    Translations.Translate("FIF_SE_IP"), // Ploppable
                    Translations.Translate("FIF_SE_IG"), // Growable
                    Translations.Translate("FIF_SE_IPR"), // Prop
                    Translations.Translate("FIF_SE_ID"), // Decal
                    Translations.Translate("FIF_SE_IT") // Tree
                };
                typeFilter.items = items;
            }
            typeFilter.selectedIndex = 0;

            typeFilter.eventSelectedIndexChanged += (c, p) =>
            {
                UpdateFilterPanels();
                Search();
            };
            
            // building filters panel
            buildingFilters = filterPanel.AddUIComponent<UIPanel>();
            buildingFilters.size = new Vector2(90, 35);
            buildingFilters.relativePosition = new Vector3(typeFilter.relativePosition.x + typeFilter.width, 0);

            // building level filter
            UILabel levelLabel = buildingFilters.AddUIComponent<UILabel>();
            levelLabel.textScale = 0.8f;
            levelLabel.padding = new RectOffset(0, 0, 8, 0);
            levelLabel.text = Translations.Translate("FIF_SE_LV");
            levelLabel.relativePosition = new Vector3(10, 5);

            levelFilter = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            levelFilter.size = new Vector2(55, 25);
            levelFilter.AddItem(Translations.Translate("FIF_SE_IA"));
            levelFilter.AddItem("1");
            levelFilter.AddItem("2");
            levelFilter.AddItem("3");
            levelFilter.AddItem("4");
            levelFilter.AddItem("5");
            levelFilter.selectedIndex = 0;
            levelFilter.relativePosition = new Vector3(levelLabel.relativePosition.x + levelLabel.width + 5, 5);

            levelFilter.eventSelectedIndexChanged += (c, i) => Search();

            // building size filter
            UILabel sizeLabel = buildingFilters.AddUIComponent<UILabel>();
            sizeLabel.textScale = 0.8f;
            sizeLabel.padding = new RectOffset(0, 0, 8, 0);
            sizeLabel.text = Translations.Translate("FIF_SE_SZ");
            sizeLabel.relativePosition = new Vector3(levelFilter.relativePosition.x + levelFilter.width + 10, 5);

            sizeFilterX = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            sizeFilterX.size = new Vector2(55, 25);
            sizeFilterX.items = filterItemsGrowable;
            sizeFilterX.selectedIndex = 0;
            sizeFilterX.relativePosition = new Vector3(sizeLabel.relativePosition.x + sizeLabel.width + 5, 5);

            sizeFilterY = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            sizeFilterY.size = new Vector2(55, 25);
            sizeFilterY.items = filterItemsGrowable;
            sizeFilterY.selectedIndex = 0;
            sizeFilterY.relativePosition = new Vector3(sizeFilterX.relativePosition.x + sizeFilterX.width + 10, 5);
            buildingFilters.width = sizeFilterY.relativePosition.x + sizeFilterY.width;

            sizeFilterX.eventSelectedIndexChanged += (c, i) => Search();
            sizeFilterY.eventSelectedIndexChanged += (c, i) => Search();

            // tool icon panel
            toolIconPanel = filterPanel.AddUIComponent<UIPanel>();
            toolIconPanel.size = new Vector2(95, 35);
            toolIconPanel.relativePosition = new Vector3(typeFilter.relativePosition.x + typeFilter.width, 0);

            // change custom tag panel visibility
            tagToolIcon = toolIconPanel.AddUIComponent<UISprite>();
            tagToolIcon.size = new Vector2(26, 21);
            tagToolIcon.atlas = FindIt.atlas;
            tagToolIcon.spriteName = "Tag";
            tagToolIcon.tooltip = Translations.Translate("FIF_SE_SCTP");
            tagToolIcon.opacity = 0.5f;
            tagToolIcon.relativePosition = new Vector3(7, 7);
            tagToolIcon.eventClicked += (c, p) =>
            {
                UpdateTagPanel();
                if (tagPanel.isVisible)
                {
                    tagToolIcon.opacity = 1.0f;
                }
                else
                {
                    tagToolIcon.opacity = 0.5f;
                }
            };

            // change extra filters panel visibility
            extraFiltersIcon = toolIconPanel.AddUIComponent<UISprite>();
            extraFiltersIcon.size = new Vector2(26, 23);
            extraFiltersIcon.atlas = FindIt.atlas;
            extraFiltersIcon.spriteName = "ExtraFilters";
            extraFiltersIcon.tooltip = Translations.Translate("FIF_SE_EFI");
            extraFiltersIcon.opacity = 0.5f;
            extraFiltersIcon.relativePosition = new Vector3(tagToolIcon.relativePosition.x + tagToolIcon.width + 5, 6);
            extraFiltersIcon.eventClicked += (c, p) =>
            {
                UpdateExtraFiltersPanel();
                if (extraFiltersPanel.isVisible)
                {
                    extraFiltersIcon.opacity = 1.0f;
                }
                else
                {
                    extraFiltersIcon.opacity = 0.5f;
                }
            };

            // panel of sort button and filter toggle tabs
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            panel.backgroundSprite = "GenericTabHovered";
            panel.size = new Vector2(parent.width, 45);
            panel.relativePosition = new Vector3(0, -panel.height + 5);

            // sort button
            sortButton = SamsamTS.UIUtils.CreateButton(panel);
            sortButton.size = new Vector2(100, 35);
            sortButton.text = Translations.Translate("FIF_SO_RE");
            sortButton.tooltip = Translations.Translate("FIF_SO_RETP");
            sortButton.relativePosition = new Vector3(5, 5);

            sortButton.eventClick += (c, p) =>
            {
                if (sortButtonTextState)
                {
                    sortButton.text = Translations.Translate("FIF_SO_NE");
                    sortButtonTextState = false;
                    sortButton.tooltip = Translations.Translate("FIF_SO_NETP");
                }
                else
                {
                    sortButton.text = Translations.Translate("FIF_SO_RE");
                    sortButtonTextState = true;
                    sortButton.tooltip = Translations.Translate("FIF_SO_RETP");
                }
                Search();
            };

            // panel of custom tags
            tagPanel = AddUIComponent<UIFilterTag>();
            tagPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            tagPanel.backgroundSprite = "GenericTab";
            tagPanel.isVisible = false;
            tagPanel.size = new Vector2(670, 35);
            tagPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height - 40 - 5);

            // panel of extra filters
            extraFiltersPanel = AddUIComponent<UIFilterExtra>();
            extraFiltersPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            extraFiltersPanel.backgroundSprite = "GenericTab";
            extraFiltersPanel.isVisible = false;
            extraFiltersPanel.size = new Vector2(670, 35);
            extraFiltersPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height - 40 - 5);

            // ploppable filter tabs
            filterPloppable = panel.AddUIComponent<UIFilterPloppable>();
            filterPloppable.isVisible = false;
            filterPloppable.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);
            filterPloppable.eventFilteringChanged += (c,p) => Search();

            // growable filter tabs
            filterGrowable = panel.AddUIComponent<UIFilterGrowable>();
            filterGrowable.isVisible = false;
            filterGrowable.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);
            filterGrowable.eventFilteringChanged += (c, p) => Search();

            // prop filter tabs
            filterProp = panel.AddUIComponent<UIFilterProp>();
            filterProp.isVisible = false;
            filterProp.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);
            filterProp.eventFilteringChanged += (c, p) => Search();

            // tree filter tabs
            filterTree = panel.AddUIComponent<UIFilterTree>();
            filterTree.isVisible = false;
            filterTree.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);
            filterTree.eventFilteringChanged += (c, p) => Search();

            UpdateFilterPanels();

            size = Vector2.zero;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input != null && !isVisible)
            {
                input.Unfocus();
            }
        }

        /// <summary>
        /// Change the visibility of building level and size filters.
        /// Also change filterPanel width
        /// </summary>
        public void UpdateBuildingFilters()
        {
            try
            {
                if (buildingFilters.isVisible)
                {
                    filterPanel.width = buildingFilters.relativePosition.x + buildingFilters.width + 5;
                }
                else
                {
                    filterPanel.width = toolIconPanel.relativePosition.x + toolIconPanel.width + 5;
                }
            }
            catch(Exception e)
            {
                Debugging.Message("UpdateBuildingFilters exception");
                Debugging.LogException(e);
            }
        }

        /// <summary>
        /// Change the visibility of filter tabs and some other UI components in searchbox
        /// </summary>
        public void UpdateFilterPanels()
        {
            //SimulationManager.instance.AddAction(() =>
            //{
                int index = typeFilter.selectedIndex;
                if (!FindIt.isRicoEnabled && index >= (int)DropDownOptions.Rico)
                {
                    index+=2;
                }

                switch ((DropDownOptions)index)
                {
                    case DropDownOptions.Ploppable:
                        HideFilterPanel(filterGrowable);
                        HideFilterPanel(filterProp);
                        HideFilterPanel(filterTree);
                        HideBuildingFilters();
                        toolIconPanel.isVisible = true;
                        ShowFilterPanel(filterPloppable);
                        break;
                    case DropDownOptions.Rico:
                        sizeFilterX.items = filterItemsRICO;
                        sizeFilterY.items = filterItemsRICO;
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterProp);
                        HideFilterPanel(filterTree);
                        toolIconPanel.isVisible = false;
                        ShowFilterPanel(filterGrowable);
                        ShowBuildingFilters();
                        break;
                    case DropDownOptions.GrwbRico:
                        sizeFilterX.items = filterItemsRICO;
                        sizeFilterY.items = filterItemsRICO;
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterProp);
                        HideFilterPanel(filterTree);
                        toolIconPanel.isVisible = false;
                        ShowFilterPanel(filterGrowable);
                        ShowBuildingFilters();
                        break;
                    case DropDownOptions.Growable:
                        sizeFilterX.items = filterItemsGrowable;
                        sizeFilterY.items = filterItemsGrowable;
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterProp);
                        HideFilterPanel(filterTree);
                        toolIconPanel.isVisible = false;
                        ShowFilterPanel(filterGrowable);
                        ShowBuildingFilters();
                        break;
                    case DropDownOptions.Prop:
                        HideFilterPanel(filterGrowable);
                        HideFilterPanel(filterPloppable);
                        HideBuildingFilters();
                        HideFilterPanel(filterTree);
                        toolIconPanel.isVisible = true;
                        ShowFilterPanel(filterProp);
                        break;
                    case DropDownOptions.Tree:
                        HideFilterPanel(filterGrowable);
                        HideFilterPanel(filterPloppable);
                        HideBuildingFilters();
                        HideFilterPanel(filterProp);
                        toolIconPanel.isVisible = true;
                        ShowFilterPanel(filterTree);
                        break;
                    default: // All, Network
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterGrowable);
                        HideFilterPanel(filterProp);
                        HideFilterPanel(filterTree);
                        HideBuildingFilters();
                        toolIconPanel.isVisible = true;
                        break;
                }
           // });
        }

        public void ShowFilterPanel(UIPanel panel)
        {
            panel.isVisible = true;
        }

        public void HideFilterPanel(UIPanel panel)
        {
            panel.isVisible = false;
        }

        public void ShowBuildingFilters()
        {
            buildingFilters.isVisible = true;
            UpdateBuildingFilters();
        }

        public void HideBuildingFilters()
        {
            buildingFilters.isVisible = false;
            UpdateBuildingFilters();
        }

        public void UpdateTagPanel()
        {
            tagPanel.isVisible = !tagPanel.isVisible;
            if(!tagPanel.isVisible) tagPanel.tagDropDownCheckBox.isChecked = false;
            UpdateTopPanelsPosition();
            tagPanel.UpdateCustomTagList();
        }

        public void UpdateExtraFiltersPanel()
        {
            extraFiltersPanel.isVisible = !extraFiltersPanel.isVisible;
            if (!extraFiltersPanel.isVisible) extraFiltersPanel.optionDropDownCheckBox.isChecked = false;
            UpdateTopPanelsPosition();
        }

        public void UpdateTopPanelsPosition()
        {
            if(extraFiltersPanel.isVisible && tagPanel.isVisible)
            {
                tagPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height - 40 - 5);
                extraFiltersPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height*2 - 40 - 5*2);
            }
            else if (extraFiltersPanel.isVisible && !tagPanel.isVisible)
            {
                extraFiltersPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height - 40 - 5);
            }
            else if (!extraFiltersPanel.isVisible && tagPanel.isVisible)
            {
                tagPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height - 40 - 5);
            }
        }

        /// <summary>
        /// Reset all filters. This mehtod will be used by Quboid's Picker mod through reflection.
        /// Don't remove this method. Update this method whenever a new filter is added.
        /// </summary>
        public void ResetFilters()
        {
            vanillaFilter.isChecked = true;
            workshopFilter.isChecked = true;
            levelFilter.selectedIndex = 0;
            sizeFilterX.selectedIndex = 0;
            sizeFilterY.selectedIndex = 0;

            UIFilterTag.instance.tagDropDownCheckBox.isChecked = false;
            UIFilterExtra.instance.optionDropDownCheckBox.isChecked = false;

            filterGrowable.SelectAll();
            filterPloppable.SelectAll();
            filterProp.SelectAll();
            filterTree.SelectAll();
        }

        public void Search()
        {
            PrefabInfo current = null;
            UIScrollPanelItem.ItemData selected = null;
            if (scrollPanel.selectedItem != null)
            {
                current = scrollPanel.selectedItem.asset.prefab;
            }

            string text = "";
            DropDownOptions type = DropDownOptions.All;

            if (input != null)
            {
                text = input.text;
                type = (DropDownOptions)typeFilter.selectedIndex;

                if (!FindIt.isRicoEnabled && type >= DropDownOptions.Rico)
                {
                    type+=2;
                }
            }

            // extra size check for growable
            if (type == DropDownOptions.Growable)
            {
                // if switch back from rico with size > 4, default size = all
                if (UISearchBox.instance.buildingSizeFilterIndex.x > 4) UISearchBox.instance.sizeFilterX.selectedIndex = 0;
                if (UISearchBox.instance.buildingSizeFilterIndex.y > 4) UISearchBox.instance.sizeFilterY.selectedIndex = 0;
            }
            List<Asset> matches = AssetTagList.instance.Find(text, type);

            // sort by most recently downloaded
            if (sortButtonTextState == false)
            {
                matches = matches.OrderByDescending(s => s.downloadTime).ToList();
            }
            // sort by relevance, same as original Find It
            else
            {
                text = text.ToLower().Trim();
                // if search input box is not empty, sort by score
                if (!text.IsNullOrWhiteSpace()) matches = matches.OrderByDescending(s => s.score).ToList();
                // if seach input box is empty, sort by asset title
                else matches = matches.OrderBy(s => s.title).ToList();
            }

            scrollPanel.Clear();
            foreach (Asset asset in matches)
            {
                if (asset.prefab != null)
                {
                    // I tried to do this directly in AssetTagList.Find()
                    // however I got some UI glitches and was not able to solve it
                    if (workshopFilter != null && vanillaFilter != null && tagPanel != null && extraFiltersPanel != null)
                    {
                        // filter out custom asset
                        if (asset.prefab.m_isCustomContent && !workshopFilter.isChecked) continue;

                        // filter out vanilla asset. will not filter out content creater pack assets
                        if (!asset.prefab.m_isCustomContent && !vanillaFilter.isChecked && !asset.isCCPBuilding) continue;

                        // filter out assets without matching custom tag
                        if (tagPanel.tagDropDownCheckBox.isChecked && tagPanel.customTagListStrArray.Length > 0)
                        {
                            if (!asset.tagsCustom.Contains(tagPanel.GetDropDownListKey())) continue;
                        }

                        // extra filters check
                        if (extraFiltersPanel.optionDropDownCheckBox.isChecked)
                        {
                            // filter asset by asset creator
                            if (extraFiltersPanel.optionDropDownMenu.selectedIndex == 0)
                            {
                                if (asset.author != extraFiltersPanel.GetAssetCreatorDropDownListKey()) continue;
                            }
                            // filter asset by building height
                            else
                            {
                                if (asset.assetType == Asset.AssetType.Ploppable || asset.assetType == Asset.AssetType.Rico || asset.assetType == Asset.AssetType.Growable)
                                {
                                    if (asset.buildingHeight > extraFiltersPanel.maxBuildingHeight) continue;
                                    if (asset.buildingHeight < extraFiltersPanel.minBuildingHeight) continue;
                                }
                                else continue;
                            }
                        }
                    }

                    UIScrollPanelItem.ItemData data = new UIScrollPanelItem.ItemData();
                    data.name = asset.title;// + "___" + asset.prefab.editorCategory;
                    data.tooltip = Asset.GetLocalizedTooltip(asset.prefab, data.name);

                    data.tooltipBox = GeneratedPanel.GetTooltipBox(TooltipHelper.GetHashCode(data.tooltip));
                    data.asset = asset;

                    scrollPanel.itemsData.Add(data);

                    if (asset.prefab == current)
                    {
                        selected = data;
                    }
                }
            }

            scrollPanel.DisplayAt(0);
            scrollPanel.selectedItem = selected;

            if (scrollPanel.selectedItem != null)
            {

                FindIt.SelectPrefab(scrollPanel.selectedItem.asset.prefab);
            }
            else
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
    }
}
