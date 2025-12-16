using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STRINGS.ITEMS;

namespace sinevil.Robot_Animal_Remastered
{
    public class Utils
    {

        // 辅助方法：获取物品显示名称（兼容元素/食物/种子）
        public static string GetItemDisplayName(Tag tag)
        {
            // 优先尝试获取元素名称
            Element element = ElementLoader.FindElementByTag(tag);
            if (element != null) return element.name;
            StringEntry name;
            // 尝试获取种子名称
            if (tag.ToString().EndsWith("Seed") && Strings.TryGet(new StringKey($"STRINGS.CREATURES.SPECIES.SEEDS.{RemoveSeedSuffix(tag.ToString()).ToUpper()}.NAME"), out name))
            {
                return name;
            }
            // 尝试获取食物名称
            if (Strings.TryGet(new StringKey($"STRINGS.ITEMS.FOOD.{tag.ToString().ToUpper()}.NAME"), out name))
            {
                return name;
            }
            // 尝试获取其他名称
            if (Strings.TryGet(new StringKey($"STRINGS.CREATURES.SPECIES.{tag.ToString().ToUpper()}.NAME"), out name))
            {
                return name;
            }
            if (Strings.TryGet(new StringKey($"STRINGS.ITEMS.INGREDIENTS.{tag.ToString().ToUpper()}.NAME"), out name))
            {
                return name;
            }
            if (tag.ToString().EndsWith("Config") && Strings.TryGet(new StringKey($"STRINGS.ITEMS.INGREDIENTS.{RemoveConfigSuffix(tag.ToString()).ToUpper()}.NAME"), out name))
            {
                return name;
            }
            // 兜底返回标签名
            return tag.ToString();
        }


        /// <summary>
        /// 去除字符串末尾的“Seed”
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns></returns>
        public static string RemoveSeedSuffix(string input) =>
            !string.IsNullOrEmpty(input)
            ? input.Substring(0, input.Length - 4)
            : input;


        /// <summary>
        /// 去除字符串末尾的“Config”
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns></returns>
        public static string RemoveConfigSuffix(string input) =>
            !string.IsNullOrEmpty(input)
            ? input.Substring(0, input.Length - 6)
            : input;
    }
}
