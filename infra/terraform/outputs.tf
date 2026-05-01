output "s3_bucket_name" {
  value = aws_s3_bucket.bets_bucket.id
}

output "sqs_queue_url" {
  value = aws_sqs_queue.bets_events.url
}
