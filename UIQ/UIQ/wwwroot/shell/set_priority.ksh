#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg

set -x
file="/nwp/npcactl/NFS/M02/etc/Rnwpnfs2_M00"
ACCOUNT="npcactl"

newprior="oper"

prior=`cat ${file} | grep "^#@ class" | cut -d' ' -f4`
if [ ${prior} != ${newprior} ]; then
	if [ ${prior} = 'urgent' ] || [ ${prior} = 'nwp' ] || [ ${prior} = 'priorop' ] || [ ${prior} = 'oper' ] || [ ${prior} = 'member' ] || [ ${prior} = 'beta' ] || [ ${prior} = 'research' ]; then
		rsh ${LOGIN_IP} -l ${ACCOUNT} "sed \"s/^#@ class = ${prior}/#@ class = ${newprior}/g\" ${file} > ${file}.tmp"
		rsh ${LOGIN_IP} -l ${ACCOUNT} "mv ${file}.tmp ${file}"
	fi
fi
