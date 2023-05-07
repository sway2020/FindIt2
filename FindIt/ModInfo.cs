// modif"-ied from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using CitiesHarmony.API;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using static AssetImporterAssetTemplate;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public const string version = "2.9.5";
        public const bool isBeta = true;
        public const bool debug = false;
        public const double updateNoticeDate = 20230507;
        public const string updateNotice =

        "- Update filters for new DLC and CCP:\n" +
        "   Hotels & Retreats DLC\n" +
        "   Railroads of Japan CCP by Kaminogi\n" +
        "   Industrial Revolution CCP by Avanya\n" +
        "   Brooklyn & Queens CCP by Prosper\n\n" +

        "- Fix the District Style filter to include Africa in Miniature CCP District Style";

        public string Name
        {
            get { return "Find It! " + (isBeta ? "[BETA] " : "") + version; }
        }

        public string Description
        {
            get { return Translations.Translate("FIF_DESC"); }
        }

        /// <summary>
        /// Called by the game when mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
            Debugging.Message("Harmony patches applied");
            // Load settings here.
            XMLUtils.LoadSettings();
            Debugging.Message("XML Settings loaded");
        }

        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }

        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                if (FindIt.instance == null)
                {
                    AssetTagList.instance = new AssetTagList();
                }

                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                // Use system default browser instead of steam overlay
                UICheckBox useDefaultBrowser = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DB"), Settings.useDefaultBrowser, (b) =>
                {
                    Settings.useDefaultBrowser = b;
                    XMLUtils.SaveSettings();
                });
                useDefaultBrowser.tooltip = Translations.Translate("FIF_SET_DBTP");
                group.AddSpace(5);

                // Allow asset creators to hide dependency assets from search results. Assets with #creator_hidden in their description will be excluded
                UICheckBox hideDependencyAsset = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_CHA"), Settings.hideDependencyAsset, (b) =>
                {
                    Settings.hideDependencyAsset = b;
                    XMLUtils.SaveSettings();
                });
                hideDependencyAsset.tooltip = Translations.Translate("FIF_SET_CHATP");
                group.AddSpace(5);

                // Center the main toolbar
                UICheckBox centerToolbar = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_CMT"), Settings.centerToolbar, (b) =>
                {
                    Settings.centerToolbar = b;
                    XMLUtils.SaveSettings();

                    if (FindIt.instance != null)
                    {
                        FindIt.instance.UpdateMainToolbar();
                    }
                });
                centerToolbar.tooltip = Translations.Translate("FIF_SET_CMTTP");
                group.AddSpace(5);

                // Unlock all
                UICheckBox unlockAll = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_UL"), Settings.unlockAll, (b) =>
                {
                    Settings.unlockAll = b;
                    XMLUtils.SaveSettings();
                });
                unlockAll.tooltip = Translations.Translate("FIF_SET_ULTP");
                group.AddSpace(5);

                /*
                // Fix bad props next loaded save
                // Implemented by samsamTS. Only needed for pre-2018 savefiles 
                UICheckBox fixProps = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_BP"), false, (b) =>
                {
                    Settings.fixBadProps = b;
                    XMLUtils.SaveSettings();
                });
                fixProps.tooltip = Translations.Translate("FIF_SET_BPTP");
                group.AddSpace(10);
                */

                // Do not show extra Find It 2 UI on vanilla panels
                UICheckBox hideExtraUIonVP = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_UIVP"), Settings.hideExtraUIonVP, (b) =>
                {
                    Settings.hideExtraUIonVP = b;
                    XMLUtils.SaveSettings();
                });
                hideExtraUIonVP.tooltip = Translations.Translate("FIF_SET_UIVPTP");
                group.AddSpace(5);

                // Disable instant search
                UICheckBox disableInstantSearch = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DIS"), Settings.disableInstantSearch, (b) =>
                {
                    Settings.disableInstantSearch = b;
                    XMLUtils.SaveSettings();
                });
                disableInstantSearch.tooltip = Translations.Translate("FIF_SET_DISTP");
                group.AddSpace(5);

                // Disable update notice
                UICheckBox disableUpdateNotice = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DUN"), Settings.disableUpdateNotice, (b) =>
                {
                    Settings.disableUpdateNotice = b;
                    XMLUtils.SaveSettings();
                });
                group.AddSpace(5);

                // Disable secondary keyboard shortcuts
                UICheckBox disableSecondaryKeyboardShortcuts = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DSK"), Settings.disableSecondaryKeyboardShortcuts, (b) =>
                {
                    Settings.disableSecondaryKeyboardShortcuts = b;
                    XMLUtils.SaveSettings();
                });
                disableSecondaryKeyboardShortcuts.tooltip = Translations.Translate("FIF_SET_DSKTP");
                group.AddSpace(5);

                // Reset Find It panel when it is closed
                UICheckBox resetPanelWhenClosed = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_RES_PANEL"), Settings.resetPanelWhenClosed, (b) =>
                {
                    Settings.resetPanelWhenClosed = b;
                    XMLUtils.SaveSettings();
                });
                resetPanelWhenClosed.tooltip = Translations.Translate("FIF_SET_RES_PANELTP");
                group.AddSpace(5);

                // Recent DLs sorting
                string[] RencentDLsSortingList =
                {
                    Translations.Translate("FIF_SET_RECPC"),
                    Translations.Translate("FIF_SET_RECPM"),
                    Translations.Translate("FIF_SET_RECFC"),
                    Translations.Translate("FIF_SET_RECFM")
                };
                UIDropDown rencentDLsDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("FIF_SET_REC"), RencentDLsSortingList, Settings.recentDLSorting, (value) =>
                {
                    Settings.recentDLSorting = value;
                    XMLUtils.SaveSettings();
                });
                rencentDLsDropDown.width = 400;
                group.AddSpace(5);

                // languate settings
                UIDropDown languageDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) =>
                {
                    Translations.Index = value;
                    XMLUtils.SaveSettings();
                });
                languageDropDown.width = 300;
                group.AddSpace(5);

                // show path to FindItCustomTags.xml
                string customTagsFilePathStr = Path.Combine(DataLocation.localApplicationData, "FindItCustomTags.xml");
                UITextField customTagsFilePath = (UITextField)group.AddTextfield(Translations.Translate("FIF_SET_CTFL"), customTagsFilePathStr, _ => { }, _ => { });
                customTagsFilePath.width = panel.width - 30;
                group.AddButton(Translations.Translate("FIF_SET_CTFOP"), () => System.Diagnostics.Process.Start(DataLocation.localApplicationData));
                group.AddSpace(5);

                // show path to FindIt.xml
                string configFilePathStr = Path.Combine(DataLocation.executableDirectory, "FindIt.xml");
                UITextField configFilePath = (UITextField)group.AddTextfield(Translations.Translate("FIF_SET_CFFL"), configFilePathStr, _ => { }, _ => { });
                configFilePath.width = panel.width - 30;
                group.AddButton(Translations.Translate("FIF_SET_CTFOP"), () => System.Diagnostics.Process.Start(DataLocation.executableDirectory));

                // shortcut keys
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.searchKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.allKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.networkKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.ploppableKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.growableKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.ricoKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.grwbRicoKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.propKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.decalKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.treeKey);
                panel.gameObject.AddComponent<OptionsKeymapping>().Init(Settings.randomSelectionKey);
                group.AddSpace(10);

            }
            catch (Exception e)
            {
                Debugging.Message("OnSettingsUI failed");
                Debugging.LogException(e);
            }
        }
    }
}
