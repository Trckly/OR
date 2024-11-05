namespace GameTheoryAlgorithm.Methods;

public struct Table
{
    public decimal[,] constraints;
    public decimal[] cb;
    public Dictionary<int, decimal> plan;
    public decimal[] deriv;
    public decimal[] delta;
    public int[] myBase;

    public Table(decimal[,] constraints, decimal[] cb, Dictionary<int, decimal> plan, decimal[] deriv, decimal[] delta, int[] myBase)
    {
        this.constraints = constraints;
        this.cb = cb;
        this.plan = plan;
        this.deriv = deriv;
        this.delta = delta;
        this.myBase = myBase;
    }
}
public abstract class Method
{
    
    protected decimal[] _objectiveFunction;
    protected decimal[,] _constraints;
    protected string[] _inequalities;
    protected decimal[] _delta;
    protected decimal[] _cb;
    protected Dictionary<int, decimal> _plan;
    protected decimal[] _deriv;
    protected decimal[] _results;
    protected int[] _base;
    protected MainWindow _mainWindow;
    
    public abstract decimal[] Solve();
    
    protected void FindDelta()
    {
        decimal[] newDelta = new decimal[_delta.Length];

        for (int i = 0; i < newDelta.Length; i++)
        {
            decimal localDelta = 0;
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

    protected void RebuildTable(int minIndexRow, int minIndexColumn)
    {
        decimal rate = _constraints[minIndexRow, minIndexColumn];

        if (minIndexColumn < _objectiveFunction.Length)
        {
            _cb[minIndexRow] = _objectiveFunction[minIndexColumn];
        }

        Dictionary<int, decimal> newPlan = _plan.ToDictionary();
        decimal[,] newConstraints = new decimal[_constraints.GetLength(0), _constraints.GetLength(1)];

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

    protected abstract bool CheckIfSolved();
}