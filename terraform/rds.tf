###############################################################################
# RDS — PostgreSQL 16
#
# Deployed into the private subnets. Ingress on 5432 is allowed only from the
# EKS node security group, so no database traffic ever crosses a public path.
###############################################################################

# Subnet group spanning the private subnets.
resource "aws_db_subnet_group" "postgres" {
  name       = "${local.name_prefix}-rds-subnet-group"
  subnet_ids = module.vpc.private_subnets

  tags = local.common_tags
}

# Security group: PostgreSQL reachable only from EKS worker nodes.
resource "aws_security_group" "rds" {
  name        = "${local.name_prefix}-rds-sg"
  description = "Allow PostgreSQL access from EKS nodes only"
  vpc_id      = module.vpc.vpc_id

  tags = local.common_tags
}

resource "aws_security_group_rule" "rds_ingress_from_eks" {
  type                     = "ingress"
  description              = "PostgreSQL from EKS worker nodes"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = module.eks.node_security_group_id
}

resource "aws_security_group_rule" "rds_egress_all" {
  type              = "egress"
  description       = "Allow all outbound"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  security_group_id = aws_security_group.rds.id
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_db_instance" "postgres" {
  identifier     = "${local.name_prefix}-rds"
  engine         = "postgres"
  engine_version = var.rds_engine_version
  instance_class = var.rds_instance_class

  allocated_storage     = var.rds_allocated_storage
  max_allocated_storage = var.rds_max_allocated_storage
  storage_type          = "gp3"
  storage_encrypted     = true

  db_name  = var.rds_db_name
  username = var.rds_username
  password = var.rds_password
  port     = 5432

  db_subnet_group_name   = aws_db_subnet_group.postgres.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  multi_az               = var.rds_multi_az
  publicly_accessible    = false

  backup_retention_period   = var.rds_backup_retention_days
  deletion_protection       = var.rds_deletion_protection
  skip_final_snapshot       = !var.rds_deletion_protection
  final_snapshot_identifier = var.rds_deletion_protection ? "${local.name_prefix}-rds-final" : null

  # Ship PostgreSQL logs to CloudWatch for prod observability.
  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]

  # Query-level performance telemetry (free tier: 7 days retention).
  performance_insights_enabled          = true
  performance_insights_retention_period = 7

  # Silence noisy diffs on the auto-managed minor version.
  auto_minor_version_upgrade = true
  apply_immediately          = false

  tags = local.common_tags
}
