using System;
using System.Linq;
using System.Windows;

namespace Lab04_OR.Methods
{
    public class LeastCostCellMethod
    {
        private readonly decimal[,] _costMatrix;
        private readonly int[,] _allocation;
        private readonly int[] _supplies;
        private readonly int[] _demands;

        public LeastCostCellMethod(decimal[,] costMatrix, int[] supplies, int[] demands)
        {
            _costMatrix = costMatrix;
            _supplies = supplies;
            _demands = demands;

            // Create an allocation matrix initialized to zero
            _allocation = new int[_supplies.Length, _demands.Length];
        }

        // Method to solve the transportation problem using Least Cost Cell method
        public decimal Solve()
        {
            decimal totalCost = 0;

            // Copy of supplies and demands to keep track of remaining
            int[] suppliesRemaining = (int[])_supplies.Clone();
            int[] demandsRemaining = (int[])_demands.Clone();

            while (suppliesRemaining.Any(s => s > 0) && demandsRemaining.Any(d => d > 0))
            {
                // Find the least cost cell
                (int minRow, int minCol) = FindLeastCostCell(suppliesRemaining, demandsRemaining);

                // Find the minimum of supply and demand for the selected cell
                int allocationAmount = Math.Min(suppliesRemaining[minRow], demandsRemaining[minCol]);

                // Allocate this amount
                _allocation[minRow, minCol] = allocationAmount;

                // Update total cost
                totalCost += allocationAmount * _costMatrix[minRow, minCol];

                // Update remaining supply and demand
                suppliesRemaining[minRow] -= allocationAmount;
                demandsRemaining[minCol] -= allocationAmount;
            }

            return totalCost;
        }

        // Method to return the cost matrix (useful for inspection or testing)
        public decimal[,] GetCostMatrix()
        {
            return _costMatrix;
        }
        public int[,] GetAllocationMatrix()
        {
            return _allocation;
        }

        // Helper method to find the least cost cell from the remaining supplies and demands
        private (int, int) FindLeastCostCell(int[] suppliesRemaining, int[] demandsRemaining)
        {
            decimal minCost = decimal.MaxValue;
            int minRow = -1;
            int minCol = -1;

            for (int i = 0; i < _supplies.Length; i++)
            {
                if (suppliesRemaining[i] > 0) // Only consider rows with remaining supplies
                {
                    for (int j = 0; j < _demands.Length; j++)
                    {
                        if (demandsRemaining[j] > 0) // Only consider columns with remaining demands
                        {
                            if (_costMatrix[i, j] <= minCost)
                            {
                                minCost = _costMatrix[i, j];
                                minRow = i;
                                minCol = j;
                            }
                        }
                    }
                }
            }

            return (minRow, minCol);
        }
    }
}
