namespace NetworkOptimisationAlgorithm.Dijkstra;

public static class DijkstraLogger
{
    public static void OutCalculationTable(List<int> weightArray, List<int> tracebackArray, Dictionary<int, bool> markedNodes)
    {
        var nodeOutStr = "";
        for (var i = 0; i < weightArray.Count; i++)
        {
            nodeOutStr += ((char)('A' + i)).ToString() + "\t";
        }
        Console.WriteLine(nodeOutStr);

        var weightArrOutStr = "";
        foreach (var weight in weightArray)
        {
            weightArrOutStr += weight == int.MaxValue ? "inf\t" : weight + "\t";
        }
        Console.WriteLine(weightArrOutStr);

        var tracebackArrOutStr = "";
        foreach (var traceback in tracebackArray)
        {
            tracebackArrOutStr += traceback == -1 ? "-\t" : (char)('A' + traceback) + "\t";
        }
        Console.WriteLine(tracebackArrOutStr);
        
        var marckedNodesArrOutStr = "";
        for (var i = 0; i < weightArray.Count; ++i)
        {
            markedNodes.TryGetValue(i, out var value);
            marckedNodesArrOutStr += value + "\t";
        }
        Console.WriteLine(marckedNodesArrOutStr);
        Console.WriteLine();
    }
}