// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// search tab

using ColossalFramework.UI;
using System.Linq;
using UnityEngine;

namespace FindIt.GUI
{
    public class UISearchTab : UIPanel
    {
        public UISearchTab instance;
        public bool isActiveTab;
        private UISprite tabIcon;
        private UILabel tabLabel;
        private UISprite closeIcon;
        private bool isSelected;
        private static int counter = 0;
        public int searchTabIndex;
        private static readonly Color32 selectedColor = new Color32(160, 160, 160, 255);
        private static readonly Color32 unSelectedColor = new Color32(110, 110, 110, 255);
        private static readonly Color32 hoveredColor = new Color32(130, 130, 130, 255);

        public enum Category
        {
            All = 0,
            Network,
            Ploppable,
            Growable,
            Rico,
            GrowableRico,
            Prop,
            Decal,
            Tree
        }
        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "FindItAtlas",
                "FindItAtlas",
                "Ingame",
                "Ingame"
            };

            public static readonly string[] spriteNames =
            {
                "ToolbarIconProps",
                "SubBarRoadsSmall",
                "ToolbarIconMonuments",
                "ToolbarIconZoning",
                "IconPolicySmallBusiness",
                "GrwbRico",
                "ToolbarIconPropsBillboards",
                "SubBarLandscaping",
                "IconPolicyForest"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_SE_IA"), // all
                Translations.Translate("FIF_SE_IN"), // network
                Translations.Translate("FIF_SE_IP"), // ploppable
                Translations.Translate("FIF_SE_IG"), // growable
                Translations.Translate("FIF_SE_IR"), // rico
                Translations.Translate("FIF_SE_IGR"), // growable/rico
                Translations.Translate("FIF_SE_IPR"), // prop
                Translations.Translate("FIF_SE_ID"), // decal
                Translations.Translate("FIF_SE_IT") // tree
            };
        }

        public override void Start()
        {
            instance = this;
            size = new Vector2(140, 25);
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "ButtonWhite";

            searchTabIndex = counter;
            counter++;

            this.eventMouseEnter += (c, p) =>
            {
                if (!isSelected)
                {
                    color = hoveredColor;
                }
            };

            this.eventMouseLeave += (c, p) =>
            {
                if (!isSelected)
                {
                    color = unSelectedColor;
                }
            };

            SetTabIcon();
            SetTabLabel();
            SetCloseIcon();

            UIPanel clickingZone = AddUIComponent<UIPanel>();
            clickingZone.backgroundSprite = "";
            clickingZone.size = size;
            clickingZone.width = 115;
            clickingZone.relativePosition = new Vector2(0, 0);
            clickingZone.eventClicked += (c, p) =>
            {
                Selected();
            };

            if (UISearchTabPanel.instance.GetSelectedTab() == null)
            {
                Selected();
                isActiveTab = true;
                isVisible = true;
                UISearchTabPanel.instance.searchTabsList.Add(this);
                UISearchTabPanel.instance.RefreshUIPositions();
            }
            else
            {
                color = unSelectedColor;
                isVisible = false;
                isSelected = false;
                isActiveTab = false;
            }
        }

        private void SetTabIcon()
        {
            tabIcon = instance.AddUIComponent<UISprite>();
            tabIcon.size = new Vector2(20, 20);
            tabIcon.atlas = SamsamTS.UIUtils.GetAtlas(CategoryIcons.atlases[(int)Category.All]);
            tabIcon.spriteName = CategoryIcons.spriteNames[(int)Category.All];
            tabIcon.relativePosition = new Vector3(3, 3);
        }

        public void ChangeTabIcon(int selectedIndex)
        {
            if (!FindIt.isRicoEnabled && selectedIndex >= (int)UISearchBox.DropDownOptions.Rico) selectedIndex += 2;

            tabIcon.atlas = SamsamTS.UIUtils.GetAtlas(CategoryIcons.atlases[selectedIndex]);
            tabIcon.spriteName = CategoryIcons.spriteNames[selectedIndex];
        }

        private void SetTabLabel()
        {
            tabLabel = instance.AddUIComponent<UILabel>();
            tabLabel.textScale = 0.8f;
            tabLabel.padding = new RectOffset(0, 0, 0, 0);
            tabLabel.width = 70;
            tabLabel.text = Translations.Translate("FIF_STAB_NTAB");
            tabLabel.relativePosition = new Vector3(28, 7);
        }

        public void ChangeTabLabel(string text)
        {
            if (text == "") tabLabel.text = " ---- ";
            else if (text.Length > 9) tabLabel.text = text.Substring(0, 9) + "...";
            else tabLabel.text = text;
        }

        private void SetCloseIcon()
        {
            closeIcon = instance.AddUIComponent<UISprite>();
            closeIcon.size = new Vector2(20, 20);
            closeIcon.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            closeIcon.spriteName = "buttonclose";
            closeIcon.relativePosition = new Vector3(117, 2);
            closeIcon.eventClicked += (c, p) =>
            {
                if (!UISearchTabPanel.instance.IsOnlyTab(instance))
                {
                    Close();
                }
            };
        }
        public void Close()
        {
            if (instance != null)
            {
                // don't change the order of these two lines
                UISearchTabPanel.instance.RemoveSearchTab(instance);
                DeactivateTab();
            }
        }

        public void Selected()
        {
            UISearchTabPanel.instance.SetSelectedTab(this);
            isSelected = true;
            color = selectedColor;
            RestoreTabData();
        }

        public void Unselected()
        {
            isSelected = false;
            color = unSelectedColor;
            StoreTabData();
        }

        private class TabData
        {
            public string inputText;
            public int typeFilterSelectedIndex;
            public bool workshopFilterIsChecked;
            public bool vanillaFilterIsChecked;
            public int sizeFilterXSelectedIndex;
            public int sizeFilterYSelectedIndex;
            public bool[] filterGrowableChecked = new bool[(int)UIFilterGrowable.Category.All];
            public bool[] filterNetworkChecked = new bool[(int)UIFilterNetwork.Category.All];
            public bool[] filterPloppableChecked = new bool[(int)UIFilterPloppable.Category.All];
            public bool[] filterPropChecked = new bool[(int)UIFilterProp.Category.All];
            public bool[] filterTreeChecked = new bool[(int)UIFilterTree.Category.All];
            public string firstAssetTitle;

            // custom tag panel
            public bool customTagPanelEnabled = false;
            public string selectedCustomtag = "";

            // extra filters panel
            public bool extraFiltersPanelEnabled = false;
            public int selectedExtraFiltersIndex = 0;

            // extra filtes panel sub-filters
            public int assetCreatorDropDownMenuSelectedIndex = 0;
            public string assetCreatorInputString = "";
            public string buildingHeightMinInputString = "";
            public string buildingHeightMaxInputString = "";
            public int builingHeightUnitSelectedIndex = 0;
            public int buildingLevelMinDropDownMenuSelectedIndex = 0;
            public int buildingLevelMaxDropDownMenuSelectedIndex = 0;
            public int dlcDropDownMenuSelectedIndex = 0;
            public int districtStyleDropDownMenuSelectedIndex = 0;

            public TabData()
            {
                Reset();
            }

            public void Reset()
            {
                inputText = "";
                typeFilterSelectedIndex = 0;
                workshopFilterIsChecked = Settings.useWorkshopFilter;
                vanillaFilterIsChecked = Settings.useVanillaFilter;
                sizeFilterXSelectedIndex = 0;
                sizeFilterYSelectedIndex = 0;
                firstAssetTitle = "";
                for (int i = 0; i < (int)UIFilterGrowable.Category.All; ++i) filterGrowableChecked[i] = true;
                for (int i = 0; i < (int)UIFilterNetwork.Category.All; ++i) filterNetworkChecked[i] = true;
                for (int i = 0; i < (int)UIFilterPloppable.Category.All; ++i) filterPloppableChecked[i] = true;
                for (int i = 0; i < (int)UIFilterProp.Category.All; ++i) filterPropChecked[i] = true;
                for (int i = 0; i < (int)UIFilterTree.Category.All; ++i) filterTreeChecked[i] = true;

                // custom tag panel
                customTagPanelEnabled = false;
                selectedCustomtag = "";

                // extra filers panel
                extraFiltersPanelEnabled = false;
                selectedExtraFiltersIndex = 0;

                // extra filtes panel sub-filters
                assetCreatorDropDownMenuSelectedIndex = 0;
                assetCreatorInputString = "";
                buildingHeightMinInputString = "";
                buildingHeightMaxInputString = "";
                builingHeightUnitSelectedIndex = 0;
                buildingLevelMinDropDownMenuSelectedIndex = 0;
                buildingLevelMaxDropDownMenuSelectedIndex = 0;
                dlcDropDownMenuSelectedIndex = 0;
                districtStyleDropDownMenuSelectedIndex = 0;
            }
        }
        private TabData tabData = new TabData();

        private void StoreTabData()
        {
            UISearchBox searchbox = UISearchBox.instance;
            tabData.inputText = searchbox.input.text;
            tabData.typeFilterSelectedIndex = searchbox.typeFilter.selectedIndex;
            tabData.workshopFilterIsChecked = searchbox.workshopFilter.isChecked;
            tabData.vanillaFilterIsChecked = searchbox.vanillaFilter.isChecked;

            UISearchBox.DropDownOptions type = (UISearchBox.DropDownOptions)tabData.typeFilterSelectedIndex;

            if (!FindIt.isRicoEnabled && type >= UISearchBox.DropDownOptions.Rico) type += 2;

            if (type == UISearchBox.DropDownOptions.Growable ||
                type == UISearchBox.DropDownOptions.Ploppable ||
                type == UISearchBox.DropDownOptions.Rico ||
                type == UISearchBox.DropDownOptions.GrwbRico
                )
            {
                tabData.sizeFilterXSelectedIndex = searchbox.sizeFilterX.selectedIndex;
                tabData.sizeFilterYSelectedIndex = searchbox.sizeFilterY.selectedIndex;
            }

            if (type == UISearchBox.DropDownOptions.Prop)
            {
                for (int i = 0; i < (int)UIFilterProp.Category.All; ++i) tabData.filterPropChecked[i] = UIFilterProp.instance.toggles[i].isChecked;
            }
            else if (type == UISearchBox.DropDownOptions.Growable ||
               type == UISearchBox.DropDownOptions.Rico ||
               type == UISearchBox.DropDownOptions.GrwbRico
               )
            {
                for (int i = 0; i < (int)UIFilterGrowable.Category.All; ++i) tabData.filterGrowableChecked[i] = UIFilterGrowable.instance.toggles[i].isChecked;
            }
            else if (type == UISearchBox.DropDownOptions.Network)
            {
                for (int i = 0; i < (int)UIFilterNetwork.Category.All; ++i) tabData.filterNetworkChecked[i] = UIFilterNetwork.instance.toggles[i].isChecked;
            }
            else if (type == UISearchBox.DropDownOptions.Ploppable)
            {
                for (int i = 0; i < (int)UIFilterPloppable.Category.All; ++i) tabData.filterPloppableChecked[i] = UIFilterPloppable.instance.toggles[i].isChecked;
            }
            else if (type == UISearchBox.DropDownOptions.Tree)
            {
                for (int i = 0; i < (int)UIFilterTree.Category.All; ++i) tabData.filterTreeChecked[i] = UIFilterTree.instance.toggles[i].isChecked;
            }

            // store custom tag panel info
            tabData.customTagPanelEnabled = UIFilterTagPanel.instance.tagDropDownCheckBox.isChecked;
            if (tabData.customTagPanelEnabled)
            {
                tabData.selectedCustomtag = UIFilterTagPanel.instance.GetDropDownListKey();
            }

            // store extra filters panel info
            tabData.extraFiltersPanelEnabled = UIFilterExtraPanel.instance.optionDropDownCheckBox.isChecked;
            if (tabData.extraFiltersPanelEnabled)
            {
                tabData.selectedExtraFiltersIndex = UIFilterExtraPanel.instance.optionDropDownMenu.selectedIndex;

                // store sub filters in each extra filter
                switch (tabData.selectedExtraFiltersIndex)
                {
                    case (int)UIFilterExtraPanel.DropDownOptions.AssetCreator:
                        tabData.assetCreatorDropDownMenuSelectedIndex = UIFilterExtraPanel.instance.assetCreatorDropDownMenu.selectedIndex;
                        tabData.assetCreatorInputString = UIFilterExtraPanel.instance.assetCreatorInput.text;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.BuildingHeight:
                        tabData.buildingHeightMinInputString = UIFilterExtraPanel.instance.buildingHeightMinInput.text;
                        tabData.buildingHeightMaxInputString = UIFilterExtraPanel.instance.buildingHeightMaxInput.text;
                        tabData.builingHeightUnitSelectedIndex = UIFilterExtraPanel.instance.builingHeightUnit.selectedIndex;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.BuildingLevel:
                        tabData.buildingLevelMinDropDownMenuSelectedIndex = UIFilterExtraPanel.instance.buildingLevelMinDropDownMenu.selectedIndex;
                        tabData.buildingLevelMaxDropDownMenuSelectedIndex = UIFilterExtraPanel.instance.buildingLevelMaxDropDownMenu.selectedIndex;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.DLC:
                        tabData.dlcDropDownMenuSelectedIndex = UIFilterExtraPanel.instance.dlcDropDownMenu.selectedIndex;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.DistrictStyle:
                        tabData.districtStyleDropDownMenuSelectedIndex = UIFilterExtraPanel.instance.districtStyleDropDownMenu.selectedIndex;
                        break;
                    default:
                        break;
                }
            }

            // store the first displayed asset title
            tabData.firstAssetTitle = FindIt.instance.scrollPanel.GetItem(0).component.name;
        }

        private void RestoreTabData()
        {
            UISearchBox searchbox = UISearchBox.instance;

            // avoid triggering duplicate searches
            searchbox.searchEnabled = false;

            searchbox.input.text = tabData.inputText;
            searchbox.typeFilter.selectedIndex = tabData.typeFilterSelectedIndex;
            searchbox.workshopFilter.isChecked = tabData.workshopFilterIsChecked;
            searchbox.vanillaFilter.isChecked = tabData.vanillaFilterIsChecked;

            UISearchBox.DropDownOptions type = (UISearchBox.DropDownOptions)tabData.typeFilterSelectedIndex;

            if (!FindIt.isRicoEnabled && type >= UISearchBox.DropDownOptions.Rico) type += 2;

            if (type == UISearchBox.DropDownOptions.Growable ||
                type == UISearchBox.DropDownOptions.Ploppable ||
                type == UISearchBox.DropDownOptions.Rico ||
                type == UISearchBox.DropDownOptions.GrwbRico
                )
            {
                searchbox.sizeFilterX.selectedIndex = tabData.sizeFilterXSelectedIndex;
                searchbox.sizeFilterY.selectedIndex = tabData.sizeFilterYSelectedIndex;
            }

            if (type == UISearchBox.DropDownOptions.Prop)
            {
                for (int i = 0; i < (int)UIFilterProp.Category.All; ++i) UIFilterProp.instance.toggles[i].isChecked = tabData.filterPropChecked[i];
            }
            else if (type == UISearchBox.DropDownOptions.Growable ||
               type == UISearchBox.DropDownOptions.Rico ||
               type == UISearchBox.DropDownOptions.GrwbRico
               )
            {
                for (int i = 0; i < (int)UIFilterGrowable.Category.All; ++i) UIFilterGrowable.instance.toggles[i].isChecked = tabData.filterGrowableChecked[i];
            }
            else if (type == UISearchBox.DropDownOptions.Network)
            {
                for (int i = 0; i < (int)UIFilterNetwork.Category.All; ++i) UIFilterNetwork.instance.toggles[i].isChecked = tabData.filterNetworkChecked[i];
            }
            else if (type == UISearchBox.DropDownOptions.Ploppable)
            {
                for (int i = 0; i < (int)UIFilterPloppable.Category.All; ++i) UIFilterPloppable.instance.toggles[i].isChecked = tabData.filterPloppableChecked[i];
            }
            else if (type == UISearchBox.DropDownOptions.Tree)
            {
                for (int i = 0; i < (int)UIFilterTree.Category.All; ++i) UIFilterTree.instance.toggles[i].isChecked = tabData.filterTreeChecked[i];
            }

            // restore custom tag panel info
            UIFilterTagPanel.instance.tagDropDownCheckBox.isChecked = tabData.customTagPanelEnabled;

            if (tabData.customTagPanelEnabled)
            {
                if (!UIFilterTagPanel.instance.isVisible)
                {
                    UISearchBox.instance.OpenCustomTagPanel();
                }

                int selectedIndex = UIFilterTagPanel.instance.GetDropDownListIndex(tabData.selectedCustomtag);
                if (selectedIndex != -1) // found the index
                {
                    UIFilterTagPanel.instance.tagDropDownMenu.selectedIndex = selectedIndex;
                }
                else // can't restore the selected tag, disable the panel
                {
                    UIFilterTagPanel.instance.tagDropDownCheckBox.isChecked = false;
                }
            }

            // restore extra filters panel info
            UIFilterExtraPanel.instance.optionDropDownCheckBox.isChecked = tabData.extraFiltersPanelEnabled;

            if (tabData.extraFiltersPanelEnabled)
            {
                if (!UIFilterExtraPanel.instance.isVisible)
                {
                    UISearchBox.instance.OpenExtraFiltersPanel();
                }
                UIFilterExtraPanel.instance.optionDropDownMenu.selectedIndex = tabData.selectedExtraFiltersIndex;

                // restore sub filters in each extra filter
                switch (tabData.selectedExtraFiltersIndex)
                {
                    case (int)UIFilterExtraPanel.DropDownOptions.AssetCreator:
                        UIFilterExtraPanel.instance.assetCreatorDropDownMenu.selectedIndex = tabData.assetCreatorDropDownMenuSelectedIndex;
                        UIFilterExtraPanel.instance.assetCreatorInput.text = tabData.assetCreatorInputString;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.BuildingHeight:
                        UIFilterExtraPanel.instance.buildingHeightMinInput.text = tabData.buildingHeightMinInputString;
                        UIFilterExtraPanel.instance.buildingHeightMaxInput.text = tabData.buildingHeightMaxInputString;
                        UIFilterExtraPanel.instance.builingHeightUnit.selectedIndex = tabData.builingHeightUnitSelectedIndex;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.BuildingLevel:
                        UIFilterExtraPanel.instance.buildingLevelMinDropDownMenu.selectedIndex = tabData.buildingLevelMinDropDownMenuSelectedIndex;
                        UIFilterExtraPanel.instance.buildingLevelMaxDropDownMenu.selectedIndex = tabData.buildingLevelMaxDropDownMenuSelectedIndex;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.DLC:
                        UIFilterExtraPanel.instance.dlcDropDownMenu.selectedIndex = tabData.dlcDropDownMenuSelectedIndex;
                        break;
                    case (int)UIFilterExtraPanel.DropDownOptions.DistrictStyle:
                        UIFilterExtraPanel.instance.districtStyleDropDownMenu.selectedIndex = tabData.districtStyleDropDownMenuSelectedIndex;
                        break;
                    default:
                        break;
                }
            }

            searchbox.searchEnabled = true;
            searchbox.Search();

            // restore the first displayed asset
            if (tabData.firstAssetTitle != null)
            {
                // try to locate in the most recent search result
                for (int i = 0; i < searchbox.searchResultList.Count; i++)
                {
                    if (tabData.firstAssetTitle == searchbox.searchResultList.ElementAt(i))
                    {
                        FindIt.instance.scrollPanel.DisplayAt(i);
                        break;
                    }
                }
            }
        }

        public void ResetTab()
        {
            tabIcon.atlas = SamsamTS.UIUtils.GetAtlas(CategoryIcons.atlases[(int)Category.All]);
            tabIcon.spriteName = CategoryIcons.spriteNames[(int)Category.All];
            tabLabel.text = Translations.Translate("FIF_STAB_NTAB");
            tabData.Reset();
        }

        public void DeactivateTab()
        {
            instance.isActiveTab = false;
            instance.isVisible = false;
        }
    }
}
