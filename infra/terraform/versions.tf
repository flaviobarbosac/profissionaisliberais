terraform {
  required_version = ">= 1.5"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }

  backend "s3" {
    bucket         = "libify-terraform-state-635198174869"
    key            = "dev/terraform.tfstate"
    region         = "sa-east-1"
    dynamodb_table = "libify-terraform-lock"
    encrypt        = true
  }
}
