resource "aws_route53_zone" "main" {
  name = var.domain_name

  tags = {
    Name = "${local.name_prefix}-zone"
  }
}

resource "aws_route53_record" "api" {
  zone_id = aws_route53_zone.main.zone_id
  name    = local.api_fqdn
  type    = "A"
  ttl     = 300
  records = [aws_eip.app.public_ip]
}

resource "aws_route53_record" "aspire" {
  zone_id = aws_route53_zone.main.zone_id
  name    = local.aspire_fqdn
  type    = "A"
  ttl     = 300
  records = [aws_eip.app.public_ip]
}
