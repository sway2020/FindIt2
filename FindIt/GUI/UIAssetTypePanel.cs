// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIAssetTypePanel : UIPanel
    {
        public static UIAssetTypePanel instance;
        // private UIDragHandle m_dragHandle;

        public enum Category
        {
            Network = 0,
            Ploppable,
            Prop,
            Decal,
            Tree,
            Growable,
            Rico,
            GrowableRico
        }

        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "Ingame",
                "Ingame",
                "FindItAtlas",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "FindItAtlas"
            };

            public static readonly string[] spriteNames =
            {
                "SubBarRoadsSmall",
                "ToolbarIconMonuments",
                "ToolbarIconPropsBillboards",
                "SubBarLandscaping",
                "IconPolicyForest",
                "ToolbarIconZoning",
                "IconPolicySmallBusiness",
                "GrwbRico"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_SE_IN"), // network
                Translations.Translate("FIF_SE_IP"), // ploppable
                Translations.Translate("FIF_SE_IPR"), // prop
                Translations.Translate("FIF_SE_ID"), // decal
                Translations.Translate("FIF_SE_IT"), // tree
                Translations.Translate("FIF_SE_IG"), // growable
                Translations.Translate("FIF_SE_IR"), // rico
                Translations.Translate("FIF_SE_IGR") // growable/rico
            };
        }

        private UICheckBox[] toggles;

        public override void Start()
        {
            instance = this;

            tooltip = Translations.Translate("FIF_ATP_TP");

            //m_dragHandle = AddUIComponent<UIDragHandle>();
            //m_dragHandle.target = instance;
            //m_dragHandle.relativePosition = Vector3.zero;
            //m_dragHandle.size = new Vector2(75, 145);

            eventPositionChanged += (c, p) =>
            {
                Settings.assetTypePanelX = relativePosition.x;
                Settings.assetTypePanelY = relativePosition.y;
                XMLUtils.SaveSettings();
            };

            toggles = new UICheckBox[8];
            for (int i = 0; i < 8; i++)
            {
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i], 0.5f, 30f);
                toggles[i].tooltip = CategoryIcons.tooltips[i];

                if (i < 4)
                {
                    toggles[i].relativePosition = new Vector3(5, 5 + 35 * i);

                }
                else
                {
                    toggles[i].relativePosition = new Vector3(40, 5 + 35 * (i - 4));
                }

                toggles[i].isChecked = true;
                toggles[i].readOnly = true;
                toggles[i].checkedBoxObject.isInteractive = false;

                toggles[i].eventClick += (c, p) =>
                {
                    // when all tabs are checked, toggle a tab will uncheck all the other tabs
                    bool check = true;
                    for (int j = 0; j < 8; j++)
                    {
                        check = check && toggles[j].isChecked;
                    }
                    if (check)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            toggles[j].isChecked = false;
                        }
                        ((UICheckBox)c).isChecked = true;
                        SelectAssetType();
                        return;
                    }
                    // when a tab is unchecked, toggle it will uncheck all the other tabs
                    if (((UICheckBox)c).isChecked == false)
                    {
                        for (int j = 0; j < 8; j++)
                            toggles[j].isChecked = false;
                        ((UICheckBox)c).isChecked = true;
                        SelectAssetType();
                        return;
                    }

                    // when a tab is already checked, toggle it will move back to select all
                    if (((UICheckBox)c).isChecked)
                    {
                        for (int j = 0; j < 8; j++)
                            toggles[j].isChecked = true;

                        UISearchBox.instance.typeFilter.selectedIndex = (int)UISearchBox.DropDownOptions.All;
                        return;
                    }
                };

            }

            if (!FindIt.isRicoEnabled)
            {
                toggles[6].isVisible = false;
                toggles[7].isVisible = false;
            }

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

        private void SelectAssetType()
        {
            int index = 0;
            int count = 0;
            for (int j = 0; j < 8; j++)
            {
                if (toggles[j].isChecked)
                {
                    index = j;
                    count += 1;
                }
            }

            if (count != 1) return;

            Category category = (Category)(index);

            int type = 0;
            if (category == Category.Network) type = (int)UISearchBox.DropDownOptions.Network;
            else if (category == Category.Ploppable) type = (int)UISearchBox.DropDownOptions.Ploppable;
            else if (category == Category.Growable) type = (int)UISearchBox.DropDownOptions.Growable;
            else if (category == Category.Prop) type = (int)UISearchBox.DropDownOptions.Prop;
            else if (category == Category.Decal) type = (int)UISearchBox.DropDownOptions.Decal;
            else if (category == Category.Tree) type = (int)UISearchBox.DropDownOptions.Tree;
            else if (category == Category.GrowableRico) type = (int)UISearchBox.DropDownOptions.GrwbRico;
            else if (category == Category.Rico) type = (int)UISearchBox.DropDownOptions.Rico;

            if (!FindIt.isRicoEnabled && type >= (int)UISearchBox.DropDownOptions.Rico)
            {
                type -= 2;
            }

            UISearchBox.instance.typeFilter.selectedIndex = type;
        }

        private void SelectTab(int tab)
        {
            if (tab == -1)
            {
                for (int i = 0; i < 8; i++)
                {
                    toggles[i].isChecked = true;
                }
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                toggles[i].isChecked = false;
            }

            toggles[tab].isChecked = true;
        }

        public void SetSelectedTab(UISearchBox.DropDownOptions option)
        {
            int type = -1;
            if (option == UISearchBox.DropDownOptions.Network) type = (int)Category.Network;
            else if (option == UISearchBox.DropDownOptions.Ploppable) type = (int)Category.Ploppable;
            else if (option == UISearchBox.DropDownOptions.Tree) type = (int)Category.Tree;
            else if (option == UISearchBox.DropDownOptions.Prop) type = (int)Category.Prop;
            else if (option == UISearchBox.DropDownOptions.Growable) type = (int)Category.Growable;
            else if (option == UISearchBox.DropDownOptions.GrwbRico) type = (int)Category.GrowableRico;
            else if (option == UISearchBox.DropDownOptions.Rico) type = (int)Category.Rico;
            else if (option == UISearchBox.DropDownOptions.Decal) type = (int)Category.Decal;

            SelectTab(type);
        }

        private Vector3 deltaPosition;
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.y = m_OwnerView.fixedHeight - mousePosition.y;
                deltaPosition = absolutePosition - mousePosition;
                BringToFront();
            }
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.y = m_OwnerView.fixedHeight - mousePosition.y;
                absolutePosition = mousePosition + deltaPosition;
            }
        }
    }
}
