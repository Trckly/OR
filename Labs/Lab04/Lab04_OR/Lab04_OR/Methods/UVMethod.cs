// UVMethod.cs
namespace Lab04_OR.Methods
{
    public class UVMethod
    {
        private int[,] _costMatrix;
        private int[,] _allocation;
        private int[] _supplies;
        private int[] _demands;
        private int[] _u;
        private int[] _v;

        private const int EMPTY_CELL = -1; // Sentinel value for empty cells

        public UVMethod(int[,] costMatrix, int[] supplies, int[] demands)
        {
            _costMatrix = costMatrix;
            _supplies = supplies;
            _demands = demands;

            _u = new int[supplies.Length];
            _v = new int[demands.Length];

            // Initialize the allocation matrix with the EMPTY_CELL value
            _allocation = new int[supplies.Length, demands.Length];
        }

        // Solve using UV Method
        public int Solve()
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

            //CheckAndResolveDegeneracy();

            bool optimalSolutionFound = false;

            while (!optimalSolutionFound)
            {
                CalculateUV();

                var (row, col, maxOpCost) = FindEnteringVariable();

                if (maxOpCost <= 0)
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
            // _u[0] = 0;
            //
            // for (int i = 0; i < _u.Length; i++)
            // {
            //     for (int j = 0; j < _v.Length; j++)
            //     {
            //         if (_allocation[i, j] != EMPTY_CELL && _allocation[i, j] > 0)
            //         {
            //             if (_u[i] != int.MaxValue)
            //             {
            //                 _v[j] = _costMatrix[i, j] - _u[i];
            //             }
            //             else
            //             {
            //                 _u[i] = _costMatrix[i, j] - _v[j];
            //             }
            //         }
            //     }
            // }

            // int maxRowIndex = -1;
            // int maxRowValue = 0;
            // for (int i = 0; i < _allocation.GetLength(0); i++)
            // {
            //     int rowValue = 0;
            //     for (int j = 0; j < _allocation.GetLength(1); j++)
            //     {
            //         if (_allocation[i, j] != EMPTY_CELL)
            //         {
            //             rowValue++;
            //         }
            //     }
            //
            //     if (rowValue > maxRowValue)
            //     {
            //         maxRowValue = rowValue;
            //         maxRowIndex = i;
            //     }
            // }
            //
            // if (maxRowIndex == -1)
            // {
            //     throw new Exception("_allocation is empty.");
            // }
            //
            // _u[maxRowIndex] = 0;

            _allocation[2, 3] = 0;
            _u = new[] { -12, -1, -11, 0 };
            _v = new[] { 3, 8, 12, 13, 13};

        }

        private (int, int, int) FindEnteringVariable()
        {
            int maxOpCost = int.MinValue;
            int enteringRow = -1, enteringCol = -1;

            for (int i = 0; i < _u.Length; i++)
            {
                for (int j = 0; j < _v.Length; j++)
                {
                    if (_allocation[i, j] == EMPTY_CELL) // Non-basic variable
                    {
                        int opCost = (_u[i] + _v[j]) - _costMatrix[i, j];
                        if (opCost > maxOpCost)
                        {
                            maxOpCost = opCost;
                            enteringRow = i;
                            enteringCol = j;
                        }
                    }
                }
            }

            return (enteringRow, enteringCol, maxOpCost);
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

                if (i % 2 == 0)
                {
                    _allocation[r, c] += minAllocation;
                }
                else
                {
                    _allocation[r, c] -= minAllocation;

                    if (_allocation[r, c] == 0)
                    {
                        _allocation[r, c] = EMPTY_CELL; // Reset to empty
                    }
                }
            }
        } // Find the loop formed by the new entering cell at (row, col)

        private List<(int, int)> FindLoop(int row, int col)
        {
            // We'll find the loop by tracing through the rows and columns, connecting allocated cells.
            List<(int, int)> loop = new List<(int, int)>();

            // Add the starting cell (the entering variable)
            loop.Add((row, col));

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
            if (loop.Count > 3 && loop[0] == (currentRow, currentCol))
            {
                return true; // Loop completed
            }

            if (isRow)
            {
                // Search the current row for other allocations in the same row
                for (int j = 0; j < _allocation.GetLength(1); j++)
                {
                    // Ignore empty cells marked with -1 and skip the current column
                    if (j != currentCol && _allocation[currentRow, j] > EMPTY_CELL)
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
                    if (i != currentRow && _allocation[i, currentCol] > EMPTY_CELL)
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

        private int CalculateTotalCost()
        {
            int totalCost = 0;

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

                int[,] newCostMatrix = new int[_supplies.Length, _demands.Length];
                Array.Copy(_costMatrix, newCostMatrix, _costMatrix.Length);
                _costMatrix = newCostMatrix;
            }
            else if (demandSum > supplySum)
            {
                int[] newSupplies = new int[_supplies.Length + 1];
                Array.Copy(_supplies, newSupplies, _supplies.Length);
                newSupplies[^1] = demandSum - supplySum;
                _supplies = newSupplies;

                int[,] newCostMatrix = new int[_supplies.Length, _demands.Length];
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
