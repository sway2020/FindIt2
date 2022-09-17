// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIQuickMenuPopUp : UIPanel
    {
        public static UIQuickMenuPopUp instance;
        private const float spacing = 5f;
        private UIDropDown instanceCounterSort;
        private UICheckBox includePOinstances;

        public override void Start()
        {
            name = "FindIt_UIQuickMenuPopUp";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(480, 250);

            UILabel title = AddUIComponent<UILabel>();
            title.text = Translations.Translate("FIF_QM_TIT");
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing * 3, spacing * 3);

            UIButton close = AddUIComponent<UIButton>();
            close.size = new Vector2(30f, 30f);
            close.text = "X";
            close.textScale = 0.9f;
            close.textColor = new Color32(0, 0, 0, 255);
            close.focusedTextColor = new Color32(0, 0, 0, 255);
            close.hoveredTextColor = new Color32(109, 109, 109, 255);
            close.pressedTextColor = new Color32(128, 128, 128, 102);
            close.textPadding = new RectOffset(8, 8, 8, 8);
            close.canFocus = false;
            close.playAudioEvents = true;
            close.relativePosition = new Vector3(width - close.width, 0);
            close.eventClicked += (c, p) => Close();

            // Sort custom tag list alphabetically. Default = sort by number of assets in each tag
            UICheckBox customTagListSort = SamsamTS.UIUtils.CreateCheckBox(this);
            customTagListSort.isChecked = Settings.customTagListSort;
            customTagListSort.label.text = Translations.Translate("FIF_SET_CTLS");
            customTagListSort.label.textScale = 0.8f;
            customTagListSort.width = size.x;
            customTagListSort.label.textColor = new Color32(0, 0, 0, 255);
            customTagListSort.relativePosition = new Vector3(title.relativePosition.x, title.relativePosition.y + title.height + 20);
            customTagListSort.eventCheckChanged += (c, i) =>
            {
                Settings.customTagListSort = customTagListSort.isChecked;
                XMLUtils.SaveSettings();
                if (UIFilterTagPanel.instance != null)
                {
                    UIFilterTagPanel.instance.UpdateCustomTagList();
                    UISearchBox.instance.Search();
                }
            };

            // Sort asset creator list alphabetically. Default = sort by number of assets in each tag
            UICheckBox assetCreatorListSort = SamsamTS.UIUtils.CreateCheckBox(this);
            assetCreatorListSort.isChecked = Settings.assetCreatorListSort;
            assetCreatorListSort.label.text = Translations.Translate("FIF_SET_ACLS");
            assetCreatorListSort.label.textScale = 0.8f;
            assetCreatorListSort.width = size.x;
            assetCreatorListSort.label.textColor = new Color32(0, 0, 0, 255);
            assetCreatorListSort.relativePosition = new Vector3(title.relativePosition.x, customTagListSort.relativePosition.y + customTagListSort.height + 10);
            assetCreatorListSort.eventCheckChanged += (c, i) =>
            {
                Settings.assetCreatorListSort = assetCreatorListSort.isChecked;
                XMLUtils.SaveSettings();
                if (UIFilterExtraPanel.instance != null)
                {
                    UIFilterExtraPanel.instance.UpdateAssetCreatorList();
                    UISearchBox.instance.Search();
                }
            };

            // show search tabs
            UICheckBox showSearchTabs = SamsamTS.UIUtils.CreateCheckBox(this);
            showSearchTabs.isChecked = Settings.showSearchTabs;
            showSearchTabs.label.text = Translations.Translate("FIF_SET_STAB");
            showSearchTabs.label.textScale = 0.8f;
            showSearchTabs.width = size.x;
            showSearchTabs.label.textColor = new Color32(0, 0, 0, 255);
            showSearchTabs.relativePosition = new Vector3(title.relativePosition.x, assetCreatorListSort.relativePosition.y + assetCreatorListSort.height + 10);
            showSearchTabs.eventCheckChanged += (c, i) =>
            {
                Settings.showSearchTabs = showSearchTabs.isChecked;
                XMLUtils.SaveSettings();
                UISearchTabPanel.instance.isVisible = Settings.showSearchTabs;
            };

            // Use light background theme
            UICheckBox useLightBackground = SamsamTS.UIUtils.CreateCheckBox(this);
            useLightBackground.isChecked = Settings.useLightBackground;
            useLightBackground.label.text = Translations.Translate("FIF_SET_BACK");
            useLightBackground.label.textScale = 0.8f;
            useLightBackground.width = size.x;
            useLightBackground.label.textColor = new Color32(0, 0, 0, 255);
            useLightBackground.tooltip = Translations.Translate("FIF_SET_BACKTP");
            useLightBackground.relativePosition = new Vector3(title.relativePosition.x, showSearchTabs.relativePosition.y + showSearchTabs.height + 10);
            useLightBackground.eventCheckChanged += (c, i) =>
            {
                Settings.useLightBackground = useLightBackground.isChecked;
                XMLUtils.SaveSettings();
                FindIt.instance.UpdateDefaultPanelBackground();
            };

            // Show asset type panel
            UICheckBox showAssetTypePanel = SamsamTS.UIUtils.CreateCheckBox(this);
            showAssetTypePanel.isChecked = Settings.showAssetTypePanel;
            showAssetTypePanel.label.text = Translations.Translate("FIF_SET_ATP");
            showAssetTypePanel.label.textScale = 0.8f;
            showAssetTypePanel.width = size.x;
            showAssetTypePanel.label.textColor = new Color32(0, 0, 0, 255);
            showAssetTypePanel.tooltip = Translations.Translate("FIF_ATP_TP");
            showAssetTypePanel.relativePosition = new Vector3(title.relativePosition.x, useLightBackground.relativePosition.y + useLightBackground.height + 10);
            showAssetTypePanel.eventCheckChanged += (c, i) =>
            {
                Settings.showAssetTypePanel = showAssetTypePanel.isChecked;
                XMLUtils.SaveSettings();
                if (Settings.showAssetTypePanel)
                {
                    UISearchBox.instance.assetTypePanel.isVisible = true;
                }
                else
                {
                    UISearchBox.instance.assetTypePanel.isVisible = false;
                    UISearchBox.instance.assetTypePanel.relativePosition = new Vector2(-80.0f, -75.0f);
                }
            };

            // Show the number of existing instances of each asset
            UICheckBox showInstancesCounter = SamsamTS.UIUtils.CreateCheckBox(this);
            showInstancesCounter.isChecked = Settings.showInstancesCounter;
            showInstancesCounter.label.text = Translations.Translate("FIF_SET_IC");
            showInstancesCounter.label.textScale = 0.8f;
            showInstancesCounter.width = size.x;
            showInstancesCounter.tooltip = Translations.Translate("FIF_SET_ICTP");
            showInstancesCounter.label.textColor = new Color32(0, 0, 0, 255);
            showInstancesCounter.relativePosition = new Vector3(title.relativePosition.x, showAssetTypePanel.relativePosition.y + showAssetTypePanel.height + 10);
            showInstancesCounter.eventCheckChanged += (c, i) =>
            {
                if (showInstancesCounter.isChecked)
                {
                    height = instanceCounterSort.relativePosition.y + instanceCounterSort.height + 30;
                }
                else
                {
                    height = showInstancesCounter.relativePosition.y + showInstancesCounter.height + 30;
                }

                Settings.showInstancesCounter = showInstancesCounter.isChecked;
                instanceCounterSort.isVisible = showInstancesCounter.isChecked;
                if (FindIt.isPOEnabled)
                {
                    includePOinstances.isVisible = showInstancesCounter.isChecked;
                }
                XMLUtils.SaveSettings();
                if (Settings.showInstancesCounter && AssetTagList.instance?.prefabInstanceCountDictionary != null)
                {
                    AssetTagList.instance.UpdatePrefabInstanceCount(UISearchBox.DropDownOptions.All);
                }
                UISearchBox.instance.Search();
            };

            // include PO instances
            includePOinstances = SamsamTS.UIUtils.CreateCheckBox(this);
            includePOinstances.isChecked = Settings.includePOinstances;
            includePOinstances.label.text = Translations.Translate("FIF_SET_PO");
            includePOinstances.label.textScale = 0.8f;
            includePOinstances.width = size.x;
            includePOinstances.tooltip = Translations.Translate("FIF_SET_ICTP");
            includePOinstances.isVisible = Settings.showInstancesCounter;
            includePOinstances.label.textColor = new Color32(0, 0, 0, 255);
            includePOinstances.relativePosition = new Vector3(showInstancesCounter.relativePosition.x + 30, showInstancesCounter.relativePosition.y + showInstancesCounter.height + 10);
            includePOinstances.eventCheckChanged += (c, i) =>
            {
                Settings.includePOinstances = includePOinstances.isChecked;
                XMLUtils.SaveSettings();
                if (Settings.showInstancesCounter)
                {
                    UISearchBox.instance.Search();
                }
            };

            instanceCounterSort = SamsamTS.UIUtils.CreateDropDown(this);
            instanceCounterSort.normalBgSprite = "TextFieldPanelHovered";
            instanceCounterSort.size = new Vector2(300, 30);
            instanceCounterSort.listHeight = 300;
            instanceCounterSort.itemHeight = 30;
            instanceCounterSort.AddItem(Translations.Translate("FIF_SET_ICO"));
            instanceCounterSort.AddItem(Translations.Translate("FIF_SET_ICUS"));
            instanceCounterSort.AddItem(Translations.Translate("FIF_SET_ICUN"));
            instanceCounterSort.selectedIndex = Settings.instanceCounterSort;
            instanceCounterSort.isVisible = Settings.showInstancesCounter;
            instanceCounterSort.relativePosition = new Vector3(showInstancesCounter.relativePosition.x + 30, includePOinstances.relativePosition.y + includePOinstances.height + 10);
            instanceCounterSort.eventSelectedIndexChanged += (c, p) =>
            {
                Settings.instanceCounterSort = instanceCounterSort.selectedIndex;
                XMLUtils.SaveSettings();
                if (Settings.showInstancesCounter)
                {
                    UISearchBox.instance.Search();
                }
            };

            if (!FindIt.isPOEnabled)
            {
                includePOinstances.isVisible = false;
                instanceCounterSort.relativePosition = new Vector3(showInstancesCounter.relativePosition.x + 30, showInstancesCounter.relativePosition.y + showInstancesCounter.height + 10);
            }

            if (showInstancesCounter.isChecked)
            {
                height = instanceCounterSort.relativePosition.y + instanceCounterSort.height + 30;
            }
            else
            {
                height = showInstancesCounter.relativePosition.y + showInstancesCounter.height + 30;
            }

            customTagListSort.Focus();
        }

        private static void Close()
        {
            if (instance != null)
            {
                //UIView.PopModal();
                UISearchBox.instance.quickMenuVisible = false;
                UISearchBox.instance.quickMenuIcon.opacity = 0.5f;
                instance.isVisible = false;
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                Close();
            }

            base.OnKeyDown(p);
        }

        public static void ShowAt(UIComponent component)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UIQuickMenuPopUp)) as UIQuickMenuPopUp;

                //UIView.PushModal(instance);
            }
            instance.Show(true);
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
