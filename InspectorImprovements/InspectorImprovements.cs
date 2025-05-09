using FrooxEngine.UIX;
using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;
using System.Linq;

namespace InspectorImprovements;

public class InspectorImprovements : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0";
	public override string Name => "InspectorImprovements";
	public override string Author => "Delta";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/XDelta/Inspectorimprovements";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> enabled = new("enabled", "", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> collapseComponents = new("collapseComponents", "Collapse button on components", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> defaultExpanded = new("defaultExpanded", "Expanded by default", () => true);

	internal static ModConfiguration Config;

	public override void OnEngineInit() {
		Config = GetConfiguration()!;
		Config.Save(true);

		Harmony harmony = new("net.deltawolf.InspectorImprovements");
		harmony.PatchAll();
	}

	[HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
	public class CollapseButtonPatch {
		public static void Postfix(Worker worker, WorkerInspector __instance) {
			if (!Config.GetValue(collapseComponents)) return;
			if (worker is Slot) return;
			AddCollapseToggle(worker, __instance);
		}
	}
	private static void AddCollapseToggle(Worker worker, WorkerInspector wi) {
		var recentComponent = wi.Slot.Children.Last();
		var headerSlot = recentComponent.Children.First();

		var expanderContent = recentComponent.AddSlot("ExpanderContent");
		var ui = new UIBuilder(headerSlot);
		expanderContent.AttachComponent<VerticalLayout>().Spacing.Value = 4;
		foreach (var child in recentComponent.Children.Skip(1).ToList()) {
			child.Parent = expanderContent;
		}
		
		RadiantUI_Constants.SetupEditorStyle(ui);
		ui.Style.FlexibleWidth = 0f;
		ui.Style.MinWidth = 40f;


		var button = ui.Button(OfficialAssets.Graphics.Icons.Tool.PerpendicularRay);
		button.Slot.OrderOffset = -1;
		Expander exp = button.Slot.AttachComponent<Expander>();
		exp.SectionRoot.Target = expanderContent;
		exp.IsExpanded = Config.GetValue(defaultExpanded);
	}
}
