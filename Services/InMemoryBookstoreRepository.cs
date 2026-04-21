using System.Globalization;
using Aep.Api;
using Example.Bookstore.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace bookstore.Services;

public sealed class InMemoryBookstoreRepository : IBookstoreRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<string, Publisher> _publishers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Store> _stores = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Book> _books = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BookEdition> _bookEditions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Isbn> _isbns = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Item> _items = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryBookstoreRepository()
    {
        var samplePublisher = new Publisher
        {
            Description = "Sample publisher",
            Path = "publishers/sample-publisher"
        };

        var sampleStore = new Store
        {
            Name = "Sample Store",
            Description = "Sample storefront",
            Path = "stores/sample-store"
        };

        _publishers[samplePublisher.Path] = samplePublisher;
        _stores[sampleStore.Path] = sampleStore;
    }

    public Publisher CreatePublisher(string? id, Publisher publisher)
    {
        lock (_gate)
        {
            var path = BuildPath("publishers", id, "publisher");
            EnsureDoesNotExist(_publishers, path, "publisher");

            var created = publisher.Clone();
            created.Path = path;
            _publishers[path] = created;

            return created.Clone();
        }
    }

    public Publisher GetPublisher(string path)
    {
        lock (_gate)
        {
            return GetRequired(_publishers, NormalizePath(path), "publisher").Clone();
        }
    }

    public Publisher UpdatePublisher(string path, Publisher publisher, FieldMask? updateMask)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            var current = GetRequired(_publishers, path, "publisher").Clone();

            if (ShouldUpdate(updateMask, "description"))
            {
                current.Description = publisher.Description;
            }

            current.Path = path;
            _publishers[path] = current;
            return current.Clone();
        }
    }

    public void DeletePublisher(string path, bool force)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            GetRequired(_publishers, path, "publisher");

            if (!force && _books.Keys.Any(key => key.StartsWith($"{path}/books/", StringComparison.OrdinalIgnoreCase)))
            {
                throw FailedPrecondition($"Publisher '{path}' still has books. Set force=true to delete it.");
            }

            _publishers.Remove(path);
        }
    }

    public (IReadOnlyList<Publisher> Results, string NextPageToken) ListPublishers(
        string? parent,
        string? pageToken,
        int maxPageSize,
        int skip,
        string? filter)
    {
        lock (_gate)
        {
            var query = _publishers.Values
                .Where(publisher => string.IsNullOrWhiteSpace(filter) ||
                                    publisher.Path.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                    publisher.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(publisher => publisher.Path)
                .Skip(Math.Max(skip, 0))
                .Select(publisher => publisher.Clone());

            return Page(query, pageToken, maxPageSize);
        }
    }

    public Publisher ApplyPublisher(string path, Publisher publisher)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            var applied = publisher.Clone();
            applied.Path = path;
            _publishers[path] = applied;

            return applied.Clone();
        }
    }

    public Store CreateStore(string? id, Store store)
    {
        lock (_gate)
        {
            var path = BuildPath("stores", id, "store");
            EnsureDoesNotExist(_stores, path, "store");

            var created = store.Clone();
            created.Path = path;
            _stores[path] = created;

            return created.Clone();
        }
    }

    public Store GetStore(string path)
    {
        lock (_gate)
        {
            return GetRequired(_stores, NormalizePath(path), "store").Clone();
        }
    }

    public Store UpdateStore(string path, Store store, FieldMask? updateMask)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            var current = GetRequired(_stores, path, "store").Clone();

            if (ShouldUpdate(updateMask, "name"))
            {
                current.Name = store.Name;
            }

            if (ShouldUpdate(updateMask, "description"))
            {
                current.Description = store.Description;
            }

            current.Path = path;
            _stores[path] = current;
            return current.Clone();
        }
    }

    public void DeleteStore(string path, bool force)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            GetRequired(_stores, path, "store");

            if (!force && _items.Keys.Any(key => key.StartsWith($"{path}/items/", StringComparison.OrdinalIgnoreCase)))
            {
                throw FailedPrecondition($"Store '{path}' still has items. Set force=true to delete it.");
            }

            _stores.Remove(path);
        }
    }

    public (IReadOnlyList<Store> Results, string NextPageToken) ListStores(
        string? parent,
        string? pageToken,
        int maxPageSize,
        int skip,
        string? filter)
    {
        lock (_gate)
        {
            var query = _stores.Values
                .Where(store => string.IsNullOrWhiteSpace(filter) ||
                                store.Path.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                store.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                store.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(store => store.Path)
                .Skip(Math.Max(skip, 0))
                .Select(store => store.Clone());

            return Page(query, pageToken, maxPageSize);
        }
    }

    public Book CreateBook(string parent, string? id, Book book)
    {
        lock (_gate)
        {
            parent = NormalizePath(parent);
            GetRequired(_publishers, parent, "publisher");

            var path = $"{parent}/books/{NormalizeId(id, "book")}";
            EnsureDoesNotExist(_books, path, "book");

            var created = book.Clone();
            created.Path = path;
            _books[path] = created;

            return created.Clone();
        }
    }

    public Book GetBook(string path)
    {
        lock (_gate)
        {
            return GetRequired(_books, NormalizePath(path), "book").Clone();
        }
    }

    public Book UpdateBook(string path, Book book, FieldMask? updateMask)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            var current = GetRequired(_books, path, "book").Clone();

            if (ShouldUpdate(updateMask, "isbn"))
            {
                current.Isbn.Clear();
                current.Isbn.Add(book.Isbn);
            }

            if (ShouldUpdate(updateMask, "price"))
            {
                current.Price = book.Price;
            }

            if (ShouldUpdate(updateMask, "published"))
            {
                current.Published = book.Published;
            }

            if (ShouldUpdate(updateMask, "edition"))
            {
                current.Edition = book.Edition;
            }

            if (ShouldUpdate(updateMask, "author"))
            {
                current.Author.Clear();
                current.Author.Add(book.Author.Select(author => author.Clone()));
            }

            current.Path = path;
            _books[path] = current;
            return current.Clone();
        }
    }

    public void DeleteBook(string path, bool force)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            GetRequired(_books, path, "book");

            if (!force && _bookEditions.Keys.Any(key => key.StartsWith($"{path}/editions/", StringComparison.OrdinalIgnoreCase)))
            {
                throw FailedPrecondition($"Book '{path}' still has editions. Set force=true to delete it.");
            }

            _books.Remove(path);
        }
    }

    public (IReadOnlyList<Book> Results, string NextPageToken) ListBooks(string parent, string? pageToken, int maxPageSize)
    {
        lock (_gate)
        {
            parent = NormalizePath(parent);
            var query = _books.Values
                .Where(book => string.IsNullOrWhiteSpace(parent) || book.Path.StartsWith($"{parent}/books/", StringComparison.OrdinalIgnoreCase))
                .OrderBy(book => book.Path)
                .Select(book => book.Clone());

            return Page(query, pageToken, maxPageSize);
        }
    }

    public Book ApplyBook(string path, Book book)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            EnsureParentPublisherExists(path);

            var applied = book.Clone();
            applied.Path = path;
            _books[path] = applied;

            return applied.Clone();
        }
    }

    public Operation ArchiveBook(string path)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            var book = GetRequired(_books, path, "book").Clone();
            book.Published = false;
            _books[path] = book;

            return CreateCompletedOperation($"{path}:archive", new ArchiveBookResponse(), "Book archived.");
        }
    }

    public BookEdition CreateBookEdition(string parent, string? id, BookEdition bookEdition)
    {
        lock (_gate)
        {
            parent = NormalizePath(parent);
            GetRequired(_books, parent, "book");

            var path = $"{parent}/editions/{NormalizeId(id, "edition")}";
            EnsureDoesNotExist(_bookEditions, path, "book edition");

            var created = bookEdition.Clone();
            created.Path = path;
            _bookEditions[path] = created;

            return created.Clone();
        }
    }

    public BookEdition GetBookEdition(string path)
    {
        lock (_gate)
        {
            return GetRequired(_bookEditions, NormalizePath(path), "book edition").Clone();
        }
    }

    public void DeleteBookEdition(string path)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            GetRequired(_bookEditions, path, "book edition");
            _bookEditions.Remove(path);
        }
    }

    public (IReadOnlyList<BookEdition> Results, string NextPageToken) ListBookEditions(
        string parent,
        string? pageToken,
        int maxPageSize)
    {
        lock (_gate)
        {
            parent = NormalizePath(parent);
            var query = _bookEditions.Values
                .Where(edition => string.IsNullOrWhiteSpace(parent) || edition.Path.StartsWith($"{parent}/editions/", StringComparison.OrdinalIgnoreCase))
                .OrderBy(edition => edition.Path)
                .Select(edition => edition.Clone());

            return Page(query, pageToken, maxPageSize);
        }
    }

    public Isbn CreateIsbn(string? id, Isbn isbn)
    {
        lock (_gate)
        {
            var path = BuildPath("isbns", id, "isbn");
            EnsureDoesNotExist(_isbns, path, "isbn");

            var created = isbn.Clone();
            created.Path = path;
            _isbns[path] = created;

            return created.Clone();
        }
    }

    public Isbn GetIsbn(string path)
    {
        lock (_gate)
        {
            return GetRequired(_isbns, NormalizePath(path), "isbn").Clone();
        }
    }

    public (IReadOnlyList<Isbn> Results, string NextPageToken) ListIsbns(string? parent, string? pageToken, int maxPageSize)
    {
        lock (_gate)
        {
            var query = _isbns.Values
                .OrderBy(isbn => isbn.Path)
                .Select(isbn => isbn.Clone());

            return Page(query, pageToken, maxPageSize);
        }
    }

    public Item CreateItem(string parent, string? id, Item item)
    {
        lock (_gate)
        {
            parent = NormalizePath(parent);
            GetRequired(_stores, parent, "store");

            if (!string.IsNullOrWhiteSpace(item.Book) && !_books.ContainsKey(NormalizePath(item.Book)))
            {
                throw NotFound("book", item.Book);
            }

            var path = $"{parent}/items/{NormalizeId(id, "item")}";
            EnsureDoesNotExist(_items, path, "item");

            var created = item.Clone();
            created.Path = path;
            _items[path] = created;

            return created.Clone();
        }
    }

    public Item GetItem(string path)
    {
        lock (_gate)
        {
            return GetRequired(_items, NormalizePath(path), "item").Clone();
        }
    }

    public Item UpdateItem(string path, Item item, FieldMask? updateMask)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            var current = GetRequired(_items, path, "item").Clone();

            if (ShouldUpdate(updateMask, "book"))
            {
                if (!string.IsNullOrWhiteSpace(item.Book) && !_books.ContainsKey(NormalizePath(item.Book)))
                {
                    throw NotFound("book", item.Book);
                }

                current.Book = item.Book;
            }

            if (ShouldUpdate(updateMask, "condition"))
            {
                current.Condition = item.Condition;
            }

            if (ShouldUpdate(updateMask, "price"))
            {
                current.Price = item.Price;
            }

            current.Path = path;
            _items[path] = current;
            return current.Clone();
        }
    }

    public void DeleteItem(string path)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            GetRequired(_items, path, "item");
            _items.Remove(path);
        }
    }

    public (IReadOnlyList<Item> Results, string NextPageToken) ListItems(
        string parent,
        string? pageToken,
        int maxPageSize,
        int skip,
        string? filter)
    {
        lock (_gate)
        {
            parent = NormalizePath(parent);
            var query = _items.Values
                .Where(item => string.IsNullOrWhiteSpace(parent) || item.Path.StartsWith($"{parent}/items/", StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(filter) ||
                               item.Path.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                               item.Book.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                               item.Condition.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Path)
                .Skip(Math.Max(skip, 0))
                .Select(item => item.Clone());

            return Page(query, pageToken, maxPageSize);
        }
    }

    public Operation MoveItem(string path, string targetStore)
    {
        lock (_gate)
        {
            path = NormalizePath(path);
            targetStore = NormalizePath(targetStore);

            var item = GetRequired(_items, path, "item").Clone();
            GetRequired(_stores, targetStore, "store");

            var newPath = $"{targetStore}/items/{GetIdFromPath(path)}";
            if (!path.Equals(newPath, StringComparison.OrdinalIgnoreCase))
            {
                EnsureDoesNotExist(_items, newPath, "item");
                _items.Remove(path);
            }

            item.Path = newPath;
            _items[newPath] = item;

            return CreateCompletedOperation($"{newPath}:move", new Empty(), "Item moved.");
        }
    }

    private static (IReadOnlyList<T> Results, string NextPageToken) Page<T>(
        IEnumerable<T> source,
        string? pageToken,
        int maxPageSize)
    {
        var list = source.ToList();
        var offset = ParsePageToken(pageToken);
        var take = maxPageSize > 0 ? maxPageSize : 50;

        var page = list.Skip(offset).Take(take).ToList();
        var nextOffset = offset + page.Count;
        var nextPageToken = nextOffset < list.Count ? nextOffset.ToString(CultureInfo.InvariantCulture) : string.Empty;

        return (page, nextPageToken);
    }

    private static int ParsePageToken(string? pageToken)
    {
        if (string.IsNullOrWhiteSpace(pageToken))
        {
            return 0;
        }

        if (int.TryParse(pageToken, NumberStyles.None, CultureInfo.InvariantCulture, out var offset) && offset >= 0)
        {
            return offset;
        }

        throw InvalidArgument("page_token must be a non-negative integer.");
    }

    private static bool ShouldUpdate(FieldMask? updateMask, params string[] fields)
    {
        if (updateMask is null || updateMask.Paths.Count == 0)
        {
            return true;
        }

        var normalizedFields = fields
            .Select(field => field.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return updateMask.Paths.Any(path =>
        {
            var lastSegment = path.Split('.').LastOrDefault() ?? string.Empty;
            var normalizedPath = lastSegment.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
            return normalizedFields.Contains(normalizedPath);
        });
    }

    private static string BuildPath(string collection, string? id, string prefix)
        => $"{collection}/{NormalizeId(id, prefix)}";

    private static string NormalizeId(string? id, string prefix)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return $"{prefix}-{Guid.NewGuid():N}"[..(prefix.Length + 9)];
        }

        return id.Trim().Replace(' ', '-').ToLowerInvariant();
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.Trim().Trim('/');
    }

    private static string GetIdFromPath(string path)
        => path[(path.LastIndexOf('/') + 1)..];

    private void EnsureParentPublisherExists(string bookPath)
    {
        var segments = NormalizePath(bookPath).Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 4)
        {
            throw InvalidArgument("Book path must match publishers/{publisher}/books/{book}.");
        }

        var publisherPath = string.Join('/', segments.Take(2));
        GetRequired(_publishers, publisherPath, "publisher");
    }

    private static T GetRequired<T>(IDictionary<string, T> source, string path, string resourceType)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw InvalidArgument($"{resourceType} path is required.");
        }

        if (!source.TryGetValue(path, out var value))
        {
            throw NotFound(resourceType, path);
        }

        return value;
    }

    private static void EnsureDoesNotExist<T>(IDictionary<string, T> source, string path, string resourceType)
    {
        if (source.ContainsKey(path))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"{resourceType} '{path}' already exists."));
        }
    }

    private static RpcException NotFound(string resourceType, string path)
        => new(new Status(StatusCode.NotFound, $"{resourceType} '{path}' was not found."));

    private static RpcException FailedPrecondition(string message)
        => new(new Status(StatusCode.FailedPrecondition, message));

    private static RpcException InvalidArgument(string message)
        => new(new Status(StatusCode.InvalidArgument, message));

    private static Operation CreateCompletedOperation(string name, IMessage response, string message)
        => new()
        {
            Name = name,
            Done = true,
            Response = Any.Pack(response),
            Message = message
        };
}
