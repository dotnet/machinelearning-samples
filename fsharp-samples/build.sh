#!/usr/bin/env bash

set -eu

cd "$(dirname "$0")"

PAKET_EXE=.paket/paket.exe

FSIARGS=""
FSIARGS2=""
OS=${OS:-"unknown"}
if [ "$OS" != "Windows_NT" ]
then
  # Can't use FSIARGS="--fsiargs -d:MONO" in zsh, so split it up
  # (Can't use arrays since dash can't handle them)
  FSIARGS="--fsiargs"
  FSIARGS2="-d:MONO"
fi

run() {
  if [ "$OS" != "Windows_NT" ]
  then
    mono "$@"
  else
    "$@"
  fi
}

echo "Executing Paket..."

FILE='paket.lock'     
if [ -f $FILE ]; then
   echo "paket.lock file found, restoring packages..."
   run $PAKET_EXE restore
else
   echo "paket.lock was not found, installing packages..."
   run $PAKET_EXE install
fi