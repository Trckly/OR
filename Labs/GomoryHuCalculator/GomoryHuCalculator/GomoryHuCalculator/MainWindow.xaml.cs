using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GomoryHuCalculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public partial class MainWindow : Window
{
    private int _nodeCount;

    public int NodeCount
    {
        get => _nodeCount;
        set => _nodeCount = value > 2 ? value : 2;
    }

    public MainWindow()
    {
        NodeCount = 8;
        
        InitializeComponent();
        GenerateTransportGrid();
        PreDefineGraph();
    }
    
    private void GenerateTransportGrid()
    {
        TransportGrid ??= new Grid();
        TransportGrid.RowDefinitions.Clear();
        TransportGrid.ColumnDefinitions.Clear();
        TransportGrid.Children.Clear();
    
        // Create Row Definitions
        for (var i = 0; i <= NodeCount; i++)
        {
            TransportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            TransportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
    
        // Create headers (top row and left column)
        for (var i = 1; i <= NodeCount; i++)
        {
            var rowHeaders = new TextBlock
            {
                Text = ((char)('A' + i - 1)).ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
            };
            Grid.SetRow(rowHeaders, i);
            Grid.SetColumn(rowHeaders, 0);
            TransportGrid.Children.Add(rowHeaders);
            
            var columnHeaders = new TextBlock
            {
                Text = ((char)('A' + i - 1)).ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
            };
            Grid.SetRow(columnHeaders, 0);
            Grid.SetColumn(columnHeaders, i);
            TransportGrid.Children.Add(columnHeaders);
        }
    
        // Fill grid cells with TextBoxes for input (except the first row/column for headers)
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                var cell = new TextBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    MinWidth = 30,
                    MinHeight = 30,
                    
                    // Styles
                    Background = new SolidColorBrush(Color.FromRgb(221, 170, 255)),
                    Foreground = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
                };
    
                // Attach the validation event handler to each TextBox
                cell.PreviewTextInput += Cell_PreviewTextInput;
    
                Grid.SetRow(cell, i);
                Grid.SetColumn(cell, j);
                TransportGrid.Children.Add(cell);
            }
        }
    }
    
    private void PreDefineGraph()
    {
        // Wikipedia
        // double [,]weightMatrix =
        // {
        //     { 0,  1,  7, -1, -1, -1},
        //     { 1,  0,  1,  3,  2, -1},
        //     { 7,  1,  0, -1,  4, -1},
        //     {-1,  3, -1,  0,  1,  6},
        //     {-1,  2,  4,  1,  0,  2},
        //     {-1, -1, -1,  6,  2,  0},
        // };
        
        // GeekForGeeks
        // double [,]weightMatrix =
        // {
        //     { 0, 10, -1, -1, -1,  8},
        //     {10,  0,  4, -1,  2,  3},
        //     {-1,  4,  0,  5,  4,  2},
        //     {-1, -1,  5,  0,  7,  2},
        //     {-1,  2,  4,  7,  0,  3},
        //     { 8,  3,  2,  2,  3,  0},
        // };
        
        // DO example
        // double [,]weightMatrix =
        // {
        //     { 0,  8,  9,  7, -1, -1, -1},
        //     { 8,  0, -1,  5,  7, -1, -1},
        //     { 9, -1,  0,  4, -1,  9, -1},
        //     { 7,  5,  4,  0,  4,  6,  8},
        //     {-1,  7, -1,  4,  0, -1,  2},
        //     {-1, -1,  9,  6, -1,  0, 11},
        //     {-1, -1, -1,  8,  2, 11,  0}
        // };
        
        // My
        // double [,]weightMatrix =
        // {
        //     { 0,  5, -1,  5,  3,  8, -1},
        //     { 5,  0, -1, -1,  9, -1, -1},
        //     {-1, -1,  0,  6,  2, -1,  6},
        //     { 5, -1,  6,  0, -1,  7,  6},
        //     { 3,  9,  2, -1,  0,  4, -1},
        //     { 8, -1, -1,  7,  4,  0, -1},
        //     {-1, -1,  6,  6, -1, -1,  0}
        // };
        
        // Danik
        int [,]weightMatrix =
        {
            { 0,  2,  8,  0,  1,  4,  0,  0},
            { 2,  0,  0,  0,  4,  7,  0,  0},
            { 8,  0,  0,  7,  0,  3,  0,  0},
            { 0,  0,  7,  0,  0,  0,  6,  0},
            { 1,  4,  0,  0,  0,  0,  0,  2},
            { 4,  7,  3,  0,  0,  0,  0,  4},
            { 0,  0,  0,  6,  0,  0,  0,  6},
            { 0,  0,  0,  0,  2,  4,  6,  0}
        };
        
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                ((TextBox)GetGridElement(TransportGrid, i, j)).Text = weightMatrix[i - 1, j - 1].ToString();
            }
        }
    }
    
    private UIElement GetGridElement(Grid grid, int row, int column)
    {
        foreach (UIElement element in grid.Children)
        {
            if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
                return element;
        }

        return null;
    }
    
    // Event handler for TextBox validation: allows only numeric input
    private void Cell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^-?0-9]+"); // Only positive and negative digits are allowed
        e.Handled = regex.IsMatch(e.Text);
    }

    private void SolveButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Read weight matrix from the grid
        var weightMatrix = new double[NodeCount, NodeCount];
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                var cell = (TextBox)TransportGrid.Children
                    .Cast<UIElement>()
                    .FirstOrDefault(element => Grid.GetRow(element) == i && Grid.GetColumn(element) == j)!;
                weightMatrix[i - 1, j - 1] = double.Parse(cell?.Text ?? string.Empty);
            }
        }

        var gomoryHu = new GomoryHuMethod(weightMatrix, IterationsPanel, TreePanel);

        var solutionMatrix = gomoryHu.Solve();
        
        ShowMatrix(solutionMatrix);
    }

    public void ShowMatrix(double[,] solutionMatrix)
    {
        var headersColor = Brushes.White;
        var cellsColor = new SolidColorBrush(Color.FromRgb(221, 170, 255));
        
        Grid dynamicGrid = new Grid
        {
            Margin = new Thickness(10),
            ShowGridLines = false // Optional: Show grid lines
        };

        for (int i = 0; i <= NodeCount; i++)
        {
            dynamicGrid.RowDefinitions.Add(new RowDefinition());
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        
        CreateCell(dynamicGrid, 0, 0, " ", headersColor);
        
        for (int i = 0; i < NodeCount; i++)
        {
            CreateCell(dynamicGrid, 0, i + 1, Convert.ToChar('A' + i).ToString(), headersColor);
            CreateCell(dynamicGrid, i + 1, 0, Convert.ToChar('A' + i).ToString(), headersColor);
        }

        for (int i = 0; i < NodeCount; i++)
        {
            for (int j = 0; j < NodeCount; j++)
            {
                var backgroundColor = solutionMatrix[i, j] == 0 ? headersColor : cellsColor;
                CreateCell(dynamicGrid, i + 1, j + 1, solutionMatrix[i, j].ToString(), backgroundColor);
            }
        }
        
        DynamicGridContainer.Children.Add(dynamicGrid);
    }

    private void CreateCell(Grid dynamicGrid, int x, int y, string text,  Brush? color = null)
    {
        TextBox textBox = new TextBox()
        {
            Text = text,
            Width = 30,
            Height = 30,
            Margin = new Thickness(5),
            BorderThickness = new Thickness(0),
            Background = color ?? Brushes.White,
            TextAlignment = TextAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
            IsReadOnly = true // Make the TextBox read-only to prevent editing
        };

        dynamicGrid.Children.Add(textBox);

        Grid.SetRow(textBox, x);
        Grid.SetColumn(textBox, y);
    }

    private void MinusButton_OnClick(object sender, RoutedEventArgs e)
    {
        NodeCount--;
        GenerateTransportGrid();
    }

    private void PlusButton_OnClick(object sender, RoutedEventArgs e)
    {
        NodeCount++;
        GenerateTransportGrid();
    }
}