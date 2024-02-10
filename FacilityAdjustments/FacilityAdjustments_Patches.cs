using Base.Core;
using Base.Defs;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities;
using PhoenixPoint.Geoscape.Entities.PhoenixBases;
using PhoenixPoint.Geoscape.Entities.PhoenixBases.FacilityComponents;
using PhoenixPoint.Geoscape.Entities.Sites;
using PhoenixPoint.Geoscape.Levels.Factions;
using PhoenixPoint.Geoscape.View.ViewControllers.PhoenixBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityAdjustments
{
    public static class FacilityEconomy
    {
        public static void Apply()
        {
            FacilityAdjustmentsLogger.Info("Applying FacilityEconomy");
            var defRepo = GameUtl.GameComponent<DefRepository>();
            var resGenFacilities = defRepo.DefRepositoryDef.AllDefs.OfType<ResourceGeneratorFacilityComponentDef>().ToList();
            FacilityAdjustmentsLogger.Info($"Updating resource outputs for [{resGenFacilities.Count}] facility defs");
            var fab = resGenFacilities.Where(x => x.name.Contains("FabricationPlant"));
            foreach (var building in fab)
            {
                ResourceUnit newOutput = new ResourceUnit
                {
                    Type = ResourceType.Materials,
                    Value = FacilityAdjustmentsMain.Main.Config.MaterialProductionPerFacility / 24f
                };
                building.BaseResourcesOutput.Set(newOutput);
                FacilityAdjustmentsLogger.Info($"Updating facility [{building.name}] with resource-per-hour value [{newOutput.Value}]");
            }

            var Lab = resGenFacilities.Where(x => x.name.Contains("ResearchLab"));
            foreach (var building in Lab)
            {
                ResourceUnit newOutput = new ResourceUnit
                {
                    Type = ResourceType.Tech,
                    Value = FacilityAdjustmentsMain.Main.Config.TechProductionPerFacility / 24f
                };
                building.BaseResourcesOutput.Set(newOutput);

                FacilityAdjustmentsLogger.Info($"Updating facility [{building.name}] with resource-per-hour value [{newOutput.Value}]");
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(ResourceGeneratorFacilityComponent), "UpdateOutput")]
    public static class ResourceGeneratorFacilityComponent_UpdateOutput_Patch
    {
        [HarmonyLib.HarmonyPostfix]
        private static void Postfix(ResourceGeneratorFacilityComponent __instance)
        {
            try
            {
                float value = 0;
                if (__instance.Facility.PxBase.Site.Owner is GeoPhoenixFaction)
                {
                    __instance.Facility.PxBase.Site.Owner.Name.Localize(null);
                    __instance.Facility.ViewElementDef.DisplayName1.Localize(null);
                    __instance.Facility.FacilityId.ToString();

                    if (__instance.Def.name.Contains("FabricationPlant") && FacilityAdjustmentsMain.Main.Config.MaterialProductionPerFacility > 0f)
                    {
                        value = FacilityAdjustmentsMain.Main.Config.MaterialProductionPerFacility / 24f;
                        var resUnit = new ResourceUnit(ResourceType.Materials, value);
                        __instance.ResourceOutput.AddUnique(resUnit);
                    }
                    else if (__instance.Def.name.Contains("ResearchLab") && FacilityAdjustmentsMain.Main.Config.TechProductionPerFacility > 0f)
                    {
                        value = FacilityAdjustmentsMain.Main.Config.TechProductionPerFacility / 24f;
                        var resUnit = new ResourceUnit(ResourceType.Tech, value);
                        __instance.ResourceOutput.AddUnique(resUnit);
                    }
                    FacilityAdjustmentsLogger.Info($"Updating output for Facility [{__instance.Facility.Def.name}, Output : {value}] at base [{__instance.Facility.PxBase.Site.Name}, {__instance.Facility.PxBase.Site.SiteId}]");
                }
            }
            catch (Exception e)
            {
                FacilityAdjustmentsLogger.Error(e);
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(UIFacilityTooltip), "Show")]
    public static class UIFacilityTooltip_Show_Patch
    {
        [HarmonyLib.HarmonyPostfix]
        private static void Postfix(UIFacilityTooltip __instance, PhoenixFacilityDef facility, GeoPhoenixBase currentBase)
        {
            try
            {
                if (currentBase == null)
                    return;

                if (facility.name.Contains("FabricationPlant"))
                {
                    float matOutputSetting = FacilityAdjustmentsMain.Main.Config.MaterialProductionPerFacility;
                    if (matOutputSetting > 0f)
                    {
                        var expandedDesc = string.Format(FTools.L10N("MATERIALPRODUCTIONPERFACILITY", "FA_DESC", "FORMATTED"), matOutputSetting.ToString("0.#"));
                        var facDesc = $"{__instance.Description.text}\n{expandedDesc}";
                        __instance.Description.text = facDesc;
                    }
                }
                else if (facility.name.Contains("ResearchLab"))
                {
                    float techOutputSetting = FacilityAdjustmentsMain.Main.Config.TechProductionPerFacility;
                    if (techOutputSetting > 0f)
                    {
                        var expandedDesc = string.Format(FTools.L10N("TECHPRODUCTIONPERFACILITY", "FA_DESC", "FORMATTED"), techOutputSetting.ToString("0.#"));
                        var facDesc = $"{__instance.Description.text}\n{expandedDesc}";
                        __instance.Description.text = facDesc;
                    }
                }
            }
            catch (Exception e)
            {
                FacilityAdjustmentsLogger.Error(e);
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(UIFacilityInfoPopup), "Show")]
    public static class UIFacilityInfoPopup_Show_Patch
    {
        [HarmonyLib.HarmonyPostfix]
        private static void Postfix(UIFacilityInfoPopup __instance, GeoPhoenixFacility facility)
        {
            try
            {
                if (facility.Def.name.Contains("FabricationPlant"))
                {
                    float matOutputSetting = FacilityAdjustmentsMain.Main.Config.MaterialProductionPerFacility;
                    if (matOutputSetting > 0f)
                    {
                        var facDesc = $"{__instance.Description.text}\nEvery plant produces {matOutputSetting:0.#} materials per day";
                        __instance.Description.text = facDesc;
                    }
                }
                else if (facility.Def.name.Contains("ResearchLab"))
                {
                    float techOutputSetting = FacilityAdjustmentsMain.Main.Config.TechProductionPerFacility;
                    if (techOutputSetting > 0f)
                    {
                        var facDesc = $"{__instance.Description.text}\nEvery lab produces {techOutputSetting:0.#} tech per day";
                        __instance.Description.text = facDesc;
                    }
                }
            }
            catch (Exception e)
            {
                FacilityAdjustmentsLogger.Error(e);
            }
        }
    }

}
