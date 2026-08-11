namespace SuperCore.RunTime;

[Module(-20000)]
public sealed partial class ResModule : Module<ResModule>
{
	protected override void OnInit()
	{
		CompleteInit();
	}
}
