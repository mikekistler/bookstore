using Example.Bookstore.V1;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace bookstore.OpenApi;

internal sealed class BookstoreDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
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
            route: "/{parent}/books",
            tag: "Books",
            listSummary: "List books under a publisher",
            createSummary: "Create a book under a publisher",
            listResponseType: typeof(ListBooksResponse),
            createBodyType: typeof(Book),
            queryParameters: [PathParameter("parent"), QueryParameter("page_token"), QueryParameter("max_page_size")],
            createParameters: [PathParameter("parent"), QueryParameter("id")]);

        AddCollectionPath(
            swaggerDoc,
            context,
            route: "/{parent}/editions",
            tag: "Book Editions",
            listSummary: "List editions under a book",
            createSummary: "Create an edition under a book",
            listResponseType: typeof(ListBookEditionsResponse),
            createBodyType: typeof(BookEdition),
            queryParameters: [PathParameter("parent"), QueryParameter("page_token"), QueryParameter("max_page_size")],
            createParameters: [PathParameter("parent"), QueryParameter("id")]);

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
            route: "/{parent}/items",
            tag: "Items",
            listSummary: "List items under a store",
            createSummary: "Create an item under a store",
            listResponseType: typeof(ListItemsResponse),
            createBodyType: typeof(Item),
            queryParameters: [PathParameter("parent"), QueryParameter("page_token"), QueryParameter("max_page_size"), QueryParameter("skip"), QueryParameter("filter")],
            createParameters: [PathParameter("parent"), QueryParameter("id")]);

        AddResourcePath(swaggerDoc, context);
        AddActionPath(swaggerDoc, context, "/{path}:archive", "Books", "Archive a book", typeof(ArchiveBookRequest), typeof(Aep.Api.Operation));
        AddActionPath(swaggerDoc, context, "/{path}:move", "Items", "Move an item to a different store", typeof(MoveItemRequest), typeof(Aep.Api.Operation));
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

    private static void AddResourcePath(OpenApiDocument document, DocumentFilterContext context)
    {
        var oneOfResources = OneOfSchema(
            context,
            typeof(Book),
            typeof(BookEdition),
            typeof(Isbn),
            typeof(Item),
            typeof(Publisher),
            typeof(Store));

        var mutableResources = OneOfSchema(
            context,
            typeof(Book),
            typeof(Item),
            typeof(Publisher),
            typeof(Store));

        var path = GetOrCreatePath(document, "/{path}");

        path.Operations[OperationType.Get] = new OpenApiOperation
        {
            Summary = "Get a resource by its canonical path",
            Tags = [new OpenApiTag { Name = "Resources" }],
            Parameters = [PathParameter("path")],
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "The matching resource.",
                    Content = JsonContent(oneOfResources)
                }
            }
        };

        path.Operations[OperationType.Patch] = new OpenApiOperation
        {
            Summary = "Update a mutable resource by path",
            Tags = [new OpenApiTag { Name = "Resources" }],
            Parameters = [PathParameter("path"), QueryParameter("update_mask")],
            RequestBody = JsonBody(mutableResources),
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "The updated resource.",
                    Content = JsonContent(mutableResources)
                }
            }
        };

        path.Operations[OperationType.Put] = new OpenApiOperation
        {
            Summary = "Apply or upsert a mutable resource by path",
            Tags = [new OpenApiTag { Name = "Resources" }],
            Parameters = [PathParameter("path")],
            RequestBody = JsonBody(mutableResources),
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "The applied resource.",
                    Content = JsonContent(mutableResources)
                }
            }
        };

        path.Operations[OperationType.Delete] = new OpenApiOperation
        {
            Summary = "Delete a resource by path",
            Tags = [new OpenApiTag { Name = "Resources" }],
            Parameters = [PathParameter("path"), QueryParameter("force")],
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "The delete completed."
                }
            }
        };
    }

    private static void AddActionPath(
        OpenApiDocument document,
        DocumentFilterContext context,
        string route,
        string tag,
        string summary,
        Type requestType,
        Type responseType)
    {
        var path = GetOrCreatePath(document, route);

        path.Operations[OperationType.Post] = new OpenApiOperation
        {
            Summary = summary,
            Tags = [new OpenApiTag { Name = tag }],
            Parameters = [PathParameter("path")],
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

    private static OpenApiSchema OneOfSchema(DocumentFilterContext context, params Type[] types)
        => new()
        {
            OneOf = types
                .Select(type => context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository))
                .ToList()
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
