// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SamsamTS
{
    public class UIUtils
    {
        // From SamsamTS: 
        // Figuring all this was a pain (no documentation whatsoever)
        // So if your are using it for your mod consider thanking me (SamsamTS)
        // Extended Public Transport UI's code helped me a lot so thanks a lot AcidFire
        public static UIButton CreateButton(UIComponent parent)
        {
            UIButton button = (UIButton)parent.AddUIComponent<UIButton>();

            button.atlas = GetAtlas("Ingame");
            button.size = new Vector2(90f, 30f);
            button.textScale = 0.9f;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(80, 80, 80, 128);
            button.canFocus = false;
            button.playAudioEvents = true;

            return button;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent)
        {
            UICheckBox checkBox = (UICheckBox)parent.AddUIComponent<UICheckBox>();

            checkBox.width = 300f;
            checkBox.height = 20f;
            checkBox.clipChildren = true;

            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.atlas = GetAtlas("Ingame");
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = GetAtlas("Ingame");
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = " ";
            checkBox.label.textScale = 0.9f;
            checkBox.label.relativePosition = new Vector3(22f, 2f);

            return checkBox;
        }

        public static UITextField CreateTextField(UIComponent parent)
        {
            UITextField textField = parent.AddUIComponent<UITextField>();

            textField.atlas = GetAtlas("Ingame");
            textField.size = new Vector2(90f, 20f);
            textField.padding = new RectOffset(6, 6, 3, 3);
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            textField.normalBgSprite = "TextFieldPanelHovered";
            textField.disabledBgSprite = "TextFieldPanelHovered";
            textField.textColor = new Color32(0, 0, 0, 255);
            textField.disabledTextColor = new Color32(80, 80, 80, 128);
            textField.color = new Color32(255, 255, 255, 255);

            return textField;
        }

        public static UIDropDown CreateDropDown(UIComponent parent)
        {
            UIDropDown dropDown = parent.AddUIComponent<UIDropDown>();

            dropDown.atlas = GetAtlas("Ingame");
            dropDown.size = new Vector2(90f, 30f);
            dropDown.listBackground = "GenericPanelLight";
            dropDown.itemHeight = 30;
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "TextFieldPanel";
            dropDown.focusedBgSprite = "TextFieldPanelHovered";
            dropDown.hoveredBgSprite = "TextFieldPanelHovered";
            dropDown.listPosition = UIDropDown.PopupListPosition.Above;
            dropDown.listWidth = 90;
            dropDown.listHeight = 500;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = new Color32(45, 52, 61, 255);
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.zOrder = 1;
            dropDown.textColor = new Color32(0, 0, 0, 255);
            dropDown.textScale = 0.8f;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.selectedIndex = 0;
            dropDown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            dropDown.itemPadding = new RectOffset(14, 0, 8, 0);
            dropDown.builtinKeyNavigation = true;

            UIButton button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.atlas = GetAtlas("Ingame");
            button.text = "";
            button.size = dropDown.size;
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "OptionsScrollbarThumb";
            button.spritePadding = new RectOffset(0, 3, 3, 0);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;
            button.textScale = 0.8f;

            dropDown.eventSizeChanged += new PropertyChangedEventHandler<Vector2>((c, t) =>
            {
                button.size = t; dropDown.listWidth = (int)t.x;
            });

            return dropDown;
        }

        public static void CreateDropDownScrollBar(UIDropDown dropDown)
        {
            // Scrollbar
            dropDown.listScrollbar = dropDown.AddUIComponent<UIScrollbar>();
            dropDown.listScrollbar.width = 20f;
            dropDown.listScrollbar.height = dropDown.listHeight;
            dropDown.listScrollbar.orientation = UIOrientation.Vertical;
            dropDown.listScrollbar.pivot = UIPivotPoint.TopRight;
            dropDown.listScrollbar.thumbPadding = new RectOffset(0, 0, 5, 5);
            dropDown.listScrollbar.minValue = 0;
            dropDown.listScrollbar.value = 0;
            dropDown.listScrollbar.incrementAmount = 50;
            dropDown.listScrollbar.AlignTo(dropDown, UIAlignAnchor.TopRight);
            dropDown.listScrollbar.autoHide = true;
            dropDown.listScrollbar.isVisible = false;
            // the game automatically creates 2 scrollbar clones: one for the drowdown itself and one for the dropdown popup list box
            // we only need the one inside the dropdown popup which will automatically be placed inside the popup
            // move the other one off screen to hide it(we can't set it to invisible or both would become invisible)
            Vector3 newPosition = FindIt.FindIt.instance.mainButton.relativePosition;
            newPosition.x += 50000;
            newPosition.y += 50000;
            dropDown.listScrollbar.relativePosition = newPosition;

            UISlicedSprite tracSprite = dropDown.listScrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = tracSprite.parent.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";
            dropDown.listScrollbar.trackObject = tracSprite;

            UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";
            dropDown.listScrollbar.thumbObject = thumbSprite;

            dropDown.listScrollbar.transform.localScale = dropDown.transform.localScale;
        }

        public static void DestroyDropDownScrollBar(UIDropDown dropDown)
        {
            UIScrollbar[] scrollbars = dropDown.GetComponentsInChildren<UIScrollbar>();
            foreach (UIScrollbar scrollbar in scrollbars)
            {
                UnityEngine.GameObject.DestroyImmediate(scrollbar.gameObject);
            }
        }

        public static UICheckBox CreateIconToggle(UIComponent parent, string atlas, string checkedSprite, string uncheckedSprite, float disabledSpriteOpacity = 1.0f, float tabSize = 35f)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            disabledSpriteOpacity = 0.3f;

            checkBox.width = tabSize;
            checkBox.height = tabSize;
            checkBox.clipChildren = true;

            UIPanel panel = checkBox.AddUIComponent<UIPanel>();
            panel.atlas = GetAtlas("Ingame");
            panel.backgroundSprite = "IconPolicyBaseRect";
            panel.size = checkBox.size;
            panel.relativePosition = Vector3.zero;

            UISprite sprite = panel.AddUIComponent<UISprite>();
            sprite.atlas = GetAtlas(atlas);

            sprite.spriteName = uncheckedSprite;
            sprite.size = checkBox.size;
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = sprite.atlas;
            ((UISprite)checkBox.checkedBoxObject).spriteName = checkedSprite;
            checkBox.checkedBoxObject.size = checkBox.size;
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.eventCheckChanged += (c, b) =>
            {
                if (checkBox.isChecked)
                {
                    panel.backgroundSprite = "IconPolicyBaseRect";
                    sprite.opacity = 1.0f;
                }
                else
                {
                    panel.backgroundSprite = "IconPolicyBaseRectDisabled";
                    sprite.opacity = disabledSpriteOpacity;
                }
                panel.Invalidate();
            };

            checkBox.eventMouseEnter += (c, p) =>
            {
                panel.backgroundSprite = "IconPolicyBaseRectHovered";
                sprite.spriteName = checkedSprite;
                sprite.opacity = 1.0f;
            };

            checkBox.eventMouseLeave += (c, p) =>
            {
                if (checkBox.isChecked)
                {
                    panel.backgroundSprite = "IconPolicyBaseRect";
                    sprite.opacity = 1.0f;
                }
                else
                {
                    panel.backgroundSprite = "IconPolicyBaseRectDisabled";
                    sprite.opacity = disabledSpriteOpacity;
                }
                sprite.spriteName = uncheckedSprite;
            };

            return checkBox;
        }

        private static UIColorField _colorFIeldTemplate;

        public static UIColorField CreateColorField(UIComponent parent)
        {
            // Creating a ColorField from scratch is tricky. Cloning an existing one instead.

            if (_colorFIeldTemplate == null)
            {
                // Get the LineTemplate (PublicTransportDetailPanel)
                UIComponent template = UITemplateManager.Get("LineTemplate");
                if (template == null) return null;

                // Extract the ColorField
                _colorFIeldTemplate = template.Find<UIColorField>("LineColor");
                if (_colorFIeldTemplate == null) return null;
            }

            UIColorField colorField = UnityEngine.Object.Instantiate<GameObject>(_colorFIeldTemplate.gameObject).GetComponent<UIColorField>();
            parent.AttachUIComponent(colorField.gameObject);

            colorField.size = new Vector2(40f, 26f);
            colorField.pickerPosition = UIColorField.ColorPickerPosition.LeftAbove;

            return colorField;
        }

        public static void ResizeIcon(UISprite icon, Vector2 maxSize)
        {
            icon.width = icon.spriteInfo.width;
            icon.height = icon.spriteInfo.height;

            if (icon.height == 0) return;

            float ratio = icon.width / icon.height;

            if (icon.width > maxSize.x)
            {
                icon.width = maxSize.x;
                icon.height = maxSize.x / ratio;
            }

            if (icon.height > maxSize.y)
            {
                icon.height = maxSize.y;
                icon.width = maxSize.y * ratio;
            }
        }

        private static Dictionary<string, UITextureAtlas> _atlases;

        public static UITextureAtlas GetAtlas(string name)
        {
            if (_atlases == null || !_atlases.ContainsKey(name))
            {
                _atlases = new Dictionary<string, UITextureAtlas>();

                UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
                for (int i = 0; i < atlases.Length; i++)
                {
                    if (!_atlases.ContainsKey(atlases[i].name))
                        _atlases.Add(atlases[i].name, atlases[i]);
                }
            }

            return _atlases[name];
        }
    }
}
