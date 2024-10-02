namespace Lab01_OR;

public class DualSimplex : IMethod
{
    private double[] _objectiveFunction;
    private double[,] _constraints;
    private string[] _inequalities;
    private double[] _delta;
    private double[] _cb;
    private double[] _deriv;
    private Dictionary<int, double> _plan;
    private double[] _results;
    private int[] _base;
    MainWindow _mainWindow;

    public DualSimplex(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results,
        MainWindow mainWindow)
    {
        for (int i = 0; i < inequalities.Length; i++)
        {
            if (inequalities[i] == ">=")
            {
                for (int j = 0; j < constraints.GetLength(0); j++)
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
        
        this._constraints = new double[constraints.GetLength(0), constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            for (int j = 0; j < constraints.GetLength(0); j++)
            {
                _constraints[i, j] = constraints[i, j];
            }
        }

        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            _constraints[i, constraints.GetLength(0) + i] = 1;
        }

        this._delta = new double[results.Length + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _delta[i] = -objectiveFunction[i];
        }

        _cb = new double[results.Length];
        _deriv = new double[_delta.Length];

        this._inequalities = inequalities;
        this._plan = new Dictionary<int, double>();
        this._base = new int[results.Length];
        for (int i = objectiveFunction.Length; i < _delta.Length; i++)
        {
            _plan.Add(i, results[i - objectiveFunction.Length]);
            _base[i - objectiveFunction.Length] = i;
        }
        
    }

    // Check whether to use the primal or dual simplex method based on inequalities
    public double[] Solve()
    {
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridDualSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            
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

        double min = 0;
        int minIndexRow = 0;
        for (int i = 0; i < _base.Length; i++)
        {
            if (_plan.GetValueOrDefault(_base[i]) < min)
            {
                min = _plan.GetValueOrDefault(_base[i]);
                minIndexRow = i;
            }
        }

        for (int i = 0; i < _deriv.Length; i++)
        {
            _deriv[i] = -_delta[i] / _constraints[minIndexRow, i];
        }

        min = Double.MaxValue;
        int minIndexColumn = 0;
        for (int i = 0; i < _deriv.Length; i++)
        {
            if (_deriv[i] < min && _deriv[i] > 0)
            {
                min = _deriv[i];
                minIndexColumn = i;
            }
        }

        _mainWindow.CreateAndAddDynamicGridDualSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);

        RebuildTable(minIndexRow, minIndexColumn);
        FindDelta();

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
                transposedConstraints[j, i] = -_constraints[i, j];
            }
        }

        _constraints = transposedConstraints;

        // Swap the objective function with the results array
        (_objectiveFunction, _results) = (_results.Select(x => -x).ToArray(), _objectiveFunction.Select(x => -x).ToArray());

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

            if (i >= _objectiveFunction.Length)
            {
                newDelta[i] = localDelta;
            }
            else
            {
                newDelta[i] = localDelta - _objectiveFunction[i];
            }
        }

        _delta = newDelta;
    }

    private void RebuildTable(int minIndexRow, int minIndexColumn)
    {
        double rate = _constraints[minIndexRow, minIndexColumn];

        if (minIndexColumn < _objectiveFunction.Length)
        {
            _cb[minIndexRow] = _objectiveFunction[minIndexColumn];
        }

        Dictionary<int, double> newPlan = _plan.ToDictionary();
        double[,] newConstraints = new double[_constraints.GetLength(0), _constraints.GetLength(1)];

        newPlan.Add(minIndexColumn, _plan[_base[minIndexRow]] / rate);
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
                    _plan[_base[i]] - _constraints[i, minIndexColumn] * _plan[_base[minIndexRow]] / rate;

                for (int j = 0; j < _delta.Length; j++)
                {
                    if (j != minIndexColumn)
                    {
                        newConstraints[i, j] = _constraints[i, j] -
                                               _constraints[minIndexRow, j] * _constraints[i, minIndexColumn] / rate;
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
        _base[minIndexRow] = minIndexColumn;

    }

    private bool CheckIfSolved()
    {
        return !_plan.Values.Any(x => x < 0);
    }
}