#!/bin/sh

echo "Updating repo and setting main branch downstream to the remote main branch"
cd REPO_Cerveza_Cristal
git pull
git switch main
git branch --set-upstream-to=origin/main
/bin/bash
