#!/bin/bash

if [ $(_iOS) == "true" ] ; then
    # Package the symbols so we can maybe get some debug information at some point
    echo "Packaging dSYM..."
    zip -r ${BUILDDIR}/`tolower ${PROJECT}`.app.dSYM.zip \
            ${BUILDDIR}/`tolower ${PROJECT}`.app.dSYM >> "${PROJDIR}/build.log" 2>&1
    assert
fi
