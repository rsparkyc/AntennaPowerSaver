using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntennaPowerSaver
{
	public class ModuleAntennaPowerSaver : PartModule
	{
		private PartModule rtAntenna = null;
		private Type rtAntennaType = null;
		public void Update()
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


			if (HighLogic.LoadedSceneIsFlight) {
				double amount = 1;
				double maxAmount = 1;
				if (vessel != null) {
					vessel.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out amount, out maxAmount);
					//Log("EC: " + amount + "/" + maxAmount);
				}
				double percentage = amount / maxAmount;

				if (percentage < 0.95) {
					Log("Disabling Antenna");
					rtAntennaType.GetMethod("SetState").Invoke(rtAntenna, new object[] { false });
				}
				if (percentage > 0.99) {
					Log("Enabling Antenna");
					rtAntennaType.GetMethod("SetState").Invoke(rtAntenna, new object[] { true });
				}
			}
		}

		private static void Log(string message)
		{
			UnityEngine.Debug.Log("[APS] - " + message);
		}
	}
}
