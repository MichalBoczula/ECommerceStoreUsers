# MongoDB index initialization and evolution

The service creates named Customer, Admin, history and Favorite indexes when it
starts. Calling the initializer again, or starting two instances against the
same database, is expected to leave the same indexes and documents in place.
Unique indexes continue to reject duplicate keys. Startup is allowed to fail
if an existing index has the same name but a different key or option set; it
does not drop, rebuild or rename an index automatically.

For an intentional index-definition change:

1. Back up the target database and inspect current data and index definitions.
   Check for duplicate values before adding or changing a unique index.
2. Plan a dedicated, reviewed database migration and compatible application
   rollout. Build replacement indexes under new names where appropriate;
   validate reads, writes, performance and uniqueness on a copy of production
   data. Account for old and new app versions running together.
3. Apply the migration as a controlled deployment step. Remove obsolete indexes
   only after all running instances use the new definition. Then start the new
   application version and confirm its initialization and readiness checks.

If startup reports a conflicting index definition, keep the existing data and
index intact; investigate the definition and roll back the app deployment or
apply the reviewed migration. Do not catch and ignore the conflict or perform
an automatic production drop/recreate during startup. A document `schemaVersion`
field is not required by this index policy and is not added implicitly.
