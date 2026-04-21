using Example.Bookstore.V1;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace bookstore.OpenApi;

internal sealed class BookstoreDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Paths.Clear();

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/publishers",
            tag: "Publishers",
            listSummary: "List publishers",
            createSummary: "Create a publisher",
            listResponseType: typeof(ListPublishersResponse),
            createBodyType: typeof(Publisher),
            queryParameters: [QueryParameter("parent"), QueryParameter("page_token"), QueryParameter("max_page_size"), QueryParameter("skip"), QueryParameter("filter")],
            createParameters: [QueryParameter("parent"), QueryParameter("id")]);

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/stores",
            tag: "Stores",
            listSummary: "List stores",
            createSummary: "Create a store",
            listResponseType: typeof(ListStoresResponse),
            createBodyType: typeof(Store),
            queryParameters: [QueryParameter("parent"), QueryParameter("page_token"), QueryParameter("max_page_size"), QueryParameter("skip"), QueryParameter("filter")],
            createParameters: [QueryParameter("parent"), QueryParameter("id")]);

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/publishers/{publisher_id}/books",
            tag: "Books",
            listSummary: "List books under a publisher",
            createSummary: "Create a book under a publisher",
            listResponseType: typeof(ListBooksResponse),
            createBodyType: typeof(Book),
            queryParameters: [PathParameter("publisher_id"), QueryParameter("page_token"), QueryParameter("max_page_size")],
            createParameters: [PathParameter("publisher_id"), QueryParameter("id")]);

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/publishers/{publisher_id}/books/{book_id}/editions",
            tag: "Book Editions",
            listSummary: "List editions under a book",
            createSummary: "Create an edition under a book",
            listResponseType: typeof(ListBookEditionsResponse),
            createBodyType: typeof(BookEdition),
            queryParameters: [PathParameter("publisher_id"), PathParameter("book_id"), QueryParameter("page_token"), QueryParameter("max_page_size")],
            createParameters: [PathParameter("publisher_id"), PathParameter("book_id"), QueryParameter("id")]);

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/isbns",
            tag: "ISBNs",
            listSummary: "List ISBN resources",
            createSummary: "Create an ISBN resource",
            listResponseType: typeof(ListIsbnsResponse),
            createBodyType: typeof(Isbn),
            queryParameters: [QueryParameter("parent"), QueryParameter("page_token"), QueryParameter("max_page_size")],
            createParameters: [QueryParameter("parent"), QueryParameter("id")]);

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/stores/{store_id}/items",
            tag: "Items",
            listSummary: "List items under a store",
            createSummary: "Create an item under a store",
            listResponseType: typeof(ListItemsResponse),
            createBodyType: typeof(Item),
            queryParameters: [PathParameter("store_id"), QueryParameter("page_token"), QueryParameter("max_page_size"), QueryParameter("skip"), QueryParameter("filter")],
            createParameters: [PathParameter("store_id"), QueryParameter("id")]);

        AddResourcePath(swaggerDoc, context, "/publishers/{publisher_id}/books/{book_id}", "Books", typeof(Book), [PathParameter("publisher_id"), PathParameter("book_id")], allowPatch: true, allowPut: true, allowDelete: true, includeForceOnDelete: true);
        AddResourcePath(swaggerDoc, context, "/publishers/{publisher_id}/books/{book_id}/editions/{book_edition_id}", "Book Editions", typeof(BookEdition), [PathParameter("publisher_id"), PathParameter("book_id"), PathParameter("book_edition_id")], allowPatch: false, allowPut: false, allowDelete: true);
        AddResourcePath(swaggerDoc, context, "/isbns/{isbn_id}", "ISBNs", typeof(Isbn), [PathParameter("isbn_id")], allowPatch: false, allowPut: false, allowDelete: false);
        AddResourcePath(swaggerDoc, context, "/stores/{store_id}/items/{item_id}", "Items", typeof(Item), [PathParameter("store_id"), PathParameter("item_id")], allowPatch: true, allowPut: false, allowDelete: true);
        AddResourcePath(swaggerDoc, context, "/publishers/{publisher_id}", "Publishers", typeof(Publisher), [PathParameter("publisher_id")], allowPatch: true, allowPut: true, allowDelete: true, includeForceOnDelete: true);
        AddResourcePath(swaggerDoc, context, "/stores/{store_id}", "Stores", typeof(Store), [PathParameter("store_id")], allowPatch: true, allowPut: false, allowDelete: true, includeForceOnDelete: true);

        AddActionPath(
            swaggerDoc,
            context,
            "/publishers/{publisher_id}/books/{book_id}:archive",
            "Books",
            "Archive a book",
            [PathParameter("publisher_id"), PathParameter("book_id")],
            typeof(ArchiveBookRequest),
            typeof(Aep.Api.Operation));
        AddActionPath(
            swaggerDoc,
            context,
            "/stores/{store_id}/items/{item_id}:move",
            "Items",
            "Move an item to a different store",
            [PathParameter("store_id"), PathParameter("item_id")],
            typeof(MoveItemRequest),
            typeof(Aep.Api.Operation));
    }

    private static void AddCollectionPath(
        OpenApiDocument document,
        DocumentFilterContext context,
        string route,
        string tag,
        string listSummary,
        string createSummary,
        Type listResponseType,
        Type createBodyType,
        IList<OpenApiParameter> queryParameters,
        IList<OpenApiParameter> createParameters)
    {
        var path = GetOrCreatePath(document, route);

        path.Operations[OperationType.Get] = new OpenApiOperation
        {
            Summary = listSummary,
            Tags = [new OpenApiTag { Name = tag }],
            Parameters = queryParameters,
            Responses = OkResponse(context, listResponseType)
        };

        path.Operations[OperationType.Post] = new OpenApiOperation
        {
            Summary = createSummary,
            Tags = [new OpenApiTag { Name = tag }],
            Parameters = createParameters,
            RequestBody = JsonBody(context, createBodyType),
            Responses = OkResponse(context, createBodyType)
        };
    }

    private static void AddResourcePath(
        OpenApiDocument document,
        DocumentFilterContext context,
        string route,
        string tag,
        Type resourceType,
        IList<OpenApiParameter> pathParameters,
        bool allowPatch,
        bool allowPut,
        bool allowDelete,
        bool includeForceOnDelete = false)
    {
        var path = GetOrCreatePath(document, route);

        path.Operations[OperationType.Get] = new OpenApiOperation
        {
            Summary = "Get a resource by its canonical path",
            Tags = [new OpenApiTag { Name = tag }],
            Parameters = pathParameters.ToList(),
            Responses = OkResponse(context, resourceType)
        };

        if (allowPatch)
        {
            path.Operations[OperationType.Patch] = new OpenApiOperation
            {
                Summary = "Update a resource by path",
                Tags = [new OpenApiTag { Name = tag }],
                Parameters = [.. pathParameters, QueryParameter("update_mask")],
                RequestBody = JsonBody(context, resourceType),
                Responses = OkResponse(context, resourceType)
            };
        }

        if (allowPut)
        {
            path.Operations[OperationType.Put] = new OpenApiOperation
            {
                Summary = "Apply or upsert a resource by path",
                Tags = [new OpenApiTag { Name = tag }],
                Parameters = pathParameters.ToList(),
                RequestBody = JsonBody(context, resourceType),
                Responses = OkResponse(context, resourceType)
            };
        }

        if (allowDelete)
        {
            var parameters = pathParameters.ToList();
            if (includeForceOnDelete)
            {
                parameters.Add(QueryParameter("force"));
            }

            path.Operations[OperationType.Delete] = new OpenApiOperation
            {
                Summary = "Delete a resource by path",
                Tags = [new OpenApiTag { Name = tag }],
                Parameters = parameters,
                Responses = OkResponse(context, typeof(Google.Protobuf.WellKnownTypes.Empty))
            };
        }
    }

    private static void AddActionPath(
        OpenApiDocument document,
        DocumentFilterContext context,
        string route,
        string tag,
        string summary,
        IList<OpenApiParameter> pathParameters,
        Type requestType,
        Type responseType)
    {
        var path = GetOrCreatePath(document, route);

        path.Operations[OperationType.Post] = new OpenApiOperation
        {
            Summary = summary,
            Tags = [new OpenApiTag { Name = tag }],
            Parameters = pathParameters.ToList(),
            RequestBody = JsonBody(context, requestType),
            Responses = OkResponse(context, responseType)
        };
    }

    private static OpenApiPathItem GetOrCreatePath(OpenApiDocument document, string route)
    {
        if (!document.Paths.TryGetValue(route, out var path))
        {
            path = new OpenApiPathItem();
            document.Paths[route] = path;
        }

        return path;
    }

    private static OpenApiRequestBody JsonBody(DocumentFilterContext context, Type type)
        => JsonBody(context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository));

    private static OpenApiRequestBody JsonBody(OpenApiSchema schema)
        => new()
        {
            Required = true,
            Content = JsonContent(schema)
        };

    private static OpenApiResponses OkResponse(DocumentFilterContext context, Type type)
        => new()
        {
            ["200"] = new OpenApiResponse
            {
                Description = "Success",
                Content = JsonContent(context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository))
            }
        };

    private static Dictionary<string, OpenApiMediaType> JsonContent(OpenApiSchema schema)
        => new()
        {
            ["application/json"] = new OpenApiMediaType
            {
                Schema = schema
            }
        };

    private static OpenApiParameter PathParameter(string name)
        => new()
        {
            Name = name,
            In = ParameterLocation.Path,
            Required = true,
            Schema = new OpenApiSchema { Type = "string" }
        };

    private static OpenApiParameter QueryParameter(string name)
        => new()
        {
            Name = name,
            In = ParameterLocation.Query,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        };
}
