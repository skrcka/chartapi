#!/bin/bash
docker run -d -p 5337:5337 -e R_HOME=/usr/lib/R --name chartapi chartapi