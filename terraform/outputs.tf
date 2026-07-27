###############################################################################
# Outputs
#
# Endpoints and identifiers consumed by the CI/CD pipeline and the Kubernetes
# ConfigMap / Secret manifests under k8s/.
###############################################################################

#------------------------------------------------------------------------------
# Networking
#------------------------------------------------------------------------------

output "vpc_id" {
  description = "ID of the VPC."
  value       = module.vpc.vpc_id
}

output "private_subnet_ids" {
  description = "IDs of the private subnets."
  value       = module.vpc.private_subnets
}

output "public_subnet_ids" {
  description = "IDs of the public subnets."
  value       = module.vpc.public_subnets
}

#------------------------------------------------------------------------------
# EKS
#------------------------------------------------------------------------------

output "eks_cluster_name" {
  description = "Name of the EKS cluster."
  value       = module.eks.cluster_name
}

output "eks_cluster_endpoint" {
  description = "Endpoint of the EKS Kubernetes API server."
  value       = module.eks.cluster_endpoint
}

output "eks_node_security_group_id" {
  description = "Security group ID attached to the EKS worker nodes."
  value       = module.eks.node_security_group_id
}

output "kubeconfig_command" {
  description = "Command to update local kubeconfig for the cluster."
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}"
}

#------------------------------------------------------------------------------
# ECR  ->  k8s deployment image references
#------------------------------------------------------------------------------

output "ecr_repository_urls" {
  description = "Map of ECR repository URLs (api/worker/rabbitmq) for image pushes and K8s manifests."
  value       = { for k, r in aws_ecr_repository.repos : k => r.repository_url }
}

output "ecr_registry" {
  description = "ECR registry host for this account/region (<account>.dkr.ecr.<region>.amazonaws.com)."
  value       = "${data.aws_caller_identity.current.account_id}.dkr.ecr.${var.aws_region}.amazonaws.com"
}

#------------------------------------------------------------------------------
# RDS  ->  k8s/secret.yaml  (DatabaseSettings__ConnectionString)
#------------------------------------------------------------------------------

output "rds_endpoint" {
  description = "RDS connection endpoint (host:port)."
  value       = aws_db_instance.postgres.endpoint
}

output "rds_address" {
  description = "RDS hostname."
  value       = aws_db_instance.postgres.address
}

output "database_connection_string" {
  description = "Ready-to-use Npgsql connection string for the app secret (password redacted; inject separately)."
  value       = "Host=${aws_db_instance.postgres.address};Port=${aws_db_instance.postgres.port};Database=${var.rds_db_name};Username=${var.rds_username};Password=__SET_FROM_SECRET__"
}

#------------------------------------------------------------------------------
# OpenSearch  ->  k8s/configmap.yaml  (OpenSearch__Endpoint)
#------------------------------------------------------------------------------

output "opensearch_endpoint" {
  description = "HTTPS endpoint of the OpenSearch domain (for OpenSearch__Endpoint)."
  value       = "https://${aws_opensearch_domain.main.endpoint}"
}

output "opensearch_dashboards_endpoint" {
  description = "OpenSearch Dashboards endpoint."
  value       = "https://${aws_opensearch_domain.main.dashboard_endpoint}"
}
