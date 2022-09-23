#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

### START Added by fujitsu 2013/03/29
OPLOG=`basename $0`
OPLOG=`echo $OPLOG | sed 's/.ksh//'`
OPLOG="OP_${OPLOG}.`date +%m%d`"
LOGPATH="$7/log"
### END Added by fujitsu 2013/03/29

set -x
#echo "$1 $2 $3 $4 $5 $6"
if [ ${1} == 'npcactl' ] ; then
  lid=`cat /nwp/${1}/${5}/${6}/etc/Lid`
  if [ $lid = "0" ] ; then 
    echo 1 > /nwp/${1}/${5}/${6}/etc/Lid
    rsh ${LOGIN_IP} -l $1 $2 $3 $4 MODEL=$5 MEMBER=$6 2>&1 | tee -a ${LOGPATH}/${OPLOG}
    echo 0 > /nwp/${1}/${5}/${6}/etc/Lid
  else 
    rsh ${LOGIN_IP} -l $1 $2 $3 $4 MODEL=$5 MEMBER=$6 2>&1 | tee -a ${LOGPATH}/${OPLOG}
  fi
else
  #echo $0 $1 $2 $3 $4
  #rsh login01 -l fjapl03 /package/fx10/scripts/runserial.sh -x LD_LIBRARY_PATH=/package/dms/lib $2 $3 $4
  rsh ${LOGIN_IP} -l $1 /package/fx10/scripts/runserial.sh $2 $3 $4 2>&1 | tee -a ${LOGPATH}/${OPLOG}
  #rsh ${LOGIN_IP} -l $1 $2 $3 $4
fi
