# TTH INVENTORY MANAGEMENT API

## Infrastructure

## Basic Design

```plantuml
@startuml
title Inventory Management System Architecture
allow_mixing
rectangle "Inventory Management" {
    node "Backend (Server)" {
        component "Web API (.NET)" as be #lightgreen
    }
}
rectangle "Database Layer" {
    cloud "AWS" {
        database "DynamoDB" as dyb #lightblue
    }
    node "Microsoft SQL Server" as sqlsrv {
        database "SQL Server" as sqss #lightblue
    }
}

' Force layout: BE -> DB
be -[hidden]-> dyb
be -[hidden]-> dyb
dyb -[hidden]-> sqss
dyb -[hidden]-> sqss
dyb -[hidden]-> sqss

' Interactions
be -> dyb: HTTPS CRUD Operation
be <. dyb: HTTPS Results

be <. sqss: HTTPS Results
be --> sqss: HTTPS Queries/Transactions

@enduml
```

## Function Design

### GetListAllProductsAsync

Retrieves all product records stored in DynamoDB.

```plantuml
@startuml
title Retrieves all products across all institutions.
actor "user" as usr
participant "FE/SwaggerUI" as fe
box "Web API"

    participant Controllers as ctl
    participant ProductsServices as srv
    participant ProductsRepository as repo
end box
database "Sql Server" as ss
database DynamoDB as dyb
usr -> fe: interact
activate fe
fe ->  ctl: **GET: /**
activate ctl
ctl -> srv: GetListAllProductsAsync()
activate srv
srv -> repo: GetListAllProductsAsync()
activate repo
alt If using DynamoDB
    repo -> dyb: **ScanAsync()** with DynamoDB pagination 
    activate dyb
    dyb --> repo: List of all products
    deactivate dyb
else If using Sql Server
    repo -> ss: **Take all** by page size
    activate ss
    ss --> repo: List of all products
    deactivate ss
end
note right
Fetches all products from 
DB with Pagination
end note
repo --> srv: List of all products 
deactivate repo
srv --> ctl: List of all products (Response DTO)
deactivate srv
ctl --> fe: Response
deactivate ctl
fe --> usr: Interact
deactivate fe

@enduml
```

### GetListProductsAsync

Retrieves products belonging to a specific institution.

```plantuml
@startuml
title Retrieves products of a specific institution.
actor "user" as usr
participant "FE/SwaggerUI" as fe
box "Web API"

    participant Controllers as ctl
    participant ProductsServices as srv
    participant ProductsRepository as repo
end box
database "Sql Server" as ss
database DynamoDB as dyb
usr -> fe: interact
activate fe
fe ->  ctl: **GET: /{institutionCode}**
activate ctl
ctl -> srv: GetListProductsAsync()
activate srv
srv -> repo: GetListProductsAsync()
activate repo
alt If using DynamoDB
    repo -> dyb: **QueryAsync()** using {institutionCode} as Key
    activate dyb
    dyb --> repo: List of products
    deactivate dyb
else If using Sql Server
    repo -> ss: Filter by {institutionCode} by page size
    activate ss
    ss --> repo: List of products
    deactivate ss
end
note right
Fetches products from 
DB with Pagination
end note
repo --> srv: List of products 
deactivate repo
srv --> ctl: List of products (Response DTO)
deactivate srv
ctl --> fe: Response
deactivate ctl
fe --> usr: Interact
deactivate fe

@enduml
```

### SearchListProductsAsync

Searches for products in a specific institution based on provided criteria.

```plantuml
@startuml
title Searches products of a specific institution.
actor "user" as usr
participant "FE/SwaggerUI" as fe
box "Web API"

    participant Controllers as ctl
    participant ProductsServices as srv
    participant ProductsRepository as repo
end box
database "Sql Server" as ss
database DynamoDB as dyb
usr -> fe: interact
activate fe
fe ->  ctl: **POST: /search**
activate ctl
ctl -> srv: SearchListProductsAsync()
activate srv
srv -> repo: SearchListProductsAsync()
activate repo
alt If using DynamoDB
    repo -> dyb: **QueryAsync()** using a Global Secondary Index (GSI).
    activate dyb
    dyb --> repo: List of products
    deactivate dyb
else If using Sql Server
    repo -> ss: Filter by query parameters with page size
    activate ss
    ss --> repo: List of products
    deactivate ss
end
note right
Fetches products from 
DB with Pagination
end note
repo --> srv: List of products 
deactivate repo
srv --> ctl: List of products (Response DTO)
deactivate srv
ctl --> fe: Response
deactivate ctl
fe --> usr: Interact
deactivate fe
@enduml
```

### RegisterProductsAsync

Registers new products for a specific institution.

```plantuml
@startuml
title Registers new products in a specific institution.
actor "user" as usr
participant "FE/SwaggerUI" as fe
box "Web API"

    participant Controllers as ctl
    participant ProductsServices as srv
    participant ProductsRepository as repo
end box
database "Sql Server" as ss
database DynamoDB as dyb
usr -> fe: interact
activate fe
fe ->  ctl: **PUT: /{institutionCode}**
activate ctl
ctl -> srv: RegisterProductsAsync()
activate srv
srv -> repo: RegisterProductsAsync()
activate repo
alt If using DynamoDB
    repo -> dyb: **TransactWriteItemsAsync()** Insert new Products using Transaction
    activate dyb
    dyb --> repo: List of products
    deactivate dyb
else If using Sql Server
    repo -> ss: Check existing and insert new Products
    activate ss
    ss --> repo: Reponse
    deactivate ss
end
note right
Check existing and 
Insert new Products into DB
end note
repo --> srv
deactivate repo
srv --> ctl
deactivate srv
ctl --> fe: Response
deactivate ctl
fe --> usr: Interact
deactivate fe
@enduml
```

### UpdateProductsAsync

Updates product information for a specific institution.

```plantuml

@startuml
title Updates products in a specific institution.
actor "user" as usr
participant "FE/SwaggerUI" as fe
box "Web API"

    participant Controllers as ctl
    participant ProductsServices as srv
    participant ProductsRepository as repo
end box
database "Sql Server" as ss
database DynamoDB as dyb
usr -> fe: interact
activate fe
fe ->  ctl: **POST: /{institutionCode}**
activate ctl
ctl -> srv: SaveProductsAsync()
activate srv
srv -> repo: SaveProductsAsync()
activate repo
alt If using DynamoDB
    repo -> dyb: **TransactWriteItemsAsync()** Update Products using Transaction
    activate dyb
    dyb --> repo: List of products
    deactivate dyb
else If using Sql Server
    repo -> ss: Check existing and update Products
    activate ss
    ss --> repo: Reponse
    deactivate ss
end
note right
Check existing and 
update Products in DB
end note
repo --> srv
deactivate repo
srv --> ctl
deactivate srv
ctl --> fe: Response
deactivate ctl
fe --> usr: Interact
deactivate fe
@enduml
```