###############################################################################
# DevPulse — Infrastructure as Code (Terraform)
#
# Provisions the AWS infrastructure that backs the DevPulse platform:
#   VPC / Subnets / SGs · Amazon EKS · Amazon RDS (PostgreSQL 16) ·
#   Amazon OpenSearch Service (VPC, fine-grained access control).
#
# Layout:
#   main.tf        provider, version constraints, remote backend, shared locals
#   variables.tf   input variables
#   outputs.tf     stack outputs (endpoints, ids, names)
#   vpc.tf         network foundation (terraform-aws-modules/vpc)
#   eks.tf         EKS cluster + managed node group (terraform-aws-modules/eks)
#   rds.tf         RDS PostgreSQL + subnet group + security group
#   opensearch.tf  OpenSearch domain (VPC) + security group
###############################################################################

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }

  # Remote state in S3 with DynamoDB state locking.
  # NOTE: the bucket and lock table must exist before `terraform init`.
  # Values here cannot use variables — override with `-backend-config` if needed.
  backend "s3" {
    bucket         = "devpulse-tfstate"
    key            = "infra/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "devpulse-tf-lock"
    encrypt        = true
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = local.common_tags
  }
}

# Current account / region context (used for ARNs, policies, outputs).
data "aws_caller_identity" "current" {}
data "aws_availability_zones" "available" {
  state = "available"
}

locals {
  # Canonical name prefix, e.g. "devpulse-prod".
  name_prefix = "${var.project}-${var.environment}"

  # Availability zones actually used by the stack (first N in the region).
  azs = slice(data.aws_availability_zones.available.names, 0, var.az_count)

  common_tags = merge(
    {
      Project     = var.project
      Environment = var.environment
      ManagedBy   = "terraform"
    },
    var.additional_tags
  )
}
