#!/bin/sh
#bgfunc(){
for i in 1 2 3 4 5;do
echo $i
sleep 1
done &
#}
#bgfunc &
for j in A B C D E;do
echo $j
sleep 2
done
