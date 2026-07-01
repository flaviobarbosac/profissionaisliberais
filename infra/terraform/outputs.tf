output "account_id" {
  value = data.aws_caller_identity.current.account_id
}

output "route53_name_servers" {
  description = "Registrar estes 4 NS no Registro.br (delegacao completa)"
  value       = aws_route53_zone.main.name_servers
}

output "route53_zone_id" {
  value = aws_route53_zone.main.zone_id
}

output "ec2_public_ip" {
  value = aws_eip.app.public_ip
}

output "api_url" {
  value = "https://${local.api_fqdn}"
}

output "aspire_url" {
  value = "https://${local.aspire_fqdn}"
}

output "front_url" {
  value = "https://${local.front_fqdn}"
}

output "rds_endpoint" {
  value = aws_db_instance.main.address
}

output "rds_port" {
  value = aws_db_instance.main.port
}

output "ecr_api_url" {
  value = aws_ecr_repository.api.repository_url
}

output "ecr_worker_url" {
  value = aws_ecr_repository.worker.repository_url
}

output "amplify_app_id" {
  value = aws_amplify_app.front.id
}

output "amplify_default_domain" {
  value = "https://${aws_amplify_branch.dev.branch_name}.${aws_amplify_app.front.default_domain}"
}

output "ssm_prefix" {
  value = "/${var.project}/${var.environment}/"
}

output "ec2_instance_id" {
  value = aws_instance.app.id
}
