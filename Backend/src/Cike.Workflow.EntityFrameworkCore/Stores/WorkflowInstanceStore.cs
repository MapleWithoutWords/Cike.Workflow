namespace Cike.EntityFrameworkCore.Stores;

public class WorkflowInstanceStore(CikeWorkflowDbContenxt context)
    : BaseStore<WorkflowInstance>(context), IWorkflowInstanceStore;
