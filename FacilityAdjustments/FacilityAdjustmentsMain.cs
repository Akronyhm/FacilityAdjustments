using Base.Levels;
using PhoenixPoint.Common.Game;
using PhoenixPoint.Modding;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FacilityAdjustments
{
	/// <summary>
	/// This is the main mod class. Only one can exist per assembly.
	/// If no ModMain is detected in assembly, then no other classes/callbacks will be called.
	/// </summary>
	public class FacilityAdjustmentsMain : ModMain
	{
		/// Config is accessible at any time, if any is declared.
		public new FacilityAdjustmentsConfig Config => this.Instance.Config as FacilityAdjustmentsConfig;
		public static FacilityAdjustmentsMain Main { get; private set; }

		public static string ModDirectory { get; set; }
		public static string LocalizationDirectory { get; set; }
		public static string LogPath;

		/// This property indicates if mod can be Safely Disabled from the game.
		/// Safely sisabled mods can be reenabled again. Unsafely disabled mods will need game restart ot take effect.
		/// Unsafely disabled mods usually cannot revert thier changes in OnModDisabled
		public override bool CanSafelyDisable => false;

		/// <summary>
		/// Callback for when mod is enabled. Called even on game starup.
		/// </summary>
		public override void OnModEnabled() {

			try
			{
				Main = this;
				int c = Dependencies.Count();
				string v = MetaData.Version.ToString();
				HarmonyLib.Harmony harmony = (HarmonyLib.Harmony)HarmonyInstance;
				string id = Instance.ID;
				GameObject go = ModGO;
				PhoenixGame game = GetGame();
				ModDirectory = Instance.Entry.Directory;
				LocalizationDirectory = System.IO.Path.Combine(ModDirectory, "Resource", "Localization");
				LogPath = System.IO.Path.Combine(ModDirectory, "FALog.log");
				FacilityAdjustmentsLogger.Initialize(LogPath, "FA", ModDirectory);
				FacilityAdjustmentsLogger.Info("Logger Initialized");
				FTools.LoadLocalization();

				if (Main != null)
				{
					FacilityAdjustmentsLogger.Info("Attempting to construct configuration fields");
					Config.ConstructConfigurationFields();
				}
				else
				{
					Logger.LogWarning("Unable to construct mod configuration fields");
					FacilityAdjustmentsLogger.Info("Unable to construct mod configuration fields");
				}
				/// Apply any general game modifications.
				FacilityAdjustmentsLogger.Info("Applying harmony patches");
				harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
				var harmonyPatches = harmony.GetPatchedMethods();
				FacilityAdjustmentsLogger.Info($"Patched [{harmonyPatches.Count()}] methods");
				FacilityEconomy.Apply();
				FacilityAdjustmentsLogger.Info("Mod initialization done.");


			}
			catch (Exception e)
			{
				FacilityAdjustmentsLogger.Error(e);
			}
		}

		/// <summary>
		/// Callback for when mod is disabled. This will be called even if mod cannot be safely disabled.
		/// Guaranteed to have OnModEnabled before.
		/// </summary>
		public override void OnModDisabled() {
			/// Undo any game modifications if possible. Else "CanSafelyDisable" must be set to false.
			/// ModGO will be destroyed after OnModDisabled.
		}

		/// <summary>
		/// Callback for when any property from mod's config is changed.
		/// </summary>
		public override void OnConfigChanged() {
			/// Config is accessible at any time.
		}


		/// <summary>
		/// In Phoenix Point there can be only one active level at a time. 
		/// Levels go through different states (loading, unloaded, start, etc.).
		/// General puprose level state change callback.
		/// </summary>
		/// <param name="level">Level being changed.</param>
		/// <param name="prevState">Old state of the level.</param>
		/// <param name="state">New state of the level.</param>
		public override void OnLevelStateChanged(Level level, Level.State prevState, Level.State state) {
			/// Alternative way to access current level at any time.
			Level l = GetLevel();
		}

		/// <summary>
		/// Useful callback for when level is loaded, ready, and starts.
		/// Usually game setup is executed.
		/// </summary>
		/// <param name="level">Level that starts.</param>
		public override void OnLevelStart(Level level) {
		}

		/// <summary>
		/// Useful callback for when level is ending, before unloading.
		/// Usually game cleanup is executed.
		/// </summary>
		/// <param name="level">Level that ends.</param>
		public override void OnLevelEnd(Level level) {
            if (level.name.Contains("HomeScreen"))
            {
                FacilityAdjustmentsLogger.Info($"{MethodBase.GetCurrentMethod().Name} called for level '{level}'; harmony re-patching everything in case config changed");
				HarmonyLib.Harmony harmony = (HarmonyLib.Harmony)HarmonyInstance;
				harmony.UnpatchAll(harmony.Id);
				harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
				FacilityEconomy.Apply();
            }
        }
	}
}
