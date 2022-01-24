# minimal-api

## Example of minimal REST API for .Net 6

Main additions to the Visual Studio template include:
* Swagger integration with code comments
* Docker support
* CORS support
* OData support
* OpenTelemetry support with exporters for AppInsights, Console, or Zipkin
* Messaging support for AzureServiceBus, Dapr, or RabbitMQ
  * Two Phase Commit (prepare/commit aka transactional) support ensuring both event messaging and DB changes both succeed or both fail
    * This is NOT recommended over the SAGA pattern.  Recommend using project `saga-api`
* Ingress-gateway or reverse-proxy support for Open Api when defining BasePath environment variable