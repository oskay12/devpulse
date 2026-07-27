###############################################################################
# Input variables
###############################################################################

#------------------------------------------------------------------------------
# General
#------------------------------------------------------------------------------

variable "project" {
  description = "Project name, used as a prefix for resource naming and tags."
  type        = string
  default     = "devpulse"
}

variable "environment" {
  description = "Deployment environment (e.g. dev, staging, prod)."
  type        = string
  default     = "prod"
}

variable "aws_region" {
  description = "AWS region to deploy into."
  type        = string
  default     = "us-east-1"
}

variable "additional_tags" {
  description = "Extra tags merged into the default tag set on every resource."
  type        = map(string)
  default     = {}
}

#------------------------------------------------------------------------------
# Networking (VPC)
#------------------------------------------------------------------------------

variable "vpc_cidr" {
  description = "CIDR block for the VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "az_count" {
  description = "Number of Availability Zones to span (>= 2 required for EKS/RDS)."
  type        = number
  default     = 2

  validation {
    condition     = var.az_count >= 2
    error_message = "az_count must be at least 2 for EKS control plane and RDS Multi-AZ support."
  }
}

variable "single_nat_gateway" {
  description = "Use a single NAT Gateway (cheaper, non-HA) instead of one per AZ."
  type        = bool
  default     = true
}

#------------------------------------------------------------------------------
# EKS
#------------------------------------------------------------------------------

variable "eks_cluster_name" {
  description = "Name of the EKS cluster (matches the existing devpulse-eks-cluster)."
  type        = string
  default     = "devpulse-eks-cluster"
}

variable "eks_cluster_version" {
  description = "Kubernetes version for the EKS control plane."
  type        = string
  default     = "1.30"
}

variable "eks_node_instance_types" {
  description = "EC2 instance types for the managed node group."
  type        = list(string)
  default     = ["t3.large"]
}

variable "eks_node_desired_size" {
  description = "Desired number of worker nodes."
  type        = number
  default     = 2
}

variable "eks_node_min_size" {
  description = "Minimum number of worker nodes."
  type        = number
  default     = 2
}

variable "eks_node_max_size" {
  description = "Maximum number of worker nodes."
  type        = number
  default     = 4
}

variable "eks_public_access" {
  description = "Whether the EKS API server endpoint is publicly reachable."
  type        = bool
  default     = true
}

#------------------------------------------------------------------------------
# RDS (PostgreSQL)
#------------------------------------------------------------------------------

variable "rds_engine_version" {
  description = "PostgreSQL engine version."
  type        = string
  default     = "16"
}

variable "rds_instance_class" {
  description = "RDS instance class."
  type        = string
  default     = "db.t3.medium"
}

variable "rds_allocated_storage" {
  description = "Initial allocated storage in GB."
  type        = number
  default     = 20
}

variable "rds_max_allocated_storage" {
  description = "Upper limit for storage autoscaling in GB (0 disables autoscaling)."
  type        = number
  default     = 100
}

variable "rds_multi_az" {
  description = "Deploy RDS across multiple AZs for high availability."
  type        = bool
  default     = false
}

variable "rds_db_name" {
  description = "Initial database name."
  type        = string
  default     = "devpulse"
}

variable "rds_username" {
  description = "Master username for the RDS instance."
  type        = string
  default     = "devpulse_user"
}

variable "rds_password" {
  description = "Master password for the RDS instance. Provide via TF_VAR_rds_password or a secret tfvars file — never commit."
  type        = string
  sensitive   = true
}

variable "rds_backup_retention_days" {
  description = "Number of days to retain automated backups."
  type        = number
  default     = 7
}

variable "rds_deletion_protection" {
  description = "Prevent accidental deletion of the RDS instance."
  type        = bool
  default     = true
}

#------------------------------------------------------------------------------
# OpenSearch
#------------------------------------------------------------------------------

variable "opensearch_domain_name" {
  description = "Name of the OpenSearch domain (matches existing devpulse-opensearch)."
  type        = string
  default     = "devpulse-opensearch"
}

variable "opensearch_engine_version" {
  description = "OpenSearch engine version (e.g. OpenSearch_2.13)."
  type        = string
  default     = "OpenSearch_2.13"
}

variable "opensearch_instance_type" {
  description = "Instance type for OpenSearch data nodes."
  type        = string
  default     = "t3.medium.search"
}

variable "opensearch_instance_count" {
  description = "Number of OpenSearch data nodes."
  type        = number
  default     = 2
}

variable "opensearch_volume_size" {
  description = "EBS volume size per OpenSearch node in GB."
  type        = number
  default     = 20
}

variable "opensearch_master_user" {
  description = "Master username for OpenSearch fine-grained access control (Internal User Database)."
  type        = string
  default     = "devpulse_admin"
}

variable "opensearch_master_password" {
  description = "Master password for OpenSearch FGAC. Provide via TF_VAR_opensearch_master_password — never commit. Must meet AWS complexity requirements."
  type        = string
  sensitive   = true
}
