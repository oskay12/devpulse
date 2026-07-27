# DevPulse — Terraform Kurulum Rehberi (Yeni AWS Hesabı)

Bu rehber, DevPulse altyapısını **sıfırdan yeni bir AWS hesabında** ayağa
kaldırır: VPC · EKS · RDS (PostgreSQL 16) · OpenSearch · ECR.

> Not: Buradaki DynamoDB tablosu yalnızca **Terraform state kilidi** içindir.
> Uygulamanın veritabanı PostgreSQL (RDS)'tir; DynamoDB uygulama verisi tutmaz.

---

## Ön koşullar

- AWS CLI kurulu ve yeni hesap için yapılandırılmış:
  ```bash
  aws configure           # veya: export AWS_PROFILE=<yeni-hesap-profili>
  aws sts get-caller-identity   # doğru hesapta olduğunu teyit et
  ```
- `terraform >= 1.5`, `kubectl`, `docker` kurulu.

---

## Adım 1 — State backend'ini oluştur (bootstrap, hesap başına bir kez)

State'i tutacak S3 bucket'ı ve kilit tablosunu oluşturur. `state_bucket_name`
**global olarak tekil** olmalı — hesap ID'sini eklemek pratik bir yöntem:

```bash
cd terraform/bootstrap
terraform init

ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
terraform apply -var="state_bucket_name=devpulse-tfstate-${ACCOUNT_ID}"
```

Çıktıdaki `state_bucket_name` ve `lock_table_name` değerlerini not al.

---

## Adım 2 — Ana stack'in backend'ini bu isimlere göre ayarla

`terraform/main.tf` içindeki `backend "s3"` bloğunda **bucket** adını Adım 1'de
oluşturduğun tekil isimle güncelle (lock tablosu varsayılan `devpulse-tf-lock`):

```hcl
backend "s3" {
  bucket         = "devpulse-tfstate-<ACCOUNT_ID>"   # <-- güncelle
  key            = "infra/terraform.tfstate"
  region         = "us-east-1"
  dynamodb_table = "devpulse-tf-lock"
  encrypt        = true
}
```

> Alternatif: bloğu elle düzenlemek yerine `-backend-config` ile de verebilirsin:
> `terraform init -backend-config="bucket=devpulse-tfstate-<ACCOUNT_ID>"`

---

## Adım 3 — Secret'ları ortam değişkeni olarak ver

Parolalar koda/tfvars'a **yazılmaz**; env değişkeni ile verilir:

```bash
export TF_VAR_rds_password='<güçlü-bir-parola>'
export TF_VAR_opensearch_master_password='<Aa1!güçlü-parola>'   # AWS karmaşıklık kuralı
```

---

## Adım 4 — Ana altyapıyı uygula

```bash
cd terraform
terraform init          # state artık S3'te
terraform plan  -var-file=example.tfvars    # önce ne yapacağını gör
terraform apply -var-file=example.tfvars
```

> EKS + RDS + OpenSearch birlikte ~20-30 dk sürebilir (özellikle OpenSearch).

Bittiğinde çıktıları al:

```bash
terraform output
```

Önemli çıktılar: `opensearch_endpoint`, `rds_address`, `ecr_registry`,
`ecr_repository_urls`, `eks_cluster_name`, `kubeconfig_command`.

---

## Adım 5 — kubeconfig'i bağla

```bash
$(terraform output -raw kubeconfig_command)
# veya:
aws eks update-kubeconfig --region us-east-1 --name devpulse-eks-cluster
kubectl get nodes
```

---

## Adım 6 — K8s manifestlerini yeni hesabın değerleriyle güncelle

Eski hesaba çakılı değerleri Terraform çıktılarıyla değiştir:

| Dosya | Alan | Yeni değer (kaynak) |
|---|---|---|
| `k8s/configmap.yaml` | `OpenSearch__Endpoint` | `terraform output -raw opensearch_endpoint` |
| `k8s/api-deployment.yaml` | image | `<ecr_registry>/devpulse/api:...` |
| `k8s/worker-deployment.yaml` | image | `<ecr_registry>/devpulse/worker:...` |
| `k8s/secret.yaml` | DB connection string | host = `terraform output -raw rds_address` |

Namespace ve secret'ları oluştur:

```bash
kubectl create namespace devpulse
# k8s/secret.yaml.template -> k8s/secret.yaml kopyala, gerçek değerleri doldur
kubectl apply -f k8s/secret.yaml -n devpulse
kubectl apply -f k8s/configmap.yaml -n devpulse
```

---

## Adım 7 — Image'ları yeni ECR'a push et ve uygula

ECR repo'ları Terraform tarafından oluşturuldu (`ecr.tf`). CI/CD'yi yeni hesabın
credential'larıyla tetikleyebilir ya da elle push edebilirsin:

```bash
REGISTRY=$(terraform output -raw ecr_registry)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin "$REGISTRY"

docker build -f src/DevPulse.Api/Dockerfile   -t "$REGISTRY/devpulse/api:latest" .
docker build -f src/DevPulse.Worker/Dockerfile -t "$REGISTRY/devpulse/worker:latest" .
docker push "$REGISTRY/devpulse/api:latest"
docker push "$REGISTRY/devpulse/worker:latest"

kubectl apply -f k8s/ -n devpulse
kubectl rollout status deployment/devpulse-api -n devpulse
```

---

## GitHub Actions kullanacaksan

`.github/workflows/deploy.yml` içindeki `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY`
repo secret'larını **yeni hesabın** credential'larıyla değiştir. `EKS_CLUSTER_NAME`
ve `AWS_REGION` aynı kaldığı sürece pipeline çalışır.

---

## Temizlik (ortamı silmek istersen)

```bash
cd terraform
terraform destroy -var-file=example.tfvars
# Not: RDS deletion_protection=true ise önce onu false yapıp apply et.
# Bootstrap bucket/tablo prevent_destroy ile korunur; en son elle silinir.
```
