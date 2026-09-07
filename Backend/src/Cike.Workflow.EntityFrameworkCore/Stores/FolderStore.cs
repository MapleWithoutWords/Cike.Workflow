namespace Cike.EntityFrameworkCore.Stores;

public class FolderStore(CikeWorkflowDbContenxt context)
    : BaseStore<Folder>(context), IFolderStore;
