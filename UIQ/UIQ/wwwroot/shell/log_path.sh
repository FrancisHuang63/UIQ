#!/bin/sh

cwd=`dirname "${0}"`
expr "${0}" : "/.*" > /dev/null || cwd=`(cd "${cwd}" && pwd)`
HpcCTL=`echo ${cwd} | cut -d/ -f3`
. /ncs/${HpcCTL}/web/shell/SV_PATH.cfg

a=`mysql -u${Hpcdb_login} -p${Hpcdb_password} << EOF
use ${HpcSQL};
select member.account,member.member_path,model.model_name, member.member_name from member left join model on member.model_id = model.model_id;
EOF` 

count=0
for str in $a
do
    #count=$(( $count+1 ))
    #while [ $(( $count % 4 )) != 0 ]
    #do
    #    path=$path/$str
    #done
    [ $str = "" ] && count=$(( $count+1 )) && continue
    if [ $(( $count % 4 )) != 0 ]
    then 
        path="$path/$str"
    else 
        path="$path /ncs/$str"
    fi
    count=$(( $count+1 ))
done
path=`echo $path | sed 's/\/\//\//g' | sed 's/\/NULL\//\//g' | cut -d ' ' -f2-`
echo $path

