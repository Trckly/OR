using System.Reflection.Metadata;

namespace NetworkOptimisationAlgorithm.Floyd;

public static class FloydLogger
{
   public static void OutFloydMatrices(int[,] shortestPathMatrix, int[,] routeMatrix, int index = -1)
   {
      var nodesCount = shortestPathMatrix.GetLength(0);

      if (index != -1)
      {
         Console.WriteLine($"Matrix {index}");
      }

      // Define column width for consistent formatting
      const int columnWidth = 6;

      // Print Shortest Path Matrix
      Console.WriteLine("Shortest Path Matrix:");
      for (var i = 0; i < nodesCount; i++)
      {
         for (var j = 0; j < nodesCount; j++)
         {
            var shortestPath = shortestPathMatrix[i, j];
            var value = shortestPath != int.MaxValue ? shortestPath.ToString() : "inf";
            Console.Write(value.PadRight(columnWidth));
         }
         Console.WriteLine();
      }

      // Print Route Matrix
      Console.WriteLine("\nRoute Matrix:");
      for (var i = 0; i < nodesCount; i++)
      {
         for (var j = 0; j < nodesCount; j++)
         {
            var route = routeMatrix[i, j];
            var value = route != -1 ? $"{(char)('A' + route)}" : "0";
            Console.Write(value.PadRight(columnWidth));
         }
         Console.WriteLine();
      }
   }
}