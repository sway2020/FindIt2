// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt.GUI
{
    public class UITagsBatchRemovePopUp : UIPanel
    {
        public static UITagsBatchRemovePopUp instance;
        private UIComponent m_button;

        private const float spacing = 5f;
        private UIButton confirmButton;
        private UIButton cancelButton;

        private UIDropDown tagDropDownMenu;
        private List<KeyValuePair<string, int>> customTagList;
        private string[] customTagListStrArray;

        private UIButton tagDropDownButton;
        private UILabel newTagNameLabel;
        private string newTagName;

        public override void Start()
        {
            name = "FindIt_BatchRemovePopUp";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(400, 240);

            UILabel title = AddUIComponent<UILabel>();
            title.text = Translations.Translate("FIF_REM_TIT");
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing * 2, spacing * 2);

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

            UILabel message = AddUIComponent<UILabel>();
            message.text = "\n" + Translations.Translate("FIF_ADD_NUM") + " " + UIFilterTagPanel.instance.batchAssetSet.Count + "\n\n" + Translations.Translate("FIF_REM_REM");
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing * 2, spacing + title.height + spacing);

            tagDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            tagDropDownMenu.normalBgSprite = "TextFieldPanelHovered";
            tagDropDownMenu.size = new Vector2(width - 2 * spacing - 100, 30);
            tagDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            tagDropDownMenu.listHeight = 300;
            tagDropDownMenu.itemHeight = 30;
            tagDropDownMenu.relativePosition = new Vector3(spacing * 2, message.relativePosition.y + message.height + spacing * 2);
            UpdateCustomTagList();
            SamsamTS.UIUtils.CreateDropDownScrollBar(tagDropDownMenu);

            tagDropDownButton = SamsamTS.UIUtils.CreateButton(this);
            tagDropDownButton.size = new Vector2(80, 30);
            tagDropDownButton.text = Translations.Translate("FIF_CO_CH");
            tagDropDownButton.textScale = 0.8f;
            tagDropDownButton.tooltip = Translations.Translate("FIF_REM_CHTP");
            tagDropDownButton.relativePosition = new Vector3(tagDropDownMenu.relativePosition.x + tagDropDownMenu.width + 5, tagDropDownMenu.relativePosition.y);
            tagDropDownButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length == 0) return;
                newTagName = GetDropDownListKey();
                newTagNameLabel.text = Translations.Translate("FIF_ADD_TAG") + " " + newTagName;
                newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
            };

            // display new tag name for confirmation
            newTagNameLabel = AddUIComponent<UILabel>();
            newTagNameLabel.size = new Vector2(200, 50);
            newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
            newTagNameLabel.text = Translations.Translate("FIF_ADD_TAG") + " " + newTagName;
            newTagNameLabel.relativePosition = new Vector3(spacing * 2, tagDropDownMenu.relativePosition.y + tagDropDownMenu.height + spacing * 2);

            confirmButton = SamsamTS.UIUtils.CreateButton(this);
            confirmButton.size = new Vector2(100, 40);
            confirmButton.text = Translations.Translate("FIF_POP_CON");
            confirmButton.relativePosition = new Vector3(spacing * 2, newTagNameLabel.relativePosition.y + newTagNameLabel.height + spacing * 3);
            confirmButton.eventClick += (c, p) =>
            {
                if (newTagName == "")
                {
                    newTagNameLabel.text = Translations.Translate("FIF_REM_ELBL");
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

                BatchRemoveTag(newTagName);
                ((UIFilterTagPanel)m_button.parent).UpdateCustomTagList();
                UISearchBox.instance.scrollPanel.Refresh();
                Close();
            };

            cancelButton = SamsamTS.UIUtils.CreateButton(this);
            cancelButton.size = new Vector2(100, 40);
            cancelButton.text = Translations.Translate("FIF_POP_CAN");
            cancelButton.relativePosition = new Vector3(confirmButton.relativePosition.x + confirmButton.width + spacing * 4, confirmButton.relativePosition.y);
            cancelButton.eventClick += (c, p) => Close();

            height = cancelButton.relativePosition.y + cancelButton.height + 10;
            cancelButton.Focus();
        }

        private static void Close()
        {
            if (instance != null)
            {
                UIView.PopModal();

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
                instance = UIView.GetAView().AddUIComponent(typeof(UITagsBatchRemovePopUp)) as UITagsBatchRemovePopUp;
                instance.m_button = component;
                instance.Show(true);
                UIView.PushModal(instance);
            }
            else
            {
                instance.m_button = component;
                instance.Show(true);
            }

            instance.newTagName = "";
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

        private void UpdateCustomTagList()
        {
            customTagList = AssetTagList.instance.GetCustomTagList();

            List<string> list = new List<string>();

            foreach (KeyValuePair<string, int> entry in customTagList)
            {
                list.Add(entry.Key.ToString() + " (" + entry.Value.ToString() + ")");
            }

            customTagListStrArray = list.ToArray();
            tagDropDownMenu.items = customTagListStrArray;
            tagDropDownMenu.selectedIndex = 0;
        }
        private string GetDropDownListKey()
        {
            return customTagList[tagDropDownMenu.selectedIndex].Key;
        }

        private void BatchRemoveTag(string newTagName)
        {
            foreach (Asset asset in UIFilterTagPanel.instance.batchAssetSet)
            {
                if (asset.tagsCustom.Contains(newTagName))
                {
                    AssetTagList.instance.RemoveCustomTag(asset, newTagName);
                }
            }
        }
    }
}
