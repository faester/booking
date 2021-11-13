#!/bin/bash

for f in $(aws ecs list-tasks --region eu-west-1 --profile mfaester --cluster booking-main|jq -r ".taskArns[]"); do
	echo "Will attempt to stop $f"
	aws ecs stop-task --cluster booking-main --region eu-west-1 --profile mfaester --task $f
done


