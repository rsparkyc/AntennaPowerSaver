using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntennaPowerSaver
{
	public class ModuleAntennaPowerSaver : PartModule
	{

		[KSPField(isPersistant = true, guiName = "Auto Power Save", guiActive = true, guiActiveEditor = false),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool autoPowerSaveActive;

		[KSPField(isPersistant = true, guiName = "Disable at", guiActive = true, guiActiveEditor = false, guiUnits = "%"),
			UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
		public float disableThreshold = 20;

		[KSPField(isPersistant = true, guiName = "Enable at", guiActive = true, guiActiveEditor = false, guiUnits = "%"),
			UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
		public float enableThreshold = 80;

		private PartModule rtAntenna = null;
		private Type rtAntennaType = null;
		public void Update()
		{
			ValidateThreshold();

			ReflectAntennaModule();

			if (HighLogic.LoadedSceneIsFlight) {
				SetPropperAntennaState();
			}
		}

		private UI_FloatRange disableFloatRange;
		private UI_FloatRange enableFloatRange;

		private void ValidateThreshold()
		{
			// Make sure that the threshold to disable an antenna is less than the threshold to reenable it
			if (enableThreshold < disableThreshold) {
				enableThreshold = disableThreshold;
			}
			if (disableFloatRange == null) {
				disableFloatRange = (UI_FloatRange)Fields["disableThreshold"].uiControlEditor;
			}

			if (enableFloatRange == null) {
				enableFloatRange = (UI_FloatRange)Fields["enableThreshold"].uiControlEditor;
			}

			enableFloatRange.minValue = disableThreshold;
			disableFloatRange.maxValue = enableThreshold;
		}

		private void SetPropperAntennaState()
		{
			if (autoPowerSaveActive) {
				double amount = 1;
				double maxAmount = 1;
				if (vessel != null) {
					vessel.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out amount, out maxAmount);
				}
				double percentage = amount / maxAmount;

				if (rtAntennaType != null) {
					bool isEnabled = (bool)rtAntennaType.GetProperty("Activated").GetGetMethod().Invoke(rtAntenna, null);
					if (isEnabled && percentage <= (disableThreshold / 100)) {
						Log("Disabling Antenna");
						rtAntennaType.GetMethod("SetState").Invoke(rtAntenna, new object[] { false });
						Log("Disabled");
					}
					if (!isEnabled && percentage >= (enableThreshold / 100)) {
						Log("Enabling Antenna");
						rtAntennaType.GetMethod("SetState").Invoke(rtAntenna, new object[] { true });
						Log("Enabled");
					}
				}
			}
		}

		private void ReflectAntennaModule()
		{
			if (part != null && rtAntenna == null) {
				foreach (var module in part.Modules) {
					Type moduleType = module.GetType();
					if (moduleType.FullName == "RemoteTech.Modules.ModuleRTAntenna") {
						rtAntenna = module;
						rtAntennaType = moduleType;
					}
				}
			}
		}

		private static void Log(string message)
		{
			UnityEngine.Debug.Log("[APS] - " + message);
		}
	}
}
