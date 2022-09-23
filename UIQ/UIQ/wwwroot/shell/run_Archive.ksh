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
#if [ ${1} == 'npcactl' ] ; then
#  lid=`cat /nwp/${1}/${5}/${6}/etc/Lid`
#  if [ $lid = "0" ] ; then 
#    echo 1 > /nwp/${1}/${5}/${6}/etc/Lid
#    rsh ${DATAMV_IP} -l $1 $2 $3 $4 MODEL=$5 MEMBER=$6 2>&1 | tee -a ${LOGPATH}/${OPLOG}
#    echo 0 > /nwp/${1}/${5}/${6}/etc/Lid
#  else 
#    rsh ${DATAMV_IP} -l $1 $2 $3 $4 MODEL=$5 MEMBER=$6 2>&1 | tee -a ${LOGPATH}/${OPLOG}
# fi
#else

### 2013/12/13 to solve the problem of Permission denied
###            touch log file first, and chmod to 766 (by SL)
rsh ${DATAMV_IP} -l $1 -n "cd ${LOGPATH}; touch $OPLOG;chmod 766 ${OPLOG}"

  if [ `echo $2 | egrep 'Earchive|Ncdf|obs'` ]; then
    rsh ${DATAMV_IP} -l $1 $2 $3 2>&1 | tee -a ${LOGPATH}/${OPLOG}
  elif [ `echo $2 | egrep 'GarchivedmsFLAG'` ]; then
    if [ $4 = "Major" ]; then
         if [ $6 = "M00" ]; then
           rsh ${DATAMV_IP} -l $1 $2 $3 $4 GG0C 2>&1 | tee -a ${LOGPATH}/${OPLOG}
           rsh ${DATAMV_IP} -l $1 $2 $3 $4 GG0G 2>&1 | tee -a ${LOGPATH}/${OPLOG}
           rsh ${DATAMV_IP} -l $1 $2 $3 $4 GGMG 2>&1 | tee -a ${LOGPATH}/${OPLOG}
         else
             rsh ${DATAMV_IP} -l $1 $2 $3 $4 GH0C 2>&1 | tee -a ${LOGPATH}/${OPLOG}
             rsh ${DATAMV_IP} -l $1 $2 $3 $4 GH0G 2>&1 | tee -a ${LOGPATH}/${OPLOG}
             rsh ${DATAMV_IP} -l $1 $2 $3 $4 GHMG 2>&1 | tee -a ${LOGPATH}/${OPLOG}
         fi
    elif [ $4 = "Post" ]; then
       rsh ${DATAMV_IP} -l $1 $2 $3 $4 Post 2>&1 | tee -a ${LOGPATH}/${OPLOG}
    fi

  else
    rsh ${DATAMV_IP} -l $1 $2 $3 $4 2>&1 | tee -a ${LOGPATH}/${OPLOG}
  fi
#fi
