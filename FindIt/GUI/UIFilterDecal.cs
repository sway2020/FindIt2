// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// Filter tabs panel for decal

using ColossalFramework.UI;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIFilterDecal : UIPanel
    {
        public static UIFilterDecal instance;

        private UICheckBox randomIcon;

        public override void Start()
        {
            instance = this;

            size = new Vector2(605, 45);

            randomIcon = SamsamTS.UIUtils.CreateIconToggle(this, "FindItAtlas", "Dice", "Dice");
            randomIcon.relativePosition = new Vector3(5, 5);
            randomIcon.tooltip = Translations.Translate("FIF_GR_RAN");
            randomIcon.isChecked = true;
            randomIcon.readOnly = true;
            randomIcon.checkedBoxObject.isInteractive = false;
            randomIcon.eventClicked += (c, p) =>
            {
                UISearchBox.instance.PickRandom();
            };

            width = parent.width;
        }
    }
}
