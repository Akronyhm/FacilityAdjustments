using Base.Core;
using Base.Platforms;
using PhoenixPoint.Home.View.ViewControllers;
using PhoenixPoint.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FacilityAdjustments
{
    /// <summary>
    /// ModConfig is mod settings that players can change from within the game.
    /// Config is only editable from players in main menu.
    /// Only one config can exist per mod assembly.
    /// Config is serialized on disk as json.
    /// </summary>
    public class FacilityAdjustmentsConfig : ModConfig
    {

        [HarmonyLib.HarmonyPatch(typeof(ModSettingController), "Init")]
        public static class ModSettingController_Init_Patch
        {
            private static void Postfix(ModSettingController __instance, string label, System.Type type)
            {

                //FacilityAdjustmentsLogger.Info($"ModSettingController Postfix [{label}]");
                //FacilityAdjustmentsLogger.Info($"Type [{type.Name}]");
                //FacilityAdjustmentsLogger.Info($"Enabled and visible [{__instance.isActiveAndEnabled}]");
                try
                {
                    if (type == typeof(int))
                    {
                        __instance.TextField.gameObject.SetActive(true);
                        __instance.ToggleField.gameObject.SetActive(false);
                        __instance.ListField.gameObject.SetActive(false);
                        __instance.SliderFormatter.gameObject.SetActive(false);
                        __instance.Label.gameObject.SetActive(true);
                        __instance.gameObject.SetActive(true);
                    }
                }
                catch (Exception e)
                {
                    FacilityAdjustmentsLogger.Error(e);
                    throw;
                }
            }
        }

        public float MaterialProductionPerFacility = 0;
        public float TechProductionPerFacility = 0;


        public void ConstructConfigurationFields()
        {
            try
            {
                FacilityAdjustmentsLogger.Info("Beginning construction of mod settings");
                var fields = this.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).ToList().Select(x => new ModConfigField(x.Name, x.GetValue(this).GetType())
                {
                    GetValue = () => x.GetValue(this),
                    SetValue = (o) =>
                    {
                        x.SetValue(this, o);
                    },
                    GetText = () => FTools.L10N(x.Name.ToUpper(), prefix:"FA_SETTING"),
                    GetDescription = () => FTools.L10N(x.Name.ToUpper(), prefix:"FA_SETTING", suffix:"DESC")
                });

                FacilityAdjustmentsLogger.Info($"Successfully detected [{fields.Count()}] mod settings");
                _internalConfigFields.AddRange(fields);
                string configFilePathRoot = GameUtl.GameComponent<PlatformComponent>().Platform.GetPlatformData().GetFilePathRoot();
                string path = System.IO.Path.Combine(configFilePathRoot, "modconfig.json");
                if (false == System.IO.File.Exists(path))
                {
                    FacilityAdjustmentsLogger.Info("Failed to find modconfig");
                    return;
                }
                var modConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, ModRawConfig>>(System.IO.File.ReadAllText(path));
                if (modConfigs != null && modConfigs.ContainsKey("com.flylake.facilityadjustments"))
                {
                    ModRawConfig rawFAConfig = modConfigs["com.flylake.facilityadjustments"];
                    if (rawFAConfig != null && ((Dictionary<string, object>)(object)rawFAConfig).Count == _internalConfigFields.Count)
                    {
                        base.LoadFromRawConfig(rawFAConfig);
                        FacilityAdjustmentsLogger.Info("Loaded raw config");
                    }
                }
            }
            catch (Exception e)
            {
                FacilityAdjustmentsLogger.Error(e);
                throw;
            }
        }
        private List<ModConfigField> _internalConfigFields { get; set; } = new List<ModConfigField>();
        public override List<ModConfigField> GetConfigFields()
        {
            return _internalConfigFields;
        }

    }
}

