#!/bin/bash

# build.sh
#
# Script to build Unity project
# usage:
#  sh build.sh NAME [DEPLOYMENTS...]
# 
# NAME        - Target name, especially if multiple targets in single project
# DEPLOYMENTS - One or more of the following deployments:
#      ios    - Build target for iOS
#      win    - Build target for Windows
#      osx    - Build target for Mac OS X

# TODO:
# - allow for more parameters
# - split into stages
# - disable stages from parameters

function tolower
{
    echo `echo $1 | tr '[A-Z]' '[a-z]'`
}

function path
{
    olddir=$(pwd)
    cd $(dirname "$1")
    dir=$(pwd)
    # change back to old directory, just in case
    cd ${olddir}
    echo ${dir}/
}

function assert
{
    if [ $? != 0 ]; then
        echo "Error"
        tail "${PROJDIR}/build.log"
        exit 1
    fi
}

function importUser
{
  source "${PROJDIR}/build-nonauto/user/$1.sh"
}

USER=`whoami`
PROJECT=$1
PROJDIR=$(path "$0")../
  
if [ -d "${PROJDIR}/build-nonauto/settings" ] ; then
  for file in `ls ${PROJDIR}/build-nonauto/settings/` ; do
    source "${PROJDIR}/build-nonauto/settings/${file}"    
  done
fi

# Make sure we're in the correct directory
cd "${PROJDIR}"

echo "Build started" > "${PROJDIR}/build.log"

for stage in "pre-build" "build" "post-build" ; do
  if [ -d "${PROJDIR}/build-nonauto/${stage}" ] ; then
    echo "Executing ${stage} scripts"
    for file in `ls ${PROJDIR}/build-nonauto/${stage}/*.sh` ; do
      source "${file}"
      assert
    done
  fi
done

# EOF