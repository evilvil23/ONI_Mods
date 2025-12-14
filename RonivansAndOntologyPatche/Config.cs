using Newtonsoft.Json;
using PeterHan.PLib.Options;
using System;

namespace sinevil.ONI_Ronivans_Patch
{

    public abstract class SingletonOptions<T> where T : class, new()
    {
        public static T Instance
        {
            get
            {

                bool flag = SingletonOptions<T>.instance == null;
                if (flag)
                {
                    T t;
                    bool flag2 = (t = PeterHan.PLib.Options.POptions.ReadSettings<T>()) == null;
                    if (flag2)
                    {
                        t = Activator.CreateInstance<T>();
                    }
                    SingletonOptions<T>.instance = t;
                }
                return SingletonOptions<T>.instance;
            }
            protected set
            {
                bool flag = value != null;
                if (flag)
                {
                    SingletonOptions<T>.instance = value;
                }
            }
        }

        // Token: 0x04000001 RID: 1
        protected static T instance;
    }

    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile("Ronivans_Patch.json", true, true)]
    [RestartRequired]
    public class Config : SingletonOptions<Config>
    {
        public Config()
        {
            this.先进金属精炼机 = false;
            this.选择性电弧炉 = false;
            this.等离子电弧炉 = false;
            this.化学混合装置 = false;
            this.先进窑炉 = false;
            this.水泥搅拌机 = false;
        }
        [JsonProperty]
        [PeterHan.PLib.Options.Option(ConfigStrings.REFINGMACHINE, "", null)]
        public bool 先进金属精炼机 { get; set; }

        [JsonProperty]
        [PeterHan.PLib.Options.Option(ConfigStrings.SELETIVE_ARCFURNACE, "", null)]
        public bool 选择性电弧炉 { get; set; }

        [JsonProperty]
        [PeterHan.PLib.Options.Option(ConfigStrings.ELECTROMAGNETIC_ARC_FURNACE, "", null)]
        public bool 等离子电弧炉 { get; set; }

        [JsonProperty]
        [PeterHan.PLib.Options.Option(ConfigStrings.CHEMICAL_MIXER, "", null)]
        public bool 化学混合装置 { get; set; }

        [JsonProperty]
        [PeterHan.PLib.Options.Option(ConfigStrings.ADVANCED_KILN, "", null)]
        public bool 先进窑炉 { get; set; }


        [JsonProperty]
        [PeterHan.PLib.Options.Option(ConfigStrings.CEMENT_MIXER, "", null)]
        public bool 水泥搅拌机 { get; set; }

        internal static class ConfigStrings
        {
            // 分类
            public const string category_0 = "STRINGS.CONGIF.CATEGORY_0";
            public const string category_1 = "STRINGS.CONGIF.CATEGORY_1";

            public const string REFINGMACHINE = "STRINGS.CONFIG.IndustrialRevolution.REFINGMACHINE"; // 先进金属精炼机
            public const string SELETIVE_ARCFURNACE = "STRINGS.CONFIG.IndustrialRevolution.SELETIVE_ARCFURNACE"; // 选择性电弧炉
            public const string ELECTROMAGNETIC_ARC_FURNACE = "STRINGS.CONFIG.IndustrialRevolution.PLASMA_ARCFURNACE"; // 等离子电弧炉
            public const string CHEMICAL_MIXER = "STRINGS.CONFIG.IndustrialRevolution.CHEMICAL_MIXING_DEVICE"; // 化学混合装置
            public const string ADVANCED_KILN = "STRINGS.CONFIG.IndustrialRevolution.ADVANCED_KILN"; // 先进窑炉
            public const string CEMENT_MIXER = "STRINGS.CONFIG.IndustrialRevolution.CEMENT_MIXER"; // 水泥搅拌机

        }
    }
}
