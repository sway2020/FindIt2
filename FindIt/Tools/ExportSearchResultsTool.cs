using ColossalFramework.IO;
using ColossalFramework.UI;
using FindIt.GUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace FindIt
{
    public static class ExportSearchResultsTool
    {
        public static void ExportHTML()
        {
            // get steam id of all workshop assets
            Dictionary<ulong, List<Asset>> steamIds = new Dictionary<ulong, List<Asset>>();

            foreach (Asset asset in UISearchBox.instance.matches)
            {
                if (!asset.prefab.m_isCustomContent) continue;
                if (asset.steamID == 0) continue;

                if (!steamIds.ContainsKey(asset.steamID))
                {
                    List<Asset> assetList = new List<Asset> { asset };
                    steamIds.Add(asset.steamID, assetList);
                }
                else
                {
                    steamIds[asset.steamID].Add(asset);
                }
            }


            string currentTime = GetFormattedDateTime();
            string path = Path.Combine(DataLocation.localApplicationData, $"FindItExportSearchResults_{currentTime}.html");
            if (File.Exists(path)) File.Delete(path);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            {
                file.WriteLine($"Find It 2 search results export<br>");
                file.WriteLine($"Export Date: {currentTime}<br><br>");

                file.WriteLine($"If you want to copy and paste into Loading Screen Mod's skip.txt file:<br><br>");

                file.WriteLine($"Loading Screen Mod considers all building types as one type (Buildings) in skip.txt<br>");
                file.WriteLine($"Loading Screen Mod considers props and decals as one type (Props) in skip.txt<br><br>");

                file.WriteLine($"<a href=\"https://steamcommunity.com/workshop/filedetails/discussion/667342976/1741105805762370419/\">Check out Loading Screen Mod's page for more information</a><br><br>");

                // output custom assets with steam ids
                file.WriteLine($"<br>----------------------------------------------<br>\n");
                file.WriteLine($"Custom assets with Steam workshop ID:<br>\n");

                foreach (var pair in steamIds)
                {
                    file.WriteLine($"<br><a href=\"https://steamcommunity.com/sharedfiles/filedetails/?id={pair.Key}\">{pair.Key}</a><br>\n");
                    file.WriteLine($"This workshop ID contains {pair.Value.Count} asset(s) in the search results<br><br>\n");

                    foreach (var asset in pair.Value)
                    {
                        file.WriteLine($"{asset.name}<br>\n");
                    }
                }

                // output custom assets without steam ids
                file.WriteLine($"<br>----------------------------------------------<br>\n");
                file.WriteLine($"<br>Local custom assets without Steam workshop ID:<br><br>\n");

                foreach (Asset asset in UISearchBox.instance.matches)
                {
                    if (!asset.prefab.m_isCustomContent) continue;
                    if (asset.steamID != 0) continue;
                    file.WriteLine($"{asset.name}<br>\n");
                }

                // output vanilla assets
                file.WriteLine($"<br>----------------------------------------------<br>\n");
                file.WriteLine($"<br>Vanilla or content creator pack assets:<br><br>\n");

                file.WriteLine($"<br>Growables:<br><br>\n");
                OutputVanillaAssetsInType(Asset.AssetType.Growable, file);

                file.WriteLine($"<br>-----------------------------------<br>\n");
                file.WriteLine($"<br>Ploppables:<br><br>\n");
                OutputVanillaAssetsInType(Asset.AssetType.Ploppable, file);

                file.WriteLine($"<br>-----------------------------------<br>\n");
                file.WriteLine($"<br>Props:<br><br>\n");
                OutputVanillaAssetsInType(Asset.AssetType.Prop, file);

                file.WriteLine($"<br>-----------------------------------<br>\n");
                file.WriteLine($"<br>Decals:<br><br>\n");
                OutputVanillaAssetsInType(Asset.AssetType.Decal, file);

                file.WriteLine($"<br>-----------------------------------<br>\n");
                file.WriteLine($"<br>Trees:<br><br>\n");
                OutputVanillaAssetsInType(Asset.AssetType.Tree, file);

                file.WriteLine($"<br>-----------------------------------<br>\n");
                file.WriteLine($"<br>Networks:<br><br>\n");
                OutputVanillaAssetsInType(Asset.AssetType.Network, file);
            }

            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Find It 2", $"FindItExportSearchResults_{currentTime}.html is exported.\n\n{path}", false);
        }

        private static void OutputVanillaAssetsInType(Asset.AssetType asseType, System.IO.StreamWriter file)
        {
            foreach (Asset asset in UISearchBox.instance.matches)
            {
                if (asset.prefab.m_isCustomContent) continue;
                if (asset.assetType != asseType) continue;
                file.WriteLine($"{asset.name}<br>\n");
            }
        }

        private static string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
        }
    }
}
