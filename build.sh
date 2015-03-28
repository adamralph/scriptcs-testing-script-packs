#!/usr/bin/env bash

# options
set -e
set -o pipefail
set -x

# determine cache dir
if [ -z $XDG_DATA_HOME ]; then
  NUGET_CACHE_DIR=$HOME/.local/share
else
  NUGET_CACHE_DIR=$XDG_DATA_HOME;
fi

# download nuget to cache dir
NUGET_URL="https://www.nuget.org/nuget.exe"
if test ! -f $NUGET_CACHE_DIR/nuget.exe; then
  mkdir -p $NUGET_CACHE_DIR
  echo Downloading latest version of NuGet.exe...
  wget -O $NUGET_CACHE_DIR/nuget.exe $NUGET_URL 2>/dev/null || curl -o $NUGET_CACHE_DIR/nuget.exe --location $NUGET_URL /dev/null
fi

# copy nuget locally
if test ! -f .nuget/nuget.exe; then
  mkdir -p .nuget
  cp $NUGET_CACHE_DIR/nuget.exe .nuget/nuget.exe
fi

# restore packages
mono .nuget/nuget.exe restore ScriptCs.Testing.ScriptPacks.sln

# build solution
xbuild ScriptCs.Testing.ScriptPacks.sln /property:Configuration=Release /nologo /verbosity:normal
