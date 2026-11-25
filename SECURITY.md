# Security

Report vulnerabilities privately to the repository owner.

The default stdio transport opens no listener. Optional HTTP mode is loopback-only and rejects foreign Host and Origin headers. It is not an authenticated remote service. Before adding privileged tools or exposing HTTP remotely, enforce authentication and authorization, validate inputs, and review data access. Keep credentials out of source control and diagnostic output.
