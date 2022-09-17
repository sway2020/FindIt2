// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FindIt.GUI
{
    public class UITagsRenamePopUp : UIPanel
    {
        public static UITagsRenamePopUp instance;
        private UIComponent m_button;

        private const float spacing = 5f;
        private UIButton confirmButton;
        private UIButton cancelButton;
        private UITextField newTagInput;

        private List<KeyValuePair<string, int>> customTagList;
        private string[] customTagListStrArray;

        private UILabel newTagNameLabel;
        private string oldTagName;
        private string newTagName;

        public override void Start()
        {
            name = "FindIt_TagsRenamePopUp";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(400, 200);

            UILabel title = AddUIComponent<UILabel>();
            title.text = Translations.Translate("FIF_RE_TIT");
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
            message.text = "\n" + Translations.Translate("FIF_RE_MSG") + "\n";
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing * 2, spacing + title.height + spacing);

            newTagInput = SamsamTS.UIUtils.CreateTextField(this);
            newTagInput.size = new Vector2(width - spacing * 4, 30);
            newTagInput.padding.top = 7;
            newTagInput.tooltip = Translations.Translate("FIF_RE_ITP");
            newTagInput.relativePosition = new Vector3(spacing * 2, message.relativePosition.y + message.height + spacing);
            newTagInput.submitOnFocusLost = false;
            newTagInput.eventTextSubmitted += (c, t) =>
            {
                // use the first string as the the tag name
                string[] tagsArr = Regex.Split(t, @"([^\w]|[-]|\s)+", RegexOptions.IgnoreCase);
                if (tagsArr[0] != "")
                {
                    newTagName = tagsArr[0];
                }

                if (newTagName == "")
                {
                    newTagNameLabel.text = Translations.Translate("FIF_RE_ELBL");
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

                if (newTagName.Length == 1)
                {
                    newTagNameLabel.text = Translations.Translate("FIF_RE_SLBL");
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

                newTagName = newTagName.ToLower().Trim();
                newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
                newTagNameLabel.text = Translations.Translate("FIF_RE_NEWLBL") + newTagName;
            };

            // display new tag name for confirmation
            newTagNameLabel = AddUIComponent<UILabel>();
            newTagNameLabel.size = new Vector2(200, 50);
            newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
            newTagNameLabel.text = Translations.Translate("FIF_RE_NEWLBL");
            newTagNameLabel.relativePosition = new Vector3(spacing * 2, newTagInput.relativePosition.y + newTagInput.height + spacing * 2);

            confirmButton = SamsamTS.UIUtils.CreateButton(this);
            confirmButton.size = new Vector2(100, 40);
            confirmButton.text = Translations.Translate("FIF_POP_CON");
            confirmButton.relativePosition = new Vector3(spacing * 2, newTagNameLabel.relativePosition.y + newTagNameLabel.height + spacing * 3);
            confirmButton.eventClick += (c, p) =>
            {
                if (newTagName == "")
                {
                    newTagNameLabel.text = Translations.Translate("FIF_RE_ELBL");
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

                if (AssetTagList.instance.tagsCustomDictionary.ContainsKey(newTagName))
                {
                    newTagNameLabel.text = Translations.Translate("FIF_RE_AELBL");
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

                if (newTagName.Length == 1)
                {
                    newTagNameLabel.text = Translations.Translate("FIF_RE_SLBL");
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

                RenameTag(oldTagName, newTagName);
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

        public static void ShowAt(UIComponent component, string currentTag)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UITagsRenamePopUp)) as UITagsRenamePopUp;
                instance.m_button = component;
                instance.Show(true);
                UIView.PushModal(instance);
            }
            else
            {
                instance.m_button = component;
                instance.Show(true);
            }

            instance.oldTagName = currentTag;
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
        }

        // rename or a tag or combine it with another tag
        private void RenameTag(string oldTagName, string newTagName)
        {
            foreach (Asset asset in AssetTagList.instance.assets.Values)
            {
                if (!asset.tagsCustom.Contains(oldTagName)) continue;

                // remove old tag
                AssetTagList.instance.RemoveCustomTag(asset, oldTagName);

                // add new tag
                if (asset.tagsCustom.Contains(newTagName)) continue;
                AssetTagList.instance.AddCustomTags(asset, newTagName);
            }
        }
    }
}
