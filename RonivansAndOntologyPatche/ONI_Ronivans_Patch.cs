extern alias plib;

using CykUtils;
using Dupes_Industrial_Overhaul.Chemical_Processing.Buildings;
using HarmonyLib;
using Metallurgy.Buildings;
using RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesEngineering;
using UnityEngine;

namespace sinevil.ONI_Ronivans_Patch
{

    public class RonivansPatches
    {
        static bool IsRonivansModLoaded = KUtils.IsModLoaded("RonivansLegacy_ChemicalProcessing");

        [HarmonyPatch(typeof(Chemical_AdvancedMetalRefineryConfig), nameof(Chemical_AdvancedMetalRefineryConfig.ConfigureBuildingTemplate))]
        public class 先进金属精炼机Patch
        {
            public static void Postfix(GameObject go)
            {
                // 检测目标Mod是否加载，未加载则直接返回
                if (!IsRonivansModLoaded)
                {
                    LogUtil.Log("先进金属精炼机：目标Mod未加载，跳过修改");
                    return;
                }

                // Mod已加载，执行原修改逻辑
                bool 先进金属精炼机 = SingletonOptions<Config>.Instance.先进金属精炼机;
                if (先进金属精炼机)
                {
                    go.AddOrGet<ComplexFabricator>().duplicantOperated = false;
                    go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
                    LogUtil.Log("先进金属精炼机：Mod已加载且开关开启，已修改为自动运行");
                }
                else
                {
                    LogUtil.Log("先进金属精炼机：Mod已加载但开关关闭，未修改");
                }
            }
        }

        [HarmonyPatch(typeof(Chemical_SelectiveArcFurnaceConfig), nameof(Chemical_SelectiveArcFurnaceConfig.ConfigureBuildingTemplate))]
        public class 选择性电弧炉Patch
        {
            public static void Postfix(GameObject go)
            {
                if (!IsRonivansModLoaded)
                {
                    LogUtil.Log("选择性电弧炉：目标Mod未加载，跳过修改");
                    return;
                }

                bool 选择性电弧炉 = SingletonOptions<Config>.Instance.选择性电弧炉;
                if (选择性电弧炉)
                {
                    go.AddOrGet<ComplexFabricator>().duplicantOperated = false;
                    go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
                    LogUtil.Log("选择性电弧炉：Mod已加载且开关开启，已修改为自动运行");
                }
                else
                {
                    LogUtil.Log("选择性电弧炉：Mod已加载但开关关闭，未修改");
                }
            }
        }

        [HarmonyPatch(typeof(Metallurgy_PlasmaFurnaceConfig), nameof(Metallurgy_PlasmaFurnaceConfig.ConfigureBuildingTemplate))]
        public class 等离子电弧炉Patch
        {
            public static void Postfix(GameObject go)
            {
                if (!IsRonivansModLoaded)
                {
                    LogUtil.Log("等离子电弧炉：目标Mod未加载，跳过修改");
                    return;
                }

                bool 等离子电弧炉 = SingletonOptions<Config>.Instance.等离子电弧炉;
                if (等离子电弧炉)
                {
                    go.AddOrGet<ComplexFabricator>().duplicantOperated = false;
                    go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
                    LogUtil.Log("等离子电弧炉：Mod已加载且开关开启，已修改为自动运行");
                }
                else
                {
                    LogUtil.Log("等离子电弧炉：Mod已加载但开关关闭，未修改");
                }
            }
        }

        [HarmonyPatch(typeof(Chemical_MixingUnitConfig), nameof(Chemical_MixingUnitConfig.ConfigureBuildingTemplate))]
        public class 化学混合装置Patch
        {
            public static void Postfix(GameObject go)
            {
                if (!IsRonivansModLoaded)
                {
                    LogUtil.Log("化学混合装置：目标Mod未加载，跳过修改");
                    return;
                }

                bool 化学混合装置 = SingletonOptions<Config>.Instance.化学混合装置;
                if (化学混合装置)
                {
                    go.AddOrGet<ComplexFabricator>().duplicantOperated = false;
                    go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
                    LogUtil.Log("化学混合装置：Mod已加载且开关开启，已修改为自动运行");
                }
                else
                {
                    LogUtil.Log("化学混合装置：Mod已加载但开关关闭，未修改");
                }
            }
        }

        [HarmonyPatch(typeof(Chemical_AdvancedKilnConfig), nameof(Chemical_AdvancedKilnConfig.ConfigureBuildingTemplate))]
        public class 先进窑炉Patch
        {
            public static void Postfix(GameObject go)
            {
                if (!IsRonivansModLoaded)
                {
                    LogUtil.Log("先进窑炉：目标Mod未加载，跳过修改");
                    return;
                }

                bool 先进窑炉 = SingletonOptions<Config>.Instance.先进窑炉;
                if (先进窑炉)
                {
                    go.AddOrGet<ComplexFabricator>().duplicantOperated = false;
                    go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
                    LogUtil.Log("先进窑炉：Mod已加载且开关开启，已修改为自动运行");
                }
                else
                {
                    LogUtil.Log("先进窑炉：Mod已加载但开关关闭，未修改");
                }
            }
        }

        [HarmonyPatch(typeof(CementMixerConfig), nameof(CementMixerConfig.ConfigureBuildingTemplate))]
        public class 水泥搅拌机Patch
        {
            public static void Postfix(GameObject go)
            {
                if (!IsRonivansModLoaded)
                {
                    LogUtil.Log("水泥搅拌机：目标Mod未加载，跳过修改");
                    return;
                }

                bool 水泥搅拌机 = SingletonOptions<Config>.Instance.水泥搅拌机;
                if (水泥搅拌机)
                {
                    go.AddOrGet<ComplexFabricator>().duplicantOperated = false;
                    go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
                    LogUtil.Log("水泥搅拌机：Mod已加载且开关开启，已修改为自动运行");
                }
                else
                {
                    LogUtil.Log("水泥搅拌机：Mod已加载但开关关闭，未修改");
                }
            }
        }
    }
}
