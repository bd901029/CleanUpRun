#!/bin/bash

# Build us an iOS project
echo "Running Unity build..."
${UNITY} -quit -batchMode -projectPath "${PROJDIR}"\
                        -executeMethod Builder.BuildGame \
                        -buildGame="${PROJECT}" \
                        -buildVersion="${VERSION}" \
                        -target=${TARGETS} \
                        -logFile "${PROJDIR}/build.log"
assert