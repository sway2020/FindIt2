// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIUpdateNoticePopUp : UIPanel
    {
        public static UIUpdateNoticePopUp instance;
        private const float spacing = 5f;
        private UIButton closeButton;

        public override void Start()
        {
            name = "FindIt_UpdateNoticePopUp";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(500, 240);

            UILabel title = AddUIComponent<UILabel>();
            title.text = "Find It! " + (ModInfo.isBeta ? "[BETA] " : "") + ModInfo.version + " " + Translations.Translate("FIF_UPN_UP");
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
            message.text = "\n" + ModInfo.updateNotice;
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing * 4, spacing + title.height + spacing);

            closeButton = SamsamTS.UIUtils.CreateButton(this);
            closeButton.size = new Vector2(100, 40);
            closeButton.text = Translations.Translate("FIF_POP_CON");
            closeButton.relativePosition = new Vector3(spacing * 2, message.relativePosition.y + message.height + spacing * 2);
            closeButton.eventClick += (c, p) =>
            {
                Close();
            };

            height = closeButton.relativePosition.y + closeButton.height + 10;
            width = message.width + 40;
            close.relativePosition = new Vector3(width - close.width, 0);
            closeButton.Focus();
        }

        private static void Close()
        {
            if (instance != null)
            {
                // UIView.PopModal();
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

        public static void ShowAt()
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UIUpdateNoticePopUp)) as UIUpdateNoticePopUp;
                instance.Show(true);
                // UIView.PushModal(instance);
            }
            else
            {
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
    }
}
