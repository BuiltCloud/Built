kubectl create -f nginx-deployment.yaml

kubectl get deployments
kubectl get rs
kubectl get pods --show-labels

��������1
kubectl set image deployment/nginx-deployment nginx=nginx:1.9.1
��������2 ֱ�ӱ༭
kubectl edit deployment/nginx-deployment

kubectl describe deployments