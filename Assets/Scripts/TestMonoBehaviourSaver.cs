using Extensions.Saver;

public class TestMonoBehaviourSaver : SaverMonoBehaviour<TestData>
{
	protected override string SaverPath => "TestDatabase";
	protected override string SaverFileName => "testMonoBehaviour.data";
}