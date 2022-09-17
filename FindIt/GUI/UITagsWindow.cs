// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FindIt.GUI
{
    public class UITagsWindow : UIPanel
    {
        public static UITagsWindow instance;

        public UITextField input;

        private UIComponent m_tagSprite;

        private UIPanel m_tagsPanel;

        private Asset m_asset;

        private const float spacing = 5f;

        private UIDropDown tagDropDownMenu;
        private List<KeyValuePair<string, int>> customTagList;
        private string[] customTagListStrArray;

        private UIButton tagDropDownAddButton;

        private UILabel tagDropDownMenuMessage;
        private UILabel inputMessage;

        public override void Start()
        {
            name = "FindIt_TagsWindow";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(320, 300);

            UILabel title = AddUIComponent<UILabel>();
            title.text = Translations.Translate("FIF_CT_TIT");
            title.textScale = 0.9f;
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing, spacing);

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

            m_tagsPanel = AddUIComponent<UIPanel>();
            m_tagsPanel.size = new Vector2(width - 2 * spacing, height - 70);
            m_tagsPanel.autoFitChildrenVertically = true;
            m_tagsPanel.autoLayout = true;
            m_tagsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            m_tagsPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_tagsPanel.autoLayoutStart = LayoutStart.TopLeft;
            m_tagsPanel.wrapLayout = true;
            m_tagsPanel.relativePosition = new Vector3(spacing, title.relativePosition.y + title.height + spacing);

            tagDropDownMenuMessage = AddUIComponent<UILabel>();
            tagDropDownMenuMessage.text = Translations.Translate("FIF_CT_DDMSG");
            tagDropDownMenuMessage.textScale = 0.9f;
            tagDropDownMenuMessage.textColor = new Color32(0, 0, 0, 255);
            tagDropDownMenuMessage.relativePosition = new Vector3(spacing, m_tagsPanel.relativePosition.y + m_tagsPanel.height + spacing * 6);

            // tag dropdown
            tagDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            tagDropDownMenu.normalBgSprite = "TextFieldPanelHovered";
            tagDropDownMenu.size = new Vector2(width - 2 * spacing - 50, 30);
            tagDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            tagDropDownMenu.listHeight = 300;
            tagDropDownMenu.itemHeight = 30;
            tagDropDownMenu.relativePosition = new Vector3(spacing, tagDropDownMenuMessage.relativePosition.y + tagDropDownMenuMessage.height + spacing);
            UpdateCustomTagList();
            SamsamTS.UIUtils.CreateDropDownScrollBar(tagDropDownMenu);

            // tag dropdown add button
            tagDropDownAddButton = SamsamTS.UIUtils.CreateButton(this);
            tagDropDownAddButton.size = new Vector2(35, 30);
            tagDropDownAddButton.text = "+";
            tagDropDownAddButton.tooltip = Translations.Translate("FIF_CT_DDTP");
            tagDropDownAddButton.relativePosition = new Vector3(spacing + tagDropDownMenu.width + 5, tagDropDownMenu.relativePosition.y);
            tagDropDownAddButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length == 0) return;
                string newTag = GetDropDownListKey();
                if (!m_asset.tagsCustom.Contains(newTag))
                {
                    AssetTagList.instance.AddCustomTags(m_asset, newTag);
                }
                Display(m_asset);
            };

            inputMessage = AddUIComponent<UILabel>();
            inputMessage.text = Translations.Translate("FIF_CT_ILBL1") + "\n" + Translations.Translate("FIF_CT_ILBL2");
            inputMessage.textScale = 0.9f;
            inputMessage.textColor = new Color32(0, 0, 0, 255);
            inputMessage.relativePosition = new Vector3(spacing, tagDropDownMenu.relativePosition.y + tagDropDownMenu.height + spacing * 2);

            input = SamsamTS.UIUtils.CreateTextField(this);
            input.size = new Vector2(width - 2 * spacing, 30);
            input.padding.top = 7;
            input.tooltip = Translations.Translate("FIF_CT_ITP");
            input.relativePosition = new Vector3(spacing, inputMessage.relativePosition.y + inputMessage.height + spacing);
            input.submitOnFocusLost = false;
            input.eventTextSubmitted += (c, t) =>
            {
                AssetTagList.instance.AddCustomTags(m_asset, t);
                Display(m_asset);
            };

            Display(m_asset);
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

        public static void ShowAt(Asset asset, UIComponent component)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UITagsWindow)) as UITagsWindow;

                instance.m_tagSprite = component;
                instance.m_asset = asset;

                instance.Show(true);

                UIView.PushModal(instance);
            }
            else
            {
                instance.m_tagSprite = component;
                instance.Display(asset);

                instance.Show(true);
            }
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

        private void Display(Asset asset)
        {
            if (asset == null) return;

            m_asset = asset;

            m_tagsPanel.autoLayout = false;

            UITag[] tags = m_tagsPanel.GetComponentsInChildren<UITag>();

            foreach (UITag tag in tags)
            {
                DestroyImmediate(tag.gameObject);
            }

            if (asset.tagsCustom.Count == 0)
            {
                m_tagsPanel.height = 0;
            }
            else
            {
                foreach (string t in asset.tagsCustom.OrderBy(s => s))
                {
                    UITag tag = m_tagsPanel.AddUIComponent<UITag>();
                    tag.text = t;
                    tag.asset = asset;
                }

                m_tagsPanel.autoLayout = true;
                m_tagsPanel.Reset();
            }

            Refresh(asset);
            UISearchBox.instance.scrollPanel.Refresh();
        }

        public void Refresh(Asset asset)
        {
            if (asset.tagsCustom.Count == 0)
            {
                m_tagSprite.isVisible = false;
            }
            else
            {
                m_tagSprite.opacity = 0.5f;
                m_tagSprite.isVisible = true;
            }

            height = m_tagsPanel.relativePosition.y + m_tagsPanel.height + 6 * spacing + tagDropDownMenuMessage.height + spacing + tagDropDownMenu.height + spacing * 2 + inputMessage.height + spacing + input.height + spacing;
            tagDropDownMenuMessage.relativePosition = new Vector3(spacing, m_tagsPanel.relativePosition.y + m_tagsPanel.height + spacing * 6);
            tagDropDownMenu.relativePosition = new Vector3(spacing, tagDropDownMenuMessage.relativePosition.y + tagDropDownMenuMessage.height + spacing);
            tagDropDownAddButton.relativePosition = new Vector3(spacing + tagDropDownMenu.width + 5, tagDropDownMenu.relativePosition.y);
            inputMessage.relativePosition = new Vector3(spacing, tagDropDownMenu.relativePosition.y + tagDropDownMenu.height + spacing * 2);
            input.relativePosition = new Vector3(spacing, inputMessage.relativePosition.y + inputMessage.height + spacing);

            input.text = "";
            input.Focus();

            if (m_tagSprite.absolutePosition.x + width > UIView.GetAView().fixedWidth)
            {
                absolutePosition = new Vector3(m_tagSprite.absolutePosition.x - width, m_tagSprite.absolutePosition.y - height);
            }
            else
            {
                absolutePosition = new Vector3(m_tagSprite.absolutePosition.x, m_tagSprite.absolutePosition.y - height);
            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input == null) return;

            if (isVisible)
            {
                input.Focus();
            }
            else
            {
                input.text = "";
                input.Unfocus();
            }
        }

        // Update custom tag list 
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
    }
}
