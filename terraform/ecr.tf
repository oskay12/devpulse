###############################################################################
# ECR — container image repositories
#
# The CI/CD pipeline (.github/workflows/deploy.yml) pushes images to these:
#   devpulse/api, devpulse/worker, devpulse/rabbitmq (mirrored public image).
# Each repo scans images on push and prunes old untagged layers automatically.
###############################################################################

locals {
  ecr_repositories = ["api", "worker", "rabbitmq"]
}

resource "aws_ecr_repository" "repos" {
  for_each = toset(local.ecr_repositories)

  name                 = "${var.project}/${each.value}"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = local.common_tags
}

# Keep the last 10 tagged images; expire untagged layers after 7 days so the
# registry does not grow unbounded from CI pushes.
resource "aws_ecr_lifecycle_policy" "repos" {
  for_each   = aws_ecr_repository.repos
  repository = each.value.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Expire untagged images older than 7 days"
        selection = {
          tagStatus   = "untagged"
          countType   = "sinceImagePushed"
          countUnit   = "days"
          countNumber = 7
        }
        action = { type = "expire" }
      },
      {
        rulePriority = 2
        description  = "Keep only the last 10 tagged images"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 10
        }
        action = { type = "expire" }
      }
    ]
  })
}
