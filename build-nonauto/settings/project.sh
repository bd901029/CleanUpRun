function _popFirst
{
    for i in $* ; do
        if [ ${i} == ${1} ] ; then continue ; fi
        echo $i
    done
}

function _getTargets
{
    echo $(_popFirst $* | xargs | sed -e 's/ /,/')
}

function _containsTarget
{
    if [[ ${TARGETS} == *"$1"* ]] ; then
        echo "true"
    else
        echo "false"
    fi
}

function _iOS
{
    echo $(_containsTarget ios)
}

function _web
{
    echo $(_containsTarget web)
}

XCODEDIR=${PROJDIR}/${PROJECT}iOS
BUILDDIR=${XCODEDIR}/build
PROFILE=${PROJDIR}/${PROJECT}_Dev.mobileprovision
OUTDIR=${PROJDIR}

TARGETS=$(_getTargets $*)
