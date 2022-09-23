#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg
. /nwp/npcactl/shell/SV_PATH.cfg

rsh ${LOGIN_IP} -l ${1} "echo 0 > ${2}/etc/Lid"
rsh ${LOGIN_IP} -l ${1} "/usr/lpp/LoadL/full/bin/llcancel ${3}"

mmdd=`cat ${2}/etc/crdate | cut -c3-6`
rsh ${LOGIN_IP} -l ${1} "echo \"$(date '+[%T]') Job Cancelled\" >> ${2}/log/job.${mmdd}"

/bin/ksh ${SHPATH}/log_collector.ksh
