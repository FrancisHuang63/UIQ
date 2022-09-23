#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

if [ $# != "3" ]; then
  echo "The Parameter must be {MODEL}, {Path} and {Class}." 
  exit -1
fi
if [ "${3}" != 'urgent' ] && [ "${3}" != 'nwp' ] && [ "${3}" != 'priorop' ] && [ "${3}" != 'oper' ] && [ "${3}" != 'member' ] && [ "${3}" != 'beta' ] && [ "${3}" != 'research' ]; then
  echo "The Class ${3} is not used." 
  exit -1
fi

ACCOUNT=${1}
NWPETC=${2}
CLASS=${3}

for Prog in `ls ${NWPETC}`
do
  prior=`cat ${NWPETC}/${Prog} | grep "^#@ class" | cut -d' ' -f4`
  if [ "${prior}" != "" ]; then
    if [ "${prior}" = 'urgent' ] || [ "${prior}" = 'nwp' ] || [ "${prior}" = 'priorop' ] || [ "${prior}" = 'oper' ] || [ "${prior}" = 'member' ] || [ "${prior}" = 'beta' ] || [ "${prior}" = 'research' ]; then
	  if [ "${prior}" != "${CLASS}" ]; then 
        rsh ${LOGIN_IP} -l ${ACCOUNT} "sed \"s/^#@ class = ${prior}/#@ class = ${CLASS}/g\" ${NWPETC}/${Prog} > ${NWPETC}/${Prog}.tmp"
        rsh ${LOGIN_IP} -l ${ACCOUNT} "mv ${NWPETC}/${Prog}.tmp ${NWPETC}/${Prog}"
	    echo "Set ${Prog} \"#@ class = ${CLASS}\""
	  else
	    echo "${Prog} \"#@ class\" is \"${CLASS}\""
	  fi
    fi
  fi
done

##rsh ${LOGIN_IP} -l ${1} "echo ${3} > ${2}/etc/Lid"
