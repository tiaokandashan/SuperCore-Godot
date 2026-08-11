namespace SuperCore.RunTime;

[Module(-10000)]
public sealed partial class DebugModule : Module<DebugModule>
{
	protected override void OnInit()
	{
		CompleteInit();
	}
}
