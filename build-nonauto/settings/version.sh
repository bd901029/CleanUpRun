# utility functions
function _readVersionFile
{
    if [ -f "${_versionFile}" ] ; then
        cat "${PROJDIR}/.version"
        return
    fi
    echo "0.0.0"
}

function _writeVersionFile
{
    echo "${_version}" > "${_versionFile}"
}

function _incrementVersion
{
    # this is pretty nasty, but all it really does is 
    # increase the revision number of the build
    echo "${_version%.*}.$((${_version##*.}+1))"
}

# local variables
_versionFile="${PROJDIR}/.version"
_version=$(_readVersionFile)
_version=$(_incrementVersion "${_version}")
# save our current version out
$(_writeVersionFile)

# The final variable that the rest of the build script is expecting
VERSION="${_version}-${USER}"