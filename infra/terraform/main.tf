terraform {
  required_version = ">= 1.7.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region                      = var.aws_region
  access_key                  = var.aws_access_key
  secret_key                  = var.aws_secret_key
  s3_use_path_style           = true
  skip_credentials_validation = true
  skip_metadata_api_check     = true
  skip_requesting_account_id  = true

  endpoints {
    s3  = var.aws_service_url
    sqs = var.aws_service_url
  }
}

resource "aws_s3_bucket" "bets_bucket" {
  bucket = var.s3_bucket_name
}

resource "aws_sqs_queue" "bets_events" {
  name = var.sqs_queue_name
}
