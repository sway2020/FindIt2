// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// Filter tabs panel for tree

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIFilterTree : UIPanel
    {
        public static UIFilterTree instance;

        public enum Category
        {
            None = -1,
            SmallTree = 0,
            MediumTree,
            LargeTree,
            All
        }

        public UICheckBox[] toggles;
        public UIButton all;
        private UICheckBox randomIcon;

        public static Category GetCategory(Asset.TreeType treeType)
        {
            switch (treeType)
            {
                case Asset.TreeType.SmallTree: return Category.SmallTree;
                case Asset.TreeType.MediumTree: return Category.MediumTree;
                case Asset.TreeType.LargeTree: return Category.LargeTree;
                default: return Category.None;
            }
        }

        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "FindItAtlas",
                "FindItAtlas",
                "FindItAtlas"
            };

            public static readonly string[] spriteNames =
            {
                "TreeSm",
                "TreeMd",
                "TreeLg"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_TREE_SM"), // small trees
                Translations.Translate("FIF_TREE_MD"), // medium trees
                Translations.Translate("FIF_TREE_LG") // large trees
            };
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

            /*atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericTabHovered";*/
            size = new Vector2(605, 45);

            // generate filter tabs UI
            toggles = new UICheckBox[(int)Category.All];
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i], 0.4f);
                toggles[i].tooltip = CategoryIcons.tooltips[i] + "\n" + Translations.Translate("FIF_SE_SC");
                toggles[i].relativePosition = new Vector3(5 + 40 * i, 5);
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

            UICheckBox last = toggles[toggles.Length - 1];

            randomIcon = SamsamTS.UIUtils.CreateIconToggle(this, "FindItAtlas", "Dice", "Dice");
            randomIcon.relativePosition = new Vector3(last.relativePosition.x + last.width + 5, 5);
            randomIcon.tooltip = Translations.Translate("FIF_GR_RAN");
            randomIcon.isChecked = true;
            randomIcon.readOnly = true;
            randomIcon.checkedBoxObject.isInteractive = false;
            randomIcon.eventClicked += (c, p) =>
            {
                UISearchBox.instance.PickRandom();
            };

            //all = SamsamTS.UIUtils.CreateButton(this);
            //all.size = new Vector2(55, 35);
            //all.text = Translations.Translate("FIF_SE_IA");
            //all.relativePosition = new Vector3(randomIcon.relativePosition.x + randomIcon.width + 5, 5);

            //all.eventClick += (c, p) =>
            //{
            //    for (int i = 0; i < (int)Category.All; i++)
            //    {
            //        toggles[i].isChecked = true;
            //    }
            //    eventFilteringChanged(this, 0);
            //};

            width = parent.width;
        }
    }
}
