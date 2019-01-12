#!/bin/bash

if [ $(_iOS) == "true" ] ; then
  if [ ${TF_UPLOAD} == "true" ] ; then
      # Distribute
      echo "Uploading to TestFlight..."
      /usr/bin/curl "http://testflightapp.com/api/builds.json" \
        -F file=@"${OUTDIR}/${PROJECT}.ipa" \
        -F dsym=@"${BUILDDIR}/`tolower ${PROJECT}`.app.dSYM.zip" \
        -F api_token="${TF_API_TOKEN}" \
        -F team_token="${TF_TEAM_TOKEN}" \
        -F notes="Manual build - ${USER}" \
        -F distribution_lists="${TF_DISTRIBUTION_LISTS}" >> "${PROJDIR}/build.log" 2>&1

      assert
  fi
fi