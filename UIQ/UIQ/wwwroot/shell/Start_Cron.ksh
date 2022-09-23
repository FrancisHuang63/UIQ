#!/bin/ksh
FPATH=/nwp/npcactl/shfun/shlib
autoload EnvLib
EnvLib

set -x
if [ $# != 1 ]
then
  echo "$0 Check|Start|Stop|Crontable"
  exit 0
fi
Type=$1 

cron_check ()
{
prefix=$1
model=$2
if [[ ${prefix} = "ncsa" ]]
then
  RCheckLogin="rsh ${datamv} rsh ${ncsaLN}"
  HPC="FX100"
else
  RCheckLogin="rsh ${npcaLN}"
  HPC="FX10"
fi
LineC=`${RCheckLogin} -l ${prefix}${model} -n "crontab -l" | wc -l`
if [[ ${LineC} -ne "0" ]]
 then
   case ${model} in
     gfs) echo "<b>${HPC}_GFS_Cron</b>"
          echo "  total ${LineC} lines"
          echo
          ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep Gsubmit | grep -v ^#"
          ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep Tsubmit | grep -v ^#"
          ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep GcheckTY | grep -v ^#"
          echo
          ;;
     wrf) echo "<b>${HPC}_WRF_Cron</b>"
          echo "  total ${LineC} lines"
          echo
          ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep Wsubmit | grep -v ^#"
          echo
          ;;
     efs) echo "<b>${HPC}_EFS_Cron</b>"
          echo "  total ${LineC} lines"
          echo
          ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep Esubmit | grep -v ^#"
          echo
          ;;
     weps) echo "<b>${HPC}_WEPS_Cron</b>"
           echo "  total ${LineC} lines"
           echo
           ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep Wsubmit | grep CEN01 | grep -v ^#"
           echo
           ;;
     rwrf) echo "<b>${HPC}_RWRF_Cron</b>"
           echo "  total ${LineC} lines"
           echo
           ${RCheckLogin} -l ${prefix}${model} -n "crontab -l | grep RWsubmit | grep -v ^#"
           echo
           ;;
   esac
fi
}

func_Check ()
{
  echo "Show crontab line counts and main submit items."
  echo
  file=`cat ${SHETC}/acctable`
  for acc in ${file}
  do
  prefix=`echo ${acc} |cut -c1-4`
  model=`echo ${acc} |cut -c5-`
  cron_check ${prefix} ${model}
  continue
  done
}

func_Start ()
{
  file=`cat ${SHETC}/acctable`
  for acc in ${file}
  do
  prefix=`echo ${acc} |cut -c1-4`
  model=`echo ${acc} |cut -c5- |tr 'a-z' 'A-Z'`
  if [[ ${prefix} = "ncsa" ]]
  then
    rsh ${ncsaLN} -l ${acc} -n "crontab src/${model}_cron"
  else
    rsh ${npcaLN} -l ${acc} -n "crontab src/${model}_cron"
  fi
  continue
  done
  ${RSHX_D} ${SHBIN}/DBUtility.ksh -y others -n Crontab -m 0004801 -v onoff=ON
}

func_Stop ()
{
  file=`cat ${SHETC}/acctable`
  for acc in ${file}
  do
  prefix=`echo ${acc} |cut -c1-4`
  if [[ ${prefix} = "ncsa" ]]
  then
    rsh ${ncsaLN} -l ${acc} -n "crontab -r"
  else
    rsh ${npcaLN} -l ${acc} -n "crontab -r"
  fi
  continue
  done
  ${RSHX_D} ${SHBIN}/DBUtility.ksh -y others -n Crontab -m 0004801 -v onoff=OFF
}

func_Crontable_Check ()
{
  file=`cat ${SHETC}/acctable`
  for acc in ${file}
  do
  prefix=`echo ${acc} |cut -c1-4`
  model=`echo ${acc} |cut -c5- |tr 'a-z' 'A-Z'`
  if [[ ${prefix} = "ncsa" ]]
  then
    rsh ${ncsaLN} -l ${acc} -n "/bin/ls src/${model}_cron"
    rsh ${ncsaLN} -l ${acc} -n "/bin/cat src/${model}_cron"
  else
    rsh ${npcaLN} -l ${acc} -n "/bin/ls src/${model}_cron"
    rsh ${npcaLN} -l ${acc} -n "/bin/cat src/${model}_cron"
  fi
  continue
  done
}

ncsaLN=`cat /nwp/ncsactl/shfun/shetc/setLN |head -1`
npcaLN=`cat /nwp/npcactl/shfun/shetc/setLN |head -1`
##datamv=`hostname`
datamv=datamv01

##### main program
case ${Type} in
  Check) func_Check ;;
  Start) 
         func_Start
         echo "Start"
         echo "$(date '+DATE: %Y/%m/%d %T') Cron Start" >> ${SHBIN}/log/CronStart.log
         func_Check ;;
  Stop) 
         func_Stop
         echo "Stop"
         echo "$(date '+DATE: %Y/%m/%d %T') Cron Stop" >> ${SHBIN}/log/CronStart.log
         func_Check ;;
  Crontable) func_Crontable_Check ;;
esac
