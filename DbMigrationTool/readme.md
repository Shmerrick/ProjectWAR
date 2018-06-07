#DbMigrationTool#

Reads sql script files from db-scripts directory and applies these to the configured DB. These files should be named : 

* prefix: configurable, default: V
* version: numbers separated by _ (one underscore)
* separator: configurable, default: __ (two underscores)
* description: words separated by single underscores
* suffix: configurable, default: .sql

The application is 'smart' in that it will not apply changes already applied. It is based upon Evolve (https://evolve-db.netlify.com). 

Logs for the migrations are captured in the logs directory.

Scripts from DB developers should be attached and versioned as part of the commit/build process.

Executing the DbMigrationTool will execute the migration (hence bringing the DB up to date).

Look at the changelog table in the DB (default db = war_accounts) for version information.