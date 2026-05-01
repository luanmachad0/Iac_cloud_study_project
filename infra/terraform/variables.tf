variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "aws_access_key" {
  type    = string
  default = "test"
}

variable "aws_secret_key" {
  type    = string
  default = "test"
}

variable "aws_service_url" {
  type    = string
  default = "http://localhost:4566"
}

variable "s3_bucket_name" {
  type    = string
  default = "sportsbetting-results-bucket"
}

variable "sqs_queue_name" {
  type    = string
  default = "bets-events"
}
