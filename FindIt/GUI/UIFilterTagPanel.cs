// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// custom tag panel

using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt.GUI
{
    public class UIFilterTagPanel : UIPanel
    {
        public static UIFilterTagPanel instance;

        public UICheckBox tagDropDownCheckBox;
        public UIDropDown tagDropDownMenu;
        private UIButton refreshButton;
        private UIButton renameButton;
        private UIButton mergeButton;
        private UIButton deleteButton;

        private UIButton batchButton;
        private UIButton batchAddButton;
        private UIButton batchRemoveButton;
        private UIButton batchSelectAllButton;
        private UIButton batchClearButton;
        public bool isBatchActionsEnabled = false;

        private List<KeyValuePair<string, int>> customTagList;
        public string[] customTagListStrArray;

        public HashSet<Asset> batchAssetSet = new HashSet<Asset>();

        public override void Start()
        {
            instance = this;

            // tag dropdown filter checkbox
            tagDropDownCheckBox = SamsamTS.UIUtils.CreateCheckBox(this);
            tagDropDownCheckBox.isChecked = false;
            tagDropDownCheckBox.width = 20;
            tagDropDownCheckBox.tooltip = Translations.Translate("FIF_TAG_DDTP");
            tagDropDownCheckBox.relativePosition = new Vector3(10, 10);
            tagDropDownCheckBox.eventCheckChanged += (c, i) =>
            {
                if (customTagListStrArray.Length == 0)
                {
                    tagDropDownCheckBox.isChecked = false;
                }
                ((UISearchBox)parent).Search();
            };

            // tag dropdown
            tagDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            tagDropDownMenu.size = new Vector2(270, 25);
            tagDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            tagDropDownMenu.listHeight = 300;
            tagDropDownMenu.itemHeight = 30;
            tagDropDownMenu.relativePosition = new Vector3(tagDropDownCheckBox.relativePosition.x + tagDropDownCheckBox.width + 5, 5);
            UpdateCustomTagList();

            tagDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (tagDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // refresh button 
            refreshButton = SamsamTS.UIUtils.CreateButton(this);
            refreshButton.size = new Vector2(80, 25);
            refreshButton.text = Translations.Translate("FIF_TAG_REF");
            refreshButton.textScale = 0.8f;
            refreshButton.textPadding = new RectOffset(0, 0, 5, 0);
            refreshButton.tooltip = Translations.Translate("FIF_TAG_REFTP");
            refreshButton.relativePosition = new Vector3(tagDropDownMenu.relativePosition.x + tagDropDownMenu.width + 2, 5);
            refreshButton.eventClick += (c, p) =>
            {
                UpdateCustomTagList();
                ((UISearchBox)parent).Search();
            };

            // rename button 
            renameButton = SamsamTS.UIUtils.CreateButton(this);
            renameButton.size = new Vector2(80, 25);
            renameButton.text = Translations.Translate("FIF_TAG_REN");
            renameButton.textScale = 0.8f;
            renameButton.textPadding = new RectOffset(0, 0, 5, 0);
            renameButton.tooltip = Translations.Translate("FIF_TAG_RENTP");
            renameButton.relativePosition = new Vector3(refreshButton.relativePosition.x + refreshButton.width + 2, 5);
            renameButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsRenamePopUp.ShowAt(renameButton, GetDropDownListKey());
                }
                else
                {
                    Debugging.Message("Custom tag rename button pressed, but no custom tag exists");
                }
            };

            // merge button 
            mergeButton = SamsamTS.UIUtils.CreateButton(this);
            mergeButton.size = new Vector2(80, 25);
            mergeButton.text = Translations.Translate("FIF_TAG_COM");
            mergeButton.textScale = 0.8f;
            mergeButton.textPadding = new RectOffset(0, 0, 5, 0);
            mergeButton.tooltip = Translations.Translate("FIF_TAG_COMTP");
            mergeButton.relativePosition = new Vector3(renameButton.relativePosition.x + renameButton.width + 2, 5);
            mergeButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsMergePopUp.ShowAt(mergeButton, GetDropDownListKey());
                }
                else
                {
                    Debugging.Message("Custom tag combine button pressed, but no custom tag exists");
                }
            };

            // delete button 
            deleteButton = SamsamTS.UIUtils.CreateButton(this);
            deleteButton.size = new Vector2(80, 25);
            deleteButton.text = Translations.Translate("FIF_TAG_DEL");
            deleteButton.textScale = 0.8f;
            deleteButton.textPadding = new RectOffset(0, 0, 5, 0);
            deleteButton.tooltip = Translations.Translate("FIF_TAG_DELTP");
            deleteButton.relativePosition = new Vector3(mergeButton.relativePosition.x + mergeButton.width + 2, 5);
            deleteButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsDeletePopUp.ShowAt(deleteButton, GetDropDownListKey());
                }
                else
                {
                    Debugging.Message("Custom tag delete button pressed, but no custom tag exists");
                }
            };

            // batch add button 
            batchAddButton = SamsamTS.UIUtils.CreateButton(this);
            batchAddButton.size = new Vector2(80, 25);
            batchAddButton.text = Translations.Translate("FIF_TAG_ADD");
            batchAddButton.textScale = 0.8f;
            batchAddButton.textPadding = new RectOffset(0, 0, 5, 0);
            batchAddButton.isVisible = false;
            batchAddButton.tooltip = Translations.Translate("FIF_TAG_ADDTP");
            batchAddButton.relativePosition = new Vector3(refreshButton.relativePosition.x + refreshButton.width + 2, 5);
            batchAddButton.eventClick += (c, p) =>
            {
                UITagsBatchAddPopUp.ShowAt(batchAddButton);
            };

            // batch remove button 
            batchRemoveButton = SamsamTS.UIUtils.CreateButton(this);
            batchRemoveButton.size = new Vector2(80, 25);
            batchRemoveButton.text = Translations.Translate("FIF_TAG_REM");
            batchRemoveButton.textScale = 0.8f;
            batchRemoveButton.textPadding = new RectOffset(0, 0, 5, 0);
            batchRemoveButton.isVisible = false;
            batchRemoveButton.tooltip = Translations.Translate("FIF_TAG_REMTP");
            batchRemoveButton.relativePosition = new Vector3(batchAddButton.relativePosition.x + batchAddButton.width + 2, 5);
            batchRemoveButton.eventClick += (c, p) =>
            {
                UITagsBatchRemovePopUp.ShowAt(batchRemoveButton);
            };

            // batch select all button 
            batchSelectAllButton = SamsamTS.UIUtils.CreateButton(this);
            batchSelectAllButton.size = new Vector2(80, 25);
            batchSelectAllButton.text = Translations.Translate("FIF_TAG_SA");
            batchSelectAllButton.textScale = 0.8f;
            batchSelectAllButton.textPadding = new RectOffset(0, 0, 5, 0);
            batchSelectAllButton.isVisible = false;
            batchSelectAllButton.tooltip = Translations.Translate("FIF_TAG_SATP");
            batchSelectAllButton.relativePosition = new Vector3(batchRemoveButton.relativePosition.x + batchRemoveButton.width + 2, 5);
            batchSelectAllButton.eventClick += (c, p) =>
            {
                if (UISearchBox.instance.matches != null)
                {
                    foreach (Asset asset in UISearchBox.instance.matches)
                    {
                        batchAssetSet.Add(asset);
                    }
                }
                UISearchBox.instance.scrollPanel.Refresh();
            };

            // batch clear selection button 
            batchClearButton = SamsamTS.UIUtils.CreateButton(this);
            batchClearButton.size = new Vector2(80, 25);
            batchClearButton.text = Translations.Translate("FIF_TAG_CLE");
            batchClearButton.textScale = 0.8f;
            batchClearButton.textPadding = new RectOffset(0, 0, 5, 0);
            batchClearButton.isVisible = false;
            batchClearButton.tooltip = Translations.Translate("FIF_TAG_CLETP");
            batchClearButton.relativePosition = new Vector3(batchSelectAllButton.relativePosition.x + batchSelectAllButton.width + 2, 5);
            batchClearButton.eventClick += (c, p) =>
            {
                batchAssetSet.Clear();
                UISearchBox.instance.scrollPanel.Refresh();
            };

            // batch button 
            batchButton = SamsamTS.UIUtils.CreateButton(this);
            batchButton.size = new Vector2(80, 25);
            batchButton.text = Translations.Translate("FIF_TAG_BAT");
            batchButton.tooltip = Translations.Translate("FIF_TAG_BATTP");
            batchButton.textScale = 0.8f;
            batchButton.textPadding = new RectOffset(0, 0, 5, 0);
            batchButton.relativePosition = new Vector3(deleteButton.relativePosition.x + deleteButton.width + 2, 5);
            batchButton.eventClick += (c, p) =>
            {
                isBatchActionsEnabled = !isBatchActionsEnabled;
                renameButton.isVisible = !isBatchActionsEnabled;
                mergeButton.isVisible = !isBatchActionsEnabled;
                deleteButton.isVisible = !isBatchActionsEnabled;
                batchAddButton.isVisible = isBatchActionsEnabled;
                batchRemoveButton.isVisible = isBatchActionsEnabled;
                batchClearButton.isVisible = isBatchActionsEnabled;
                batchSelectAllButton.isVisible = isBatchActionsEnabled;
                if (isBatchActionsEnabled)
                {
                    batchAssetSet.Clear();
                    batchButton.text = Translations.Translate("FIF_TAG_BAC");
                    batchButton.relativePosition = new Vector3(batchClearButton.relativePosition.x + batchClearButton.width + 2, 5);
                    width = UISearchBox.instance.sizeFilterX.position.x + batchClearButton.width + 5;
                }
                else
                {
                    batchButton.text = Translations.Translate("FIF_TAG_BAT");
                    batchButton.relativePosition = new Vector3(deleteButton.relativePosition.x + deleteButton.width + 2, 5);
                    width = UISearchBox.instance.sizeFilterX.position.x;
                }
                UISearchBox.instance.scrollPanel.Refresh();
            };

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

        /// <summary>
        /// Refresh customg tag list and the data for custom tag panel display
        /// </summary>
        public void UpdateCustomTagList()
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
            if (customTagListStrArray.Length == 0)
            {
                tagDropDownCheckBox.isChecked = false;
            }

            tagDropDownMenu.items = customTagListStrArray;

            SamsamTS.UIUtils.DestroyDropDownScrollBar(tagDropDownMenu);
            SamsamTS.UIUtils.CreateDropDownScrollBar(tagDropDownMenu);
        }

        /// <summary>
        /// Return the selected custom tag string in the custom tag panel dropdown
        /// </summary>
        public string GetDropDownListKey()
        {
            return customTagList[tagDropDownMenu.selectedIndex].Key;
        }

        /// <summary>
        /// Given string key, find the corresponding index in the custom tag dropdown,
        /// return -1 if not found
        /// </summary>
        public int GetDropDownListIndex(string key)
        {
            UpdateCustomTagList();

            for (int i = 0; i < customTagList.Count; ++i)
            {
                if (customTagList[i].Key == key)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
