namespace Lab01_OR.Methods;

public class CuttingPlane : Method
{
    private Table _table;
    private int _realVariablesCount;
    public CuttingPlane(decimal[] objectiveFunction, decimal[,] constraints, string[] inequalities, decimal[] results,
        MainWindow mainWindow)
    {
        _objectiveFunction = objectiveFunction;
        _constraints = constraints;
        _inequalities = inequalities;
        _results = results;
        _mainWindow = mainWindow;
        _realVariablesCount = _objectiveFunction.GetLength(0);
    }

    public override decimal[] Solve()
    {
        var simplex = new Simplex(_objectiveFunction, _constraints, _inequalities, _results, _mainWindow);
        var result = simplex.Solve();
        var table = simplex.GetTable();
        _plan = new Dictionary<int, decimal>(table.plan);
        _constraints = table.constraints;
        _cb = table.cb;
        _deriv = table.deriv;
        _delta = table.delta;
        _base = table.myBase;

        _table = table;
        return IterateByDualSimplex(result);
    }

    private decimal[] IterateByDualSimplex(decimal[] result)
    {
        if (CheckIfSolved())
        {
            for(int i = 0; i < _realVariablesCount; i++)
            {
                result[i] = Convert.ToInt32(result[i]);
            }
            result[^1] = Convert.ToInt32(result[^1]);
            return result;
        }
        
        var planFromResult = result.Select(x => x - Math.Truncate(x)).ToArray();
        decimal max = 0;
        int index = -1;
        for (int i = 0; i < _base.Length - 1; i++)
        {
            if (planFromResult[_base[i]] > max)
            {
                max = planFromResult[_base[i]];
                index = i;
            }
        }
        
        decimal[,] newConstraints = new decimal[_table.constraints.GetLength(0)+1, _table.constraints.GetLength(1)+1];
        for (int i = 0; i < _table.constraints.GetLength(0); i++)
        {
            for (int j = 0; j < _table.constraints.GetLength(1); j++)
            {
                newConstraints[i, j] = _table.constraints[i, j];
            }

            newConstraints[i, _table.constraints.GetLength(1)] = 0;
        }

        for (int i = 0; i < _table.constraints.GetLength(0); i++)
        {
            newConstraints[newConstraints.GetLength(0) - 1, i] = 0;
        }
        for (int i = _table.constraints.GetLength(0); i < _table.constraints.GetLength(1); i++)
        {
            decimal constraint = _table.constraints[index, i];
            if (constraint >= 0.0000001M)
            {
                newConstraints[newConstraints.GetLength(0) - 1, i] = -constraint;
            }
            else
            {
                newConstraints[newConstraints.GetLength(0) - 1, i] = -(max / (1 - max) * Math.Abs(constraint));
            }
        }
        
        newConstraints[newConstraints.GetLength(0) - 1, _table.constraints.GetLength(1)] = 1;
        
        var newRes = new decimal[_results.GetLength(0) + 1];
        for (int i = 0; i < _results.GetLength(0); i++)
        {
            newRes[i] = _results[i];
        }
        newRes[_results.GetLength(0)] = -max;

        _constraints = newConstraints;
        _results = newRes;
        var newObjectiveFunction = new decimal[_objectiveFunction.GetLength(0) + 1];
        for (int i = 0; i < _objectiveFunction.GetLength(0); i++)
        {
            newObjectiveFunction[i] = _objectiveFunction[i];
        }
        newObjectiveFunction[_objectiveFunction.GetLength(0)] = 0;
        _objectiveFunction = newObjectiveFunction;
        
        var newCb = new decimal[_cb.GetLength(0) + 1];
        for (int i = 0; i < _cb.GetLength(0); i++)
        {
            newCb[i] = _cb[i];
        }
        newCb[_cb.GetLength(0)] = 0;
        _cb = newCb;
        
        var newBase = new int[_base.GetLength(0) + 1];
        for (int i = 0; i < _base.GetLength(0); i++)
        {
            newBase[i] = _base[i];
        }
        newBase[_base.GetLength(0)] = _constraints.GetLength(1)-1;
        _base = newBase;
        _plan.Add(_base[_base.GetLength(0) - 1], -max);
        
        var newDelta = new decimal[_delta.GetLength(0) + 1];
        for (int i = 0; i < _delta.GetLength(0); i++)
        {
            newDelta[i] = _delta[i];
        }
        newDelta[_delta.GetLength(0)] = 0;
        _delta = newDelta;
        
        _deriv = new decimal[_delta.GetLength(0)];

        Table newTable = new Table()
        {
            constraints = _constraints,
            cb = _cb,
            plan = _plan,
            delta = _delta,
            deriv = _deriv,
            myBase = _base,
        };

        var dual = new DualSimplex(_objectiveFunction, _results, _mainWindow, newTable);
        var finalResult = dual.Solve();
        
        var table = dual.GetTable();
        _plan = new Dictionary<int, decimal>(table.plan);
        _constraints = table.constraints;
        _cb = table.cb;
        _deriv = table.deriv;
        _delta = table.delta;
        _base = table.myBase;

        _table = table;

        return IterateByDualSimplex(finalResult);
    }

    protected override bool CheckIfSolved()
    {
        List<decimal> results = new List<decimal>();
        for (int i = 0; i < _realVariablesCount; i++)
        {
            results.Add(_plan[_base[i]]);
        }

        return results.All(x => x % 1 is > 0.99999999M or < 0.00000001M);
    }
}