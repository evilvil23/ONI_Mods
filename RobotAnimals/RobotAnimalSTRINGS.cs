namespace sinevil.Robot_Animal_Remastered
{
    public class RobotAnimalSTRINGS
    {
        public static void DoReplacement()
        {
            LocString.CreateLocStringKeys(typeof(RobotAnimalSTRINGS), "");
        }

        public class BUILDINGS
        {
            public class RobotStaterpillar
            {
                public static LocString NAME = "机械蛞蝓";
                public static LocString EFFECT = "机械蛞蝓以各种金属矿石或精炼金属为食并在进食时排放氢气。";
                public static LocString DESC = "抛去华而不实的外表，返璞归真。";
            }
            public class RobotStego
            {
                public static LocString NAME = "机械尖块兽";
                public static LocString EFFECT = "尖块兽是一种常见于花园生态的 2*2 小动物，以漫花果为食并产出泥炭和硬肉。";
                public static LocString DESC = "像尖块兽一样将美味的漫花果转化为泥炭并掉落硬肉。";
            }
            public class RobotHatch
            {
                public static LocString NAME = "机械哈奇";
                public static LocString EFFECT = "好吃哈奇是一种杂食性的陆生小动物，自然生成于砂岩生态，可以人工养殖。好吃哈奇主要用于生产煤炭。";
                public static LocString DESC = "哈奇进食后产出煤炭，是殖民地煤炭的主要来源之一。";
            }
        }

        public class CONFIGURATIONITEM
        {
            public static LocString CATEGORY = "建筑配置";

            public class RobotStaterpillar_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械蛞蝓转化系数";
            }
            public class RobotStego_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械尖块兽转化系数";
            }
            public class RobotHatch_UI
            {
                public static LocString ROCK_CONVERSION_COEFFICIENT = "机械哈奇 岩石配方转化系数";
                public static LocString FOOD_CONVERSION_COEFFICIENT = "机械哈奇 食物配方转化系数";
            }


        }
            
        
        
    }
}
