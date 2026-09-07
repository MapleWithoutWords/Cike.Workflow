namespace Cike.EntityFrameworkCore.Stores;

public class BookmarkStore(CikeWorkflowDbContenxt context)
    : BaseStore<BookmarkEntity>(context), IBookmarkStore;
