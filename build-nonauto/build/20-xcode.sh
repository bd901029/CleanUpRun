#!/bin/bash

if [ $(_iOS) == "true" ] ; then
    # enter our newly created directory
    cd "${XCODEDIR}"
    # make sure that we rebuild the pch (yeah, this defies the point of a pch a little...)
    touch "${XCODEDIR}/Classes/iPhone_target_Prefix.pch"

    # try to build our xcode project
    echo "Running Xcode build..."
    xcodebuild -configuration Debug -sdk iphoneos6.1 >> "${PROJDIR}/build.log" 2>&1
fi