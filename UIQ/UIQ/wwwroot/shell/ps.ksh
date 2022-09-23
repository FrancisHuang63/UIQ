#!/bin/ksh
#rsh $1 ps -ef|grep '^ npca'| grep -v 'grep'
ps -ef | grep '^npca' | grep -v 'grep'
