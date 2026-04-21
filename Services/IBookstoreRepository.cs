using Aep.Api;
using Example.Bookstore.V1;
using Google.Protobuf.WellKnownTypes;

namespace bookstore.Services;

public interface IBookstoreRepository
{
    Publisher CreatePublisher(string? id, Publisher publisher);
    Publisher GetPublisher(string path);
    Publisher UpdatePublisher(string path, Publisher publisher, FieldMask? updateMask);
    void DeletePublisher(string path, bool force);
    (IReadOnlyList<Publisher> Results, string NextPageToken) ListPublishers(
        string? parent,
        string? pageToken,
        int maxPageSize,
        int skip,
        string? filter);
    Publisher ApplyPublisher(string path, Publisher publisher);

    Store CreateStore(string? id, Store store);
    Store GetStore(string path);
    Store UpdateStore(string path, Store store, FieldMask? updateMask);
    void DeleteStore(string path, bool force);
    (IReadOnlyList<Store> Results, string NextPageToken) ListStores(
        string? parent,
        string? pageToken,
        int maxPageSize,
        int skip,
        string? filter);

    Book CreateBook(string parent, string? id, Book book);
    Book GetBook(string path);
    Book UpdateBook(string path, Book book, FieldMask? updateMask);
    void DeleteBook(string path, bool force);
    (IReadOnlyList<Book> Results, string NextPageToken) ListBooks(string parent, string? pageToken, int maxPageSize);
    Book ApplyBook(string path, Book book);
    Operation ArchiveBook(string path);

    BookEdition CreateBookEdition(string parent, string? id, BookEdition bookEdition);
    BookEdition GetBookEdition(string path);
    void DeleteBookEdition(string path);
    (IReadOnlyList<BookEdition> Results, string NextPageToken) ListBookEditions(
        string parent,
        string? pageToken,
        int maxPageSize);

    Isbn CreateIsbn(string? id, Isbn isbn);
    Isbn GetIsbn(string path);
    (IReadOnlyList<Isbn> Results, string NextPageToken) ListIsbns(string? parent, string? pageToken, int maxPageSize);

    Item CreateItem(string parent, string? id, Item item);
    Item GetItem(string path);
    Item UpdateItem(string path, Item item, FieldMask? updateMask);
    void DeleteItem(string path);
    (IReadOnlyList<Item> Results, string NextPageToken) ListItems(
        string parent,
        string? pageToken,
        int maxPageSize,
        int skip,
        string? filter);
    Operation MoveItem(string path, string targetStore);
}
