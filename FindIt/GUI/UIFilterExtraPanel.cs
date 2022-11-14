// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// extra filters panel

using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIFilterExtraPanel : UIPanel
    {
        public static UIFilterExtraPanel instance;

        public UICheckBox optionDropDownCheckBox;
        public UIDropDown optionDropDownMenu;

        // asset creator
        public UITextField assetCreatorInput;
        private UISprite assetCreatorSearchIcon;
        public UIDropDown assetCreatorDropDownMenu;
        private List<KeyValuePair<string, int>> assetCreatorList;
        private string[] assetCreatorListStrArray;

        // building height
        private UILabel buildingHeightMinLabel;
        private UILabel buildingHeightMaxLabel;
        public UITextField buildingHeightMinInput;
        public UITextField buildingHeightMaxInput;
        public UIDropDown builingHeightUnit;
        public float minBuildingHeight = float.MinValue;
        public float maxBuildingHeight = float.MaxValue;

        // building level
        private UILabel buildingLevelMinLabel;
        private UILabel buildingLevelMaxLabel;
        public UIDropDown buildingLevelMinDropDownMenu;
        public UIDropDown buildingLevelMaxDropDownMenu;

        // unused assets
        private UIButton exportAllUnusedButton;
        private UIButton exportSearchedUnusedButton;

        // unused assets
        private UIButton exportAllUsedButton;
        private UIButton exportSearchedUsedButton;

        // DLC & CCP
        public UIDropDown dlcDropDownMenu;

        // District Style
        public UIDropDown districtStyleDropDownMenu;
        public List<DistrictStyle> districtStyleList;
        private string[] districtStyleListStrArray;

        public enum DropDownOptions
        {
            AssetCreator = 0,
            BuildingHeight,
            BuildingLevel,
            DistrictStyle,
            DLC,
            SubBuildings,
            UsedAssets,
            UnusedAssets,
            LocalCustom,
            WorkshopCustom,
            WithCustomTag,
            WithoutCustomTag,
            TerrainConforming,
            NonTerrainConforming,
            CreatorHidden
        }

        string[] options = {
                    Translations.Translate("FIF_EF_AC"), // Asset Creator
                    Translations.Translate("FIF_EF_BH"), // Building Height
                    Translations.Translate("FIF_SE_LV"), // Building Level
                    Translations.Translate("FIF_EF_DS"), // District Style
                    Translations.Translate("FIF_EF_DLC"), // Require DLC/CCP
                    Translations.Translate("FIF_EF_SB"), // Sub-building
                    Translations.Translate("FIF_EF_US"), // Used Asset
                    Translations.Translate("FIF_EF_UN"), // Unused Asset
                    Translations.Translate("FIF_EF_LC"), // Local Custom
                    Translations.Translate("FIF_EF_WC"), // Workshop Subscription
                    Translations.Translate("FIF_EF_CT"), // With Custom Tag
                    Translations.Translate("FIF_EF_NCT"), // Without Custom Tag
                    Translations.Translate("FIF_PROP_TC"), // Terrain conforming
                    Translations.Translate("FIF_PROP_NTC"), // Non-Terrain conforming
                    "creator_hidden" // assets that are suggested to be hidden by their creators
                };

        public enum DLCDropDownOptions
        {
            BaseGame = 0,
            AfterDark,
            Airports,
            Campus,
            GreenCities,
            Industries,
            MassTransit,
            NaturalDisasters,
            Parklife,
            PlazasAndPromenades,
            SnowFall,
            SunsetHarbor,
            ArtDeco,
            HighTechBuildings,
            EuropeanSuburbias,
            UniverisityCity,
            ModernCityCenter,
            ModernJapan,
            TrainStations,
            BridgesPiers,
            VehiclesoftheWorld,
            MidCenturyModern,
            SeasideResorts,
            Skyscrapers,
            HeartOfKorea,
            Concerts,
            DeluxeUpgrade,
            MatchDay,
            PearlsFromTheEast,
            Stadiums
        }

        public override void Start()
        {
            instance = this;

            // extra filter checkbox
            optionDropDownCheckBox = SamsamTS.UIUtils.CreateCheckBox(this);
            optionDropDownCheckBox.isChecked = false;
            optionDropDownCheckBox.width = 20;
            optionDropDownCheckBox.relativePosition = new Vector3(10, 10);
            optionDropDownCheckBox.eventCheckChanged += (c, i) =>
            {
                if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.SubBuildings)
                {
                    if (optionDropDownCheckBox.isChecked)
                        UISearchBox.instance.typeFilter.selectedIndex = (int)UISearchBox.DropDownOptions.All;
                }

                ((UISearchBox)parent).Search();
            };

            // extra filter dropdown
            optionDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            optionDropDownMenu.size = new Vector2(230, 25);
            optionDropDownMenu.listHeight = 300;
            optionDropDownMenu.itemHeight = 30;
            optionDropDownMenu.items = options;
            optionDropDownMenu.selectedIndex = 0;
            optionDropDownMenu.relativePosition = new Vector3(optionDropDownCheckBox.relativePosition.x + optionDropDownCheckBox.width + 5, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.optionDropDownMenu);

            optionDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                HideAll();

                if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.AssetCreator)
                {
                    UpdateAssetCreatorOptionVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.BuildingHeight)
                {
                    UpdateBuildingHeightOptionVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.BuildingLevel)
                {
                    UpdateBuildingLevelOptionVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.UnusedAssets)
                {
                    UpdateUnusedAssetsVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.UsedAssets)
                {
                    UpdateUsedAssetsVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.DLC)
                {
                    UpdateDLCVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.DistrictStyle)
                {
                    UpdateDistrictStyleVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.SubBuildings)
                {
                    if (optionDropDownCheckBox.isChecked)
                        UISearchBox.instance.typeFilter.selectedIndex = (int)UISearchBox.DropDownOptions.All;
                }

                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // asset creator
            assetCreatorInput = SamsamTS.UIUtils.CreateTextField(this);
            assetCreatorInput.size = new Vector2(110, 25);
            assetCreatorInput.padding.top = 5;
            assetCreatorInput.isVisible = true;
            assetCreatorInput.text = "";
            assetCreatorInput.textScale = 0.9f;
            assetCreatorInput.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 20, 5);
            assetCreatorInput.eventTextChanged += (c, p) =>
            {
                if (assetCreatorInput.text == "") return;

                for (int i = 0; i < assetCreatorDropDownMenu.items.Length; ++i)
                {
                    if (assetCreatorDropDownMenu.items[i].ToLower().StartsWith(assetCreatorInput.text.ToLower()))
                    {
                        assetCreatorDropDownMenu.selectedIndex = i;
                        return;
                    }
                }
                for (int i = 0; i < assetCreatorDropDownMenu.items.Length; ++i)
                {
                    if (assetCreatorDropDownMenu.items[i].ToLower().Contains(assetCreatorInput.text.ToLower()))
                    {
                        assetCreatorDropDownMenu.selectedIndex = i;
                        return;
                    }
                }
            };

            // search icon
            assetCreatorSearchIcon = AddUIComponent<UISprite>();
            assetCreatorSearchIcon.size = new Vector2(25, 30);
            assetCreatorSearchIcon.atlas = FindIt.atlas;
            assetCreatorSearchIcon.spriteName = "FindItDisabled";
            assetCreatorSearchIcon.isVisible = true;
            assetCreatorSearchIcon.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 20, 3);

            assetCreatorDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            assetCreatorDropDownMenu.size = new Vector2(270, 25);
            assetCreatorDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            assetCreatorDropDownMenu.listHeight = 300;
            assetCreatorDropDownMenu.itemHeight = 30;
            UpdateAssetCreatorList();
            assetCreatorDropDownMenu.isVisible = true;
            assetCreatorDropDownMenu.relativePosition = new Vector3(assetCreatorInput.relativePosition.x + assetCreatorInput.width + 10, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.assetCreatorDropDownMenu);

            assetCreatorDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // building height min label
            buildingHeightMinLabel = this.AddUIComponent<UILabel>();
            buildingHeightMinLabel.textScale = 0.8f;
            buildingHeightMinLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingHeightMinLabel.text = "Min:";
            buildingHeightMinLabel.isVisible = false;
            buildingHeightMinLabel.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);

            // building height min input box
            buildingHeightMinInput = SamsamTS.UIUtils.CreateTextField(this);
            buildingHeightMinInput.size = new Vector2(60, 25);
            buildingHeightMinInput.padding.top = 5;
            buildingHeightMinInput.isVisible = false;
            buildingHeightMinInput.text = "";
            buildingHeightMinInput.relativePosition = new Vector3(buildingHeightMinLabel.relativePosition.x + buildingHeightMinLabel.width + 10, 5);
            buildingHeightMinInput.eventTextChanged += (c, p) =>
            {
                if (float.TryParse(buildingHeightMinInput.text, out minBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1)
                    {
                        minBuildingHeight *= 0.3048f;
                    }
                    ((UISearchBox)parent).Search();

                }
                if (buildingHeightMinInput.text == "")
                {
                    minBuildingHeight = float.MinValue;
                    ((UISearchBox)parent).Search();
                }
            };

            // building height max label
            buildingHeightMaxLabel = this.AddUIComponent<UILabel>();
            buildingHeightMaxLabel.textScale = 0.8f;
            buildingHeightMaxLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingHeightMaxLabel.text = "Max:";
            buildingHeightMaxLabel.isVisible = false;
            buildingHeightMaxLabel.relativePosition = new Vector3(buildingHeightMinInput.relativePosition.x + buildingHeightMinInput.width + 20, 5);

            // building height max input box
            buildingHeightMaxInput = SamsamTS.UIUtils.CreateTextField(this);
            buildingHeightMaxInput.size = new Vector2(60, 25);
            buildingHeightMaxInput.padding.top = 5;
            buildingHeightMaxInput.isVisible = false;
            buildingHeightMaxInput.text = "";
            buildingHeightMaxInput.relativePosition = new Vector3(buildingHeightMaxLabel.relativePosition.x + buildingHeightMaxLabel.width + 10, 5);
            buildingHeightMaxInput.eventTextChanged += (c, p) =>
            {

                if (float.TryParse(buildingHeightMaxInput.text, out maxBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1)
                    {
                        maxBuildingHeight *= 0.3048f;
                    }
                    ((UISearchBox)parent).Search();
                }

                if (buildingHeightMaxInput.text == "")
                {
                    maxBuildingHeight = float.MaxValue;
                    ((UISearchBox)parent).Search();
                }
            };

            // building height unit
            builingHeightUnit = SamsamTS.UIUtils.CreateDropDown(this);
            builingHeightUnit.size = new Vector2(80, 25);
            builingHeightUnit.listHeight = 210;
            builingHeightUnit.itemHeight = 30;
            builingHeightUnit.AddItem(Translations.Translate("FIF_EF_MET"));
            builingHeightUnit.AddItem(Translations.Translate("FIF_EF_FEE"));
            builingHeightUnit.selectedIndex = 0;
            builingHeightUnit.isVisible = false;
            builingHeightUnit.relativePosition = new Vector3(buildingHeightMaxInput.relativePosition.x + buildingHeightMaxInput.width + 30, 5);
            builingHeightUnit.eventSelectedIndexChanged += (c, p) =>
            {
                if (float.TryParse(buildingHeightMinInput.text, out minBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1) minBuildingHeight *= 0.3048f;
                }

                if (float.TryParse(buildingHeightMaxInput.text, out maxBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1) maxBuildingHeight *= 0.3048f;
                }
                if (buildingHeightMinInput.text == "") minBuildingHeight = float.MinValue;
                if (buildingHeightMaxInput.text == "") maxBuildingHeight = float.MaxValue;
                ((UISearchBox)parent).Search();
            };

            // building level min label
            buildingLevelMinLabel = this.AddUIComponent<UILabel>();
            buildingLevelMinLabel.textScale = 0.8f;
            buildingLevelMinLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingLevelMinLabel.text = "Min:";
            buildingLevelMinLabel.isVisible = false;
            buildingLevelMinLabel.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);

            // building level min dropdown
            buildingLevelMinDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            buildingLevelMinDropDownMenu.size = new Vector2(60, 25);
            buildingLevelMinDropDownMenu.listHeight = 300;
            buildingLevelMinDropDownMenu.itemHeight = 30;
            buildingLevelMinDropDownMenu.AddItem("1");
            buildingLevelMinDropDownMenu.AddItem("2");
            buildingLevelMinDropDownMenu.AddItem("3");
            buildingLevelMinDropDownMenu.AddItem("4");
            buildingLevelMinDropDownMenu.AddItem("5");
            buildingLevelMinDropDownMenu.isVisible = false;
            buildingLevelMinDropDownMenu.selectedIndex = 0;
            buildingLevelMinDropDownMenu.relativePosition = new Vector3(buildingLevelMinLabel.relativePosition.x + buildingLevelMinLabel.width + 10, 5);
            buildingLevelMinDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // building level max label
            buildingLevelMaxLabel = this.AddUIComponent<UILabel>();
            buildingLevelMaxLabel.textScale = 0.8f;
            buildingLevelMaxLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingLevelMaxLabel.text = "Max:";
            buildingLevelMaxLabel.isVisible = false;
            buildingLevelMaxLabel.relativePosition = new Vector3(buildingLevelMinDropDownMenu.relativePosition.x + buildingLevelMinDropDownMenu.width + 20, 5);

            // building level max dropdown
            buildingLevelMaxDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            buildingLevelMaxDropDownMenu.size = new Vector2(60, 25);
            buildingLevelMaxDropDownMenu.listHeight = 300;
            buildingLevelMaxDropDownMenu.itemHeight = 30;
            buildingLevelMaxDropDownMenu.AddItem("1");
            buildingLevelMaxDropDownMenu.AddItem("2");
            buildingLevelMaxDropDownMenu.AddItem("3");
            buildingLevelMaxDropDownMenu.AddItem("4");
            buildingLevelMaxDropDownMenu.AddItem("5");
            buildingLevelMaxDropDownMenu.isVisible = false;
            buildingLevelMaxDropDownMenu.selectedIndex = 0;
            buildingLevelMaxDropDownMenu.relativePosition = new Vector3(buildingLevelMaxLabel.relativePosition.x + buildingLevelMaxLabel.width + 10, 5);
            buildingLevelMaxDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // export all unused asset list
            exportAllUnusedButton = SamsamTS.UIUtils.CreateButton(this);
            exportAllUnusedButton.size = new Vector2(80, 25);
            exportAllUnusedButton.text = Translations.Translate("FIF_EF_UNEXP");
            exportAllUnusedButton.textScale = 0.8f;
            exportAllUnusedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportAllUnusedButton.tooltip = Translations.Translate("FIF_EF_UNEXPTP");
            exportAllUnusedButton.isVisible = false;
            exportAllUnusedButton.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 15, 5);
            exportAllUnusedButton.eventClick += (c, p) =>
            {
                ExportUnusedTool.ExportUnused(true);
            };

            // export searched unused asset list
            exportSearchedUnusedButton = SamsamTS.UIUtils.CreateButton(this);
            exportSearchedUnusedButton.size = new Vector2(130, 25);
            exportSearchedUnusedButton.text = Translations.Translate("FIF_EF_UNEXPSE");
            exportSearchedUnusedButton.textScale = 0.8f;
            exportSearchedUnusedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportSearchedUnusedButton.tooltip = Translations.Translate("FIF_EF_UNEXPTPSE");
            exportSearchedUnusedButton.isVisible = false;
            exportSearchedUnusedButton.relativePosition = new Vector3(exportAllUnusedButton.relativePosition.x + exportAllUnusedButton.width + 5, 5);
            exportSearchedUnusedButton.eventClick += (c, p) =>
            {
                ExportUnusedTool.ExportUnused(false);
            };

            // export all used asset list
            exportAllUsedButton = SamsamTS.UIUtils.CreateButton(this);
            exportAllUsedButton.size = new Vector2(80, 25);
            exportAllUsedButton.text = Translations.Translate("FIF_EF_UNEXP");
            exportAllUsedButton.textScale = 0.8f;
            exportAllUsedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportAllUsedButton.tooltip = Translations.Translate("FIF_EF_USEXPTP");
            exportAllUsedButton.isVisible = false;
            exportAllUsedButton.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 15, 5);
            exportAllUsedButton.eventClick += (c, p) =>
            {
                ExportUsedTool.ExportUsed(true);
            };

            // export searched used asset list
            exportSearchedUsedButton = SamsamTS.UIUtils.CreateButton(this);
            exportSearchedUsedButton.size = new Vector2(130, 25);
            exportSearchedUsedButton.text = Translations.Translate("FIF_EF_UNEXPSE");
            exportSearchedUsedButton.textScale = 0.8f;
            exportSearchedUsedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportSearchedUsedButton.tooltip = Translations.Translate("FIF_EF_USEXPTPSE");
            exportSearchedUsedButton.isVisible = false;
            exportSearchedUsedButton.relativePosition = new Vector3(exportAllUsedButton.relativePosition.x + exportAllUsedButton.width + 5, 5);
            exportSearchedUsedButton.eventClick += (c, p) =>
            {
                ExportUsedTool.ExportUsed(false);
            };

            // DLC & CCP
            dlcDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            dlcDropDownMenu.size = new Vector2(300, 25);
            dlcDropDownMenu.listHeight = 300;
            dlcDropDownMenu.itemHeight = 30;
            dlcDropDownMenu.AddItem("Base Game");
            dlcDropDownMenu.AddItem("After Dark DLC");
            dlcDropDownMenu.AddItem("Airports DLC");
            dlcDropDownMenu.AddItem("Campus DLC");
            dlcDropDownMenu.AddItem("Green Cities DLC");
            dlcDropDownMenu.AddItem("Industries DLC");
            dlcDropDownMenu.AddItem("Mass Transit DLC");
            dlcDropDownMenu.AddItem("Natural Disasters DLC");
            dlcDropDownMenu.AddItem("Parklife DLC");
            dlcDropDownMenu.AddItem("Plazas & Promenades DLC");
            dlcDropDownMenu.AddItem("Snow Fall DLC");
            dlcDropDownMenu.AddItem("Sunset Harbor DLC");
            dlcDropDownMenu.AddItem("Art Deco CCP");
            dlcDropDownMenu.AddItem("High-Tech Buildings CCP");
            dlcDropDownMenu.AddItem("European Suburbias CCP");
            dlcDropDownMenu.AddItem("University City CCP");
            dlcDropDownMenu.AddItem("Modern City Center CCP");
            dlcDropDownMenu.AddItem("Modern Japan CCP");
            dlcDropDownMenu.AddItem("Train Stations CCP");
            dlcDropDownMenu.AddItem("Bridges & Piers CCP");
            dlcDropDownMenu.AddItem("Vehicles of the World CCP");
            dlcDropDownMenu.AddItem("Mid-Century Modern CCP");
            dlcDropDownMenu.AddItem("Seaside Resorts CCP");
            dlcDropDownMenu.AddItem("Skyscrapers CCP");
            dlcDropDownMenu.AddItem("Heart of Korea CCP");
            dlcDropDownMenu.AddItem("Concerts DLC");
            dlcDropDownMenu.AddItem("Deluxe Upgrade Pack");
            dlcDropDownMenu.AddItem("Match Day DLC");
            dlcDropDownMenu.AddItem("Pearls from the East DLC");
            dlcDropDownMenu.AddItem("Stadiums: European Club Pack DLC");
            dlcDropDownMenu.isVisible = false;
            dlcDropDownMenu.selectedIndex = 0;
            dlcDropDownMenu.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.dlcDropDownMenu);

            dlcDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // District Style
            districtStyleDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            districtStyleDropDownMenu.size = new Vector2(425, 25);
            districtStyleDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            districtStyleDropDownMenu.listHeight = 300;
            districtStyleDropDownMenu.itemHeight = 30;
            districtStyleDropDownMenu.isVisible = false;
            UpdateDistrictStyleList();
            districtStyleDropDownMenu.selectedIndex = 0;
            districtStyleDropDownMenu.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 26, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.districtStyleDropDownMenu);
            districtStyleDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

        }
        public void Close()
        {
            if (instance != null)
            {
                instance.isVisible = false;
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        // Update asset creator list 
        public void UpdateAssetCreatorList()
        {
            assetCreatorList = AssetTagList.instance.GetAssetCreatorList();

            List<string> list = new List<string>();

            foreach (KeyValuePair<string, int> entry in assetCreatorList)
            {
                list.Add(entry.Key.ToString() + " (" + entry.Value.ToString() + ")");
            }

            assetCreatorListStrArray = list.ToArray();
            assetCreatorDropDownMenu.items = assetCreatorListStrArray;
            assetCreatorDropDownMenu.selectedIndex = 0;
        }

        public string GetAssetCreatorDropDownListKey()
        {
            return assetCreatorList[assetCreatorDropDownMenu.selectedIndex].Key;
        }

        private void UpdateAssetCreatorOptionVisibility(bool visibility)
        {
            assetCreatorDropDownMenu.isVisible = visibility;
            assetCreatorInput.isVisible = visibility;
            assetCreatorSearchIcon.isVisible = visibility;
        }

        private void UpdateBuildingHeightOptionVisibility(bool visibility)
        {
            buildingHeightMinLabel.isVisible = visibility;
            buildingHeightMinInput.isVisible = visibility;
            buildingHeightMaxLabel.isVisible = visibility;
            buildingHeightMaxInput.isVisible = visibility;
            builingHeightUnit.isVisible = visibility;
        }

        private void UpdateBuildingLevelOptionVisibility(bool visibility)
        {
            buildingLevelMinDropDownMenu.isVisible = visibility;
            buildingLevelMaxDropDownMenu.isVisible = visibility;
            buildingLevelMinLabel.isVisible = visibility;
            buildingLevelMaxLabel.isVisible = visibility;
        }

        private void UpdateUnusedAssetsVisibility(bool visibility)
        {
            exportAllUnusedButton.isVisible = visibility;
            exportSearchedUnusedButton.isVisible = visibility;
        }
        private void UpdateUsedAssetsVisibility(bool visibility)
        {
            exportAllUsedButton.isVisible = visibility;
            exportSearchedUsedButton.isVisible = visibility;
        }
        private void UpdateDLCVisibility(bool visibility)
        {
            dlcDropDownMenu.isVisible = visibility;
        }

        public void UpdateDistrictStyleList()
        {
            var styles = DistrictManager.instance.m_Styles;
            if (styles == null) return;

            districtStyleList = new List<DistrictStyle>();

            foreach (var style in styles)
            {
                districtStyleList.Add(style);
            }

            List<string> list = new List<string>();
            foreach (DistrictStyle style in districtStyleList)
            {
                if (style.Name.Equals(DistrictStyle.kEuropeanStyleName)) list.Add("European / Vanilla (" + style.Count.ToString() + ")");
                else if (style.Name.Equals(DistrictStyle.kEuropeanSuburbiaStyleName)) list.Add("European Suburbia (" + style.Count.ToString() + ")");
                else if (style.Name.Equals(DistrictStyle.kModderPack5StyleName)) list.Add("Modern City Center (" + style.Count.ToString() + ")");
                else if (style.Name.Equals(DistrictStyle.kModderPack11StyleName)) list.Add("Mid-Century Modern (" + style.Count.ToString() + ")");
                else if (style.Name.Equals(DistrictStyle.kModderPack14StyleName)) list.Add("Heart of Korea (" + style.Count.ToString() + ")");
                else list.Add(style.FullName + " (" + style.Count.ToString() + ")");
            }

            districtStyleListStrArray = list.ToArray();
            districtStyleDropDownMenu.items = districtStyleListStrArray;
            districtStyleDropDownMenu.selectedIndex = 0;
        }

        private void UpdateDistrictStyleVisibility(bool visibility)
        {
            districtStyleDropDownMenu.isVisible = visibility;
        }

        private void HideAll()
        {
            UpdateAssetCreatorOptionVisibility(false);
            UpdateBuildingHeightOptionVisibility(false);
            UpdateBuildingLevelOptionVisibility(false);
            UpdateUnusedAssetsVisibility(false);
            UpdateUsedAssetsVisibility(false);
            UpdateDLCVisibility(false);
            UpdateDistrictStyleVisibility(false);
        }
    }
}
