// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UITag : UILabel
    {
        private UIButton m_remove;

        public Asset asset;

        public override void Start()
        {
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");

            size = new Vector2(20, 25);
            padding = new RectOffset(3, 18, 3, 3);
            textColor = new Color32(109, 109, 109, 255);
            textScale = 0.8f;

            wordWrap = false;
            autoSize = true;

            m_remove = AddUIComponent<UIButton>();
            m_remove.size = new Vector2(16, 16);

            m_remove.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            m_remove.normalBgSprite = "buttonclose";
            m_remove.hoveredBgSprite = "buttonclosehover";
            m_remove.pressedBgSprite = "buttonclosepressed";
            m_remove.playAudioEvents = true;
            m_remove.canFocus = false;

            m_remove.isVisible = false;

            m_remove.eventClicked += (c, p) =>
            {
                AssetTagList.instance.RemoveCustomTag(asset, text);
                isVisible = false;
                //UITagsWindow.instance.input.Focus();
                UITagsWindow.instance.Refresh(asset);
            };
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_remove != null)
            {
                m_remove.relativePosition = new Vector3(width - m_remove.width - 3, (height - m_remove.height) / 2);
            }
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            textColor = new Color32(0, 0, 0, 255);
            backgroundSprite = "GenericPanelLight";
            m_remove.isVisible = true;
        }

        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            textColor = new Color32(109, 109, 109, 255);
            backgroundSprite = "";
            m_remove.isVisible = false;
        }
    }
}
