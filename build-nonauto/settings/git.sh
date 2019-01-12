GIT_COMMIT="false"

function _gitMessage
{
    if [ $(_web) == "false" ] ; then
        echo "WebPlayer build"
    else
        echo "Manual build"
    fi
}
