using System.Runtime.InteropServices.JavaScript;
using System.Collections.Generic;

namespace NetworkOptimisationAlgorithm.Dijkstra;

public class DijkstraAlgorithm
{
    private int [,] _weightMatrix;

    private List<int> _weightArray;
    private List<int> _tracebackArray;
    private Dictionary<int, bool> _markedNodes;
    
    public List<int> WeightArray => _weightArray;
    public List<int> TracebackArray => _tracebackArray;
    public Dictionary<int, bool> MarkedNodes => _markedNodes;

    public DijkstraAlgorithm(int[,] weightMatrix)
    {
        var nodesCount = weightMatrix.GetLength(0);
        
        _markedNodes = new Dictionary<int, bool>();
        _weightArray = new List<int>();
        _tracebackArray = new List<int>();
        _weightMatrix = new int[nodesCount, nodesCount];
        
        // Here negative values are being changed into max values and written into _weightMatrix
        // Also all extra arrays are being initialized with default values
        for (var i = 0; i < nodesCount; i++)
        {
            _weightArray.Add(int.MaxValue);
            _tracebackArray.Add(-1);
            
            for (var j = 0; j < nodesCount; j++)
            {
                _weightMatrix[i, j] = weightMatrix[i, j] < 0 ? int.MaxValue : weightMatrix[i, j];
            }
        }
    }

    public void Solve()
    {
        var bSolved = false;

        while (!bSolved)
        {
            // Initial mark on first node
            CalculatePath();
            bSolved = IsSolved();
            DijkstraLogger.OutCalculationTable(_weightArray, _tracebackArray, _markedNodes);
        }
    }

    private void CalculatePath()
    {
        // Get min path value of unmarked elements
        var min = int.MaxValue;
        var minIndex = -1;
        for (var i = 0; i < _weightArray.Count; i++)
        {
            _markedNodes.TryGetValue(i, out var value);
            if (_weightArray[i] != int.MaxValue && !value)
            {
                if (min <= _weightArray[i]) continue;
                min = _weightArray[i];
                minIndex = i;
            }
        }
        
        if (_markedNodes.Count == 0)
        {
            _markedNodes.Add(0, false);
            _weightArray[0] = 0;
            _tracebackArray[0] = -1;
            minIndex = 0;
        }
        else
        {
            _markedNodes.Add(minIndex, false);
        }

        for (var j = 0; j < _weightArray.Count; j++)
        {
            var rowWeight = Dijkstra(minIndex, j);
            if (rowWeight != _weightArray[j])
            {
                _tracebackArray[j] = minIndex;
            }
            _weightArray[j] = rowWeight;
        }
        
        _markedNodes[minIndex] = true;
    }

    private int Dijkstra(int minIndex, int targetIndex)
    {
        var pathWeight = _weightMatrix[minIndex, targetIndex];
        if (pathWeight == int.MaxValue)
        {
            return Math.Min(_weightArray[targetIndex], pathWeight);
        }
        return Math.Min(_weightArray[targetIndex], _weightArray[minIndex] + _weightMatrix[minIndex, targetIndex]);
    }

    private bool IsSolved()
    {
        if(_markedNodes.Count < _weightArray.Count) return false;
        
        for (var i = 0; i < _markedNodes.Count; i++)
        {
            if(_markedNodes[i] == false) return false;
        }

        return true;
    }
}