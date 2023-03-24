using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FindIt
{
    public static class ProceduralObjectsTool
    {
        private static Dictionary<string, uint> poInstanceCount = new Dictionary<string, uint>();

        public static void UpdatePOInfoList()
        {
            poInstanceCount.Clear();

            try
            {
                GameObject gameLogicObject = GameObject.Find("Logic_ProceduralObjects");
                if (gameLogicObject == null) return;

                Type ProceduralObjectsLogicType = Type.GetType("ProceduralObjects.ProceduralObjectsLogic, ProceduralObjects");
                if (ProceduralObjectsLogicType == null) return;

                Type ProceduralObjectType = Type.GetType("ProceduralObjects.Classes.ProceduralObject, ProceduralObjects");
                if (ProceduralObjectType == null) return;

                Component logic = gameLogicObject.GetComponent("ProceduralObjectsLogic");
                if (logic == null) return;

                object poList = ProceduralObjectsLogicType.GetField("proceduralObjects").GetValue(logic);
                if (poList == null) return;

                foreach (var i in poList as IList)
                {
                    string basePrefabName = ProceduralObjectType.GetField("basePrefabName").GetValue(i).ToString();
                    string infoType = ProceduralObjectType.GetField("baseInfoType").GetValue(i).ToString();
                    if (basePrefabName != null && infoType != null)
                    {
                        // Debugging.Message("PO instance found - " + basePrefabName.ToString());

                        if (!poInstanceCount.ContainsKey(basePrefabName))
                        {
                            poInstanceCount.Add(basePrefabName, 1);
                        }
                        else
                        {
                            poInstanceCount[basePrefabName] += 1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }

        public static uint GetPrefabPOInstanceCount(PrefabInfo info)
        {
            if (!(info is PropInfo) && !(info is BuildingInfo)) return 0;

            if (poInstanceCount.ContainsKey(info.name)) return poInstanceCount[info.name];

            return 0;
        }

        private static string storedPrefabName = "";
        private static List<Vector3> storedPositions = new List<Vector3>();
        private static int storedPositionsSize = 0;
        private static int storedPOCounter = 0;
        public static Vector3 GetPOInstancePosition(PrefabInfo prefab)
        {
            try
            {
                GameObject gameLogicObject = GameObject.Find("Logic_ProceduralObjects");
                if (gameLogicObject == null)
                {
                    return Vector3.zero;
                }

                Type ProceduralObjectsLogicType = Type.GetType("ProceduralObjects.ProceduralObjectsLogic, ProceduralObjects");
                Type ProceduralObjectType = Type.GetType("ProceduralObjects.Classes.ProceduralObject, ProceduralObjects");
                Component logic = gameLogicObject.GetComponent("ProceduralObjectsLogic");
                object poList = ProceduralObjectsLogicType.GetField("proceduralObjects").GetValue(logic);

                storedPositions.Clear();
                foreach (var i in poList as IList)
                {
                    string basePrefabName = ProceduralObjectType.GetField("basePrefabName").GetValue(i).ToString();
                    string infoType = ProceduralObjectType.GetField("baseInfoType").GetValue(i).ToString();
                    if (basePrefabName != null && infoType != null && basePrefabName == prefab.name)
                    {
                        object positionObject = ProceduralObjectType.GetField("m_position").GetValue(i);
                        Vector3 position = (Vector3)positionObject;
                        storedPositions.Add(position);
                    }
                }
                if (storedPositions.Count == 0) return Vector3.zero;

                if (storedPrefabName != prefab.name)
                {
                    storedPrefabName = prefab.name;
                    storedPOCounter = 0;
                    storedPositionsSize = storedPositions.Count;
                }
                else
                {
                    if (storedPositionsSize != storedPositions.Count)
                    {
                        storedPOCounter = 0;
                        storedPositionsSize = storedPositions.Count;
                    }
                }
                Vector3 result = storedPositions[storedPOCounter];
                if (storedPOCounter == storedPositions.Count - 1) storedPOCounter = 0;
                else
                {
                    storedPOCounter += 1;
                }
                return result;
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }

            return Vector3.zero;
        }
    }
}