namespace Cike.EntityFrameworkCore.Stores;

public class BookmarkQueueItemStore(CikeWorkflowDbContenxt context)
    : BaseStore<BookmarkQueueItem>(context), IBookmarkQueueItemStore;
