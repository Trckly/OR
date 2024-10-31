namespace NetworkOptimisationAlgorithm.Floyd;

public class FloydAlgorithm
{
    private int [,] _shortestPathMatrix;
    private int[,] _routeMatrix;
    
    public int [,] ShortestPathMatrix => _shortestPathMatrix;
    public int[,] RouteMatrix => _routeMatrix;
    
    public FloydAlgorithm(int[,] weightMatrix)
    {
        var nodesCount = weightMatrix.GetLength(0);
        
        _shortestPathMatrix = new int[nodesCount, nodesCount];
        _routeMatrix = new int[nodesCount, nodesCount];
        
        // Here negative values are being changed into max values and written into _shortestPathMatrix
        // _routeMatrix is being populated with column indexes. Same node path represents as -1 for convenience.
        for (var i = 0; i < nodesCount; i++)
        {
            for (var j = 0; j < nodesCount; j++)
            {
                _shortestPathMatrix[i, j] = weightMatrix[i, j] < 0 ? int.MaxValue : weightMatrix[i, j];
                if (_shortestPathMatrix[i, j] != int.MaxValue)
                    _routeMatrix[i, j] = j;
            }
        }
    }

    public void Solve()
    {
        FloydLogger.OutFloydMatrices(_shortestPathMatrix, _routeMatrix, 0);

        for (var i = 0; i < _shortestPathMatrix.GetLength(0); i++)
        {
            Floyd(i);
            FloydLogger.OutFloydMatrices(_shortestPathMatrix, _routeMatrix, i);
        }
    }

    private void Floyd(int nodeIndex)
    {
        var nodesCount = _shortestPathMatrix.GetLength(0);

        for (var i = 0; i < nodesCount; i++)
        {
            if(i == nodeIndex) continue;

            var nodeRowValue = _shortestPathMatrix[i, nodeIndex];
            if(nodeRowValue == int.MaxValue) continue;

            for (var j = 0; j < nodesCount; j++)
            {
                if (i == j) continue;

                if (j == nodeIndex) continue;

                var nodeColumnValue = _shortestPathMatrix[nodeIndex, j];
                if (nodeColumnValue == int.MaxValue) continue;

                if (_shortestPathMatrix[i, nodeIndex] + _shortestPathMatrix[nodeIndex, j] < _shortestPathMatrix[i, j])
                {
                    _shortestPathMatrix[i, j] = _shortestPathMatrix[i, nodeIndex] + _shortestPathMatrix[nodeIndex, j];
                    _routeMatrix[i, j] = _routeMatrix[i, nodeIndex];
                }
            }
        }
    }
}