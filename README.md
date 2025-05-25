# Assignment

## Quick Start

Clone the repo and cd into the project directory:

```bash
git clone https://github.com/albarp/su-assignment.git
cd su-assignment
```

Build the docker image:

```bash
docker build -t mytest .
```

Give execution permission to the three script files:

```bash
chmod +x scripts/build.sh
chmod +x scripts/tests.sh
chmod +x scripts/run.sh
```

Execute these three scripts, in oder, in a docker container run from the build image:

```bash
docker run -v $(pwd):/mnt -p 9090:9090 -w /mnt mytest ./scripts/build.sh
```
```bash
docker run -v $(pwd):/mnt -p 9090:9090 -w /mnt mytest ./scripts/tests.sh
```
```bash
docker run -v $(pwd):/mnt -p 9090:9090 -w /mnt mytest ./scripts/run.sh
```

Once the server is running, you can invoke the API with curl. Here's an example matching the assignment requirements:

```bash
curl -X POST http://localhost:9090/order \
-H "Content-Type: application/json" \
-d '{
  "order": {
    "items": [
      {
        "product_id": 1,
        "quantity": 1
      },
      {
        "product_id": 2,
        "quantity": 5
      },
      {
        "product_id": 3,
        "quantity": 1
      }
    ]
  }
}'
```

Expected response:
```json
{
    "order_id": 1,
    "order_price": 12.5,
    "order_vat": 1.25,
    "items":[
      {
        "product_id": 1,
        "quantity": 1,
        "price": 2,
        "vat": 0.2
      },
      {
        "product_id": 2,
        "quantity": 5,
        "price": 7.5,
        "vat":0.75
      },
      {
        "product_id": 3,
        "quantity": 1,
        "price": 3,
        "vat": 0.3
      }
    ]
}
```

Note: There is no endpoint to retrieve the list of Items, Pricing, or VAT. You can find the sample data in the `src/PurchaseCart.DataAccessSqlite/DBSeeder.cs` file.

# General Considerations

This is a .NET 8.0 Web API application.

I have implemented the proposed assignment as a three-layer architecture:
- Controller
- Application Logic
- Data Access

This is a well-known approach for applications that need to:
1. Receive requests through API (Controller)
2. Execute business logic (Application Logic)
3. Access a database (Data Access)

Regarding the database, I chose SQLite because it is lightweight and easy to use, which is appropriate for this simple assignment application. While I evaluated a non-relational database, I believe this problem is a classical fit for a relational database. I didn't use an ORM to avoid coupling the domain models to a particular framework.

Both unit and integration tests are under the same project for simplicity, but they can be run independently in a hypothetical CI/CD pipeline.

Note on API Response Format:
The API returns numerical values (prices and VAT) as floating-point numbers without forced decimal places. For example, a price might be returned as `12.5` instead of `12.50`. This is a deliberate design choice to maintain the values as numbers rather than strings, which is more appropriate for numerical calculations and data processing. 

Please ignore docker-compose files, they are used to setup y dev environment

## Layers

### Controller (PurchaseCart.API)
This layer handles:
- Server initialization
- API route exposure
- Request handling

The `Program.cs` file contains the startup logic that:
- Registers services for Dependency Injection
- Configures middleware for routing and other features

The `Controller/OrderController.cs` contains the `CreateOrder` method serving the POST /order API endpoint. This method performs payload validation (defined in `Models/OrderRequest.cs`) before invoking the application logic.

### Application Logic (PurchaseCart.Domain)
This layer contains the assignment's domain abstractions and implementation logic. The `Logic/OrderLogic.cs` file contains the `CalcAndSave` method that:
1. Retrieves data from the data source
2. Computes single items' current pricing and VAT
3. Calculates the order total
4. Stores the order
5. Returns the result to the caller

#### Data Model
The `Entities` directory contains the following entities:
- `Item`: Represents a product
- `Pricing`: Contains item pricing with a reference to the Item
- `VAT`: Contains item VAT rates with a reference to the Item
- `ItemOrderPrice`: Represents the relationship between items and their prices and vat at the time of the order. This is used in computation alone, it is not stored in the the DB.
- `Order`: Represents a customer order
- `OrderItem`: Represents items within an order

### Data Access (PurchaseCart.DataAccessSqlite)
This layer implements SQLite data access. Key components include:
- `DBSchemaInitializer`: A basic implementation for database setup (needs improvement for migrations)
- `DBSeeder`: A utility class for populating sample data

The SQLite database file (`purchasecart.production.db`) is located in the publish folder.

## Tests (PurchaseCart.Tests)
The test project is organized into two directories:
- `Integration`: Contains database-related tests
- `Unit`: Contains application logic tests

Tests follow the Arrange-Act-Assert pattern and use the xUnit framework.

## CI/CD (Dockerfile and Script Files)
The current implementation uses scripts for build, test, and release. In a production environment, I would:
- Create a multi-stage build for publishing
- Move environment variables out of the Dockerfile
- Provide environment variables at runtime through the container orchestrator

## Logging Considerations

The application uses Microsoft.Extensions.Logging (ILogger) as the standard logging framework. While basic logging is implemented, there are several areas that could be improved for a production environment:
- Propagate the Trce ID, provided ASP.NET Core built in logging to the application logger
- Extend logs for the Application Logic and Data Access layers

