using PeterHan.PLib.Options;
using STRINGS;
using System.Collections.Generic;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace sinevil.Robot_Animal_Remastered
{
    public static class Configration
    {
        internal static readonly ConfigurationItem config = SingletonOptions<ConfigurationItem>.Instance;
    }
    /**
     * 机械蛞蝓
     */
    public class RobotStaterpillarConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotStaterpillar";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotStaterpillar_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;

        // 配置存储规则（隐藏+保鲜）
        List<Storage.StoredItemModifier> gasOutputModifiers = new List<Storage.StoredItemModifier>
        {
        Storage.StoredItemModifier.Hide,
        Storage.StoredItemModifier.Preserve
        };

        public override BuildingDef CreateBuildingDef()
        {
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            string[] constructionMaterials = { "METAL" };
            float[] constructionMass = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0] };

            // 创建建筑定义
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(
                id: ID,
                width: BUILD_WIDTH,
                height: BUILD_HEIGHT,
                anim: ANIM,
                hitpoints: HIT_POINTS,
                construction_time: CONSTRUCTION_TIME,
                construction_mass: constructionMass,
                construction_materials: constructionMaterials,
                melting_point: MELTING_POINT,
                build_location_rule: build_location_rule,
                decor: BUILDINGS.DECOR.PENALTY.TIER1,
                noise: NOISE_POLLUTION.NOISY.TIER5,
                0.2f
            );

            // 建筑特殊属性配置
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.EnergyConsumptionWhenActive = 60f;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;
            // 气体输出端口
            buildingDef.OutputConduitType = ConduitType.Gas;
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            complexFabricator.outputOffset = new Vector3(1f, 0f, 0f);


            go.AddOrGet<FabricatorIngredientStatusManager>();
            // 1. 创建复杂制造器存储（包含in/out/build三类存储）
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);

            // ========== 新增：配置氢气输出存储 ==========
            // ① 设定输出存储容量（按需调整，示例为1000kg）
            complexFabricator.outStorage.capacityKg = 1000f;
            // ② 强制生产的氢气自动存入outStorage（核心：替代掉落/暴露）
            complexFabricator.storeProduced = true;

            complexFabricator.outStorage.SetDefaultStoredItemModifiers(gasOutputModifiers);

            // ========== 新增：添加气体管道分配器（核心输出组件） ==========
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            // ① 绑定氢气存储源：仅从outStorage提取产物
            conduitDispenser.storage = complexFabricator.outStorage;
            // ② 匹配端口类型：气体（与建筑定义的OutputConduitType一致）
            conduitDispenser.conduitType = ConduitType.Gas;
            // ③ 无过滤（仅生产氢气，无需筛选）
            conduitDispenser.elementFilter = null;
            // ④ 强制持续输出：只要存储有氢气就自动泵入气体端口
            conduitDispenser.alwaysDispense = true;

            // 获取设置中的转化系数
            float conversionCoefficient = Configration.config.robotStaterpillar_Conversion_Coefficient;

            // ========== 添加配方 ==========
            // 筛选：固体 + 矿石标签 + 非不可粉碎 + 有有效相变的元素
            foreach (Element element in ElementLoader.elements.FindAll((Element e) =>
                e.IsSolid &&                  // 必须是固体
                e.HasTag(GameTags.Ore) &&     // 核心：限定为矿石类元素（替代原金属标签）
                !e.HasTag(GameTags.Noncrushable) // 排除不可粉碎的矿石
            ))
            {
                // 补充：校验相变逻辑（避免无效配方）
                Element lowTempTransition = element.highTempTransition.lowTempTransition;
                if (lowTempTransition != null && lowTempTransition != element)
                {
                    // 日志明确标注矿石+目标产物，便于调试
                    Debug.Log("机械蛞蝓添加配方: 矿石[" + element.tag + "] --> Hydrogen", go);
                    // 调用配方添加方法（保留原参数，仅限定原材料为矿石）
                    this.addConfigureRecipes(element.tag, element.name, 60f, conversionCoefficient*60);
                }
                else
                {
                    // 日志提示无效相变的矿石，便于排查
                    Debug.LogWarning("机械蛞蝓配方跳过: 矿石[" + element.tag + "] 无相变目标", go);
                }
            }



        }
        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械蛞蝓已加载");
        }

        // 添加配方
        // 输入参数：tag：资源标签，name：资源名称，consumedAmount：消耗量，outputAmount：产出量
        // 产出：氢气 Hydrogen
        private void addConfigureRecipes(Tag tag, string name, float consumedAmount, float outputAmount)
        {
            
            ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(tag, consumedAmount)
            };
            ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(GameTags.Hydrogen, outputAmount)
            };
            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("RobotHatch", array, array2), array, array2);
            // 配方消耗时间 60秒
            complexRecipe.time = 60f;
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, name, ELEMENTS.HYDROGEN.NAME);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create(ID)
            };
        }
    }

    /**
     * 机械尖块兽
     */
    public class RobotStegoConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotStego";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotStego_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;

        // 配置存储规则（隐藏+保鲜）
        List<Storage.StoredItemModifier> gasOutputModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Preserve
        };

        public override BuildingDef CreateBuildingDef()
        {
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            string[] constructionMaterials = { "METAL" };
            float[] constructionMass = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0] };

            // 创建建筑定义
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(
                id: ID,
                width: BUILD_WIDTH,
                height: BUILD_HEIGHT,
                anim: ANIM,
                hitpoints: HIT_POINTS,
                construction_time: CONSTRUCTION_TIME,
                construction_mass: constructionMass,
                construction_materials: constructionMaterials,
                melting_point: MELTING_POINT,
                build_location_rule: build_location_rule,
                decor: BUILDINGS.DECOR.PENALTY.TIER1,
                noise: NOISE_POLLUTION.NOISY.TIER5,
                0.2f
            );

            // 建筑特殊属性配置
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.EnergyConsumptionWhenActive = 60f;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);

            // 获取设置中的转化系数
            float conversionCoefficient = Configration.config.robotStego_Conversion_Coefficient;
            // ========== 添加配方 ==========
            // 当系数为1时，每0.4kg漫花果产出20kg泥炭和0.5kg硬肉
            this.addConfigureRecipes("VineFruit", STRINGS.ITEMS.FOOD.VINEFRUIT.NAME, 0.4f, conversionCoefficient*20, conversionCoefficient*0.5f);
            this.addConfigureRecipes("PrickleFruit", STRINGS.ITEMS.FOOD.PRICKLEFRUIT.NAME, 0.08125f, conversionCoefficient*20, conversionCoefficient*0.5f);
            this.addConfigureRecipes("SwampFruit", STRINGS.ITEMS.FOOD.SWAMPFRUIT.NAME, 0.07065f, conversionCoefficient*30, conversionCoefficient*0.5f);

        }
        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械尖块兽已加载");
        }

        // 添加配方
        // 输入参数：tag：资源标签，name：资源名称，consumedAmount：消耗量，outputAmount0：泥炭产出量 ,outputAmount1：硬肉产出量
        // 产出：泥炭 Peat
        private void addConfigureRecipes(string tag, string name, float consumedAmount, float outputAmount0, float outputAmount1)
        {
            ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(TagExtensions.ToTag(tag), consumedAmount)
            };
            ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Peat.CreateTag(), outputAmount0),
                new ComplexRecipe.RecipeElement("DinosaurMeat", outputAmount1)
            };
            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("RobotStego", array, array2), array, array2);
            complexRecipe.time = 60f;
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, name, ELEMENTS.CARBON.NAME);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create("RobotStego")
            };
        }
    }

    /**
     * 机械哈奇
     */
    public class RobotHatchConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotHatch";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotHatch_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;

        // 复用哈奇的饮食常量（与原版逻辑对齐）
        private const SimHashes EMIT_ELEMENT = SimHashes.Carbon;
        private static float KG_ROCK_EATEN_PER_CYCLE = 14f; // 每60s吃掉的矿石质量
        private float CONVERSION_EFFICIENCY_ROCK = Configration.config.robotHatch_Rock_Conversion_Coefficient; // 岩石转换效率
        private float CONVERSION_EFFICIENCY_FOOD = Configration.config.robotHatch_Food_Conversion_Coefficient; // 食物转化效率

        private float recipeTime = 60f;

        // 配置存储规则（隐藏+保鲜）
        List<Storage.StoredItemModifier> gasOutputModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Preserve
        };
        public override BuildingDef CreateBuildingDef()
        {
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            string[] constructionMaterials = { "METAL" };
            float[] constructionMass = { BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0] };

            // 创建建筑定义
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(
                id: ID,
                width: BUILD_WIDTH,
                height: BUILD_HEIGHT,
                anim: ANIM,
                hitpoints: HIT_POINTS,
                construction_time: CONSTRUCTION_TIME,
                construction_mass: constructionMass,
                construction_materials: constructionMaterials,
                melting_point: MELTING_POINT,
                build_location_rule: build_location_rule,
                decor: BUILDINGS.DECOR.PENALTY.TIER1,
                noise: NOISE_POLLUTION.NOISY.TIER5,
                0.2f
            );

            // 建筑特殊属性配置
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.EnergyConsumptionWhenActive = 60f;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002320 File Offset: 0x00000520
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);

            float KG_CARBON_OUT_FOR_ORE = KG_ROCK_EATEN_PER_CYCLE * CONVERSION_EFFICIENCY_ROCK; //  岩石配方每60s产出的碳量
            this.AddConfigureRecipes("Sand", ELEMENTS.SAND.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 沙子
            this.AddConfigureRecipes("SandStone", ELEMENTS.SANDSTONE.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 砂岩
            this.AddConfigureRecipes("Clay", ELEMENTS.CLAY.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 粘土
            this.AddConfigureRecipes("CrushedRock", ELEMENTS.CRUSHEDROCK.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 碎石
            this.AddConfigureRecipes("Dirt", ELEMENTS.DIRT.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 泥土
            this.AddConfigureRecipes("SedimentaryRock", ELEMENTS.SEDIMENTARYROCK.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 沉积岩
            this.AddConfigureRecipes("IgneousRock", ELEMENTS.IGNEOUSROCK.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 火成岩
            this.AddConfigureRecipes("Obsidian", ELEMENTS.OBSIDIAN.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 黑曜石
            this.AddConfigureRecipes("Granite", ELEMENTS.GRANITE.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 花岗岩
            this.AddConfigureRecipes("Shale", ELEMENTS.SHALE.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 页岩


            Tag carbonTag = EMIT_ELEMENT.CreateTag();
            string carbonName = ELEMENTS.CARBON.NAME;


            // 2. 食物饮食配方（对应BaseHatchConfig.FoodDiet）
            List<Diet.Info> foodDiet = BaseHatchConfig.FoodDiet(carbonTag, 0f, CONVERSION_EFFICIENCY_FOOD, null, 0f);
            GenerateRecipesFromDietInfo(foodDiet, carbonName, CONVERSION_EFFICIENCY_FOOD);

        }

        /// <summary>
        /// 为机械哈奇添加配方
        /// </summary>
        /// <param name="tag">输入物标签</param>
        /// <param name="name">输入物名称</param>
        /// <param name="consumedAmount">消耗量</param>
        /// <param name="outputAmount">煤炭产出量</param>
        private void AddConfigureRecipes(Tag tag, string name, float consumedAmount, float outputAmount)
        {
            ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(tag, consumedAmount)
            };
            ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement("Carbon", outputAmount)
            };
            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(ID, array, array2), array, array2);
            complexRecipe.time = recipeTime;
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, name, ELEMENTS.CARBON.NAME);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create(ID)
            };
        }

        // 从Diet.Info生成对应配方
        private void GenerateRecipesFromDietInfo(List<Diet.Info> dietInfos, string outputName, float conversionEfficiency)
        {
            foreach (Diet.Info dietInfo in dietInfos)
            {
                foreach (Tag inputTag in dietInfo.consumedTags)
                {
                    // 获取输入物品名称（兼容元素和食物标签）
                    string inputName = GetItemDisplayName(inputTag);

                    // 计算消耗/产出量（复用哈奇的每周期消耗量逻辑）
                    float consumedAmount = KG_ROCK_EATEN_PER_CYCLE;
                    // 食物类物品按卡路里调整消耗量（避免数值过大）
                    consumedAmount = GetFoodAdjustedConsumption(inputTag);
                    float outputAmount = consumedAmount * conversionEfficiency;

                    // 生成单个配方
                    AddConfigureRecipes(inputTag, inputName, consumedAmount, outputAmount);
                }
            }
        }



        // 辅助方法：获取物品显示名称（兼容元素/食物）
        private string GetItemDisplayName(Tag tag)
        {
            // 优先尝试获取元素名称
            Element element = ElementLoader.FindElementByTag(tag);
            if (element != null) return element.name;
            // 尝试获取食物名称
            StringEntry foodName;
            if (Strings.TryGet(new StringKey($"STRINGS.ITEMS.FOOD.{tag.ToString().ToUpper()}.NAME"), out foodName))
            {
                return foodName;
            }

            // 兜底返回标签名
            return tag.ToString();
        }

        // 辅助方法：调整食物类物品的消耗量（避免与岩石使用相同的14kg）
        private float GetFoodAdjustedConsumption(Tag foodTag)
        {
            // 遍历所有食物类型，按卡路里计算合理消耗量
            foreach (EdiblesManager.FoodInfo foodInfo in EdiblesManager.GetAllLoadedFoodTypes())
            {
                if (foodInfo.Id == foodTag.ToString() && foodInfo.CaloriesPerUnit > 0)
                {
                    // 按卡路里反算消耗量（目标：每周期消耗的总卡路里与原版哈奇一致）
                    return (HatchTuning.STANDARD_CALORIES_PER_CYCLE / foodInfo.CaloriesPerUnit) * recipeTime/600;
                }
            }
            // 兜底值
            return 1f;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
        }


    }

}