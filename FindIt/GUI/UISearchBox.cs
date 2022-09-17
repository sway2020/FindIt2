// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// main UI class

using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FindIt.GUI
{
    // Check Picker mod's code before changing the accessibility of some data members to private, or it may break Picker mod's current reflection code
    public class UISearchBox : UIPanel
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static UISearchBox instance;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public UIPanel inputPanel;
        public UITextField input;
        public UIScrollPanel scrollPanel;
        public UIPanel panel;
        public UISprite searchIcon;
        public UISprite clearButton;

        /// <summary>
        /// Also manipulated by the Picker mod. Don't change its accessibility. 
        /// Need to notify Quboid if a new dropdown item is added, or the item order is changed
        /// </summary>
        public UIDropDown typeFilter;

        public UILabel sizeLabel;
        public UIDropDown sizeFilterX;
        public UIDropDown sizeFilterY;
        private UIFilterGrowable filterGrowable;
        private UIFilterPloppable filterPloppable;
        private UIFilterProp filterProp;
        private UIFilterTree filterTree;
        private UIFilterNetwork filterNetwork;
        private UIFilterDecal filterDecal;

        public UICheckBox workshopFilter;
        public UICheckBox vanillaFilter;
        private UICheckBox sortToggle;

        private UISprite refreshDisplayIcon;
        private UISprite locateInstanceIcon;

        private UISprite tagToolIcon;
        public UIFilterTagPanel tagPanel;
        private UISprite extraFiltersIcon;
        public UIFilterExtraPanel extraFiltersPanel;
        public UISprite quickMenuIcon;
        public bool quickMenuVisible;

        public UIAssetTypePanel assetTypePanel;
        public UISearchTabPanel searchTabPanel;

        public List<Asset> matches;
        public List<string> searchResultList = new List<string>();

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
        private readonly string[] filterItemsGrowable = { Translations.Translate("FIF_SE_IA"), "1", "2", "3", "4" };
        private readonly string[] filterItemsRICO = { Translations.Translate("FIF_SE_IA"), "1", "2", "3", "4", "5-8", "9-12", "13+" };

        public Vector2 BuildingSizeFilterIndex
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
            UnityEngine.Random.InitState(System.Environment.TickCount);

            // panel for search input box, type filter and building filters
            inputPanel = AddUIComponent<UIPanel>();
            inputPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            inputPanel.backgroundSprite = "GenericTab";
            inputPanel.color = new Color32(196, 200, 206, 255);
            inputPanel.size = new Vector2(parent.width, 35);
            inputPanel.relativePosition = new Vector2(0, -inputPanel.height - 40);

            // search input box
            input = SamsamTS.UIUtils.CreateTextField(inputPanel);
            input.size = new Vector2(250, 28);
            input.padding.top = 6;
            input.relativePosition = new Vector3(5, 4);

            string search = null;
            input.eventTextChanged += (c, p) =>
            {
                if (Settings.disableInstantSearch && p != "") return;

                // change icon of the selected search tab 
                UISearchTabPanel.instance.GetSelectedTab().ChangeTabLabel(p);

                search = p;
                Search();
            };

            input.eventTextCancelled += (c, p) =>
            {
                if (search != null) input.text = search;
            };

            input.eventKeyDown += (component, eventParam) =>
            {
                if (eventParam.keycode == KeyCode.DownArrow || eventParam.keycode == KeyCode.UpArrow)
                {
                    if (typeFilter != null)
                    {
                        typeFilter.selectedIndex = Mathf.Clamp(typeFilter.selectedIndex + (eventParam.keycode == KeyCode.DownArrow ? 1 : -1), 0, typeFilter.items.Length);
                    }
                }
            };

            // if instant search is disabled, press ENTER to make a new search
            input.eventTextSubmitted += (c, p) =>
            {
                if (!Settings.disableInstantSearch) return;

                if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
                {
                    // change icon of the selected search tab 
                    UISearchTabPanel.instance.GetSelectedTab().ChangeTabLabel(p);

                    if (search != p && !input.hasFocus) input.Focus();
                    search = p;
                    Search();
                }
            };

            // search icon. Click to export search results
            searchIcon = inputPanel.AddUIComponent<UISprite>();
            searchIcon.size = new Vector2(25, 30);
            searchIcon.atlas = FindIt.atlas;
            searchIcon.spriteName = "FindItDisabled";
            searchIcon.relativePosition = new Vector3(5, 4);
            searchIcon.tooltip = Translations.Translate("FIF_SE_ICONTP");
            searchIcon.eventMouseEnter += (c, p) =>
            {
                searchIcon.spriteName = "FindItFocused";
            };

            searchIcon.eventMouseLeave += (c, p) =>
            {
                searchIcon.spriteName = "FindItDisabled";
            };
            searchIcon.eventClicked += (c, p) =>
            {
                UIExportSearchResultsPopUp.ShowAt(searchIcon);
            };

            // clear search box
            clearButton = inputPanel.AddUIComponent<UISprite>();
            clearButton.size = new Vector2(26, 26);
            clearButton.atlas = FindIt.atlas;
            clearButton.spriteName = "Clear";
            clearButton.tooltip = Translations.Translate("FIF_SE_SEBTP");
            clearButton.opacity = 0.5f;
            clearButton.relativePosition = new Vector3(input.relativePosition.x + input.width + 3, 4);
            clearButton.eventClicked += (c, p) =>
            {
                input.text = "";
                //PickerRandomTest();
            };
            clearButton.eventMouseEnter += (c, p) =>
            {
                clearButton.opacity = 0.9f;
            };

            clearButton.eventMouseLeave += (c, p) =>
            {
                clearButton.opacity = 0.5f;
            };

            // asset type filter. Also Manipulated by the Picker mod through reflection.
            // Need to notify Quboid if a new dropdown item is added, or the item order is changed
            typeFilter = SamsamTS.UIUtils.CreateDropDown(inputPanel);
            typeFilter.name = "FindIt_AssetTypeFilter";
            typeFilter.size = new Vector2(105, 25);
            typeFilter.tooltip = Translations.Translate("FIF_POP_SCR");
            typeFilter.relativePosition = new Vector3(clearButton.relativePosition.x + clearButton.width + 7, 5);

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

                // change icon of the selected search tab 
                UISearchTabPanel.instance.GetSelectedTab().ChangeTabIcon(typeFilter.selectedIndex);

                Search();
            };

            // workshop filter checkbox (custom assets saved in local asset folder are also included)
            workshopFilter = SamsamTS.UIUtils.CreateCheckBox(inputPanel);
            workshopFilter.isChecked = Settings.useWorkshopFilter;
            workshopFilter.width = 80;
            workshopFilter.label.text = Translations.Translate("FIF_SE_WF");
            workshopFilter.label.textScale = 0.8f;
            workshopFilter.relativePosition = new Vector3(typeFilter.relativePosition.x + typeFilter.width + 12, 10);
            workshopFilter.eventCheckChanged += (c, i) =>
            {
                Settings.useWorkshopFilter = workshopFilter.isChecked;
                XMLUtils.SaveSettings();
                Search();
            };

            // vanilla filter checkbox
            vanillaFilter = SamsamTS.UIUtils.CreateCheckBox(inputPanel);
            vanillaFilter.isChecked = Settings.useVanillaFilter;
            vanillaFilter.width = 80;
            vanillaFilter.label.text = Translations.Translate("FIF_SE_VF");
            vanillaFilter.label.textScale = 0.8f;
            vanillaFilter.relativePosition = new Vector3(workshopFilter.relativePosition.x + workshopFilter.width, 10);
            vanillaFilter.eventCheckChanged += (c, i) =>
            {
                Settings.useVanillaFilter = vanillaFilter.isChecked;
                XMLUtils.SaveSettings();
                Search();
            };

            // Refresh Display
            refreshDisplayIcon = inputPanel.AddUIComponent<UISprite>();
            refreshDisplayIcon.size = new Vector2(26, 22);
            refreshDisplayIcon.atlas = FindIt.atlas;
            refreshDisplayIcon.spriteName = "Refresh";
            refreshDisplayIcon.tooltip = Translations.Translate("FIF_REF_DIS");
            refreshDisplayIcon.opacity = 0.45f;
            refreshDisplayIcon.relativePosition = new Vector3(vanillaFilter.relativePosition.x + vanillaFilter.width + 5, 6.5f);
            refreshDisplayIcon.eventClicked += (c, p) =>
            {
                AssetTagList.instance.UpdatePrefabInstanceCount(DropDownOptions.All);
                if (FindIt.isPOEnabled && Settings.includePOinstances) ProceduralObjectsTool.UpdatePOInfoList();
                UISearchBox.instance.scrollPanel.Refresh();
            };

            refreshDisplayIcon.eventMouseEnter += (c, p) =>
            {
                refreshDisplayIcon.opacity = 0.9f;
            };

            refreshDisplayIcon.eventMouseLeave += (c, p) =>
            {
                refreshDisplayIcon.opacity = 0.45f;
            };

            // Locate Instance
            LocateNextInstanceTool.Initialize();
            locateInstanceIcon = inputPanel.AddUIComponent<UISprite>();
            locateInstanceIcon.size = new Vector2(26, 23);
            locateInstanceIcon.atlas = FindIt.atlas;
            locateInstanceIcon.spriteName = "Locate";
            locateInstanceIcon.tooltip = Translations.Translate("FIF_LOC_TOOL");
            locateInstanceIcon.opacity = 0.45f;
            locateInstanceIcon.relativePosition = new Vector3(refreshDisplayIcon.relativePosition.x + refreshDisplayIcon.width + 4, 5.5f);
            locateInstanceIcon.eventClicked += (c, p) =>
            {
                Event e = Event.current;
                if (e.shift)
                {
                    LocateNextInstanceTool.LocateNextInstance(true); // find PO instance
                }
                else
                {
                    LocateNextInstanceTool.LocateNextInstance(false); // find normal asset instance
                }
            };


            locateInstanceIcon.eventMouseEnter += (c, p) =>
            {
                locateInstanceIcon.opacity = 1.0f;
            };

            locateInstanceIcon.eventMouseLeave += (c, p) =>
            {
                locateInstanceIcon.opacity = 0.45f;
            };

            // change custom tag panel visibility
            tagToolIcon = inputPanel.AddUIComponent<UISprite>();
            tagToolIcon.size = new Vector2(26, 21);
            tagToolIcon.atlas = FindIt.atlas;
            tagToolIcon.spriteName = "Tag";
            tagToolIcon.tooltip = Translations.Translate("FIF_SE_SCTP");
            tagToolIcon.opacity = 0.5f;
            tagToolIcon.relativePosition = new Vector3(locateInstanceIcon.relativePosition.x + locateInstanceIcon.width + 3.5f, 7);
            tagToolIcon.eventClicked += (c, p) =>
            {
                if (!tagPanel.isVisible)
                {
                    OpenCustomTagPanel();
                }
                else
                {
                    CloseCustomTagPanel();
                }
            };

            tagToolIcon.eventMouseEnter += (c, p) =>
            {
                tagToolIcon.opacity = 1.0f;
            };

            tagToolIcon.eventMouseLeave += (c, p) =>
            {
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
            extraFiltersIcon = inputPanel.AddUIComponent<UISprite>();
            extraFiltersIcon.size = new Vector2(26, 23);
            extraFiltersIcon.atlas = FindIt.atlas;
            extraFiltersIcon.spriteName = "ExtraFilters";
            extraFiltersIcon.tooltip = Translations.Translate("FIF_SE_EFI");
            extraFiltersIcon.opacity = 0.5f;
            extraFiltersIcon.relativePosition = new Vector3(tagToolIcon.relativePosition.x + tagToolIcon.width + 4, 6);

            extraFiltersIcon.eventClicked += (c, p) =>
            {
                if (!extraFiltersPanel.isVisible)
                {
                    OpenExtraFiltersPanel();
                }
                else
                {
                    CloseExtraFiltersPanel();
                }
            };

            extraFiltersIcon.eventMouseEnter += (c, p) =>
            {
                extraFiltersIcon.opacity = 1.0f;
            };

            extraFiltersIcon.eventMouseLeave += (c, p) =>
            {
                if (extraFiltersPanel.isVisible)
                {
                    extraFiltersIcon.opacity = 1.0f;
                }
                else
                {
                    extraFiltersIcon.opacity = 0.5f;
                }
            };

            quickMenuIcon = inputPanel.AddUIComponent<UISprite>();
            quickMenuIcon.size = new Vector2(26, 23);
            quickMenuIcon.atlas = FindIt.atlas;
            quickMenuIcon.spriteName = "QuickMenu";
            quickMenuIcon.tooltip = Translations.Translate("FIF_QM_TIT");
            quickMenuIcon.opacity = 0.5f;
            quickMenuIcon.relativePosition = new Vector3(extraFiltersIcon.relativePosition.x + extraFiltersIcon.width + 4, 6);
            quickMenuIcon.eventClicked += (c, p) =>
            {
                UIQuickMenuPopUp.ShowAt(quickMenuIcon);
                quickMenuVisible = true;
                quickMenuIcon.opacity = 1.0f;
            };
            quickMenuIcon.eventMouseEnter += (c, p) =>
            {
                quickMenuIcon.opacity = 1.0f;
            };

            quickMenuIcon.eventMouseLeave += (c, p) =>
            {
                if (quickMenuVisible)
                {
                    quickMenuIcon.opacity = 1.0f;
                }
                else
                {
                    quickMenuIcon.opacity = 0.5f;
                }
            };

            // building size filter
            sizeFilterX = SamsamTS.UIUtils.CreateDropDown(inputPanel);
            sizeFilterX.size = new Vector2(55, 25);
            sizeFilterX.items = filterItemsGrowable;
            sizeFilterX.selectedIndex = 0;
            sizeFilterX.relativePosition = new Vector3(quickMenuIcon.relativePosition.x + quickMenuIcon.width + 9, 5);

            sizeLabel = inputPanel.AddUIComponent<UILabel>();
            sizeLabel.textScale = 0.8f;
            sizeLabel.padding = new RectOffset(0, 0, 8, 0);
            sizeLabel.text = "x";
            sizeLabel.tooltip = Translations.Translate("FIF_SE_SIZEX");
            sizeLabel.relativePosition = new Vector3(sizeFilterX.relativePosition.x + sizeFilterX.width + 3.5f, 5);

            sizeLabel.eventClick += (c, p) =>
            {
                // avoid triggering duplicate searches
                searchEnabled = false;

                sizeFilterX.selectedIndex = 0;
                sizeFilterY.selectedIndex = 0;

                searchEnabled = true;
                Search();
            };

            sizeFilterY = SamsamTS.UIUtils.CreateDropDown(inputPanel);
            sizeFilterY.size = new Vector2(55, 25);
            sizeFilterY.items = filterItemsGrowable;
            sizeFilterY.selectedIndex = 0;
            sizeFilterY.relativePosition = new Vector3(sizeLabel.relativePosition.x + sizeLabel.width + 2f, 5);

            sizeFilterX.eventSelectedIndexChanged += (c, i) => Search();
            sizeFilterY.eventSelectedIndexChanged += (c, i) => Search();

            // panel of sort button and filter toggle tabs
            panel = AddUIComponent<UIPanel>();
            panel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            if (!Settings.useLightBackground) panel.backgroundSprite = "GenericTabHovered";
            else panel.backgroundSprite = "GenericTab";
            panel.size = new Vector2(parent.width, 45);
            panel.relativePosition = new Vector3(0, -panel.height + 5);

            // sort checkbox toggle tab
            sortToggle = CreateSortToggle(panel);
            sortToggle.relativePosition = new Vector3(5, 5);

            // ploppable filter tabs
            filterPloppable = panel.AddUIComponent<UIFilterPloppable>();
            filterPloppable.isVisible = false;
            filterPloppable.relativePosition = new Vector3(sortToggle.relativePosition.x + sortToggle.width, 0);
            filterPloppable.eventFilteringChanged += (c, p) => Search();

            // growable filter tabs
            filterGrowable = panel.AddUIComponent<UIFilterGrowable>();
            filterGrowable.isVisible = false;
            filterGrowable.relativePosition = new Vector3(sortToggle.relativePosition.x + sortToggle.width, 0);
            filterGrowable.eventFilteringChanged += (c, p) => Search();

            // prop filter tabs
            filterProp = panel.AddUIComponent<UIFilterProp>();
            filterProp.isVisible = false;
            filterProp.relativePosition = new Vector3(sortToggle.relativePosition.x + sortToggle.width, 0);
            filterProp.eventFilteringChanged += (c, p) => Search();

            // tree filter tabs
            filterTree = panel.AddUIComponent<UIFilterTree>();
            filterTree.isVisible = false;
            filterTree.relativePosition = new Vector3(sortToggle.relativePosition.x + sortToggle.width, 0);
            filterTree.eventFilteringChanged += (c, p) => Search();

            // network filter tabs
            filterNetwork = panel.AddUIComponent<UIFilterNetwork>();
            filterNetwork.isVisible = false;
            filterNetwork.relativePosition = new Vector3(sortToggle.relativePosition.x + sortToggle.width, 0);
            filterNetwork.eventFilteringChanged += (c, p) => Search();

            // decal filter tabs
            filterDecal = panel.AddUIComponent<UIFilterDecal>();
            filterDecal.isVisible = false;
            filterDecal.relativePosition = new Vector3(sortToggle.relativePosition.x + sortToggle.width, 0);

            UpdateFilterPanels();
            CreateAssetTypePanel();
            if (Settings.showAssetTypePanel) assetTypePanel.isVisible = true;
            CreateCustomTagPanel();
            CreateExtraFiltersPanel();
            CreateSearchTabPanel();

            size = Vector2.zero;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input != null && !isVisible)
            {
                input.Unfocus();
            }

            // do some initialization work when the UI is first shown
            if (!FindIt.instance.firstVisibleFlag && isVisible)
            {
                FindIt.instance.firstVisibleFlag = true;

                // show update notice
                if (!Settings.disableUpdateNotice && (ModInfo.updateNoticeDate > Settings.lastUpdateNotice))
                {
                    UIUpdateNoticePopUp.ShowAt();
                    UIUpdateNoticePopUp.instance.relativePosition += new Vector3(-200, -200);
                    Settings.lastUpdateNotice = ModInfo.updateNoticeDate;
                    XMLUtils.SaveSettings();
                }
            }

            // reset panel if the option is enabled
            else if (Settings.resetPanelWhenClosed && !isVisible)
            {
                // avoid triggering duplicate searches
                searchEnabled = false;

                input.text = "";
                typeFilter.selectedIndex = 0;
                workshopFilter.isChecked = true;
                vanillaFilter.isChecked = true;
                sizeFilterX.selectedIndex = 0;
                sizeFilterY.selectedIndex = 0;
                UIFilterProp.instance.SelectAll();
                UIFilterGrowable.instance.SelectAll();
                UIFilterNetwork.instance.SelectAll();
                UIFilterPloppable.instance.SelectAll();
                UIFilterTree.instance.SelectAll();

                searchEnabled = true;
                Search();
            }
        }

        /// <summary>
        /// Change the visibility of filter tabs and some other UI components in searchbox
        /// </summary>
        private void UpdateFilterPanels()
        {
            int index = typeFilter.selectedIndex;
            if (!FindIt.isRicoEnabled && index >= (int)DropDownOptions.Rico)
            {
                index += 2;
            }

            HideAllFilterTabs();
            if (UIAssetTypePanel.instance != null)
            {
                UIAssetTypePanel.instance.SetSelectedTab((DropDownOptions)index);
            }

            switch ((DropDownOptions)index)
            {
                case DropDownOptions.Ploppable:
                    sizeFilterX.items = filterItemsRICO;
                    sizeFilterY.items = filterItemsRICO;
                    ShowFilterPanel(filterPloppable);
                    ShowBuildingFilters();
                    break;
                case DropDownOptions.Rico:
                    sizeFilterX.items = filterItemsRICO;
                    sizeFilterY.items = filterItemsRICO;
                    ShowFilterPanel(filterGrowable);
                    ShowBuildingFilters();
                    break;
                case DropDownOptions.GrwbRico:
                    sizeFilterX.items = filterItemsRICO;
                    sizeFilterY.items = filterItemsRICO;
                    ShowFilterPanel(filterGrowable);
                    ShowBuildingFilters();
                    break;
                case DropDownOptions.Growable:
                    sizeFilterX.items = filterItemsGrowable;
                    sizeFilterY.items = filterItemsGrowable;
                    ShowFilterPanel(filterGrowable);
                    ShowBuildingFilters();
                    break;
                case DropDownOptions.Prop:
                    ShowFilterPanel(filterProp);
                    break;
                case DropDownOptions.Tree:
                    ShowFilterPanel(filterTree);
                    break;
                case DropDownOptions.Network:
                    ShowFilterPanel(filterNetwork);
                    break;
                case DropDownOptions.Decal:
                    ShowFilterPanel(filterDecal);
                    break;
                default: // All
                    break;
            }
        }

        private static void ShowFilterPanel(UIPanel panel)
        {
            panel.isVisible = true;
        }

        private static void HideFilterPanel(UIPanel panel)
        {
            panel.isVisible = false;
        }

        private void ShowBuildingFilters()
        {
            sizeFilterX.isVisible = true;
            sizeFilterY.isVisible = true;
            sizeLabel.isVisible = true;
            inputPanel.width = 859.0f;
        }

        private void HideBuildingFilters()
        {
            sizeFilterX.isVisible = false;
            sizeFilterY.isVisible = false;
            sizeLabel.isVisible = false;
            inputPanel.width = sizeFilterX.position.x;
        }

        private void CreateExtraFiltersPanel()
        {
            if (extraFiltersPanel != null) return;
            extraFiltersPanel = AddUIComponent<UIFilterExtraPanel>();
            extraFiltersPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            extraFiltersPanel.backgroundSprite = "GenericTab";
            extraFiltersPanel.color = new Color32(196, 200, 206, 255);
            extraFiltersPanel.isVisible = false;
            extraFiltersPanel.size = new Vector2(sizeFilterX.position.x, 35);
            extraFiltersPanel.relativePosition = new Vector2(0, -inputPanel.height - extraFiltersPanel.height - 40);
        }

        //private void DestroyExtraFiltersPanel()
        //{
        //    if (extraFiltersPanel == null) return;
        //    extraFiltersPanel.Close();
        //    RemoveUIComponent(extraFiltersPanel);
        //    extraFiltersPanel = null;
        //}

        public void OpenExtraFiltersPanel()
        {
            extraFiltersIcon.opacity = 1.0f;
            extraFiltersPanel.isVisible = true;
            extraFiltersPanel.optionDropDownCheckBox.isChecked = true;
            UpdateTopPanelsPosition();
        }

        public void CloseExtraFiltersPanel()
        {
            extraFiltersIcon.opacity = 0.5f;
            extraFiltersPanel.isVisible = false;
            extraFiltersPanel.optionDropDownCheckBox.isChecked = false;
            UpdateTopPanelsPosition();
        }

        private void CreateCustomTagPanel()
        {
            if (tagPanel != null) return;
            tagPanel = AddUIComponent<UIFilterTagPanel>();
            tagPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            tagPanel.backgroundSprite = "GenericTab";
            tagPanel.color = new Color32(196, 200, 206, 255);
            tagPanel.isVisible = false;
            tagPanel.size = new Vector2(sizeFilterX.position.x, 35);
            tagPanel.relativePosition = new Vector2(0, -inputPanel.height - tagPanel.height - 40);
        }

        //private void DestroyCustomTagPanel()
        //{
        //    if (tagPanel == null) return;
        //    tagPanel.Close();
        //    RemoveUIComponent(tagPanel);
        //    tagPanel = null;
        //    UISearchBox.instance.scrollPanel.Refresh();
        //}

        public void OpenCustomTagPanel()
        {
            tagToolIcon.opacity = 1.0f;
            tagPanel.isVisible = true;
            UpdateTopPanelsPosition();
        }

        public void CloseCustomTagPanel()
        {
            tagToolIcon.opacity = 0.5f;
            tagPanel.isVisible = false;
            tagPanel.tagDropDownCheckBox.isChecked = false;
            Search();
            UpdateTopPanelsPosition();
        }

        public void CreateAssetTypePanel()
        {
            if (assetTypePanel != null) return;
            assetTypePanel = AddUIComponent<UIAssetTypePanel>();
            assetTypePanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            assetTypePanel.backgroundSprite = "GenericTab";
            assetTypePanel.color = new Color32(196, 200, 206, 255);
            assetTypePanel.isVisible = false;
            assetTypePanel.size = new Vector2(75, 145);
            assetTypePanel.relativePosition = new Vector2(Settings.assetTypePanelX, Settings.assetTypePanelY);
        }

        public void DestroyAssetTypePanel()
        {
            if (assetTypePanel == null) return;
            assetTypePanel.Close();
            RemoveUIComponent(assetTypePanel);
            assetTypePanel = null;

            Settings.assetTypePanelX = -80.0f;
            Settings.assetTypePanelY = -75.0f;
            XMLUtils.SaveSettings();
        }

        private UICheckBox CreateSortToggle(UIComponent parent)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            float tabSize = 35f;
            string checkedSprite = "Relevance";
            string uncheckedSprite = "Recent";
            string atlas = "FindItAtlas";

            checkBox.width = tabSize;
            checkBox.height = tabSize;
            checkBox.clipChildren = true;
            checkBox.isChecked = true;
            checkBox.tooltip = Translations.Translate("FIF_SO_RETP");

            UIPanel panel = checkBox.AddUIComponent<UIPanel>();
            panel.atlas = SamsamTS.UIUtils.GetAtlas("FindItAtlas");
            panel.backgroundSprite = "SortToggle";
            panel.size = checkBox.size;
            panel.relativePosition = Vector3.zero;

            UISprite sprite = panel.AddUIComponent<UISprite>();
            sprite.atlas = SamsamTS.UIUtils.GetAtlas(atlas);

            sprite.spriteName = checkedSprite;
            sprite.size = checkBox.size;
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = sprite.atlas;
            ((UISprite)checkBox.checkedBoxObject).spriteName = checkedSprite;
            checkBox.checkedBoxObject.size = checkBox.size;
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.readOnly = true;
            checkBox.checkedBoxObject.isInteractive = false; // Don't eat my double click event please

            checkBox.eventClick += (c, p) =>
            {
                checkBox.isChecked = !checkBox.isChecked;
            };
            checkBox.eventCheckChanged += (c, b) =>
            {
                if (checkBox.isChecked)
                {
                    sprite.spriteName = checkedSprite;
                    Settings.useRelevanceSort = true;
                    XMLUtils.SaveSettings();
                    checkBox.tooltip = Translations.Translate("FIF_SO_RETP");
                }
                else
                {
                    sprite.spriteName = uncheckedSprite;
                    Settings.useRelevanceSort = false;
                    XMLUtils.SaveSettings();
                    checkBox.tooltip = Translations.Translate("FIF_SO_NETP");
                }
                panel.Invalidate();
                Search();
            };
            checkBox.eventMouseEnter += (c, p) =>
            {
                panel.opacity = 0.4f;
            };

            checkBox.eventMouseLeave += (c, p) =>
            {
                panel.opacity = 1.0f;
            };

            if (!Settings.useRelevanceSort)
            {
                checkBox.isChecked = false;
            }
            return checkBox;
        }

        private void CreateSearchTabPanel()
        {
            if (searchTabPanel != null) return;
            searchTabPanel = AddUIComponent<UISearchTabPanel>();
            searchTabPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            searchTabPanel.backgroundSprite = "GenericTab";
            searchTabPanel.color = new Color32(196, 200, 206, 255);
            searchTabPanel.isVisible = true;
            searchTabPanel.size = new Vector2(sizeFilterX.position.x, 35);
            searchTabPanel.relativePosition = new Vector2(0, 2 - inputPanel.height - searchTabPanel.height - 40);
        }

        private void UpdateTopPanelsPosition()
        {
            if (extraFiltersPanel.isVisible && tagPanel.isVisible)
            {
                tagPanel.relativePosition = new Vector2(0, 2 - inputPanel.height - tagPanel.height - 40);
                extraFiltersPanel.relativePosition = new Vector2(0, 4 - inputPanel.height - extraFiltersPanel.height - tagPanel.height - 40);
                searchTabPanel.relativePosition = new Vector2(0, 6 - inputPanel.height - extraFiltersPanel.height - tagPanel.height - searchTabPanel.height - 40);
            }
            else if (extraFiltersPanel.isVisible && !tagPanel.isVisible)
            {
                extraFiltersPanel.relativePosition = new Vector2(0, 2 - inputPanel.height - extraFiltersPanel.height - 40);
                searchTabPanel.relativePosition = new Vector2(0, 4 - inputPanel.height - extraFiltersPanel.height - searchTabPanel.height - 40);
            }
            else if (!extraFiltersPanel.isVisible && tagPanel.isVisible)
            {
                tagPanel.relativePosition = new Vector2(0, 2 - inputPanel.height - tagPanel.height - 40);
                searchTabPanel.relativePosition = new Vector2(0, 4 - inputPanel.height - tagPanel.height - searchTabPanel.height - 40);
            }
            else
            {
                searchTabPanel.relativePosition = new Vector2(0, 2 - inputPanel.height - searchTabPanel.height - 40);
            }
        }

        /// <summary>
        /// Reset all filters. This method is used by Quboid's Picker mod through reflection.
        /// Don't remove this method. Update this method whenever a new filter is added.
        /// </summary>
        public void ResetFilters()
        {
            // avoid triggering duplicate searches
            searchEnabled = false;

            input.text = "";

            vanillaFilter.isChecked = true;
            workshopFilter.isChecked = true;
            sizeFilterX.selectedIndex = 0;
            sizeFilterY.selectedIndex = 0;

            UIFilterTagPanel.instance.tagDropDownCheckBox.isChecked = false;
            UIFilterExtraPanel.instance.optionDropDownCheckBox.isChecked = false;

            filterGrowable.SelectAll();
            filterPloppable.SelectAll();
            filterProp.SelectAll();
            filterTree.SelectAll();
            filterNetwork.SelectAll();

            searchEnabled = true;
            Search();
        }

        public bool searchEnabled = true;
        public void Search()
        {
            // avoid triggering duplicate searches
            if (!searchEnabled) return;

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
                    type += 2;
                }
            }

            // set up prop categories for props generated by Elektrix's TVP mod. Need the TVP Patch mod
            if ((FindIt.isTVPPatchEnabled || FindIt.isTVP2Enabled) && !AssetTagList.instance.isTVPPatchModProcessed)
                AssetTagList.instance.SetTVPProps();

            // set up prop  for props generated by NTCPP mod. Need the Non-terrain Conforming Props mod
            if (FindIt.isNTCPEnabled && !AssetTagList.instance.isNTCPModProcessed)
                AssetTagList.instance.SetNTCPProps();

            // extra size check for growable
            if (type == DropDownOptions.Growable)
            {
                // if switch back from rico with size > 4, default size = all
                if (UISearchBox.instance.BuildingSizeFilterIndex.x > 4) UISearchBox.instance.sizeFilterX.selectedIndex = 0;
                if (UISearchBox.instance.BuildingSizeFilterIndex.y > 4) UISearchBox.instance.sizeFilterY.selectedIndex = 0;
            }

            matches = AssetTagList.instance.Find(text, type);

            // sort by used/unused instance count
            if (Settings.showInstancesCounter && Settings.instanceCounterSort != 0)
            {
                if (Settings.instanceCounterSort == 1)
                {
                    if (Settings.includePOinstances)
                    {
                        matches = matches.OrderByDescending(s => (s.instanceCount + s.poInstanceCount)).ToList();
                    }
                    else
                    {
                        matches = matches.OrderByDescending(s => s.instanceCount).ToList();
                    }
                }
                else
                {
                    if (Settings.includePOinstances)
                    {
                        matches = matches.OrderBy(s => (s.instanceCount + s.poInstanceCount)).ToList();
                    }
                    else
                    {
                        matches = matches.OrderBy(s => s.instanceCount).ToList();
                    }
                }
            }

            // sort by most recently downloaded
            else if (!Settings.useRelevanceSort)
            {
                matches = matches.OrderByDescending(s => s.downloadTime).ToList();
            }
            // sort by relevance, same as original Find It
            else
            {
                // sort network by ui priority instead
                if (UISearchBox.instance?.typeFilter.selectedIndex == 1)
                {
                    matches = matches.OrderBy(s => s.uiPriority).ToList();
                }
                else
                {
                    text = text.ToLower().Trim();
                    // if search input box is not empty, sort by score
                    if (!text.IsNullOrWhiteSpace())
                    {
                        float maxScore = 0;
                        foreach (Asset assetItr in matches)
                        {
                            if (assetItr.score > 0)
                            {
                                maxScore = assetItr.score;
                                break;
                            }
                        }
                        if (maxScore > 0) matches = matches.OrderByDescending(s => s.score).ToList();
                        else matches = matches.OrderBy(s => s.title).ToList();
                    }
                    // if seach input box is empty, sort by asset title
                    else matches = matches.OrderBy(s => s.title).ToList();
                }
            }

            scrollPanel.Clear();
            searchResultList.Clear();
            foreach (Asset asset in matches)
            {
                if (asset.prefab != null)
                {
#pragma warning disable IDE0017 // Simplify object initialization
                    UIScrollPanelItem.ItemData data = new UIScrollPanelItem.ItemData();
#pragma warning restore IDE0017 // Simplify object initialization
                    data.name = asset.title;// + "_" + asset.steamID;
                    data.tooltip = Asset.GetLocalizedTooltip(asset, asset.prefab, data.name);
                    data.tooltipBox = GeneratedPanel.GetTooltipBox(TooltipHelper.GetHashCode(data.tooltip));
                    data.asset = asset;

                    scrollPanel.itemsData.Add(data);
                    searchResultList.Add(data.name);
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

        /// <summary>
        /// Pick a random asset from recent search result
        /// </summary>
        public void PickRandom()
        {
            if (searchResultList.Count == 0) return;

            int index = UnityEngine.Random.Range(0, searchResultList.Count);
            string name = searchResultList.ElementAt(index);
            FindIt.instance.scrollPanel.DisplayAt(index);
            foreach (UIButton button in FindIt.instance.scrollPanel.GetComponentsInChildren<UIButton>())
            {
                if (button.name == name)
                {
                    button.SimulateClick();
                    break;
                }
            }
        }
        public void HideAllFilterTabs()
        {
            HideFilterPanel(filterGrowable);
            HideFilterPanel(filterPloppable);
            HideBuildingFilters();
            HideFilterPanel(filterProp);
            HideFilterPanel(filterTree);
            HideFilterPanel(filterNetwork);
            HideFilterPanel(filterDecal);
        }

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter
        private void OnTooltipClicked(UIComponent c, UIMouseEventParameter p)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (!p.used && p.buttons == UIMouseButton.Right)
            {
                input.text = "";
            }
        }

        /// <summary>
        /// Used by Quboid's Picker mod. Reset necessary filters and try to locate the asset.
        /// Return false if the asset can't be found
        /// </summary>
        public bool Picker(PrefabInfo info)
        {
            // check if the prefab exists in Find It's asset list
            Asset targetAsset = null;
            foreach (Asset asset in AssetTagList.instance.assets.Values)
            {
                if (asset.prefab == info)
                {
                    targetAsset = asset;
                    break;
                }
            }
            if (targetAsset == null)
            {
                // Debugging.Message("Picker - target doesn't exist in Find It 2's catalog");
                return false;
            }

            // avoid triggering duplicate searches
            searchEnabled = false;

            if (targetAsset.assetType == Asset.AssetType.Rico || targetAsset.assetType == Asset.AssetType.Growable
                || targetAsset.assetType == Asset.AssetType.Ploppable)
            {
                /*
                // set type drop-down
                if (targetAsset.assetType == Asset.AssetType.Growable) typeFilter.selectedIndex = (int)DropDownOptions.Growable;
                else typeFilter.selectedIndex = (int)DropDownOptions.Rico;
                */

                // set building size filter
                if (sizeFilterX.selectedIndex != 0) // if not 'all'
                {
                    if (!AssetTagList.CheckBuildingSizeXY(targetAsset.size.x, BuildingSizeFilterIndex.x)) // if wrong size option
                    {
                        sizeFilterX.selectedIndex = 0;
                    }
                }
                if (sizeFilterY.selectedIndex != 0) // if not 'all'
                {
                    if (!AssetTagList.CheckBuildingSizeXY(targetAsset.size.y, BuildingSizeFilterIndex.y)) // if wrong size option
                    {
                        sizeFilterY.selectedIndex = 0;
                    }
                }

                // select filter tab
                BuildingInfo buildingInfo = targetAsset.prefab as BuildingInfo;
                if (buildingInfo == null)
                {
                    searchEnabled = true;
                    return false;
                }

                if (targetAsset.assetType != Asset.AssetType.Ploppable)
                {
                    if (!UIFilterGrowable.instance.IsSelected(UIFilterGrowable.GetCategory(buildingInfo.m_class)))
                    {
                        UIFilterGrowable.instance.SelectAll();
                    }
                }
                else
                {
                    if (!UIFilterPloppable.instance.IsSelected(UIFilterPloppable.GetCategory(buildingInfo.m_class)))
                    {
                        UIFilterPloppable.instance.SelectAll();
                    }
                }
            }

            else if (targetAsset.assetType == Asset.AssetType.Tree)
            {
                if (!UIFilterTree.instance.IsSelected(UIFilterTree.GetCategory(targetAsset.treeType)))
                {
                    UIFilterTree.instance.SelectAll();
                }
            }

            else if (targetAsset.assetType == Asset.AssetType.Network)
            {
                if (!UIFilterNetwork.instance.IsSelected(UIFilterNetwork.GetCategory(targetAsset.networkType)))
                {
                    UIFilterNetwork.instance.SelectAll();
                }
            }

            else if (targetAsset.assetType == Asset.AssetType.Prop)
            {
                /*
                // set type drop-down
                typeFilter.selectedIndex = (int)DropDownOptions.Prop - (FindIt.isRicoEnabled ? 0 : 2);
                */

                // select filter tab
                if (!UIFilterProp.instance.IsSelected(UIFilterProp.GetCategory(targetAsset.propType)))
                {
                    UIFilterProp.instance.SelectAll();
                }
            }
            else if (targetAsset.assetType == Asset.AssetType.Decal)
            {
                /*
                // set type drop-down
                typeFilter.selectedIndex = (int)DropDownOptions.Decal - (FindIt.isRicoEnabled ? 0 : 2);
                */
            }
            else
            {
                // Debugging.Message("Picker - wrong asset type");
                searchEnabled = true;
                return false;
            }

            input.text = "";
            if (UIFilterTagPanel.instance?.tagDropDownCheckBox != null) UIFilterTagPanel.instance.tagDropDownCheckBox.isChecked = false;
            if (UIFilterExtraPanel.instance?.optionDropDownCheckBox != null) UIFilterExtraPanel.instance.optionDropDownCheckBox.isChecked = false;

            if (targetAsset.prefab.m_isCustomContent) workshopFilter.isChecked = true;
            else vanillaFilter.isChecked = true;

            searchEnabled = true;
            Search();

            // try to locate in the most recent search result
            bool found = false;
            for (int i = 0; i < searchResultList.Count; i++)
            {
                if (targetAsset.title == searchResultList.ElementAt(i))
                {
                    FindIt.instance.scrollPanel.DisplayAt(i);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // Debugging.Message("Picker - not found in the most recent search result");
                return false;
            }

            // try to locate in the displayed buttons
            found = false;
            foreach (UIButton button in FindIt.instance.scrollPanel.GetComponentsInChildren<UIButton>())
            {
                if (button.name == targetAsset.title)
                {
                    found = true;
                    button.SimulateClick();
                    break;
                }
            }
            if (!found)
            {
                // Debugging.Message("Picker - not found in the displayed buttons");
                return false;
            }

            // Debugging.Message("Picker - found");
            return true;
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void PickerRandomTest()
#pragma warning restore IDE0051 // Remove unused private members
        {
            int index = UnityEngine.Random.Range(0, AssetTagList.instance.assets.Count);
            Asset testTarget = AssetTagList.instance.assets.ElementAt(index).Value;
            // Debugging.Message($"Test target: {testTarget.title}");
            Picker(testTarget.prefab);
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (p.keycode == KeyCode.Escape)
            {
                if (hasFocus)
                {
                    // If the search box is focussed, unfocus.
                    input.Unfocus();
                }
            }
            base.OnKeyDown(p);
        }

#pragma warning disable IDE0051 // Remove unused private members
        void OnGUI()
#pragma warning restore IDE0051 // Remove unused private members
        {
            try
            {
                if (!UIView.HasModalInput() && UIView.HasInputFocus() && input.hasFocus)
                {
                    // UUI does not handle keys when input has focus.
                    // handle hotkeys only if Findit panel is the input that has focus
                    Event e = Event.current;
                    if (Settings.searchKey.IsPressed(e))
                        Settings.OpenFindIt(-1);
                    else if (Settings.allKey.IsPressed(e))
                        Settings.OpenFindIt(0);
                    else if (Settings.networkKey.IsPressed(e))
                        Settings.OpenFindIt(1);
                    else if (Settings.ploppableKey.IsPressed(e))
                        Settings.OpenFindIt(2);
                    else if (Settings.grwbRicoKey.IsPressed(e))
                        Settings.OpenFindIt(3);
                    else if (Settings.ricoKey.IsPressed(e))
                        Settings.OpenFindIt(4);
                    else if (Settings.grwbRicoKey.IsPressed(e))
                        Settings.OpenFindIt(5);
                    else if (Settings.propKey.IsPressed(e))
                        Settings.OpenFindIt(6);
                    else if (Settings.decalKey.IsPressed(e))
                        Settings.OpenFindIt(7);
                    else if (Settings.treeKey.IsPressed(e))
                        Settings.OpenFindIt(8);
                    else if (Settings.randomSelectionKey.IsPressed(e))
                        Settings.OpenFindIt(-2);
                }
            }
            catch (Exception e)
            {
                Debug.Log("OnGUI failed");
                Debug.LogException(e);
            }
        }
    }
}
