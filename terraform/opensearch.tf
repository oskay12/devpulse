###############################################################################
# OpenSearch Service — VPC domain with fine-grained access control
#
# BasicAuth in the app maps to OpenSearch fine-grained access control backed by
# the Internal User Database. Node-to-node and at-rest encryption plus HTTPS are
# mandatory when FGAC is enabled. The domain lives in the VPC and only accepts
# HTTPS (443) from the EKS node security group.
###############################################################################

# Service-linked role required for creating VPC-based domains.
# Ignored gracefully if it already exists in the account.
resource "aws_iam_service_linked_role" "opensearch" {
  aws_service_name = "opensearchservice.amazonaws.com"
  description      = "Service-linked role for Amazon OpenSearch Service"

  # Account-wide singleton — if it already exists, remove/import this resource.
  lifecycle {
    ignore_changes = all
  }
}

# CloudWatch log groups for OpenSearch application, search-slow and index-slow
# logs. Prod observability into slow queries and errors.
resource "aws_cloudwatch_log_group" "opensearch" {
  for_each = toset(["application", "index-slow", "search-slow"])

  name              = "/aws/opensearch/${var.opensearch_domain_name}/${each.value}"
  retention_in_days = 14
  tags              = local.common_tags
}

# Resource policy allowing the OpenSearch service to write to the log groups.
resource "aws_cloudwatch_log_resource_policy" "opensearch" {
  policy_name = "${local.name_prefix}-opensearch-logs"

  policy_document = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "es.amazonaws.com"
        }
        Action = [
          "logs:PutLogEvents",
          "logs:CreateLogStream",
        ]
        Resource = [for lg in aws_cloudwatch_log_group.opensearch : "${lg.arn}:*"]
      }
    ]
  })
}

resource "aws_security_group" "opensearch" {
  name        = "${local.name_prefix}-opensearch-sg"
  description = "Allow HTTPS access to OpenSearch from EKS nodes only"
  vpc_id      = module.vpc.vpc_id

  tags = local.common_tags
}

resource "aws_security_group_rule" "opensearch_ingress_from_eks" {
  type                     = "ingress"
  description              = "HTTPS from EKS worker nodes"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.opensearch.id
  source_security_group_id = module.eks.node_security_group_id
}

resource "aws_opensearch_domain" "main" {
  domain_name    = var.opensearch_domain_name
  engine_version = var.opensearch_engine_version

  cluster_config {
    instance_type          = var.opensearch_instance_type
    instance_count         = var.opensearch_instance_count
    zone_awareness_enabled = var.opensearch_instance_count > 1

    dynamic "zone_awareness_config" {
      for_each = var.opensearch_instance_count > 1 ? [1] : []
      content {
        availability_zone_count = min(var.opensearch_instance_count, var.az_count)
      }
    }
  }

  ebs_options {
    ebs_enabled = true
    volume_type = "gp3"
    volume_size = var.opensearch_volume_size
  }

  # VPC deployment — one subnet per AZ the domain spans.
  vpc_options {
    subnet_ids         = slice(module.vpc.private_subnets, 0, var.opensearch_instance_count > 1 ? min(var.opensearch_instance_count, var.az_count) : 1)
    security_group_ids = [aws_security_group.opensearch.id]
  }

  # HTTPS + node-to-node + at-rest encryption are prerequisites for FGAC.
  domain_endpoint_options {
    enforce_https       = true
    tls_security_policy = "Policy-Min-TLS-1-2-2019-07"
  }

  node_to_node_encryption {
    enabled = true
  }

  encrypt_at_rest {
    enabled = true
  }

  # Publish application, index-slow and search-slow logs to CloudWatch.
  dynamic "log_publishing_options" {
    for_each = {
      ES_APPLICATION_LOGS = aws_cloudwatch_log_group.opensearch["application"].arn
      INDEX_SLOW_LOGS     = aws_cloudwatch_log_group.opensearch["index-slow"].arn
      SEARCH_SLOW_LOGS    = aws_cloudwatch_log_group.opensearch["search-slow"].arn
    }
    content {
      log_type                 = log_publishing_options.key
      cloudwatch_log_group_arn = log_publishing_options.value
    }
  }

  # Fine-grained access control backed by the Internal User Database (BasicAuth).
  advanced_security_options {
    enabled                        = true
    internal_user_database_enabled = true

    master_user_options {
      master_user_name     = var.opensearch_master_user
      master_user_password = var.opensearch_master_password
    }
  }

  # With FGAC the domain access policy can allow all principals; authorization
  # is enforced by fine-grained roles, and the network is locked down by the SG.
  access_policies = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = { AWS = "*" }
        Action    = "es:*"
        Resource  = "arn:aws:es:${var.aws_region}:${data.aws_caller_identity.current.account_id}:domain/${var.opensearch_domain_name}/*"
      }
    ]
  })

  tags = local.common_tags

  depends_on = [
    aws_iam_service_linked_role.opensearch,
    aws_cloudwatch_log_resource_policy.opensearch,
  ]
}
