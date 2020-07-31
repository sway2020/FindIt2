﻿// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace FindIt.GUI
{
    public class UIFilterExtra : UIPanel
    {
        public static UIFilterExtra instance;

        public UICheckBox optionDropDownCheckBox;
        public UIDropDown optionDropDownMenu;

        public UIDropDown assetCreatorDropDownMenu;

        // building height
        private UILabel minLabel;
        private UILabel maxLabel;
        public UITextField minInput;
        public UITextField maxInput;
        public UIDropDown builingHeightUnit;
        public float minBuildingHeight = float.MinValue;
        public float maxBuildingHeight = float.MaxValue;

        private List<KeyValuePair<string, int>> assetCreatorList;
        public string[] assetCreatorListStrArray;

        string[] options = {
                    Translations.Translate("FIF_EF_AC"), // Asset creator
                    Translations.Translate("FIF_EF_BH") // Building height
                };

        public override void Start()
        {
            instance = this;

            // tag dropdown filter checkbox
            optionDropDownCheckBox = SamsamTS.UIUtils.CreateCheckBox(this);
            optionDropDownCheckBox.isChecked = false;
            optionDropDownCheckBox.width = 20;
            optionDropDownCheckBox.relativePosition = new Vector3(10, 10);
            optionDropDownCheckBox.eventCheckChanged += (c, i) =>
            {
                ((UISearchBox)parent).Search();
            };

            // tag dropdown
            optionDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            optionDropDownMenu.size = new Vector2(200, 25);
            optionDropDownMenu.listHeight = 210;
            optionDropDownMenu.itemHeight = 30;
            optionDropDownMenu.items = options;
            optionDropDownMenu.selectedIndex = 0;
            optionDropDownMenu.relativePosition = new Vector3(optionDropDownCheckBox.relativePosition.x + optionDropDownCheckBox.width + 5, 5);
            optionDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if(optionDropDownMenu.selectedIndex == 0)
                {
                    UpdateAssetCreatorOptionVisibility(true);
                    UpdateBuildingHeightOptionVisibility(false);
                }
                else
                {
                    UpdateAssetCreatorOptionVisibility(false);
                    UpdateBuildingHeightOptionVisibility(true);
                }

                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // asset creator dropdown
            assetCreatorDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            assetCreatorDropDownMenu.size = new Vector2(270, 25);
            assetCreatorDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            assetCreatorDropDownMenu.listHeight = 300;
            assetCreatorDropDownMenu.itemHeight = 30;
            UpdateAssetCreatorList();
            assetCreatorDropDownMenu.isVisible = true;
            assetCreatorDropDownMenu.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);
            assetCreatorDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // building height min label
            minLabel = this.AddUIComponent<UILabel>();
            minLabel.textScale = 0.8f;
            minLabel.padding = new RectOffset(0, 0, 8, 0);
            minLabel.text = "Min:";
            minLabel.isVisible = false;
            minLabel.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);

            // building height min input box
            minInput = SamsamTS.UIUtils.CreateTextField(this);
            minInput.size = new Vector2(60, 25);
            minInput.padding.top = 5;
            minInput.isVisible = false;
            minInput.text = "";
            minInput.relativePosition = new Vector3(minLabel.relativePosition.x + minLabel.width + 10, 5);
            minInput.eventTextChanged += (c, p) =>
            {
                if (float.TryParse(minInput.text, out minBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1)
                    {
                        minBuildingHeight *= 0.3048f;
                    }
                    ((UISearchBox)parent).Search();

                }
                if (minInput.text == "")
                {
                    minBuildingHeight = float.MinValue;
                    ((UISearchBox)parent).Search();
                }
            };

            // building height max label
            maxLabel = this.AddUIComponent<UILabel>();
            maxLabel.textScale = 0.8f;
            maxLabel.padding = new RectOffset(0, 0, 8, 0);
            maxLabel.text = "Max:";
            maxLabel.isVisible = false;
            maxLabel.relativePosition = new Vector3(minInput.relativePosition.x + minInput.width + 20, 5);

            // building height max input box
            maxInput = SamsamTS.UIUtils.CreateTextField(this);
            maxInput.size = new Vector2(60, 25);
            maxInput.padding.top = 5;
            maxInput.isVisible = false;
            maxInput.text = "";
            maxInput.relativePosition = new Vector3(maxLabel.relativePosition.x + maxLabel.width + 10, 5);
            maxInput.eventTextChanged += (c, p) =>
            {

                if (float.TryParse(maxInput.text, out maxBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1)
                    {
                        maxBuildingHeight *= 0.3048f;
                    }
                    ((UISearchBox)parent).Search();
                }

                if (maxInput.text == "")
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
            builingHeightUnit.relativePosition = new Vector3(maxInput.relativePosition.x + maxInput.width + 30, 5);
            builingHeightUnit.eventSelectedIndexChanged += (c, p) =>
            {
                if (float.TryParse(minInput.text, out minBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1) minBuildingHeight *= 0.3048f;
                }

                if (float.TryParse(maxInput.text, out maxBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1) maxBuildingHeight *= 0.3048f;
                }
                if (minInput.text == "") minBuildingHeight = float.MinValue;
                if (maxInput.text == "") maxBuildingHeight = float.MaxValue;
                ((UISearchBox)parent).Search();
            };
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

        public void UpdateAssetCreatorOptionVisibility(bool visibility)
        {
            assetCreatorDropDownMenu.isVisible = visibility;
        }

        public void UpdateBuildingHeightOptionVisibility(bool visibility)
        {
            minLabel.isVisible = visibility;
            minInput.isVisible = visibility;
            maxLabel.isVisible = visibility;
            maxInput.isVisible = visibility;
            builingHeightUnit.isVisible = visibility;
        }
    }
}