// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// Filter tabs panel for growable

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIFilterGrowable : UIPanel
    {
        public static UIFilterGrowable instance;

        public enum Category
        {
            None = -1,
            ResidentialLow = 0,
            ResidentialHigh,
            ResidentialLowEco,
            ResidentialHighEco,
            ResidentialWallToWall,
            CommercialLow,
            CommercialHigh,
            CommercialEco,
            CommercialWallToWall,
            OfficeGeneric,
            OfficeHightech,
            OfficeWallToWall,
            Industrial,
            Farming,
            Forestry,
            Oil,
            Ore,
            CommercialLeisure,
            CommercialTourism,
            Unsorted,
            All
        }

        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "Thumbnails",
                "Thumbnails",
                "FindItAtlas",
                "FindItAtlas",
                "Ingame",
                "Thumbnails",
                "Thumbnails",
                "Thumbnails",
                "Ingame",
                "Thumbnails",
                "Thumbnails",
                "Ingame",
                "Thumbnails",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame"
            };

            public static readonly string[] spriteNames =
            {
                "ZoningResidentialLow",
                "ZoningResidentialHigh",
                "ZoningResidentialLowEco",
                "ZoningResidentialHighEco",
                "IconPolicyResidentialWallToWall",
                "ZoningCommercialLow",
                "ZoningCommercialHigh",
                "DistrictSpecializationOrganic",
                "IconPolicyCommercialWallToWall",
                "ZoningOffice",
                "DistrictSpecializationHightech",
                "IconPolicyOfficeWallToWall",
                "ZoningIndustrial",
                "IconPolicyFarming",
                "IconPolicyForest",
                "IconPolicyOil",
                "IconPolicyOre",
                "IconPolicyLeisure",
                "IconPolicyTourist",
                "ToolbarIconHelp"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_GR_LDR"), // low density residential
                Translations.Translate("FIF_GR_HDR"), // high density residential
                Translations.Translate("FIF_GR_LDRE"), // low density residential eco
                Translations.Translate("FIF_GR_HDRE"), // high density residential eco
                Translations.Translate("FIF_GR_RW2W"), // residential wall-to-wall
                Translations.Translate("FIF_GR_LDC"), // low density commercial
                Translations.Translate("FIF_GR_HDC"), // high density commercial
                Translations.Translate("FIF_GR_CE"), // commercial eco
                Translations.Translate("FIF_GR_CW2W"), // commercial wall-to-wall
                Translations.Translate("FIF_GR_O"), // office
                Translations.Translate("FIF_GR_ITC"), // IT cluster
                Translations.Translate("FIF_GR_OW2W"), // office wall-to-wall
                Translations.Translate("FIF_GR_GI"), // industrial
                Translations.Translate("FIF_GR_FAI"), // farming
                Translations.Translate("FIF_GR_FOI"), // forest
                Translations.Translate("FIF_GR_OII"), // oil
                Translations.Translate("FIF_GR_ORI"), // ore
                Translations.Translate("FIF_GR_LC"), // leisure commercial
                Translations.Translate("FIF_GR_TC"), // tourist commercial
                Translations.Translate("FIF_PROP_UNS") // Unsorted
            };
        }

        public UICheckBox[] toggles;
        private UICheckBox randomIcon;

        public static Category GetCategory(ItemClass itemClass)
        {
            switch (itemClass.m_subService)
            {
                case ItemClass.SubService.ResidentialLow: return Category.ResidentialLow;
                case ItemClass.SubService.ResidentialHigh: return Category.ResidentialHigh;
                case ItemClass.SubService.ResidentialLowEco: return Category.ResidentialLowEco;
                case ItemClass.SubService.ResidentialHighEco: return Category.ResidentialHighEco;
                case ItemClass.SubService.ResidentialWallToWall: return Category.ResidentialWallToWall;
                case ItemClass.SubService.CommercialLow: return Category.CommercialLow;
                case ItemClass.SubService.CommercialHigh: return Category.CommercialHigh;
                case ItemClass.SubService.CommercialEco: return Category.CommercialEco;
                case ItemClass.SubService.CommercialWallToWall: return Category.CommercialWallToWall;
                case ItemClass.SubService.CommercialLeisure: return Category.CommercialLeisure;
                case ItemClass.SubService.CommercialTourist: return Category.CommercialTourism;
                case ItemClass.SubService.IndustrialGeneric: return Category.Industrial;
                case ItemClass.SubService.IndustrialFarming: return Category.Farming;
                case ItemClass.SubService.IndustrialForestry: return Category.Forestry;
                case ItemClass.SubService.IndustrialOil: return Category.Oil;
                case ItemClass.SubService.IndustrialOre: return Category.Ore;
                case ItemClass.SubService.OfficeGeneric: return Category.OfficeGeneric;
                case ItemClass.SubService.OfficeHightech: return Category.OfficeHightech;
                case ItemClass.SubService.OfficeWallToWall: return Category.OfficeWallToWall;
                default: return Category.Unsorted;
            }
        }

        public bool IsSelected(Category category)
        {
            return toggles[(int)category].isChecked;
        }

        public bool IsAllSelected()
        {
            for (int i = 0; i < (int)Category.All; i++)
            {
                if (!toggles[i].isChecked)
                {
                    return false;
                }
            }
            return true;
        }

        public void SelectAll()
        {
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i].isChecked = true;
            }
        }

        public event PropertyChangedEventHandler<int> eventFilteringChanged;

        public override void Start()
        {
            instance = this;

            size = new Vector2(605, 45);

            // Zoning
            toggles = new UICheckBox[(int)Category.All];
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i], 0.4f);
                toggles[i].tooltip = CategoryIcons.tooltips[i] + "\n" + Translations.Translate("FIF_SE_SC");
                toggles[i].relativePosition = new Vector3(5 + 37 * i, 5);
                toggles[i].isChecked = true;
                toggles[i].readOnly = true;
                toggles[i].checkedBoxObject.isInteractive = false;

                toggles[i].eventClick += (c, p) =>
                {
                    Event e = Event.current;

                    if (e.shift || e.control)
                    {
                        ((UICheckBox)c).isChecked = !((UICheckBox)c).isChecked;
                        eventFilteringChanged(this, 0);
                    }
                    else
                    {
                        // when all tabs are checked, toggle a tab will uncheck all the other tabs
                        bool check = true;
                        for (int j = 0; j < (int)Category.All; j++)
                        {
                            check = check && toggles[j].isChecked;
                        }
                        if (check)
                        {
                            for (int j = 0; j < (int)Category.All; j++)
                            {
                                toggles[j].isChecked = false;
                            }
                            ((UICheckBox)c).isChecked = true;
                            eventFilteringChanged(this, 0);
                            return;
                        }

                        // when a tab is unchecked, toggle it will uncheck all the other tabs
                        if (((UICheckBox)c).isChecked == false)
                        {
                            for (int j = 0; j < (int)Category.All; j++)
                                toggles[j].isChecked = false;
                            ((UICheckBox)c).isChecked = true;
                            eventFilteringChanged(this, 0);
                            return;
                        }

                        // when a tab is already checked, toggle it will move back to select all
                        if (((UICheckBox)c).isChecked)
                        {
                            for (int j = 0; j < (int)Category.All; j++)
                                toggles[j].isChecked = true;
                            eventFilteringChanged(this, 0);
                            return;
                        }
                    }
                };
            }

            UICheckBox last = toggles[toggles.Length - 1];

            randomIcon = SamsamTS.UIUtils.CreateIconToggle(this, "FindItAtlas", "Dice", "Dice");
            randomIcon.relativePosition = new Vector3(last.relativePosition.x + last.width + 3, 5);
            randomIcon.tooltip = Translations.Translate("FIF_GR_RAN");
            randomIcon.isChecked = true;
            randomIcon.readOnly = true;
            randomIcon.checkedBoxObject.isInteractive = false;
            randomIcon.eventClicked += (c, p) =>
            {
                UISearchBox.instance.PickRandom();
            };

            width = parent.width;
        }
    }
}
