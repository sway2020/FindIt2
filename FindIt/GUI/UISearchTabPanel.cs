// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// search tabs panel

using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt.GUI
{
    public class UISearchTabPanel : UIPanel
    {
        public static UISearchTabPanel instance;
        private UISprite newTabIcon;
        private UISearchTab selectedTab;
        private readonly int maxNumOfTabs = 8;
        private List<UISearchTab> searchTabsPool = new List<UISearchTab>();
        public List<UISearchTab> searchTabsList = new List<UISearchTab>();

        public override void Start()
        {
            instance = this;
            searchTabsList.Clear();
            SetNewTabIcon();
            for (int i = 0; i < maxNumOfTabs; ++i)
            {
                UISearchTab newSearchTab = AddUIComponent<UISearchTab>();
                searchTabsPool.Add(newSearchTab);
            }
            instance.isVisible = Settings.showSearchTabs;
        }

        private void AddNewSearchTab()
        {
            // find an inactive tab in the pool
            foreach (UISearchTab tab in searchTabsPool)
            {
                if (!tab.isActiveTab)
                {
                    tab.isActiveTab = true;
                    tab.isVisible = true;
                    tab.ResetTab();

                    searchTabsList.Add(tab);
                    RefreshUIPositions();
                    tab.Selected();

                    break;
                }
            }
        }

        public void RemoveSearchTab(UISearchTab tab)
        {
            if (selectedTab == tab)
            {
                for (int i = 0; i < searchTabsList.Count; ++i)
                {
                    if (searchTabsList[i] == tab)
                    {
                        // if selected tab is the last tab, select the left neighbor tab
                        // else, select the right neighbor tab
                        if (i == searchTabsList.Count - 1) searchTabsList[i - 1].Selected();
                        else searchTabsList[i + 1].Selected();
                        break;
                    }
                }
            }

            searchTabsList.Remove(tab);
            RefreshUIPositions();
        }

        public void RefreshUIPositions()
        {
            for (int i = 0; i < searchTabsList.Count; ++i)
            {
                searchTabsList[i].relativePosition = new Vector2(5 + i * 145, 5);
            }
            newTabIcon.relativePosition = new Vector3(5 + searchTabsList.Count * 145 + 2, 7);
            instance.width = newTabIcon.relativePosition.x + 25f;
        }

        private void SetNewTabIcon()
        {
            newTabIcon = instance.AddUIComponent<UISprite>();
            newTabIcon.size = new Vector2(20, 20);
            newTabIcon.opacity = 0.5f;
            newTabIcon.eventMouseEnter += (c, p) =>
            {
                newTabIcon.opacity = 1.0f;
            };
            newTabIcon.eventMouseLeave += (c, p) =>
            {
                newTabIcon.opacity = 0.5f;
            };
            newTabIcon.eventClicked += (c, p) =>
            {
                AddNewSearchTab();
            };
            newTabIcon.atlas = SamsamTS.UIUtils.GetAtlas("FindItAtlas");
            newTabIcon.spriteName = "NewTab";
            newTabIcon.relativePosition = new Vector3(3, 7);
        }

        public void SetSelectedTab(UISearchTab tab)
        {
            if (selectedTab != null)
            {
                selectedTab.Unselected();
            }
            selectedTab = tab;
        }

        public UISearchTab GetSelectedTab()
        {
            return selectedTab;
        }

        public bool IsOnlyTab(UISearchTab tab)
        {
            if (searchTabsList.Count == 1 && searchTabsList[0] == tab) return true;
            return false;
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
    }
}
