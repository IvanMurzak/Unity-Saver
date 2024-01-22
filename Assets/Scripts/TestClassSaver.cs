using System;
using System.Threading.Tasks;
using Extensions.Saver;

public class TestClassSaver
{
    public Saver<TestData> saver;

    // Should be called from main thread, in Awake or Start method for example
    public void Init()
    {
        saver = new Saver<TestData>("TestDatabase", "testClass.data", new TestData());
    }

    public TestData Load() => saver?.Load();

    public async Task Save(Action onComplete = null) => await saver?.Save(onComplete);
}
