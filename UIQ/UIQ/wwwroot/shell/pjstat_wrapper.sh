#!/bin/sh

check=0

pjstat -A --data |
while IFS=, read a1 a2 a3 a4 a5 a6 a7 a8 a9 a10 a11 a12 a13
do

  if [ "${a1}" = "H" ]
  then
     if [ ${check} -eq 0 ]
     then
        printf "\t%s %s %s %s %s %s %s %s %s %s\n" ${a3} ${a4} ${a5} ${a6} ${a7} ${a8} ${a9} ${a10} ${a11} ${a12}
         check=1
     else
        printf "\t%s %30s %6s %6s %20s %16s %10s %12s\n" ${a2} ${a3} ${a4} ${a5} ${a6} ${a7} ${a8} ${a9}
         check=2
     fi
  else
     if [ ${check} -eq 1 ]
     then
        if [ "${a2}" = "s" ]
        then
            printf "%s\t%6s %6s %5s %5s %7s %6s %6s %4s %5s %5s\n" ${a2} ${a3} ${a4} ${a5} ${a6} ${a7} ${a8} ${a9} ${a10} ${a11} ${a12} ${a13}
        else
            printf "\t%6s %6s %5s %5s %7s %6s %6s %4s %5s %5s\n" ${a1} ${a2} ${a3} ${a4} ${a5} ${a6} ${a7} ${a8} ${a9} ${a10} ${a11} ${a12}
        fi
     else
	if [ "${a7}" = "-" ]
        then
            printf "\t%6s %30s %6s %6s %20s %15s %10s %12s\n" ${a2} ${a3} ${a4} ${a5} ${a6} ${a7} ${a8} ${a9}
        else
            printf "\t%6s %30s %6s %6s %20s %6s %9s %10s %12s\n" ${a1} ${a2} ${a3} ${a4} ${a5} ${a6} ${a7} ${a8} ${a9}
        fi
     fi
  fi

done
