# Usage
Run the mesh via one of the run scripts ./scripts/run.ps1 or ./scripts/run.sh
and test it by sending a CURL request to post man.

```

curl -uri http://127.0.0.1:54492/get?foo=bar -headers @{forward-to="postman"}
```

# Service Configuration

Add sections to appsettings.json for service configuration.  Each service has a pool of one or more pre-configured hosts to be selected at random when proxying requests.


# Routing traffic through the mesh.

You can run the mesh server on any port, and the clients of the mesh should direct the traffic to that
IP/URL/Port with a host header that points to one of the configuration items.

# Performance Tests

We tested ServiceMesh by sending 10,000 requests with a small JSON payload.
The results for average request timeare as follows:

1. Without Micromesh: 10ms
2. With Micromesh No Logging: 75ms
3. With Micromesh and Logging: 168ms