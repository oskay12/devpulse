###############################################################################
# Example variable values.
#
# Copy to a real (git-ignored) file — e.g. prod.tfvars — and fill in secrets:
#   cp example.tfvars prod.tfvars
#   terraform plan -var-file=prod.tfvars
#
# Or export secrets as environment variables so they never touch disk:
#   export TF_VAR_rds_password='...'
#   export TF_VAR_opensearch_master_password='...'
###############################################################################

project     = "devpulse"
environment = "prod"
aws_region  = "us-east-1"

# --- Networking ---
vpc_cidr           = "10.0.0.0/16"
az_count           = 2
# For real prod, prefer one NAT gateway per AZ for HA (higher cost):
single_nat_gateway = true

# --- EKS ---
eks_cluster_name        = "devpulse-eks-cluster"
eks_cluster_version     = "1.30"
eks_node_instance_types = ["t3.large"]
eks_node_desired_size   = 2

# --- RDS ---
rds_engine_version        = "16"
rds_instance_class        = "db.t3.medium"
rds_db_name               = "devpulse"
rds_username              = "devpulse_user"
rds_multi_az              = true # HA for prod
rds_backup_retention_days = 7
rds_deletion_protection   = true
# rds_password            -> set via TF_VAR_rds_password (do NOT put secrets here)

# --- OpenSearch ---
opensearch_domain_name    = "devpulse-opensearch"
opensearch_engine_version = "OpenSearch_2.13"
opensearch_instance_type  = "t3.medium.search"
opensearch_master_user    = "devpulse_admin"
# opensearch_master_password -> set via TF_VAR_opensearch_master_password
