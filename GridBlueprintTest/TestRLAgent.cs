using GridBlueprint.Model;
using Mars.Interfaces.Environments;

namespace GridBlueprintTest;

public class TestRLAgent
{
    private RLAgent agent;

    /*
     * Setup is called before each test-function.
     */
    [SetUp]
    public void Setup()
    {
        agent = new RLAgent();
        // TODO: maybe call init here?
    }


    [Test]
    public void GetQTest()
    {
        // TODO: it is the same: need to be tested with "filled Q"
        RLAgent agent = new RLAgent();
        Dictionary<(Position state, int action), float> q = agent.GetQ();
        Dictionary<(Position state, int action), float> expectedQ =
            new Dictionary<(Position state, int action), float>();

        CollectionAssert.AreEqual(expectedQ, q);
    }

    [Test]
    public void LoadQTableFromCsvTest()
    {
        string filePath = "Resources/testing_q_table.csv";
        var firstLine = "1;1;0;2,3";
        var lastLine = "1;5;4;2,4";

        File.WriteAllLines(filePath, new string[] { firstLine, lastLine });

        agent.LoadQTableFromCsv(filePath);
        var q = agent.GetQ();
        var firstKey = q.Keys.First();
        var lastKey = q.Keys.Last();
        var firstLineFromQ = firstKey.state.X + ";" + firstKey.state.Y + ";" + firstKey.action + ";" + q[firstKey];
        var lastLineFromQ = lastKey.state.X + ";" + lastKey.state.Y + ";" + lastKey.action + ";" + q[lastKey];

        // Assert
        Assert.That(firstLineFromQ, Is.EqualTo(firstLine));
        Assert.That(lastLineFromQ, Is.EqualTo(lastLine));
    }

    [Test]
    public void ExportQTableTest()
    {
        string filePath = "";
        agent.ExportQTable(filePath);

        var firstLine = "";
        var lastLine = "";
    }
}