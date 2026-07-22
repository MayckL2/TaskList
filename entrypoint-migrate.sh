#!/bin/bash
set -e

# Longer wait for database to fully stabilize
for attempt in {1..60}; do
  echo "[$attempt/60] Attempting database migration..."
  if /root/.dotnet/tools/dotnet-ef database update; then
    echo "Migration succeeded!"
    exit 0
  fi
  echo "Migration failed, waiting 2 seconds before retry..."
  sleep 2
done

echo "Failed to migrate database after all retries"
exit 1
