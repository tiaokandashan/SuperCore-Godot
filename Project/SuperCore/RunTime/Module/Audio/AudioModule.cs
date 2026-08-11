namespace SuperCore.RunTime;

[Module(1000)]
public sealed partial class AudioModule : Module<AudioModule>
{
	protected override void OnInit()
	{
		CompleteInit();
	}
}
