# Local startup and MongoDB failure behavior

Copy `.env.example` to `.env`, set a local-only MongoDB username and strong
password, and start the stack with Docker Compose:

```bash
docker compose up -d --build
curl -i http://localhost:8080/health/live
curl -i http://localhost:8080/health/ready
```

Compose starts MongoDB, initializes the `rs0` replica set, and only then starts
the API. `/health/ready` must return `200` before sending business traffic.
The API uses `MongoDbSettings__ConnectionString` from Compose; it must point to
the writable replica-set primary for Customer/Admin transactions.

During host startup, a read-only MongoDB `ping` can be attempted up to three
times with a three-second limit per attempt and delays of 0.5 and 1 second.
The whole probe plus index initialization has a 20-second deadline. Host stop
or cancellation interrupts the probe, delay or index command. Once the probe
passes, the named index initialization runs exactly once; conflicting index
definitions, authentication errors and index failures stop startup. An
interrupted index build may have completed in MongoDB; restarting safely
checks/creates the same named definitions, as verified in REF-09/6.

The startup policy never retries an entire index operation or a Customer,
Admin or Favorite write/transaction. A failed startup requires correcting the
connection, credentials or index conflict and restarting the instance. For
controlled index changes see [index evolution](mongo-index-evolution.md).
The API's readiness check continues to report transient MongoDB outages after
startup without restarting the process.
