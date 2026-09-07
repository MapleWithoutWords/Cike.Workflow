namespace Cike.EntityFrameworkCore.Stores;

public class WorkflowDefinitionStore(CikeWorkflowDbContenxt context)
    : BaseStore<WorkflowDefinition>(context), IWorkflowDefinitionStore;
