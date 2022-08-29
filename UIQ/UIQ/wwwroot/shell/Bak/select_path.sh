#!/bin/sh
../../shell/SV_PATH.cfg
if [ $# != "2" ]; then
    echo "The Parameter must be {MODEL} and {MEMBER}." 
    exit -1
fi

a=`mysql -u${Hpcdb_login} -p${Hpcdb_password} << EOF
use ${HpcSQL};
select member.account,member.member_path,model.model_name, member.member_name from member left join model on member.model_id = model.model_id where model.model_name = "${1}" and member.member_name = "${2}";
EOF`
count=0
for str in $a
do
    #count=$(( $count+1 ))
    #while [ $(( $count % 4 )) != 0 ]
    #do
    #    path=$path/$str
    #done
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
