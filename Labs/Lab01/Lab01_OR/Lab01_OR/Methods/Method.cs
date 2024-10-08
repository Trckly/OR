namespace Lab01_OR.Methods;

public struct Table
{
    public double[,] constraints;
    public double[] cb;
    public Dictionary<int, double> plan;
    public double[] deriv;
    public double[] delta;
    public int[] myBase;

    public Table(double[,] constraints, double[] cb, Dictionary<int, double> plan, double[] deriv, double[] delta, int[] myBase)
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
    
    protected double[] _objectiveFunction;
    protected double[,] _constraints;
    protected string[] _inequalities;
    protected double[] _delta;
    protected double[] _cb;
    protected Dictionary<int, double> _plan;
    protected double[] _deriv;
    protected double[] _results;
    protected int[] _base;
    protected MainWindow _mainWindow;
    
    public abstract double[] Solve();
    
    protected void FindDelta()
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

    protected void RebuildTable(int minIndexRow, int minIndexColumn)
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

    protected abstract bool CheckIfSolved();
}