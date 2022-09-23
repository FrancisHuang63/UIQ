#!/bin/ksh
. /nwp/npcactl/ncshome/cfg/SMS.cfg

rsh ${SMSHOST} -n ${SHPATH}/sms_start.ksh

exit 0
