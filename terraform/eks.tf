###############################################################################
# EKS — cluster + managed node group
#
# Nodes run in the private subnets. The node security group is reused by rds.tf
# and opensearch.tf to scope database / search access to the cluster only.
###############################################################################

module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "~> 20.24"

  cluster_name    = var.eks_cluster_name
  cluster_version = var.eks_cluster_version

  # Public API endpoint (toggleable); private access is always on so nodes and
  # in-cluster tooling reach the control plane over the VPC.
  cluster_endpoint_public_access  = var.eks_public_access
  cluster_endpoint_private_access = true

  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnets

  # Grant the identity running Terraform cluster-admin so it can manage
  # aws-auth / access entries right after creation.
  enable_cluster_creator_admin_permissions = true

  # Core add-ons managed by EKS.
  cluster_addons = {
    coredns                = {}
    eks-pod-identity-agent = {}
    kube-proxy             = {}
    vpc-cni                = {}
  }

  eks_managed_node_group_defaults = {
    ami_type = "AL2023_x86_64_STANDARD"
  }

  eks_managed_node_groups = {
    default = {
      instance_types = var.eks_node_instance_types

      min_size     = var.eks_node_min_size
      max_size     = var.eks_node_max_size
      desired_size = var.eks_node_desired_size

      labels = {
        role = "general"
      }
    }
  }

  tags = local.common_tags
}
