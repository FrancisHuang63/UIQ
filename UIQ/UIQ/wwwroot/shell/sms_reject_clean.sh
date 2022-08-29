#!/bin/sh
#d=`dirname "${0}"`
#expr "${0}" : "/.*" > /dev/null || cwd=`(cd "${cwd}" && pwd)`
#HpcCTL=`echo "${cwd}" | cut -d/ -f3`
HpcCTL=`whoami`
. /ncs/${HpcCTL}/web/shell/SV_PATH.cfg

set -x
rm -rf ${UIPATH}log/SMS/sms_start_reject.log

echo "Log clean !!" > /tmp/sms_start_reject_test.log