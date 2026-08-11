namespace SuperCore.RunTime;

[Module(10000)]
public sealed partial class ProcedureModule : Module<ProcedureModule>
{
	protected override void OnInit()
	{
		CompleteInit();
	}
}
