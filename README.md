# WCPS on Kubernetes

WCPS is a .NET solution for a product catalog backend, with a React/Vite web client scaffold. The backend is split into API, service, repository, and data projects so HTTP handling, application operations, persistence, and database entities have separate responsibilities.

## Architecture

```text
Client / HTTP
    |
    v
WCPS.API (ASP.NET Core controllers, Swagger, dependency injection)
    |
    v
WCPS.Service.Contract + WCPS.Services (product service)
    |
    v
WCPS.Repository.Contract + WCPS.Repository (repository, unit of work, mapping)
    |
    v
WCPS.DATA (EF Core StorageContext, SQL Server, ASP.NET Identity)
    |
    v
WCPS.Data.Model (database entities)
```

`WCPS.Service.Model` contains product models passed through the service and repository contracts. `WCPS.Repository.Mapper.RepositoryProfile` maps those models to and from database entities with AutoMapper.

### Projects

| Project                    | Responsibility                                                                                                                                                         |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `WCPS.API`                 | Main ASP.NET Core API host. Configures SQL Server, dependency injection, controllers, and development Swagger.                                                         |
| `WCPS.Service.Contract`    | Service interfaces, including `IProductService`.                                                                                                                       |
| `WCPS.Services`            | Product service implementation; calls repository operations through `IUnitOfWork`.                                                                                     |
| `WCPS.Repository.Contract` | Repository and unit-of-work interfaces.                                                                                                                                |
| `WCPS.Repository`          | EF-backed product repository, repository factory, unit of work, and AutoMapper profile.                                                                                |
| `WCPS.DATA`                | EF Core `StorageContext`, migrations, SQL Server and Identity configuration.                                                                                           |
| `WCPS.Data.Model`          | Persistence entities, currently including `Product`.                                                                                                                   |
| `WCPS.Service.Model`       | Product model used by service and repository APIs.                                                                                                                     |
| `WCPS.WEB/WCPS.WEB.Server` | ASP.NET Core host configured with SPA proxy support for the Vite client. It currently has only a weather sample controller and does not register the product services. |
| `WCPS.WEB/wcps.web.client` | React 19, TypeScript, and Vite client. Currently displays the weather sample, not product data.                                                                        |
| `WCPSWeb/WCPSWeb.Server`   | Separate .NET 8 web/API template project; not part of the configured launch profile.                                                                                   |
| `WCPS.Console`             | Console template project; currently prints `Hello, World!`.                                                                                                            |

## Product request flow

The API registers `StorageContext`, `IProductRepository`, `IRepositoryFactory`, `IUnitOfWork`, and `IProductService` in its dependency-injection container. A product request is handled by `WeatherForecastController`, forwarded to `ProductService`, then to `ProductRepository`. The repository uses EF Core to read or write `Products`, and AutoMapper converts between the database `WCPS.Data.Model.Product` and the service `WCPS.Service.Model.Product`.

The current product endpoints are:

| Method | Route                       | Behavior                                               |
| ------ | --------------------------- | ------------------------------------------------------ |
| `GET`  | `/WeatherForecast/products` | Returns products, or `404` if none are found.          |
| `POST` | `/WeatherForecast/product`  | Creates a product and returns `201` on success.        |
| `GET`  | `/WeatherForecast`          | Returns sample weather data from the ASP.NET template. |

The React client currently calls `weatherforecast`; it has not yet been wired to the product endpoints. The product endpoints also currently live in the weather sample controller, so a future API cleanup could move them to a dedicated products controller.

## Requirements

- .NET SDK 10.0 for the main API and SPA server projects.
- SQL Server LocalDB (the default connection string targets `(localdb)\\mssqllocaldb`).
- Node.js and npm if you want to run/build the React client separately.
- .NET SDK 8.0 only if you also intend to build the standalone `WCPSWeb` template project.

## Run the API

From the repository root:

```powershell
dotnet restore WCPS.sln
dotnet run --project WCPS.API
```

The API launch profile uses `http://localhost:5201` and `https://localhost:7282`. In Development, Swagger is available at `/swagger`, for example `https://localhost:7282/swagger`.

The default database connection is in `WCPS.API/appsettings.json`. Override it for your environment rather than committing credentials. Apply the checked-in EF Core migrations before using database-backed product operations:

```powershell
dotnet ef database update --project WCPS.DATA --startup-project WCPS.API
```

If the `dotnet-ef` command is unavailable, install the EF Core command-line tool compatible with your .NET/EF Core SDK.

## Run the web client

The `WCPS.WEB` solution launch profile starts `WCPS.WEB.Server` and its Vite client together through ASP.NET Core SPA proxy. In Visual Studio, select the solution's `New Profile` launch profile. The server's configured HTTP URL is `http://localhost:5286`; its HTTPS URL is `https://localhost:7182`.

To run only the Vite development server:

```powershell
cd WCPS.WEB/wcps.web.client
npm install
npm run dev
```

Available client scripts include `npm run build`, `npm run lint`, and `npm run preview`.

## Run on Minikube

This deploys `WCPS.API` and a local SQL Server Developer container. SQL Server is included because the API's product endpoints use it. SQL Server data is kept in a Minikube persistent volume. The React web app is not deployed.

You need Docker Desktop running, Minikube, kubectl, and about 6 GB of memory available to Minikube. Run each step in **PowerShell**, from the repository root (`D:\Projects\WCPS_Kubernetes`).

### 1. Start Minikube

```powershell
minikube start --driver=docker --cpus=4 --memory=6144
```

Wait until Minikube reports that the cluster started.

### 2. Build the API image

Run this from the repository root:

```powershell
docker build -f Dockerfile.api -t wcps-api:dev .
```

Load the API image into Minikube so Kubernetes can run it without pulling from a registry:

```powershell
minikube image load wcps-api:dev
```

### 3. Create the namespace and SQL password

The namespace is a workspace inside Kubernetes for these resources. Create it first:

```powershell
kubectl apply -f k8s/minikube/namespace.yaml
```

Choose a strong local SQL Server password (at least 8 characters, including uppercase, lowercase, a number, and a symbol). Enter it when prompted; PowerShell stores it in a variable, then Kubernetes stores it in a Secret:

```powershell
$saPassword = Read-Host "Enter a local SQL Server SA password"
kubectl create secret generic wcps-secrets --namespace wcps --from-literal="MSSQL_SA_PASSWORD=$saPassword" --dry-run=client -o yaml | kubectl apply -f -
Remove-Variable saPassword
```

The `--dry-run... | kubectl apply` part lets you repeat this step if the Secret already exists. The password is not written to a file in the repository. This is for local development; do not reuse this password in Azure.

### 4. Deploy the app and database

```powershell
kubectl apply -k k8s/minikube
```

The Minikube API runs with the `Development` environment so its Swagger UI is enabled. This setting is only for this local overlay; use `Production` in a hosted environment.

Kubernetes may take a minute or two to start SQL Server. Check the pods:

```powershell
kubectl get pods -n wcps
```

Wait until `sqlserver-0` and `wcps-api-...` show `Running` and the API pod shows `1/1` in the `READY` column. If a pod is still starting, run the command again after a short wait.

### 5. Access the API

Open a **second PowerShell window** and start port forwarding:

```powershell
  kubectl port-forward -n wcps service/wcps-api 9000:8080
```

Keep that command running, then open <http://localhost:9000/swagger> in your browser. The same API is available at <http://localhost:9000/WeatherForecast>. Stop port forwarding later with `Ctrl+C`. Product endpoints are under `/WeatherForecast/products` and `/WeatherForecast/product`; they require the database schema to have been created.

### Update the API after changing code

Run these commands from the repository root after editing API code. They rebuild the container image, load it into Minikube, and replace the running API pod:

```powershell
docker build -f Dockerfile.api -t wcps-api:dev .
minikube image load wcps-api:dev
kubectl rollout restart deployment/wcps-api -n wcps
kubectl rollout status deployment/wcps-api -n wcps
```

For changes to Kubernetes manifests, apply them with `kubectl apply -k k8s/minikube`. If you change an environment value in `configmap.yaml`, also restart the API deployment so the pod gets the updated value. You do not need to rebuild the image for manifest-only changes.

### If something does not start

```powershell
kubectl get pods,pvc -n wcps
kubectl describe pod -n wcps <pod-name>
kubectl logs -n wcps deployment/wcps-api
kubectl logs -n wcps statefulset/sqlserver
```

For `<pod-name>`, copy a pod name from `kubectl get pods -n wcps`. SQL Server needs 2 GiB of memory by itself; if Minikube reports insufficient memory, increase the memory allocated to Minikube.

To stop the cluster and keep its data, run `minikube stop`. To remove the deployment and its local database data, run `kubectl delete namespace wcps`.

These manifests deploy the API and a local database for testing. For Azure Kubernetes Service, the next step is to push the API image to Azure Container Registry, use a managed database such as Azure SQL, and configure an ingress or Azure Load Balancer.

## Repository layout

```text
WCPS.API/                    Main API host and controllers
WCPS.Services/               Product service implementation
WCPS.Service.Contract/       Service interfaces
WCPS.Service.Model/          Service-layer product model
WCPS.Repository/             EF repositories, unit of work, mapping
WCPS.Repository.Contract/    Repository abstractions
WCPS.DATA/                   EF Core context and migrations
WCPS.Data.Model/             Database entities
WCPS.WEB/                    SPA-enabled ASP.NET host and React client
WCPSWeb/                     Separate .NET 8 template host/client
WCPS.Console/                Console template project
WCPS.sln                     Main solution
```
