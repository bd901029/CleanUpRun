#!/bin/bash

if [ $(_iOS) == "true" ] ; then
    # archive and sign our build
    echo "Running xcrun packaging..."
    /usr/bin/xcrun -sdk iphoneos PackageApplication -v "${BUILDDIR}/`tolower ${PROJECT}`.app" \
                                                    -o "${OUTDIR}/${PROJECT}.ipa" \
                                                    --sign "${DEVELOPER}" \
                                                    --embed "${PROFILE}" >> "${PROJDIR}/build.log" 2>&1
    assert
fi
