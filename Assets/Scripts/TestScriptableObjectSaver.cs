using UnityEngine;
using Extensions.Saver;

[CreateAssetMenu(menuName = "Example (Saver)/Test Scriptable Object Saver", fileName = "Test Scriptable Object Saver", order = 0)]
public class TestScriptableObjectSaver : SaverScriptableObject<TestData>
{
    protected override string SaverPath => "TestDatabase";
    protected override string SaverFileName => "testScriptableObject.data";

    protected override void OnDataLoaded(TestData data)
    {
        
    }
}