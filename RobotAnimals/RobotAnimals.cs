using CykUtils;
using PeterHan.PLib.Options;
using STRINGS;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;
using static STRINGS.DUPLICANTS.TRAITS;
using BUILDINGS = TUNING.BUILDINGS;

namespace sinevil.Robot_Animal_Remastered
{
    public static class Configration
    {
        internal static readonly ConfigurationItem config = SingletonOptions<ConfigurationItem>.Instance;
        public static float PowerMulti = config.powerMulti;
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

        private const float recipeTime = 60f;

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
            buildingDef.EnergyConsumptionWhenActive = 60f*Configration.PowerMulti;
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
            go.AddOrGet<CopyBuildingSettings>();
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
                    this.addConfigureRecipes(element.tag, element.name, 60f * Configration.PowerMulti, conversionCoefficient*60 * Configration.PowerMulti);
                }
                else
                {
                    // 日志提示无效相变的矿石，便于排查
                    Debug.LogWarning("机械蛞蝓配方跳过: 矿石[" + element.tag + "] 无相变目标", go);
                }
            }
            // 筛选：精炼金属
            foreach (Element element in ElementLoader.elements.FindAll((Element e) =>
                e.IsSolid &&
                e.HasTag(GameTags.RefinedMetal) &&
                !e.disabled
            ))
            {
                this.addConfigureRecipes(element.tag, element.name, 60f * Configration.PowerMulti, conversionCoefficient * 60 * Configration.PowerMulti);
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
            complexRecipe.time = recipeTime;
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
            buildingDef.EnergyConsumptionWhenActive = 60f * Configration.PowerMulti;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            go.AddOrGet<CopyBuildingSettings>();
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
            this.addConfigureRecipes("VineFruit", STRINGS.ITEMS.FOOD.VINEFRUIT.NAME, 0.4f * Configration.PowerMulti, 20 * conversionCoefficient * Configration.PowerMulti, 0.5f * conversionCoefficient * Configration.PowerMulti);
            this.addConfigureRecipes("PrickleFruit", STRINGS.ITEMS.FOOD.PRICKLEFRUIT.NAME, 0.08125f * Configration.PowerMulti, 20* conversionCoefficient * Configration.PowerMulti, 0.5f * conversionCoefficient * Configration.PowerMulti);
            this.addConfigureRecipes("SwampFruit", STRINGS.ITEMS.FOOD.SWAMPFRUIT.NAME, 0.07065f * Configration.PowerMulti, 30 * conversionCoefficient * Configration.PowerMulti, 0.5f * conversionCoefficient * Configration.PowerMulti);

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
        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械尖块兽已加载");
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
        private Tag EMIT_ELEMENT = SimHashes.Carbon.CreateTag();
        private static float KG_ROCK_EATEN_PER_CYCLE = 14f * Configration.PowerMulti; // 每60s吃掉的矿石质量
        private float CONVERSION_EFFICIENCY_ROCK = Configration.config.robotHatch_Rock_Conversion_Coefficient; // 岩石转换效率
        private float CONVERSION_EFFICIENCY_FOOD = Configration.config.robotHatch_Food_Conversion_Coefficient; // 食物转化效率

        private float recipeTime = 60f;

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
            buildingDef.EnergyConsumptionWhenActive = 30f * Configration.PowerMulti;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);

            float KG_CARBON_OUT_FOR_ORE = KG_ROCK_EATEN_PER_CYCLE * CONVERSION_EFFICIENCY_ROCK; //  岩石配方每60s产出的碳量
            this.AddConfigureRecipe("Sand", ELEMENTS.SAND.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 沙子
            this.AddConfigureRecipe("SandStone", ELEMENTS.SANDSTONE.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 砂岩
            this.AddConfigureRecipe("Clay", ELEMENTS.CLAY.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 粘土
            this.AddConfigureRecipe("CrushedRock", ELEMENTS.CRUSHEDROCK.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 碎石
            this.AddConfigureRecipe("Dirt", ELEMENTS.DIRT.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 泥土
            this.AddConfigureRecipe("SedimentaryRock", ELEMENTS.SEDIMENTARYROCK.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 沉积岩
            this.AddConfigureRecipe("IgneousRock", ELEMENTS.IGNEOUSROCK.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 火成岩
            this.AddConfigureRecipe("Obsidian", ELEMENTS.OBSIDIAN.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 黑曜石
            this.AddConfigureRecipe("Granite", ELEMENTS.GRANITE.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 花岗岩
            this.AddConfigureRecipe("Shale", ELEMENTS.SHALE.NAME, KG_ROCK_EATEN_PER_CYCLE, KG_CARBON_OUT_FOR_ORE); // 页岩

            this.AddConfigureRecipe(
                new List<Tag> { SimHashes.Salt.CreateTag() },
                new List<StringKey> { new StringKey("STRINGS.ELEMENTS.SOLIDBORAX.NAME") },
                new List<float> { 14f },
                new List<Tag> { "SolidBorax" },
                new List<StringKey> { new StringKey("STRINGS.ELEMENTS.SALT.NAME") },
                new List<float> { 3.5f },
                recipeTime);

            string carbonName = ELEMENTS.CARBON.NAME;


            // 2. 食物饮食配方（对应BaseHatchConfig.FoodDiet）
            List<Diet.Info> foodDiet = BaseHatchConfig.FoodDiet(EMIT_ELEMENT, 0f, CONVERSION_EFFICIENCY_FOOD, null, 0f);
            CykUtils.LogUtil.Log($"机械哈奇已获取{foodDiet.Count} 个食物食谱");
            GenerateRecipesFromDietInfo(foodDiet, CONVERSION_EFFICIENCY_FOOD);

        }

        /// <summary>
        /// 最通用的配方添加方法
        /// 为机械机械小动物添加配方
        /// </summary>
        /// <param name="inTags">输入物标签列表</param>
        /// <param name="inNames">输入物名称列表</param>
        /// <param name="consumedAmounts">输入物消耗量列表</param>
        /// <param name="outTags">输出物标签列表</param>
        /// <param name="outNames">输出物名称列表</param>
        /// <param name="outputAmounts">输出物产量列表</param>
        /// <param name="recipeTime">配方时间</param>
        private void AddConfigureRecipe(List<Tag> inTags, List<StringKey> inNames, List<float> consumedAmounts, List<Tag> outTags, List<StringKey> outNames, List<float> outputAmounts, float recipeTime)
        {
            // 步骤1：创建临时列表存储批量生成的RecipeElement
            List<ComplexRecipe.RecipeElement> array = new List<ComplexRecipe.RecipeElement>();
            List<ComplexRecipe.RecipeElement> array2 = new List<ComplexRecipe.RecipeElement>();
            // 步骤2：遍历inTags和consumedAmounts，批量添加元素
            for (int i = 0; i < inTags.Count; i++)
                array.Add(new ComplexRecipe.RecipeElement(inTags[i], consumedAmounts[i]));

            // 步骤3：遍历outTags和outputAmounts，批量添加元素
            for (int i = 0; i < outTags.Count; i++)
                array2.Add(new ComplexRecipe.RecipeElement(outTags[i], outputAmounts[i]));

            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(ID, array, array2), array.ToArray(), array2.ToArray());
            complexRecipe.time = recipeTime;

            // 拼接输入物字符串 和 输出物字符串
            StringEntry ot;
            string inString = string.Join(", ", inNames.Select(entry =>
            {
                Strings.TryGet(entry, out ot);
                return ot;
            }));
            string outString = string.Join(", ", outNames.Select(entry =>
            {
                Strings.TryGet(entry, out ot);
                return ot;
            }));

            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, inString, outString);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create(ID)
            };
        }

        /// <summary>
        /// 为机械机械小动物添加配方
        /// </summary>
        /// <param name="tag">输入物标签</param>
        /// <param name="name">输入物名称</param>
        /// <param name="consumedAmount">消耗量</param>
        /// <param name="outputAmount">煤炭产出量</param>
        private void AddConfigureRecipe(Tag tag, string name, float consumedAmount, float outputAmount)
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
        private void GenerateRecipesFromDietInfo(List<Diet.Info> dietInfos, float conversionEfficiency)
        {
            foreach (Diet.Info dietInfo in dietInfos)
            {
                foreach (Tag inputTag in dietInfo.consumedTags)
                {
                    // 获取输入物品名称（兼容元素和食物标签）
                    string inputName = Utils.GetItemDisplayName(inputTag);

                    // 计算消耗/产出量（复用哈奇的每周期消耗量逻辑）
                    float consumedAmount = KG_ROCK_EATEN_PER_CYCLE;
                    // 食物类物品按卡路里调整消耗量（避免数值过大）
                    consumedAmount = GetFoodAdjustedConsumption(inputTag) * Configration.PowerMulti;
                    float outputAmount = consumedAmount * conversionEfficiency * Configration.PowerMulti;

                    // 生成单个配方
                    AddConfigureRecipe(inputTag, inputName, consumedAmount, outputAmount);
                }
            }
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
            CykUtils.LogUtil.Log("机械哈奇已加载");
        }
    }

    /**
     * 机械帕库鱼
     */
    public class RobotPacuConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotPacu";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotPacu_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;

        private float recipeTime = 60f;

        private float CONVERSION_EFFICIENCY = Configration.config.robotPacu_Conversion_Coefficient; // 食物转化效率

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
            buildingDef.EnergyConsumptionWhenActive = 30f * Configration.PowerMulti;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);

            // 1. 藻类和海梳蕨叶配方
            AddConfigureRecipe(SimHashes.Algae.CreateTag(), 0.75f * Configration.PowerMulti, 0.38f * CONVERSION_EFFICIENCY * Configration.PowerMulti);
            AddConfigureRecipe(KelpConfig.ID, 2f * Configration.PowerMulti, 1f * CONVERSION_EFFICIENCY * Configration.PowerMulti);



            List<Tag> seedTags = new List<Tag>();
            { 
                seedTags.Add("ColdWheatSeed"); // 冰霜麦粒
                seedTags.Add("DewDripperPlantSeed"); // 露珠藤种子
                seedTags.Add("FlyTrapPlantSeed"); // 露饵花种子
                seedTags.Add("GardenFoodPlantSeed"); // 汗甜玉米种子
                seedTags.Add("GasGrassSeed"); // 释气草种子
                seedTags.Add("HardSkinBerryPlantSeed"); // 刺壳果灌木种子
                seedTags.Add("CarrotPlantSeed"); // 羽叶果薯种子
                seedTags.Add("ButterflyPlantSeed"); // 拟种
                seedTags.Add("BasicFabricMaterialPlantSeed"); // 顶针芦苇种子
                seedTags.Add("BasicSingleHarvestPlantSeed"); // 米虱木种子
                seedTags.Add("BeanPlantSeed"); // 小吃豆
                seedTags.Add("BlueGrassSeed"); // 气囊芦荟种子
                seedTags.Add("MushroomSeed"); // 夜幕菇孢子
                seedTags.Add("SwampLilySeed"); // 芳香百合种子
                seedTags.Add("SaltPlantSeed"); // 沙盐藤种子
                seedTags.Add("SwampHarvestPlantSeed"); // 沼浆笼种子
                seedTags.Add("SeaLettuceSeed"); // 水草种子
                seedTags.Add("SpiceVineSeed"); // 火椒种子
                seedTags.Add("WormPlantSeed"); // 虫果种子
                seedTags.Add("SpaceTreeSeed"); // 糖心树种子
                seedTags.Add("PrickleFlowerSeed"); // 毛刺花种子

                seedTags.Add("SnowSculptures_PineCone"); // mod 圣诞树_松果


            }

            GenerateRecipesFromTags(seedTags : seedTags, CONVERSION_EFFICIENCY);

        }

        /// <summary>
        /// 为机械机械小动物添加配方
        /// </summary>
        /// <param name="tag">输入物标签</param>
        /// <param name="name">输入物名称</param>
        /// <param name="consumedAmount">消耗量</param>
        /// <param name="outputAmount">产出量</param>
        private void AddConfigureRecipe(Tag tag, float consumedAmount, float outputAmount)
        {
            AddConfigureRecipe(new List<Tag> { tag }, new List<float> { consumedAmount }, new List<Tag> { SimHashes.ToxicSand.CreateTag() }, new List<float> { outputAmount } , recipeTime);
        }

        /// <summary>
        /// 为机械机械小动物添加配方
        /// </summary>
        /// <param name="inTags">输入物标签列表</param>
        /// <param name="consumedAmounts">输入物消耗量列表</param>
        /// <param name="outTags">输出物标签列表</param>
        /// <param name="outputAmounts">输出物产量列表</param>
        /// <param name="recipeTime">配方时间</param>
        private void AddConfigureRecipe(List<Tag> inTags, List<float> consumedAmounts,List<Tag> outTags, List<float> outputAmounts, float recipeTime)
        {
            // 步骤1：创建临时列表存储批量生成的RecipeElement
            List<ComplexRecipe.RecipeElement> array = new List<ComplexRecipe.RecipeElement>();
            List<ComplexRecipe.RecipeElement> array2 = new List<ComplexRecipe.RecipeElement>();
            // 步骤2：遍历inTags和consumedAmounts，批量添加元素
            for (int i = 0; i < inTags.Count; i++)
                array.Add(new ComplexRecipe.RecipeElement(inTags[i], consumedAmounts[i]));
            
            // 步骤3：遍历outTags和outputAmounts，批量添加元素
            for (int i = 0; i < outTags.Count; i++)
                array2.Add(new ComplexRecipe.RecipeElement(outTags[i], outputAmounts[i]));

            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(ID, array, array2), array.ToArray(), array2.ToArray());
            complexRecipe.time = recipeTime;

            // 拼接输入物字符串
            string inString = "";
            for (int i = 0; i < inTags.Count; i++)
            {
                
                inString += Utils.GetItemDisplayName(inTags[i]);
                if (i < inTags.Count - 1) inString += ", ";
            }
            // 拼接输出物字符串
            string outString = "";
            for (int i = 0; i < outTags.Count; i++)
            {
                outString += Utils.GetItemDisplayName(outTags[i]);
                if (i < outTags.Count - 1) outString += ", ";
            }
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, inString, outString);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create(ID)
            };
        }


        /// <summary>
        /// 为机械帕库鱼批量添加配方
        /// 每颗种子转化为500g污染土
        /// </summary>
        /// <param name="dietInfos"></param>
        /// <param name="outputName"></param>
        /// <param name="conversionEfficiency"></param>
        private void GenerateRecipesFromTags(List<Tag> seedTags, float conversionEfficiency)
        {
            // 产物列表
            List<Tag> productList = new List<Tag> { SimHashes.ToxicSand.CreateTag(), TagExtensions.ToTag("FishMeat"), TagExtensions.ToTag("PacuEgg") };
            // 产物数量
            List<float> outputAmountsList = new List<float> { 0.5f * conversionEfficiency * Configration.PowerMulti, 0.025f * conversionEfficiency * Configration.PowerMulti, 1f * conversionEfficiency * Configration.PowerMulti };

            CykUtils.LogUtil.Log("为机械帕库鱼添加" + seedTags.Count + "个种子配方 ");
            foreach (Tag inputTag in seedTags)
            {
                AddConfigureRecipe(
                    new List<Tag> { inputTag },
                    new List<float> { 10 * Configration.PowerMulti },
                    productList,
                    outputAmountsList,
                    120f
                    );
            }

        }


        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械帕库鱼已加载");
        }
    }


    /**
     * 机械抛壳蟹
     */
    public class RobotPokeshellConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotPokeshell";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 2;
        // 建筑动画名称
        private const string ANIM = "RobotPokeshell_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;

        private float recipeTime = 60f;

        private float CONVERSION_EFFICIENCY = Configration.config.robotPokeshell_Conversion_Coefficient; // 配方转化效率

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
            buildingDef.EnergyConsumptionWhenActive = 30f * Configration.PowerMulti;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);

            List<Tag> inTags = new List<Tag>() { SimHashes.ToxicSand.CreateTag() };
            List<float> consumedAmounts = new List<float>() { 7f * Configration.PowerMulti };
            AddConfigureRecipe(inTags, consumedAmounts, 
                new List<Tag> { "CrabShell", SimHashes.Sand.CreateTag()}, 
                new List<float> { 1f * CONVERSION_EFFICIENCY * Configration.PowerMulti, 0.02f * CONVERSION_EFFICIENCY * Configration.PowerMulti },
                recipeTime);
            AddConfigureRecipe(inTags, consumedAmounts,
                new List<Tag> { "ShellfishMeat", SimHashes.Sand.CreateTag() },
                new List<float> { 0.4f * CONVERSION_EFFICIENCY * Configration.PowerMulti, 0.02f * CONVERSION_EFFICIENCY * Configration.PowerMulti },
                recipeTime);

            // 工业革命配方： 炉渣 --> 硝酸盐结晶
            AddConfigureRecipe(new List<Tag>() { "SolidSlag" }, consumedAmounts,
                new List<Tag> { "AmmoniumSalt" },
                new List<float> { 5.25f * CONVERSION_EFFICIENCY * Configration.PowerMulti },
                recipeTime);

        }


        /// <summary>
        /// 为机械机械小动物添加配方
        /// </summary>
        /// <param name="inTags">输入物标签列表</param>
        /// <param name="consumedAmounts">输入物消耗量列表</param>
        /// <param name="outTags">输出物标签列表</param>
        /// <param name="outputAmounts">输出物产量列表</param>
        /// <param name="recipeTime">配方时间</param>
        private void AddConfigureRecipe(List<Tag> inTags, List<float> consumedAmounts, List<Tag> outTags, List<float> outputAmounts, float recipeTime)
        {
            // 步骤1：创建临时列表存储批量生成的RecipeElement
            List<ComplexRecipe.RecipeElement> array = new List<ComplexRecipe.RecipeElement>();
            List<ComplexRecipe.RecipeElement> array2 = new List<ComplexRecipe.RecipeElement>();
            // 步骤2：遍历inTags和consumedAmounts，批量添加元素
            for (int i = 0; i < inTags.Count; i++)
                array.Add(new ComplexRecipe.RecipeElement(inTags[i], consumedAmounts[i]));

            // 步骤3：遍历outTags和outputAmounts，批量添加元素
            for (int i = 0; i < outTags.Count; i++)
                array2.Add(new ComplexRecipe.RecipeElement(outTags[i], outputAmounts[i]));

            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(ID, array, array2), array.ToArray(), array2.ToArray());
            complexRecipe.time = recipeTime;

            // 拼接输入物字符串
            string inString = "";
            for (int i = 0; i < inTags.Count; i++)
            {

                inString += Utils.GetItemDisplayName(inTags[i]);
                if (i < inTags.Count - 1) inString += ", ";
            }
            // 拼接输出物字符串
            string outString = "";
            for (int i = 0; i < outTags.Count; i++)
            {
                outString += Utils.GetItemDisplayName(outTags[i]);
                if (i < outTags.Count - 1) outString += ", ";
            }
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, inString, outString);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create(ID)
            };
        }
        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械抛壳蟹已加载");
        }
    }

    /**
     * 机械喷浮飞鱼
     */
    public class RobotPuftConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotPuft";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotPuft_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;


        private float CONVERSION_EFFICIENCY = Configration.config.robotPuft_Conversion_Coefficient; // 配方转化效率

        public override BuildingDef CreateBuildingDef()
        {
            BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
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
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            Prioritizable.AddRef(go);
            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showInUI = true;
            storage.capacityKg = 200f;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            ElementConsumer elementConsumer = go.AddOrGet<ElementConsumer>();
            elementConsumer.elementToConsume = SimHashes.ContaminatedOxygen;
            elementConsumer.consumptionRate = 2f;
            elementConsumer.capacityKG = 5f;
            elementConsumer.consumptionRadius = 3;
            elementConsumer.showInStatusPanel = true;
            elementConsumer.sampleCellOffset = new Vector3(0f, 0f, 0f);
            elementConsumer.isRequired = false;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showDescriptor = false;
            elementConsumer.ignoreActiveChanged = true;
            ElementDropper elementDropper = go.AddComponent<ElementDropper>();
            elementDropper.emitMass = 10f;
            elementDropper.emitTag = SimHashes.SlimeMold.CreateTag();
            elementDropper.emitOffset = new Vector3(0f, 0f, 0f);
            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[]
            {
                new ElementConverter.ConsumedElement(SimHashes.ContaminatedOxygen.CreateTag(), 0.1f, true)
            };
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(0.1f*CONVERSION_EFFICIENCY, SimHashes.SlimeMold, 0f, false, true, 0f, 0.5f, 0.25f, byte.MaxValue, 0, true)
            };
            go.AddOrGet<MyAirFilter>();
            go.AddOrGet<KBatchedAnimController>().randomiseLoopedOffset = true;



        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械喷浮飞鱼已加载");
        }
    }

    /**
     * 机械锹环田鼠
     */
    public class RobotShoveVoleConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotShoveVole";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotShoveVole_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;

        private float recipeTime = 300;

        private float CONVERSION_EFFICIENCY = Configration.config.robotShoveVole_Conversion_Coefficient; // 配方转化效率

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
            buildingDef.EnergyConsumptionWhenActive = 30f * Configration.PowerMulti;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.heatedTemperature = 353.15f;
            complexFabricator.duplicantOperated = false;
            complexFabricator.showProgressBar = false;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);


            List<float> consumedAmounts = new List<float>() { 480f * Configration.PowerMulti };
            List<Tag> outTags = new List<Tag> { MeatConfig.ID, GingerConfig.ID };
            List<float> outputAmounts = new List<float> { 20f * CONVERSION_EFFICIENCY * Configration.PowerMulti, 16f * CONVERSION_EFFICIENCY * Configration.PowerMulti };
            // 浮土
            AddConfigureRecipe(
                new List<Tag> { SimHashes.Regolith.CreateTag()},
                consumedAmounts,
                outTags,
                outputAmounts,
                recipeTime);

            AddConfigureRecipe(
                new List<Tag> { SimHashes.Dirt.CreateTag() },
                consumedAmounts,
                outTags,
                outputAmounts,
                recipeTime);
            AddConfigureRecipe(
                new List<Tag> { SimHashes.IronOre.CreateTag() },
                consumedAmounts,
                outTags,
                outputAmounts,
                recipeTime);

        }


        /// <summary>
        /// 为机械机械小动物添加配方
        /// </summary>
        /// <param name="inTags">输入物标签列表</param>
        /// <param name="consumedAmounts">输入物消耗量列表</param>
        /// <param name="outTags">输出物标签列表</param>
        /// <param name="outputAmounts">输出物产量列表</param>
        /// <param name="recipeTime">配方时间</param>
        private void AddConfigureRecipe(List<Tag> inTags, List<float> consumedAmounts, List<Tag> outTags, List<float> outputAmounts, float recipeTime)
        {
            // 步骤1：创建临时列表存储批量生成的RecipeElement
            List<ComplexRecipe.RecipeElement> array = new List<ComplexRecipe.RecipeElement>();
            List<ComplexRecipe.RecipeElement> array2 = new List<ComplexRecipe.RecipeElement>();
            // 步骤2：遍历inTags和consumedAmounts，批量添加元素
            for (int i = 0; i < inTags.Count; i++)
                array.Add(new ComplexRecipe.RecipeElement(inTags[i], consumedAmounts[i]));

            // 步骤3：遍历outTags和outputAmounts，批量添加元素
            for (int i = 0; i < outTags.Count; i++)
                array2.Add(new ComplexRecipe.RecipeElement(outTags[i], outputAmounts[i]));

            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(ID, array, array2), array.ToArray(), array2.ToArray());
            complexRecipe.time = recipeTime;

            // 拼接输入物字符串
            string inString = "";
            for (int i = 0; i < inTags.Count; i++)
            {

                inString += Utils.GetItemDisplayName(inTags[i]);
                if (i < inTags.Count - 1) inString += ", ";
            }
            // 拼接输出物字符串
            string outString = "";
            for (int i = 0; i < outTags.Count; i++)
            {
                outString += Utils.GetItemDisplayName(outTags[i]);
                if (i < outTags.Count - 1) outString += ", ";
            }
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, inString, outString);
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create(ID)
            };
        }
        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            CykUtils.LogUtil.Log("机械锹环田鼠已加载");
        }
    }

    /**
     * 机械浮游生物
     */
    public class RobotSlicksterConfig : IBuildingConfig
    {
        // 建筑唯一ID
        private const string ID = "RobotSlickster";
        // 基础建筑属性常量
        private const int BUILD_WIDTH = 1;
        private const int BUILD_HEIGHT = 1;
        // 建筑动画名称
        private const string ANIM = "RobotSlickster_kanim";
        // 建筑生命值
        private const int HIT_POINTS = 100;
        // 施工时间
        private const float CONSTRUCTION_TIME = 10f;
        // 熔点 75摄氏度
        private const float MELTING_POINT = 167;


        private float CONVERSION_EFFICIENCY = Configration.config.robotSlickster_Conversion_Coefficient; // 配方转化效率

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
            buildingDef.EnergyConsumptionWhenActive = 30f;
            buildingDef.ExhaustKilowattsWhenActive = 0.5f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;

            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000031C4 File Offset: 0x000013C4
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showInUI = true;
            storage.capacityKg = 200f;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            ElementConsumer elementConsumer = go.AddOrGet<ElementConsumer>();
            elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
            elementConsumer.consumptionRate = 0.5f;
            elementConsumer.capacityKG = 5f;
            elementConsumer.consumptionRadius = 3;
            elementConsumer.showInStatusPanel = true;
            elementConsumer.sampleCellOffset = new Vector3(0f, 0f, 0f);
            elementConsumer.isRequired = false;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showDescriptor = false;
            elementConsumer.ignoreActiveChanged = true;
            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[]
            {
                new ElementConverter.ConsumedElement(SimHashes.CarbonDioxide.CreateTag(), 0.2f, true)
            };
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(0.2f*CONVERSION_EFFICIENCY, SimHashes.CrudeOil, 0f, false, false, 0f, 0.5f, 0.25f, byte.MaxValue, 0, true)
            };
            go.AddOrGet<MyAirFilter>();
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000032D3 File Offset: 0x000014D3
        public override void DoPostConfigureComplete(GameObject go)
        {
            Prioritizable.AddRef(go);
            go.AddOrGet<LoopingSounds>();
            go.AddOrGetDef<ActiveController.Def>();
            go.AddOrGet<KBatchedAnimController>().randomiseLoopedOffset = true;
            CykUtils.LogUtil.Log("机械浮游生物已加载");
        }

        
    }
}