#!/bin/ksh
. /nwp/npcactl/shell/SV_PATH.cfg
set -x
#echo "$1"

rsh ${LOGIN_IP} -l npcagfs /usr/bin/pjsub -x dtg=\"$1\" /nwp/npcagfs/GFS/SLP/etc/GFS_SLP_UI
