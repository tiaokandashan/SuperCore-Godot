namespace SuperCore.RunTime;

[Module(-8000)]
public sealed partial class HotUpdateModule : Module<HotUpdateModule>
{
	protected override void OnInit()
	{
		CompleteInit();
	}
}
