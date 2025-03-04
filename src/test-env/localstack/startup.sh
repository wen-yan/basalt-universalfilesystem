#!/bin/bash

echo "start-localstack.sh is executing"
/usr/local/bin/docker-entrypoint.sh &

apt-get install -y jq

### avoid exiting
tail -f /dev/null