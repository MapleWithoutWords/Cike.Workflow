namespace Cike.EntityFrameworkCore.Stores;

public class FolderStore(CikeWorkflowDbContenxt context, ICacheService<FolderCacheModel> cacheService)
    : BaseStore<Folder, FolderCacheModel>(context, cacheService), IFolderStore;
