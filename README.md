**Overview**

This project implements a small microservices-based application to
manage a graph structure. Each graph consists of nodes and edges (with a
max of two edges per node). Service A handles the CRUD operations for
the nodes, while Service B manages the database updates and validation.
The communication between the two services happens via WebSocket, and
everything is containerized using Docker Compose.

**Design Details**

**Graph Structure**

-   **Nodes**: Each node in the graph represents a single vertex.

-   **Edges**: Each node can have a maximum of two edges connecting it
    to other nodes, making it an undirected graph. You can add a node
    without any edges or with up to two edges

> **\*\***You cannot add an edge to a non-existing node. Before adding
> an edge, the node must first be created. This ensures the integrity of
> the graph and prevents any invalid edge creation.

**Database Design**

I used PostgreSQL for this solution, and the database has two tables:

1.  **Nodes**: Stores the individual nodes (vertices).

2.  **Edges**: Represents the connections (edges) between nodes. Since
    the graph is undirected, an edge can point from either node to the
    other.

The structure ensures scalability and thread safety. I use EF Core for
managing the database operations, which are all asynchronous.

**Microservices**

-   **Service A**: This service exposes the CRUD operations for
    manipulating the graph structure via HTTP requests. It uses JWT for
    authentication and authorization to ensure secure access to the
    endpoints.

-   **Service B**: This service handles the WebSocket communication with
    Service A. It receives messages from Service A, processes them, and
    updates the database accordingly. Service B uses transactions to
    keep the database operations safe and ensures data integrity.

**Security & Thread Safety**

-   **JWT Validation**: Requests to Service A are validated using JWT to
    ensure only authorized users can perform actions.

-   **Concurrency Control**: The nodes and edges use a concurrency token
    to avoid race conditions, ensuring safe concurrent access to the
    database.

-   **Transactions**: Service B uses transactions to handle database
    updates safely, preventing partial updates.

**Scalability**

The design is optimized for scalability. WebSocket communication ensures
a real-time, efficient message exchange, and the services are designed
to handle an increase in message volume. Using Docker Compose makes it
easy to scale the services independently.

**Docker Compose**

The project is containerized using Docker Compose. Each service (Service
A, Service B) runs in its own container, and PostgreSQL also runs in a
separate container. This setup ensures isolation and simplifies
deployment.

-   **Service A**: Handles HTTP CRUD operations.

-   **Service B:** Runs internally within the Docker network and
    communicates with Service A via WebSocket but is **not exposed** to
    the outside world.

-   **PostgreSQL:** The database container is also isolated within the
    network and is **not exposed** externally. Only internal
    communication within the Docker network can access the database
    using the container name postgres:5432.Async Programming **& Thread
    Safety**

Both services are implemented using asynchronous programming to ensure
they can handle multiple requests concurrently without blocking. The
database operations are also asynchronous, and transactions ensure the
process is safe from race conditions.

**Setup Instructions**

1.  Clone the repository.

2.  Run docker-compose up \--build to start all the containers (Service
    A, Service B, PostgreSQL).

\*\*A Postman collection has been included in the project to help test
the CRUD operations and WebSocket communication.
