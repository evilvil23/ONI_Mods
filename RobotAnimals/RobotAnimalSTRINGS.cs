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
                public static LocString EFFECT = "像尖块兽一样将美味的漫花果转化为泥炭并掉落硬肉。";
                public static LocString DESC = "尖块兽是一种常见于花园生态的 2*2 小动物，以漫花果为食并产出泥炭和硬肉。";
            }
            public class RobotHatch
            {
                public static LocString NAME = "机械哈奇";
                public static LocString EFFECT = "哈奇进食后产出煤炭，是殖民地煤炭的主要来源之一。";
                public static LocString DESC = "好吃哈奇是一种杂食性的陆生小动物，自然生成于砂岩生态，可以人工养殖。好吃哈奇主要用于生产煤炭。";
            }
            public class RobotPacu
            {
                public static LocString NAME = "机械帕库鱼";
                public static LocString EFFECT = "帕库鱼进食后产出污染土，凭借快速的繁殖速度，是殖民地蛋白质的主要来源之一。";
                public static LocString DESC = "帕库鱼是一种优质的蛋白质来源，自然生成于湿地生态的污染水水域中，既可吃死亡后掉落的帕库鱼片，也可吃蛋。";
            }

            public class RobotPokeshell
            {
                public static LocString NAME = "机械抛壳蟹";
                public static LocString EFFECT = "抛壳蟹死亡时，会掉落 10 千克的蟹壳，它可以由碎石机粉碎为石灰，进而用于炼钢。";
                public static LocString DESC = "抛壳蟹是一种生活在海洋生态之中的两栖小动物，在气体或液体环境中都可以生存。他们会以 70 千克/周期的速度食用腐烂物或者污染土，";
            }
            public class RobotPuft
            {
                public static LocString NAME = "机械喷浮飞鱼";
                public static LocString EFFECT = "喷浮飞鱼和它的不同亚种以不同的气体作为食物。在吸入对应的气体后，它们会排泄出会对应的固体物质。";
                public static LocString DESC = "喷浮飞鱼是一种可以在空中（包括真空）飞行的小动物，常见于湿地生态。";
            }
            public class RobotShoveVole
            {
                public static LocString NAME = "机械锹环田鼠";
                public static LocString EFFECT = "珍馐田鼠的身上会生长羽刺，可以使用修剪站来收获这些羽刺来获得滋补根。";
                public static LocString DESC = "锹环田鼠是一种能够钻入固体砖块中并移动的小动物，常见于太空生态。它们可以进食浮土并食量巨大。";
            }
            public class RobotSlickster
            {
                public static LocString NAME = "机械浮油生物";
                public static LocString EFFECT = "浮油生物能将二氧化碳转化为原油。";
                public static LocString DESC = "浮油生物是一种悬浮生物，常见于油质生态。浮油生物能够悬浮在固体和足够质量的液体之上。";
            }
        }

        public class CONFIGURATIONITEM
        {
            public static LocString CATEGORY = "建筑配置";
            public static LocString POWERMULTI = "机械小动物能耗与产量倍率";
            public static LocString POWERMULTI_TIP = "影响除机械浮游生物和机械飞鱼外其他机械小动物的能耗和转化速度";

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
            public class RobotPacu_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械帕库鱼转化系数";
            }
            public class RobotPokeshell_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械抛壳蟹转化系数";
            }
            public class RobotPuft_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械喷浮飞鱼转化系数";
            }
            public class RobotShoveVole_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械锹环田鼠转化系数";
            }
            public class RobotSlickster_UI
            {
                public static LocString CONVERSION_COEFFICIENT = "机械浮游生物转化系数";
            }
        }
            
        
        
    }
}
