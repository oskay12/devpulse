###############################################################################
# Bootstrap — Terraform remote state backend
#
# Creates the S3 bucket and DynamoDB table that hold the *main* stack's remote
# state and state lock. Run this ONCE per AWS account, BEFORE `terraform init`
# in the parent directory.
#
# This directory intentionally uses LOCAL state (there is no backend yet to
# store it in). Commit the resulting bootstrap/terraform.tfstate or keep it
# safe — it only tracks these two bootstrap resources.
#
# Usage:
#   cd terraform/bootstrap
#   terraform init
#   terraform apply -var="state_bucket_name=<globally-unique-name>"
###############################################################################

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project   = var.project
      ManagedBy = "terraform"
      Purpose   = "terraform-state-backend"
    }
  }
}

variable "aws_region" {
  description = "AWS region for the state backend."
  type        = string
  default     = "us-east-1"
}

variable "project" {
  description = "Project name, used for tagging and default resource names."
  type        = string
  default     = "devpulse"
}

variable "state_bucket_name" {
  description = "Globally-unique S3 bucket name for Terraform state. S3 bucket names are global — pick something unique, e.g. devpulse-tfstate-<account-id>."
  type        = string
}

variable "lock_table_name" {
  description = "DynamoDB table name for Terraform state locking."
  type        = string
  default     = "devpulse-tf-lock"
}

# ---------------------------------------------------------------------------
# S3 bucket holding the remote state, with versioning + encryption + no public
# access. Versioning lets you recover a previous state if one gets corrupted.
# ---------------------------------------------------------------------------
resource "aws_s3_bucket" "state" {
  bucket = var.state_bucket_name

  # State is critical — refuse accidental `terraform destroy` of the bucket.
  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_s3_bucket_versioning" "state" {
  bucket = aws_s3_bucket.state.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "state" {
  bucket = aws_s3_bucket.state.id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "state" {
  bucket = aws_s3_bucket.state.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# ---------------------------------------------------------------------------
# DynamoDB table used purely for state locking (NOT application data — the
# DevPulse app uses PostgreSQL/RDS). One item per locked state file.
# ---------------------------------------------------------------------------
resource "aws_dynamodb_table" "lock" {
  name         = var.lock_table_name
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"

  attribute {
    name = "LockID"
    type = "S"
  }
}

output "state_bucket_name" {
  description = "Name of the S3 state bucket — put this in the parent backend block."
  value       = aws_s3_bucket.state.id
}

output "lock_table_name" {
  description = "Name of the DynamoDB lock table — put this in the parent backend block."
  value       = aws_dynamodb_table.lock.name
}

output "backend_config_hint" {
  description = "Backend block to paste into ../main.tf."
  value       = <<-EOT
    backend "s3" {
      bucket         = "${aws_s3_bucket.state.id}"
      key            = "infra/terraform.tfstate"
      region         = "${var.aws_region}"
      dynamodb_table = "${aws_dynamodb_table.lock.name}"
      encrypt        = true
    }
  EOT
}
