namespace Cike.Workflow.Core.Activities;

public interface IVariableContainer : IActivity
{
    /// <summary>
    /// A collection of variables within the scope of the variable container.
    /// </summary>
    ICollection<Variable> Variables { get; }
}
