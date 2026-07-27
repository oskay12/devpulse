###############################################################################
# VPC — network foundation
#
# Public subnets host NAT gateways and internet-facing LoadBalancers.
# Private subnets host EKS worker nodes, RDS and OpenSearch.
# Kubernetes ELB role tags let the AWS Load Balancer Controller discover
# subnets automatically.
###############################################################################

module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "~> 5.8"

  name = "${local.name_prefix}-vpc"
  cidr = var.vpc_cidr

  azs = local.azs

  # Split the VPC CIDR into private/public /20 blocks per AZ.
  private_subnets = [for i in range(var.az_count) : cidrsubnet(var.vpc_cidr, 4, i)]
  public_subnets  = [for i in range(var.az_count) : cidrsubnet(var.vpc_cidr, 4, i + 8)]

  enable_nat_gateway   = true
  single_nat_gateway   = var.single_nat_gateway
  enable_dns_hostnames = true
  enable_dns_support   = true

  # Required by the EKS control plane / AWS Load Balancer Controller so it can
  # place internal and internet-facing load balancers on the right subnets.
  public_subnet_tags = {
    "kubernetes.io/role/elb"                        = "1"
    "kubernetes.io/cluster/${var.eks_cluster_name}" = "shared"
  }

  private_subnet_tags = {
    "kubernetes.io/role/internal-elb"               = "1"
    "kubernetes.io/cluster/${var.eks_cluster_name}" = "shared"
  }

  tags = local.common_tags
}
