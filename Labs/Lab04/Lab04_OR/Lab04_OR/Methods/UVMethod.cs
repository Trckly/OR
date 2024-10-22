// UVMethod.cs
namespace Lab04_OR.Methods
{
    public class UVMethod
    {
        private MainWindow _mainWindow;
        private decimal[,] _costMatrix;
        private decimal[,] _deltaMatrix;
        private int[,] _allocation;
        private int[] _supplies;
        private int[] _demands;
        private decimal[] _u;
        private decimal[] _v;

        private const int EMPTY_CELL = -1; // Sentinel value for empty cells

        public UVMethod(MainWindow mainWindow, decimal[,] costMatrix, int[] supplies, int[] demands)
        {
            _mainWindow = mainWindow;
            _costMatrix = costMatrix;
            _supplies = supplies;
            _demands = demands;
            _deltaMatrix = new decimal[_costMatrix.GetLength(0), _costMatrix.GetLength(1)];

            _u = new decimal[supplies.Length];
            _v = new decimal[demands.Length];

            // Initialize the allocation matrix with the EMPTY_CELL value
            _allocation = new int[supplies.Length, demands.Length];
        }

        // Solve using UV Method
        public decimal Solve()
        {
            var leastCostSolver = new LeastCostCellMethod(_costMatrix, _supplies, _demands);
            leastCostSolver.Solve();
            _allocation = leastCostSolver.GetAllocationMatrix();
            for (int i = 0; i < _allocation.GetLength(0); i++)
            {
                for (int j = 0; j < _allocation.GetLength(1); j++)
                {
                    if(_allocation[i, j] == 0)
                        _allocation[i, j] = EMPTY_CELL; // Indicate unallocated cells
                }
            }

            if (!IsBalanced())
            {
                BalanceSystem();
            }

            CheckAndResolveDegeneracy();

            bool optimalSolutionFound = false;

            while (!optimalSolutionFound)
            {
                CalculateUV();

                var (row, col, maxOpCost) = FindEnteringVariable();

                _mainWindow.ShowMatrixUV(_allocation, _deltaMatrix, _u, _v);
                if (maxOpCost >= 0)
                {
                    optimalSolutionFound = true;
                }
                else
                {
                    PerformPivot(row, col);
                }
            }

            return CalculateTotalCost();
        }

        private void CalculateUV()
        {
            _u = new decimal[_allocation.GetLength(0)];
            _v = new decimal[_allocation.GetLength(1)];
            bool[] _uIsSet = new bool [_u.Length];
            bool[] _vIsSet = new bool [_v.Length];

            int maxValues = 0;
            int rowWithMaxValues = 0;
            for (int i = 0; i < _allocation.GetLength(0); i++)
            {
                int numOfValuesInRow = 0;
                for (int j = 0; j < _allocation.GetLength(1); j++)
                {
                    numOfValuesInRow += _allocation[i, j] != EMPTY_CELL ? 1 : 0;
                }

                if (numOfValuesInRow > maxValues)
                {
                    maxValues = numOfValuesInRow;
                    rowWithMaxValues = i;
                }
            }

            _u[rowWithMaxValues] = 0;
            _uIsSet[rowWithMaxValues] = true;

            while(!(_uIsSet.All(x => x) && _vIsSet.All(x => x)))
            {
                for (int i = 0; i < _u.Length; i++)
                {
                    for (int j = 0; j < _v.Length; j++)
                    {
                        if (_allocation[i, j] != EMPTY_CELL)
                        {
                            if (_uIsSet[i] && !_vIsSet[j])
                            {
                                _v[j] = _costMatrix[i, j] - _u[i];
                                _vIsSet[j] = true;
                            }
                            else if (_vIsSet[j] && !_uIsSet[i])
                            {
                                _u[i] = _costMatrix[i, j] - _v[j];
                                _uIsSet[i] = true;
                            }
                        }
                    }
                }
            }
        }

        private (int, int, decimal) FindEnteringVariable()
        {
            decimal minOpCost = 0;
            int enteringRow = -1, enteringCol = -1;

            for (int i = 0; i < _u.Length; i++)
            {
                for (int j = 0; j < _v.Length; j++)
                {
                    if (_allocation[i, j] == EMPTY_CELL) // Non-basic variable
                    {
                        decimal opCost = _costMatrix[i, j] - (_u[i] + _v[j]);
                        _deltaMatrix[i, j] = opCost;
                        if (opCost < minOpCost)
                        {
                            minOpCost = opCost;
                            enteringRow = i;
                            enteringCol = j;
                        }
                    }
                }
            }

            return (enteringRow, enteringCol, minOpCost);
        }

        private void PerformPivot(int row, int col)
        {
            List<(int, int)> loop = FindLoop(row, col);

            if (loop == null || loop.Count == 0)
            {
                throw new InvalidOperationException("No valid loop found for reallocation.");
            }

            int minAllocation = int.MaxValue;

            for (int i = 1; i < loop.Count; i += 2)
            {
                (int r, int c) = loop[i];
                if (_allocation[r, c] < minAllocation)
                {
                    minAllocation = _allocation[r, c];
                }
            }

            for (int i = 0; i < loop.Count; i++)
            {
                (int r, int c) = loop[i];


                if (_allocation[r, c] == 0 && !(r == row && c == col) && minAllocation == 0)
                {
                    _allocation[r, c] = EMPTY_CELL; // Reset to empty
                    continue;
                }
                
                if (i % 2 == 0)
                {
                    _allocation[r, c] += minAllocation;
                }
                else
                {
                    _allocation[r, c] -= minAllocation;
                }
            }
        }

        // Find the loop formed by the new entering cell at (row, col)
        private List<(int, int)> FindLoop(int row, int col)
        {
            // We'll find the loop by tracing through the rows and columns, connecting allocated cells.
            List<(int, int)> loop = new List<(int, int)>();

            // Add the starting cell (the entering variable)
            loop.Add((row, col));
            _allocation[row, col] = 0;

            // We now need to find a path that alternates between rows and columns
            // and returns to the starting point.
            // For simplicity, we'll perform a search to find the loop.

            bool loopFound = TraceLoop(loop, row, col, true); // Start with row trace

            if (!loopFound)
            {
                return null; // No loop found
            }

            return loop;
        }

// Recursive helper function to trace the loop
// - "isRow" indicates whether we're tracing rows (if true) or columns (if false)
        private bool TraceLoop(List<(int, int)> loop, int currentRow, int currentCol, bool isRow)
        {
            // Base case: If loop has returned to the starting point and has more than 3 elements, it's complete
            (int row, int col) = loop[0];
            if (loop.Count > 3 && (row == currentRow || col == currentCol))
            {
                return true; // Loop completed
            }

            if (isRow)
            {
                // Search the current row for other allocations in the same row
                for (int j = 0; j < _allocation.GetLength(1); j++)
                {
                    // Ignore empty cells marked with -1 and skip the current column
                    if (j != currentCol && _allocation[currentRow, j] != EMPTY_CELL)
                    {
                        // Add the next step in the loop
                        loop.Add((currentRow, j));

                        // Recur to trace the column now
                        if (TraceLoop(loop, currentRow, j, false))
                        {
                            return true;
                        }

                        // Backtrack if no loop found
                        loop.RemoveAt(loop.Count - 1);
                    }
                }
            }
            else
            {
                // Search the current column for other allocations in the same column
                for (int i = 0; i < _allocation.GetLength(0); i++)
                {
                    // Ignore empty cells marked with -1 and skip the current row
                    if (i != currentRow && _allocation[i, currentCol] != EMPTY_CELL)
                    {
                        // Add the next step in the loop
                        loop.Add((i, currentCol));

                        // Recur to trace the row now
                        if (TraceLoop(loop, i, currentCol, true))
                        {
                            return true;
                        }

                        // Backtrack if no loop found
                        loop.RemoveAt(loop.Count - 1);
                    }
                }
            }

            return false; // No valid loop found
        }


        private void CheckAndResolveDegeneracy()
        {
            int m = _allocation.GetLength(0);
            int n = _allocation.GetLength(1);
            int requiredAllocations = m + n - 1;
            int currentAllocations = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (_allocation[i, j] != EMPTY_CELL && _allocation[i, j] > 0)
                    {
                        currentAllocations++;
                    }
                }
            }
            if (currentAllocations < requiredAllocations)
            {
                ResolveDegeneracy(currentAllocations, requiredAllocations);
            }
        }
        private void ResolveDegeneracy(int currentAllocations, int requiredAllocations)
        {
            int m = _allocation.GetLength(0);
            int n = _allocation.GetLength(1);
            Dictionary<int, List<int>> keyCells = new Dictionary<int, List<int>>();
            for (int i = 0; i < n; i++)
            {
                keyCells.Add(i, new List<int>());
                for (int j = 0; j < m; j++)
                {
                    if (_allocation[j, i] != EMPTY_CELL)
                    {
                        keyCells[i].Add(j);
                    }
                }
            }
            var fulfilledRows = keyCells
                .Where(x => x.Value.Count >= 2)
                .SelectMany(x => x.Value)
                .Distinct()
                .ToList();
            var lonelyCells = keyCells
                .Where(x => x.Value.Count < 2 && !fulfilledRows
                    .Contains(x.Value.FirstOrDefault()))
                .Select(x => x.Key)
                .ToList();
            foreach (var lonelyCell in lonelyCells)
            {
                _allocation[fulfilledRows.FirstOrDefault(), lonelyCell] = 0;
                currentAllocations++;
                if (currentAllocations == requiredAllocations)
                    break;
            }
        }

        private bool IsForbidden(int row, int col)
        {
            return false;
        }

        private decimal CalculateTotalCost()
        {
            decimal totalCost = 0;

            for (int i = 0; i < _u.Length; i++)
            {
                for (int j = 0; j < _v.Length; j++)
                {
                    if (_allocation[i, j] != EMPTY_CELL && _allocation[i, j] > 0)
                    {
                        totalCost += _allocation[i, j] * _costMatrix[i, j];
                    }
                }
            }

            return totalCost;
        }

        private bool IsBalanced()
        {
            return _supplies.Sum() == _demands.Sum();
        }

        private void BalanceSystem()
        {
            int supplySum = _supplies.Sum();
            int demandSum = _demands.Sum();

            if (supplySum > demandSum)
            {
                int[] newDemands = new int[_demands.Length + 1];
                Array.Copy(_demands, newDemands, _demands.Length);
                newDemands[^1] = supplySum - demandSum;
                _demands = newDemands;

                decimal[,] newCostMatrix = new decimal[_supplies.Length, _demands.Length];
                Array.Copy(_costMatrix, newCostMatrix, _costMatrix.Length);
                _costMatrix = newCostMatrix;
            }
            else if (demandSum > supplySum)
            {
                int[] newSupplies = new int[_supplies.Length + 1];
                Array.Copy(_supplies, newSupplies, _supplies.Length);
                newSupplies[^1] = demandSum - supplySum;
                _supplies = newSupplies;

                decimal[,] newCostMatrix = new decimal[_supplies.Length, _demands.Length];
                Array.Copy(_costMatrix, newCostMatrix, _costMatrix.Length);
                _costMatrix = newCostMatrix;
            }
        }

        public int[,] GetAllocation()
        {
            return _allocation;
        }
    }
}
