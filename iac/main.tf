provider "aws" {
  access_key                  = "mock_access_key"
  secret_key                  = "mock_secret_key"
  region                      = "us-east-1"

  skip_credentials_validation = true
  skip_metadata_api_check     = true
  skip_requesting_account_id  = true

  endpoints {
    sqs = "http://dpe.localstack.internal:4566"
    sns = "http://dpe.localstack.internal:4566"
  }
}

resource "aws_sqs_queue" "emailNotificationQueue" {
  name                      = "EmailNotificationQueue"
  delay_seconds             = 0
  max_message_size          = 2048
  message_retention_seconds = 86400
  receive_wait_time_seconds = 10
  tags = {
    Environment = "Local"
  }
}

resource "aws_sqs_queue" "productStockQueue" {
  name                      = "ProductStockQueue"
  delay_seconds             = 0
  max_message_size          = 2048
  message_retention_seconds = 86400
  receive_wait_time_seconds = 10
  tags = {
    Environment = "Local"
  }
}

resource "aws_sqs_queue" "invoiceQueue" {
  name                      = "InvoiceQueue"
  delay_seconds             = 0
  max_message_size          = 2048
  message_retention_seconds = 86400
  receive_wait_time_seconds = 10
  tags = {
    Environment = "Local"
  }
}

resource "aws_sns_topic" "order_confirmed" {
  name = "OrderConfirmedTopic"
}

resource "aws_sns_topic_subscription" "product_stock" {
  topic_arn            = aws_sns_topic.order_confirmed.arn
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.productStockQueue.arn
  raw_message_delivery = false
}

resource "aws_sns_topic_subscription" "invoice" {
  topic_arn            = aws_sns_topic.order_confirmed.arn
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.invoiceQueue.arn
  raw_message_delivery = false
}