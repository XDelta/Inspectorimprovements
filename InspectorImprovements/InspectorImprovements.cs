using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

using ResoniteModLoader;

namespace InspectorImprovements;

public class InspectorImprovements : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.3";
	public override string Name => "InspectorImprovements";
	public override string Author => "Delta";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/XDelta/Inspectorimprovements";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> collapseComponents = new("collapseComponents", "Enable the collapse functionality on components", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> defaultExpanded = new("defaultExpanded", "Expanded components by default", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<long> collaspeOrderOffset = new("collaspeOrderOffset", "Collaspe button OrderOffset", () => -1);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<CollapseStyle> collapseStyle = new("collapseStyle", "How should collapsing components be styled (button with icon or full header)", () => CollapseStyle.Line);
	internal static ModConfiguration? Config;

	public override void OnEngineInit() {
		Config = GetConfiguration()!;
		Config.Save(true);

		Harmony harmony = new("net.deltawolf.InspectorImprovements");
		harmony.PatchAll();
	}

	enum CollapseStyle {
		Line,         // |--|
		Header       // Use the title header instead of the separate button.
	}
	[HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
	public class CollapseButtonPatch {
		//also skip on allowContainer, this is for when a component is opened in an inspector by itself. No point showing or generating there.
		public static void Postfix(Worker worker, WorkerInspector __instance) {
			if (!collapseComponents.Value) { return; }
			if (worker is Slot) { return; }
			AddCollapseToggle(worker, __instance);
		}

		internal static void AddCollapseToggle(Worker worker, WorkerInspector wi) {
			var recentComponent = wi.Slot.Children.Last();
			var headerSlot = recentComponent.Children.First();
			var expanderContent = recentComponent.AddSlot("ExpanderContent");
			var ui = new UIBuilder(headerSlot);
			expanderContent.AttachComponent<VerticalLayout>().Spacing.Value = 4;
			foreach (var child in recentComponent.Children.Skip(1).ToList()) {
				child.Parent = expanderContent;
			}
			var mainHeaderSlot = headerSlot.Children.First(s => s.GetComponent<ReferenceProxySource>() != null);
			var mainHeaderButton = mainHeaderSlot.GetComponent<Button>();

			RadiantUI_Constants.SetupEditorStyle(ui);
			ui.Style.FlexibleWidth = 0f;
			ui.Style.MinWidth = 40f;
			Button? expBtn;
			switch (collapseStyle.Value) {
				case CollapseStyle.Line:
					expBtn = ui.Button(OfficialAssets.Graphics.Icons.Tool.PerpendicularRay);
					expBtn.Slot.OrderOffset = collaspeOrderOffset.Value;
					break;
				case CollapseStyle.Header:
					expBtn = mainHeaderButton;
					break;
				default:
					expBtn = ui.Button(OfficialAssets.Graphics.Icons.Tool.PerpendicularRay);
					expBtn.Slot.OrderOffset = collaspeOrderOffset.Value;
					break;
			}
			Expander exp = expBtn.Slot.AttachComponent<Expander>();
			exp.SectionRoot.Target = expanderContent;
			exp.IsExpanded = defaultExpanded.Value;
		}
	}


}
