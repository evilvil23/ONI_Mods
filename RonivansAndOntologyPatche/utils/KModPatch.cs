using CykUtils;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;


namespace Mechanical_Animal_Remastered_.utils
{
    public class KModPatch
    {
        public class ModLoad : UserMod2
        {

            public static string Namespace { get; private set; }
            public override void OnLoad(Harmony harmony)
            {
                //--------------------------
                base.OnLoad(harmony);
                ModLoad.Namespace = base.GetType().Namespace;
                //--------------------------
                //配置项组件
                PUtil.InitLibrary(true);
                new POptions().RegisterOptions(this, typeof(sinevil.ONI_Ronivans_Patch.Config));
                //--------------------------
                Namespace = base.GetType().Namespace;
                //--------------------------
            }
            // 本地化补丁，初始化时注册并加载翻译字符串。//
            [HarmonyPatch(typeof(Localization), "Initialize")]

            private class Translate_Initialize_Patch
            {
                public static void Postfix()
                {
                    Loc.Translate(typeof(sinevil.ONI_Ronivans_Patch.STRINGS), true);
                }
            }

        }
    }
}
