#!/bin/bash
export PROJDIR XCODEDIR

if [ $(_iOS) == "true" ] ; then
    echo "Adding additional frameworks..."
    python "${PROJDIR}/build-nonauto/build/15-patch-frameworks.py" ${FRAMEWORKS} >> "${PROJDIR}/build.log" 2>&1
    assert
fi
