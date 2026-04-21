# bookstore

Bookstore gRPC service in .NET with:

- support for the `bookstore.proto` contract
- JSON/HTTP transcoding for REST-style access
- OpenAPI generation at `/openapi/v1.json`

## Run

```bash
dotnet run
```

Then open `http://localhost:5238/openapi/v1.json` for the generated OpenAPI document.
