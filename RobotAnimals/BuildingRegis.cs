using HarmonyLib;
using System.Collections.Generic;


namespace sinevil.Robot_Animal_Remastered.utils
{

    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    public class AddToBuild
    {
        /// <summary>
        /// 添加建筑到建筑菜单
        /// </summary>
        /// <param name="id">建筑ID</param>
        /// <param name="name">建筑名称</param>
        /// <param name="desc">建筑描述</param>
        /// <param name="effect">建筑效果</param>
        private static void Add(string id, string name, string desc, string effect)
        {
            string str = "STRINGS.BUILDINGS.PREFABS." + id.ToUpper() + ".";
            LocString locString = new LocString(name, str + "NAME");
            Strings.Add(new string[]
            {
                locString.key.String,
                locString.text
            });
            LocString locString2 = new LocString(desc, str + "DESC");
            Strings.Add(new string[]
            {
                locString2.key.String,
                locString2.text
            });
            LocString locString3 = new LocString(effect, str + "EFFECT");
            Strings.Add(new string[]
            {
                locString3.key.String,
                locString3.text
            });
            ModUtil.AddBuildingToPlanScreen("Utilities", id);
        }

        public static void Prefix()
        {
            AddToBuild.Add("RobotStaterpillar", 
                RobotAnimalSTRINGS.BUILDINGS.RobotStaterpillar.NAME, 
                RobotAnimalSTRINGS.BUILDINGS.RobotStaterpillar.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotStaterpillar.EFFECT
                );
            AddToBuild.Add("RobotStego",
                RobotAnimalSTRINGS.BUILDINGS.RobotStego.NAME,
                RobotAnimalSTRINGS.BUILDINGS.RobotStego.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotStego.EFFECT
                );
            AddToBuild.Add("RobotHatch",
                RobotAnimalSTRINGS.BUILDINGS.RobotHatch.NAME,
                RobotAnimalSTRINGS.BUILDINGS.RobotHatch.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotHatch.EFFECT
                );
            AddToBuild.Add("RobotPacu",
                RobotAnimalSTRINGS.BUILDINGS.RobotPacu.NAME,
                RobotAnimalSTRINGS.BUILDINGS.RobotPacu.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotPacu.EFFECT
                );
            AddToBuild.Add("RobotPokeshell",
                RobotAnimalSTRINGS.BUILDINGS.RobotPokeshell.NAME,
                RobotAnimalSTRINGS.BUILDINGS.RobotPokeshell.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotPokeshell.EFFECT
                );
            AddToBuild.Add("RobotPuft",
                RobotAnimalSTRINGS.BUILDINGS.RobotPuft.NAME,
                RobotAnimalSTRINGS.BUILDINGS.RobotPuft.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotPuft.EFFECT
            );
            AddToBuild.Add("RobotShoveVole",
                RobotAnimalSTRINGS.BUILDINGS.RobotShoveVole.NAME,
                RobotAnimalSTRINGS.BUILDINGS.RobotShoveVole.DESC,
                RobotAnimalSTRINGS.BUILDINGS.RobotShoveVole.EFFECT
            );
        }
    }

    [HarmonyPatch(typeof(Db), "Initialize")]
    internal class AddToTech
    {
        public static void Postfix()
        {
            // 添加到科技树
            List<string> unlockedItemIDs = Db.Get().Techs.Get("Smelting").unlockedItemIDs;
            unlockedItemIDs.Add("RobotStaterpillar");
            unlockedItemIDs.Add("RobotStego");
            unlockedItemIDs.Add("RobotHatch");
            unlockedItemIDs.Add("RobotPacu");
            unlockedItemIDs.Add("RobotPokeshell");
            unlockedItemIDs.Add("RobotPuft");
            unlockedItemIDs.Add("RobotShoveVole");
        }
    }

}