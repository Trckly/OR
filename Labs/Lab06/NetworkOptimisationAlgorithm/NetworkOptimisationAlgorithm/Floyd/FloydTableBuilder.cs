using System.Windows;
using System.Windows.Controls;

namespace NetworkOptimisationAlgorithm.Floyd;

public static class FloydTableBuilder
{
    public static void BuildFinalTable(int [,] shortestPathMatrix, int [,] routeMatrix, StackPanel dynamicGridContainer)
    {
        var nodeCount = shortestPathMatrix.GetLength(0);
        const int headerCount = 3;
        
        var tableTitle = new TextBlock
        {
            Text = "Floyd Result Table: ",
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 16,
            FontStyle = FontStyles.Oblique,
            Margin = new Thickness(0,10,0,0)
        };
        dynamicGridContainer.Children.Add(tableTitle);
    
        var resultGrid = new Grid()
        {
            ShowGridLines = true,
            Margin = new Thickness(10)
        };
        
        for (var i = 0; i <= nodeCount; i++)
        {
            resultGrid.RowDefinitions.Add(new RowDefinition());
    
            if (i < headerCount)
                resultGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
    
        CreateCell(resultGrid, 0, 0, "Route");
        CreateCell(resultGrid, 0, 1, "Path");
        CreateCell(resultGrid, 0, 2, "Length");
    
        for (var i = 1; i <= nodeCount; ++i)
        {
            var pathStr = TracePath(routeMatrix, i - 1);
    
            CreateCell(resultGrid, i, 0, $"A-{(char)('A' + i - 1)}");
            CreateCell(resultGrid, i, 1, $"{pathStr}");
            CreateCell(resultGrid, i, 2, $"{shortestPathMatrix[0, i - 1]}");
        }
    
        dynamicGridContainer.Children.Add(resultGrid);
    }

    private static string TracePath(int [,] routeMatrix, int destinationIndex)
    {
        var path = new List<int>();
        var at = 0;
        for (; at != destinationIndex; at = routeMatrix[at, destinationIndex])
        {
            path.Add(at);
        }
        path.Add(at);

        var pathStr = string.Join("->", path.Select(p => (char)('A' + p)));
        
        return pathStr;
    }

    private static void CreateCell(Grid dynamicGrid, int x, int y, string text)
    {
        var textBox = new TextBlock
        {
            Text = text,
            Width = 150,
            Height = 30,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
    
        Grid.SetRow(textBox, x);
        Grid.SetColumn(textBox, y);
        dynamicGrid.Children.Add(textBox);
    }
}