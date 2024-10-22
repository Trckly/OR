using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Lab04_OR.Methods;
using System.Linq;
using System.Windows.Media;

namespace Lab04_OR;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _numSuppliers;
    private int _numDemands;

    public MainWindow()
    {
        _numSuppliers = 3;
        _numDemands = 3;

        InitializeComponent();
        GenerateTransportGrid();
        PreDefine();
    }

    private void SupplierComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _numSuppliers = int.Parse((SupplierComboBox.SelectedItem as ComboBoxItem)!.Content.ToString()!);
        GenerateTransportGrid();
    }

    private void ConsumerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _numDemands = int.Parse((ConsumerComboBox.SelectedItem as ComboBoxItem)!.Content!.ToString()!);
        GenerateTransportGrid();
    }

    private void GenerateTransportGrid()
    {
        TransportGrid ??= new Grid();
        TransportGrid.RowDefinitions.Clear();
        TransportGrid.ColumnDefinitions.Clear();
        TransportGrid.Children.Clear();

        // Create Row Definitions (numSuppliers + 1 for header, +1 for supplies input)
        for (int i = 0; i <= _numSuppliers + 1; i++)
        {
            TransportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        // Create Column Definitions (numDemands + 1 for header, +1 for demands input)
        for (int j = 0; j <= _numDemands + 1; j++)
        {
            TransportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // Create headers (top row and left column)
        for (int i = 1; i <= _numSuppliers; i++)
        {
            TextBlock supplierHeader = new TextBlock
            {
                Text = "A" + i,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(supplierHeader, i);
            Grid.SetColumn(supplierHeader, 0);
            TransportGrid.Children.Add(supplierHeader);
        }
        TextBlock demand = new TextBlock
        {
            Text = "D",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(demand, _numSuppliers + 1);
        Grid.SetColumn(demand, 0);
        TransportGrid.Children.Add(demand);

        for (int j = 1; j <= _numDemands; j++)
        {
            TextBlock consumerHeader = new TextBlock
            {
                Text = "B" + j,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(consumerHeader, 0);
            Grid.SetColumn(consumerHeader, j);
            TransportGrid.Children.Add(consumerHeader);
        }
        TextBlock supplies = new TextBlock
        {
            Text = "S",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(supplies, 0);
        Grid.SetColumn(supplies, _numDemands + 1);
        TransportGrid.Children.Add(supplies);

        // Fill grid cells with TextBoxes for input (except the first row/column for headers)
        for (int i = 1; i <= _numSuppliers; i++)
        {
            for (int j = 1; j <= _numDemands; j++)
            {
                TextBox cell = new TextBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    MinWidth = 30,
                    MinHeight = 30,
                };

                // Attach the validation event handler to each TextBox
                cell.PreviewTextInput += Cell_PreviewTextInput;

                Grid.SetRow(cell, i);
                Grid.SetColumn(cell, j);
                TransportGrid.Children.Add(cell);
            }
        }

        // Add TextBoxes for supplies (last column)
        for (int i = 1; i <= _numSuppliers; i++)
        {
            TextBox supplyBox = new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 30,
                MinHeight = 30,
            };

            supplyBox.PreviewTextInput += Cell_PreviewTextInput;

            Grid.SetRow(supplyBox, i);
            Grid.SetColumn(supplyBox, _numDemands + 1);
            TransportGrid.Children.Add(supplyBox);
        }

        // Add TextBoxes for demands (last row)
        for (int j = 1; j <= _numDemands; j++)
        {
            TextBox demandBox = new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 30,
                MinHeight = 30,
            };

            demandBox.PreviewTextInput += Cell_PreviewTextInput;

            Grid.SetRow(demandBox, _numSuppliers + 1);
            Grid.SetColumn(demandBox, j);
            TransportGrid.Children.Add(demandBox);
        }
        TextBox sum = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 30,
            MinHeight = 30,
        };

        sum.PreviewTextInput += Cell_PreviewTextInput;

        Grid.SetRow(sum, _numSuppliers + 1);
        Grid.SetColumn(sum, _numDemands + 1);
        TransportGrid.Children.Add(sum);
    }

    // Event handler for TextBox validation: allows only numeric input
    private void Cell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+"); // Only digits are allowed
        e.Handled = regex.IsMatch(e.Text);
    }
    private void SolveButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Read cost matrix from the grid
        decimal[,] costMatrix = new decimal[_numSuppliers, _numDemands];
        for (int i = 1; i <= _numSuppliers; i++)
        {
            for (int j = 1; j <= _numDemands; j++)
            {
                TextBox cell = (TextBox)TransportGrid.Children
                    .Cast<UIElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);
                costMatrix[i - 1, j - 1] = decimal.Parse(cell.Text);
            }
        }

        // Read supplies and demands
        int[] supplies = new int[_numSuppliers];
        for (int i = 1; i <= _numSuppliers; i++)
        {
            TextBox supplyBox = (TextBox)TransportGrid.Children
                .Cast<UIElement>()
                .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == _numDemands + 1);
            supplies[i - 1] = int.Parse(supplyBox.Text);
        }

        int[] demands = new int[_numDemands];
        for (int j = 1; j <= _numDemands; j++)
        {
            TextBox demandBox = (TextBox)TransportGrid.Children
                .Cast<UIElement>()
                .FirstOrDefault(e => Grid.GetRow(e) == _numSuppliers + 1 && Grid.GetColumn(e) == j);
            demands[j - 1] = int.Parse(demandBox.Text);
        }

        // Solve using UV method
        var uvMethod = new UVMethod(this, costMatrix, supplies, demands);
        decimal result = uvMethod.Solve();

        // Display result (total cost)
        MessageBox.Show($"Total Transportation Cost: {result}", "Result");
    }

    public void ShowMatrixUV(int[,] allocationMatrix, decimal[,] deltaMatrix, decimal[] u, decimal[] v)
    {
        Grid dynamicGrid = new Grid
        {
            Margin = new Thickness(10),
            ShowGridLines = true // Optional: Show grid lines
        };

        int rows = allocationMatrix.GetLength(0) + 1;
        int cols = allocationMatrix.GetLength(1) + 1;

        for (int i = 0; i < rows; i++)
        {
            dynamicGrid.RowDefinitions.Add(new RowDefinition());
        }

        for (int i = 0; i < cols; i++)
        {
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        
        CreateCell(dynamicGrid, 0, 0, @"U\V");

        for (int i = 0; i < rows - 1; i++)
        {
            CreateCell(dynamicGrid, i + 1, 0, u[i].ToString());
        }

        for (int i = 0; i < cols - 1; i++)
        {
            CreateCell(dynamicGrid, 0, i + 1, v[i].ToString());
        }

        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                if (allocationMatrix[i, j] == -1)
                {
                    CreateCell(dynamicGrid, i + 1, j + 1, deltaMatrix[i, j].ToString(), Brushes.Aqua);
                }
                else
                {
                    CreateCell(dynamicGrid, i + 1, j + 1, allocationMatrix[i, j].ToString(), Brushes.Lime);
                }
            }
        }
        
        DynamicGridContainer.Children.Add(dynamicGrid);
    }

    private void CreateCell(Grid dynamicGrid, int x, int y, string text,  Brush? color = null)
    {
        TextBox textBox = new TextBox()
        {
            Text = text,
            Width = 60,
            Height = 30,
            Margin = new Thickness(5),
            Background = color ?? Brushes.White,
            IsReadOnly = true // Make the TextBox read-only to prevent editing
        };

        dynamicGrid.Children.Add(textBox);

        Grid.SetRow(textBox, x);
        Grid.SetColumn(textBox, y);
    }

    private void PreDefine()
    {
        // decimal[,] costs = new [,]
        // {
        //     {10M, 7, 4, 1, 4},
        //     {2, 7, 10, 6, 11},
        //     {8, 5, 3, 2, 2},
        //     {11, 8, 12, 16, 13},
        // };
        // int[] demands = new[] { 200, 200, 100, 100, 250 };
        // int[] suppliers = new[] { 100, 250, 200, 300 };
        //     
        // SupplierComboBox.SelectedValue = "4";
        // ConsumerComboBox.SelectedValue = "5";
        
        
        decimal[,] costs = new [,]
        {
            {3M, 7, 1, 5, 4},
            {7, 5, 8, 6, 3},
            {6, 4, 8, 3, 2},
            {3, 1, 7, 4, 2},
        };
        int[] demands = new[] { 10, 35, 15, 25, 35 };
        int[] suppliers = new[] { 30, 5, 45, 40 };
            
        SupplierComboBox.SelectedValue = "4";
        ConsumerComboBox.SelectedValue = "5";
        
        
        // decimal[,] costs = new [,]
        // {
        //     {4.2M, 10, 5, 9},
        //     {5, 8, 5, 9},
        //     {6, 4, 4, 7.3M},
        //     {7, 5, 11, 4},
        //     {3, 11, 8, 5}
        // };
        // int[] demands = new[] { 35, 22, 30, 15};
        // int[] suppliers = new[] { 17, 33, 20, 12, 20 };
        //     
        // SupplierComboBox.SelectedValue = "5";
        // ConsumerComboBox.SelectedValue = "4";

        for (int i = 1; i <= _numSuppliers; i++)
        {
            for (int j = 1; j <= _numDemands; j++)
            {
                ((TextBox)GetGridElement(TransportGrid, i, j)).Text = costs[i - 1, j - 1].ToString();
            }
        }
        for(int i = 1; i <= _numSuppliers; i++)
        {
            ((TextBox)GetGridElement(TransportGrid, i, _numDemands + 1)).Text = suppliers[i - 1].ToString();
        }
        for(int i = 1; i <= _numDemands; i++)
        {
            ((TextBox)GetGridElement(TransportGrid, _numSuppliers + 1, i)).Text = demands[i - 1].ToString();
        }

        ((TextBox)GetGridElement(TransportGrid, _numSuppliers + 1, _numDemands + 1)).Text = suppliers.Sum().ToString();

    }

    private UIElement GetGridElement(Grid grid, int row, int column)
    {
        foreach (UIElement element in grid.Children)
        {
            if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
            {
                return element;
            }
        }
        return null;
    }
}
