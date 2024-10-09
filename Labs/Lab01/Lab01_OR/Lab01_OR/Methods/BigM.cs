namespace Lab01_OR.Methods;

public class BigM : IMethod
{
    private double[] _objectiveFunction;
    private double[] _baseCoefficient;
    private double[,] _constraints;
    private string[] _inequalities;
    private double[] _delta;
    private double[] _cb;
    private double[] _deriv;
    private Dictionary<int, double> _plan;
    private double[] _results;
    private int[] _base;
    MainWindow _mainWindow;

    public BigM(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results,
        MainWindow mainWindow)
    {
        for (int i = 0; i < inequalities.Length; i++)
        {
            if (inequalities[i] == ">=")
            {
                for (int j = 0; j < constraints.GetLength(1); j++)
                {
                    constraints[i, j] = -constraints[i, j];
                }
                
                results[i] = -results[i];
            }
        }
        
        _mainWindow = mainWindow;
        _objectiveFunction = objectiveFunction;
        _constraints = constraints;
        _inequalities = inequalities;
        _results = results;

        ApplyDualSimplexTransform();
        Initialize(_objectiveFunction, _constraints, _inequalities, _results);
    }
    public void Initialize(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results)
    {
        
        this._constraints = new double[constraints.GetLength(0), 2 * constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            for (int j = 0; j < constraints.GetLength(1); j++)
            {
                _constraints[i, j] = constraints[i, j];
            }
        }

        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            _constraints[i, constraints.GetLength(1) + i] = -1;
            _constraints[i, constraints.GetLength(1) + i + constraints.GetLength(0)] = 1;
        }
        _baseCoefficient = new double[2 * constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _baseCoefficient[i] = objectiveFunction[i];
        }

        for (int i = _constraints.GetLength(1) - results.Length; i < _constraints.GetLength(1); i++)
        {
            _baseCoefficient[i] = 1000000000;
        }

        this._delta = new double[2 * results.Length + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _delta[i] = -objectiveFunction[i];
        }

        _cb = new double[results.Length];
        for (int i = 0; i < _cb.Length; i++)
        {
            _cb[i] = 1000000000;
        }
        _deriv = new double[_constraints.GetLength(0)];

        this._inequalities = inequalities;
        this._plan = new Dictionary<int, double>();
        this._base = new int[results.Length];
        for (int i = _constraints.GetLength(1) - results.Length; i < _constraints.GetLength(1); i++)
        {
            _plan.Add(i, results[i - (_constraints.GetLength(1) - results.Length)]);
            _base[i - (_constraints.GetLength(1) - results.Length)] = i;
        }
        
    }

    // Check whether to use the primal or dual simplex method based on inequalities
    public double[] Solve()
    {
        FindDelta();
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            
            double[] result = new double[_delta.Length + 1];
            for (int i = 0; i < _delta.Length; i++)
            {
                result[i] = _plan.GetValueOrDefault(i);
            }

            for (int i = 0; i < _base.Length; i++)
            {
                result[result.Length - 1] += _plan.GetValueOrDefault(_base[i]) * _cb[i];
            }

            return result;
        }

        double max = 0;
        int maxIndexColumn = 0;
        for (int i = 0; i < _delta.Length; i++)
        {
            if (_delta[i] > max)
            {
                max = _delta[i];
                maxIndexColumn = i;
            }
        }

        for (int i = 0; i < _deriv.Length; i++)
        {
            _deriv[i] = _plan.GetValueOrDefault(_base[i]) / _constraints[i, maxIndexColumn];
        }
        

        var min = Double.MaxValue;
        int minIndexRow = 0;
        for (int i = 0; i < _deriv.Length; i++)
        {
            if (_deriv[i] < min && _deriv[i] > 0)
            {
                min = _deriv[i];
                minIndexRow = i;
            }
        }
        
        

        _mainWindow.CreateAndAddDynamicGridSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);

        RebuildTable(minIndexRow, maxIndexColumn);

        return Solve();
    }
    
    // Transform the problem to use dual Simplex
    private void ApplyDualSimplexTransform()
    {
        // Transpose the constraints array
        double[,] transposedConstraints = new double[_constraints.GetLength(1), _constraints.GetLength(0)];
        for (int i = 0; i < _constraints.GetLength(0); i++)
        {
            for (int j = 0; j < _constraints.GetLength(1); j++)
            {
                transposedConstraints[j, i] = _constraints[i, j];
            }
        }

        _constraints = transposedConstraints;

        // Swap the objective function with the results array
        (_objectiveFunction, _results) = (_results, _objectiveFunction);

    }


    private void FindDelta()
    {
        double[] newDelta = new double[_delta.Length];

        for (int i = 0; i < newDelta.Length; i++)
        {
            double localDelta = 0;
            for (int j = 0; j < _base.Length; j++)
            {
                localDelta += _cb[j] * _constraints[j, i];
            }

            newDelta[i] = localDelta - _baseCoefficient[i];
        }

        _delta = newDelta;
    }

    private void RebuildTable(int minIndexRow, int maxIndexColumn)
    {
        double rate = _constraints[minIndexRow, maxIndexColumn];

            _cb[minIndexRow] = _baseCoefficient[maxIndexColumn];

        Dictionary<int, double> newPlan = _plan.ToDictionary();
        double[,] newConstraints = new double[_constraints.GetLength(0), _constraints.GetLength(1)];

        newPlan.Add(maxIndexColumn, _plan[_base[minIndexRow]] / rate);
        newPlan.Remove(_base[minIndexRow]);

        for (int i = 0; i < _delta.Length; i++)
        {
            newConstraints[minIndexRow, i] = _constraints[minIndexRow, i] / rate;
        }

        for (int i = 0; i < _base.Length; i++)
        {
            if (i != minIndexRow)
            {
                newPlan[_base[i]] =
                    _plan[_base[i]] - _constraints[i, maxIndexColumn] * _plan[_base[minIndexRow]] / rate;

                for (int j = 0; j < _delta.Length; j++)
                {
                    if (j != maxIndexColumn)
                    {
                        newConstraints[i, j] = _constraints[i, j] -
                                               _constraints[minIndexRow, j] * _constraints[i, maxIndexColumn] / rate;
                    }
                    else
                    {
                        newConstraints[i, j] = 0;
                    }
                }
            }
        }

        _constraints = newConstraints;
        _plan = newPlan;
        _base[minIndexRow] = maxIndexColumn;

    }

    private bool CheckIfSolved()
    {
        return !_delta.Any(x => x > 0.000001);
    }
}