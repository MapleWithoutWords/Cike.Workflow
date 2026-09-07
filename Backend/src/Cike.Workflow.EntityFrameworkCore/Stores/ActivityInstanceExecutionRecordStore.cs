namespace Cike.EntityFrameworkCore.Stores;

public class ActivityInstanceExecutionRecordStore(CikeWorkflowDbContenxt context)
    : BaseStore<ActivityInstanceExecutionRecord>(context), IActivityInstanceExecutionRecordStore;
