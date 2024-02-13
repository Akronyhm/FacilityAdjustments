using Base.UI;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityAdjustments
{
    public static class FTools
    {
		public static string AssemblyName = "akronyhm.facilityadjustments";
		static string LocalizationFileName = "L10N.csv";
		public static void LoadLocalization()
		{
			try
			{
				FacilityAdjustmentsLogger.Info("Loading localizations");
				var modDir = FacilityAdjustmentsMain.ModDirectory;
				var locDir = FacilityAdjustmentsMain.LocalizationDirectory;
				FacilityAdjustmentsLogger.Info($"localization folder [{locDir}]");
				var localizationFile = new System.IO.FileInfo(System.IO.Path.Combine(locDir, LocalizationFileName));
				if (localizationFile.Exists)
				{
					FacilityAdjustmentsLogger.Info($"Importing localization csv [{localizationFile.FullName}]");
					ImportLocalizationFromFile(localizationFile.FullName);
				}
			}
			catch (Exception e)
			{
				FacilityAdjustmentsLogger.Error(e);
			}
		}

		private static void ImportLocalizationFromFile(System.IO.FileInfo fileInfo, string category = null)
		{
            try
            {
				if (false == fileInfo.Exists)
					return;

				var l10nData = System.IO.File.ReadAllText(fileInfo.FullName);
				if (false == l10nData.EndsWith("\n"))
					l10nData += "\n";
				LanguageSourceData source = (category == null) ? LocalizationManager.Sources[0] : LocalizationManager.Sources.First((LanguageSourceData src) => src.GetCategories(false, null).Contains(category));
				if (source != null)
				{
					int preCount = source.mTerms.Count;
					source.Import_CSV(string.Empty, l10nData, eSpreadsheetUpdateMode.AddNewTerms, ',');
					LocalizationManager.LocalizeAll(true);
					int afterCount = source.mTerms.Count;
					FacilityAdjustmentsLogger.Info($"Added {afterCount - preCount} new localization entries");
				}
				else
				{
					FacilityAdjustmentsLogger.Info($"Localization category not found [{category}]");
				}
            }
            catch (Exception e)
            {
                FacilityAdjustmentsLogger.Error(e);
            }
        }

		public static string L10N(string key, string prefix = null, string suffix = null)
		{
			try
			{
				var combined = $"{(false == string.IsNullOrEmpty(prefix) ? $"{prefix}_" : "")}{key}{(false == string.IsNullOrEmpty(suffix) ? $"_{suffix}" : "")}";
				return new LocalizedTextBind(combined).Localize(null);
			}
			catch (Exception e)
			{
				FacilityAdjustmentsLogger.Error(e);
				return key;
			}
		}

		private static void ImportLocalizationFromFile(string localizationFileName, string category = null)
        {
			ImportLocalizationFromFile(new System.IO.FileInfo(localizationFileName), category);
        }

        public static void OutputHarmonyPatchInfo(string assemblyFilter = null)
        {
			FacilityAdjustmentsLogger.Info($"======================================================================");
			FacilityAdjustmentsLogger.Info($"=========================== HARMONY REPORT ===========================");
			FacilityAdjustmentsLogger.Info($"======================================================================");
			var activePatches = HarmonyLib.Harmony.GetAllPatchedMethods().ToList();
			FacilityAdjustmentsLogger.Info($"There are [{activePatches.Count()}] active harmony patches");
			foreach (var patch in activePatches)
			{
				var patchInfos = HarmonyLib.Harmony.GetPatchInfo(patch);
				if (false == string.IsNullOrWhiteSpace(assemblyFilter))
				{
					if (patchInfos.Prefixes.Any(x => x.PatchMethod.DeclaringType.FullName.StartsWith(assemblyFilter))
						|| patchInfos.Postfixes.Any(x => x.PatchMethod.DeclaringType.FullName.StartsWith(assemblyFilter))
						|| patchInfos.Finalizers.Any(x => x.PatchMethod.DeclaringType.FullName.StartsWith(assemblyFilter))
						|| patchInfos.Transpilers.Any(x => x.PatchMethod.DeclaringType.FullName.StartsWith(assemblyFilter)))
					{
						FacilityAdjustmentsLogger.Info($"A harmony patch for [{patch.DeclaringType.Name}.{patch.Name}] is active");
					}
					else
					{
						continue;
					}
				}
				else
                {
					FacilityAdjustmentsLogger.Info($"A harmony patch for [{patch.DeclaringType.Name}.{patch.Name}] is active");
				}
				
				if (patchInfos.Prefixes.Count > 0)
				{
					FacilityAdjustmentsLogger.Info("   === PRE FIXES ===");
				}
				foreach (var prefix in patchInfos.Prefixes)
				{
					FacilityAdjustmentsLogger.Info($"   Pre Fix [{prefix.PatchMethod.DeclaringType.FullName}.{prefix.PatchMethod.Name}]");
				}

				if (patchInfos.Postfixes.Count > 0)
				{
					FacilityAdjustmentsLogger.Info("   === POST FIXES ===");
				}
				foreach (var postFix in patchInfos.Postfixes)
				{
					FacilityAdjustmentsLogger.Info($"   Post Fix [{postFix.PatchMethod.DeclaringType.FullName}.{postFix.PatchMethod.Name}]");
				}

				if (patchInfos.Finalizers.Count > 0)
				{
					FacilityAdjustmentsLogger.Info("   === FINALIZERS ===");
				}
				foreach (var finalizer in patchInfos.Finalizers)
				{
					FacilityAdjustmentsLogger.Info($"   Finalizer [{finalizer.PatchMethod.DeclaringType.FullName}.{finalizer.PatchMethod.Name}]");
				}

				if (patchInfos.Transpilers.Count > 0)
				{
					FacilityAdjustmentsLogger.Info("   === TRANSPILERS ===");
				}
				foreach (var transpiler in patchInfos.Transpilers)
				{
					FacilityAdjustmentsLogger.Info($"   Transpilers [{transpiler.PatchMethod.DeclaringType.FullName}.{transpiler.PatchMethod.Name}]");
				}
			}
			FacilityAdjustmentsLogger.Info($"======================================================================");
			FacilityAdjustmentsLogger.Info($"============================= END REPORT =============================");
			FacilityAdjustmentsLogger.Info($"======================================================================");
		}
    }
}
