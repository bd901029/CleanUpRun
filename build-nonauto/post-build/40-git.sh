#!/bin/bash

if [ ${GIT_COMMIT} == "true" ] ; then
  cd ${PROJDIR}
  git commit -am "$(_gitMessage)"
fi
