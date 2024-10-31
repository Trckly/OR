using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NetworkOptimisationAlgorithm.Dijkstra;

public class DijkstraTableBuilder
{
    public static void BuildFinalTable(List<int> weightArray, List<int> tracebackArray, StackPanel dynamicGridContainer)
    {
        var nodeCount = weightArray.Count;
        const int headerCount = 3;

        var tableTitle = new TextBlock
        {
            Text = "Dijkstra Result Table: ",
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
            TracePath(weightArray, tracebackArray, i - 1, out var path, out var length);

            CreateCell(resultGrid, i, 0, $"A-{(char)('A' + i - 1)}");
            CreateCell(resultGrid, i, 1, $"{path}");
            CreateCell(resultGrid, i, 2, $"{length}");
        }

        dynamicGridContainer.Children.Add(resultGrid);
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
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };

        Grid.SetRow(textBox, x);
        Grid.SetColumn(textBox, y);
        dynamicGrid.Children.Add(textBox);
    }

    private static void TracePath(List<int> weightArray, List<int> tracebackArray, int destinationIndex,
        out string path, out int length)
    {
        var traceList = new List<int>();
        traceList.Add(destinationIndex);

        length = weightArray[destinationIndex];
        var traceIndex = destinationIndex;
        do
        {
            traceIndex = tracebackArray[traceIndex];
            if (traceIndex != -1)
            {
                traceList.Add(traceIndex);
            }
        } while (traceIndex != -1);
        
        traceList.Reverse();

        path = "";
        for (var i = 0; i < traceList.Count; ++i)
        {
            path += i != traceList.Count - 1 ? $"{(char)('A' + traceList[i])}->" : $"{(char)('A' + traceList[i])}";
        }
    }
}