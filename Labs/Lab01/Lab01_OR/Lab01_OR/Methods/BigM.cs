namespace Lab01_OR.Methods;

public class BigM : Method
{
    private decimal[] _baseCoefficient;

    public BigM(decimal[] objectiveFunction, decimal[,] constraints, string[] inequalities, decimal[] results,
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
    public void Initialize(decimal[] objectiveFunction, decimal[,] constraints, string[] inequalities, decimal[] results)
    {
        
        this._constraints = new decimal[constraints.GetLength(0), 2 * constraints.GetLength(0) + objectiveFunction.Length];
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
        _baseCoefficient = new decimal[2 * constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _baseCoefficient[i] = objectiveFunction[i];
        }

        for (int i = _constraints.GetLength(1) - results.Length; i < _constraints.GetLength(1); i++)
        {
            _baseCoefficient[i] = 1000000000;
        }

        this._delta = new decimal[2 * results.Length + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _delta[i] = -objectiveFunction[i];
        }

        _cb = new decimal[results.Length];
        for (int i = 0; i < _cb.Length; i++)
        {
            _cb[i] = 1000000000;
        }
        _deriv = new decimal[_constraints.GetLength(0)];

        this._inequalities = inequalities;
        this._plan = new Dictionary<int, decimal>();
        this._base = new int[results.Length];
        for (int i = _constraints.GetLength(1) - results.Length; i < _constraints.GetLength(1); i++)
        {
            _plan.Add(i, results[i - (_constraints.GetLength(1) - results.Length)]);
            _base[i - (_constraints.GetLength(1) - results.Length)] = i;
        }
        
    }

    // Check whether to use the primal or dual simplex method based on inequalities
    public override decimal[] Solve()
    {
        FindDelta();
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            
            decimal[] result = new decimal[_delta.Length + 1];
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

        decimal max = 0;
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
        

        var min = decimal.MaxValue;
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
        decimal[,] transposedConstraints = new decimal[_constraints.GetLength(1), _constraints.GetLength(0)];
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


    private new void FindDelta()
    {
        decimal[] newDelta = new decimal[_delta.Length];

        for (int i = 0; i < newDelta.Length; i++)
        {
            decimal localDelta = 0;
            for (int j = 0; j < _base.Length; j++)
            {
                localDelta += _cb[j] * _constraints[j, i];
            }

            newDelta[i] = localDelta - _baseCoefficient[i];
        }

        _delta = newDelta;
    }

    private new void RebuildTable(int minIndexRow, int maxIndexColumn)
    {
        decimal rate = _constraints[minIndexRow, maxIndexColumn];

            _cb[minIndexRow] = _baseCoefficient[maxIndexColumn];

        Dictionary<int, decimal> newPlan = _plan.ToDictionary();
        decimal[,] newConstraints = new decimal[_constraints.GetLength(0), _constraints.GetLength(1)];

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

    protected override bool CheckIfSolved()
    {
        return !_delta.Any(x => x > 0.000001M);
    }
}