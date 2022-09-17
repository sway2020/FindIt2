// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// Filter tabs panel for network

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIFilterNetwork : UIPanel
    {
        public static UIFilterNetwork instance;

        public enum Category
        {
            None = -1,
            TinyRoads = 0,
            SmallRoads,
            MediumRoads,
            LargeRoads,
            Highway,
            Path,
            ZonablePath,
            Fence,
            WaterStructures,
            Utility,
            Train,
            Metro,
            Aircraft,
            OneWay,
            Parking,
            NoParking,
            Bus,
            TrolleyBus,
            Bike,
            Tram,
            Unsorted,
            All
        }

        public UICheckBox[] toggles;
        public UIButton all;
        private UICheckBox randomIcon;

        public static Category GetCategory(Asset.NetworkType networkType)
        {
            switch (networkType)
            {
                case Asset.NetworkType.TinyRoads: return Category.TinyRoads;
                case Asset.NetworkType.SmallRoads: return Category.SmallRoads;
                case Asset.NetworkType.MediumRoads: return Category.MediumRoads;
                case Asset.NetworkType.LargeRoads: return Category.LargeRoads;
                case Asset.NetworkType.Highway: return Category.Highway;
                case Asset.NetworkType.Path: return Category.Path;
                case Asset.NetworkType.ZonablePath: return Category.ZonablePath;
                case Asset.NetworkType.Fence: return Category.Fence;
                case Asset.NetworkType.WaterStructures: return Category.WaterStructures;
                case Asset.NetworkType.Utility: return Category.Utility;
                case Asset.NetworkType.Train: return Category.Train;
                case Asset.NetworkType.Metro: return Category.Metro;
                case Asset.NetworkType.Aircraft: return Category.Aircraft;
                case Asset.NetworkType.Unsorted: return Category.Unsorted;
                default: return Category.None;
            }
        }

        public static bool IsOneWay(NetInfo info)
        {
            if (info.m_hasBackwardVehicleLanes != info.m_hasForwardVehicleLanes) return true;
            return false;
        }

        public static bool HasParking(NetInfo info)
        {
            return info.m_hasParkingSpaces;
        }

        public static bool HasBikeLane(NetInfo info)
        {
            // get all lanes
            foreach (NetInfo.Lane laneInfo in info.m_lanes)
            {
                if ((laneInfo.m_vehicleType & VehicleInfo.VehicleType.Bicycle) != 0) return true;
            }
            return false;
        }

        public static bool HasTramLane(NetInfo info)
        {
            // get all lanes
            foreach (NetInfo.Lane laneInfo in info.m_lanes)
            {
                if ((laneInfo.m_vehicleType & VehicleInfo.VehicleType.Tram) != 0) return true;
            }
            return false;
        }

        public static bool HasBuslane(NetInfo info)
        {
            // get all lanes
            foreach (NetInfo.Lane laneInfo in info.m_lanes)
            {
                if ((laneInfo.m_vehicleType & VehicleInfo.VehicleType.Car) != 0)
                {
                    if ((laneInfo.m_laneType & NetInfo.LaneType.TransportVehicle) != 0) return true;
                }
            }
            return false;
        }

        public static bool HasTrolleyBusLane(NetInfo info)
        {
            // get all lanes
            foreach (NetInfo.Lane laneInfo in info.m_lanes)
            {
                if ((laneInfo.m_vehicleType & VehicleInfo.VehicleType.Trolleybus) != 0) return true;
            }
            return false;
        }

        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "FindItAtlas",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "FindItAtlas",
                "FindItAtlas",
                "FindItAtlas",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame"
            };

            public static readonly string[] spriteNames =
            {
                "TinyRoads",
                "SubBarRoadsSmall",
                "SubBarRoadsMedium",
                "SubBarRoadsLarge",
                "SubBarRoadsHighway",
                "SubBarLandscapingPaths",
                "UIFilterSandstone",
                "SubBarLandscapingFences",
                "SubBarLandscapingWaterStructures",
                "ToolbarIconElectricity",
                "SubBarPublicTransportTrain",
                "SubBarPublicTransportMetro",
                "SubBarPublicTransportPlane",
                "Oneway",
                "Parking",
                "NoParking",
                "SubBarPublicTransportBus",
                "SubBarPublicTransportTrolleybus",
                "IconPolicyEncourageBiking",
                "SubBarPublicTransportTramHovered",
                "ToolbarIconHelp"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_NET_TNR"), // Tiny Roads
                Translations.Translate("FIF_NET_SMR"), // Small Roads
                Translations.Translate("FIF_NET_MDR"), // Medium Roads
                Translations.Translate("FIF_NET_LGR"), // Large Roads
                Translations.Translate("FIF_NET_HGHW"), // Highway
                Translations.Translate("FIF_NET_PATH"), // Path
                Translations.Translate("FIF_NET_ZPATH"), // Zonable Path
                Translations.Translate("FIF_NET_WALL"), // Fence & Wall
                Translations.Translate("FIF_NET_WAT"), // Water Structures
                Translations.Translate("FIF_NET_UTI"), // Utility
                Translations.Translate("FIF_NET_TRA"), // Train
                Translations.Translate("FIF_NET_MET"), // Metro
                Translations.Translate("FIF_PROP_AIR"), // Aircraft
                Translations.Translate("FIF_NET_ONE"), // One-way Roads
                Translations.Translate("FIF_NET_PAR"), // Roads with parking spaces
                Translations.Translate("FIF_NET_NOP"), // Roads without parking spaces
                Translations.Translate("FIF_NET_BUS"), // Roads with bus lanes
                Translations.Translate("FIF_NET_TRO"), // Roads with trolley bus lanes
                Translations.Translate("FIF_NET_BIK"), // Roads and paths with bike lanes
                Translations.Translate("FIF_NET_TRM"), // Roads with tram tracks
                Translations.Translate("FIF_PROP_UNS") // Unsorted
            };
        }

        public bool IsSelected(Category category)
        {
            return toggles[(int)category].isChecked;
        }

        public bool IsOnlySelected(Category category)
        {
            int selected = 0;
            for (int i = 0; i < (int)Category.All; i++)
            {
                if (toggles[i].isChecked) selected += 1;
            }
            if (selected == 1 && IsSelected(category)) return true;
            return false;
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

        public bool IsAnyRoadPathSelected()
        {
            for (int i = (int)Category.TinyRoads; i < (int)Category.Fence; i++)
            {
                if (toggles[i].isChecked)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsAnyExtraFlagSelected()
        {
            for (int i = (int)Category.OneWay; i < (int)Category.Unsorted; i++)
            {
                if (toggles[i].isChecked)
                {
                    return true;
                }
            }
            return false;
        }

        public void SelectAll()
        {
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i].isChecked = true;
            }
        }

        public static bool IsNormalRoads(Asset.NetworkType type)
        {
            if (type != Asset.NetworkType.TinyRoads && type != Asset.NetworkType.SmallRoads &&
                type != Asset.NetworkType.MediumRoads && type != Asset.NetworkType.LargeRoads) return false;
            return true;
        }

        public event PropertyChangedEventHandler<int> eventFilteringChanged;

        public override void Start()
        {
            instance = this;

            size = new Vector2(605, 45);

            // generate filter tabs UI
            toggles = new UICheckBox[(int)Category.All];
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i], 0.4f);
                toggles[i].tooltip = CategoryIcons.tooltips[i] + "\n" + Translations.Translate("FIF_SE_SC");
                toggles[i].relativePosition = new Vector3(5 + 36.5f * i, 5);
                toggles[i].isChecked = true;
                toggles[i].readOnly = true;
                toggles[i].checkedBoxObject.isInteractive = false; // Don't eat my double click event please

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

            // hide "Tiny Roads" tab if the category is empty
            if (!AssetTagList.instance.tinyRoadsExist)
            {
                for (int i = 1; i < (int)Category.All; i++)
                {
                    toggles[i].relativePosition = new Vector3(5 + 36.5f * (i - 1), 5);
                }
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

            /*
            all = SamsamTS.UIUtils.CreateButton(this);
            all.size = new Vector2(55, 35);
            all.text = Translations.Translate("FIF_SE_IA");
            all.relativePosition = new Vector3(randomIcon.relativePosition.x + last.width + 5, 5);

            all.eventClick += (c, p) =>
            {
                for (int i = 0; i < (int)Category.All; i++)
                {
                    toggles[i].isChecked = true;
                }
                eventFilteringChanged(this, 0);
            };
            */

            width = parent.width;
        }
    }
}
