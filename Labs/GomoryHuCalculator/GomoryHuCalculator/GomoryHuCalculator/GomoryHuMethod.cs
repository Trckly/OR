using System.Runtime.InteropServices.JavaScript;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GomoryHuCalculator;

struct Dot
{
    public int X{get; set; }
    public int Y{get;set;}
}

public class GomoryHuMethod
{
    private double[,] _initialGraphMatrix;
    private double[,] _solutionGraphMatrix;
    private List<int> _initialVertices;
    private List<List<int>> _graph;
    private int _nodesCount;
    
    public List<int> InitialVertices => _initialVertices;
    public List<List<int>> Graph => _graph;
    public int NodesCount => _nodesCount;

    public StackPanel IterationsPanel { get; set; }
    public StackPanel TreePanel { get; set; }
    
    private readonly SolidColorBrush _foregroundColor = new SolidColorBrush(Color.FromRgb(102, 17, 153));
    
    public GomoryHuMethod(double [,] initialGraphMatrix, StackPanel iterationsPanel, StackPanel treePanel)
    {
        _initialGraphMatrix = initialGraphMatrix;
        _nodesCount = initialGraphMatrix.GetLength(0);
        
        _graph = new List<List<int>>();
        _initialVertices  = new List<int>();
        for(int i=0; i<_nodesCount; ++i)
        {
            _initialVertices.Add(i);
        }

        _solutionGraphMatrix = new double[_initialGraphMatrix.GetLength(0), _initialGraphMatrix.GetLength(1)];

        IterationsPanel = iterationsPanel;
        TreePanel = treePanel;
    }

    public double[,] Solve()
    {
        if (_graph.Count == _nodesCount)
        {
            return _solutionGraphMatrix;
        }

        int prevVertex = -1;
        while(_initialVertices.Count > 0)
        {
            if (_initialVertices.Count == 1 && _graph.Count == 1)
            {
                _graph.Add([_initialVertices.Last()]);
                _initialVertices.Remove(_initialVertices.Last());
            }
            else
            {
                int nextVertex = FindNextVertex(prevVertex);
                double nextVertexSTCut = FindSTCut([nextVertex]);

                double stCut = TryToPlace(nextVertex, nextVertexSTCut);
                prevVertex = nextVertex;
            }

            ShowIterations(IterationsPanel);

        }

        CreateOutputString(TreePanel, "Cursed Tree:", _foregroundColor, true);
        foreach (var list in _graph)
        {
            SetSolutionMatrix(list);
        }
        
        return _solutionGraphMatrix;
    }

    private void SetSolutionMatrix(List<int> blackList)
    {
        string resultingString;
        
        (int vertex, double cut) = FindMinimalSTCutVertex(blackList);
        
        blackList.Remove(vertex);
        List<int> trackedVertices = [vertex];
        
        resultingString = $"{Convert.ToChar('a' + vertex)} - {cut}";
        
        InsertValuesInMatrix(vertex, cut, trackedVertices);

        while (blackList.Count > 0)
        {
            vertex = FindConnectedVertex(trackedVertices.Last(), blackList);
            blackList.Remove(vertex);
            trackedVertices.Add(vertex);
            cut = FindSTCut(trackedVertices);
            
            InsertValuesInMatrix(vertex, cut, trackedVertices);

            resultingString += $" - {Convert.ToChar('a' + vertex)} - {cut}";
        }
        CreateOutputString(TreePanel, resultingString, _foregroundColor);
    }

    private int FindConnectedVertex(int lastVertex, List<int> blackList)
    {
        foreach (var vertex in blackList)
        {
            if (_initialGraphMatrix[vertex, lastVertex] > 0.00001)
            {
                return vertex;
            }
        }

        return -1;
    }

    private void InsertValuesInMatrix(int vertex, double cut, List<int> blackList)
    {
        foreach (var listVertex in blackList)
        {
            for (int i = 0; i < _nodesCount; ++i)
            {
                if (!blackList.Contains(i) &&
                    (_solutionGraphMatrix[i, listVertex] > cut || _solutionGraphMatrix[i, listVertex] < 0.00001))
                {
                    _solutionGraphMatrix[listVertex, i] = cut;
                    _solutionGraphMatrix[i, listVertex] = cut;
                }
            }
        }
    }

    private (int, double) FindMinimalSTCutVertex(List<int> blackList)
    {
        double minimalSTCut = 0;
        int minimalVertex = 0;
        foreach (var vertex in blackList)
        {
            var nextCut = FindSTCut([vertex]);

            if (nextCut < minimalSTCut || minimalSTCut == 0)
            {
                minimalSTCut = nextCut;
                minimalVertex = vertex;
            }
        }

        return (minimalVertex, minimalSTCut);
    }

    private double TryToPlace(int nextVertex, double selfSTcut)
    {
        bool canPlace = false;
        List<int>? currentList = null;
        double currentSTCut = selfSTcut;
        foreach (var list in _graph)
        {
            foreach (var vertex in list)
            {
                if (_initialGraphMatrix[nextVertex, vertex] > 0)
                {
                    int[] checkList = new int[list.Count + 1];
                    list.CopyTo(checkList, 0);
                    checkList[^1] = nextVertex;
                    
                    var cut = FindSTCut(checkList.ToList());
                    if (currentSTCut > cut || (list.Any(v => FindSTCut([v]) > cut) && FindSTCut(list) > cut ))
                    {
                        currentList = list;
                        currentSTCut = cut;
                        canPlace = true;
                        break;
                    }
                }
            }
        }

        if (canPlace)
            currentList!.Add(nextVertex);
        else
            _graph.Add([nextVertex]);

        return currentSTCut;
    }

    private int FindNextVertex(int prevVertex)
    {
        double maxEdgeWeight = 0;
        int vertexWithMaxEdge = 0;
        foreach (var vertex in _initialVertices)
        {
            if (prevVertex != -1)
            {
                if (_initialGraphMatrix[vertex, prevVertex] >= maxEdgeWeight)
                {
                    maxEdgeWeight = _initialGraphMatrix[vertex, prevVertex];
                    vertexWithMaxEdge = vertex;
                }
            }
            else
            {
                for (int i = 0; i < _nodesCount; ++i)
                {
                    if (_initialGraphMatrix[vertex, i] >= maxEdgeWeight)
                    {
                        maxEdgeWeight = _initialGraphMatrix[vertex, i];
                        vertexWithMaxEdge = vertex;
                    }
                }
            }
        }

        _initialVertices.Remove(vertexWithMaxEdge);
        return vertexWithMaxEdge;
    }

    private double DivideGraph(int s, int t, List<int> blackList)
    {
        double minimumLenght = 0;
        Queue<int> trackedV = new Queue<int>(blackList);

        while (trackedV.Count != 0)
        {
            trackedV.Dequeue();
            var stCut = FindSTCut(trackedV.ToList());
            minimumLenght = minimumLenght > stCut ? stCut : minimumLenght;
        }
        
        trackedV = new Queue<int>(blackList);
        for (int i = 0; i < _solutionGraphMatrix.GetLength(0); ++i)
        {
            if(_solutionGraphMatrix[s, i] > 0 && i != t)
            {
                trackedV.Enqueue(i);
                var stCut = FindSTCut(trackedV.ToList());
                minimumLenght = minimumLenght > stCut ? stCut : minimumLenght;
            }
        }
        
        
        return minimumLenght;
    }

    private double FindSTCut(List<int> blackList)
    {
        double currentLenght = 0;
        foreach (var x in blackList)
        {
            for(int i = 0; i < _initialGraphMatrix.GetLength(1); ++i)
            {
                var lenght = _initialGraphMatrix[x, i];
                currentLenght += ((lenght > 0) && blackList.All(v => v != i)) ? lenght : 0;
            }
        }

        return currentLenght;
    }
    
    public void ShowIterations(StackPanel treePanel)
    {
        var foregroundColor = new SolidColorBrush(Color.FromRgb(102, 17, 153));
        
        CreateOutputString(treePanel, $"Iteration: {NodesCount - InitialVertices.Count}", foregroundColor, true);
        
        foreach (var list in Graph)
        {
            if (list.Count > 0)
            {
                var charList = list.Select(l => ((char)('a' + l)).ToString());
                var groupString = $"({string.Join("->", charList)})";
                CreateOutputString(treePanel, $"{groupString}", foregroundColor);
            }
            else
            {
                CreateOutputString(treePanel, "()", foregroundColor); // Handles empty lists
            }
        }
    }
    
    private void CreateOutputString(StackPanel stackPanel, string text,  Brush? color = null,  bool? header = false)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            Width = 750,
            Margin = header == true ? new Thickness(0, 20, 0, 0) : new Thickness(0),
            Foreground = color ?? Brushes.White,
            TextAlignment = TextAlignment.Center,
            FontSize = 16,
            FontFamily = new FontFamily("Courier New"),
            FontWeight = FontWeights.Bold
        };

        stackPanel.Children.Add(textBlock);
    }
}