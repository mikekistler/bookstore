using Aep.Api;
using Example.Bookstore.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace bookstore.Services;

/// <summary>
/// Implements the Bookstore gRPC contract and exposes the same operations through JSON transcoding.
/// </summary>
public sealed class BookstoreService(IBookstoreRepository repository) : Example.Bookstore.V1.Bookstore.BookstoreBase
{
    public override Task<Book> CreateBook(CreateBookRequest request, ServerCallContext context)
        => Task.FromResult(repository.CreateBook(request.Parent, request.Id, request.Book ?? new Book()));

    public override Task<Book> GetBook(GetBookRequest request, ServerCallContext context)
        => Task.FromResult(repository.GetBook(request.Path));

    public override Task<Book> UpdateBook(UpdateBookRequest request, ServerCallContext context)
        => Task.FromResult(repository.UpdateBook(request.Path, request.Book ?? new Book(), request.UpdateMask));

    public override Task<Empty> DeleteBook(DeleteBookRequest request, ServerCallContext context)
    {
        repository.DeleteBook(request.Path, request.Force);
        return Task.FromResult(new Empty());
    }

    public override Task<ListBooksResponse> ListBooks(ListBooksRequest request, ServerCallContext context)
    {
        var (results, nextPageToken) = repository.ListBooks(request.Parent, request.PageToken, request.MaxPageSize);
        var response = new ListBooksResponse { NextPageToken = nextPageToken };
        response.Results.Add(results);
        return Task.FromResult(response);
    }

    public override Task<Book> ApplyBook(ApplyBookRequest request, ServerCallContext context)
        => Task.FromResult(repository.ApplyBook(request.Path, request.Book ?? new Book()));

    public override Task<Operation> ArchiveBook(ArchiveBookRequest request, ServerCallContext context)
        => Task.FromResult(repository.ArchiveBook(request.Path));

    public override Task<BookEdition> CreateBookEdition(CreateBookEditionRequest request, ServerCallContext context)
        => Task.FromResult(repository.CreateBookEdition(request.Parent, request.Id, request.BookEdition ?? new BookEdition()));

    public override Task<BookEdition> GetBookEdition(GetBookEditionRequest request, ServerCallContext context)
        => Task.FromResult(repository.GetBookEdition(request.Path));

    public override Task<Empty> DeleteBookEdition(DeleteBookEditionRequest request, ServerCallContext context)
    {
        repository.DeleteBookEdition(request.Path);
        return Task.FromResult(new Empty());
    }

    public override Task<ListBookEditionsResponse> ListBookEditions(ListBookEditionsRequest request, ServerCallContext context)
    {
        var (results, nextPageToken) = repository.ListBookEditions(request.Parent, request.PageToken, request.MaxPageSize);
        var response = new ListBookEditionsResponse { NextPageToken = nextPageToken };
        response.Results.Add(results);
        return Task.FromResult(response);
    }

    public override Task<Isbn> CreateIsbn(CreateIsbnRequest request, ServerCallContext context)
        => Task.FromResult(repository.CreateIsbn(request.Id, request.Isbn ?? new Isbn()));

    public override Task<Isbn> GetIsbn(GetIsbnRequest request, ServerCallContext context)
        => Task.FromResult(repository.GetIsbn(request.Path));

    public override Task<ListIsbnsResponse> ListIsbns(ListIsbnsRequest request, ServerCallContext context)
    {
        var (results, nextPageToken) = repository.ListIsbns(request.Parent, request.PageToken, request.MaxPageSize);
        var response = new ListIsbnsResponse { NextPageToken = nextPageToken };
        response.Results.Add(results);
        return Task.FromResult(response);
    }

    public override Task<Item> CreateItem(CreateItemRequest request, ServerCallContext context)
        => Task.FromResult(repository.CreateItem(request.Parent, request.Id, request.Item ?? new Item()));

    public override Task<Item> GetItem(GetItemRequest request, ServerCallContext context)
        => Task.FromResult(repository.GetItem(request.Path));

    public override Task<Item> UpdateItem(UpdateItemRequest request, ServerCallContext context)
        => Task.FromResult(repository.UpdateItem(request.Path, request.Item ?? new Item(), request.UpdateMask));

    public override Task<Empty> DeleteItem(DeleteItemRequest request, ServerCallContext context)
    {
        repository.DeleteItem(request.Path);
        return Task.FromResult(new Empty());
    }

    public override Task<ListItemsResponse> ListItems(ListItemsRequest request, ServerCallContext context)
    {
        var (results, nextPageToken) = repository.ListItems(
            request.Parent,
            request.PageToken,
            request.MaxPageSize,
            request.Skip,
            request.Filter);

        var response = new ListItemsResponse { NextPageToken = nextPageToken };
        response.Results.Add(results);
        return Task.FromResult(response);
    }

    public override Task<Operation> MoveItem(MoveItemRequest request, ServerCallContext context)
        => Task.FromResult(repository.MoveItem(request.Path, request.TargetStore));

    public override Task<Publisher> CreatePublisher(CreatePublisherRequest request, ServerCallContext context)
        => Task.FromResult(repository.CreatePublisher(request.Id, request.Publisher ?? new Publisher()));

    public override Task<Publisher> GetPublisher(GetPublisherRequest request, ServerCallContext context)
        => Task.FromResult(repository.GetPublisher(request.Path));

    public override Task<Publisher> UpdatePublisher(UpdatePublisherRequest request, ServerCallContext context)
        => Task.FromResult(repository.UpdatePublisher(request.Path, request.Publisher ?? new Publisher(), request.UpdateMask));

    public override Task<Empty> DeletePublisher(DeletePublisherRequest request, ServerCallContext context)
    {
        repository.DeletePublisher(request.Path, request.Force);
        return Task.FromResult(new Empty());
    }

    public override Task<ListPublishersResponse> ListPublishers(ListPublishersRequest request, ServerCallContext context)
    {
        var (results, nextPageToken) = repository.ListPublishers(
            request.Parent,
            request.PageToken,
            request.MaxPageSize,
            request.Skip,
            request.Filter);

        var response = new ListPublishersResponse { NextPageToken = nextPageToken };
        response.Results.Add(results);
        return Task.FromResult(response);
    }

    public override Task<Publisher> ApplyPublisher(ApplyPublisherRequest request, ServerCallContext context)
        => Task.FromResult(repository.ApplyPublisher(request.Path, request.Publisher ?? new Publisher()));

    public override Task<Store> CreateStore(CreateStoreRequest request, ServerCallContext context)
        => Task.FromResult(repository.CreateStore(request.Id, request.Store ?? new Store()));

    public override Task<Store> GetStore(GetStoreRequest request, ServerCallContext context)
        => Task.FromResult(repository.GetStore(request.Path));

    public override Task<Store> UpdateStore(UpdateStoreRequest request, ServerCallContext context)
        => Task.FromResult(repository.UpdateStore(request.Path, request.Store ?? new Store(), request.UpdateMask));

    public override Task<Empty> DeleteStore(DeleteStoreRequest request, ServerCallContext context)
    {
        repository.DeleteStore(request.Path, request.Force);
        return Task.FromResult(new Empty());
    }

    public override Task<ListStoresResponse> ListStores(ListStoresRequest request, ServerCallContext context)
    {
        var (results, nextPageToken) = repository.ListStores(
            request.Parent,
            request.PageToken,
            request.MaxPageSize,
            request.Skip,
            request.Filter);

        var response = new ListStoresResponse { NextPageToken = nextPageToken };
        response.Results.Add(results);
        return Task.FromResult(response);
    }
}
