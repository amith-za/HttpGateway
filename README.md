# HttpGateway

An HTTP Router that I used for routing in an HTTP API Microservice Architecture.

Deveoped as an ASP.Net application with RethinkDB as the configuration store

The HttpGateway is configured via a DSL with the format:

```
backend "[backend-name]" from [registry-name] { [config-key] = "[config-value]", [config-key] = "[config-value]", ... }  

match
    [http-verb] "[route-template]"
modules
    [module-name] { [config-key] = "[config-value]" }
when
    {} use "[backend-name]" // default, required 
    { [routing-expression]} use "[backend-name]"
```

An example of the routing configuration

```
backend "stable" from builtin { service_name = "HelloApi", use_version = "1.0"}  
backend "experimental" from builtin { service_name = "HelloApi", use_latest = "true"}  

match
    GET "hello/{name}"
    GET "docs/hello/{*path}"
modules
    promote_param { params = ["group"] }
when
    {} use "stable"
    { request["group"] == "testing" } use "experimental"
```

The example defines two backends one named 'stable' and another 'experimental'. Stable is setup to use the builtin registry and target service HelloApi version 1.0 whilst Experimental uses the builtin registry and targets the latest available version of HelloApi.

In the match section the verbs and routes that are supported by HelloApi are defined. For this route the promote_param module is run to promote the group query string parameter to a routing property.

By default request will be routed to the stable backend but if the request has a routing property "group" and the value of that property is "testing" then the request is routed to the Experimental backend.
