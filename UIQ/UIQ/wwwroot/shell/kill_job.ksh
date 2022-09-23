#!/bin/ksh
set -x

list=`ps aux | grep npcactl | grep -v '-ksh' | awk '{print $2}'`
for id in $list 
do
  kill -9 $id
done
