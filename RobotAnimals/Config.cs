using Newtonsoft.Json;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sinevil.Robot_Animal_Remastered
{


    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile("RobotAnimal.json", true, true)]
    [RestartRequired]
    public class ConfigurationItem : SingletonOptions<ConfigurationItem>
    {

        /// <summary>
        /// 哈奇岩石配方转换系数
        /// </summary>
        [Option(ConfigStrings.Hatch_Rock_ConversionCoefficient, "", ConfigStrings.category, Format = "F1")]
        [Limit(0, 1)]
        [JsonProperty]
        public float robotHatch_Rock_Conversion_Coefficient { get; set; } = 0.5f;

        /// <summary>
        /// 哈奇食物配方转换系数
        /// </summary>
        [Option(ConfigStrings.Hatch_Food_ConversionCoefficient, "", ConfigStrings.category, Format = "F2")]
        [Limit(0, 1)]
        [JsonProperty]
        public float robotHatch_Food_Conversion_Coefficient { get; set; } = 0.75f;

        /// <summary>
        /// 帕库鱼配方转换系数
        /// </summary>
        [Option(ConfigStrings.PacuConversionCoefficient, "", ConfigStrings.category, Format = "F1")]
        [Limit(0, 10)]
        [JsonProperty]
        public float robotPacu_Conversion_Coefficient { get; set; } = 1f;

        /// <summary>
        /// 抛壳蟹配方转换系数
        /// </summary>
        [Option(ConfigStrings.PokeshellConversionCoefficient, "", ConfigStrings.category, Format = "F1")]
        [Limit(0, 10)]
        [JsonProperty]
        public float robotPokeshell_Conversion_Coefficient { get; set; } = 1f;

        /// <summary>
        /// 喷浮飞鱼配方转换系数
        /// </summary>
        [Option(ConfigStrings.PuftConversionCoefficient, "", ConfigStrings.category, Format = "F2")]
        [Limit(0, 1)]
        [JsonProperty]
        public float robotPuft_Conversion_Coefficient { get; set; } = 0.95f;

        /// <summary>
        /// 锹环田鼠配方转换系数
        /// </summary>
        [Option(ConfigStrings.ShoveVoleConversionCoefficient, "", ConfigStrings.category, Format = "F1")]
        [Limit(0, 10)]
        [JsonProperty]
        public float robotShoveVole_Conversion_Coefficient { get; set; } = 1f;

        /// <summary>
        /// 蛞蝓配方转换系数
        /// </summary>
        [Option(ConfigStrings.StaterpillarConversionCoefficient, "", ConfigStrings.category, Format = "F1")]
        [Limit(0.1f, 1)]
        [JsonProperty]
        public float robotStaterpillar_Conversion_Coefficient { get; set; } = 0.1f;

        /// <summary>
        /// 尖块兽配方转换系数
        /// </summary>
        [Option(ConfigStrings.StaterpillarConversionCoefficient, "", ConfigStrings.category, Format = "F0")]
        [Limit(1, 10)]
        [JsonProperty]
        public float robotStego_Conversion_Coefficient { get; set; } = 1f;


    }
    internal static class ConfigStrings
    {
        // 分类
        public const string category = "RobotAnimalSTRINGS.CONFIGURATIONITEM.CATEGORY";

        public const string Hatch_Rock_ConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotHatch_UI.ROCK_CONVERSION_COEFFICIENT";
        public const string Hatch_Food_ConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotHatch_UI.FOOD_CONVERSION_COEFFICIENT";

        public const string PacuConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotPacu_UI.CONVERSION_COEFFICIENT";
        public const string PokeshellConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotPokeshell_UI.CONVERSION_COEFFICIENT";
        public const string PuftConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotPuft_UI.CONVERSION_COEFFICIENT";
        public const string ShoveVoleConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotShoveVole_UI.CONVERSION_COEFFICIENT";

        public const string StaterpillarConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotStaterpillar_UI.CONVERSION_COEFFICIENT";
        public const string StegoConversionCoefficient = "RobotAnimalSTRINGS.CONFIGURATIONITEM.RobotStego_UI.CONVERSION_COEFFICIENT";


    }
    
}
