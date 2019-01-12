#!/bin/bash

if [ $(_iOS) == "true" ] ; then
    if [ -e ${INSTALLER} -a -e ${INFO} ] ; then
        if [ `${INFO} | wc -l` -gt 1 ] ; then
            # install
            echo "Installing to device..."
            ${INSTALLER} -i "${OUTDIR}/${PROJECT}.ipa" >> "${PROJDIR}/build.log" 2>&1
        fi
    fi
fi
