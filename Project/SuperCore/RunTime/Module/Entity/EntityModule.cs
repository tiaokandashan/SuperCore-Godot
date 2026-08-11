namespace SuperCore.RunTime;

[Module(2000)]
public sealed partial class EntityModule : Module<EntityModule>
{
	protected override void OnInit()
	{
		CompleteInit();
	}
}
