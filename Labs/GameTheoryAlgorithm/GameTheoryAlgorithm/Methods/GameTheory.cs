using Lab01_OR;

namespace GameTheoryAlgorithm.Methods;

public class GameTheory
{
    private decimal[,] _gameMatrix;
    private MainWindow _mainWindow;
    public GameTheory(decimal[,] gameMatrix, MainWindow mainWindow)
    {
        _gameMatrix = gameMatrix;
        _mainWindow = mainWindow;
    }

    public (List<decimal>, List<decimal>, decimal) Solve()
    {
        var (aStrategy, minInRows)   = GetMinInRows();
        var (bStrategy, minInColumns) = GetMinInColumns();

        if (minInRows == minInColumns)
        {
            return ([aStrategy], [bStrategy], minInRows);
        }

        do
        {

        } while (
            ReduceBStrategies() &&
            ReduceAStrategies());
        
        decimal[] objectiveFunction = new decimal[_gameMatrix.GetLength(1)];
        for (int i = 0; i < _gameMatrix.GetLength(1); i++)
        {
            objectiveFunction[i] = 1;
        }
        decimal[] results = new decimal[_gameMatrix.GetLength(0)];
        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            results[i] = 1;
        }
        
        string[] inequalities = ["none"];
        
        var simplex = new Simplex(objectiveFunction, _gameMatrix, inequalities, results, _mainWindow);
        var simplexResult = simplex.Solve();
        Table table = simplex.GetTable();

        var gamePrice = 1 / simplexResult[^1];
        
        var y = new List<decimal>();
        for (int i = 0; i < simplexResult.GetLength(0) - 1; i++)
        {
            y.Add(simplexResult[i] * gamePrice);
        }
        
        var x = new List<decimal>();
        for (int i = y.Count; i < table.delta.GetLength(0); i++)
        {
            x.Add(table.delta[i] * gamePrice);
        }
        
        return (x, y, gamePrice);
    }

    private bool ReduceAStrategies()
    {
        var rowsToSave = new List<int>();
        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            rowsToSave.Add(i);
        }

        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < _gameMatrix.GetLength(0); j++)
            {
                bool canBeDeleted = true;
                for (int k = 0; k < _gameMatrix.GetLength(1); k++)
                {
                    if (_gameMatrix[i, k] > _gameMatrix[j, k] || i == j)
                    {
                        canBeDeleted = false;
                        break;
                    }
                }

                if (canBeDeleted && rowsToSave.Contains(i))
                {
                    rowsToSave.Remove(i);
                }
            }
        }

        if (rowsToSave.Count == _gameMatrix.GetLength(0)) return false;
        
        var newGameMatrix = new decimal[rowsToSave.Count, _gameMatrix.GetLength(1)];
        int realRow = 0;
        foreach (var nextRow in rowsToSave)
        {
            for (int i = 0; i < _gameMatrix.GetLength(1); i++)
            {
                newGameMatrix[realRow, i] = _gameMatrix[nextRow, i];
            }
            realRow++;
        }

        _gameMatrix = newGameMatrix;

        Console.WriteLine("Matrix with reduced rows:");
        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < _gameMatrix.GetLength(1); j++)
            {
                Console.Write(_gameMatrix[i, j] + ", ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        
        return true;
    }

    private bool ReduceBStrategies()
    {
        var columnsToSave = new List<int>();
        for (int i = 0; i < _gameMatrix.GetLength(1); i++)
        {
            columnsToSave.Add(i);
        }
        
        for (int i = 0; i < _gameMatrix.GetLength(1); i++)
        {
            for (int j = 0; j < _gameMatrix.GetLength(1); j++)
            {
                bool canBeDeleted = true;
                for (int k = 0; k < _gameMatrix.GetLength(0); k++)
                {
                    if(_gameMatrix[k, i] < _gameMatrix[k, j] || i == j)
                    {
                        canBeDeleted = false;
                        break;
                    }
                }

                if (canBeDeleted && columnsToSave.Contains(i))
                {
                    columnsToSave.Remove(i);
                }
            }
        }

        if (columnsToSave.Count == _gameMatrix.GetLength(1)) return false;
        
        var newGameMatrix = new decimal[_gameMatrix.GetLength(0), columnsToSave.Count];
        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            int realColumn = 0;
            foreach (var nextColumn in columnsToSave)
            {
                newGameMatrix[i, realColumn] = _gameMatrix[i, nextColumn];
                realColumn++;
            }
        }
        
        _gameMatrix = newGameMatrix;

        Console.WriteLine("Matrix with reduced columns:");
        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < _gameMatrix.GetLength(1); j++)
            {
                Console.Write(_gameMatrix[i, j] + ", ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();

        return true;
    }

    private (int, decimal) GetMinInRows()
    {
        int maxMinIndexInRows = 0;
        decimal maxElement = -1;
        for (int i = 0; i < _gameMatrix.GetLength(0); i++)
        {
            decimal currentMinElement  = _gameMatrix[i,0];
            for (int j = 0; j < _gameMatrix.GetLength(1); j++)
            {
                if (_gameMatrix[i,j] < currentMinElement)
                {
                    currentMinElement = _gameMatrix[i,j];
                }
            }

            if (currentMinElement > maxElement)
            {
                maxElement = currentMinElement;
                maxMinIndexInRows = i;
            }
        }
        
        return (maxMinIndexInRows, maxElement);
    }

    private (int, decimal) GetMinInColumns()
    {
        int minMaxIndexInColumns = 0;
        decimal minElement = -1;
        for (int j = 0; j < _gameMatrix.GetLength(1); j++)
        {
            decimal currentMaxElement  = _gameMatrix[0,j];
            for (int i = 0; i < _gameMatrix.GetLength(0); i++)
            {
                if (_gameMatrix[i,j] > currentMaxElement)
                {
                    currentMaxElement = _gameMatrix[i,j];
                }
            }

            if (currentMaxElement < minElement || minElement == -1)
            {
                minElement = currentMaxElement;
                minMaxIndexInColumns = j;
            }
        }
        
        return (minMaxIndexInColumns, minElement);
        
    }
}