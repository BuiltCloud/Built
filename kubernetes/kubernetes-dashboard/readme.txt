kubectl �����������򵥵� kubernetes-dashboard ���񣺣�Quick language switcher chrome��������л���Ӣ�ģ��������ý�����Ҫ�������ö�����һ���Ϳ����ˣ�����ѡ������ԣ�
1��
kubectl create -f https://raw.githubusercontent.com/kubernetes/dashboard/master/src/deploy/recommended/kubernetes-dashboard.yaml
�鿴
kubectl get deployments --namespace kube-system
�� Dashboard ������Ϻ󣬿���ʹ�� kubectl �ṩ�� Proxy ���������ʸ���壺
kubectl proxy

# �����µ�ַ��
# http://localhost:8001/api/v1/namespaces/kube-system/services/https:kubernetes-dashboard:/proxy/
������ʱ������Գ��Ա༭ kubernetes-dashboard ����ClusterIP ��Ϊ NodePort��https://github.com/kubernetes/dashboard/wiki/Accessing-Dashboard---1.7.X-and-above��