#!/bin/sh
MODEL=$1
MEMBER=$2
[[ $# != 2 ]] && echo "MODEL and MEMBER are needed!!" && exit 1
echo "kill ${MODEL} ${MEMBER} Fail" >> ../log/${MODEL}/${MEMBER}.log 
