#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

if [ ${1} == 'npcactl' ] ; then
  rsh ${LOGIN_IP} -l $1 $2 $3 $4 $5
else
  rsh ${LOGIN_IP} -l $1 $2 $3
fi
