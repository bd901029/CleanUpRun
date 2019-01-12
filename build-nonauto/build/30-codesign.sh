#!/bin/bash

if [ $(_iOS) == "true" ] ; then
    # make sure we're signed with the correct identity
    echo "CodeSigning app..."
    codesign -f --sign "${DEVELOPER}" "${BUILDDIR}/`tolower ${PROJECT}`.app" >> "${PROJDIR}/build.log" 2>&1
    assert
fi
 