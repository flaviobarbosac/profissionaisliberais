resource "aws_ssm_parameter" "db_password" {
  name  = "/${var.project}/${var.environment}/db/password"
  type  = "SecureString"
  value = random_password.db.result
}

resource "aws_ssm_parameter" "db_connection" {
  name = "/${var.project}/${var.environment}/db/connection_string"
  type = "SecureString"
  value = join("", [
    "Host=", aws_db_instance.main.address,
    ";Port=", aws_db_instance.main.port,
    ";Database=", var.db_name,
    ";Username=", var.db_username,
    ";Password=", random_password.db.result,
    ";SSL Mode=Require;Trust Server Certificate=true"
  ])
}

resource "aws_ssm_parameter" "jwt_key" {
  name  = "/${var.project}/${var.environment}/jwt/key"
  type  = "SecureString"
  value = "ALTERAR-chave-dev-minimo-32-caracteres-libify"

  lifecycle {
    ignore_changes = [value]
  }
}

resource "aws_ssm_parameter" "asaas_webhook_base_url" {
  name  = "/${var.project}/${var.environment}/asaas/webhook_base_url"
  type  = "String"
  value = "https://${local.api_fqdn}"
}

resource "aws_ssm_parameter" "cors_origin" {
  name  = "/${var.project}/${var.environment}/cors/origin"
  type  = "String"
  value = "https://${local.front_fqdn}"
}
